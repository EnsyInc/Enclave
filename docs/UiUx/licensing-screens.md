# Licensing Service — Screens & Flows (spec for UI/UX handoff)

[Figma](https://www.figma.com/design/l6BSNZ2P2zn1ZPFevqbRio/Enclave-Design-System-v1?node-id=0-1&p=f&t=q4bX35ha4V7tcLn7-0)

See [licensing-service.md](../licensing-service.md) for the underlying entities and feature list this spec is built on.

**Scope note**: this document specifies *what* screens exist, what data/actions each needs, and the flows between them. It intentionally does not prescribe layout, visual design, or component choices — that's for the UI/UX designer to own.

> **Note**: org user management uses an **Admin** vs **Reader** role on the User entity — Admin can manage org info, licenses, and other users; Reader can view and submit requests but can't manage users. The first user invited to an org (during onboarding) defaults to Admin.

---

## Common layout elements

These are persistent across every screen in a given backoffice, not per-screen — called out once here instead of repeated in every screen's description below.

### Header
Present on every page in both backoffices:
- Logo/brand mark
- Light/dark mode toggle (see [branding.md](./branding.md))
- Current identity — a user icon + name; clicking reveals name, email, role, and a logout action

### Sidebar (primary navigation)
Persistent left-hand navigation between the tabs/sections listed below. Ideally collapsible — when collapsed, only icons show (plus the pending-request badge, see below).

- **Admin backoffice** tabs: Products, Organizations, Licenses, License Requests. License Requests shows a badge/counter of pending requests.
- **Customer backoffice** tabs: My Licenses, My License Requests, Users.

> **Cross-service note**: the admin and customer backoffices are single shell apps each spanning *both* Licensing and Monitoring, even though the two are separate backend services. In practice this sidebar sits alongside the Monitoring tabs from [monitoring-screens.md](./monitoring-screens.md) (e.g. admin gets a Status Dashboard/Incidents tab too; customer gets a My Services Status tab too) — this doc only lists the Licensing-specific entries to avoid duplicating monitoring-screens.md.

### Additional shared pages
- **Not Found** — shown for an unmatched route, and also shown instead of a dedicated "Forbidden" page when a logged-in user hits something they don't have access to, so unauthorized users can't tell whether a resource exists but is just blocked, or doesn't exist at all.

### Error toast
A toast notification pattern used platform-wide (not Licensing-specific) for surfacing errors: appears at the bottom of the page, has a depleting "loading bar" until it auto-dismisses, pauses the bar on hover, and has a manual close (×) button.

---

## Admin backoffice

### Screen inventory
| Screen | Purpose |
|---|---|
| Products list | Browse/manage the product catalog |
| Product form | Create or edit a product |
| Organizations list | Browse/manage customer organizations |
| Organization detail | View an org's info, users, and licenses; entry point to grant a license |
| Licenses list (global) | Search/filter all licenses across every org and product |
| License detail | View a single license, its history, and take action on it |
| Grant License | Form to create a new license for an org |
| License Requests queue | Triage pending new-license and renewal requests |
| License Request detail | Approve or reject a specific request |

### Per-screen detail

**Products list** — Data: Name, Status (`Active`/`Retired`). Actions: search bar, Create Product (above the table), Edit, Retire, Delete per row. Empty state: "No products yet — create your first product."

**Product form** — Fields: Name (required), Description. Opens as an overlay on the current page.

**Organizations list** — Data: Name, Status (`Active`/`Unlicensed`/`License Near Expiry`/`Deactivated` — computed from licenses, except Deactivated which is manual), Primary Contact (email). Actions: search bar, filter by status (more filter criteria TBD), Create Organization, click a row to open Organization detail.

**Organization detail** — Sections:
- **Info** — org overview fields; an Edit button toggles them into editable inputs and swaps itself for Cancel/Save. Also a Deactivate/Activate toggle button that manually sets the org's status to Deactivated, overriding whatever the computed status would otherwise show; toggling back to Activate falls back to the computed status.
- **Licenses** — list scoped to this org (product name, expiry). Edit-license action per row, plus a Grant License button.
- **Users** — list scoped to this org (name, email, role [Admin/Reader], status [Active/Deactivated/Invite Sent]). Edit, Deactivate, and Delete actions per row, plus an Invite User button.

Creating a new org: form fields are Name + Admin Email — this both creates the org and invites its first user (who defaults to the Admin role) in one step.

**Licenses list (global)** — Data: Org, Product, Status (`Active`/`Expired`/`Suspended`/`Revoked`), Expiry Date, Time Left Until Expiry. Filters: org, product, status. Sortable columns. Action: click a row to open License detail.

**License detail** — Data: product, org, start date, expiry date, status (`Active`/`Expired`/`Suspended`/`Revoked`), status-change history. Edit action makes only start/expiry dates editable. If a License Request is pending against this license, a page-header banner reads "Client has requested a license renewal" linking to that request, instead of a persistent section. Buttons above the sections: Suspend, Revoke.

**Grant License** — Fields: Product (dropdown), Start Date (date picker, defaults to today), Expiry Date (date picker, must be after start date). Opens as an overlay on the current page.

**License Requests queue** — Data: Org, Product, Existing License (link, present only for renewal requests), Requested By, Requested On, Status (Pending/Approved/Rejected). There's no separate "type" field for new vs. renewal — it's inferred from whether the Existing License link is present. Action: click a row to open detail.

**License Request detail** — Data: requested by, existing license link (if a renewal), notes from the client. Actions:
- **Approve** — for a **new-license** request (no existing license), opens two date pickers (start date, end date); for a **renewal** request (existing license present), opens a single date picker (new expiry date) — the license's start date doesn't change.
- **Reject** — opens an optional text box for a reason, shown back to the customer.

### Key flows
- **A1 — Add a new product**: Products list → Product form (create) → save → appears in list.
- **A2 — Onboard a new org**: Organizations list → Create Organization (name + admin email) → org created, first user invited as Admin → Organization detail → Grant License.
- **A3 — Grant an additional license**: Organization detail (or global Licenses list) → Grant License → select product/dates → save.
- **A4 — Suspend/revoke a license**: License detail → Suspend or Revoke → confirm.
- **A5 — Action a license request**: License Requests queue → open request → Approve (new: start+end dates; renewal: new expiry date only) or Reject (optional reason).

---

## Customer backoffice

### Screen inventory
| Screen | Purpose |
|---|---|
| My Licenses | View the org's licenses at a glance |
| License detail | View one license, request a new license or renewal if eligible |
| Request License / Request Renewal | Submit a new-license or renewal request |
| My License Requests | Track (and cancel) submitted requests |
| Users | Self-service: invite/manage users within the org (Admin role only) |

### Per-screen detail

**My Licenses** — Data: Product, Status, Expiry Date, Time Left Until Expiry. Inline "Request Renewal" action per row appears only when the license is near/after expiry (proposed default: within 30 days of expiry, or already expired — exact threshold configurable). A standalone "Request License" button (not tied to a specific row) covers requesting a product the org doesn't yet have.

**License detail** *(may work better as a modal, to avoid a deep page hierarchy for customers)* — Same fields as the My Licenses row, plus license history. Request Renewal action, same eligibility rule as above.

**Request License / Request Renewal** — Fields: Product (dropdown — prepopulated if opened via "Request Renewal" on a specific license), Notes (free text). Opens as an overlay.

**My License Requests** — Data: Product, Requested On, Requested By, Status, and (once resolved) rejection reason if rejected. Delete/cancel action per row — only shown while status is Pending. Click a row for a detail popup with the same fields.

**Users** *(Admin role only)* — Data: name, email, status (Active/Deactivated/Invite Sent), role (Admin/Reader). Actions: Invite User (list-style form — supports inviting multiple users at once, each row an email + role), Edit (name/role, and possibly email), Deactivate, Delete. Reader-role users do not see these management actions.

### Key flows
- **C1 — Check license status**: My Licenses → scan expiry/status at a glance.
- **C2 — Request a new license**: My Licenses → Request License → pick product, add notes → submit → appears in My License Requests as Pending.
- **C3 — Request a renewal**: My Licenses (or License detail) → Request Renewal (product prepopulated) → submit → appears in My License Requests as Pending.
- **C4 — Track or cancel a request**: My License Requests → see Approved/Rejected (with reason) once admin acts, or cancel while still Pending.
- **C5 — Manage org users** *(Admin role only)*: Users → Invite (single or batch) → new user(s) get access once provisioned; Edit/Deactivate/Delete as needed.

---

## Cross-cutting business rules

- **License Requests cover two cases**: a request for a product the org doesn't yet hold a license for, and a renewal request against an existing license. There's no separate stored "type" field — it's inferred from whether an existing-license link is present.
- **Renewal eligibility**: "Request Renewal" is only available near expiry or after expiry — not for a license with plenty of time remaining. Window length is a configurable default, not hardcoded to 30 days.
- **Approval is always manual, and differs by type**: new-license approval sets both start and end dates; renewal approval sets only a new expiry date (the existing license's start date doesn't change). There is no auto-extend either way.
- **Cancel/withdraw a request** is only available to the customer while the request is still Pending — not after it's been Approved or Rejected.
- **Suspend vs Revoke**: both are admin-only, immediate actions independent of the request flow (not something a customer can trigger or reverse).
- **Org status is computed**, not manually set — except Deactivated, a manual admin toggle that overrides the computed value; reactivating falls back to the computed status.
- **User lifecycle**: Invite Sent → Active once provisioned; Deactivated is reversible/soft, Delete is not. Both actions exist as distinct options, not a single "remove" — and both are available on the admin side (Organization detail → Users) as well as customer self-service.
- **Admin/Reader role**: needed to gate the self-service Users screen — at least one user per org must be Admin (the first user invited during onboarding defaults to Admin).
- **Validation**: expiry date must be after start date; a new expiry set during request approval cannot be in the past.
- **Deferred**: seat/tier/usage-based license caps are not part of v1 (see [licensing-service.md](../licensing-service.md)) — Grant License and License detail intentionally have no seat/quantity field for now.
