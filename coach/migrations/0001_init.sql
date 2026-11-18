-- Cloverleaf Coach schema. BRAIN.md [C133].
--
-- Two kinds of tables:
--   * Snapshot tables (athletes, events, seasons, season_bests, meets, snapshot_meta) are a read-only copy of the
--     home SQL Server, replaced wholesale by POST /api/snapshot each night. Never edit them here.
--   * Owned tables (workouts, ...) hold what coaches create on this site. They refer to snapshot ids WITHOUT foreign
--     keys, because the snapshot tables are emptied and refilled on every load.

CREATE TABLE athletes (
  id INTEGER PRIMARY KEY,
  first_name TEXT NOT NULL,
  last_name TEXT NOT NULL,
  gender INTEGER NOT NULL,          -- 1 male, 2 female
  graduation_year INTEGER NOT NULL
);

CREATE TABLE events (
  id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  event_key TEXT NOT NULL,
  event_type INTEGER NOT NULL,      -- 0 field, 1 running (individual events only)
  gender INTEGER NOT NULL,
  environment INTEGER NOT NULL,     -- 1 outdoor, 2 indoor
  event_category INTEGER NOT NULL,
  sort_order INTEGER NOT NULL,
  distance_m REAL                   -- parsed from the name for running events; NULL for field events
);

CREATE TABLE seasons (
  id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  start_date TEXT NOT NULL,
  end_date TEXT NOT NULL,
  is_current INTEGER NOT NULL
);

-- An athlete's best mark per event per season (marks only; no NM/DNF rows).
CREATE TABLE season_bests (
  athlete_id INTEGER NOT NULL,
  event_id INTEGER NOT NULL,
  season_id INTEGER NOT NULL,
  time_seconds REAL,
  distance_inches REAL,
  meet_name TEXT NOT NULL,
  meet_date TEXT NOT NULL,
  PRIMARY KEY (athlete_id, event_id, season_id)
);

CREATE TABLE meets (
  id INTEGER PRIMARY KEY,
  name TEXT NOT NULL,
  date TEXT NOT NULL,
  environment INTEGER NOT NULL,
  season_id INTEGER NOT NULL
);

CREATE TABLE snapshot_meta (
  id INTEGER PRIMARY KEY CHECK (id = 1),
  version INTEGER NOT NULL,
  generated_at TEXT NOT NULL,
  loaded_at TEXT NOT NULL
);

CREATE TABLE workouts (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  title TEXT NOT NULL,
  athlete_id INTEGER,               -- snapshot athlete id, or NULL for a workout built from a typed goal
  event_name TEXT,
  event_distance_m REAL NOT NULL,
  goal_seconds REAL NOT NULL,
  sets_json TEXT NOT NULL,          -- [{ reps, distance, effort, rest }]
  notes TEXT,
  created_by TEXT NOT NULL,         -- Cloudflare Access email, or "local" in wrangler dev
  created_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%SZ', 'now'))
);
CREATE INDEX ix_workouts_created_at ON workouts (created_at DESC);
