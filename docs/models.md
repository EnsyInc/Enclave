# Domain Models

Diagrams for the platform's data model. The **Licensing** section is grounded in the actual code (`EnsyInc.Enclave.Core/Models/*.cs`, `EnsyInc.Enclave.DataAccess.EF/Configuration/*Config.cs`) — field names, types, and relationships are copied from there, not the requirements docs, so they reflect what's actually built. The **Monitoring** section is design-only (from [monitoring-service.md](./monitoring-service.md)); no Monitoring code exists in this repo. See [features.md](./features.md) for the matching feature/endpoint inventory.

All Licensing entities inherit the same base fields (`Id: Guid` PK, `CreatedAt`, `UpdatedAt?`, `DeletedAt?`) and use soft delete (`DeletedAt` set, filtered out of unique indexes) rather than hard row removal.

## Licensing domain model (implemented)

### Entity relationships

```mermaid
%%{init: {'theme': 'dark'}}%%
erDiagram
    ORG ||--o{ USER : "has"
    ORG |o--o{ USER : "primary contact (optional, unenforced 1:1)"
    ORG ||--o{ LICENSE : "holds"
    PRODUCT ||--o{ LICENSE : "granted as"
    ORG ||--o{ LICENSE_REQUEST : "submitted for"
    PRODUCT ||--o{ LICENSE_REQUEST : "requested for"
    USER ||--o{ LICENSE_REQUEST : "requested by"
    LICENSE |o--o{ LICENSE_REQUEST : "renews (optional)"

    ORG {
        guid Id PK
        string Name
        string Status "Active | Deactivated"
        guid PrimaryUserId FK "nullable"
    }
    USER {
        guid Id PK
        string Name
        string Email "unique among non-deleted rows"
        guid OrgId FK
        string Status "InviteSent | Active | Deactivated"
        string Role "Reader | Admin"
    }
    PRODUCT {
        guid Id PK
        string Name "unique among non-deleted rows"
        string Description "nullable"
        string Status "Active | Retired | Upcoming"
    }
    LICENSE {
        guid Id PK
        guid OrgId FK
        guid ProductId FK
        datetime Start
        datetime End
        string Status "Scheduled | Active | Expired | Suspended | Revoked"
    }
    LICENSE_REQUEST {
        guid Id PK
        guid OrgId FK
        guid ProductId FK
        guid UserId FK "requested by"
        guid ExistingLicenseId FK "nullable, renewal only"
        string RequestNotes "nullable"
        string RejectionReason "nullable"
        string Status "Pending | Approved | Rejected"
    }
```

Notes on relationships that don't map cleanly to plain FK cardinality:

- **Org ↔ User is bidirectional-ish but only one direction is a real FK.** `User.OrgId` is a required FK to `Org` (an org has many users). `Org.PrimaryUserId` is a separate, optional FK to `User` — nothing in the schema enforces that the primary-contact user actually belongs to that same org, or that a user is primary contact for only one org.
- **License ↔ LicenseRequest has no stored discriminator.** Whether a request is "new" or "a renewal" is inferred purely from whether `ExistingLicenseId` is null, not a stored `Type` field.
- **One active license per (org, product)** is enforced by a unique index on `(OrgId, ProductId)` filtered to non-deleted rows — not visible in a plain ER diagram, called out here instead.
- All FK relationships use `DeleteBehavior.Restrict` (no cascade deletes) and are unidirectional in EF (no inverse navigation collections configured on the "one" side).

### Status state machines

`UserRole` (`Reader` / `Admin`) is a flat classification, not a state machine, so it's omitted below.

**Product.Status** — the only status *not* gated by dedicated endpoints: `POST`/`PUT` accept a `Status` value directly (`[JsonRequired] ProductStatus Status` on both create and update requests), so any transition is possible via a plain update. `POST /products/{id}/retire` exists as a convenience action on top of that.

```mermaid
%%{init: {'theme': 'dark'}}%%
stateDiagram-v2
    [*] --> Active : POST /products (Status=Active)
    [*] --> Upcoming : POST /products (Status=Upcoming)
    Upcoming --> Active : PUT /products/{id}
    Active --> Retired : POST /products/{id}/retire, or PUT
    Retired --> Active : PUT /products/{id}
```

**Org.Status** — gated: `Status` is not present on `CreateOrgRequest`/`UpdateOrgRequest`; the only mutators are the two action endpoints below.

```mermaid
%%{init: {'theme': 'dark'}}%%
stateDiagram-v2
    [*] --> Active : POST /orgs
    Active --> Deactivated : POST /orgs/{id}/deactivate
    Deactivated --> Active : POST /orgs/{id}/reactivate
```

**User.Status** — gated similarly (`Status` isn't on `InviteUserRequest`/`UpdateUserRequest`). Note: no code path was found that moves a user from `InviteSent` to `Active` (no "accept invite" endpoint exists yet) — likely a gap to close when the invite-acceptance flow is built.

```mermaid
%%{init: {'theme': 'dark'}}%%
stateDiagram-v2
    [*] --> InviteSent : POST /orgs/{orgId}/users
    InviteSent --> Active : not yet implemented
    Active --> Deactivated : POST .../users/{id}/deactivate
    Deactivated --> Active : POST .../users/{id}/reactivate
```

**License.Status** — fully gated by service logic, no direct status setter on any request DTO.

```mermaid
%%{init: {'theme': 'dark'}}%%
stateDiagram-v2
    [*] --> Scheduled : POST /licenses (Start is in the future)
    [*] --> Active : POST /licenses (Start has passed)
    Scheduled --> Active : not yet implemented (no scheduler)
    Active --> Suspended : POST /licenses/{id}/suspend
    Suspended --> Active : approving a renewal request
    Active --> Revoked : POST /licenses/{id}/revoke
    Suspended --> Revoked : POST /licenses/{id}/revoke
    Active --> Expired : not yet implemented (no scheduler)
```

`LicenseStatus.Expired` exists in the enum but nothing in the current code transitions a license into it — there's no background job checking `End` against the current date.

**LicenseRequest.Status** — one-way, terminal once reviewed (`409 Conflict` if re-approved/rejected).

```mermaid
%%{init: {'theme': 'dark'}}%%
stateDiagram-v2
    [*] --> Pending : (customer submission — not yet implemented)
    Pending --> Approved : POST /license-requests/{id}/approve
    Pending --> Rejected : POST /license-requests/{id}/reject
```

## Monitoring domain model (designed, not implemented)

Built from [monitoring-service.md](./monitoring-service.md) prose only — no Core models, EF entities, or controllers exist for any of this yet.

```mermaid
%%{init: {'theme': 'dark'}}%%
erDiagram
    PRODUCT ||--o| PRODUCT_HEALTH_TARGET : "has (1:1, v1)"
    PRODUCT_HEALTH_TARGET ||--o{ HEALTH_CHECK_RESULT : "produces"
    PRODUCT ||--o{ SMOKE_TEST_RESULT : "ingests"
    PRODUCT }o--o{ INCIDENT : "affects"
    PRODUCT ||--o{ UPTIME_HISTORY : "aggregates into"

    PRODUCT_HEALTH_TARGET {
        guid Id PK
        guid ProductId FK
        string Endpoint "health-check URL"
    }
    HEALTH_CHECK_RESULT {
        guid Id PK
        guid ProductHealthTargetId FK
        bool Up
        datetime Timestamp
    }
    SMOKE_TEST_RESULT {
        guid Id PK
        guid ProductId FK
        string Result "ok | degraded | down"
        datetime Timestamp
        string Note "internal-only signal, not the public badge"
    }
    INCIDENT {
        guid Id PK
        string Title
        string Description
        string Severity "Minor | Major | Critical"
        string Status "Investigating | Identified | Monitoring | Resolved"
        string RootCauseNotes "internal/customer-backoffice only, not public"
        datetime OpenedAt
        datetime ResolvedAt "nullable"
    }
    UPTIME_HISTORY {
        guid Id PK
        guid ProductId FK
        date Day
        float UptimePercent
    }
```

`PRODUCT }o--o{ INCIDENT` is many-to-many (an incident can affect multiple products; a product can have multiple concurrent incidents) and would need a join entity/table, e.g. `INCIDENT_PRODUCT`, not shown above for brevity. `INCIDENT` would also have a nested "timeline of updates" collection (timestamped entries), omitted from the attribute list above for the same reason.

### Incident status (standard status-page workflow)

```mermaid
%%{init: {'theme': 'dark'}}%%
stateDiagram-v2
    [*] --> Investigating
    Investigating --> Identified
    Identified --> Monitoring
    Monitoring --> Resolved
    Investigating --> Resolved
    Identified --> Resolved
```

### Public status badge derivation

Not a state machine on a stored field — a computed value, re-derived from the two live inputs each time the badge is rendered.

```mermaid
%%{init: {'theme': 'dark'}}%%
flowchart TD
    A[Health check result] -->|down| B[Major Outage]
    A -->|up| C{Open incident affecting this product?}
    C -->|no| D[Operational]
    C -->|yes, severity Critical| E[Major Outage]
    C -->|yes, severity Major| F[Partial Outage]
    C -->|yes, severity Minor| G[Degraded]
    H[Smoke test: ok / degraded / down] -.->|informs the decision to open an incident;<br/>never sets the badge directly| C
```

Exact severity → badge mapping is noted in the requirements as "finalized at implementation time" — the diagram above reflects the stated principle (liveness sets the floor, incident severity moves the needle on degraded states), not a locked-in mapping.
