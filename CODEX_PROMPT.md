# Codex Task — DADO Retail

Build the DADO Retail platform described in `README.md`, `AGENTS.md` and `docs/`.

Start with Phase 1 only and produce a runnable foundation:

1. Create `DadoRetail.sln` targeting .NET 8.
2. Create projects `DadoRetail.Domain`, `DadoRetail.Application`, `DadoRetail.Infrastructure`, `DadoRetail.Api` and test projects.
3. Configure project references following Clean Architecture.
4. Add PostgreSQL + EF Core infrastructure and initial `DadoRetailDbContext`.
5. Implement foundational entities for User, Role, Permission, Store, Warehouse, CashRegister, Product, Barcode and AuditLog.
6. Add JWT authentication and RBAC permission policy infrastructure.
7. Add health endpoint and Swagger/OpenAPI.
8. Add Dockerfiles/docker-compose for API + PostgreSQL and `.env.example` without secrets.
9. Add first EF Core migration.
10. Add unit/integration tests for authentication and product barcode uniqueness.

Do not build fake production bank/fiscal/1C integrations in Phase 1. Define interfaces only where needed. Follow secure defaults and all invariants in `AGENTS.md`.

At completion, update README with exact local run commands and summarize created files, migrations and tests.
