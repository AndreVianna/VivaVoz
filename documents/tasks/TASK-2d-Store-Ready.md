# Delivery 2d: Final MVP Polish

**Branch:** `delivery-2d`
**Base:** `main` (after 2c merge)
**Goal:** Final MVP polish before packaging

---

## Tasks

### TASK-2d.1: Change Default Model to Base
**Priority:** High | **Estimate:** 1h

The default Whisper model should be `base` instead of `tiny`. Base offers significantly better transcription accuracy with marginal speed difference.

**Changes:**
- Update `Settings.DefaultWhisperModelSize` from `"tiny"` to `"base"`
- Update migration if needed to change existing default
- Update any tests that assume `tiny` as default
- Ensure the onboarding/first-run downloads Base if not already present

**Acceptance Criteria:**
- [ ] New installs default to Base model
- [ ] Existing users who haven't changed their model get upgraded to Base
- [ ] All tests pass with new default

---

### TASK-2d.2: Re-Transcribe Model Dropdown
**Priority:** High | **Estimate:** 3h

Add a model selection dropdown next to the Re-Transcribe button. Users should be able to re-transcribe a recording with any installed model without changing their global default.

**Changes:**
- Add ComboBox next to Re-Transcribe button showing installed models
- Default selection = current active model
- Re-Transcribe uses the selected model (not the global default)
- Only show installed models in the dropdown

**Acceptance Criteria:**
- [ ] Dropdown visible next to Re-Transcribe button
- [ ] Only installed models shown
- [ ] Defaults to active model
- [ ] Re-transcribing with different model works correctly
- [ ] Original transcription model shown in recording detail (e.g., "Transcribed with: base")

---

### TASK-2d.3: Onboarding Wizard
**Priority:** High | **Estimate:** 4h

*(Previously TASK-2.12)*

First-run experience guiding users through setup:
1. **Welcome** — Value prop, brief intro
2. **Model Selection** — Base is default (bundled). Offer Medium/Large download.
3. **Test Recording** — Record → transcribe → show result
4. **Hotkey Setup** — Show default, allow customization

**Acceptance Criteria:**
- [ ] Wizard shows on first launch only
- [ ] Model download with progress bar
- [ ] Test recording works end-to-end
- [ ] Hotkey configuration works
- [ ] "Don't show again" or completion flag persisted

---

### TASK-2d.4: Update Checker
**Priority:** Medium | **Estimate:** 2h

*(Previously TASK-2.16)*

Check for updates on startup (opt-in auto-update, off by default).

**Changes:**
- Check GitHub releases API for newer version
- Show notification if update available
- Settings toggle for auto-check
- Manual "Check for Updates" in Settings

**Acceptance Criteria:**
- [ ] Checks GitHub releases on startup (if enabled)
- [ ] Shows update notification with download link
- [ ] Settings toggle works
- [ ] Manual check works

---

### TASK-2d.5: In-App Help
**Priority:** Low | **Estimate:** 1h

*(Previously TASK-2.17)*

Minimal help/about section.

**Changes:**
- About dialog (version, credits, links)
- Hotkey reference card
- Link to GitHub issues for support

**Acceptance Criteria:**
- [ ] About dialog accessible from menu/settings
- [ ] Shows version, hotkeys, support link

---

## Task Order
1. **2d.1** (default model) — quick, unblocks everything
2. **2d.2** (re-transcribe dropdown) — UX improvement
3. **2d.3** (onboarding) — first-run experience
4. **2d.4** (update checker) — nice to have for launch
5. **2d.5** (help) — minimal effort

## Test Target
All existing tests pass + new tests for 2d.1, 2d.2, 2d.3.

---

*Note: MSIX Installer & Store Packaging moved to Delivery 2e — only after everything is 100%.*
