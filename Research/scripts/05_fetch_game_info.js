const fs = require('fs');
const https = require('https');

// Load cookies
const cookieData = JSON.parse(fs.readFileSync('/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/vitaliiamaz_cookies', 'utf8'));
const cookieString = cookieData.map(c => `${c.name}=${c.value}`).join('; ');
const accessToken = cookieData.find(c => c.name === 'access_token_web')?.value;

const options = {
  hostname: 'api.crashmania.com',
  port: 443,
  path: '/api/catalog/games',
  method: 'GET',
  headers: {
    'Cookie': cookieString,
    'Authorization': accessToken ? `Bearer ${accessToken}` : '',
    'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
    'Accept': 'application/json',
    'Origin': 'https://game.crashmania.com',
    'Referer': 'https://game.crashmania.com/'
  }
};

const req = https.request(options, res => {
  let data = '';
  res.on('data', chunk => data += chunk);
  res.on('end', () => {
    console.log('Status:', res.statusCode);
    if (res.statusCode === 200) {
      try {
        const json = JSON.parse(data);
        fs.writeFileSync('/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/catalog_games.json', JSON.stringify(json, null, 2));
        console.log('Saved catalog_games.json');
        
        let game1002 = null;
        if (Array.isArray(json)) {
            game1002 = json.find(g => g.id == 1002 || g.gameCode == 1002 || g.providerGameId == 1002);
        } else if (json.data && Array.isArray(json.data)) {
            game1002 = json.data.find(g => g.id == 1002 || g.gameCode == 1002 || g.providerGameId == 1002);
        }
        
        if (game1002) {
            console.log('Found Game 1002:', game1002);
        } else {
            console.log('Game 1002 not found in catalog');
        }
      } catch(e) {
        console.error('Error parsing JSON', e);
      }
    } else {
      console.log(data);
    }
  });
});

req.on('error', e => console.error(e));
req.end();
