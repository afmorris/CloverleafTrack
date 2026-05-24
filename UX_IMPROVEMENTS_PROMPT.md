# Cloverleaf Track UX Improvements

You are working in the locally-cloned `CloverleafTrack` repository — an ASP.NET Core MVC site (Razor views, Tailwind, server-rendered) that publishes a track & field team's results, roster, leaderboard, and meet history. The visual redesign looks nicer than the old site but has regressed in usability. Your job is to implement the changes below.

## Working principles

- Preserve the existing visual language (fonts, spacing, dark mode, gradient-on-accent style) — these changes are about **usability**, not a re-skin. When in doubt, keep the new look and fix the structure underneath.
- Keep everything server-rendered where possible. Add small amounts of vanilla JS only when interactivity genuinely needs it (filtering, search, tab persistence). Don't introduce a frontend framework.
- All new interactive UI must work without JavaScript loaded (progressive enhancement): a no-JS visitor should still see meaningful, scannable content.
- All new interactive UI must be keyboard-navigable and screen-reader-friendly. Use semantic HTML (`<button>`, `<details>`, `<input type="search">`) and `aria-*` attributes where appropriate. Do not use emoji as the *only* indicator for a piece of functional information; pair with text or `aria-label`.
- Mobile-first. Test that every new view collapses cleanly at narrow widths (~380px) before considering it done.
- After each major change below, run the existing test suite (`dotnet test`) and fix any breakage. Add new tests for new logic where it makes sense (filter predicates, search indexers).
- Each numbered section below is a discrete unit of work and a reasonable commit boundary. Do them in the order listed unless there's a strong reason to deviate.

---

## 1. Remove in-season graphs from athlete detail pages

**Why:** The "Career Progression" section on athlete detail pages (`Views/Roster/Details.cshtml`) currently renders one block per event with stats like "Career PR / Competitions / Career improvement / All-time rank" plus what appears to be intended as a chart/visualization that loads very poorly on mobile and provides limited value relative to the cost.

**What to do:**

- Locate the "Career Progression" section on the athlete details view.
- Remove the graph/chart rendering entirely.
- Keep the numerical summary stats (Career PR, Competitions count, Career improvement, All-time rank) but consolidate them into a single compact row or small grid at the top of each event's section, not a per-event large block.
- If any JS libraries (Chart.js, D3, etc.) were included *only* for these graphs, remove them from the layout and from `wwwroot/lib` or wherever they live.
- The "Performance by Season" section below it can stay — it's a flat list of performances, which is fine.

---

## 2. Add a global search

**Why:** Right now the only way to find any athlete, meet, or event is to scroll. This is the single biggest usability gap vs. Athletic.net / MileSplit.

**What to do:**

- Add a search input to the top navigation (`Views/Shared/_Layout.cshtml` or `_MainNavigation.cshtml`) — placed to the left of the dark mode toggle. Use `<input type="search">` with a placeholder like "Search athletes, meets, events…".
- The search should be **client-side**, backed by a static JSON index generated server-side at request time (or cached). Generate a `/search-index.json` endpoint (or embed the index inline in the layout via a `ViewComponent`) that contains an array of search records, each with: `{ type: "athlete" | "meet" | "event", label, sublabel, url }`.
  - Athletes: label = full name, sublabel = class + gender, url = athlete details page.
  - Meets: label = meet name, sublabel = date + location, url = meet details.
  - Events (leaderboard): label = "Boys 1600M (Outdoor)" etc., sublabel = current record holder + mark, url = leaderboard event detail.
- Wire the search input to a small vanilla JS module (`wwwroot/js/search.js`) that:
  - Loads the index on first focus of the search input (not on page load — avoids inflating every request).
  - Does a simple case-insensitive substring + token-prefix match against `label` and `sublabel`.
  - Renders results as a dropdown below the input, grouped by type (Athletes / Meets / Events) with up to 5 results per group.
  - Supports arrow-key navigation and Enter to navigate.
  - Closes on outside click or Escape.
- On mobile, the search input should expand to a full-width sheet when focused so results aren't crammed against the nav.
- No-JS fallback: if JS is disabled, the search input can submit to a server-side `/search?q=…` action that renders a basic results page. (If that's too much scope, at minimum make sure the input doesn't break the layout when JS is off.)

---

## 3. Add filter chips to Roster, Leaderboard, and Meets

**Why:** Once a visitor lands on a list page, they need to narrow it down. Currently they can't.

**What to do:**

### Roster (`Views/Roster/Index.cshtml`)

Add a filter bar above the athletes list with these chip groups:

- **Status:** Current Athletes | Former Athletes | All  *(default: Current)*
- **Gender:** All | Boys | Girls
- **Category:** All | Sprints | Distance | Hurdles | Jumps | Throws
- **Class** *(only when Status = Current or All):* All | Freshman | Sophomore | Junior | Senior  
  When Status = Former, show graduation-year chips instead: All | 2025 | 2024 | 2023 | …

Chips are toggle buttons. Filtering is client-side via a small JS module that hides/shows athlete cards based on `data-*` attributes you'll add to each card (`data-gender`, `data-category`, `data-class`, `data-status`).

### Leaderboard (`Views/Leaderboard/Index.cshtml`)

Add filter chips above the records list:

- **Environment:** Outdoor | Indoor *(replaces the existing two-tab control — see section 4 below)*
- **Gender:** All | Boys | Girls | Mixed Relays
- **Category:** All | Sprints | Distance | Hurdles | Relays | Jumps | Throws

### Meets (`Views/Meets/Index.cshtml`)

Add filter chips above the seasons list:

- **Environment:** All | Outdoor | Indoor
- **Season:** dropdown listing every season (2025-2026, 2024-2025, …) with "All seasons" as default. Selecting one collapses everything except that season.

### Implementation notes for chips

- Use a consistent component-like partial (`_FilterChipGroup.cshtml`) that takes a label, a list of options, the active option, and a `data-filter-key` attribute.
- The active chip should have a clear visual treatment (filled accent color); inactive chips are outlined/ghost.
- A shared `wwwroot/js/filters.js` module wires up all chip groups: on click, toggle `data-active`, then walk the list of items below it and apply `hidden` based on which `data-*` values match the active chips.
- Persist filter state in the URL hash (`#category=throws&gender=boys`) so links are shareable and back-button works.
- All filter chips must be `<button>` elements with `aria-pressed="true|false"`.

---

## 4. Replace Outdoor/Indoor tabs with segmented control or stacked sections

**Why:** The current tab pattern hides content. First-time visitors don't realize there's indoor data unless they happen to notice the tab.

**What to do:**

- On the **Leaderboard page**, remove the Outdoor/Indoor tab control entirely. Instead, the new Environment filter chip group (from section 3) handles the choice. Default to "Outdoor" during outdoor season and "Indoor" during indoor season — determine season programmatically based on current date (Mar–Jun = outdoor default, Dec–Feb = indoor default; otherwise outdoor).
- On the **Home page** (`Views/Home/Index.cshtml`), the "Season Leaders" and "Recent Highlights" tabs should also be replaced with a segmented control styled as `[ Outdoor | Indoor ]` showing event counts in parens, e.g. `Outdoor (47)  Indoor (12)`. This makes it obvious that both exist. Use the same date-based defaulting.
- Persist the user's environment choice in `localStorage` under key `ctf.environment` so it carries across pages within a session.
- The Home page tab labels currently render via JS-applied gradient classes — move that styling into Razor so the segmented control has its active state on first paint (no flash of unstyled tabs).

---

## 5. Roster: one card per athlete + category filter

**Why:** Currently a multi-event athlete appears in multiple sections (sprints, jumps, etc.), which inflates the list and makes the same athlete repeat 3+ times.

**What to do:**

- Refactor the roster index to render **one card per athlete**, not one per athlete-per-category.
- Each card shows the athlete's name, class, gender icon, and a compact list of their top events across all categories. Show up to 4 events; if they have more, show "+ N more" as before.
- The category filter chip (from section 3) controls which athletes are visible and which events show on the card:
  - When `category=all`, show all athletes with their top 4 events (any category).
  - When `category=throws`, show only athletes who have at least one throws event, and only show their throws events on the card.
- Drop the existing per-category headings (`### Sprints`, `### Distance`, etc.) on the Current Roster section. The filter chips replace them.
- Keep alphabetical sort within the filtered list as the default. Add a sort dropdown with options: **Name (A→Z)**, **Class (Senior → Freshman)**, **Top event** (by best mark relative to school record — this can be a future enhancement; if too complex, omit for now).
- The Former Athletes section keeps its class-of-year grouping, but **default each year's accordion to collapsed**. Show just the year header with the athlete count, expandable on click. Use `<details><summary>` for this.

Relevant files: `Views/Roster/Index.cshtml`, `Views/Shared/_RosterActiveAthletesList.cshtml`, `Views/Shared/_AthleteCategorySection.cshtml`, `Views/Shared/_AthleteCard.cshtml`, `Views/Shared/_RosterFormerAthletesList.cshtml`, `Views/Shared/_FormerAthleteYearGroupSection.cshtml`, and probably the `RosterViewModel` to support a flat athlete list with all events per athlete.

---

## 6. Meets: reverse-chronological + flatten the season groupings

**Why:** Visitors arrive looking for the most recent results, not the 19th-most-recent. The current expand-from-3 pattern also creates a visual break between the always-shown and on-expand meets.

**What to do:**

- Within each season, sort meets **reverse-chronologically** (most recent first).
- For the **current season**, show all completed meets in one continuous list (no Take(3)+collapse). Upcoming meets get their own clearly-separated section *above* the completed meets, titled "Coming Up" with a green left-border treatment.
- For **past seasons**, default the entire season to a single collapsed `<details>` row that shows season name + total meet count + PR/SR badges. On expand, show all meets in that season in one continuous list (no nested collapse inside).
- The current-season section should not be wrapped in `<details>` — always open.
- Update `_MeetsCard.cshtml` and `_MeetsSeasonsList.cshtml` accordingly.

---

## 7. Active-athlete leaderboards per event

**Why:** A common question — "who's the top current 1600m runner on the team?" — has no good answer today. Season leaders on the home page only show top 3.

**What to do:**

- On each leaderboard event detail page (`Views/Leaderboard/Details.cshtml`), add a new section **above** the all-time top performances called "Current Season — Active Athletes". This shows the top 10 currently-rostered athletes ranked by their season-best mark in this event for the current season. If fewer than 10 exist, show however many there are.
- Also add a link from each event card on the main leaderboard page to its detail page (the event name is already linked — verify it works).
- If feasible without a major refactor, add a roster-page secondary view: at the top of the roster, near the filter chips, add a small toggle "By Athlete | By Event". When "By Event" is active, the roster page renders as a list of events, each showing the top 5 active athletes for that event ranked by season-best. Default is "By Athlete". If this is more than a day of work, skip it for now and just do the leaderboard-detail addition — note it as a follow-up.

---

## 8. Visual polish

**Why:** Too many gradient surfaces means nothing draws the eye.

**What to do:**

- Audit every `bg-gradient-*` use across the views. Restrict gradients to **two cases only**:
  1. The "hero number" stat tiles on the home page Season-at-a-Glance and on the meets summary (these *should* pop).
  2. The active state of the environment filter chips.
- Replace all other gradient backgrounds (meet list items, athlete cards, leader cards, event category headers, etc.) with flat neutral surfaces (`bg-white dark:bg-gray-800` with subtle borders).
- The Home page "Season at a Glance" stats are currently all the same size. Promote "School Records" to ~1.5x larger than the others, since it's the most exciting number for visitors. The "X of Y meets" progress stat should be the smallest and least prominent (it's mostly informational, not celebratory).
- Heading hierarchy: standardize H1 to `text-4xl font-bold`, H2 to `text-2xl font-bold`, H3 to `text-lg font-semibold`. Audit pages and apply consistently.
- Move the inline `<style>` from `_AthleteCard.cshtml` into `wwwroot/css/site.css`.
- Fix the dark-mode flash: move the theme initialization into a synchronous inline `<script>` block in `<head>` *before* the stylesheet link, so the `dark` class is set on `<html>` before first paint.

---

## 9. Accessibility & semantics pass

**Why:** Emoji-as-UI and missing semantics hurt screen reader users and aren't reliable across platforms.

**What to do:**

- Wherever ☀️ / 🏢 is used as a functional indicator (filter labels, event badges, list items), pair the emoji with a text label and add `aria-label` to any standalone instances. Or replace with small SVG icons in `wwwroot/img/icons/` and reference them inline.
- Ensure all `<a>` tags wrapping cards (`_AthleteCard.cshtml`, `_MeetListItem.cshtml`) have meaningful link text or `aria-label` — currently they wrap mixed content and screen readers will read everything inside, which is fine but verbose.
- Make sure the filter chips are real `<button>` elements with `aria-pressed`, not styled divs.
- Make sure the search dropdown has `role="listbox"` and items have `role="option"`.
- Verify color contrast for the gray-on-gray text used for sublabels in dark mode (e.g. meet location text in `_MeetListItem.cshtml`). The current `text-gray-400` on `bg-gray-800` is borderline.

---

## 10. Small fixes / cleanup

- **Footer:** Add the current copyright year (server-side: `@DateTime.Now.Year`), a link to Cloverleaf High School's main athletic page if one is known (otherwise leave a placeholder), and keep "Made with ♥ by Coach Tony". Modest layout: copyright on left, "Made with ♥…" on right, school link in the middle.
- **Page titles:** Every page's `<title>` should follow the pattern `{Page name} · Cloverleaf Track & Field`. Currently inconsistent (some are `{Title} - Cloverleaf Track`).
- **`<meta name="description">`:** Add per-page descriptions for SEO. Home: team summary + current season highlights. Athletes: athlete name + top events. Meets: meet name + date + location. Leaderboard: event-specific.
- Remove the unused `links` array declared and then re-iterated in `_MainNavigation.cshtml` — the first foreach loop (lines 15–19) computes nothing and is dead code.
- Audit for any other dead code, unused partials, or commented-out blocks while you're in there.

---

## What's out of scope

Don't do these — they're for a later round:

- Year-over-year meet comparison (linking a meet to its prior-year edition).
- Class-records / freshman-records leaderboard tab.
- Coaching staff page.
- News / announcements feature.
- Adding charts back in any form.
- Database schema changes.
- Authentication or admin-side changes beyond what's necessary to keep admin views compiling.

---

## When you're done

- Run `dotnet build` and `dotnet test` and ensure both pass cleanly.
- Run the site locally and click through every page (home, roster, athlete detail, leaderboard, leaderboard event detail, meets, meet detail, seasons) verifying:
  - The search works from every page.
  - Filter chips work and persist via URL hash.
  - Mobile (DevTools 380px width) renders cleanly with no horizontal scroll.
  - Dark mode has no flash on load.
  - Keyboard navigation works for search and filter chips.
- Write a short summary in the final response describing what was done, what was deferred, and any decisions you made where the prompt was ambiguous.
