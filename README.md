# Kyc.Aggregation
Service for aggregating KYC data with persistent caching

Here is a **clean, interview-ready `README.md`** you can drop straight into the repository.
It explains **architecture, structure, and intent**, without going into code-level detail — ideal for reviewers and for guiding GitHub Copilot.

---

# KYC Aggregation Service

## Overview

This repository contains a **.NET backend service** that implements a **KYC Aggregation API**.
The service aggregates customer and KYC data from multiple external endpoints, consolidates it into a single response, and exposes it through a unified API.

The service **does not own a business domain**. Instead, it acts as an **aggregation and caching layer** in front of an external Customer Data API.

Key characteristics:

* Clean Architecture
* CQRS with MediatR
* Persistent caching across application restarts
* Thin controllers
* Strong separation of concerns

---

## Architecture Overview

The solution follows **Clean Architecture principles** with **explicit dependency direction**:

```
Api → Application → Contracts
Api → Infrastructure → Application → Contracts
```

* Dependencies always point **inward**
* High-level policy is isolated from low-level implementation details
* Infrastructure concerns are replaceable without changing core logic

There is **no Domain layer** by design, as this service does not own or enforce business invariants.

---

## Solution Structure

```
Kyc.Aggregation.sln
src/
  Kyc.Aggregation.Contracts/
  Kyc.Aggregation.Application/
  Kyc.Aggregation.Infrastructure/
  Kyc.Aggregation.Api/
tests/
  Kyc.Aggregation.Application.Tests/
```

---

## Project Responsibilities

### `Kyc.Aggregation.Contracts`

**Purpose:**
Defines the **public API contract**.

**Contains:**

* DTOs returned by the API (e.g. `AggregatedKycDataDto`)
* Enums and simple value types that are part of the response contract

**Rules:**

* No dependencies
* Stable and safe to share with other services

---

### `Kyc.Aggregation.Application`

**Purpose:**
Implements **use-case orchestration and application-level policy**.

**Contains:**

* CQRS queries and handlers (MediatR)
* Aggregation logic
* Cache and persistence policies (freshness, fallback rules)
* Interfaces (ports) for:

  * External APIs
  * Persistent storage
  * In-memory caching
  * Time/clock abstractions

**Rules:**

* Depends only on `Contracts`
* Does **not** reference EF Core, HttpClient, or IMemoryCache directly
* Defines *what* should happen, not *how*

---

### `Kyc.Aggregation.Infrastructure`

**Purpose:**
Implements **technical and external concerns**.

**Contains:**

* Typed HTTP clients for the external Customer Data API
* Vendor-specific DTOs
* Entity Framework Core DbContext and snapshot entities
* Persistent cache implementation (database-backed)
* Hot cache implementation using `IMemoryCache`

**Rules:**

* Depends on `Application` and `Contracts`
* Implements interfaces defined in Application
* No business or orchestration logic

---

### `Kyc.Aggregation.Api`

**Purpose:**
HTTP hosting and delivery mechanism.

**Contains:**

* ASP.NET Core controllers
* Middleware (error handling, logging)
* Authentication/authorization (if applicable)
* Dependency injection composition root

**Rules:**

* Controllers are thin
* Controllers delegate directly to MediatR
* No business logic in controllers

---

## CQRS Approach

This service uses **CQRS** with **MediatR**.

* The API exposes a **single read-only query**:

  * `GetAggregatedKycData`
* Controllers translate HTTP requests into queries
* Handlers orchestrate:

  1. In-memory cache lookup
  2. Persistent snapshot lookup
  3. External API calls (if needed)
  4. Data aggregation
  5. Cache updates

---

## Caching Strategy

The service implements **two levels of caching**:

### 1. Hot Cache (In-Memory)

* Uses `IMemoryCache`
* Fast, per-instance
* Cleared on application restart

### 2. Persistent Cache (Database)

* Uses Entity Framework Core
* Stores aggregated KYC snapshots
* Survives application restarts

**Caching policy** is defined in the Application layer.
**Caching mechanisms** are implemented in the Infrastructure layer.

---

## Error Handling

* Exception handling is centralized via middleware (API layer)
* User-facing errors are returned as RFC7807 `application/problem+json` with a `traceId`
* Application errors use typed exceptions:
  * `NotFoundException`  404
  * `ValidationException`  400
  * `ExternalDependencyException`  503
* System-level errors are logged with appropriate severity
* Controllers and handlers remain thin and do not contain HTTP-specific error mapping

---

## Testing Strategy

* **Unit tests** target the Application layer
* External dependencies are mocked via interfaces
* Infrastructure and API are tested through integration tests if needed

---

## Design Rationale

* **No Domain layer:**
  The service aggregates and caches data it does not own.

* **Snapshot-based persistence:**
  Persisted data is treated as a read model, not a rich entity.

* **Interfaces in Application:**
  Enables testability and clean separation of policy vs. implementation.

* **Thin API layer:**
  Ensures transport concerns do not leak into application logic.

---

## Possible Future Improvements

* Distributed cache (e.g. Redis)
* Background refresh of stale snapshots
* API versioning
* Observability (metrics, tracing)
* Circuit breakers for external APIs

---

## Final Notes

This project prioritizes:

* Maintainability
* Clear responsibility boundaries
* Testability
* Real-world pragmatism over theoretical purity