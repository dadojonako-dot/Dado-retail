# AGENTS.md — DADO Retail Codex Instructions

## Mission
Implement DADO Retail as a production-oriented centralized retail platform for supermarket operations, integrated with 1С:Розница 2.3.

## Mandatory architecture
Use Clean Architecture with projects: Domain, Application, Infrastructure, Api. PostgreSQL is the system database. EF Core migrations are required. POS and admin clients communicate only through authenticated API; never connect directly to PostgreSQL.

## Security
Use JWT authentication, refresh-token strategy, RBAC and fine-grained permissions. Passwords must use a modern password hasher. Never store PAN, CVV or PIN. All critical business actions must be auditable.

## Business invariants
Stock changes only through posted business documents. Documents follow Draft -> Posted -> Cancelled/Adjusted lifecycle. Posted documents cannot be silently edited. Use UUID/external IDs and idempotency for integrations and offline synchronization.

## Payments
Create `IPaymentProvider` abstraction. Payment statuses: CREATED, WAITING_PAYMENT, PAID, FAILED, EXPIRED, CANCELLED, REFUNDED. Dynamic QR is unique per payment/check, contains exact payable amount, expires, cannot be reused after success, and payment success must be confirmed by provider API rather than a cashier button.

## Fiscalization
Create `IFiscalizationProvider`. Ordinary retail sales follow fiscalization workflow. Do not implement an unrestricted cashier switch to hide sales from fiscalization. Any legally permitted non-fiscal operation requires configured reason code, permission, audit and optionally supervisor approval. Payment confirmation and fiscal status are separate states.

## 1C integration
Create adapter boundary for 1С:Розница 2.3. Do not invent undocumented production endpoints. Support external IDs, idempotency and synchronization logs. Implement sandbox/file/mock adapters first; actual HTTP/REST/file exchange mapping is configured after official exchange format is supplied.

## Development order
1. Solution skeleton and shared conventions.
2. Domain entities/value objects/enums.
3. PostgreSQL DbContext and migrations.
4. Authentication/RBAC/audit.
5. Product/catalog/barcodes/pricing.
6. Warehouse/stock/document posting.
7. POS/sales/returns.
8. Payment/acquiring/dynamic QR.
9. Fiscalization.
10. Inventory/purchases/transfers.
11. Loyalty/promotions.
12. Reports.
13. 1C integration.
14. React admin and POS UI.
15. Automated tests and deployment.

## Definition of done
Every feature includes validation, authorization, audit where applicable, API contract, database migration if needed, tests for core rules and documentation. Never commit secrets. Keep `.env.example` with placeholders only.
