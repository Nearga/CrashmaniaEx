#!/bin/bash
# Deobfuscate all JS bundles using prettier + js-beautify
# Raw files stay in raw/js/ — output goes to deobfuscated/

RAW="/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/raw/js"
OUT="/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/deobfuscated"

mkdir -p "$OUT"

deobfuscate() {
  local infile="$RAW/$1"
  local outfile="$OUT/$1"

  if [ ! -f "$infile" ]; then
    echo "✗ Missing: $1"
    return
  fi

  echo "→ Processing $1 ($(du -sh "$infile" | cut -f1))..."

  # Step 1: prettier for initial formatting
  npx -y prettier --parser babel \
    --print-width 100 \
    --tab-width 2 \
    --single-quote \
    --trailing-comma none \
    "$infile" > "${outfile}.tmp1" 2>/dev/null

  if [ $? -ne 0 ] || [ ! -s "${outfile}.tmp1" ]; then
    echo "  prettier failed, falling back to js-beautify..."
    npx -y js-beautify \
      --indent-size 2 \
      --max-preserve-newlines 2 \
      --end-with-newline \
      "$infile" > "${outfile}.tmp1" 2>/dev/null
  fi

  # Step 2: js-beautify pass for cleaner output
  npx -y js-beautify \
    --indent-size 2 \
    --max-preserve-newlines 1 \
    --jslint-happy \
    --end-with-newline \
    "${outfile}.tmp1" > "$outfile" 2>/dev/null

  if [ $? -eq 0 ] && [ -s "$outfile" ]; then
    rm -f "${outfile}.tmp1"
    local insize=$(du -sh "$infile" | cut -f1)
    local outsize=$(du -sh "$outfile" | cut -f1)
    echo "  ✓ $1: $insize → $outsize"
  else
    # fallback: just use the tmp1 if step 2 failed
    mv "${outfile}.tmp1" "$outfile" 2>/dev/null
    echo "  ~ $1: beautify step 2 failed, using step 1 output"
  fi
}

echo "=== Starting deobfuscation ==="
echo "Output: $OUT"
echo ""

deobfuscate "index-CBIll7jp.js"
deobfuscate "FilterSystem-BTxhDZq7.js"
deobfuscate "browserAll-tfaR-e5t.js"
deobfuscate "init-CcIkbYkd.js"
deobfuscate "webworkerAll-D7b6Ui4A.js"

echo ""
echo "=== Done! Deobfuscated files: ==="
ls -lh "$OUT/"
