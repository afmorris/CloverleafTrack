# Results-level comparison: MileSplit vs the database

For every DB meet matched to a MileSplit meet (125 with raw results, plus the 11 meets imported in BRAIN.md [C71]-[C73]), each Cloverleaf row in MileSplit's Hy-Tek text was compared with the DB result for the same meet, event and athlete; relays are matched team by team (grouped by the four leg names) and one-to-one by time. Script: `scripts/crawl/compare_ms_vs_db.py` (BRAIN.md [C74], [C75]).

**Summary:** about 3,380 results agree. Imported from this comparison: 32 (first pass, `results-missing-ms.sql`) and 82 more (`results-missing-ms-2/3/4.sql`: individual marks from throws/jumps "relays" at the New London relay meets, Seated events, medley relays, second relay teams and 8 Oberlin relays). Still open: 39 individual and 5 relay marks that differ, 17 rows the matcher could not place (mostly parsing artifacts and one middle schooler, Savannah Reho), 7 relays without four resolvable legs.

## Artifacts to ignore
- Hy-Tek prints hand-timed tenths as hundredths (27.6 appears as `27.06`); the DB's tenths are right. The comparison treats `x.0y` as `x.y`.
- Column handling bugs found and fixed while doing this: header rows with `Grade`/`Mark` instead of `Year`/`Finals` made the parser read the Seed column as the result; event headers with qualifiers (`Varsity`, `METRO`, `COUNTY`, `Div 2`), medley relays and `Weight Throw Wheelchair` were attributed to the previous event.

## Small differences (27): recommended to take MileSplit's mark
Same athlete and event, within 3.5% (times) or 6 inches (field). Awaiting your go-ahead; the DB value is shown first.
| Meet | Event | Athlete | DB | MileSplit |
|---|---|---|---|---|
| 2025-05-17 2025 Metro Athletic Conference Champions | Boys 400M | Isaac Smith | 53.19 | 52.79 |
| 2025-05-17 2025 Metro Athletic Conference Champions | Girls 400M | Katie Wershing | 1:03.38 | 1:01.32 |
| 2024-05-03 2024 Fairless Twilight Invitational | Boys Discus | Tim Shores | 123' 1.00" | 123' 0.00" |
| 2024-05-03 2024 Fairless Twilight Invitational | Girls Shot Put | Karoline Kreider | 32' 7.00" | 32' 2.50" |
| 2024-05-10 2024 Metro Athletic Conference Champions | Girls 200M | Riley Evans | 25.55 | 25.82 |
| 2024-05-24 2024 Division I Regional Track Meet | Girls 400M | Riley Evans | 56.44 | 57.88 |
| 2023-04-04 2023 Ravenna-Streetsboro Double Dual | Boys 200M | Oakley Kemp | 25.19 | 25.10 |
| 2023-05-05 2023 Fairless Twilight Invitational | Girls Pole Vault | Payten Shetterly | 6' 6.00" | 6' 0.00" |
| 2022-05-09 2022 Smithie Frosh-Soph Invitational | Girls 800M | Brooke Gucker | 2:42.65 | 2:42.56 |
| 2022-05-09 2022 Smithie Frosh-Soph Invitational | Boys Shot Put | Tim Shores | 38' 8.00" | 38' 3.00" |
| 2021-05-01 2021 Nikki Reynolds Invitational | Girls 800M | Alyssa Stoner | 2:30.69 | 2:30.09 |
| 2021-05-01 2021 Nikki Reynolds Invitational | Boys Shot Put | Luke Yuravak | 41' 1.25" | 41' 5.25" |
| 2021-05-21 2021 Division I District Track Meet	 | Boys 1600M | Nathan Naples | 5:05.77 | 5:05.05 |
| 2021-05-21 2021 Division I District Track Meet	 | Girls 800M | Allison Yagersz | 2:35.84 | 2:35.73 |
| 2018-05-09 2018 PTC Track Championships | Girls 100M | Maddie Boltz | 13.53 | 13.59 |
| 2018-05-09 2018 PTC Track Championships | Girls 100M | Sara Keener | 13.40 | 13.69 |
| 2018-05-09 2018 PTC Track Championships | Girls 200M | Emily Romano | 27.51 | 27.61 |
| 2018-05-09 2018 PTC Track Championships | Boys 100M | Zack Munchick | 11.52 | 11.73 |
| 2018-05-09 2018 PTC Track Championships | Girls 400M | Tommie Kurtz | 1:00.85 | 1:02.54 |
| 2018-05-09 2018 PTC Track Championships | Girls 400M | Maddie Boltz | 1:05.72 | 1:05.93 |
| 2018-05-09 2018 PTC Track Championships | Boys 400M | Frank Stepp | 52.12 | 52.42 |
| 2017-04-15 2017 Mansfield Mehock Relays | Girls 1600M | Katelyn Malena | 6:19.31 | 6:19.25 |
| 2017-04-28 2017 Orange Relays | Boys Discus | Nate Clingan | 113' 7.00" | 113' 7.50" |
| 2017-05-10 2017 PTC Track Championships | Girls 400M | Jenna Oliver | 1:04.39 | 1:04.48 |
| 2017-05-10 2017 PTC Track Championships | Boys 400M | Ryan Curtis | 55.11 | 55.92 |
| 2017-05-10 2017 PTC Track Championships | Girls 300M Hurdles | Zana Ross | 48.70 | 49.04 |
| 2017-05-10 2017 PTC Track Championships | Girls 200M | Jenna Oliver | 28.19 | 28.60 |

## Large differences (12): check against the official sheet
| Meet | Event | Athlete | DB | MileSplit |
|---|---|---|---|---|
| 2025-04-26 2025 Green Twilight Relays | Girls Discus | Presly McConnell | 96' 5.00" | 69' 5.00" |
| 2025-05-03 2025 Nikki Reynolds Invitational | Boys Discus | Clay Combs | 157' 10.00" | 157' 0.00" |
| 2025-05-12 2025 Stars of Track and Field | Boys Long Jump | Collin Saksa | 15' 8.25" | 18' 5.25" |
| 2024-05-03 2024 Fairless Twilight Invitational | Girls Discus | Karoline Kreider | 85' 1.00" | 78' 10.00" |
| 2024-05-06 2024 Stars of Track and Field | Boys Discus | Dylan Wittman | 103' 3.00" | 100' 8.00" |
| 2024-05-24 2024 Division I Regional Track Meet | Girls 100M | Riley Evans | 12.25 | 12.78 |
| 2024-05-24 2024 Division I Regional Track Meet | Boys Discus | Tim Shores | 128' 6.00" | 129' 7.00" |
| 2023-04-04 2023 Ravenna-Streetsboro Double Dual | Boys Shot Put | Dominic Nau | 31' 0.00" | 38' 2.00" |
| 2018-04-27 2018 Orange Relays | Girls 1600M | Sara Naples | 6:17.24 | 12:48.68 |
| 2018-04-27 2018 Orange Relays | Boys 1600M | Dillon West | 5:12.55 | 10:01.05 |
| 2018-05-09 2018 PTC Track Championships | Boys 400M | Blake Wilson | 54.88 | 57.50 |
| 2017-05-20 2017 Division II District Track Meet | Boys 800M | Nick Bailey | 2:18.46 | 2:12.94 |

## Relay times that differ
| Meet | Relay | DB | MileSplit |
|---|---|---|---|
| 2025-04-26 2025 Green Twilight Relays | Boys 4x1600M Relay | 25:42.98 | 19:56.86 |
| 2025-04-26 2025 Green Twilight Relays | Boys 4x100M Relay | 48.14 | 44.00 |
| 2025-05-24 2025 Division II District Track Meet | Boys 4x400M Relay | 3:30.96 | 2:08.50 |
| 2024-05-17 2024 Division I District Track Meet | Girls 4x400M Relay | 4:15.93 | 4:10.33 |
| 2024-05-24 2024 Division I Regional Track Meet | Girls 4x200M Relay | 1:42.75 | 1:44.61 |

## Not covered
- athletic.net-only content; DB meets with no MileSplit counterpart (all dual meets, most league meets).
- Prelim/final rounds and attempts (only the best mark is compared).
- Savannah Reho (8th grade) is intentionally ignored.
