# API Outline

Base: `/api/v1`

## Auth
`POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout`.

## Catalog
`GET/POST /products`, `GET/PUT /products/{id}`, `GET /products/by-barcode/{barcode}`, category/barcode/price endpoints.

## Stock
`GET /stocks`, `GET /stocks/movements`, receipt/purchase, transfer, write-off and adjustment endpoints. Posting is explicit and permission protected.

## POS / Sales
`POST /sales`, `POST /sales/{id}/items`, `POST /sales/{id}/complete`, `POST /sales/{id}/cancel`, `POST /sales/{id}/returns`.

## Payments
`POST /sales/{id}/payments`, `GET /payments/{id}`, `GET /payments/{id}/status`, `POST /payments/{id}/cancel`, `POST /payments/{id}/refund`. QR response may include provider-safe QR payload/image representation and expiry.

## Fiscalization
`POST /sales/{id}/fiscalize`, `GET /sales/{id}/fiscal-status`, `POST /fiscal/{id}/retry`. Lawful non-fiscal workflow uses separate permission-controlled request/approval endpoints and configured reason codes.

## Inventory
Create inventory, enter counts, calculate differences, approve and post adjustment.

## Loyalty
Customer lookup/registration, balance, accrual, redemption and transaction history.

## Reports
Sales, stock, movement, gross margin, cashier, payments, fiscalization control, inventory discrepancies and loyalty.

## Integration
Internal/provider webhook endpoints are authenticated/verified and idempotent. 1C synchronization endpoints are isolated from public client APIs.

All APIs must publish OpenAPI documentation, standardized validation errors and correlation IDs.
