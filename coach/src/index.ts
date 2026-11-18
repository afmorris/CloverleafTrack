// Cloverleaf Coach Worker: JSON API under /api/*. Static pages come from ./public via the ASSETS binding.
// Sign-in is Cloudflare Access in front of the whole hostname; this code trusts Access and only reads the email
// header it adds. POST /api/snapshot is the exception: it is called by a script, so it checks a bearer token.
// BRAIN.md [C133].
import { Hono } from "hono";
import plans, { userEmail } from "./plans";
import { snapshotStatements, validateSnapshot, type Snapshot } from "./snapshot";

type Env = { DB: D1Database; ASSETS: Fetcher; SNAPSHOT_TOKEN?: string };

const app = new Hono<{ Bindings: Env }>().basePath("/api");

/** Constant-time string compare, so the token can't be guessed one character at a time. */
function safeEqual(a: string, b: string): boolean {
  const ea = new TextEncoder().encode(a);
  const eb = new TextEncoder().encode(b);
  let diff = ea.length ^ eb.length;
  for (let i = 0; i < Math.max(ea.length, eb.length); i++) diff |= (ea[i] ?? 0) ^ (eb[i] ?? 0);
  return diff === 0;
}

app.get("/health", (c) => c.json({ ok: true }));

app.get("/me", (c) => c.json({ email: userEmail(c.req.raw) }));

app.get("/snapshot", async (c) => {
  const meta = await c.env.DB.prepare("SELECT version, generated_at, loaded_at FROM snapshot_meta WHERE id = 1").first();
  return c.json(meta ?? { version: null, generated_at: null, loaded_at: null });
});

app.post("/snapshot", async (c) => {
  const expected = c.env.SNAPSHOT_TOKEN;
  const given = (c.req.header("Authorization") ?? "").replace(/^Bearer\s+/i, "");
  if (!expected) return c.json({ error: "SNAPSHOT_TOKEN is not configured" }, 503);
  if (!safeEqual(given, expected)) return c.json({ error: "unauthorized" }, 401);

  let body: unknown;
  try { body = await c.req.json(); } catch { return c.json({ error: "body is not JSON" }, 400); }
  const errors = validateSnapshot(body);
  if (errors.length) return c.json({ error: "invalid snapshot", details: errors }, 400);

  const s = body as Snapshot;
  await c.env.DB.batch(snapshotStatements(c.env.DB, s, new Date().toISOString()));
  return c.json({
    loaded: { athletes: s.athletes.length, events: s.events.length, seasons: s.seasons.length,
      seasonBests: s.seasonBests.length, meets: s.meets.length },
    generatedAt: s.generatedAt
  });
});

app.get("/athletes", async (c) => {
  const { results } = await c.env.DB.prepare(
    `SELECT id, first_name AS firstName, last_name AS lastName, gender, graduation_year AS graduationYear
     FROM athletes ORDER BY last_name, first_name`).all();
  return c.json(results);
});

/** Every season best for one athlete, newest season first; the page works out PRs and picks events. */
app.get("/athletes/:id/marks", async (c) => {
  const id = Number(c.req.param("id"));
  if (!Number.isInteger(id)) return c.json({ error: "bad id" }, 400);
  const { results } = await c.env.DB.prepare(
    `SELECT sb.event_id AS eventId, e.name AS eventName, e.event_type AS eventType, e.environment,
            e.distance_m AS distanceM, e.sort_order AS sortOrder,
            sb.season_id AS seasonId, s.name AS seasonName, s.start_date AS seasonStart,
            sb.time_seconds AS timeSeconds, sb.distance_inches AS distanceInches,
            sb.meet_name AS meetName, sb.meet_date AS meetDate
     FROM season_bests sb
     JOIN events e ON e.id = sb.event_id
     JOIN seasons s ON s.id = sb.season_id
     WHERE sb.athlete_id = ?
     ORDER BY e.sort_order, s.start_date DESC`).bind(id).all();
  return c.json(results);
});

type SetInput = { reps: number; distance: number; effort: number; rest: number };

function cleanSets(raw: unknown): SetInput[] | null {
  if (!Array.isArray(raw) || raw.length === 0 || raw.length > 20) return null;
  const sets: SetInput[] = [];
  for (const r of raw) {
    const s = r as Record<string, unknown>;
    const set = { reps: Number(s.reps), distance: Number(s.distance), effort: Number(s.effort), rest: Number(s.rest ?? 0) };
    if (!(set.reps >= 1 && set.reps <= 50 && set.distance > 0 && set.distance <= 10000
      && set.effort >= 50 && set.effort <= 110 && set.rest >= 0 && set.rest <= 3600)) return null;
    sets.push(set);
  }
  return sets;
}

app.get("/workouts", async (c) => {
  const { results } = await c.env.DB.prepare(
    `SELECT w.id, w.title, w.athlete_id AS athleteId, a.first_name || ' ' || a.last_name AS athleteName,
            w.event_name AS eventName, w.event_distance_m AS eventDistanceM, w.goal_seconds AS goalSeconds,
            w.sets_json AS setsJson, w.notes, w.created_by AS createdBy, w.created_at AS createdAt
     FROM workouts w LEFT JOIN athletes a ON a.id = w.athlete_id
     ORDER BY w.created_at DESC LIMIT 100`).all();
  return c.json(results.map(({ setsJson, ...w }) => ({ ...w, sets: JSON.parse(String(setsJson)) })));
});

app.post("/workouts", async (c) => {
  let b: Record<string, unknown>;
  try { b = await c.req.json(); } catch { return c.json({ error: "body is not JSON" }, 400); }
  const title = String(b.title ?? "").trim().slice(0, 200);
  const goalSeconds = Number(b.goalSeconds);
  const eventDistanceM = Number(b.eventDistanceM);
  const athleteId = b.athleteId == null || b.athleteId === "" ? null : Number(b.athleteId);
  const sets = cleanSets(b.sets);
  if (!title) return c.json({ error: "title is required" }, 400);
  if (!(goalSeconds > 0 && eventDistanceM > 0)) return c.json({ error: "goal and event distance are required" }, 400);
  if (athleteId !== null && !Number.isInteger(athleteId)) return c.json({ error: "bad athleteId" }, 400);
  if (!sets) return c.json({ error: "sets are missing or out of range" }, 400);

  const row = await c.env.DB.prepare(
    `INSERT INTO workouts (title, athlete_id, event_name, event_distance_m, goal_seconds, sets_json, notes, created_by)
     VALUES (?, ?, ?, ?, ?, ?, ?, ?) RETURNING id`)
    .bind(title, athleteId, b.eventName ? String(b.eventName).slice(0, 100) : null, eventDistanceM, goalSeconds,
      JSON.stringify(sets), b.notes ? String(b.notes).slice(0, 2000) : null, userEmail(c.req.raw))
    .first<{ id: number }>();
  return c.json({ id: row?.id }, 201);
});

app.get("/workouts/:id", async (c) => {
  const id = Number(c.req.param("id"));
  if (!Number.isInteger(id)) return c.json({ error: "bad id" }, 400);
  const w = await c.env.DB.prepare(
    `SELECT w.id, w.title, w.athlete_id AS athleteId, a.first_name || ' ' || a.last_name AS athleteName,
            w.event_name AS eventName, w.event_distance_m AS eventDistanceM, w.goal_seconds AS goalSeconds,
            w.sets_json AS setsJson, w.notes, w.created_by AS createdBy, w.created_at AS createdAt
     FROM workouts w LEFT JOIN athletes a ON a.id = w.athlete_id WHERE w.id = ?`).bind(id).first();
  if (!w) return c.json({ error: "not found" }, 404);
  const { setsJson, ...rest } = w;
  return c.json({ ...rest, sets: JSON.parse(String(setsJson)) });
});

app.delete("/workouts/:id", async (c) => {
  const id = Number(c.req.param("id"));
  if (!Number.isInteger(id)) return c.json({ error: "bad id" }, 400);
  const r = await c.env.DB.prepare("DELETE FROM workouts WHERE id = ?").bind(id).run();
  return r.meta.changes ? c.body(null, 204) : c.json({ error: "not found" }, 404);
});

app.route("/", plans);

app.notFound((c) => c.json({ error: "not found" }, 404));
app.onError((err, c) => {
  console.error(err);
  return c.json({ error: "server error" }, 500);
});

export default app;
