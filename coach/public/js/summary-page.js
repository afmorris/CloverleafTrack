// Practice summary: a season plan's practices week by week, one line per practice (time, length, blocks), with
// phases and meets. /practice/summary?plan=N[&group=throws][&blocks=0]. Printable. BRAIN.md [C138].
import { api, el, params, fill } from "./app.js";
import { formatDay, formatRange, formatShort, today } from "./dates.js";
import { GROUPS, formatMinutes, groupLabel, planTime, practiceLength } from "./practice.js";
import { isChampionshipMeet, phaseKind, summarizeWeeks } from "./season.js";

const root = document.getElementById("content");
let plans = [], plan = null, practices = [], meets = [];
let group = params.get("group") ?? "";
let showBlocks = params.get("blocks") !== "0";

/** The season plan covering today, else the one starting next, else the newest. */
function defaultPlan(list) {
  const now = today();
  return list.find((p) => p.startDate <= now && now <= p.endDate)
    ?? [...list].filter((p) => p.startDate > now).sort((a, b) => a.startDate.localeCompare(b.startDate))[0]
    ?? list[0];
}

function setUrl() {
  const q = new URLSearchParams({ plan: plan.id });
  if (group) q.set("group", group);
  if (!showBlocks) q.set("blocks", "0");
  history.replaceState(null, "", `/practice/summary?${q}`);
}

async function load(id) {
  plan = plans.find((p) => String(p.id) === String(id)) ?? defaultPlan(plans);
  [practices, meets] = await Promise.all([
    api(`/practices?from=${plan.startDate}&to=${plan.endDate}`), api(`/meets?from=${plan.startDate}&to=${plan.endDate}`)]);
  setUrl();
  render();
}

const pill = (ph) => ph ? el("span", { class: "phase-pill", style: `--phase:${phaseKind(ph.kind).color}` }, ph.name) : null;
const meetChip = (m) => el("span", { class: `chip small meet${isChampionshipMeet(m.name) ? " champ" : ""}` }, m.name);

function weekSection(w) {
  const total = w.days.flatMap((d) => d.plans).reduce((s, p) => s + practiceLength(p.blocks), 0);
  const head = el("div", { class: "summary-week-head" },
    el("h2", {}, w.offseason ? w.label : `Week ${w.label}`),
    el("span", { class: "muted" }, formatRange(w.start, w.end)),
    pill(w.phase),
    el("span", { class: "muted" }, w.practiceCount ? `${w.practiceCount} ${w.practiceCount === 1 ? "plan" : "plans"} · ${formatMinutes(total)}` : "No plans"));
  if (!w.days.length) return el("section", { class: "summary-week empty-week" }, head);
  return el("section", { class: "summary-week" }, head,
    el("table", { class: "summary-table" },
      el("tbody", {}, w.days.flatMap((d) => {
        const rows = d.plans.map((p, i) => el("tr", {},
          el("td", { class: "day-cell" }, i === 0 ? formatDay(d.date) : ""),
          el("td", {}, el("a", { href: `/practice/edit?id=${p.id}` }, p.title),
            showBlocks && p.blocks.length ? el("div", { class: "blocks-line" },
              p.blocks.map((b) => `${b.title || b.kind}${b.minutes ? ` ${b.minutes}` : ""}`).join(" · ")) : null),
          el("td", { class: "time-cell" }, planTime(p.notes)),
          el("td", { class: "num" }, formatMinutes(practiceLength(p.blocks)))));
        const unplannedMeets = d.meets.filter((m) => !d.plans.some((p) => p.title.includes(m.name)));
        if (unplannedMeets.length) {
          rows.push(el("tr", {}, el("td", { class: "day-cell" }, d.plans.length ? "" : formatDay(d.date)),
            el("td", { colspan: 3 }, unplannedMeets.map(meetChip))));
        }
        return rows;
      }))));
}

function render() {
  const visible = group ? practices.filter((p) => p.groups.includes(group)) : practices;
  const weeks = summarizeWeeks(plan, visible, meets);
  const shown = visible.length;
  const minutes = visible.reduce((s, p) => s + practiceLength(p.blocks), 0);
  document.title = `Practice summary: ${plan.name}`;
  const picker = el("select", { id: "s-plan", style: "width:auto", onchange: (e) => load(e.target.value) },
    plans.map((p) => el("option", { value: p.id, selected: p.id === plan.id }, p.name)));
  const groupSel = el("select", { id: "s-group", style: "width:auto", onchange: (e) => { group = e.target.value; setUrl(); render(); } },
    el("option", { value: "" }, "All groups"),
    GROUPS.map((g) => el("option", { value: g.id, selected: g.id === group }, g.label)));
  const blocksBox = el("input", { id: "s-blocks", type: "checkbox", checked: showBlocks, style: "width:auto",
    onchange: (e) => { showBlocks = e.target.checked; setUrl(); render(); } });

  fill(root,
    el("div", { class: "page-head" }, el("h1", {}, `Practice summary: ${plan.name}`),
      el("a", { class: "no-print", href: `/season?id=${plan.id}` }, "Season plan")),
    el("p", { class: "lede" }, `${formatShort(plan.startDate)} – ${formatShort(plan.endDate)}`,
      plan.firstPractice ? ` · first practice ${formatShort(plan.firstPractice)}` : "",
      ` · ${shown} plans · ${formatMinutes(minutes)}${group ? ` (${groupLabel(group)})` : ""}`),
    el("div", { class: "toolbar no-print" },
      el("label", { class: "visually-hidden", for: "s-plan" }, "Season plan"), picker,
      el("label", { class: "visually-hidden", for: "s-group" }, "Event group"), groupSel,
      el("label", { for: "s-blocks", style: "margin:0;font-weight:500;display:flex;gap:.4rem;align-items:center" }, blocksBox, "Show blocks"),
      el("span", { class: "spacer" }),
      el("button", { type: "button", class: "secondary", onclick: () => window.print() }, "Print")),
    weeks.map(weekSection));
}

(async () => {
  plans = await api("/season-plans");
  if (!plans.length) {
    return fill(root, el("h1", {}, "Practice summary"),
      el("p", { class: "empty" }, "Make a season plan first: the summary follows its dates and phases. ", el("a", { href: "/season" }, "Season plans")));
  }
  await load(params.get("plan"));
})().catch((e) => fill(root, el("p", { class: "error" }, e.message)));
