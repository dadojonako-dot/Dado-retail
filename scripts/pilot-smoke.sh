#!/usr/bin/env bash
set -euo pipefail
API="${API_URL:-http://localhost:8080}"
USER="${PILOT_USER:-pilotadmin}"
PASS="${PILOT_PASSWORD:?Set PILOT_PASSWORD}"
req(){ curl -fsS -H "Content-Type: application/json" ${TOKEN:+-H "Authorization: Bearer $TOKEN"} "$@"; }
json(){ python3 -c "import json,sys; print(json.load(sys.stdin)$1)"; }
echo "[1/10] health"; curl -fsS "$API/health" >/dev/null
TOKEN=$(req -d "{\"username\":\"$USER\",\"password\":\"$PASS\"}" "$API/api/v1/auth/login" | json "['accessToken']")
echo "[2/10] bootstrap ids"
STORE=$(req "$API/api/v1/stores" | json "[0]['id']")
WAREHOUSE=$(req "$API/api/v1/warehouses?storeId=$STORE" | json "[0]['id']")
REGISTER=$(req "$API/api/v1/cash-registers?storeId=$STORE" | json "[0]['id']")
STAMP=$(date +%s); BARCODE="990000$STAMP"; SKU="PILOT-$STAMP"
echo "[3/10] product"; PRODUCT=$(req -d "{\"sku\":\"$SKU\",\"name\":\"Pilot Test Product\",\"unitOfMeasure\":\"pcs\",\"barcodes\":[\"$BARCODE\"]}" "$API/api/v1/products" | json "['id']")
echo "[4/10] price"; req -d "{\"productId\":\"$PRODUCT\",\"storeId\":\"$STORE\",\"amount\":10.00}" "$API/api/v1/prices" >/dev/null
echo "[5/10] supplier + receipt"; SUPPLIER=$(req -d "{\"name\":\"Pilot Supplier $STAMP\"}" "$API/api/v1/suppliers" | json "['id']"); PURCHASE=$(req -d "{\"supplierId\":\"$SUPPLIER\",\"warehouseId\":\"$WAREHOUSE\",\"documentNumber\":\"PILOT-$STAMP\"}" "$API/api/v1/purchases" | json "['id']"); req -d "{\"barcode\":\"$BARCODE\",\"quantity\":5,\"purchasePrice\":5}" "$API/api/v1/purchases/$PURCHASE/items/by-barcode" >/dev/null; req -X POST "$API/api/v1/purchases/$PURCHASE/post" >/dev/null
echo "[6/10] sale"; SALE=$(req -d "{\"storeId\":\"$STORE\",\"warehouseId\":\"$WAREHOUSE\",\"cashRegisterId\":\"$REGISTER\"}" "$API/api/v1/sales" | json "['id']"); req -d "{\"barcode\":\"$BARCODE\",\"quantity\":2}" "$API/api/v1/sales/$SALE/items/by-barcode" >/dev/null; req -X POST "$API/api/v1/sales/$SALE/checkout" >/dev/null
echo "[7/10] QR payment"; PAYMENT=$(req -d '{"method":2,"amount":20}' "$API/api/v1/sales/$SALE/payments" | json "['id']"); req -X POST "$API/api/v1/payments/$PAYMENT/start" >/dev/null; sleep 4; req "$API/api/v1/payments/$PAYMENT/status" >/dev/null
echo "[8/10] fiscalize"; req -X POST "$API/api/v1/fiscalization/sales/$SALE" >/dev/null
echo "[9/10] return"; RETURN=$(req -d "{\"saleId\":\"$SALE\",\"items\":[{\"productId\":\"$PRODUCT\",\"quantity\":1}]}" "$API/api/v1/returns" | json "['id']"); req -X POST "$API/api/v1/returns/$RETURN/complete" >/dev/null
echo "[10/10] verify stock"; QTY=$(req "$API/api/v1/stocks?warehouseId=$WAREHOUSE&productId=$PRODUCT" | json "[0]['quantity']"); test "$QTY" = "4" || test "$QTY" = "4.0" || { echo "Expected stock 4, got $QTY"; exit 1; }
echo "DADO Retail Pilot smoke test PASSED"
