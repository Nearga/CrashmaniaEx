const fs = require('fs');
const zlib = require('zlib');

const inputFile = process.argv[2];
const outputFile = process.argv[3];

if (!inputFile || !outputFile) {
  console.error("Usage: node 09_decompress_brotli.js <input_file> <output_file>");
  process.exit(1);
}

try {
  const inputBuffer = fs.readFileSync(inputFile);
  const decompressed = zlib.brotliDecompressSync(inputBuffer);
  fs.writeFileSync(outputFile, decompressed);
  console.log(`Successfully decompressed ${inputFile} to ${outputFile} (${decompressed.length} bytes)`);
} catch (err) {
  console.error("Decompression failed:", err);
  process.exit(1);
}
