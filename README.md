# DADO Retail System

Централизованная автоматизированная система учета товаров, складских операций, продаж и расчетов сети магазинов «ДАДО».

## Technology
- .NET 8 / ASP.NET Core Web API
- PostgreSQL + Entity Framework Core
- React Admin
- JWT + RBAC
- REST/JSON + OpenAPI/Swagger
- Docker Compose + Nginx
- Integration adapter for 1С:Розница 2.3
- Acquiring and dynamic QR provider adapters
- Fiscalization provider adapter

## Modules
Products, barcodes, prices, promotions, stock, purchases, transfers, inventory, POS, sales and returns, payments, QR acquiring, fiscalization, users/roles/permissions, audit, customers/loyalty, reports and 1C integration.

## Codex
Read `AGENTS.md` and `docs/` before implementation. Work in phases. Do not invent production bank, fiscal or 1C API contracts; use interfaces and sandbox adapters until official provider specifications are supplied.
