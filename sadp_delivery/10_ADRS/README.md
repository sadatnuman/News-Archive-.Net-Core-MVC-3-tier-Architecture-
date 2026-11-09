# Architecture Decision Records

ADRs are Week 9 material and belong to the Week 9 milestone (P1 p1), **not** to the midterm. Format: W9 s20, with valid statuses from W12 s7 (Proposed, Accepted, Deprecated, Superseded by ADR-NNN).

## ADR-001: Modular monolith in three layers

**Status:** Accepted

**Context.**
- A small student team (2–5).
- Under 10,000 users expected.
- One deployment target.
- The features share data heavily: articles, users, comments and history.

**Decision.** We will build one ASP.NET Core deployable with UI → BLL → DAL layers, enforced at compile time.

**Alternatives considered:**
- *Microservices from day one.* Rejected: per-service pipelines and data ownership cost more than the team can run (W2 s29–30, W6 s7).
- *Two-tier (controllers using EF directly).* Rejected: the business rules would be untestable without a database (W2 s8).

**Consequences.**
- (+) One build and one deploy.
- (+) Simple ACID transactions.
- (+) Fast tests with fakes.
- (−) The whole app scales as one unit.
- (−) One crash takes down every feature.

**Quality attributes served:** Maintainability, Reliability.

## ADR-002: SQL Server with EF Core and code-first migrations

**Status:** Accepted

**Context.** The editorial state must be correct, and the data is relational, in third normal form.

**Decision.** We will use SQL Server through EF Core 10, with every schema change as a migration.

**Alternatives considered:**
- *A document store.* Rejected: we need joins across 6+ entities and atomic status changes.
- *Raw ADO.NET.* Rejected: more code and a higher injection risk.

**Consequences.**
- (+) ACID transactions and constraints.
- (+) Parameterised queries.
- (−) One database node is a single point of failure.
- (−) Full-text search is not installed on Express (see ADR-004).

**Quality attributes served:** Reliability, Security.

## ADR-003: Optimistic concurrency on article status

**Status:** Accepted

**Context.** Two admins could approve and reject the same story at once. Before this change the last write won silently.

**Decision.** We will mark `Articles.StatusId` as a concurrency token. A stale save returns `false` and the API answers 409.

**Alternatives considered:**
- *A `rowversion` column.* Rejected: every view-count update would change it, so approvals would fail falsely while readers read the story.
- *Pessimistic locks.* Rejected: they hold locks across the user's think time.

**Consequences.**
- (+) Exactly one review decision wins, and it is tested.
- (+) No schema change.
- (−) An edit form opened long ago can still overwrite a newer edit when the status did not change (open, P2).

**Quality attributes served:** Reliability.

## ADR-004: Own word index instead of SQL full-text search

**Status:** Accepted

**Context.**
- `LIKE '%word%'` over article text cannot use an index.
- SQL Server Express here has no full-text search installed.
- The search must work for Bangla.

**Decision.** We will store distinct lower-case words in `ArticleWords (Word, ArticleId)`. Search uses a prefix `LIKE 'term%'`, which is an index seek.

**Alternatives considered:**
- *SQL full-text.* Rejected: not available on every target, and Bangla word breaking is uncertain.
- *An external search engine.* Rejected: an extra service to operate.

**Consequences.**
- (+) Indexed and portable; tested in English and Bangla.
- (−) Words are matched only at their start (`way` does not find *railway*).
- (−) The index must be kept in sync; this is done on save and by a 30 s background job.

**Quality attributes served:** Performance.

## ADR-005: Shared store for session, lockout and reset codes

**Status:** Proposed

**Context.** Lockouts, reset codes, session and cache live in process memory. A second instance would not see them (`08` R2).

**Decision.** We will move them to SQL Server tables, or to Redis if one becomes available, before running more than one instance.

**Alternatives considered:**
- *Sticky sessions.* Rejected: they break when an instance restarts, and lockouts would still be split.

**Consequences.**
- (+) Horizontal scaling becomes possible.
- (+) State survives restarts.
- (−) Each sign-in costs one more database round trip.
- (−) A new critical dependency if Redis is used.

**Quality attributes served:** Scalability, Availability, Security.

## ADR-006: Email delivery behind a factory method

**Status:** Accepted

**Context.**
- The SMTP and outbox senders duplicated their logging and error handling.
- New channels may come later.
- Mail failures must never fail a page.

**Decision.** We will use an abstract `EmailSender` (creator) with `CreateTransport()`, plus one transport per channel, chosen by configuration.

**Alternatives considered:**
- *An `if/else` on the channel inside one sender.* Rejected: OCP and SRP violations (W3 s13).

**Consequences.**
- (+) A new channel is two new classes.
- (+) Error containment lives in one place and is tested.
- (−) Two more small types.

**Quality attributes served:** Availability, Maintainability.
