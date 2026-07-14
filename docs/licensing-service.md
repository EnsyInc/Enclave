# Licensing Service — Requirements

See [overview.md](./00_overview.md) for how this service relates to Monitoring.

## Entities

### Product
A licensable product/service.
- Name
- Description
- Status (active / deprecated?)

Owned entirely by this service — the admin creates, edits, and retires products.

### Org (customer)
A thin customer record — **not a CRM**.
- Name
- Status
- Primary contact

Deliberately minimal. Designed so a future dedicated client-management app can become the richer source of truth for customer relationship data (billing, contracts, communication history), keyed by the same Org ID, without needing to migrate license data.

### User
Belongs to an Org. An org can have multiple users. Backed by Active Directory (auth design TBD — likely Entra ID/B2B).

### License
Links one Org to one Product.
- Start date / expiry date (time-bound; supports renewal)
- Seat count — a cap, **informational only for v1** (no usage metering or reporting from the product back to this service)
- Status: `Active` / `Expired` / `Suspended` / `Revoked` / `Pending Renewal` (proposed default set)
- Flat Org↔Product relationship — no per-instance/deployment binding
- No feature/tier gating in v1

### Renewal Request
Customer-initiated, admin-actioned.
- Requested by (user)
- License reference
- Status: `Pending` / `Approved` / `Rejected`
- Timestamps

## Admin backoffice features

- Product CRUD (create/edit/retire products available for licensing)
- Org CRUD (thin customer records) + manage users under an org
- Grant a new license (select org + product + seats + expiry)
- View/search all licenses (filter by org, product, status, expiring-soon)
- Suspend / revoke a license
- Review and approve/reject renewal requests

## Customer backoffice features

- View own org's licenses: which products, seat count, expiry date, status
- Submit a renewal request for an expiring/expired license
- User management under the org (admin-only vs. self-service) — deferred, not critical for v1

## Explicitly deferred / out of scope for v1

- **License validation/enforcement mechanism** — how a licensed product actually checks it's licensed at runtime (online API call vs. signed offline token). This service only tracks license records for now; enforcement is a future decision.
- **Usage metering** — products reporting real consumption back against seat caps.
- **Multi-instance/per-deployment licenses**.
- **Feature/tier-based gating**.
