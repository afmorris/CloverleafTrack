// Athlete handout: one practice week rewritten for athletes, to paste into an Apptegy message or save as a PDF.
// Pure helpers shared by the page and the tests. Coach notes, assumptions, and recorded marks are left out on
// purpose: only the time, place, weekly focus, blocks, and meet logistics go to athletes. BRAIN.md [C139].
import { formatDay } from "./dates.js";
import { planTime } from "./practice.js";

const firstLine = (notes) => String(notes ?? "").split(/\n/)[0];
const paragraphs = (notes) => String(notes ?? "").split(/\n\s*\n/).map((p) => p.trim()).filter(Boolean);

/**
 * The place written after the time at the start of a plan's notes: "3:00-5:15 PM, rings + HS lifting room. Season
 * week 3." gives "rings + HS lifting room". Returns "" when the notes don't start with a time.
 */
export function planPlace(notes) {
  const line = firstLine(notes);
  const time = planTime(line);
  if (!time) return "";
  const rest = line.slice(line.indexOf(time) + time.length).replace(/^\s*\([^)]*\)/, "").replace(/^\s*,\s*/, "");
  return rest.split(/\.\s/)[0].replace(/\.$/, "").trim();
}

/** The "Focus this week: ..." text from a plan's notes, without the label, or "". */
export function weekFocus(notes) {
  const p = paragraphs(notes).find((x) => /^Focus this week:/i.test(x));
  return p ? p.replace(/^Focus this week:\s*/i, "").trim() : "";
}

const isMeetPlan = (p) => /^Meet:/i.test(p.title ?? "");

/**
 * The athlete view of a week. practices and meets are the API rows for the week; group limits practices to one
 * event group ("" = all). Meet plans ("Meet: ...") show their first notes paragraph (place, start, departure);
 * snapshot meets without such a plan show by name. Days with nothing on them are left out.
 */
export function buildHandout({ practices, meets, group = "" }) {
  const list = (practices ?? []).filter((p) => !group || (p.groups ?? []).includes(group))
    .sort((a, b) => a.date.localeCompare(b.date) || (a.id ?? 0) - (b.id ?? 0));
  const focus = list.map((p) => weekFocus(p.notes)).find(Boolean) ?? "";
  const dates = [...new Set([...list.map((p) => p.date), ...(meets ?? []).map((m) => m.date)])].sort();
  const days = dates.map((date) => {
    const plans = list.filter((p) => p.date === date);
    const entries = plans.map((p) => isMeetPlan(p)
      ? { kind: "meet", title: p.title.replace(/^Meet:\s*/i, ""), info: paragraphs(p.notes)[0] ?? "" }
      : { kind: "practice", title: p.title, time: planTime(p.notes), place: planPlace(p.notes),
          blocks: (p.blocks ?? []).map((b) => ({ title: b.title || b.kind, minutes: b.minutes || 0, details: b.details || "" })) });
    for (const m of (meets ?? []).filter((x) => x.date === date)) {
      if (!plans.some((p) => isMeetPlan(p) && p.title.includes(m.name))) entries.push({ kind: "meet", title: m.name, info: "" });
    }
    return { date, entries };
  }).filter((d) => d.entries.length);
  return { focus, days };
}

/** Plain text for an Apptegy message: heading, optional message, weekly focus, then each day. */
export function handoutText(handout, { heading, message = "", details = true }) {
  const out = [heading];
  if (message.trim()) out.push("", message.trim());
  if (handout.focus) out.push("", `Focus this week: ${handout.focus}`);
  for (const d of handout.days) {
    out.push("", formatDay(d.date).toUpperCase());
    for (const e of d.entries) {
      if (e.kind === "meet") {
        out.push(`MEET: ${e.title}`);
        if (e.info) out.push(e.info);
        continue;
      }
      out.push([e.title, e.time, e.place].filter(Boolean).join(" · "));
      for (const b of e.blocks) {
        const head = `- ${b.title}${b.minutes ? ` (${b.minutes} min)` : ""}`;
        out.push(details && b.details ? `${head}: ${b.details.replace(/\s*\n\s*/g, " ")}` : head);
      }
    }
  }
  return out.join("\n");
}
