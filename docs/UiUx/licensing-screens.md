# Licensing Service — Screens & Flows (spec for UI/UX handoff)

See [licensing-service.md](../licensing-service.md) for the underlying entities and feature list this spec is built on.

**Scope note**: this document specifies *what* screens exist, what data/actions each needs, and the flows between them. It intentionally does not prescribe layout, visual design, or component choices — that's for the UI/UX designer to own.

> **Note**: self-service user management (below) requires some users within an org to be able to manage others. This spec introduces a lightweight **Org Admin** vs **Org Member** distinction on the User entity to support that — not in the original requirements doc, added here and confirmed with the user. The first user added to an org (during admin onboarding) defaults to Org Admin.

---

## Common layout elements

These are persistent across every screen in a given backoffice, not per-screen — called out once here instead of repeated in every screen's description below.

### Header
Present on every page in both backoffices:
- Logo/brand mark
- Light/dark mode toggle (see [branding.md](./branding.md))
- Current identity — admin backoffice shows the internal admin user; customer backoffice shows the logged-in user's name and their org
- Logout action

### Sidebar (primary navigation)
Persistent left-hand navigation between the tabs/sections listed below.

- **Admin backoffice** tabs: Products, Orgs, Licenses, Renewal Requests. Renewal Requests should show a badge/counter of pending requests, since that's the one queue admin needs to keep an eye on.
- **Customer backoffice** tabs: My Licenses, My Renewal Requests, Org Users (visible only to Org Admins).

> **Cross-service note**: the admin and customer backoffices are single shell apps each spanning *both* Licensing and Monitoring, even though the two are separate backend services. In practice this sidebar sits alongside the Monitoring tabs from [monitoring-screens.md](./monitoring-screens.md) (e.g. admin gets a Status Dashboard/Incidents tab too; customer gets a My Services Status tab too) — this doc only lists the Licensing-specific entries to avoid duplicating monitoring-screens.md.

---

## Admin backoffice

### Screen inventory
| Screen | Purpose |
|---|---|
| Products list | Browse/manage the product catalog |
| Product form | Create or edit a product |
| Orgs list | Browse/manage customer organizations |
| Org detail | View an org's info, users, and licenses; entry point to grant a license |
| Licenses list (global) | Search/filter all licenses across every org and product |
| License detail | View a single license, its history, and take action on it |
| Grant License | Form to create a new license for an org |
| Renewal Requests queue | Triage pending renewal requests |
| Renewal Request detail | Approve or reject a specific request |

### Per-screen detail

**Products list** — Data: Name, Description, Status. Actions: Create Product, Edit, Retire. Empty state: "No products yet — create your first product."

**Product form** — Fields: Name (required, unique), Description, Status. Validation: name required.

**Orgs list** — Data: Org Name, Status, Primary Contact, count of active licenses. Actions: Create Org, open Org detail.

**Org detail** — Sections: org info (editable), Users (list; add first user during onboarding, remove/support-edit), Licenses (list scoped to this org). Actions: Edit org, Grant License (jumps to Grant License pre-filled with this org).

**Licenses list (global)** — Data: Org, Product, Seats, Expiry, Status. Filters: org, product, status, "expiring soon". Action: open License detail.

**License detail** — Data: org, product, seats, start/expiry dates, status, status-change history, link to any open Renewal Request. Actions: Suspend, Revoke, manually edit seats/expiry.

**Grant License** — Fields: Org (existing, searchable), Product, Seats (positive integer), Start Date, Expiry Date (must be after start date).

**Renewal Requests queue** — Data: Org, Product, License ref, Requested By, Requested On, Status (Pending/Approved/Rejected). Action: open detail.

**Renewal Request detail** — Data: requestor, license reference, current expiry, any note from customer. Actions: **Approve** (admin manually enters the new expiry date), **Reject** (optional reason shown back to customer).

### Key flows
- **A1 — Add a new product**: Products list → Product form (create) → save → appears in list.
- **A2 — Onboard a new org**: Orgs list → create org → Org detail → add first user → Grant License (from Org detail).
- **A3 — Grant an additional license**: Org detail (or global Licenses list) → Grant License → select product/seats/expiry → save.
- **A4 — Suspend/revoke a license**: License detail → Suspend or Revoke → confirm.
- **A5 — Action a renewal request**: Renewal Requests queue → open request → Approve (set new expiry) or Reject.

---

## Customer backoffice

### Screen inventory
| Screen | Purpose |
|---|---|
| My Licenses | View the org's licenses at a glance |
| License detail | View one license, request renewal if eligible |
| Request Renewal | Submit a renewal request |
| My Renewal Requests | Track status of submitted requests |
| Org Users | Self-service: invite/remove users within the org (Org Admin only) |

### Per-screen detail

**My Licenses** — Data: Product, Seats, Expiry, Status. Inline "Request Renewal" action appears only when the license is near/after expiry (proposed default: within 30 days of expiry, or already expired — exact threshold configurable).

**License detail** — Same fields as the list row, plus status history. Action: Request Renewal (same eligibility rule as above).

**Request Renewal** — Confirms which license, optional free-text note to the admin, submit.

**My Renewal Requests** — Data: Product, Submitted On, Status (Pending/Approved/Rejected), admin's response/reason if rejected.

**Org Users** *(Org Admin role only)* — Data: list of users in the org with role (Org Admin/Org Member). Actions: invite new user, remove a user, promote/demote Org Admin role. Regular Org Members do not see management actions.

### Key flows
- **C1 — Check license status**: My Licenses → scan expiry/status at a glance.
- **C2 — Request a renewal**: My Licenses (or License detail) → Request Renewal → submit → appears in My Renewal Requests as Pending.
- **C3 — Track a renewal request**: My Renewal Requests → see Approved/Rejected once admin acts.
- **C4 — Manage org users** *(Org Admin only)*: Org Users → Invite → new user gets access once provisioned.

---

## Cross-cutting business rules

- **Renewal eligibility**: "Request Renewal" is only available near expiry or after expiry — not for a license with plenty of time remaining. Window length is a configurable default, not hardcoded to 30 days.
- **Renewal approval**: always a manual admin action — the admin explicitly sets the new expiry date when approving; there is no auto-extend.
- **Suspend vs Revoke**: both are admin-only, immediate actions independent of the renewal flow (not something a customer can trigger or reverse).
- **Validation**: seats must be a positive integer; expiry date must be after start date; a new expiry set during renewal approval cannot be in the past.
- **Org Admin role**: needed to gate the self-service Org Users screen — at least one user per org must be an Org Admin (the first user added by internal admin during onboarding should default to Org Admin).
