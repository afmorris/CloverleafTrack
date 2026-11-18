// Validates the nightly snapshot from /Admin/CoachSnapshot and turns it into one D1 batch (a single transaction):
// empty every snapshot table, then refill it. BRAIN.md [C133].
import { parseEventDistance } from "../public/js/pace.js";

export const SNAPSHOT_VERSION = 1;

export interface Snapshot {
  version: number;
  generatedAt: string;
  athletes: { id: number; firstName: string; lastName: string; gender: number; graduationYear: number }[];
  events: {
    id: number; name: string; eventKey: string; eventType: number; gender: number;
    environment: number; eventCategory: number; sortOrder: number;
  }[];
  seasons: { id: number; name: string; startDate: string; endDate: string; isCurrentSeason: boolean }[];
  seasonBests: {
    athleteId: number; eventId: number; seasonId: number; timeSeconds: number | null;
    distanceInches: number | null; meetName: string; meetDate: string;
  }[];
  meets: { id: number; name: string; date: string; environment: number; seasonId: number }[];
}

/** Returns a list of problems; empty means the snapshot is safe to load. */
export function validateSnapshot(body: unknown): string[] {
  const errors: string[] = [];
  if (typeof body !== "object" || body === null) return ["body is not a JSON object"];
  const s = body as Record<string, unknown>;
  if (s.version !== SNAPSHOT_VERSION) errors.push(`version must be ${SNAPSHOT_VERSION}`);
  if (typeof s.generatedAt !== "string") errors.push("generatedAt is missing");
  for (const key of ["athletes", "events", "seasons", "seasonBests", "meets"]) {
    if (!Array.isArray(s[key])) errors.push(`${key} must be an array`);
  }
  if (errors.length) return errors;
  // An empty roster almost always means the export broke; refuse rather than wipe the site.
  if ((s.athletes as unknown[]).length === 0) errors.push("athletes is empty");
  if ((s.events as unknown[]).length === 0) errors.push("events is empty");
  return errors;
}

// D1 allows at most 100 bound parameters per statement, so multi-row inserts are chunked to fit.
const MAX_PARAMS = 100;

function insertChunks(db: D1Database, table: string, columns: string[], rows: unknown[][]): D1PreparedStatement[] {
  const perStatement = Math.max(1, Math.floor(MAX_PARAMS / columns.length));
  const out: D1PreparedStatement[] = [];
  for (let i = 0; i < rows.length; i += perStatement) {
    const chunk = rows.slice(i, i + perStatement);
    const placeholders = chunk.map(() => `(${columns.map(() => "?").join(", ")})`).join(", ");
    out.push(db.prepare(`INSERT INTO ${table} (${columns.join(", ")}) VALUES ${placeholders}`).bind(...chunk.flat()));
  }
  return out;
}

export function snapshotStatements(db: D1Database, s: Snapshot, loadedAt: string): D1PreparedStatement[] {
  return [
    ...["season_bests", "meets", "seasons", "events", "athletes"].map((t) => db.prepare(`DELETE FROM ${t}`)),
    ...insertChunks(db, "athletes", ["id", "first_name", "last_name", "gender", "graduation_year"],
      s.athletes.map((a) => [a.id, a.firstName, a.lastName, a.gender, a.graduationYear])),
    ...insertChunks(db, "events",
      ["id", "name", "event_key", "event_type", "gender", "environment", "event_category", "sort_order", "distance_m"],
      s.events.map((e) => [e.id, e.name, e.eventKey, e.eventType, e.gender, e.environment, e.eventCategory,
        e.sortOrder, e.eventType === 1 ? parseEventDistance(e.name) : null])),
    ...insertChunks(db, "seasons", ["id", "name", "start_date", "end_date", "is_current"],
      s.seasons.map((x) => [x.id, x.name, x.startDate, x.endDate, x.isCurrentSeason ? 1 : 0])),
    ...insertChunks(db, "season_bests",
      ["athlete_id", "event_id", "season_id", "time_seconds", "distance_inches", "meet_name", "meet_date"],
      s.seasonBests.map((b) => [b.athleteId, b.eventId, b.seasonId, b.timeSeconds, b.distanceInches,
        b.meetName, b.meetDate])),
    ...insertChunks(db, "meets", ["id", "name", "date", "environment", "season_id"],
      s.meets.map((m) => [m.id, m.name, m.date, m.environment, m.seasonId])),
    db.prepare(`INSERT INTO snapshot_meta (id, version, generated_at, loaded_at) VALUES (1, ?, ?, ?)
                ON CONFLICT (id) DO UPDATE SET version = excluded.version, generated_at = excluded.generated_at,
                loaded_at = excluded.loaded_at`).bind(s.version, s.generatedAt, loadedAt)
  ];
}
