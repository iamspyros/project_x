#!/bin/bash
# ──────────────────────────────────────────────────
# Proposal Generator – Docker bootstrap script
# Creates data folders, copies sample files, starts app
# ──────────────────────────────────────────────────
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

echo "╔══════════════════════════════════════════════╗"
echo "║     Proposal Generator – Docker Setup        ║"
echo "╚══════════════════════════════════════════════╝"
echo ""

# Create persistent data directories
echo "→ Creating data directories..."
mkdir -p data/price-import data/templates data/storage data/db

# Copy sample price CSV if the folder is empty
if [ -z "$(ls -A data/price-import 2>/dev/null)" ]; then
    echo "→ Copying sample price CSV into data/price-import/..."
    cp price-import/*.csv data/price-import/ 2>/dev/null || true
fi

echo "→ Building and starting containers..."
docker compose up --build -d

echo ""
echo "╔══════════════════════════════════════════════╗"
echo "║  ✓ Proposal Generator is running!            ║"
echo "║                                              ║"
echo "║  Open: http://localhost:8080                 ║"
echo "║                                              ║"
echo "║  Data folders (on your host):                ║"
echo "║    ./data/price-import/  ← drop CSV files    ║"
echo "║    ./data/templates/     ← drop .docx files  ║"
echo "║    ./data/storage/       ← generated PDFs    ║"
echo "║    ./data/db/            ← SQLite database   ║"
echo "║                                              ║"
echo "║  Commands:                                   ║"
echo "║    docker compose logs -f   (view logs)      ║"
echo "║    docker compose down      (stop)           ║"
echo "║    docker compose up -d     (start again)    ║"
echo "╚══════════════════════════════════════════════╝"
