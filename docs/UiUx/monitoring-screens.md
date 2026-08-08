# Monitoring Service — Screens & Flows (spec for UI/UX handoff)

See [monitoring-service.md](../monitoring-service.md) for the underlying entities, feature list, and status-derivation rules this spec is built on.

**Scope note**: this document specifies *what* screens exist, what data/actions each needs, and the flows between them. It intentionally does not prescribe layout, visual design, or component choices — that's for the UI/UX designer to own.

---

## Admin backoffice

### Screen inventory
| Screen | Purpose |
|---|---|
| Status Dashboard | At-a-glance live status of every product |
| Product Monitoring Config | Configure the health-check endpoint per product |
| Incidents list | Browse/filter all incidents |
| Incident detail | View and update a single incident |
| Create/Update Incident form | Open a new incident or add a timeline update |

### Per-screen detail

**Status Dashboard** — Data per product: current health check signal (up/down), current smoke test signal (ok/degraded/down — internal-only, not shown to customers), computed public badge (see [status derivation](../monitoring-service.md#status-derivation-how-the-badge-is-computed)), count of open incidents. Action: "Open Incident" shortcut per row.

**Product Monitoring Config** — Fields: health check endpoint URL, poll interval. Action: save/update.

**Incidents list** — Data: affected product(s), title, severity, status, opened/resolved timestamps. Filters: product, status, severity. Action: open detail, create new.

**Incident detail** — Data: title, description, affected product(s), severity, status, timeline of updates, root-cause/internal notes, opened/resolved timestamps. Actions: add timeline update, edit severity/status, resolve.

**Create/Update Incident form** — Fields: affected product(s) (multi-select), title, description, severity (Minor/Major/Critical), status (Investigating/Identified/Monitoring/Resolved), optional root-cause/internal notes. Timeline updates are appended entries over the incident's life, not a one-time field.

### Key flows
- **M1 — React to automated detection**: Status Dashboard shows a product down (health check) or degraded (smoke test, internal-only) → admin opens an Incident from that row, pre-filled with the affected product.
- **M2 — Manually report an issue automation missed**: Incidents list → Create Incident → select affected product(s), set severity/status, describe → save.
- **M3 — Update an ongoing incident**: Incident detail → add timeline update (status change, new info) → eventually mark Resolved.

---

## Public status page (no login required)

### Screen inventory
| Screen | Purpose |
|---|---|
| Status Overview | Public landing page: current status of every product |
| Incident History | Public list of past incidents |
| Product Uptime detail | Aggregate uptime history for one product |

### Per-screen detail

**Status Overview** — Data: per-product badge (Operational/Degraded/Partial Outage/Major Outage), derived per [status derivation](../monitoring-service.md#status-derivation-how-the-badge-is-computed). A system-wide banner appears if any product has an active Major/Critical incident.

**Incident History** — Data: past incidents (title, severity, affected product(s), opened/resolved timestamps, public-facing timeline entries only — no root-cause/internal notes).

**Product Uptime detail** — Data: rounded/aggregate uptime % over time per product (e.g. "99.95% last 90 days"), status-over-time graph.

### Key flows
- **M4 — Public visitor checks overall health**: Status Overview → see all products' current badges → optionally drill into Incident History or a product's Uptime detail.

---

## Customer backoffice (authenticated status view)

### Screen inventory
| Screen | Purpose |
|---|---|
| My Services Status | Status overview scoped to the org's licensed products |
| Incident detail (customer) | Same as public incident view, plus root-cause/internal notes |
| Precise Uptime detail | Exact downtime timestamps/durations, not just rounded % |

### Per-screen detail

**My Services Status** — Same as the public Status Overview, but scoped only to products the org holds a license for.

**Incident detail (customer)** — Same fields as the public incident view, plus **root-cause/internal notes** (not shown publicly).

**Precise Uptime detail** — Exact timestamps and durations of downtime for the org's licensed products, versus the public page's rounded aggregate %.

### Key flows
- **M5 — Customer checks their services' status**: My Services Status → scoped badges for licensed products only.
- **M6 — Customer drills into an incident**: Incident detail (customer) → sees root-cause notes and precise downtime figures not available on the public page.

---

## Cross-cutting business rules

- **Status badge derivation** is fully specified in [monitoring-service.md § Status derivation](../monitoring-service.md#status-derivation-how-the-badge-is-computed) — not duplicated here. Summary for UI purposes: health check sets the floor (up/down), smoke test is admin-only and never shown as the badge itself, and an open incident's severity is what actually produces a Degraded/Partial Outage/Major Outage badge.
- **No incident notifications in v1** — customers must visit the page themselves; no email/webhook screens are in scope for this round.
- **Region aggregation** — if/when multi-region monitoring instances exist, this frontend aggregates across them; no per-region screens exist in this spec.
