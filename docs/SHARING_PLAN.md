# RoadScript Sharing & Distribution Plan

> Make RoadScript's outputs travel further — as a downloadable image, as portable
> markdown, and as an installable app that other apps can hand content to. Three
> independent features sharing a common theme; each ships on its own commit.

---

## Table of Contents

1. [Goals & Non-Goals](#1-goals--non-goals)
2. [Commit Roadmap](#2-commit-roadmap)
3. [Feature 1 — Markdown Export](#3-feature-1--markdown-export)
4. [Feature 2 — SVG Export](#4-feature-2--svg-export)
5. [Feature 3 — PWA Install + Share Target](#5-feature-3--pwa-install--share-target)
6. [Cross-Cutting Concerns](#6-cross-cutting-concerns)
7. [Testing Checklist](#7-testing-checklist)
8. [Risk Register](#8-risk-register)

---

## 1. Goals & Non-Goals

### Goals
- Export the current roadmap as portable Markdown (clipboard + `.md` download).
- Export the current roadmap as a vector SVG file.
- Make RoadScript installable as a PWA on mobile and desktop.
- Wire up Web Share Target so other apps can send content to RoadScript.

### Non-Goals (deferred, do not scope-creep here)
- PNG export — explicit follow-up; SVG first per design decision.
- Full offline / Blazor runtime caching — separate effort. Service worker here is a no-op stub solely to enable `share_target`.
- "Today" line / per-roadmap date range — owned by a separate plan.
- iOS Safari share target — platform does not support `share_target`; document and move on.
- Restyling `ShareModal` beyond adding new sections — keep existing share-link UX untouched.

---

## 2. Commit Roadmap

Each commit is independently reviewable and independently shippable. Suggested
order (smallest blast radius first, with cross-feature primitives extracted along the way):

| # | Commit | Files Touched | Approx Lines | Depends On |
|---|---|---|---|---|
| 1 | Generic JS download + clipboard interop helpers | 1 | ~30 | — |
| 2 | Markdown export service + ShareModal section | 4 | ~250 | 1 |
| 3 | SVG export service + ShareModal section | 4 | ~400 | 1 |
| 4 | PWA manifest + minimal service worker + 512 icon | 4 | ~80 | — |
| 5 | Share-target intake (modal + URL param handling) | 4 | ~150 | 4 |

Branch: `claude/epic-bardeen-RUx2c` (already in flight). Open as one or multiple PRs at your discretion — splitting commits 1+2, 1+3, 4+5 into separate PRs is fine, all five into one is also fine.

---

## 3. Feature 1 — Markdown Export

### 3.1 User-facing behavior
A new **"Export as Markdown"** section in `ShareModal` (below the existing "Export Session Data" section) with two buttons:
- **`📋 Copy to Clipboard`** — copies the rendered markdown to clipboard; brief toast/inline confirmation on success.
- **`📥 Download .md`** — triggers browser download of `<sanitized-title>.md`.

Scope is the **currently-open tab's roadmap**, not the whole session. (Session-level JSON export already exists for the all-data case.)

### 3.2 Output shape
```markdown
# {Title}
_{Subtitle}_

## Columns
- **{label}** _{sub}_   ← one bullet per column, sub in italics if present

## Milestones
- 🚩 **{title}** _(at {start}%)_   ← icon emoji-mapped from item icon name

## {Lane.Title}
_progress: {percent}% ({history.start} – {history.end})_  ← only if `history` present

- **{Item.Title}** _(col {start}, {length} wide)_
  - {markdown from item.Details, re-indented under the bullet}
- ~~**{Greyed Item.Title}**~~ _(blocked)_   ← greyed items rendered with strikethrough
                                                hidden items omitted entirely
```

**Decisions baked in:**
- `hidden: true` items are **omitted** from markdown export.
- `greyed: true` items are kept but wrapped in `~~...~~` with an `_(blocked)_` suffix.
- `spanning: true` items get an `_(ongoing)_` suffix.
- Item icons are mapped to emoji via a small lookup (`flag → 🚩`, `rocket → 🚀`, `star → ⭐`, etc.) with a fallback to `•` for unmapped icons.
- Item `Details` already contains markdown (per `Services/MarkdownRenderer.cs:7`) — emit it verbatim under the item bullet, indented two spaces per nesting level.

### 3.3 Existing primitives leveraged
- `Models/RoadmapModels.cs` — data model.
- `Services/MarkdownRenderer.cs` — items already store markdown in `Details`; no conversion needed.
- `Components/ShareModal.razor` — has the right home and visual conventions (sections with icons + descriptions + primary buttons).

### 3.4 Files
- **New:** `Services/MarkdownExportService.cs` — static class with `public static string Export(RoadmapData data)`. Pure function, no DI, no JS. Build a `StringBuilder`, return the result. Mirrors the style of existing `Services/ShareService.cs`.
- **New:** `Services/MarkdownExportService.cs` includes a small private `IconToEmoji(string? iconName)` helper.
- **Modified:** `Components/ShareModal.razor` — add a new `share-section` block with two buttons. Wire up handlers `CopyAsMarkdown()` and `DownloadAsMarkdown()` in the `@code` block.
- **Modified:** `Components/ShareModal.razor` — add `[Parameter] public RoadmapData? CurrentRoadmap { get; set; }` so the modal has access to the data it's exporting.
- **Modified:** `Pages/Home.razor` — pass `CurrentRoadmap="@_data"` to the `<ShareModal>` invocation (find existing `<ShareModal …>` usage).
- **Modified:** `wwwroot/js/roadscript-interop.js` — add `copyToClipboard(text)` (returns Promise<boolean>) and a generic `downloadTextFile(filename, content, mimeType)` if not already present. **Note:** existing JSON download flow is hardcoded to `application/json` (`wwwroot/js/roadscript-interop.js:459-468`); generalize it OR add the new helper alongside.

### 3.5 Acceptance
- Open any seeded template, click Share → Export as Markdown → Copy.
- Paste into a plain-text editor. Visually confirm: title is H1, subtitle in italics, each lane is an H2, items are bullets with details indented underneath.
- Click Download. File arrives named `<title>.md`, opens cleanly in any markdown viewer.
- Hidden items absent. Greyed items struck through. Spanning items annotated.
- Item icons render as emoji or `•` (no raw icon names leaked).
- Title sanitization: spaces → `-`, strip `/\?<>:*|"` from filename.

---

## 4. Feature 2 — SVG Export

### 4.1 User-facing behavior
A new **"Export as Image"** section in `ShareModal` (below Markdown export) with:
- **`📥 Download SVG`** — triggers browser download of `<sanitized-title>.svg`.

A short blurb beneath: _"Vector format — opens in any browser, Figma, or modern editor. PNG support coming later."_

### 4.2 Implementation approach
**Server-side render via C#** — walk `RoadmapData`, emit an SVG string. Pure function, no DOM dependency.

**Why server-side rather than DOM-snapshot:**
- Independent of viewport / scroll / responsive layout.
- Doesn't require the roadmap to be visible/rendered to export.
- Deterministic output given the same data.
- No external JS library (`html2canvas` etc.) to maintain.
- Pairs cleanly with eventual PNG export (rasterize SVG via canvas in JS).

**Item bodies use `<foreignObject>`** containing the existing markdown-rendered HTML. Trade-off documented in §8 risk register: some image viewers don't honor `<foreignObject>`. v1 accepts this; PNG follow-up will rasterize via canvas which handles `<foreignObject>` correctly.

### 4.3 SVG structure (target)
```
<svg viewBox="0 0 W H" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <style>/* embedded subset of app.css for items, lanes, milestones */</style>
  </defs>

  <g class="title-area">
    <text class="roadmap-title">{Title}</text>
    <text class="roadmap-subtitle">{Subtitle}</text>
  </g>

  <g class="columns-header">
    <!-- one <rect> + <text> per column -->
  </g>

  <g class="milestone-band">
    <!-- header-band milestones (laneIndex == -1) -->
  </g>

  <g class="lanes">
    <g class="lane" transform="translate(0, Y)">
      <rect class="lane-bg" />
      <text class="lane-label">{Lane.Title}</text>
      <g class="history-bar">…</g>
      <g class="items">
        <foreignObject x="…" y="…" width="…" height="…">
          <div xmlns="http://www.w3.org/1999/xhtml">… item markup with inline styles …</div>
        </foreignObject>
      </g>
    </g>
  </g>

  <text class="attribution" x="…" y="…">Made with RoadScript.NET</text>
</svg>
```

Dimensions:
- Width: fixed at **1400px** for v1 (typical desktop roadmap width). Document; PNG follow-up may take a `scale` param.
- Height: computed from `sum(lane heights) + title + columns header + milestone band + footer`.

### 4.4 Layout math
Reuse the same proportions the renderer uses:
- Column width = `(width - laneLabelWidth) / data.Columns.Count`.
- Lane Y-offset = `cumulative sum of (laneBaseHeight * lane.Height)`.
- Item X = `start * columnWidth + laneLabelWidth`.
- Item width = `length * columnWidth`.
- Milestone X = `start% * (width - laneLabelWidth) + laneLabelWidth`.
- Item row stacking inside lanes — for v1, skip the existing greedy row-packing (which lives in components) and lay items out in a single row, **overlapping where they overlap**. This is a v1 limitation; track as known issue if it produces ugly output for your real roadmaps. Promote to full row-packing in a follow-up commit if needed.

### 4.5 Existing primitives leveraged
- `Models/RoadmapModels.cs` — data model.
- `Services/ThemeService.cs` — colors, fonts (read the values; do not depend on DOM-applied CSS).
- `Services/MarkdownRenderer.cs::RenderToHtml(text, laneColor)` — already produces sanitized HTML; reuse verbatim inside `<foreignObject>`.
- `Components/ShareModal.razor` — same modal home as markdown export.

### 4.6 Files
- **New:** `Services/SvgExportService.cs` — `public static string Export(RoadmapData data, ThemeService theme)`. Class composed of small private builders: `BuildTitleSvg`, `BuildColumnsHeaderSvg`, `BuildMilestoneBandSvg`, `BuildLaneSvg`, `BuildItemSvg`. ~400 lines.
- **New:** internal `LayoutMath` static helpers inside `SvgExportService.cs` (avoid extracting to separate file unless reused later).
- **Modified:** `Components/ShareModal.razor` — new section, `DownloadAsSvg()` handler. Reuses the `[Parameter] public RoadmapData? CurrentRoadmap` added in §3.4.
- **Modified:** `wwwroot/js/roadscript-interop.js` — reuse `downloadTextFile` from §3.4 with `mimeType: 'image/svg+xml'`.

### 4.7 Acceptance
- Open any seeded template, click Share → Download SVG.
- File downloads as `<title>.svg`. Open in browser tab: visually resembles the rendered roadmap (title, columns, lanes, items, milestones).
- Open in Figma: imports as native vector with text editable.
- Re-export after editing a single item title: change reflected in next download.
- Empty roadmap (no lanes/items): exports successfully with title + columns only, no errors.
- Long title (>80 chars): truncates with ellipsis or wraps cleanly; doesn't break layout.
- 10+ lanes, 30+ items: file size stays under ~200KB; opens without lag.

---

## 5. Feature 3 — PWA Install + Share Target

### 5.1 User-facing behavior
- **Desktop Chrome/Edge:** Install icon appears in the address bar after first visit. Clicking installs RoadScript as a standalone window.
- **Android Chrome:** "Add to Home Screen" prompt appears after engagement. Once installed, RoadScript appears in the system share sheet.
- **iOS Safari:** Add-to-home-screen works (via the existing apple-touch meta tags); share target is **not supported by the platform** and is silently absent. Document this limitation.
- **Share Target flow (Android):** User in Twitter/Notes/Browser → tap Share → tap RoadScript → RoadScript opens with a small modal pre-filled with the shared text, prompting "Add to which roadmap?" with folder/tab pickers and a Confirm button.

### 5.2 Files
- **New:** `wwwroot/manifest.webmanifest`:
  ```json
  {
    "name": "RoadScript",
    "short_name": "RoadScript",
    "description": "Browser-based roadmap visualization and planning tool",
    "start_url": "/",
    "scope": "/",
    "display": "standalone",
    "background_color": "#1e1e2f",
    "theme_color": "#1e1e2f",
    "orientation": "any",
    "icons": [
      { "src": "/icon-192.png", "sizes": "192x192", "type": "image/png", "purpose": "any" },
      { "src": "/icon-512.png", "sizes": "512x512", "type": "image/png", "purpose": "any maskable" }
    ],
    "share_target": {
      "action": "/",
      "method": "GET",
      "params": { "title": "share_title", "text": "share_text", "url": "share_url" }
    }
  }
  ```
- **New:** `wwwroot/service-worker.js` (minimal no-op — required to qualify as installable on some browsers):
  ```js
  self.addEventListener('install', () => self.skipWaiting());
  self.addEventListener('activate', (e) => e.waitUntil(self.clients.claim()));
  self.addEventListener('fetch', () => { /* network-first; no caching in v1 */ });
  ```
- **New:** `wwwroot/icon-512.png` — **ACTION ITEM:** 512x512 PNG icon needed. Can be produced by upscaling the existing `wwwroot/icon-192.png` (`sips -z 512 512 icon-192.png -o icon-512.png` on macOS, or any image editor). Ideally produce a maskable-safe version (subject within central 80% of frame).
- **Modified:** `wwwroot/index.html`:
  - Add `<link rel="manifest" href="manifest.webmanifest" />` inside `<head>`.
  - Add a small inline `<script>` at the end of `<body>` that calls `navigator.serviceWorker.register('/service-worker.js')` if available, guarded by `'serviceWorker' in navigator`.
- **New:** `Components/ShareTargetModal.razor` — small modal that:
  - Reads `share_title` / `share_text` / `share_url` from query string via `UrlNavigationService`.
  - Pre-fills item title from `share_title` (or first line of `share_text`).
  - Pre-fills item description from `share_text` + `share_url` if present.
  - Renders folder dropdown (default: last-active folder) and tab dropdown (default: last-active tab).
  - Renders lane dropdown (default: first lane in selected tab).
  - "Add to Roadmap" button creates a new `Item` with `start = (columnCount - 1)` (last column, conventional "inbox" position) and appends to the selected lane's items.
- **Modified:** `Pages/Home.razor` — on `OnAfterRenderAsync` first render, check if URL contains `share_title` or `share_text` query params; if present, surface `<ShareTargetModal>`. Clear params from URL after handling via `UrlNavigationService` so a refresh doesn't reopen the modal.
- **Modified:** `Services/UrlNavigationService.cs` — add `(string? title, string? text, string? url) ParseShareTargetParams()` that reads query string and returns the share-target payload.

### 5.3 Existing primitives leveraged
- `wwwroot/index.html` already declares `mobile-web-app-capable`, `apple-mobile-web-app-capable`, theme color, and apple-touch icon meta. PWA-ready apart from manifest + SW.
- `Services/UrlNavigationService.cs` — extend with query-string parsing.
- `Services/StorageService.cs` — folder/tab/item creation already supported.
- `Models/RoadmapModels.cs` — `Item` model fits shared payload (title + description-style detail).

### 5.4 Acceptance
- **Install (desktop Chrome):** Visit roadscript.net → install icon appears in address bar → click → app launches as standalone window with RoadScript icon in the OS dock.
- **Install (Android Chrome):** Visit → "Add to Home Screen" prompt appears (or available via menu) → install → icon on home screen → opens chromeless.
- **Lighthouse PWA audit:** ≥ 90 score on the installability checks (manifest present, valid, icons sized correctly, SW registered, HTTPS).
- **Share Target (Android, after install):** Open Chrome, long-press a link → Share → RoadScript appears in share sheet → tap → RoadScript opens with `<ShareTargetModal>` showing the link text and a folder/tab/lane picker → Confirm creates a new item in the chosen lane.
- **Existing behavior unchanged:** Existing share-link flow (`#share/<code>`) still works; PWA install does not break unauthenticated navigation.
- **No share-target params in URL:** Modal does not surface.

---

## 6. Cross-Cutting Concerns

### 6.1 Where new UI lives
Both Markdown and SVG export live as **new sections inside `ShareModal`**, below the existing "Share This Roadmap" and "Export Session Data" sections. Match the existing visual pattern (`share-section` div with icon + heading + description + primary button). Do not redesign the modal layout.

Section order in modal after this work:
1. Share This Roadmap (existing)
2. Export Session Data (existing)
3. Export as Markdown (new — §3)
4. Export as Image (new — §4)

### 6.2 Service registration
- `MarkdownExportService` and `SvgExportService` are **static** classes (mirror `ShareService`). No DI registration needed.
- No changes to `Program.cs` for export services.
- No changes to `Program.cs` for PWA — service worker registration is in `index.html`.

### 6.3 CI / Deploy considerations
- `azure-deploy.yml` ships `wwwroot/` as the static output, so `manifest.webmanifest`, `service-worker.js`, and `icon-512.png` will deploy automatically once committed.
- The deploy workflow excludes `claude/` branches from production deploy (`.github/workflows/azure-deploy.yml:14`), so this work will only go live when merged to `master`.
- Service worker `fetch` handler is a no-op — no cache invalidation concerns on deploy. If full offline support is later added, that plan must handle cache-busting separately.

### 6.4 Mobile UI
- Markdown / SVG export buttons in `ShareModal` already work on mobile since the modal is shared across layouts.
- Share-target modal must respect mobile breakpoints — reuse existing mobile modal conventions from `Components/Mobile/MobilePropertySheet.razor` for sizing/positioning.

### 6.5 Backward compatibility
- All three features are additive. No data-model changes. No URL-format changes (share-target uses additional query params, doesn't alter existing `#share/...` hash routes).
- Existing share-link, JSON export, and import flows untouched.

---

## 7. Testing Checklist

### Markdown Export
- [ ] Title becomes H1, subtitle becomes italic line below.
- [ ] Each non-empty lane becomes its own H2 section.
- [ ] Items rendered as bullets with start/length annotation in italics.
- [ ] Item `Details` markdown preserved with correct indentation.
- [ ] `hidden: true` items excluded.
- [ ] `greyed: true` items wrapped in `~~ ~~` with `_(blocked)_` suffix.
- [ ] `spanning: true` items annotated `_(ongoing)_`.
- [ ] Milestones rendered in their own H2 section with icon emoji + position.
- [ ] Clipboard copy works in Chrome, Safari, Firefox (HTTPS required for clipboard API).
- [ ] Download produces valid `.md` named `<sanitized-title>.md`.
- [ ] Filename sanitization strips `/\?<>:*|"`.
- [ ] Empty roadmap (no lanes) exports title + subtitle only without crashing.

### SVG Export
- [ ] SVG opens in Chrome, Firefox, Safari standalone.
- [ ] Title, subtitle, columns header visible.
- [ ] Lane backgrounds + lane labels rendered.
- [ ] Items in correct horizontal positions with correct widths.
- [ ] Header-band milestones rendered (those without `laneIndex`).
- [ ] In-lane milestones rendered (those with `laneIndex`, if present).
- [ ] Item titles and details visible (via `<foreignObject>`).
- [ ] Greyed items have reduced opacity.
- [ ] Hidden items absent from SVG.
- [ ] Imports into Figma with editable text.
- [ ] File size stays under ~200KB for a typical 5-lane / 20-item roadmap.
- [ ] Long title doesn't break layout.
- [ ] Empty roadmap exports without errors.
- [ ] Re-exporting after editing reflects the edit.

### PWA Install
- [ ] Lighthouse PWA audit ≥ 90 on installability.
- [ ] Desktop Chrome install icon appears.
- [ ] Installed app launches standalone (no browser chrome).
- [ ] Android Chrome "Add to Home Screen" works.
- [ ] App icon on home screen uses `icon-512.png`.
- [ ] `manifest.webmanifest` validates per W3C spec.
- [ ] Service worker registers without console errors.
- [ ] Existing app behavior unaffected after SW activation.

### Share Target
- [ ] Sharing text from Chrome → RoadScript opens modal.
- [ ] Modal pre-fills with shared title/text/URL.
- [ ] Folder/tab/lane dropdowns populated from current state.
- [ ] Confirm creates item in selected lane.
- [ ] Cancel closes modal without creating an item.
- [ ] URL query params cleared after handling (refresh doesn't re-open modal).
- [ ] No-share-params visit doesn't surface modal.
- [ ] iOS Safari: install works, share target gracefully absent.

---

## 8. Risk Register

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|---|---|---|
| R1 | SVG `<foreignObject>` doesn't render in Slack/Notion image previews | Medium | Medium | Document as v1 limitation. PNG follow-up (rasterized via canvas) will solve this for those destinations. |
| R2 | Clipboard API requires HTTPS + user gesture | Low | Low | Production is HTTPS. Button click is a user gesture. Localhost dev works on Chrome via `localhost` exception. |
| R3 | Service worker breaks Blazor hot-reload during `dotnet watch` | Medium | Low | Register SW only when `location.hostname !== 'localhost'`, or unregister-on-localhost helper in `index.html`. Document. |
| R4 | iOS Safari doesn't support `share_target` | Known | Low | Not a regression — feature is additive. Document as Android/desktop Chrome feature. |
| R5 | Missing 512x512 icon blocks PWA install | High | Medium | Plan calls out as ACTION ITEM in §5.2. Quick to produce via `sips` or image editor. |
| R6 | SVG layout math drifts from rendered layout over time | Medium | Low | v1 acknowledges single-row item layout (no row packing). If real-use roadmaps overflow, promote to row-packing in a follow-up commit. |
| R7 | Adding `CurrentRoadmap` parameter to `ShareModal` breaks existing call sites | Low | Low | Parameter is nullable with sensible default; only one call site in `Pages/Home.razor`. |
| R8 | `manifest.webmanifest` MIME type misconfigured on Azure SWA | Low | Medium | Azure SWA serves `.webmanifest` correctly by default. Verify with browser devtools after first deploy. |
| R9 | Service worker caches stale Blazor framework files across deploys | None (v1) | — | v1 SW does no caching. Defer to future offline-support effort. |

---

## Appendix A — Icon-to-Emoji Mapping (Markdown Export)

For v1 use a compact lookup; expand as needed. Unmapped icons fall back to `•`.

```
flag      → 🚩       rocket    → 🚀       star      → ⭐
diamond   → 💎       checkbox  → ✅       warning   → ⚠️
fire      → 🔥       bug       → 🐛       lightning → ⚡
heart     → ❤️        target    → 🎯       clock     → ⏰
calendar  → 📅       gear      → ⚙️        chart     → 📊
question  → ❓       lock      → 🔒        key       → 🔑
```

Define in `Services/MarkdownExportService.cs` as `private static readonly Dictionary<string, string> IconEmojiMap`.

---

## Appendix B — Out of Scope (Explicit)

These came up in discussion but are deferred to keep this plan executable:

- **PNG raster export** — follow-up after SVG ships. Implementation: load SVG into a canvas via `Image` + `canvas.drawImage`, then `canvas.toBlob('image/png')`. Single new button in the existing "Export as Image" section.
- **Full offline support** — separate plan. Requires SW to cache `_framework/*`, `_content/*`, `dotnet.*.wasm`, app shell, with deploy-time cache-busting via build hash.
- **"Today" line / per-roadmap date range** — separate feature plan.
- **Sub-item check-off / done state** — separate feature plan.
- **Bulk-paste-text-as-items** — separate feature plan.
- **Restyling the ShareModal layout** — explicitly out of scope. Add sections; do not redesign.
