const fs = require('fs');
const https = require('https');

// Load cookies
const cookieData = JSON.parse(fs.readFileSync('/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/vitaliiamaz_cookies', 'utf8'));
const cookieString = cookieData.map(c => `${c.name}=${c.value}`).join('; ');
const accessToken = cookieData.find(c => c.name === 'access_token_web')?.value;

const headers = {
  'Cookie': cookieString,
  'Authorization': accessToken ? `Bearer ${accessToken}` : '',
  'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko)',
  'Accept': 'application/json',
  'Origin': 'https://game.crashmania.com',
  'Referer': 'https://game.crashmania.com/'
};

const fetchUrl = (path) => new Promise((resolve, reject) => {
  const req = https.request({
    hostname: 'api.crashmania.com',
    port: 443,
    path,
    method: 'GET',
    headers
  }, res => {
    let data = '';
    res.on('data', chunk => data += chunk);
    res.on('end', () => resolve({ status: res.statusCode, data }));
  });
  req.on('error', reject);
  req.end();
});

async function main() {
  console.log('Fetching catalog...');
  const endpoints = ['/api/catalog', '/api/lobby', '/api/mg/SweepCoins/games', '/api/mg/Coins/games'];
  
  for (const ep of endpoints) {
    console.log(`Trying ${ep}...`);
    const res = await fetchUrl(ep);
    console.log(`Status: ${res.status}`);
    
    if (res.status === 200) {
      fs.writeFileSync(`/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/${ep.replace(/[^a-z0-9]/gi, '_')}.json`, res.data);
      if (res.data.includes('1002')) {
        console.log(`!!! Found 1002 in ${ep}`);
      }
    }
  }
}

main().catch(console.error);
