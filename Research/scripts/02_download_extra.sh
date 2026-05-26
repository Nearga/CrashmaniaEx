#!/bin/bash
BASE="https://game.crashmania.com"
OUT="/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/raw"
INPUT="$OUT/extra_assets.txt"
SUCCESS=0
FAILED=0

while IFS= read -r path; do
  # Skip dynamic paths
  if [[ "$path" == *'${'* ]]; then continue; fi
  
  dir="$OUT$(dirname "$path")"
  file="$OUT$path"
  mkdir -p "$dir"
  
  if [ -f "$file" ] && [ -s "$file" ]; then
    continue
  fi
  
  status=$(curl -s -o "$file" -w "%{http_code}" "$BASE$path")
  if [ "$status" = "200" ]; then
    echo "OK: $path"
    SUCCESS=$((SUCCESS+1))
  else
    rm -f "$file"
    FAILED=$((FAILED+1))
  fi
done < "$INPUT"

echo ""
echo "=== Extra downloads: $SUCCESS OK, $FAILED skipped ==="
