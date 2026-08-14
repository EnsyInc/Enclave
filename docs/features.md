# Feature List

Ground truth for the **Licensing** section below is the actual code (`src/EnsyInc.Enclave/EnsyInc.Enclave.Api/Controllers/`); the **Monitoring** section is requirements-only — no Monitoring code exists yet. See [00_overview.md](./00_overview.md) for platform context, [licensing-service.md](./licensing-service.md) / [monitoring-service.md](./monitoring-service.md) for full requirements, and [models.md](./models.md) for entity/state diagrams.

## Licensing service — implemented

### Products (`/products`)

- `GET /products`, `GET /products/{id}` — list (optionally filtered) / get a product.
- `POST /products` — create a product.
- `PUT /products/{id}` — update a product's name/description.
- `POST /products/{id}/retire` — mark a product `Retired` (distinct from delete — status change, not removal).
- `DELETE /products/{id}` — soft-delete a product; idempotent.

### Orgs (`/orgs`)

- `GET /orgs`, `GET /orgs/{id}` — list (optionally filtered by name) / get an org.
- `POST /orgs` — create an org.
- `PUT /orgs/{id}` — update an org's name.
- `POST /orgs/{id}/deactivate` — deactivate an org, **overriding its computed status** until reactivated.
- `POST /orgs/{id}/reactivate` — reactivate a deactivated org, **falling back to its computed status**.
- `DELETE /orgs/{id}` — soft-delete an org; idempotent.

### Users (`/orgs/{orgId}/users`)

- `GET /orgs/{orgId}/users`, `GET /orgs/{orgId}/users/{id}` — list / get a user within an org.
- `POST /orgs/{orgId}/users` — invite a single user.
- `POST /orgs/{orgId}/users/batch` — invite multiple users at once.
- `PUT /orgs/{orgId}/users/{id}` — update a user.
- `POST /orgs/{orgId}/users/{id}/deactivate` / `.../reactivate` — toggle a user's lifecycle status.
- `DELETE /orgs/{orgId}/users/{id}` — soft-delete a user; idempotent.

### Licenses (`/licenses`)

- `GET /licenses`, `GET /licenses/{id}` — list (filterable by org/product/status) / get a license.
- `POST /licenses` — grant a new license (org + product + start/end dates). Conflicts (`409`) if the org already holds an active license for that product; one active license per (org, product) is enforced by a unique index.
- `PUT /licenses/{id}` — update a license's start/end dates directly (admin correction, outside the request-review flow).
- `POST /licenses/{id}/suspend` — temporarily invalidate a license.
- `POST /licenses/{id}/revoke` — permanently invalidate a license.
- `DELETE /licenses/{id}` — soft-delete a license; idempotent.

### License requests (`/license-requests`)

- `GET /license-requests`, `GET /license-requests/{id}` — list (filterable by org/product/status) / get a request.
- `POST /license-requests/{id}/approve` — approve a request. For a **new-license** request, both start and end dates are set from the request body; for a **renewal** (an `ExistingLicenseId` is present), only the end date is applied — the existing license's start date is untouched. Fails `409` if the request isn't `Pending`, or if the org already has a conflicting active license.
- `POST /license-requests/{id}/reject` — reject a request with an optional reason. Fails `409` if the request isn't `Pending`.
- **No `POST /license-requests`** — creating/submitting a request is a customer-facing action that isn't built yet; this controller is admin-review-only (list, approve, reject).

## Licensing service — business rules & state machines

- **Org.Status** is computed from the org's licenses in the general case, but `Deactivated` is a manual admin override that takes precedence; reactivating drops back to the computed value. (The full computed vocabulary from the requirements — `Unlicensed`, `License Near Expiry` — isn't a literal stored enum value yet; today's `OrgStatus` enum is just `Active` / `Deactivated`, see [models.md](./models.md).)
- **License uniqueness**: a unique index on `(OrgId, ProductId)` (filtered to non-deleted rows) enforces at most one active license per org/product pair.
- **License Request type inference**: there's no stored "type" field — `ExistingLicenseId` present means renewal, absent means new-license.
- **License Request review is terminal**: approve/reject only succeed while `Status == Pending`; both are one-way transitions enforced by the service layer (`409 Conflict` otherwise).
- **Deletes are soft and idempotent**: every `DELETE` endpoint sets `DeletedAt` rather than removing the row, and succeeds even if the entity is already gone.
- User invite/lifecycle: `InviteSent → Active → Deactivated` (see [models.md](./models.md) for the full state diagram); the "at least one Admin per org" rule from the requirements is not yet visible as an enforced constraint in the controllers above.

## Monitoring service — designed, not implemented

No Monitoring code exists in this repo yet (no controllers, models, or EF entities). These are requirements only, from [monitoring-service.md](./monitoring-service.md):

- **Admin**: view live per-product status (health-check signal + internal-only smoke-test signal); create/update/resolve incidents with severity and a timeline of updates; configure each product's health-check endpoint.
- **Public status page** (no login): overall per-product status (`Operational` / `Degraded` / `Partial Outage` / `Major Outage`); current and past incidents with timelines; rounded historical uptime metrics.
- **Customer backoffice** (authenticated, scoped to the org's licensed products): same data as the public page, plus incident root-cause/internal notes and precise (non-rounded) downtime metrics.
- **Status derivation rule**: the health check sets the floor (up → at least `Operational`, down → `Major Outage`); an open incident's severity can refine the badge further; smoke-test results are an internal-only signal that feeds the admin's decision to open an incident but never touch the public badge directly.

## Explicitly deferred (v1)

- License validation/enforcement mechanism (how a licensed product proves it's licensed at runtime).
- Multiple license types (seat-based, tier-based, usage-based) — v1 ships a flat org↔product↔date-range record only.
- Usage metering (relevant once usage-based licenses exist).
- Multi-instance/per-deployment licenses and feature/tier-based gating.
- Incident notifications (email/webhook subscriptions) — status page is check-it-yourself in v1.
- Region-aware modeling inside a single Monitoring service instance — handled operationally via one Monitoring instance per region instead, aggregated at the frontend.
- Roles/permissions system for the admin backoffice (single admin user for now).
