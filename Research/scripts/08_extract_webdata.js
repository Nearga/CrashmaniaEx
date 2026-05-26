const fs = require('fs');
const path = require('path');

const dataFile = process.argv[2];
const outDir = process.argv[3];

if (!dataFile || !outDir) {
  console.error("Usage: node extract_webdata.js <data_file> <out_dir>");
  process.exit(1);
}

const buffer = fs.readFileSync(dataFile);
let offset = 0;

// Read signature
let signature = "";
while (buffer[offset] !== 0 && offset < buffer.length) {
  signature += String.fromCharCode(buffer[offset]);
  offset++;
}
offset++; // skip null byte

if (signature !== "UnityWebData1.0") {
  console.error("Not a UnityWebData1.0 file. Found:", signature);
  process.exit(1);
}

console.log("Found signature:", signature);

// Read header length (uint32 LE)
const headerLength = buffer.readUInt32LE(offset);
offset += 4;
console.log("Header Length:", headerLength);

fs.mkdirSync(outDir, { recursive: true });

while (offset < headerLength) {
  const fileOffset = buffer.readUInt32LE(offset);
  offset += 4;
  const fileLength = buffer.readUInt32LE(offset);
  offset += 4;
  
  const pathLength = buffer.readUInt32LE(offset);
  offset += 4;
  
  let filePath = "";
  for (let i = 0; i < pathLength; i++) {
    filePath += String.fromCharCode(buffer[offset]);
    offset++;
  }
  
  console.log(`Extracting: ${filePath} (offset: ${fileOffset}, len: ${fileLength})`);
  
  const outPath = path.join(outDir, filePath);
  fs.mkdirSync(path.dirname(outPath), { recursive: true });
  
  const fileData = buffer.slice(fileOffset, fileOffset + fileLength);
  fs.writeFileSync(outPath, fileData);
}

console.log("Done!");
