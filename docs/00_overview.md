# Licensing + Monitoring Platform — Overview

## Purpose

A platform to support a portfolio of future products/services:

1. **Licensing** — grant, track, and renew licenses for products, per customer organization.
2. **Monitoring** — track the health/status of those products and communicate incidents to customers via a status page.

Full requirements: [licensing-service.md](./licensing-service.md), [monitoring-service.md](./monitoring-service.md).

Screens & flows for UI/UX handoff: [UiUx/licensing-screens.md](./UiUx/licensing-screens.md), [UiUx/monitoring-screens.md](./UiUx/monitoring-screens.md).

Logo, color palette, and theming requirements: [UiUx/branding.md](./UiUx/branding.md).

## Two services, not one

Licensing and Monitoring share the concept of a **Product**, but are deliberately split into two logically separate services (communicating via API, not a shared database):

- **Different operational profile**: Licensing is CRUD/admin-workload; Monitoring is polling/background-job-workload (health checks, smoke test ingestion, uptime aggregation).
- **Blast-radius isolation**: a status page should stay up even if the Licensing service has an incident of its own. Coupling them would undermine the point of having a status page.
- Already partially split in practice: a separate smoke-test runner service/deployment feeds results into Monitoring, and future multi-region deployments will run one Monitoring instance per region.

## Audiences (both services)

- **Admin backoffice** — internal, single admin user for now. No roles/permissions system needed yet.
- **Customer backoffice** — external. One **Org** per customer, with multiple individual **Users** per org (backed by Active Directory — concrete auth/federation model intentionally not pinned down yet; for FE purposes, assume "a logged-in user belongs to an org" without designing the login flow).

## Explicitly deferred (v1)

These were raised and consciously scoped out, not forgotten:

- License validation/enforcement mechanism (how a licensed product proves it's licensed at runtime).
- Usage metering (products reporting real consumption back against seat caps).
- Multi-instance/per-deployment licenses and feature/tier-based gating.
- Incident notifications (email/webhook subscriptions).
- Region-aware modeling inside a single Monitoring service instance (handled operationally via per-region deployments instead).

## Status

Requirements only — no application code exists for this design yet. The current `EnsyInc.Enclave` codebase (`src/`) is an unrelated bare skeleton (single `App` entity, stub CRUD routes) that will be superseded, not extended, by this design. Frontend design is a follow-up conversation.
