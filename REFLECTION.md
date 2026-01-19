##### CAVEATS AND DESIGN DECISIONS

### 1. **Persistent Caching**

The persistent cache stores the aggregated KYC snapshot returned by the service, rather than vendor-specific DTOs. 
This reduces coupling to external schemas, simplifies persistence, and ensures the cache can directly satisfy future 
requests even after application restarts

I opted for handling the caching strategy through multiple workflows injected using the decorator pattern, but if there were more endpoints requiring similar caching logic,
I would consider implementing a more generic caching mechanism using pipeline behaviors in MediatR to reduce code duplication.

### 2. **Error Handling**

User-facing errors such as validation failures and not found errors are exposed to the user with clear messages, while system-level and unexpected errors are simply logged.
Given a larger scope and a more complex domain, I would consider adding more granular error types 
like domain-specific exceptions or infrastructure-related errors to provide better context and handling strategies.

The logging strategy focuses on capturing essential information for error handling and not debug/information logs, to avoid excessive verbosity.

### 3. **Code Quality and Reusability**

I opted for a clean architecture approach, left out the domain layer since this application doesnt own any entities. 
The Infrastructure layer is designed to be swappable, the application layer simply exposes the interfaces.

Each layer has its own DI extention method to make it easier to extend with more services

### 4. **Unit Testing**

The unit test file has a test harness which gathers common helper logic. Each public method that is tested is given a class, 
and I opted for single line assertions to make it extra clear in the test output which method and path is failing or passing. 
I'd rather have multiple smaller tests than to have one bigger test that is less specific. 
Furthermore I try to keep the arrange act and assert sections as small as possible for readability

Tests should generally follow the naming convention "Method-Outcome-GivenScenario"

I used fakeitEasy because there might be mixed opinions on Moq due to past controversies regarding hidden telemetry.



###### ARCHITECTURE OVERVIEW 

# Kyc.Aggregation
Service for aggregating KYC data with persistent caching

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
Application → Contracts
Infrastructure → Application + Contracts
Api → Application + Contracts (+ Infrastructure for DI registration)
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
* Entity Framework Core DbContext and snapshot entities
* Persistent cache implementation (database-backend)
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
* Handlers pass the request onto workflows which orchestrate:
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


