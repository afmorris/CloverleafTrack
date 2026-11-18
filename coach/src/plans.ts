// Practice plans, the template library, season plans, and the meet schedule. Mounted under /api. BRAIN.md [C134].
import { Hono, type Context } from "hono";
import { cleanBlocks, cleanDate, cleanGroups, cleanPhases, cleanText, type Phase } from "./validate";

type Env = { DB: D1Database };
type C = Context<{ Bindings: Env }>;

export const userEmail = (req: Request) => req.headers.get("Cf-Access-Authenticated-User-Email") ?? "local";
const now = () => new Date().toISOString().replace(/\.\d+Z$/, "Z");
const idParam = (c: C) => { const n = Number(c.req.param("id")); return Number.isInteger(n) && n > 0 ? n : null; };
const bad = (c: C, error: string) => c.json({ error }, 400);
async function body(c: C): Promise<Record<string, unknown> | null> {
  try { const b = await c.req.json(); return b && typeof b === "object" ? b : null; } catch { return null; }
}
const parseJson = (s: unknown) => JSON.parse(String(s));

const plans = new Hono<{ Bindings: Env }>();

// ---- Meets (snapshot) ------------------------------------------------------------------------------------------

plans.get("/meets", async (c) => {
  const from = c.req.query("from"), to = c.req.query("to");
  const stmt = from && to
    ? c.env.DB.prepare("SELECT id, name, date, environment FROM meets WHERE date BETWEEN ? AND ? ORDER BY date").bind(from, to)
    : c.env.DB.prepare("SELECT id, name, date, environment FROM meets ORDER BY date");
  return c.json((await stmt.all()).results);
});

/** The snapshot seasons, newest first: used to prefill a season plan's dates. */
plans.get("/seasons", async (c) => {
  const { results } = await c.env.DB.prepare(
    "SELECT id, name, start_date AS startDate, end_date AS endDate, is_current AS isCurrent FROM seasons ORDER BY start_date DESC LIMIT 6").all();
  return c.json(results);
});

// ---- Templates ------------------------------------------------------------------------------------------------

const templateRow = (r: Record<string, unknown>) => ({
  id: r.id, name: r.name, description: r.description, builtIn: !!r.built_in, createdBy: r.created_by,
  updatedAt: r.updated_at, groups: parseJson(r.groups_json), blocks: parseJson(r.blocks_json)
});

plans.get("/templates", async (c) => {
  const { results } = await c.env.DB.prepare("SELECT * FROM practice_templates ORDER BY built_in DESC, name").all();
  return c.json(results.map(templateRow));
});

plans.get("/templates/:id", async (c) => {
  const id = idParam(c);
  if (!id) return bad(c, "bad id");
  const r = await c.env.DB.prepare("SELECT * FROM practice_templates WHERE id = ?").bind(id).first();
  return r ? c.json(templateRow(r)) : c.json({ error: "not found" }, 404);
});

function cleanTemplate(b: Record<string, unknown>) {
  const name = cleanText(b.name, 120);
  if (!name) return { error: "Give the template a name." };
  const groups = cleanGroups(b.groups, true);
  if (groups.error) return { error: groups.error };
  const blocks = cleanBlocks(b.blocks);
  if (blocks.error) return { error: blocks.error };
  if (!blocks.value!.length) return { error: "Add at least one block." };
  // Templates never carry recorded marks.
  const clean = blocks.value!.map((x) => ({ ...x, results: [] }));
  return { name, description: cleanText(b.description, 500) || null, groups: groups.value!, blocks: clean };
}

plans.post("/templates", async (c) => {
  const b = await body(c);
  if (!b) return bad(c, "body is not JSON");
  const t = cleanTemplate(b);
  if ("error" in t) return bad(c, t.error!);
  const r = await c.env.DB.prepare(
    `INSERT INTO practice_templates (name, description, groups_json, blocks_json, built_in, created_by)
     VALUES (?, ?, ?, ?, 0, ?) RETURNING id`)
    .bind(t.name, t.description, JSON.stringify(t.groups), JSON.stringify(t.blocks), userEmail(c.req.raw))
    .first<{ id: number }>();
  return c.json({ id: r?.id }, 201);
});

plans.put("/templates/:id", async (c) => {
  const id = idParam(c);
  const b = await body(c);
  if (!id || !b) return bad(c, "bad request");
  const t = cleanTemplate(b);
  if ("error" in t) return bad(c, t.error!);
  const r = await c.env.DB.prepare(
    `UPDATE practice_templates SET name = ?, description = ?, groups_json = ?, blocks_json = ?, updated_at = ?
     WHERE id = ? AND built_in = 0`)
    .bind(t.name, t.description, JSON.stringify(t.groups), JSON.stringify(t.blocks), now(), id).run();
  return r.meta.changes ? c.json({ id }) : c.json({ error: "Not found, or a built-in template (copy it instead)." }, 404);
});

plans.delete("/templates/:id", async (c) => {
  const id = idParam(c);
  if (!id) return bad(c, "bad id");
  const r = await c.env.DB.prepare("DELETE FROM practice_templates WHERE id = ? AND built_in = 0").bind(id).run();
  return r.meta.changes ? c.body(null, 204) : c.json({ error: "Not found, or a built-in template." }, 404);
});

// ---- Practice plans -------------------------------------------------------------------------------------------

const planRow = (r: Record<string, unknown>) => ({
  id: r.id, date: r.date, title: r.title, notes: r.notes, createdBy: r.created_by, updatedBy: r.updated_by,
  updatedAt: r.updated_at, groups: parseJson(r.groups_json), blocks: parseJson(r.blocks_json)
});

plans.get("/practices", async (c) => {
  const from = cleanDate(c.req.query("from"), "from"), to = cleanDate(c.req.query("to"), "to");
  if (from.error || to.error) return bad(c, from.error ?? to.error!);
  const { results } = await c.env.DB.prepare(
    "SELECT * FROM practice_plans WHERE date BETWEEN ? AND ? ORDER BY date, id").bind(from.value, to.value).all();
  return c.json(results.map(planRow));
});

plans.get("/practices/:id", async (c) => {
  const id = idParam(c);
  if (!id) return bad(c, "bad id");
  const r = await c.env.DB.prepare("SELECT * FROM practice_plans WHERE id = ?").bind(id).first();
  return r ? c.json(planRow(r)) : c.json({ error: "not found" }, 404);
});

function cleanPlan(b: Record<string, unknown>) {
  const date = cleanDate(b.date, "Date");
  if (date.error) return { error: date.error };
  const title = cleanText(b.title, 120);
  if (!title) return { error: "Give the plan a title." };
  const groups = cleanGroups(b.groups, true);
  if (groups.error) return { error: groups.error };
  const blocks = cleanBlocks(b.blocks);
  if (blocks.error) return { error: blocks.error };
  return { date: date.value!, title, groups: groups.value!, blocks: blocks.value!, notes: cleanText(b.notes, 4000) || null };
}

plans.post("/practices", async (c) => {
  const b = await body(c);
  if (!b) return bad(c, "body is not JSON");
  const p = cleanPlan(b);
  if ("error" in p) return bad(c, p.error!);
  const who = userEmail(c.req.raw);
  const r = await c.env.DB.prepare(
    `INSERT INTO practice_plans (date, title, groups_json, blocks_json, notes, created_by, updated_by)
     VALUES (?, ?, ?, ?, ?, ?, ?) RETURNING id`)
    .bind(p.date, p.title, JSON.stringify(p.groups), JSON.stringify(p.blocks), p.notes, who, who)
    .first<{ id: number }>();
  return c.json({ id: r?.id }, 201);
});

plans.put("/practices/:id", async (c) => {
  const id = idParam(c);
  const b = await body(c);
  if (!id || !b) return bad(c, "bad request");
  const p = cleanPlan(b);
  if ("error" in p) return bad(c, p.error!);
  const r = await c.env.DB.prepare(
    `UPDATE practice_plans SET date = ?, title = ?, groups_json = ?, blocks_json = ?, notes = ?, updated_by = ?,
     updated_at = ? WHERE id = ?`)
    .bind(p.date, p.title, JSON.stringify(p.groups), JSON.stringify(p.blocks), p.notes, userEmail(c.req.raw), now(), id)
    .run();
  return r.meta.changes ? c.json({ id }) : c.json({ error: "not found" }, 404);
});

/** Copy a plan to another date. Recorded marks stay with the original. */
plans.post("/practices/:id/copy", async (c) => {
  const id = idParam(c);
  const b = await body(c);
  const date = cleanDate(b?.date, "Date");
  if (!id || date.error) return bad(c, date.error ?? "bad id");
  const src = await c.env.DB.prepare("SELECT * FROM practice_plans WHERE id = ?").bind(id).first();
  if (!src) return c.json({ error: "not found" }, 404);
  const blocks = (parseJson(src.blocks_json) as { results?: unknown[] }[]).map((x) => ({ ...x, results: [] }));
  const who = userEmail(c.req.raw);
  const r = await c.env.DB.prepare(
    `INSERT INTO practice_plans (date, title, groups_json, blocks_json, notes, created_by, updated_by)
     VALUES (?, ?, ?, ?, ?, ?, ?) RETURNING id`)
    .bind(date.value, src.title, src.groups_json, JSON.stringify(blocks), src.notes, who, who).first<{ id: number }>();
  return c.json({ id: r?.id }, 201);
});

plans.delete("/practices/:id", async (c) => {
  const id = idParam(c);
  if (!id) return bad(c, "bad id");
  const r = await c.env.DB.prepare("DELETE FROM practice_plans WHERE id = ?").bind(id).run();
  return r.meta.changes ? c.body(null, 204) : c.json({ error: "not found" }, 404);
});

// ---- Season plans ---------------------------------------------------------------------------------------------

const phaseCols = `kind, name, start_date AS startDate, end_date AS endDate, volume, intensity, focus`;

plans.get("/season-plans", async (c) => {
  const { results } = await c.env.DB.prepare(
    `SELECT id, name, start_date AS startDate, end_date AS endDate, notes, updated_at AS updatedAt
     FROM season_plans ORDER BY start_date DESC`).all();
  const { results: phases } = await c.env.DB.prepare(
    `SELECT season_plan_id AS planId, ${phaseCols} FROM season_phases ORDER BY start_date`).all();
  return c.json(results.map((p) => ({ ...p, phases: phases.filter((x) => x.planId === p.id).map(({ planId, ...x }) => x) })));
});

plans.get("/season-plans/:id", async (c) => {
  const id = idParam(c);
  if (!id) return bad(c, "bad id");
  const plan = await c.env.DB.prepare(
    `SELECT id, name, start_date AS startDate, end_date AS endDate, notes, updated_at AS updatedAt
     FROM season_plans WHERE id = ?`).bind(id).first();
  if (!plan) return c.json({ error: "not found" }, 404);
  const { results } = await c.env.DB.prepare(
    `SELECT ${phaseCols} FROM season_phases WHERE season_plan_id = ? ORDER BY start_date`).bind(id).all();
  return c.json({ ...plan, phases: results });
});

function cleanSeason(b: Record<string, unknown>) {
  const name = cleanText(b.name, 120);
  if (!name) return { error: "Give the season plan a name." };
  const start = cleanDate(b.startDate, "Start"), end = cleanDate(b.endDate, "End");
  if (start.error || end.error) return { error: start.error ?? end.error! };
  if (end.value! < start.value!) return { error: "The season ends before it starts." };
  const phases = cleanPhases(b.phases ?? [], start.value!, end.value!);
  if (phases.error) return { error: phases.error };
  return { name, startDate: start.value!, endDate: end.value!, notes: cleanText(b.notes, 4000) || null, phases: phases.value! };
}

/** Phase inserts for a plan. planId null means "the plan inserted earlier in this same batch". */
function insertPhases(db: D1Database, planId: number | null, phases: Phase[]): D1PreparedStatement[] {
  const owner = planId === null ? "(SELECT MAX(id) FROM season_plans)" : "?";
  return phases.map((p) => {
    const stmt = db.prepare(
      `INSERT INTO season_phases (season_plan_id, kind, name, start_date, end_date, volume, intensity, focus)
       VALUES (${owner}, ?, ?, ?, ?, ?, ?, ?)`);
    const values = [p.kind, p.name, p.startDate, p.endDate, p.volume, p.intensity, p.focus];
    return planId === null ? stmt.bind(...values) : stmt.bind(planId, ...values);
  });
}

plans.post("/season-plans", async (c) => {
  const b = await body(c);
  if (!b) return bad(c, "body is not JSON");
  const s = cleanSeason(b);
  if ("error" in s) return bad(c, s.error!);
  const db = c.env.DB;
  // One batch = one transaction, so MAX(id) is the plan inserted by the first statement.
  const res = await db.batch([
    db.prepare(`INSERT INTO season_plans (name, start_date, end_date, notes, created_by) VALUES (?, ?, ?, ?, ?)`)
      .bind(s.name, s.startDate, s.endDate, s.notes, userEmail(c.req.raw)),
    ...insertPhases(db, null, s.phases),
    db.prepare("SELECT MAX(id) AS id FROM season_plans")
  ]);
  const last = res[res.length - 1].results[0] as { id: number };
  return c.json({ id: last.id }, 201);
});

plans.put("/season-plans/:id", async (c) => {
  const id = idParam(c);
  const b = await body(c);
  if (!id || !b) return bad(c, "bad request");
  const s = cleanSeason(b);
  if ("error" in s) return bad(c, s.error!);
  const exists = await c.env.DB.prepare("SELECT 1 FROM season_plans WHERE id = ?").bind(id).first();
  if (!exists) return c.json({ error: "not found" }, 404);
  const db = c.env.DB;
  await db.batch([
    db.prepare("UPDATE season_plans SET name = ?, start_date = ?, end_date = ?, notes = ?, updated_at = ? WHERE id = ?")
      .bind(s.name, s.startDate, s.endDate, s.notes, now(), id),
    db.prepare("DELETE FROM season_phases WHERE season_plan_id = ?").bind(id),
    ...insertPhases(db, id, s.phases)
  ]);
  return c.json({ id });
});

plans.delete("/season-plans/:id", async (c) => {
  const id = idParam(c);
  if (!id) return bad(c, "bad id");
  const db = c.env.DB;
  const res = await db.batch([
    db.prepare("DELETE FROM season_phases WHERE season_plan_id = ?").bind(id),
    db.prepare("DELETE FROM season_plans WHERE id = ?").bind(id)
  ]);
  return res[1].meta.changes ? c.body(null, 204) : c.json({ error: "not found" }, 404);
});

export default plans;
