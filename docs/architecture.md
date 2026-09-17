# DADO Retail — Architecture

## Components

```text
POS / Customer Display / Admin / Mobile clients
                    |
                HTTPS API
                    |
            DADO Retail API
                    |
  +-----------------+-------------------+
  |                 |                   |
Application      Integration         Security
  |                 |                   |
Domain       Payment / Fiscal / 1C    RBAC/Audit
  |                 |                   |
  +------------- Infrastructure --------+
                    |
              PostgreSQL
```

## Backend projects
- `DadoRetail.Domain`: entities, value objects, domain rules, enums.
- `DadoRetail.Application`: use cases, DTOs, validators, service interfaces.
- `DadoRetail.Infrastructure`: EF Core, repositories, provider adapters, integration implementations.
- `DadoRetail.Api`: REST endpoints, auth, middleware, Swagger.

## Main aggregates
Product, Price, Store, Warehouse, StockMovement, Purchase, Transfer, Inventory, Sale, Return, Payment, FiscalTransaction, Customer, LoyaltyTransaction, Promotion, User, Role, Permission, AuditLog.

## Reliability
Use database transactions for document posting. Use optimistic concurrency where useful. Integration requests require idempotency keys. Provider callbacks/webhooks must be idempotent. Store integration attempts and errors for reconciliation.

## Offline POS
Offline sales may be supported only where legally and technically permissible. Local queue uses UUIDs and sync status. Server remains authoritative. Duplicate submissions must not create duplicate sales/payments.
