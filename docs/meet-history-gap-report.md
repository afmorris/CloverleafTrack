# Meet history gap report

Compares the 307 meets in the database (Oct 2026) with athletic.net, MileSplit and baumspage.com. Method: the athletic.net and
MileSplit team schedules were crawled earlier (`scripts/crawl/matches.tsv`, 306 meet rows, 153 matched to DB meets); baumspage.com
was crawled for this report (archived OHSAA tournament pages 2003-2011 and its 2011 archive of ~500 meet pages, searched for
"Cloverleaf"); MileSplit raw results were re-fetched for all 128 matched meets and Cloverleaf row counts compared with DB performance
counts. "Gap" means a candidate to check: nothing here was written to the database.

## 1. Coverage by year

| Year | DB meets | athletic.net rows | MileSplit rows | External meets with no DB meet |
|---|---|---|---|---|
| 2003 | 11 | 0 | 0 | 0 |
| 2004 | 14 | 0 | 0 | 0 |
| 2005 | 4 | 3 | 0 | 1 |
| 2006 | 5 | 2 | 0 | 1 |
| 2007 | 8 | 3 | 0 | 1 |
| 2008 | 4 | 1 | 0 | 0 |
| 2009 | 7 | 6 | 0 | 3 |
| 2010 | 7 | 5 | 0 | 2 |
| 2011 | 9 | 4 | 0 | 1 |
| 2012 | 7 | 1 | 0 | 1 |
| 2013 | 10 | 2 | 0 | 0 |
| 2014 | 5 | 4 | 0 | 0 |
| 2015 | 5 | 4 | 0 | 1 |
| 2016 | 8 | 6 | 0 | 1 |
| 2017 | 8 | 7 | 10 | 3 |
| 2018 | 10 | 5 | 12 | 2 |
| 2019 | 13 | 10 | 16 | 8 |
| 2020 | 5 | 4 | 5 | 0 |
| 2021 | 21 | 11 | 15 | 2 |
| 2022 | 22 | 14 | 21 | 6 |
| 2023 | 19 | 8 | 14 | 1 |
| 2024 | 23 | 13 | 21 | 4 |
| 2025 | 20 | 9 | 21 | 7 |
| 2026 | 21 | 21 | 28 | 14 |

Before 2003 the database has 41 meets and neither site has any data.

Where the sources have no data at all (before 2003, and every dual meet and most league meets), the database is the only record.

## 2. Meets the sources list for Cloverleaf that the database does not have

Each row had no DB meet with a matching name; the nearest DB meet within 3 days is shown so look-alikes can be ruled out.
"Likely missing" = no DB meet within 3 days or only an unrelated one.

### Likely missing (high school meets)
| Date | Meet | Sources | Nearest DB meet |
|---|---|---|---|
| 2005-05-19 | OHSAA NE Division I District, North Canton | AN | none (baumspage has full results) |
| 2006-05-17 | OHSAA NE Division I District, North Canton | AN | none (baumspage has full results) |
| 2009-05-22 | NEO North Canton District, Division I | AN | none (baumspage has full results) |
| 2010-05-22 | OHSAA NE North Canton District, Division I | AN | none (baumspage has full results) |
| 2008 | NE Division I District (North Canton) | baumspage | none (baumspage page has Cloverleaf rows) |
| 2009-04-17/18 | Mansfield Mehock Relays | AN (two listings) | none |
| 2011-05-07 | Fitch Optimist Meet | AN, baumspage | none |
| 2012-05-05 | Optimist Meet (32nd) | AN | none |
| 2016-05-07 | Optimist Meet | AN | none |
| 2024-05-04 | 43rd Optimist Meet | AN, MS | 2024-05-03 Fairless Twilight (different meet) |
| 2017-05-05 | Dwight Schar Invitational; Jackson Twilight | MS | none |
| 2018-04-07 | New London Relays | MS | none |
| 2018-04-21 | Triway Invitational | MS | none (DB has Triway 2014, 2016) |
| 2019-04-06 | New London Relays / Invitational | AN, MS | none |
| 2019-04-20 | Triway Invitational | MS | none |
| 2019-04-16 | Ravenna v Streetsboro v Cloverleaf | MS | 2019-04-13 Cowen Redbird (different meet) |
| 2019-05-06 | Smithie Frosh/Soph Invite (JV) | MS | 2019-05-08 PTC Championships (different) |
| 2023-04-21 | Green Twilight Relays | MS | 2023-04-18 Norton-Springfield dual (different) |
| 2025-04-08 | Streetsboro vs. Cloverleaf and Ravenna | MS | none |
| 2024-06-13 | New Balance Nationals Outdoor | MS | none |

### Likely missing indoor meets
2019-03-02 OATCCC D2/3 Indoor State (AN, MS); 2021-12-12 Tiffin HS Open #1; 2022-02-27 Kent St HS Meet #3;
2025-02-23 Kent State HS Meet #5 (AN, MS); 2025-12-13 Akron Rust-Buster Open (AN, MS), Oberlin Run-Jump-Throw (MS);
2025-12-27 Extreme Vault Club Qualifier (MS); 2026-01-17 Akron Rubber City Open (AN, MS); 2026-01-24 Kent State HS Meet #1 (AN, MS);
2026-02-01 Mount Union Qualifier #1 (AN, MS; DB has Heidelberg on 01-31); 2026-02-07 YSU HS Meet #3 (MS);
2026-02-08 Ashland Light Giver (AN, MS); 2026-02-20 Ashland High Performance Invite (MS); 2026-02-22 Kent State HS Meet #3 (AN, MS);
2026-03-06 OATCCC D2 Indoor State (MS); 2017-02-28 "Conversion Time" (MS); 2019-01-27 BW Indoor is in the DB (name mismatch only).

### Probably NOT gaps (verify before importing)
- 2010-04-24 Amherst Comet Relays: DB has it as 2010-04-27 (date differs by 3 days).
- 2015-05-20 "D1 Nordonia" is DB #179 (District); 2019-01-27 Baldwin-Wallace is DB #267.
- 2022-04-26 Cloverleaf Early Bird Relays (AN, MS): DB has a Field-Coventry Double Dual that day; intentionally not linked.
- 2022-05-06 "Metro Athletic Conference Championship" (MS): DB's 2022 MAC is dated 2022-05-12; probably the same meet with a start-date mismatch.
- 2021-05-15 and 2026-05-16 OHSAA 7th/8th Grade State, Ashland/Holmes County MS meets, "AB Team Import": not high school varsity.

## 3. baumspage.com (archive 2003-2011 only)

baumspage covers northwest-Ohio meets live and keeps an archive that stops at the 2011 season. What it has for Cloverleaf:

| Meet | baumspage years with Cloverleaf rows | In the DB | Gap |
|---|---|---|---|
| OHSAA NE Division I District (North Canton) | 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011 | 2003, 2004, 2007, 2011 | **2005, 2006, 2008, 2009, 2010** |
| OHSAA Division I Regional | 2003, 2004, 2005, 2006, 2009, 2010, 2011 | 2003-2007, 2009-2011 | none missing; DB regionals have only 1-9 results each, baumspage has full heat sheets |
| OHSAA State (participants / heat sheets) | 2007, 2009, 2010, 2011 | 2003-2011 | DB has 1-3 results per year |
| Akron Suburban League championship | 2005-2011 (full results) | 2003-2011 | none; useful to verify the 2005-2011 Suburban League rows (all unlinked to any other source) |
| Husky Invitational (Northwestern) | 2007-2011 | 2008-2011 | **2007** |
| West Geauga Wolverine Invitational | 2007, 2008 | 2007 | **2008** |
| Kimberly Relays | 2006, 2007, 2010 | 2006, 2007, 2009, 2010 | none; 2009 not found on baumspage |
| Amherst Comet Relays (Steele) | 2006-2011 | 2004, 2006-2011 | none |
| Optimist Meet, Austintown-Fitch | 2011 | none | **2011** |
| Nordonia Knight Relays | 2011 (score sheets) | none | **2011** |

Not on baumspage: any dual meets, Elk Relays, Mansfield Mehock Relays, Orange Relays, Bob Knoll, Triway, Berea Relays, Shelby Relays.
Because the baumspage archive pages are static HTML result tables, they are a good source for filling the 2003-2011 district,
regional and Suburban League results at athlete level.

## 4. Meets that exist in the DB but are thin or placeholders

- 21 meets are named "Unknown Meet" (dated May 1 of 1962, 1975, 1976, 1979, 1980, 1982, 1986, 1987, 1990, 1993, 1999, 2000,
  2001, 2002, 2005, 2006, 2007, 2009, 2011, 2013, 2017) with 1-4 results each: the date and meet are unknown.
- 56 meets have fewer than 10 results; most are state/regional/indoor-state meets (1-9 results, as only a few athletes qualify) and the
  early indoor meets of 2012-2013 (1-3 results each).
- Seasons with unusually few meets: 2005 (4), 2006 (5), 2008 (4), 2014 (5), 2015 (5).

## 5. Performance-level suspects (MileSplit rows vs DB results)

Meets where MileSplit has many more Cloverleaf rows than the DB has results (prelim/final double rows inflate the MS numbers, so
treat as leads): 2019 Nikki Reynolds Invitational (MS 97 rows vs DB 42), 2021 Cloverleaf Indoor Meet 1 (48 vs 24),
2021 Division I Regional (11 vs 6), 2022 Cloverleaf Early Bird Relays (58 vs 34), 2024 OHSAA State (7 vs 4).
MileSplit returned no Cloverleaf text for 13 matched meets (e.g. 2017-2019 state meets, 2025 Chippewa Invitational) so those could not be checked.

## 6. DB meets with no athletic.net / MileSplit counterpart (2005 onward)

88 meets: the 2021-2026 dual and double-dual meets (about 40), the Suburban League championships 2005-2013, Comet Relays, Kimberly
Relays, Husky Invitational and Elk Relays 2006-2013, and a handful of others. These are not necessarily wrong; no online source
exists to cross-check them. 66 more pre-2005 meets are in the same position (paper records only).

## 7. Suggested order to close the gaps
1. Import the five missing NE District results (2005, 2006, 2008, 2009, 2010) from baumspage (plus athletic.net where it lists them).
2. Add the missing named meets (Husky 2007, Wolverine 2008, Optimist 2011/2012/2016/2024, Nordonia 2011, Mansfield Mehock 2009,
   New London Relays 2018/2019, Triway 2018/2019, Green Twilight 2023, Streetsboro 2019/2025 and the 2025-26 indoor meets) from the
   site that has full results.
3. Identify the 21 "Unknown Meet" placeholders by matching athletes and marks to baumspage / athletic.net / MileSplit.
4. Spot-check the performance-level suspects in section 5.
5. Verify the 2005-2013 Suburban League rows against baumspage.
