const fs = require('fs');
const { execSync } = require('child_process');

const url = "https://github.com/Perfare/Il2CppDumper/releases/download/v6.7.46/Il2CppDumper-net7-v6.7.46.zip";
const zipPath = "/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/tools/Il2CppDumper-net7.zip";
const destDir = "/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/tools/Il2CppDumper";

console.log("Downloading Il2CppDumper via curl...");
try {
  execSync(`curl -L -o "${zipPath}" "${url}"`);
  console.log("Download completed. Unzipping...");
  
  fs.mkdirSync(destDir, { recursive: true });
  execSync(`unzip -o -q "${zipPath}" -d "${destDir}"`);
  console.log("Successfully unzipped to", destDir);
} catch (e) {
  console.error("Failed:", e.message);
}
