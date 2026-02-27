# VivaVoz MSIX Build Script (run on Windows)
# Prerequisites: .NET 10 SDK, Windows 10 SDK
# Usage: .\publish-msix.ps1 [-Arch x64|arm64]

param(
    [ValidateSet("x64", "arm64")]
    [string]$Arch = "x64"
)

$ErrorActionPreference = "Stop"

$ProjectDir = "source\VivaVoz"
$OutputDir = "publish\msix"
$AppDir = "$OutputDir\app"
$RuntimeId = "win-$Arch"

Write-Host "Building VivaVoz MSIX package ($RuntimeId)..." -ForegroundColor Cyan

# 1. Clean previous output
if (Test-Path $AppDir) {
    Remove-Item $AppDir -Recurse -Force
}

# 2. Publish self-contained for target architecture
Write-Host "Publishing for $RuntimeId..." -ForegroundColor Yellow
dotnet publish "$ProjectDir\VivaVoz.csproj" `
    -c Release `
    -r $RuntimeId `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -o $AppDir

# 3. Fix native library layout for MSIX
# MSIX needs native DLLs in the app root, not in runtimes/ subfolder
Write-Host "Fixing native library layout..." -ForegroundColor Yellow

# Search for whisper native DLLs in runtimes folder tree
$found = $false
$searchPaths = @(
    "$AppDir\runtimes\$RuntimeId\native",
    "$AppDir\runtimes\$RuntimeId"
)

foreach ($searchPath in $searchPaths) {
    if (Test-Path $searchPath) {
        $dlls = Get-ChildItem $searchPath -Filter "*.dll" -ErrorAction SilentlyContinue
        if ($dlls) {
            foreach ($dll in $dlls) {
                Copy-Item $dll.FullName $AppDir -Force
                Write-Host "  Copied $($dll.Name) to app root" -ForegroundColor Green
            }
            $found = $true
            break
        }
    }
}

# Also check if they ended up directly in runtimes/ subfolders
if (-not $found) {
    $whisperDlls = Get-ChildItem "$AppDir\runtimes" -Filter "whisper.dll" -Recurse -ErrorAction SilentlyContinue
    if ($whisperDlls) {
        $nativeDir = $whisperDlls[0].DirectoryName
        Write-Host "  Found native DLLs in: $nativeDir" -ForegroundColor Yellow
        Get-ChildItem $nativeDir -Filter "*.dll" | ForEach-Object {
            Copy-Item $_.FullName $AppDir -Force
            Write-Host "  Copied $($_.Name) to app root" -ForegroundColor Green
        }
        $found = $true
    }
}

if (-not $found) {
    # Check if DLLs are already in app root (some publish modes do this)
    $whisperDll = Get-ChildItem $AppDir -Filter "whisper.dll" -ErrorAction SilentlyContinue
    if ($whisperDll) {
        Write-Host "  Native DLLs already in app root." -ForegroundColor Green
        $found = $true
    }
}

if (-not $found) {
    Write-Error "whisper.dll not found anywhere in output. Check Whisper.net.Runtime package."
    exit 1
}

# Remove runtimes/ folder (linux, macos, other archs - not needed in MSIX)
if (Test-Path "$AppDir\runtimes") {
    Remove-Item "$AppDir\runtimes" -Recurse -Force
    Write-Host "  Removed runtimes/ folder (unused platforms)" -ForegroundColor Green
}

# Remove macOS Metal shader if present
if (Test-Path "$AppDir\ggml-metal.metal") {
    Remove-Item "$AppDir\ggml-metal.metal" -Force
    Write-Host "  Removed ggml-metal.metal (macOS only)" -ForegroundColor Green
}

# 4. Copy Whisper Base model
$modelsDir = "$AppDir\models"
if (-not (Test-Path $modelsDir)) { New-Item -ItemType Directory -Path $modelsDir | Out-Null }
if (Test-Path "models\ggml-base.bin") {
    Copy-Item "models\ggml-base.bin" "$modelsDir\ggml-base.bin"
    Write-Host "  Whisper Base model bundled." -ForegroundColor Green
} else {
    Write-Warning "ggml-base.bin not found in models/. Download from HuggingFace first."
    Write-Warning "  https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin"
}

# 5. Copy AppxManifest
Copy-Item "$ProjectDir\Package.appxmanifest" "$AppDir\AppxManifest.xml" -Force
Write-Host "  AppxManifest.xml copied." -ForegroundColor Green

# 6. Verify native DLLs are in place
Write-Host ""
Write-Host "Verifying native libraries in app root..." -ForegroundColor Yellow
$requiredDlls = @("whisper.dll", "ggml-base-whisper.dll", "ggml-cpu-whisper.dll", "ggml-whisper.dll")
$allFound = $true
foreach ($dll in $requiredDlls) {
    if (Test-Path "$AppDir\$dll") {
        Write-Host "  [OK] $dll" -ForegroundColor Green
    } else {
        Write-Host "  [MISSING] $dll" -ForegroundColor Red
        $allFound = $false
    }
}

if (-not $allFound) {
    Write-Error "Missing native DLLs in app root. MSIX will fail at runtime."
    exit 1
}

# 7. Create MSIX
Write-Host ""
Write-Host "Creating MSIX package..." -ForegroundColor Yellow

# Find makeappx.exe
$sdkPaths = @(
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\makeappx.exe",
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x64\makeappx.exe"
)

$makeappx = $null
foreach ($path in $sdkPaths) {
    if (Test-Path $path) {
        $makeappx = $path
        break
    }
}

if (-not $makeappx) {
    Write-Error "makeappx.exe not found. Install Windows 10 SDK."
    exit 1
}

$msixPath = "$OutputDir\VivaVoz-$Arch.msix"
& $makeappx pack /d $AppDir /p $msixPath /l /o

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS: $msixPath" -ForegroundColor Green
    $size = [math]::Round((Get-Item $msixPath).Length / 1MB, 1)
    Write-Host "  Size: ${size} MB" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Test locally: Add-AppPackage -Path $msixPath"
    Write-Host "  2. Upload to Microsoft Partner Center for Store distribution"
} else {
    Write-Error "makeappx.exe failed with exit code $LASTEXITCODE"
    exit 1
}
