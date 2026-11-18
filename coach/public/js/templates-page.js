// Template library: filter by event group, start a plan from a template, copy or edit custom ones.
import { api, el, groupPicker, fill } from "./app.js";
import { formatMinutes, GROUPS, groupLabel, practiceLength } from "./practice.js";

let templates = [];
let filter = new Set();

function render() {
  fill(document.getElementById("filters"),
    el("span", { class: "status" }, "Show:"),
    groupPicker(GROUPS, filter, (next) => { filter = next; render(); }, "Filter by event group"));
  const shown = templates.filter((t) => !filter.size || t.groups.some((g) => filter.has(g)));
  const list = document.getElementById("list");
  if (!shown.length) return fill(list, el("p", { class: "empty" }, "No templates for these groups."));
  fill(list, ...shown.map((t) => el("article", { class: "card" },
    el("span", { class: "badge" }, t.builtIn ? "Built-in" : `Custom · ${t.createdBy}`),
    el("h2", {}, t.name),
    t.description ? el("p", {}, t.description) : null,
    el("div", { class: "chips" }, t.groups.map((g) => el("span", { class: "chip small" }, groupLabel(g))),
      el("span", { class: "chip small" }, formatMinutes(practiceLength(t.blocks)))),
    el("ol", {}, t.blocks.map((b) => el("li", {}, b.title || b.kind, b.minutes ? ` (${b.minutes}m)` : ""))),
    el("div", { class: "row" },
      el("a", { class: "button", href: `/practice/edit?template=${t.id}` }, "Use in a plan"),
      el("a", { class: "button secondary", href: t.builtIn ? `/templates/edit?copy=${t.id}` : `/templates/edit?id=${t.id}` },
        t.builtIn ? "Copy" : "Edit")))));
}

api("/templates").then((t) => { templates = t; render(); })
  .catch((e) => fill(document.getElementById("list"), el("p", { class: "error" }, e.message)));
