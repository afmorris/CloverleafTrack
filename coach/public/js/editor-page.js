// Editor for a practice plan (body[data-mode=plan], /practice/edit?id= | ?date=&template=) or a practice template
// (body[data-mode=template], /templates/edit?id= | ?copy=). Both are a list of blocks; plans also have a date,
// notes, and recorded marks. BRAIN.md [C134].
import { api, el, groupPicker, markDirty, params, fill } from "./app.js";
import { formatDay, isIsoDate, today, weekStart } from "./dates.js";
import { formatTime } from "./pace.js";
import {
  BLOCK_KINDS, GROUPS, blocksFromTemplate, formatMinutes, groupLabel, kindLabel, mergeTemplateBlocks, moveBlock, newBlock,
  practiceLength
} from "./practice.js";

const isPlan = document.body.dataset.mode === "plan";
const root = document.getElementById("editor");
const s = {
  id: null, date: isIsoDate(params.get("date")) ? params.get("date") : today(), title: "", description: "", notes: "",
  groups: new Set(), blocks: [], builtIn: false
};
let templates = [], workouts = [], athletes = [];
let status = "";

const change = () => markDirty(true);
const setStatus = (text, isError = false) => {
  status = text;
  const node = document.getElementById("status");
  if (node) { node.textContent = text; node.className = `status${isError ? " error" : ""}`; }
};

// ---- Loading ---------------------------------------------------------------------------------------------------

async function init() {
  try {
    const [t, w, a] = await Promise.all([
      isPlan ? api("/templates") : Promise.resolve([]), api("/workouts"), isPlan ? api("/athletes") : Promise.resolve([])]);
    templates = t; workouts = w; athletes = a;

    if (params.get("id")) {
      const x = await api(`${isPlan ? "/practices" : "/templates"}/${encodeURIComponent(params.get("id"))}`);
      Object.assign(s, { id: x.id, title: isPlan ? x.title : x.name, description: x.description ?? "", notes: x.notes ?? "",
        groups: new Set(x.groups), blocks: x.blocks, builtIn: !!x.builtIn });
      if (isPlan) s.date = x.date;
    } else if (!isPlan && params.get("copy")) {
      const x = await api(`/templates/${encodeURIComponent(params.get("copy"))}`);
      Object.assign(s, { title: `Copy of ${x.name}`, description: x.description ?? "", groups: new Set(x.groups), blocks: x.blocks });
    } else if (isPlan && params.get("template")) {
      const x = templates.find((t) => String(t.id) === params.get("template"));
      if (x) Object.assign(s, { title: x.name, groups: new Set(x.groups), blocks: blocksFromTemplate(x.blocks, x.groups) });
    }
  } catch (e) {
    return fill(root, el("p", { class: "error" }, e.message));
  }
  render();
}

// ---- Rendering ---------------------------------------------------------------------------------------------------

function field(label, input) {
  const id = input.id || `f-${Math.random().toString(36).slice(2)}`;
  input.id = id;
  return el("div", {}, el("label", { for: id }, label), input);
}

function input(attrs, onInput) {
  return el("input", { ...attrs, oninput: (e) => { onInput(e.target.value); change(); } });
}

function workoutSummary(w) {
  return `${w.title}: ${w.eventName ?? `${w.eventDistanceM}m`} goal ${formatTime(w.goalSeconds)}, `
    + w.sets.map((x) => `${x.reps}×${x.distance} @ ${x.effort}%`).join(", ");
}

function athleteSelect(value, onChange) {
  const sel = el("select", { onchange: (e) => { onChange(e.target.value ? Number(e.target.value) : null); change(); } },
    el("option", { value: "" }, "Athlete…"));
  for (const [gender, label] of [[2, "Girls"], [1, "Boys"]]) {
    const group = el("optgroup", { label });
    for (const a of athletes.filter((x) => x.gender === gender)) {
      group.append(el("option", { value: a.id, selected: a.id === value }, `${a.lastName}, ${a.firstName}`));
    }
    sel.append(group);
  }
  return sel;
}

function renderBlock(b, i) {
  const update = (k) => (v) => { b[k] = v; };
  const kind = el("select", { onchange: (e) => { b.kind = e.target.value; change(); render(); } },
    BLOCK_KINDS.map((k) => el("option", { value: k.id, selected: k.id === b.kind }, k.label)));
  const minutes = input({ type: "number", min: 0, max: 240, value: b.minutes ?? 0, inputmode: "numeric" },
    (v) => { b.minutes = Number(v) || 0; updateTotal(); });
  const head = el("div", { class: "block-head" },
    field("Kind", kind),
    el("div", { class: "title" }, field("Title", input({ value: b.title, maxlength: 120, placeholder: kindLabel(b.kind) }, update("title")))),
    field("Minutes", minutes),
    el("div", { class: "move" },
      el("button", { type: "button", class: "secondary", "aria-label": `Move block ${i + 1} up`, disabled: i === 0,
        onclick: () => { s.blocks = moveBlock(s.blocks, i, -1); change(); render(); } }, "↑"),
      el("button", { type: "button", class: "secondary", "aria-label": `Move block ${i + 1} down`, disabled: i === s.blocks.length - 1,
        onclick: () => { s.blocks = moveBlock(s.blocks, i, 1); change(); render(); } }, "↓"),
      el("button", { type: "button", class: "link", "aria-label": `Remove block ${i + 1}`,
        onclick: () => { s.blocks.splice(i, 1); change(); render(); } }, "Remove")));

  const groupsBox = el("div", { class: "sub" });
  const drawGroups = () => fill(groupsBox, groupPicker(GROUPS.filter((g) => s.groups.has(g.id) || b.groups.includes(g.id)),
    new Set(b.groups), (next) => { b.groups = GROUPS.map((g) => g.id).filter((g) => next.has(g)); change(); drawGroups(); },
    `Groups for block ${i + 1}`));
  drawGroups();

  const details = el("textarea", { maxlength: 2000, placeholder: "Reps, distances, cues…",
    oninput: (e) => { b.details = e.target.value; change(); } }, b.details ?? "");

  const extra = [];
  if (b.kind === "workout") {
    const linked = workouts.find((w) => w.id === b.workoutId);
    extra.push(el("div", { class: "sub" }, field("Goal-pace workout (optional)",
      el("select", { onchange: (e) => { b.workoutId = e.target.value ? Number(e.target.value) : null; change(); render(); } },
        el("option", { value: "" }, workouts.length ? "None" : "No saved workouts yet"),
        workouts.map((w) => el("option", { value: w.id, selected: w.id === b.workoutId }, w.title)))),
      linked ? el("div", { class: "linked-workout" }, workoutSummary(linked), " · ",
        el("a", { href: `/pace?workout=${linked.id}` }, "open targets")) : null));
  }
  if (b.kind === "measured") {
    const rows = el("div", {});
    const drawRows = () => fill(rows, ...(b.results ?? []).map((r, j) => el("div", { class: "results-row" },
      athleteSelect(r.athleteId, (v) => { r.athleteId = v; }),
      el("input", { value: r.mark ?? "", maxlength: 40, placeholder: "Mark", "aria-label": "Mark",
        oninput: (e) => { r.mark = e.target.value; change(); } }),
      el("button", { type: "button", class: "link", onclick: () => { b.results.splice(j, 1); change(); drawRows(); } }, "Remove"))));
    drawRows();
    extra.push(el("div", { class: "sub grid" },
      field("Attempts each", input({ type: "number", min: 1, max: 20, value: b.attempts ?? "", inputmode: "numeric" },
        (v) => { b.attempts = v ? Number(v) : null; }))));
    if (isPlan) {
      extra.push(el("div", { class: "sub" }, el("label", {}, "Best marks"), rows,
        el("button", { type: "button", class: "secondary", onclick: () => {
          (b.results ??= []).push({ athleteId: null, mark: "" }); change(); drawRows();
        } }, "Add a mark")));
    }
  }

  return el("div", { class: "block" }, head, groupsBox, el("div", { class: "sub" }, details), extra);
}

function updateTotal() {
  const t = document.getElementById("total");
  if (t) t.textContent = `Practice length ${formatMinutes(practiceLength(s.blocks))}`;
  if (t) t.title = "The longest event group's time; groups train side by side.";
}

function render() {
  const groupsBox = el("div", {});
  const drawGroups = () => fill(groupsBox, groupPicker(GROUPS, s.groups, (next) => {
    s.groups = next; change(); drawGroups();
  }));
  drawGroups();

  const readOnly = !isPlan && s.builtIn;
  const heading = isPlan
    ? (s.id ? "Edit practice plan" : "New practice plan")
    : (readOnly ? "Built-in template" : s.id ? "Edit template" : "New template");

  const details = el("section", { class: "card" },
    el("div", { class: "grid" },
      isPlan ? field("Date", input({ type: "date", value: s.date, required: true }, (v) => { s.date = v; })) : null,
      field(isPlan ? "Title" : "Name", input({ value: s.title, maxlength: 120, required: true,
        placeholder: isPlan ? "e.g. Tuesday: speed + throws" : "e.g. 400 group speed endurance" }, (v) => { s.title = v; }))),
    el("div", { class: "sub" }, el("label", {}, "Event groups"), groupsBox),
    el("div", { class: "sub" }, field(isPlan ? "Notes" : "Description",
      el("textarea", { maxlength: isPlan ? 4000 : 500, oninput: (e) => { isPlan ? (s.notes = e.target.value) : (s.description = e.target.value); change(); } },
        isPlan ? s.notes : s.description))));

  const fromTemplate = isPlan ? el("section", { class: "card" },
    el("h2", {}, "Add blocks from a template"),
    el("div", { class: "row" },
      el("select", { id: "template-pick", "aria-label": "Template" },
        el("option", { value: "" }, "Choose a template…"),
        GROUPS.map((g) => el("optgroup", { label: g.label },
          templates.filter((t) => t.groups[0] === g.id).map((t) => el("option", { value: t.id }, t.name))))),
      el("button", { type: "button", class: "secondary", onclick: () => {
        const t = templates.find((x) => String(x.id) === document.getElementById("template-pick").value);
        if (!t) return;
        // The template's groups join the plan, so its blocks keep their groups.
        s.groups = new Set([...s.groups, ...t.groups]);
        if (!s.title) s.title = t.name;
        s.blocks = mergeTemplateBlocks(s.blocks, t.blocks, [...s.groups]);
        change(); render();
      } }, "Add blocks")),
    el("p", { class: "hint" }, "Adds the template's blocks and event groups. A warm-up or cool-down you already have is shared, not repeated.")) : null;

  const addKind = el("select", { "aria-label": "Kind of block to add" }, BLOCK_KINDS.map((k) => el("option", { value: k.id }, k.label)));
  const blocks = el("section", { class: "card" },
    el("div", { class: "page-head" }, el("h2", {}, "Blocks"), el("span", { id: "total", class: "status" })),
    s.blocks.length ? s.blocks.map(renderBlock) : el("p", { class: "empty" }, "No blocks yet."),
    el("div", { class: "row" }, addKind,
      el("button", { type: "button", class: "secondary", onclick: () => {
        s.blocks.push(newBlock(addKind.value, [...s.groups])); change(); render();
      } }, "Add block")));

  const actions = el("section", { class: "card" }, el("div", { class: "row" },
    readOnly ? null : el("button", { type: "button", onclick: save }, "Save"),
    isPlan && s.id ? el("a", { class: "button secondary", href: `/practice?week=${weekStart(s.date)}` }, "Back to week") : null,
    isPlan ? el("button", { type: "button", class: "secondary", onclick: () => { renderPrint(); window.print(); } }, "Print") : null,
    isPlan && s.id ? el("button", { type: "button", class: "secondary", onclick: saveAsTemplate }, "Save as template") : null,
    readOnly && s.id ? el("a", { class: "button", href: `/templates/edit?copy=${s.id}` }, "Make an editable copy") : null,
    !isPlan && s.id && !readOnly ? el("a", { class: "button secondary", href: `/practice/edit?template=${s.id}` }, "Use in a plan") : null,
    s.id && !readOnly ? el("button", { type: "button", class: "link", onclick: remove }, "Delete") : null,
    el("span", { id: "status", class: "status", role: "status" }, status)),
    isPlan && s.id ? el("div", { class: "row sub" },
      el("label", { for: "copy-date", style: "margin:0" }, "Copy to"),
      el("input", { type: "date", id: "copy-date", style: "width:auto" }),
      el("button", { type: "button", class: "secondary", onclick: copyTo }, "Copy plan")) : null);

  fill(root, el("h1", {}, heading),
    readOnly ? el("p", { class: "lede" }, "Built-in templates can't be changed. Use one in a plan, or make an editable copy.") : null,
    details, fromTemplate, blocks, actions);
  if (readOnly) root.querySelectorAll("input, textarea, select, .block button, .chip.toggle").forEach((n) => { n.disabled = true; });
  updateTotal();
}

/** Clean, read-only version of the plan for printing (the editor is hidden in print). */
function renderPrint() {
  const name = (id) => { const a = athletes.find((x) => x.id === id); return a ? `${a.firstName} ${a.lastName}` : ""; };
  fill(document.getElementById("print-view"),
    el("h1", {}, s.title), el("p", {}, `${formatDay(s.date)} · ${[...s.groups].map(groupLabel).join(", ")} · ${formatMinutes(practiceLength(s.blocks))}`),
    s.notes ? el("p", {}, s.notes) : null,
    el("table", {}, el("tbody", {}, s.blocks.map((b) => {
      const w = workouts.find((x) => x.id === b.workoutId);
      return el("tr", {},
        el("td", { class: "num" }, b.minutes ? `${b.minutes}m` : ""),
        el("td", {}, el("strong", {}, b.title || kindLabel(b.kind)), ` (${b.groups.map(groupLabel).join(", ")})`,
          b.details ? el("div", {}, b.details) : null,
          w ? el("div", {}, workoutSummary(w)) : null,
          b.attempts ? el("div", {}, `${b.attempts} attempts each`) : null,
          b.results?.length ? el("div", {}, b.results.filter((r) => r.mark).map((r) => `${name(r.athleteId)} ${r.mark}`).join(" · ")) : null));
    }))));
}
window.addEventListener("beforeprint", () => { if (isPlan) renderPrint(); });

// ---- Actions ---------------------------------------------------------------------------------------------------

function payload() {
  const blocks = s.blocks.map((b) => ({ ...b, results: (b.results ?? []).filter((r) => r.athleteId || r.mark) }));
  return isPlan
    ? { date: s.date, title: s.title, groups: [...s.groups], blocks, notes: s.notes }
    : { name: s.title, description: s.description, groups: [...s.groups], blocks };
}

async function save() {
  const base = isPlan ? "/practices" : "/templates";
  try {
    if (s.id) await api(`${base}/${s.id}`, { method: "PUT", body: JSON.stringify(payload()) });
    else {
      const r = await api(base, { method: "POST", body: JSON.stringify(payload()) });
      s.id = r.id;
      history.replaceState(null, "", `${isPlan ? "/practice/edit" : "/templates/edit"}?id=${r.id}`);
    }
    markDirty(false);
    status = "Saved.";
    render();
  } catch (e) {
    setStatus(e.message, true);
  }
}

async function saveAsTemplate() {
  const name = prompt("Template name", s.title);
  if (!name) return;
  try {
    const r = await api("/templates", { method: "POST", body: JSON.stringify({ name, description: s.notes, groups: [...s.groups], blocks: payload().blocks }) });
    setStatus("Saved as a template.");
    templates = await api("/templates");
    return r;
  } catch (e) { setStatus(e.message, true); }
}

async function copyTo() {
  const date = document.getElementById("copy-date").value;
  if (!isIsoDate(date)) return setStatus("Pick a date to copy to.", true);
  try {
    const r = await api(`/practices/${s.id}/copy`, { method: "POST", body: JSON.stringify({ date }) });
    setStatus("");
    fill(document.getElementById("status"), "Copied to ", el("a", { href: `/practice/edit?id=${r.id}` }, formatDay(date)), ".");
  } catch (e) { setStatus(e.message, true); }
}

async function remove() {
  if (!confirm(`Delete “${s.title}”? This can't be undone.`)) return;
  try {
    await api(`${isPlan ? "/practices" : "/templates"}/${s.id}`, { method: "DELETE" });
    markDirty(false);
    location.href = isPlan ? `/practice?week=${weekStart(s.date)}` : "/templates";
  } catch (e) { setStatus(e.message, true); }
}

init();
