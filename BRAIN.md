# BRAIN.md — AI-Readable Accepted Changes Log

This file documents all accepted, shipped changes to CloverleafTrack that are not obvious from reading the current code. It is written for future AI assistants to load alongside CLAUDE.md. Read CLAUDE.md first for architecture context, then use this file to understand what changed from the original implementation and why.

---

## Format

Each entry is:
- **What changed** — the code that was written
- **Why** — the reason / bug being fixed
- **Key files** — exact paths to changed files
- **Watch out** — gotchas or related places to keep in sync

---

## Change Log

---

### [C1] Outdoor-First Tab Ordering (All Pages)

**What changed:**
All pages with Indoor/Outdoor tab pairs now default to showing the OUTDOOR tab first. The tab that renders first in HTML is Outdoor. `DOMContentLoaded` fires `.click()` on the outdoor tab button to ensure it is visually active on load.

**Why:**
Default rendering landed on Indoor. Outdoor is the primary season; it should be the default view.

**Key files:**
- `CloverleafTrack.Web/Views/Leaderboard/Index.cshtml`
- `CloverleafTrack.Web/Views/Seasons/Details.cshtml`
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`

**Pattern:**
```js
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('tab-outdoor').click();
});
```

---

### [C2] Ascending Meet Ordering on Season Detail + Meets Index

**What changed:**
Meets are displayed in ascending date order (oldest meet first) on both the Season Details page and the Meets index page.

**Why:**
Meets were displaying in inconsistent order. Ascending chronological order is the natural reading order for a season's schedule/results.

**Key files:**
- `CloverleafTrack.Services/MeetService.cs` — `GetMeetsIndexAsync`, `GetSeasonDetailsAsync`
- `CloverleafTrack.DataAccess/Repositories/MeetRepository.cs` — `GetMeetsForSeasonAsync` uses `ORDER BY m.Date ASC`

**Watch out:**
`Details.cshtml` for Season Details applies an in-view `OrderBy(m => m.Date)` on the ViewModel list. This is an intentional exception to the "sort in service layer" rule because the ViewModel list is reused in multiple contexts.

---

### [C3] Relay Field Event ModelState Bug Fix

**What changed:**
The admin Performance `Create` action unconditionally removes all `RelayAthleteIds.*` keys from `ModelState` before calling `ModelState.IsValid`. Non-positive IDs are then filtered from the relay athlete list before insert.

**Why:**
Hidden relay slot `<select>` elements submit empty string `""` which fails `int` model binding, producing spurious ModelState errors that blocked valid relay performance entry.

**Key files:**
- `CloverleafTrack.Web/Areas/Admin/Controllers/PerformancesController.cs`

---

### [C4] Mixed Relay Support

**What changed:**
The leaderboard, meet details page, and admin forms now fully support `Gender.Mixed` (= 3) relay events.

**LeaderboardViewModel** — added two new properties:
```csharp
public List<LeaderboardCategoryViewModel> MixedOutdoorCategories { get; set; } = new();
public List<LeaderboardCategoryViewModel> MixedIndoorCategories { get; set; } = new();
```

**LeaderboardService** — `GetLeaderboardAsync` now filters for `Gender.Mixed` and populates both new lists.

**New partial** — `CloverleafTrack.Web/Views/Shared/_LeaderboardMixedSection.cshtml`:
- Model: `Tuple<bool, List<LeaderboardCategoryViewModel>>` (Item1 = isIndoor)
- Uses purple `border-purple-500` accent color
- Renders relay member links via `evt.RelayMembers` and `evt.MeetSlug`

**Leaderboard/Index.cshtml** — after Boys/Girls grid in each environment tab:
```html
@if (Model.MixedOutdoorCategories.Any())
{
    <!-- Mixed Relays section with purple heading -->
    @await Html.PartialAsync("_LeaderboardMixedSection", new Tuple<bool, List<LeaderboardCategoryViewModel>>(false, Model.MixedOutdoorCategories))
}
```

**Meets/Details.cshtml** — full-width Mixed Relays section rendered after Boys/Girls grid when `Model.MixedEvents.Any()`.

**Key files:**
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardViewModel.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.Web/Views/Shared/_LeaderboardMixedSection.cshtml` (NEW)
- `CloverleafTrack.Web/Views/Leaderboard/Index.cshtml`
- `CloverleafTrack.Web/Views/Meets/Details.cshtml`

---

### [C5] RunningRelayEvents Table — Separate from Events

**What changed:**
Mixed relay events are stored in a separate `RunningRelayEvents` table, NOT in the `Events` table.

**Why:**
The `Events` table schema does not cover relay-specific fields. `RunningRelayEvents` has its own schema.

**RunningRelayEvents schema:**
```sql
CREATE TABLE RunningRelayEvents (
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    -- (other columns per actual DDL provided by user)
)
```

**Key watch-out:**
- `Id` is `UNIQUEIDENTIFIER`, NOT an auto-increment `INT`. Use `NEWID()` on insert.
- There is NO `EventKey` column in `RunningRelayEvents`.
- When inserting mixed relay events, target `RunningRelayEvents`, not `Events`.

---

### [C6] AthletePerformanceDto — Added SeasonStartDate and RelayAthletes

**What changed:**
Two fields added to `AthletePerformanceDto`:

```csharp
public DateTime SeasonStartDate { get; set; }
public string? RelayAthletes { get; set; }
```

**Why:**
- `SeasonStartDate`: SQL `UNION ALL` cannot `ORDER BY` a non-selected column. `s.StartDate` must be projected into both branches so the wrapping subquery can order by it.
- `RelayAthletes`: relay team members are passed through from SQL `STRING_AGG` to the service and view layers.

**Key files:**
- `CloverleafTrack.DataAccess/Dtos/AthletePerformanceDto.cs`

---

### [C7] AthleteRepository — UNION ALL for Individual + Relay Performances

**What changed:**
`GetAllPerformancesForAthleteAsync` was rewritten from a single-table query to a UNION ALL query that returns both individual and relay performances for a given athlete.

**Why:**
Relay performances have `AthleteId = NULL` on the `Performance` row; the athlete link is in `PerformanceAthletes`. The original query only returned individual performances.

**SQL pattern:**
```sql
SELECT * FROM (
    -- Branch 1: Individual performances (AthleteId = @AthleteId)
    SELECT p.Id as PerformanceId,
           ...,
           s.StartDate as SeasonStartDate,
           NULL as RelayAthletes
    FROM Performances p
    INNER JOIN Events e ON e.Id = p.EventId
    INNER JOIN Meets m ON m.Id = p.MeetId
    INNER JOIN Seasons s ON s.Id = m.SeasonId
    WHERE p.AthleteId = @AthleteId

    UNION ALL

    -- Branch 2: Relay performances where athlete is in PerformanceAthletes
    SELECT p.Id as PerformanceId,
           ...,
           s.StartDate as SeasonStartDate,
           (SELECT STRING_AGG(a2.FirstName + ' ' + a2.LastName, '|~|')
            FROM PerformanceAthletes pa2
            INNER JOIN Athletes a2 ON a2.Id = pa2.AthleteId
            WHERE pa2.PerformanceId = p.Id) as RelayAthletes
    FROM Performances p
    INNER JOIN PerformanceAthletes pa ON pa.PerformanceId = p.Id
    INNER JOIN Events e ON e.Id = p.EventId
    INNER JOIN Meets m ON m.Id = p.MeetId
    INNER JOIN Seasons s ON s.Id = m.SeasonId
    WHERE pa.AthleteId = @AthleteId AND p.AthleteId IS NULL
) AS combined
ORDER BY SeasonStartDate DESC, EventCategorySortOrder, MeetDate DESC
```

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/AthleteRepository.cs`

**Watch out:**
- Both UNION branches must select `s.StartDate as SeasonStartDate` — required for outer ORDER BY.
- `STRING_AGG` separator is `|~|`. Split in C# using `RelayAthletes?.Split("|~|")`.
- Relay athlete name format from SQL is `FirstName LastName` (not `LastName, FirstName`).

---

### [C8] Relay PRs in Roster Details — PersonalBest Flag Unreliable

**What changed:**
`AthleteService.GetAthleteDetailsAsync` computes relay Personal Records via best-per-event logic, NOT by relying on the `PersonalBest` flag.

**Why:**
The admin performance entry form does NOT reliably set `PersonalBest = true` on relay `Performance` rows. Relying on the flag caused relay events to appear in the season section but never in the Personal Records table.

**Pattern in AthleteService:**
```csharp
// Individual PRs — rely on the PersonalBest flag (reliable for individuals)
var individualPRs = performances
    .Where(p => p.PersonalBest && p.RelayAthletes == null)
    .GroupBy(p => p.EventId)
    .Select(g => g.OrderByDescending(p => p.MeetDate).First());

// Relay bests — compute best per relay event regardless of flag
var relayBests = performances
    .Where(p => p.RelayAthletes != null)
    .GroupBy(p => p.EventId)
    .Select(g => g.First().TimeSeconds.HasValue
        ? g.OrderBy(p => p.TimeSeconds).First()
        : g.OrderByDescending(p => p.DistanceInches).First());

var personalRecords = individualPRs.Concat(relayBests).Select(...).ToList();
```

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`

---

### [C9] School Records Recalculation — AllTimeRank == 1 Proxy (Individual + Relay)

**What changed:**
`IsSchoolRecord` for relay performances in `PersonalRecordViewModel` uses `AllTimeRank == 1` as the school record proxy. `TotalSchoolRecords` in `AthleteDetailsViewModel` uses `AllTimeRank == 1` for **both** individual and relay performances (distinct by EventId).

**Why:**
The `SchoolRecord` flag on `Performance` rows is NOT reliably cleared when a newer performance supersedes the record — `sp_RebuildLeaderboards` does not reset it. An athlete with a relay at #1 all-time or an individual event where the flag is stale would show 0 School Records in the hero. Using `AllTimeRank == 1` from the Leaderboards table (which IS kept current by the SP) is authoritative for both cases.

**Pattern in AthleteService:**
```csharp
IsSchoolRecord = p.RelayAthletes == null ? p.SchoolRecord : p.AllTimeRank == 1,
```

```csharp
TotalSchoolRecords = performances
    .Where(p => p.AllTimeRank == 1)
    .Select(p => p.EventId)
    .Distinct()
    .Count(),
```

**Note:** The `IsSchoolRecord` mapping in `PersonalRecordViewModel` still uses `p.SchoolRecord` for individual rows (via the ternary above). That is fine for the PR table display because `sp_RebuildLeaderboards` does update `SchoolRecord` for individual performances after it is fixed (see C17). The hero count must not use the flag because it may be stale before the SP fix runs.

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.ViewModels/Athletes/PersonalRecordViewModel.cs`
- `CloverleafTrack.ViewModels/Athletes/AthleteDetailsViewModel.cs`

---

### [C10] ViewModels — Relay Member Fields

**What changed:**
Added relay support fields to three ViewModels:

**`IndividualPerformanceViewModel`:**
```csharp
public string? RelayAthletes { get; set; }
public bool IsRelay => RelayAthletes != null;
public string[] RelayMembers => RelayAthletes?.Split("|~|") ?? Array.Empty<string>();
```

**`PersonalRecordViewModel`:**
```csharp
public bool IsSchoolRecord { get; set; }
public string? RelayAthletes { get; set; }
public bool IsRelay => RelayAthletes != null;
public string[] RelayMembers => RelayAthletes?.Split("|~|") ?? Array.Empty<string>();
```

**`AthleteTopEventViewModel`:**
```csharp
using Environment = CloverleafTrack.Models.Enums.Environment;
public Environment Environment { get; set; }
```

**Key files:**
- `CloverleafTrack.ViewModels/Athletes/IndividualPerformanceViewModel.cs`
- `CloverleafTrack.ViewModels/Athletes/PersonalRecordViewModel.cs`
- `CloverleafTrack.ViewModels/Athletes/AthleteTopEventViewModel.cs`

---

### [C11] Roster Details Page — Relay Display and Enhancements

**What changed — hero section:**
TopSprintEvent and TopFieldEvent now show an Indoor/Outdoor badge:
```cshtml
@if (Model.TopSprintEvent.Environment == CloverleafTrack.Models.Enums.Environment.Indoor)
{ <span>🏢 Indoor</span> }
else { <span>☀️ Outdoor</span> }
```

**What changed — Personal Records table:**
Relay team members displayed below the event name as linked names joined by ` / `:
```cshtml
@if (pr.IsRelay)
{
    <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">
        @for (int i = 0; i < pr.RelayMembers.Length; i++)
        {
            <a href="/athletes/@...">@pr.RelayMembers[i]</a>
            @if (i < pr.RelayMembers.Length - 1) { <span> / </span> }
        }
    </div>
}
```

SR (School Record) badge in the Rank column shown when `pr.IsSchoolRecord` is true (even when AllTimeRank is null — covers relay records).

**What changed — season performance rows:**
Relay team members shown below the performance mark/date/meet row using the same ` / ` separator pattern.

**Key files:**
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`

**Watch out:**
`hasTopTenRanks` check was expanded to `|| pr.IsSchoolRecord` so the Rank column renders for school records even when the athlete has no top-10 ranked individual performances.

---

### [C12] Season Ordering Fix — SeasonStartDate GroupBy Key

**What changed:**
Season grouping in `GetAthleteDetailsAsync` now groups by both `SeasonName` AND `SeasonStartDate`, and orders by `SeasonStartDate` descending.

**Why:**
Grouping by `SeasonName` string alone gave no reliable ordering. The new `SeasonStartDate` DTO field enables correct descending chronological ordering.

**Pattern:**
```csharp
var seasons = performances
    .GroupBy(p => new { p.SeasonName, p.SeasonStartDate })
    .OrderByDescending(g => g.Key.SeasonStartDate)
    .Select(...)
```

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`

---

## Relay Flag Reliability — Summary Table

This is the most important behavioral invariant to remember:

| Flag | Individual performances | Relay performances |
|---|---|---|
| `PersonalBest` | ✅ Reliable — set by admin form | ❌ NOT reliable — may be false even for best relay |
| `SchoolRecord` | ⚠️ Snapshot only — NOT cleared when a newer record supersedes it | ❌ NOT reliable — may be false even for #1 all-time relay |
| `AllTimeRank` | ✅ Set by `sp_RebuildLeaderboards` — use as current SR indicator | ✅ Set by `sp_RebuildLeaderboards` — use as SR proxy |

**Rule:** Never use `p.SchoolRecord` to determine whether a performance is *currently* the school record. It means "was the record when this flag was last written." Use `AllTimeRank = 1` from the Leaderboards table instead. `sp_RebuildLeaderboards` keeps Leaderboards current but does NOT retroactively clear the `SchoolRecord` flag on Performance rows that have been beaten.

---

### [C13] Unit Test Suite — Initial Build-Out

**What changed:**
A full xUnit unit test suite was written and made green across `CloverleafTrack.Tests/Unit/`:

| File | Tests | What is covered |
|---|---|---|
| `Services/SeasonServiceTests.cs` | 2 | `GetCurrentSeasonAsync` returns `EndDate.Year` (not Id); throws `InvalidOperationException` when no current season |
| `Services/MeetServiceTests.cs` | 13 | Null slug, meet info, PR/SR counts, Boys/Girls/Mixed splits, event category ordering, meets index grouping and season ordering |
| `Services/LeaderboardServiceTests.cs` | 11 | Gender/environment partitioning, Mixed isolation, category grouping, relay-type separation, details null-when-empty, IsRelayEvent detection, PRs-only de-duplication |
| `Services/AthleteServiceTests.cs` | 18 | Active/former roster grouping, PR formatting, null slug, relay PR via best-per-event (not flag), TotalPRs counts only individuals, TotalSchoolRecords includes relay AllTimeRank==1, relay member parsing |
| `Utilities/PerformanceFormatHelperTests.cs` | 43 | `ParseTime` / `FormatTime` (sub-minute, colon, suffix, invalid), `ParseDistance` / `FormatDistance` (feet+inches, dash, total-inches, natural language, invalid), `FormatPerformance`, `FormatImprovement`, round-trip theories |

**Why:**
No automated test coverage existed. Tests were needed to lock in the relay flag workarounds (C8, C9) and the service-layer business logic that is not obvious from reading the code.

**Bugs found and fixed during test authoring:**

1. `SeasonServiceTests` constructor — `performanceRepository` and `meetRepository` were declared as fields but never initialized with `new Mock<...>()`, causing `NullReferenceException` on construction.

2. `SeasonServiceTests.GetCurrentSeason_ReturnsCorrectSeasonId` — asserted `result == 3` (the season Id) but `GetCurrentSeasonAsync()` returns `EndDate.Year`. Fixed assertion to `today.AddYears(2).Year`.

3. `PerformanceFormatHelperTests.ParseDistance_RoundTrip_WithFormatDistance` — theory assumed `FormatDistance` round-trips to the same compact input format (`"19'4"`) but the helper always produces `"19' 4\""` (canonical with space). Fixed by adding a third `InlineData` argument for the expected formatted string.

**Key files:**
- `CloverleafTrack.Tests/Unit/Services/SeasonServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Services/LeaderboardServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs`
- `CloverleafTrack.Tests/Unit/Utilities/PerformanceFormatHelperTests.cs`

**Watch out:**
- `GetCurrentSeasonAsync()` returns `EndDate.Year` (an int representing the calendar year), not the season DB row Id. This is intentional — callers use the year as a display/filtering key.
- `FormatDistance` canonical output always includes a space: `"F' I\""`. The `ParseDistance` input parser accepts multiple input formats (compact, dash, natural language) but `FormatDistance` output is always the spaced canonical form.

---

## Relay Athlete Name Format

- **Stored in DB:** `STRING_AGG(a.FirstName + ' ' + a.LastName, '|~|')`
- **Format:** `FirstName LastName` (NOT `LastName, FirstName`)
- **Separator:** `|~|` (chosen to be unlikely to appear in real names)
- **C# split:** `RelayAthletes?.Split("|~|") ?? Array.Empty<string>()`
- **Display format:** Names joined by ` / ` inline, each linked to athlete's roster page

---

### [C14] Unit Test Suite — Models and ViewModels Layer

**What changed:**
Expanded the unit test suite to cover the Models and ViewModels layers. Added 66 new tests across 8 new test files, bringing the total to 153.

| File | Tests | What is covered |
|---|---|---|
| `Unit/Models/MeetTests.cs` | 6 | `Meet.Slug` generation via SlugHelper, `Meet.ResultsUrl` format (`/meets/{slug}`) |
| `Unit/ViewModels/Admin/SeasonProgressViewModelTests.cs` | 6 | `PercentComplete` integer division, zero-guard when `TotalMeets == 0`, truncation (not rounding) |
| `Unit/ViewModels/Admin/LocationOptionViewModelTests.cs` | 4 | `DisplayText` conditional: full `"Name (City, State)"` when both present, falls back to `Name` when either is empty |
| `Unit/ViewModels/Admin/AdminPerformanceOptionViewModelTests.cs` | 9 | `AthleteOptionViewModel.DisplayText` (`"Last, First (Year)"`); `EventOptionViewModel.DisplayText` + `CategoryName` for all categories including null; `MeetOptionViewModel.DisplayText` with date/env formatting |
| `Unit/ViewModels/Athletes/IndividualPerformanceViewModelTests.cs` | 9 | `IsRelay` (null check) and `RelayMembers` (`|~|` split) on both `IndividualPerformanceViewModel` and `PersonalRecordViewModel` |
| `Unit/ViewModels/Leaderboard/LeaderboardEventViewModelTests.cs` | 5 | `RelayMembers` (uses `IsNullOrEmpty` guard, not null check — different from athlete VMs), `AthleteFullName` concat |
| `Unit/ViewModels/Meets/MeetListItemViewModelTests.cs` | 4 | `IsUpcoming` date comparison against `DateTime.Now` |
| `Unit/ViewModels/Seasons/SeasonCardViewModelTests.cs` | 9 | `IndoorSchoolRecordCount` + `OutdoorSchoolRecordCount` null coalescing; `StatusBadge` for all `SeasonStatus` values |

**Bug found during test authoring:**

`MeetTests.Slug_StripsSpecialCharacters` initially asserted that `"St. Mary's Invitational"` would produce a slug without a period. **SlugHelper actually keeps periods** — it produces `"st.-marys-invitational"`. The apostrophe is stripped but the period is not. Fixed by removing the `.NotContain(".")` assertion.

**Watch out:**

- `SlugHelper` (Slugify NuGet package) **keeps periods** and **strips apostrophes**. Do not assume all special characters are removed.
- `LeaderboardEventViewModel.RelayMembers` uses `string.IsNullOrEmpty(RelayName)` as its guard — it handles both null and empty-string `RelayName`. This differs from `IndividualPerformanceViewModel.RelayMembers` which uses a null check on `RelayAthletes`. These are intentionally different because `RelayName` defaults to `string.Empty`, while `RelayAthletes` is nullable.
- `SeasonProgressViewModel.PercentComplete` uses **integer division** (`EnteredMeets * 100 / TotalMeets`), so 1/3 → 33 and 2/3 → 66 (truncates, not rounds). Do not change this to floating-point without understanding downstream display consequences.

**Key files:**
- `CloverleafTrack.Tests/Unit/Models/MeetTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Admin/SeasonProgressViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Admin/LocationOptionViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Admin/AdminPerformanceOptionViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Athletes/IndividualPerformanceViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Leaderboard/LeaderboardEventViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Meets/MeetListItemViewModelTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/ViewModels/Seasons/SeasonCardViewModelTests.cs` (NEW)

---

### [C16] Roster Details — Mobile Responsive Chart Layout

**What changed:**
The season trajectory chart panel on the Roster Details page (per-event performance section) is now responsive. On mobile it stacks below the performance rows; on wider screens it sits side-by-side to the right.

**Why:**
The original layout used `flex gap-4 items-start` with a fixed `w-72 flex-shrink-0` chart panel. On narrow screens the chart overflowed the container and the performance rows were squished to ~86px wide — unusable. Also, meet names in performance rows overflowed because the `flex-1` cell lacked `min-w-0`.

**Pattern used:**
`flex-wrap` (already compiled) instead of `flex-col sm:flex-row` (responsive Tailwind classes that would require a CSS rebuild to take effect). An inline `style="min-width:260px"` on the performances div ensures `flex-wrap` triggers wrapping at ~564px (260 + 16 gap + 288 chart), giving side-by-side on tablet/desktop and stacked on mobile without requiring new Tailwind classes to be compiled.

Meet name link gets `min-w-0` on the flex-1 container and `block truncate` on the `<a>` tag to prevent the inner row from overflowing on narrow screens.

**Key files:**
- `CloverleafTrack.Web/Views/Roster/Details.cshtml`

**Watch out:**
- Do NOT use new Tailwind responsive variants (e.g. `sm:flex-row`) in this file without also running the Tailwind CSS build. These classes are not compiled by default and will be silently ignored.
- The `min-width:260px` inline style on the performances div is load-bearing for the flex-wrap stacking behavior. Removing it collapses the performances div to near-zero width because `flex-1 min-w-0` has no minimum.

---

### [C21] Home Page "Season at a Glance" — SchoolRecordsBroken Uses Leaderboards

**What changed:**
`HomeRepository.GetHomePageStatsAsync` now counts school records for the season by joining to the `Leaderboards` table (`Rank = 1`) instead of filtering on `p.SchoolRecord = 1`.

```sql
-- Before
(SELECT COUNT(*) FROM Performances p
 INNER JOIN Meets m ON m.Id = p.MeetId
 WHERE m.SeasonId = @SeasonId AND p.SchoolRecord = 1) AS SchoolRecordsBroken,

-- After
(SELECT COUNT(*) FROM Performances p
 INNER JOIN Meets m ON m.Id = p.MeetId
 INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id AND lb.Rank = 1
 WHERE m.SeasonId = @SeasonId) AS SchoolRecordsBroken,
```

**Why:**
Same stale flag issue as C17/C20. `p.SchoolRecord` is not cleared when a newer record supersedes it, so the count was stuck at 0 (or wrong) for the current season.

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/HomeRepository.cs`

---

### [C17] "On This Day" — SchoolRecord Flag Is Stale on Superseded Performances

**What changed:**
`HomeRepository.GetPerformanceOnThisDayAsync` no longer uses `p.SchoolRecord` to determine or sort by school record status. It now derives school record status live from the Leaderboards table (`AllTimeRank = 1`).

**Before:**
```sql
p.SchoolRecord AS IsSchoolRecord,
ORDER BY p.SchoolRecord DESC, ...
```

**After:**
```sql
CASE WHEN (SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) = 1
     THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsSchoolRecord,
ORDER BY
    CASE WHEN (...AllTimeRank...) = 1 THEN 0 ELSE 1 END,
    CASE WHEN (...AllTimeRank...) <= 3 THEN 0 ELSE 1 END,
    m.Date DESC
```

**Why:**
When a new school record is set, the old performance's `SchoolRecord = 1` flag in the `Performances` table is NOT cleared. The flag is a snapshot of status at entry/rebuild time, not a live indicator. The old performance was being sorted to the top and displayed as "set the school record" even after being beaten.

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/HomeRepository.cs`

**Watch out:**
- `p.SchoolRecord` is unreliable for determining CURRENT school record status on **any** performance row (individual or relay) when the question is "is this still the record today?" It means "was the record at the time this flag was last written." Use `AllTimeRank = 1` from the Leaderboards table for current status.
- The existing reliability table in this file (end of C9 section) was updated to reflect this — see the updated summary below.
- `sp_RebuildLeaderboards` keeps the `Leaderboards` table current but does NOT retroactively clear the `SchoolRecord` flag on Performance rows that are no longer #1.

---

### [C15] Athlete Progression Charts — ViewModel + Service + View

**What changed:**

Added per-event progression charts to the Roster Details page, building on two major design decisions:
1. **Season view — Option 1**: table of performance rows on the left, Chart.js line chart on the right, side-by-side per event group.
2. **Career Progression — Mockup B**: dedicated "Career Progression" section below the season accordion with a tabbed event selector and full career-arc chart per event.

Three supporting changes were made before the view rewrite:

- `IndividualPerformanceViewModel` — added `public double? RawValue { get; set; }` — raw numeric value (TimeSeconds for running, DistanceInches for field) needed for Chart.js data arrays. Formatted strings are human-readable but can't be plotted.
- `EventPerformanceGroupViewModel` — added `public bool IsFieldEvent { get; set; }` — drives Chart.js `reverse` axis option (running events: lower = better so Y-axis is inverted; field events: higher = better, normal axis).
- `AthleteService.GetAthleteDetailsAsync` — set `EventId`, `IsFieldEvent`, and `RawValue` in the performance mapping. `EventId` was always 0 before because the `GroupBy` key was used but `EventId` was never assigned.

**Key behaviors in `Details.cshtml` rewrite:**

- **AllTimeRank shown for all athletes**: removed the `<= 10` guard — every athlete sees their rank regardless of value.
- **Chart.js lazy initialization**: charts in hidden accordion panels have zero size when initialized on `DOMContentLoaded`. Season charts are built when the accordion first opens (tracked via `canvas._chart`). Career charts are built when the career tab is first clicked (tracked via `careerCharts[idx]` object).
- **Relay events excluded from charts**: relay team compositions change meet-to-meet, making a progression line meaningless. Chart panel is suppressed for relay event groups; only the existing performance row layout is shown.
- **PR takes precedence over SB**: when a performance is both a PR and SB, the PR badge/color wins. Chart point types: `"pr"` (amber) → `"sb"` (blue) → `"normal"` (green).
- **Data passing**: `data-*` attributes on `<canvas>` elements carry JSON arrays for labels, values, point types, formatted performances, and ranks. Avoids CSP issues with inline scripts holding data.
- **`@functions` block**: `FormatImprovement(double delta, bool isField)` computes career improvement delta server-side for the stats row in the career section.

**New / updated test files:**

- `AthleteServiceTests.cs` — expanded with `GetAthleteDetailsAsync` tests covering: null athlete, no performances, individual PR uses PersonalBest flag, relay PR uses best-per-event regardless of flag, TotalPRs excludes relays, TotalSchoolRecords includes relay events at AllTimeRank==1, relay school record detection, season ordering by date, relay members parsed from `|~|` string.
- `LeaderboardServiceTests.cs` (NEW) — covers `GetLeaderboardAsync` gender/environment partitioning, category grouping, relay category separation, ordering; covers `GetLeaderboardDetailsAsync` null handling, event info, all-performances count, PRs-only per-athlete deduplication, relay event detection.
- `MeetServiceTests.cs` (NEW) — covers `GetMeetDetailsAsync` null slug, name/location, PR/SR counts, boys/girls/mixed split, unique athlete count, event ordering (Sprints→Distance→Hurdles→Running Relays→Jumps→Throws); covers `GetMeetsIndexAsync` total count, season grouping, season ordering descending, meets within season ascending.

**Watch out:**

- `EventPerformanceGroupViewModel.EventId` was always 0 before this change. Tests or code that relied on it being 0 would be wrong — it's now populated from the `GroupBy` key.
- Chart.js with hidden panels: always lazy-initialize charts. Initializing in `DOMContentLoaded` while the container is `display:none` produces a 0×0 canvas and broken charts.
- `RawValue` is `DistanceInches ?? TimeSeconds` — for relay field events this correctly picks distance; for all running events this picks time. Null when neither is set (shouldn't happen in practice but guard in JS with `!= null` filter).

**Key files:**
- `CloverleafTrack.ViewModels/Athletes/IndividualPerformanceViewModel.cs` (MODIFIED — added RawValue)
- `CloverleafTrack.ViewModels/Athletes/EventPerformanceGroupViewModel.cs` (MODIFIED — added IsFieldEvent)
- `CloverleafTrack.Services/AthleteService.cs` (MODIFIED — set EventId, IsFieldEvent, RawValue)
- `CloverleafTrack.Web/Views/Roster/Details.cshtml` (REWRITTEN)
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` (EXPANDED)
- `CloverleafTrack.Tests/Unit/Services/LeaderboardServiceTests.cs` (NEW)
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs` (NEW)

---

### [C19] sp_RebuildLeaderboards — Now Manages SchoolRecord Flag

**What changed:**
Added two steps to `sp_RebuildLeaderboards` in `docs/schema.sql`:

```sql
-- Step 7: Reset SchoolRecord flag on all Performances
UPDATE Performances SET SchoolRecord = 0;

-- Step 8: Set SchoolRecord = 1 for individual performances ranked #1 all-time
UPDATE p SET p.SchoolRecord = 1
FROM Performances p
INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id
WHERE lb.Rank = 1 AND p.AthleteId IS NOT NULL;
```

These run after the Leaderboards table is fully rebuilt (steps 9–10, previously 7–8).

**Why:**
The SP previously managed `PersonalBest` and `SeasonBest` (reset-all then recalculate) but never touched `SchoolRecord`. When a new record was set, the old performance kept `SchoolRecord = 1` forever. This caused stale flags to surface in "On This Day", meet details hero counts, and anywhere else that used the flag for current school record status.

**Key files:**
- `docs/schema.sql` — SP definition updated. Run `ALTER PROCEDURE` against the live DB to apply.

**Watch out:**
- After altering the SP, run `EXEC sp_RebuildLeaderboards` once to backfill all stale flags.
- Relay performances still get `SchoolRecord = 0` from this SP — they are excluded by `AND p.AthleteId IS NOT NULL`. The app uses `AllTimeRank = 1` as the SR proxy for relay rows, which is correct and authoritative.
- The schema comment on the SP header was updated to reflect that it now manages SchoolRecord in addition to PersonalBest and SeasonBest.

---

### [C20] Meet Details Hero — TotalSchoolRecords Uses AllTimeRank == 1

**What changed:**
`MeetService.GetMeetDetailsAsync` now counts school records using `AllTimeRank == 1` instead of the stale `p.SchoolRecord` flag:

```csharp
// Before
TotalSchoolRecords = performances.Count(p => p.SchoolRecord),

// After
TotalSchoolRecords = performances.Count(p => p.AllTimeRank == 1),
```

`AllTimeRank` is already populated from the Leaderboards table by `MeetRepository.GetPerformancesForMeetAsync`.

**Why:**
Same root cause as the athlete details hero (C9) and "On This Day" (C17): `p.SchoolRecord` is a stale snapshot that is not cleared when a newer record supersedes it. A meet with 2 school records was showing 0 because both performances had stale `SchoolRecord = 0` flags.

**Key files:**
- `CloverleafTrack.Services/MeetService.cs`
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs` — `BuildPerf` helper gained `allTimeRank` parameter; school records test updated to pass `allTimeRank: 1` instead of `sr: true`.

---

### [C18] Roster Index — Relay Events Contribute to Event Category Grouping

**What changed:**
The Roster Index active-athlete grouping now includes relay event participation. Athletes who only run relays (or whose relay events span categories) appear in the correct individual-event section rather than being invisible or grouped under Relays.

**Repository (`AthleteRepository.GetAllWithPerformancesAsync`):**
- Added `e.EventType` to the Event SELECT (was missing; defaulted to 0 = Field which masked issues)
- Added UNION ALL branch for relay performances via `PerformanceAthletes` (relay `Performance` rows have `AthleteId = NULL` so they were previously excluded by the `INNER JOIN Performances p ON p.AthleteId = a.Id` filter)

**Service (`AthleteService`):**
Two new static helpers:

`GetRosterCategory(Event)` — maps relay EventType to the corresponding individual EventCategory:
- `JumpRelay` → `Jumps`
- `ThrowsRelay`, `FieldRelay` → `Throws`
- `RunningRelay` → `Sprints` unless the event name contains distance keywords ("distance medley", "dmr", "800", "1500", "1600", "mile", "2000", "3200") → `Distance`
- Everything else → `ev.EventCategory` (stored value, used as-is for individual events)

`IsDistanceBasedEvent(Event)` — replaces the old `EventCategory is Throws or Jumps` check in the PR lookup. Handles relay EventTypes explicitly because relay events have `EventCategory.Relays`, not Throws/Jumps:
- `FieldRelay`, `JumpRelay`, `ThrowsRelay` → true (use DistanceInches)
- `Field` → fall back to `EventCategory is Throws or Jumps`
- `Running`, `RunningRelay` → false (use TimeSeconds)

**Watch out:**
- `EventCategory.Relays` no longer appears as a key in the active athletes dictionary. All relay-only athletes are now bucketed under individual categories.
- An athlete with both individual 100m and 4×100m relay participation appears once in Sprints with both events listed (de-duplication is handled by the existing `GroupBy(x => x.Athlete.Id)` within each category group).
- The RunningRelay → Sprints/Distance name-based heuristic checks for substrings: "800" in the event name means distance relay. Sprint Medley Relay doesn't contain a distance number in its standard name so it correctly maps to Sprints.

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/AthleteRepository.cs`
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.Tests/Unit/Services/AthleteServiceTests.cs` (5 new tests)

---

### [C22] Season Index + Season Details — SchoolRecord Counts Use Leaderboards

**What changed:**
Four locations were still using the stale `p.SchoolRecord` flag to count school records for seasons. All updated to use `Leaderboards.Rank = 1` as the authoritative source.

**1. `SeasonRepository.GetSeasonsWithMeetsAsync` SQL:**
Added AllTimeRank subquery between `p.*` and `e.*` so Dapper's multi-mapping assigns it to the `Performance` type:
```sql
(SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) AS AllTimeRank,
```

**2. `SeasonService.GetSeasonCardsAsync`:**
```csharp
// Before
.Where(p => p.SchoolRecord)

// After
.Where(p => p.AllTimeRank == 1)
```
Applied to both `IndoorSchoolRecords` and `OutdoorSchoolRecords` LINQ chains.

**3. `PerformanceRepository.CountSchoolRecordsBrokenForSeasonAsync` SQL:**
```sql
-- Before
WHERE m.SeasonId = @SeasonId AND p.SchoolRecord = 1

-- After (uses INNER JOIN on Leaderboards Rank = 1)
INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id AND lb.Rank = 1
WHERE m.SeasonId = @SeasonId
```

**4. `MeetRepository.GetMeetsForSeasonAsync` SQL:**
Replaced `COUNT(CASE WHEN p.SchoolRecord = 1 ...)` (which was in a GROUP BY query and couldn't easily join Leaderboards per-row) with a correlated subquery:
```sql
(SELECT COUNT(*) FROM Performances p2 INNER JOIN Leaderboards lb ON lb.PerformanceId = p2.Id AND lb.Rank = 1 WHERE p2.MeetId = m.Id) AS SchoolRecordCount,
```

**Why:**
Season Index was showing 0 Indoor SRs and 0 Outdoor SRs for seasons where records were set. Season Details "Season Overview" SR count was also wrong. Root cause identical to C17/C19/C20: `p.SchoolRecord` is a stale snapshot.

**Watch out:**
- `Performance.AllTimeRank` (added in the previous session) is what allows the Dapper multi-mapping approach in `GetSeasonsWithMeetsAsync`. The subquery must stay between `p.*` and `e.*` in the SELECT column order — otherwise Dapper maps it to the wrong type.
- `CountSchoolRecordsBrokenForSeasonAsync` now counts ALL performances at rank 1 for a season (not just those with the stale flag set), which is the correct behavior.

**5. `MeetRepository.GetAllMeetsWithStatsAsync` SQL (Meet Index page SR counts):**
Same correlated-subquery fix as `GetMeetsForSeasonAsync`:
```sql
(SELECT COUNT(*) FROM Performances p2 INNER JOIN Leaderboards lb ON lb.PerformanceId = p2.Id AND lb.Rank = 1 WHERE p2.MeetId = m.Id) AS SchoolRecordCount,
```

**Key files:**
- `CloverleafTrack.DataAccess/Repositories/SeasonRepository.cs`
- `CloverleafTrack.Services/SeasonService.cs`
- `CloverleafTrack.DataAccess/Repositories/PerformanceRepository.cs`
- `CloverleafTrack.DataAccess/Repositories/MeetRepository.cs` (both `GetMeetsForSeasonAsync` and `GetAllMeetsWithStatsAsync`)

---

### [C23] Full SchoolRecord Sweep — All Remaining Stale Flag Usages Eliminated

**What changed:**
A codebase-wide audit found the remaining places still using `p.SchoolRecord` (the DB flag) instead of `AllTimeRank == 1`. Every one was updated.

**`AthleteService.cs`** — three locations:
- `PersonalRecordViewModel.IsSchoolRecord`: was `p.RelayAthletes == null ? p.SchoolRecord : p.AllTimeRank == 1`; simplified to `p.AllTimeRank == 1` for both
- `SeasonPerformanceViewModel.SchoolRecordCount`: `seasonGroup.Count(p => p.SchoolRecord)` → `seasonGroup.Count(p => p.AllTimeRank == 1)`
- `IndividualPerformanceViewModel.IsSchoolRecord`: `p.SchoolRecord` → `p.AllTimeRank == 1`

**`MeetService.cs`** — both event group builders (`AddEventGroupsForCategory` and `AddEventGroupsFromList`):
- `MeetPerformanceViewModel.IsSchoolRecord`: `p.SchoolRecord` → `p.AllTimeRank == 1` (AllTimeRank already populated from `GetPerformancesForMeetAsync`)

**`LeaderboardService.cs`** — both performance list projections:
- `LeaderboardPerformanceViewModel.IsSchoolRecord`: `perf.SchoolRecord` → `perf.AllTimeRank == 1` (AllTimeRank already populated as `lb.Rank` from the leaderboard query's `LEFT JOIN Leaderboards`)

**`HomeRepository.cs`** — `GetRecentTopPerformanceAsync`:
- `recentSql`: replaced `p.SchoolRecord AS IsSchoolRecord` with Leaderboards subquery; updated `ORDER BY p.SchoolRecord DESC` to `ORDER BY CASE WHEN (SELECT MIN(lb.Rank) ...) = 1 THEN 0 ELSE 1 END`
- `seasonBestSql`: replaced `p.SchoolRecord AS IsSchoolRecord` with `CAST(1 AS BIT)` (safe — this query already `INNER JOIN Leaderboards lb ... AND lb.Rank = 1`)

**`AdminPerformanceRepository.cs`** — `GetAllWithDetailsAsync`:
- Added `(SELECT MIN(lb.Rank) FROM Leaderboards lb WHERE lb.PerformanceId = p.Id) AS AllTimeRank` subquery (placed after `p.*`, before `a.*` for correct Dapper multi-mapping)

**`Admin/Views/Performances/Index.cshtml`**:
- `@if (perf.SchoolRecord)` → `@if (perf.AllTimeRank == 1)`

**Why:**
The `SchoolRecord` DB flag had been trusted in ~10 places across services, repositories, and a Razor view. Each one was either showing 0 SRs or displaying SR badges for performances that no longer hold the record. The universal fix is `AllTimeRank == 1` — this reads directly from the `Leaderboards` table which is always rebuilt fresh by `sp_RebuildLeaderboards`.

**Watch out:**
- `p.SchoolRecord` (the DB column) still exists on the `Performance` model and is still written by `sp_RebuildLeaderboards` for individual rows. It should be treated as a DB-level implementation detail only — never read in application code.
- `Performance.AllTimeRank` is a C# property only (`int?`), not a DB column. It is `null` unless the query explicitly includes a Leaderboards subquery or join. Any new query that needs `IsSchoolRecord` must add the subquery — do not assume it will be populated by `p.*`.
- The Dapper multi-mapping rule: when adding the AllTimeRank subquery to a multi-type SELECT, it must sit after `p.*` and before the next model's `Id` split column, otherwise Dapper assigns it to the wrong type.

**Key files:**
- `CloverleafTrack.Services/AthleteService.cs`
- `CloverleafTrack.Services/MeetService.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.DataAccess/Repositories/HomeRepository.cs`
- `CloverleafTrack.DataAccess/Repositories/AdminPerformanceRepository.cs`
- `CloverleafTrack.Web/Areas/Admin/Views/Performances/Index.cshtml`

---

### [C24] Leaderboard Details — School Record Progression Timeline + Chart

**What changed:**
Added a "School Record History" section to the leaderboard event detail page (`/leaderboard/{eventKey}`). It shows every time the school record was broken, who broke it, the improvement over the previous record, and a step-line chart of the progression over time. Record-setting rows in the all-performances and PRs-only tables are highlighted with an amber left border and a dimmed SR badge.

**New ViewModel: `SchoolRecordMomentViewModel`**
One entry in the progression (athlete, formatted performance, raw numeric value for Chart.js, improvement delta as formatted string, `IsCurrentRecord` flag).

**Updated ViewModels:**
- `LeaderboardPerformanceViewModel` — added `WasRecordAtTime` (bool): was the school record at the moment it was performed, regardless of whether it still is now
- `LeaderboardDetailsViewModel` — added `IsFieldEvent` (bool) and `SchoolRecordProgression` (List)

**`LeaderboardService.GetLeaderboardDetailsAsync` — C# progression computation:**

No new SQL query. Uses the performances already fetched by `GetAllPerformancesForEventAsync`:
```csharp
// 1. Sort chronologically (same-day ties: best mark first)
var chronological = allPerformances
    .OrderBy(p => p.MeetDate)
    .ThenBy(p => isFieldEvent ? -(p.DistanceInches ?? 0) : (p.TimeSeconds ?? double.MaxValue))
    .ToList();

// 2. Walk and track running best
double? runningBest = null;
foreach (var perf in chronological)
{
    var value = isFieldEvent ? perf.DistanceInches : perf.TimeSeconds;
    var isNewRecord = runningBest == null ||
                      (isFieldEvent ? value > runningBest : value < runningBest);
    if (isNewRecord) { /* add to progression, record PerformanceId */ }
    runningBest = value;
}

// 3. Sort for display: best-first (distance desc / time asc)
// 4. Set WasRecordAtTime on AllPerformances and PersonalRecordsOnly via HashSet<int>
```

**`FormatImprovement(double delta, bool isField)` helper** — formats the delta between consecutive records:
- Field: `"+2' 6.25\""` (feet+inches)
- Running: `"-0.43s"` or `"-1:02.50"` for multi-minute events

**`Details.cshtml` — UI:**
- Collapsible card above the toggle buttons; collapsed by default
- Left column: vertical timeline of record holders with green improvement badge, "Current SR" callout for the latest, "First recorded school record" for the oldest
- Right column: Chart.js step-line chart, lazy-initialized on first open (`srChartBuilt` guard). Y-axis is reversed for running events (lower = better); normal for field events. Data passed via `data-*` attributes on the `<canvas>` to avoid CSP issues.
- Both performance tables: amber `border-l-2 border-l-amber-500 bg-amber-500/5` on `WasRecordAtTime` rows. SR badge shown at full opacity for current record, dimmed for former records.

**Watch out:**
- The progression is computed from `GetAllPerformancesForEventAsync` which already fetches all performances ordered best-first. Re-sorting chronologically in C# is intentional — do not add a new SQL query.
- `RawValue` on `SchoolRecordMomentViewModel` is `DistanceInches` for field events and `TimeSeconds` for running events. The Chart.js Y-axis `reverse: !isField` handles direction — do not negate values.
- Same-day tie-breaking: if two performances happen on the same date, the better mark is processed first (it's the one that "set the record"). The worse mark on the same day is skipped since it can't be a new best.
- The panel is collapsed by default. The chart is not initialized until first open (`srChartBuilt` flag). This mirrors the athlete progression chart lazy-init pattern (C15).

**Key files:**
- `CloverleafTrack.ViewModels/Leaderboard/SchoolRecordMomentViewModel.cs` (NEW)
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardPerformanceViewModel.cs`
- `CloverleafTrack.ViewModels/Leaderboard/LeaderboardDetailsViewModel.cs`
- `CloverleafTrack.Services/LeaderboardService.cs`
- `CloverleafTrack.Web/Views/Leaderboard/Details.cshtml`

---

### [C25] Meet Participants, Entries, Placings & Season Scoring

**What changed:**
Full feature: pre-meet entry tracking, post-meet placing entry, scoring templates, and a season scoring page.

**Key additions:**

*Models (CloverleafTrack.Models):*
- `MeetType` enum: `Dual=1, DoubleDual=2, Invitational=3`
- `ScoringTemplate`, `ScoringTemplatePlace`, `MeetParticipant`, `MeetEventScoringOverride`, `MeetEntry`, `MeetEntryAthlete`, `MeetPlacing`
- `Meet` — added `MeetType`, `ScoringTemplateId`, `Participants`, `ScoringTemplate`
- `Season` — added `ScoringEnabled`

*DTOs:*
- `MeetEntryDto` — flat DTO for meet entry list; `IsRelay` and `AthleteDisplayName` are computed properties
- `ScoringDataDto` — one row per athlete per placing; relay rows expanded per member via UNION ALL in the repository

*Repository interfaces + implementations (7 new):*
- `IScoringTemplateRepository` / `ScoringTemplateRepository`
- `IAdminScoringTemplateRepository` / `AdminScoringTemplateRepository`
- `IAdminMeetParticipantRepository` / `AdminMeetParticipantRepository`
- `IAdminMeetEntryRepository` / `AdminMeetEntryRepository`
- `IAdminMeetPlacingRepository` / `AdminMeetPlacingRepository`
- `IMeetPlacingRepository` / `MeetPlacingRepository`
- `ISeasonScoringRepository` / `SeasonScoringRepository`
- `IMeetRepository` — added `GetParticipantsForMeetAsync`
- `AdminMeetRepository`, `AdminSeasonRepository` — updated INSERT/UPDATE SQL for new columns

*Services:*
- `IScoringService` / `ScoringService` — aggregates ScoringDataDto rows per (AthleteId, Gender) accumulating Full/Split points by breakdown; returns null if season not found or `ScoringEnabled=false`
- `MeetService` — injects `IMeetPlacingRepository`; `GetMeetDetailsAsync` now runs parallel tasks including placings; builds `placingLookup` dictionary for `BuildOrderedEventGroups`
- `SeasonService.GetSeasonDetailsAsync` — now populates `SeasonId` and `ScoringEnabled` on `SeasonDetailsViewModel`

*ViewModels (key):*
- `Scoring/AthleteScoreRowViewModel`, `Scoring/SeasonScoringViewModel`
- `Meets/MeetDetailsViewModel` — added `MeetType`, `Participants`, `HasScoring`
- `Meets/MeetPerformanceViewModel` — added `AthleteSlug?`, `List<PerformancePlacingViewModel> Placings`, `HasPlacing`; nested `PerformancePlacingViewModel` with `MedalEmoji` computed property
- `Seasons/SeasonDetailsViewModel` — added `SeasonId`, `ScoringEnabled`

*Admin controllers (2 new):*
- `ScoringTemplatesController` — CRUD for templates; Delete blocked for built-in templates
- `MeetEntriesController` — Index, AddEntry, GetAthletesForEvent (AJAX), EnterResult, DeleteEntry; EnterResult POST creates Performance + PerformanceAthletes + links MeetEntry + creates MeetPlacings with FullPoints/SplitPoints

*Public controller:*
- `SeasonsController` — added `Scoring(string name)` action at `/seasons/{name}/scoring`; returns 404 if `ScoringEnabled=false`

*Views (new):*
- `Areas/Admin/Views/ScoringTemplates/` — Index, Create, Edit
- `Areas/Admin/Views/MeetEntries/` — Index, _EntryEventGroup (partial), AddEntry, EnterResult
- `Views/Seasons/Scoring.cshtml` + `Views/Seasons/_ScoringGenderPanel.cshtml`
- `Views/Meets/Details.cshtml` — placing badges (🥇🥈🥉 or ordinal) added to Notes column in all three gender sections
- `Views/Seasons/Details.cshtml` — "🏆 Season Scoring" link shown when `ScoringEnabled`

*Schema (`docs/schema.sql`):*
- `ALTER TABLE Seasons ADD ScoringEnabled BIT`
- `ALTER TABLE Meets ADD MeetType SMALLINT, ScoringTemplateId INT`
- New tables: `ScoringTemplates`, `ScoringTemplatePlaces`, `MeetParticipants`, `MeetEventScoringOverrides`, `MeetEntries`, `MeetEntryAthletes`, `MeetPlacings`
- Seed: built-in "Dual Meet (5-3-1)" template with places 1→5, 2→3, 3→1
- `RunningRelayEvents` table documented (pre-existing; added to schema file)

**Watch out:**
- `MeetPlacings.MeetParticipantId` is NULL for invitational placings. Two filtered UNIQUE indexes handle uniqueness: one WHERE NOT NULL (per-opponent), one WHERE NULL (overall). SQL Server treats NULL != NULL in unique indexes, so both cases are covered without a composite unique constraint.
- Double Dual meets have 3 teams; one `MeetPlacing` row is created per opponent per performance. `EnterResultViewModel.PlaceInputs` has one `PlaceInputRow` per `MeetParticipant`.
- Relay scoring: `FullPoints` = each relay member gets the template's full points for that place. `SplitPoints` = `FullPoints / AthleteCount`. Both are stored; a UI toggle selects which to display.
- `GetTemplatePointsAsync` in `AdminMeetPlacingRepository` resolves points via: event override template → meet default template → 0 (out of range). All three cases return 0 silently if the place exceeds the template.
- `MeetEntry.AthleteId` is NULL for relays; relay athletes are in `MeetEntryAthletes`. `GetAthleteEventCountForMeetAsync` uses UNION ALL to count both individual entries and relay memberships (to enforce the 4-event limit display flag).
- `ScoringEnabled` on Season is the gate for both the public scoring route (404 if false) and the "Season Scoring" button on the season details page.
- `MeetService` tests required updating: `MeetService` constructor now requires `IMeetPlacingRepository`; the test mock must be set up with `.ReturnsAsync(new List<MeetPlacing>())` to prevent `GroupBy` NullReferenceException.
- `SeasonDetailsViewModel` now has `SeasonId` and `ScoringEnabled` — populated by `SeasonService.GetSeasonDetailsAsync`.

**Key files:**
- `CloverleafTrack.Models/Enums/MeetType.cs`
- `CloverleafTrack.Models/Meet.cs`, `Season.cs`, `MeetPlacing.cs`, `MeetEntry.cs`, `MeetEntryAthlete.cs`, `MeetParticipant.cs`, `ScoringTemplate.cs`, `ScoringTemplatePlace.cs`, `MeetEventScoringOverride.cs`
- `CloverleafTrack.DataAccess/Dtos/MeetEntryDto.cs`, `ScoringDataDto.cs`
- `CloverleafTrack.DataAccess/Interfaces/I*Repository.cs` (7 new interfaces + IMeetRepository updated)
- `CloverleafTrack.DataAccess/Repositories/Admin/*Repository.cs` (3 updated: AdminMeet, AdminSeason, AdminAthlete)
- `CloverleafTrack.Services/ScoringService.cs`, `MeetService.cs`, `SeasonService.cs`
- `CloverleafTrack.Services/Interfaces/IScoringService.cs`
- `CloverleafTrack.ViewModels/Scoring/`, `Meets/MeetPerformanceViewModel.cs`, `Meets/MeetDetailsViewModel.cs`, `Seasons/SeasonDetailsViewModel.cs`, `Admin/Meets/MeetFormViewModel.cs`
- `CloverleafTrack.Web/Areas/Admin/Controllers/ScoringTemplatesController.cs`, `MeetEntriesController.cs`, `MeetsController.cs`
- `CloverleafTrack.Web/Controllers/SeasonsController.cs`
- `CloverleafTrack.Web/Views/Meets/Details.cshtml`, `Views/Seasons/Details.cshtml`, `Scoring.cshtml`, `_ScoringGenderPanel.cshtml`
- `CloverleafTrack.Web/Program.cs` (DI registrations)
- `CloverleafTrack.Tests/Unit/Services/MeetServiceTests.cs`
- `docs/schema.sql`

---
