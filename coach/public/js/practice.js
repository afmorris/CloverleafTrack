// Practice plan model: event groups, block kinds, and small pure helpers. Shared by the pages, the Worker
// (validation), and the tests.

export const GROUPS = [
  { id: "sprints", label: "Sprints" },
  { id: "hurdles", label: "Hurdles" },
  { id: "distance", label: "Distance" },
  { id: "jumps", label: "Jumps" },
  { id: "throws", label: "Throws" }
];
export const GROUP_IDS = GROUPS.map((g) => g.id);
export const groupLabel = (id) => GROUPS.find((g) => g.id === id)?.label ?? id;

export const BLOCK_KINDS = [
  { id: "warmup", label: "Warm-up" },
  { id: "drills", label: "Drills" },
  { id: "workout", label: "Workout" },
  { id: "measured", label: "Measured attempts" },
  { id: "strength", label: "Strength" },
  { id: "cooldown", label: "Cool-down" },
  { id: "other", label: "Other" }
];
export const BLOCK_KIND_IDS = BLOCK_KINDS.map((k) => k.id);
export const kindLabel = (id) => BLOCK_KINDS.find((k) => k.id === id)?.label ?? id;

/** A new, empty block. Groups default to the plan's groups. */
export function newBlock(kind = "workout", groups = []) {
  return { kind, title: "", minutes: 15, groups: [...groups], details: "", workoutId: null, attempts: null, results: [] };
}

export function totalMinutes(blocks) {
  return (blocks ?? []).reduce((sum, b) => sum + (Number(b.minutes) > 0 ? Number(b.minutes) : 0), 0);
}

/**
 * How long practice runs. Groups train side by side, so this is the longest single group's time: the sum of the
 * blocks each group is in (a block with no groups counts for everyone).
 */
export function practiceLength(blocks) {
  const list = blocks ?? [];
  const groups = new Set(list.flatMap((b) => b.groups ?? []));
  if (!groups.size) return totalMinutes(list);
  let longest = 0;
  for (const g of groups) {
    longest = Math.max(longest, totalMinutes(list.filter((b) => !b.groups?.length || b.groups.includes(g))));
  }
  return longest;
}

/** "1h 35m", "45m". */
export function formatMinutes(m) {
  if (!(m > 0)) return "0m";
  const h = Math.floor(m / 60), r = m % 60;
  return h ? (r ? `${h}h ${r}m` : `${h}h`) : `${r}m`;
}

/** Copy template blocks into a plan, keeping only groups the plan trains (a block with none left keeps all). */
export function blocksFromTemplate(templateBlocks, planGroups) {
  return (templateBlocks ?? []).map((b) => {
    const kept = (b.groups ?? []).filter((g) => !planGroups?.length || planGroups.includes(g));
    return { ...newBlock(b.kind), ...b, groups: kept.length ? kept : [...(planGroups ?? [])], results: [] };
  });
}

/**
 * Add a template's blocks to a plan that already has some. A warm-up or cool-down the plan already has is not
 * repeated: the template's groups are added to the existing one, so a combined practice keeps one shared warm-up.
 */
export function mergeTemplateBlocks(existing, templateBlocks, planGroups) {
  const out = existing.map((b) => ({ ...b, groups: [...(b.groups ?? [])] }));
  for (const b of blocksFromTemplate(templateBlocks, planGroups)) {
    const shared = (b.kind === "warmup" || b.kind === "cooldown") && out.find((x) => x.kind === b.kind);
    if (shared) {
      const groups = new Set([...shared.groups, ...b.groups]);
      shared.groups = GROUP_IDS.filter((g) => groups.has(g));
      shared.minutes = Math.max(Number(shared.minutes) || 0, Number(b.minutes) || 0);
    } else {
      out.push(b);
    }
  }
  // Keep the cool-down last.
  const cool = out.findIndex((x) => x.kind === "cooldown");
  if (cool >= 0 && cool < out.length - 1) out.push(out.splice(cool, 1)[0]);
  return out;
}

/** Swap block i with its neighbour in direction dir (-1 up, +1 down). Returns a new array. */
export function moveBlock(blocks, i, dir) {
  const j = i + dir;
  if (j < 0 || j >= blocks.length) return blocks;
  const out = [...blocks];
  [out[i], out[j]] = [out[j], out[i]];
  return out;
}

/** Number of marks recorded in measured-attempt blocks (marks are free text, e.g. 18' 4" or 42.5m). */
export function resultCount(blocks) {
  return (blocks ?? []).reduce((n, b) => n + (b.results?.filter((r) => r.mark)?.length ?? 0), 0);
}
