# athletic.net import (prelim/semi/final rounds + field attempt series)

Pilot: Metro Athletic Conference Championships 2026 (athletic.net meet 637822 = DB meet 297).

1. Scrape (browser pane, no login). Results page `/TrackAndField/meet/{id}/results/all`; field series
   `/results/{m|f}/1/{event}?tab=field-series` (columns = attempts, "Foul"/"Pass" literal).
   - The API (`/api/v1/...`) 403s without the page's own token header; read the rendered DOM instead.
   - The results list is virtualized and only renders after the tab is painted (take a screenshot after
     `navigate`); scroll in ~40% viewport steps and accumulate `.event-block` rows; use `textContent`
     (off-screen `innerText` is empty). Round = the header `<small>` ("Varsity - Semi-Finals"); gender is in the
     event link (`/results/m/…` vs `/results/f/…`).
2. Read-only dump of the DB meet's performances (`Id|Gender|Event|Athletes|TimeSeconds|DistanceInches|AttemptCount`).
3. `python3 -I build_rounds_sql.py <dbMeetId> <dump> <running.txt> <series.json> <out.sql>` writes a transaction
   that re-asserts each mark before inserting `PerformanceRounds` / `PerformanceAttempts` rows. Review, then run.

Only athletes with 2+ rounds get a chain (a lone semifinal is not stored). High jump / pole vault series are
bar heights, not distances, and are not supported by `PerformanceAttempts`.

## No-mark results (NM / NH / DNF / DQ)

`scan_no_marks.js` (see its header) scans a meet's varsity event pages for Cloverleaf rows that have no mark. athletic.net shows
`ND` in the place column of a field-series page when every attempt was a foul, `NH` for a high jump / pole vault with no cleared
bar, and `DNF` / `DQ` / `DNS` after the team on running results. Output is turned into `docs/imports/2026-no-marks.sql` by hand
(a handful of rows): assert athlete + event, insert a `ResultStatus` Performance only if the athlete has no row for that event at
that meet. DNS (did not start) is deliberately not imported. The browser pane must be visible while it runs.
