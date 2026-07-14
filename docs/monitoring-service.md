# Monitoring Service — Requirements

See [overview.md](./00_overview.md) for how this service relates to Licensing.

## Entities

### Product health target
One health-check endpoint per Product for v1.

Future: a product may have multiple region deployments; each region runs its own Monitoring service instance, and the frontend aggregates across regions rather than this service modeling regions itself.

### Health check result
Polling result (up/down) + timestamp, from this service calling each product's health endpoint on an interval.

### Smoke test result
Ingested from a separate smoke-test runner service/deployment, which reports `ok` / `degraded` / `down` based on synthetic performance tests. Integration mechanism (push API vs. webhook) is TBD at implementation time.

### Incident
Manually created by the admin — either when automated checks don't catch a problem, or to annotate one that did.
- Title
- Description
- Affected product(s)
- Severity/impact: `Minor` / `Major` / `Critical`
- Status: `Investigating` / `Identified` / `Monitoring` / `Resolved` (standard status-page workflow)
- Timeline of updates
- Opened / resolved timestamps

### Uptime history
Historical uptime % and status-over-time per product, derived from health check + smoke test results, retained for display (e.g. "99.95% last 90 days").

## Status derivation (how the badge is computed)

- **Health check is authoritative for the base signal**: up → product is at least `Operational`; down → `Major Outage`.
- **Smoke test results are an internal-only admin signal** — they do not directly change the public/customer badge. A degraded smoke test result is something the admin sees and can act on by manually opening an Incident against the product.
- **An open incident's severity refines the badge** — e.g. health check up + an open `Critical` incident → shown as `Major Outage`/`Partial Outage` per the incident, not silently `Operational`. Exact severity→badge mapping is finalized at implementation time; the principle is that liveness sets the floor, and incidents (informed by smoke tests) are what actually move the needle on degraded states.

## Admin features

- View live status of all products (current health signal + smoke test signal as a separate internal-only indicator)
- Create / update / resolve incidents, with severity and timeline updates
- Configure per-product health check endpoint

## Public status page (no login required)

- Overall status per product: `Operational` / `Degraded` / `Partial Outage` / `Major Outage`, derived per "Status derivation" above
- Current and past incidents with timelines
- Historical uptime metrics/graphs (rounded/aggregate, e.g. "99.95% last 90 days")

## Customer backoffice (authenticated) status view

- Same status/incident data as the public page, scoped to products the org is licensed for, plus:
  - **Root-cause / internal notes** on incidents (detail not shown publicly)
  - **Precise downtime metrics** — exact timestamps/duration of downtime for their licensed products, rather than just the public's rounded uptime %

## Explicitly deferred / out of scope for v1

- **Incident notifications** (email/webhook) — v1 is status-page-only; customers check manually. Can be added later as a per-product subscription feature.
- **Region-aware modeling** within a single Monitoring service instance — handled operationally via per-region deployments instead, aggregated at the frontend.
