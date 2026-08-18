# ⚛️ RBMK-1000 Nuclear Reactor Simulation & Telemetry Engine

A domain-driven backend service and physics simulation engine built with C# and .NET that models real-time nuclear thermodynamics, neutronics feedback loops, and grid-wide channel telemetry.

The project simulates the complex physical dynamics of an RBMK-1000 nuclear reactor core—including positive void reactivity, Doppler resonance absorption, subcooled convective heat transfer, and 2D spatial thermal conduction—processed via a high-frequency background tick worker and orchestrated using CQRS.

---

## 🛠️ Tech Stack & Architecture

* **Framework:** .NET 10 / C# (ASP.NET Core Web API)
* **Architecture:** Clean Architecture / Domain-Driven Design (DDD)
* **Application Pattern:** CQRS via **MediatR**
* **Data Access:** Entity Framework Core, PostgreSQL, Repository & Unit of Work Patterns
* **Concurrency & Processing:** Background Services (`IHostedService` / Tick Loop)
* **Testing:** xUnit, **Moq** (unit testing handlers, repositories, and domain models)

---

## 🏛️ System Architecture & Domain Design

The system separates physics calculations from application workflows using a clean layered architecture:

```text
       +--------------------+               +--------------------+
       |  REST Controllers  |               | Background Worker  |
       |  (Web API Layer)   |               |  (PeriodicTimer)   |
       +--------------------+               +--------------------+
                 |                                    |
                 +-----------------+------------------+
                                   | Dispatches CQRS Requests
                                   v
+-----------------------------------------------------------------+
|                         MediatR Pipeline                        |
|                                                                 |
|   Commands (State Mutations):     Queries (Telemetry Reads):    |
|   - ProcessReactorTickCommand     - GetMonitoredReactorGridIdsQuery|
|   - [Control & Config Commands]   - GetReactorOverviewQuery     |
|                                   - Get2DGridDesignQuery        |
+-----------------------------------------------------------------+
                                   |
                                   v
+-----------------------------------------------------------------+
|                      Domain & Physics Engine                    |
|                                                                 |
|  - Convective Heat Transfer      - Fast/Thermal Flux Balance    |
|  - Doppler Feedback Loops        - Subcooled Phase Transitions  |
|  - Spatial Conduction            - Void Reactivity Coefficients |
+-----------------------------------------------------------------+
                                   |
                                   v
+-----------------------------------------------------------------+
|                       Data Access Layer                         |
|                                                                 |
|              IUnitOfWork  <--->  IReactorGridRepository         |
+-----------------------------------------------------------------+
```
