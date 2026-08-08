# Licensing Service — Requirements

See [overview.md](./00_overview.md) for how this service relates to Monitoring.

## Entities

### Product
A licensable product/service.
- Name
- Description
- Status: `Active` / `Retired` — set via the admin's Retire action, not a freely editable field.

Owned entirely by this service — the admin creates, edits, retires, and can permanently delete products (Delete is a separate, outright-removal action alongside the Retire status change).

### Org (customer)
A thin customer record — **not a CRM**.
- Name
- Status: `Active` / `Unlicensed` / `License Near Expiry` / `Deactivated` — **computed** from the org's licenses (an org with no active license is Unlicensed, one with a license expiring soon is License Near Expiry, etc.), except `Deactivated`, which is a manual admin action (a toggle) that overrides whatever the computed value would otherwise be. Reactivating falls back to the computed status.
- Primary contact (email)

Deliberately minimal. Designed so a future dedicated client-management app can become the richer source of truth for customer relationship data (billing, contracts, communication history), keyed by the same Org ID, without needing to migrate license data.

### User
Belongs to an Org. An org can have multiple users. Backed by Active Directory (auth design TBD — likely Entra ID/B2B).
- Name, Email
- Role: `Admin` / `Reader` — Admin can manage org info, licenses, and other users within the org; Reader can view licenses/status and submit license/renewal requests but can't manage users. At least one Admin per org (the first user invited during org onboarding defaults to Admin).
- Lifecycle status: `Active` / `Deactivated` / `Invite Sent`

Whether Email is editable after invite (vs. fixed by the AD-backed identity) is TBD.

### License
Links one Org to one Product.
- Start date / expiry date (time-bound; supports renewal)
- Status: `Active` / `Expired` / `Suspended` / `Revoked` — no separate "Pending Renewal" status; a pending request against a license is tracked on the License Request entity itself and surfaced via a banner/link, not by changing the license's own status.
- Flat Org↔Product relationship — no per-instance/deployment binding
- **No seat count, tier, or usage dimension in v1** — a license is just an org↔product↔date-range record. See "Future: multiple license types" below.
- Status-change history — an audit trail of status transitions (e.g. grant, Active → Suspended → Revoked, renewal-driven expiry changes), surfaced on both the admin and customer License detail views.

### License Request
Customer-initiated, admin-actioned. Covers **both** a request for a brand-new license (org doesn't have one for that product yet) and a renewal request for an existing license. There is no separate stored "type" field — whether a request is new-license or renewal is inferred from whether the org already holds a license for that product (existing license reference present = renewal; absent = new).
- Org, Product
- Existing license reference — present only when this is a renewal; absent for a new-license request
- Requested by (user)
- Notes (free text from the customer)
- Status: `Pending` / `Approved` / `Rejected`, with a rejection reason shown back to the customer if rejected
- Timestamps

Approval differs by type: a **new-license** request sets both a start date and an end date (nothing exists yet); a **renewal** request sets only a new end/expiry date — the existing license's start date doesn't change. Customers may cancel/withdraw their own pending request (only while it's still `Pending`).

## Admin backoffice features

- Product CRUD (create/edit/retire/delete products available for licensing)
- Org CRUD (thin customer records) + manage users under an org (invite — single or batch, edit, deactivate, delete)
- Grant a new license (select org + product + start/expiry dates)
- View/search all licenses (filter by org, product, status; sortable, with time-left-until-expiry visible)
- Suspend / revoke a license
- Review and approve/reject license requests (new-license or renewal)

## Customer backoffice features

- View own org's licenses: which products, expiry date, time left until expiry, status
- Submit a request for a new license (product they don't yet have) or a renewal of an existing one
- Cancel/withdraw their own pending request
- Self-service org user management (Admin role only): invite (single or batch), edit, deactivate, delete users within their org

## Explicitly deferred / out of scope for v1

- **License validation/enforcement mechanism** — how a licensed product actually checks it's licensed at runtime (online API call vs. signed offline token). This service only tracks license records for now; enforcement is a future decision.
- **Future: multiple license types** — seat-based, tier-based, and usage-based licenses are planned, added per-product as each product actually needs that dimension (e.g. one product might be seat-capped, another usage-metered). v1 ships with the simple flat license record only; this is deliberately deferred rather than dropped.
- **Usage metering** — products reporting real consumption back against a cap — relevant once usage-based licenses exist.
- **Multi-instance/per-deployment licenses**.
