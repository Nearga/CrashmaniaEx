const puppeteer = require('puppeteer');
const http = require('https');

function fetchProxies() {
  return new Promise((resolve, reject) => {
    const url = 'https://proxylist.geonode.com/api/proxy-list?limit=50&country=US&sort_by=lastChecked&sort_type=desc';
    http.get(url, (res) => {
      let data = '';
      res.on('data', (chunk) => data += chunk);
      res.on('end', () => {
        try {
          const json = JSON.parse(data);
          resolve(json.data.map(p => {
            const proto = p.protocols.includes('socks5') ? 'socks5' : (p.protocols.includes('socks4') ? 'socks4' : 'http');
            return { proto, ip: p.ip, port: p.port };
          }));
        } catch (e) {
          reject(e);
        }
      });
    }).on('error', reject);
  });
}

async function test() {
  console.log('Fetching proxies...');
  const proxies = await fetchProxies();
  console.log(`Found ${proxies.length} proxies. Testing for bypass...`);

  for (const p of proxies) {
    const proxyUrl = `${p.proto}://${p.ip}:${p.port}`;
    console.log(`Testing proxy: ${proxyUrl}`);
    let browser;
    try {
      browser = await puppeteer.launch({
        headless: true,
        args: [
          '--no-sandbox',
          '--disable-setuid-sandbox',
          `--proxy-server=${proxyUrl}`
        ]
      });
      const page = await browser.newPage();
      await page.setUserAgent('Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36');
      
      // Go to lobby with a short timeout
      await page.goto('https://game.crashmania.com/lobby', { waitUntil: 'domcontentloaded', timeout: 12000 });
      const text = await page.evaluate(() => document.body.innerText);
      
      if (text.includes('Unfortunately, CrashMania Social Casino is not available')) {
        console.log('  -> Blocked by region restriction.');
      } else if (text.trim().length === 0) {
        console.log('  -> Empty page.');
      } else {
        console.log('  -> SUCCESS! Proxy bypassed regional block.');
        console.log('  -> Page snippet:', text.slice(0, 300));
        await browser.close();
        break;
      }
    } catch (err) {
      console.log(`  -> Failed: ${err.message}`);
    } finally {
      if (browser) await browser.close();
    }
  }
}

test().catch(console.error);
