// Input checks for plans, templates, and season phases. Each returns { value } or { error } with a message a coach
// can act on. Rules for groups, block kinds, and phases come from the shared browser modules. BRAIN.md [C134].
import { isIsoDate } from "../public/js/dates.js";
import { BLOCK_KIND_IDS, GROUP_IDS } from "../public/js/practice.js";
import { PHASE_KIND_IDS, phaseKind, phaseProblems } from "../public/js/season.js";

export type Result<T> = { value: T; error?: undefined } | { value?: undefined; error: string };

export interface Block {
  kind: string; title: string; minutes: number; groups: string[]; details: string;
  workoutId: number | null; attempts: number | null; results: { athleteId: number; mark: string }[];
}
export interface Phase {
  kind: string; name: string; startDate: string; endDate: string; volume: number; intensity: number; focus: string;
}

const str = (v: unknown, max: number) => (typeof v === "string" ? v.trim().slice(0, max) : "");
const int = (v: unknown) => (v === null || v === undefined || v === "" ? null : Number(v));
const inRange = (n: number | null, lo: number, hi: number) => n !== null && Number.isInteger(n) && n >= lo && n <= hi;

export function cleanGroups(raw: unknown, required: boolean): Result<string[]> {
  if (!Array.isArray(raw)) return { error: "groups must be a list" };
  const groups = [...new Set(raw.map(String))];
  const bad = groups.filter((g) => !GROUP_IDS.includes(g));
  if (bad.length) return { error: `unknown group: ${bad.join(", ")}` };
  if (required && groups.length === 0) return { error: "Pick at least one event group." };
  return { value: GROUP_IDS.filter((g) => groups.includes(g)) }; // canonical order
}

export function cleanBlocks(raw: unknown): Result<Block[]> {
  if (!Array.isArray(raw)) return { error: "blocks must be a list" };
  if (raw.length > 30) return { error: "A plan can have at most 30 blocks." };
  const out: Block[] = [];
  for (const [i, r] of raw.entries()) {
    const b = (r ?? {}) as Record<string, unknown>;
    const n = i + 1;
    if (!BLOCK_KIND_IDS.includes(String(b.kind))) return { error: `Block ${n}: unknown kind.` };
    const minutes = int(b.minutes) ?? 0;
    if (!inRange(minutes, 0, 240)) return { error: `Block ${n}: minutes must be 0-240.` };
    const groups = cleanGroups(b.groups ?? [], false);
    if (groups.error) return { error: `Block ${n}: ${groups.error}` };
    const workoutId = int(b.workoutId);
    if (workoutId !== null && !Number.isInteger(workoutId)) return { error: `Block ${n}: bad workout.` };
    const attempts = int(b.attempts);
    if (attempts !== null && !inRange(attempts, 1, 20)) return { error: `Block ${n}: attempts must be 1-20.` };
    const rawResults = Array.isArray(b.results) ? b.results : [];
    if (rawResults.length > 80) return { error: `Block ${n}: too many results.` };
    const results = [];
    for (const rr of rawResults) {
      const res = (rr ?? {}) as Record<string, unknown>;
      const athleteId = int(res.athleteId);
      const mark = str(res.mark, 40);
      if (athleteId === null || !Number.isInteger(athleteId)) return { error: `Block ${n}: pick an athlete for each mark.` };
      if (mark) results.push({ athleteId, mark });
    }
    out.push({
      kind: String(b.kind), title: str(b.title, 120), minutes, groups: groups.value!, details: str(b.details, 2000),
      workoutId, attempts, results
    });
  }
  return { value: out };
}

export function cleanDate(raw: unknown, label: string): Result<string> {
  return isIsoDate(raw) ? { value: raw as string } : { error: `${label} must be a date (YYYY-MM-DD).` };
}

export function cleanPhases(raw: unknown, start: string, end: string): Result<Phase[]> {
  if (!Array.isArray(raw)) return { error: "phases must be a list" };
  if (raw.length > 20) return { error: "A season plan can have at most 20 phases." };
  const out: Phase[] = [];
  for (const [i, r] of raw.entries()) {
    const p = (r ?? {}) as Record<string, unknown>;
    const n = i + 1;
    if (!PHASE_KIND_IDS.includes(String(p.kind))) return { error: `Phase ${n}: unknown kind.` };
    if (!isIsoDate(p.startDate) || !isIsoDate(p.endDate)) return { error: `Phase ${n}: start and end must be dates.` };
    const volume = int(p.volume), intensity = int(p.intensity);
    if (!inRange(volume, 1, 5) || !inRange(intensity, 1, 5)) return { error: `Phase ${n}: volume and intensity must be 1-5.` };
    out.push({
      kind: String(p.kind), name: str(p.name, 80) || phaseKind(String(p.kind)).label,
      startDate: p.startDate as string, endDate: p.endDate as string, volume: volume!, intensity: intensity!,
      focus: str(p.focus, 500)
    });
  }
  const problems = phaseProblems(out, start, end);
  if (problems.length) return { error: problems.join(" ") };
  out.sort((a, b) => a.startDate.localeCompare(b.startDate));
  return { value: out };
}

export { str as cleanText };
