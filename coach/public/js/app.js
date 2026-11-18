// Shared bits for every page: API helper and the snapshot freshness line in the footer.
export async function api(path, options = {}) {
  const res = await fetch(`/api${path}`, {
    ...options,
    headers: { "Content-Type": "application/json", ...(options.headers ?? {}) }
  });
  if (res.status === 204) return null;
  const body = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(body.error ?? `Request failed (${res.status})`);
  return body;
}

/** Replace a node's children, skipping null/false and flattening arrays (plain replaceChildren prints "null"). */
export function fill(node, ...children) {
  node.replaceChildren(...children.flat(Infinity).filter((c) => c != null && c !== false));
  return node;
}

export function el(tag, attrs = {}, ...children) {
  const node = document.createElement(tag);
  for (const [k, v] of Object.entries(attrs)) {
    if (v == null || v === false) continue;
    if (k.startsWith("on")) node.addEventListener(k.slice(2), v);
    else if (k === "class") node.className = v;
    else node.setAttribute(k, v === true ? "" : v);
  }
  for (const c of children.flat(Infinity)) if (c != null && c !== false) node.append(c);
  return node;
}

const NAV = [
  { id: "practice", href: "/practice", label: "Practice" },
  { id: "templates", href: "/templates", label: "Templates" },
  { id: "season", href: "/season", label: "Season plan" },
  { id: "pace", href: "/pace", label: "Goal pace" }
];

/** Fills <header class="site-header" data-page="..."> so every page shares one nav. */
function renderHeader() {
  const header = document.querySelector("header.site-header");
  if (!header) return;
  const page = header.dataset.page;
  header.replaceChildren(el("div", { class: "inner" },
    el("a", { class: "brand", href: "/" }, "Cloverleaf ", el("span", {}, "Coach")),
    el("nav", { class: "site-nav", "aria-label": "Tools" },
      NAV.map((n) => el("a", { href: n.href, "aria-current": n.id === page ? "page" : null }, n.label)))));
}

/** Warn before leaving a page with unsaved edits. Call markDirty(true/false). */
let dirty = false;
export function markDirty(v = true) { dirty = v; }
window.addEventListener("beforeunload", (e) => { if (dirty) { e.preventDefault(); e.returnValue = ""; } });

export const params = new URLSearchParams(location.search);

/** Group toggle chips. selected is a Set; onChange gets the new Set. */
export function groupPicker(groups, selected, onChange, label = "Event groups") {
  return el("div", { class: "chips", role: "group", "aria-label": label },
    groups.map((g) => el("button", {
      type: "button", class: "chip toggle", "aria-pressed": String(selected.has(g.id)),
      onclick: () => {
        const next = new Set(selected);
        next.has(g.id) ? next.delete(g.id) : next.add(g.id);
        onChange(next);
      }
    }, g.label)));
}

async function showSnapshot() {
  const target = document.getElementById("snapshot-status");
  if (!target) return;
  try {
    const meta = await api("/snapshot");
    target.textContent = meta.generated_at
      ? `Results data from cloverleaftrack.com as of ${new Date(meta.generated_at).toLocaleString()}.`
      : "No results data loaded yet.";
  } catch {
    target.textContent = "Couldn't check the results data.";
  }
}
renderHeader();
showSnapshot();
