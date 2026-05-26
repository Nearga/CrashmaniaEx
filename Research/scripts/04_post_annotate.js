#!/usr/bin/env node
/**
 * post-beautify-annotator.js
 * 
 * After js-beautify runs, this script adds useful annotations:
 * 1. Detects React component functions and labels them
 * 2. Marks API endpoint strings
 * 3. Highlights route definitions
 * 4. Adds section headers for major code blocks
 * 5. Extracts a summary index
 */

const fs = require("fs");
const path = require("path");

const inputFile = process.argv[2];
const outputFile = process.argv[3];

if (!inputFile || !outputFile) {
  console.error("Usage: node post-annotate.js <input.js> <output.js>");
  process.exit(1);
}

console.log(`Annotating ${inputFile}...`);
let code = fs.readFileSync(inputFile, "utf8");
const lines = code.split("\n");
const annotations = [];

// --- Pass 1: Find notable patterns and build index ---

const index = {
  routes: [],
  apiEndpoints: [],
  reactComponents: [],
  gameProviders: [],
  bonusTypes: [],
  storeItems: [],
  eventHandlers: [],
};

lines.forEach((line, i) => {
  const lineNo = i + 1;
  const trimmed = line.trim();

  // Routes
  if (/path:\s*["'`]\/[a-zA-Z]/.test(trimmed)) {
    const m = trimmed.match(/path:\s*["'`](\/[^"'`]+)["'`]/);
    if (m) index.routes.push({ line: lineNo, path: m[1] });
  }

  // API endpoints
  if (/https?:\/\/api\.crashmania\.com/.test(trimmed)) {
    const m = trimmed.match(/(https?:\/\/api\.crashmania\.com[^\s"'`),]+)/);
    if (m) index.apiEndpoints.push({ line: lineNo, url: m[1] });
  }

  // React components (functions returning JSX)
  if (/return\s*\(?\s*\/\*\s*#__PURE__\s*\*\//.test(trimmed) ||
      /\(0,\s*[A-Z]\.jsx\)\(["'`]div/.test(trimmed) ||
      /\(0,\s*[A-Z]\.jsxs?\)\(/.test(trimmed)) {
    // Found JSX render
    // Look back for function name
    for (let j = Math.max(0, i - 5); j < i; j++) {
      const prev = lines[j].trim();
      const fnMatch = prev.match(/(?:function\s+([A-Z][a-zA-Z0-9]+)|const\s+([A-Z][a-zA-Z0-9]+)\s*=)/);
      if (fnMatch) {
        const name = fnMatch[1] || fnMatch[2];
        if (name && !index.reactComponents.find(c => c.name === name)) {
          index.reactComponents.push({ line: lineNo, name });
        }
        break;
      }
    }
  }

  // Game providers
  if (/elagame|slotmill|infingame|mg-poc|websdk/i.test(trimmed)) {
    index.gameProviders.push({ line: lineNo, snippet: trimmed.substring(0, 80) });
  }

  // Bonus/reward types
  if (/hourly.bonus|weekly.streak|monthly.calendar|welcome.bonus|coinback|level.up|rolling.offer/i.test(trimmed)) {
    const m = trimmed.match(/(hourly[_-]?bonus|weekly[_-]?streak|monthly[_-]?calendar|welcome[_-]?bonus|coinback|level[_-]?up|rolling[_-]?offer)/i);
    if (m && !index.bonusTypes.find(b => b.type === m[1])) {
      index.bonusTypes.push({ line: lineNo, type: m[1] });
    }
  }
});

// --- Pass 2: Inline annotations ---
const annotatedLines = lines.map((line, i) => {
  const lineNo = i + 1;

  // Add comment before route definitions
  const routeEntry = index.routes.find(r => r.line === lineNo);
  if (routeEntry) {
    return `\n  /* ROUTE: ${routeEntry.path} */\n${line}`;
  }

  // Add comment before API calls
  const apiEntry = index.apiEndpoints.find(a => a.line === lineNo);
  if (apiEntry) {
    return `  /* API: ${apiEntry.url} */\n${line}`;
  }

  return line;
});

// --- Build index header ---
const header = `
/**
 * ============================================================
 * CRASHMANIA DEOBFUSCATED SOURCE - AUTO-ANNOTATED
 * ============================================================
 * Source: https://game.crashmania.com/assets/${path.basename(inputFile)}
 * Processed: ${new Date().toISOString()}
 * 
 * INDEX:
 * 
 * ROUTES (${index.routes.length}):
${index.routes.map(r => ` *   Line ${String(r.line).padStart(6)}: ${r.path}`).join("\n")}
 * 
 * API ENDPOINTS (${index.apiEndpoints.length}):
${[...new Set(index.apiEndpoints.map(a => a.url))].map(u => ` *   ${u}`).join("\n")}
 * 
 * BONUS TYPES REFERENCED (${index.bonusTypes.length}):
${index.bonusTypes.map(b => ` *   Line ${String(b.line).padStart(6)}: ${b.type}`).join("\n")}
 *
 * GAME PROVIDERS REFERENCED:
${index.gameProviders.slice(0, 10).map(g => ` *   Line ${String(g.line).padStart(6)}: ${g.snippet}`).join("\n")}
 * ============================================================
 */

`;

const finalCode = header + annotatedLines.join("\n");
fs.writeFileSync(outputFile, finalCode);

// --- Write separate index JSON ---
const indexPath = outputFile.replace(".js", ".index.json");
fs.writeFileSync(indexPath, JSON.stringify(index, null, 2));

console.log(`✓ Annotated: ${outputFile}`);
console.log(`✓ Index:     ${indexPath}`);
console.log(`  Routes: ${index.routes.length}`);
console.log(`  API endpoints: ${index.apiEndpoints.length}`);
console.log(`  Bonus types: ${index.bonusTypes.length}`);
