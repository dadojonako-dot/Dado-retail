# Database Model

Target: PostgreSQL.

## Core tables
`users`, `roles`, `permissions`, `user_roles`, `role_permissions`, `stores`, `warehouses`, `cash_registers`, `products`, `barcodes`, `prices`, `suppliers`, `stock_balances`, `stock_movements`, `purchases`, `purchase_items`, `transfers`, `transfer_items`, `inventories`, `inventory_items`, `sales`, `sale_items`, `returns`, `return_items`, `payments`, `acquiring_transactions`, `fiscal_transactions`, `fiscal_receipts`, `non_fiscal_operation_reasons`, `customers`, `loyalty_transactions`, `promotions`, `audit_logs`, `integration_messages`.

## General rules
Use UUID primary keys for business/integration entities. Store timestamps in UTC and convert at presentation boundary. Monetary values use `numeric`, never floating point. Quantity uses suitable decimal precision. Use unique indexes for barcodes and external integration IDs where applicable.

## Stock
`stock_balances` is an optimized current balance projection. `stock_movements` is the auditable movement ledger. Posting a stock-affecting document writes movement rows and updates balances atomically.

## Sales traceability
A sale must be traceable through: Sale -> Cashier/Register/Store -> Payment -> AcquiringTransaction (when applicable) -> FiscalTransaction -> FiscalReceipt or configured lawful alternative basis -> AuditLog.

## Audit
Audit records are append-only from application perspective and include user, timestamp, IP/workstation where available, action, entity type/id, old/new values or structured change summary and correlation ID.
