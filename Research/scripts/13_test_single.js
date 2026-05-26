const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer');

const COOKIES_PATH = path.join(__dirname, '../vitaliiamaz_cookies');
const OUT_PATH = path.join(__dirname, '../raw/index_screenshots/single_test.png');

async function test() {
  let cookies = [];
  if (fs.existsSync(COOKIES_PATH)) {
    cookies = JSON.parse(fs.readFileSync(COOKIES_PATH, 'utf8'));
  }

  const browser = await puppeteer.launch({ headless: true });
  const context = browser.defaultBrowserContext();
  await context.overridePermissions('https://game.crashmania.com', ['geolocation']);

  const page = await browser.newPage();
  await page.emulateTimezone('America/New_York');
  await page.setGeolocation({ latitude: 40.7895, longitude: -74.0565 });
  await page.setUserAgent('Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36');
  await page.setExtraHTTPHeaders({ 'Accept-Language': 'en-US,en;q=0.9' });

  if (cookies.length > 0) {
    await page.setCookie(...cookies);
  }

  console.log('Navigating to lobby...');
  await page.goto('https://game.crashmania.com/lobby', { waitUntil: 'networkidle2' });
  await new Promise(resolve => setTimeout(resolve, 8000));

  const text = await page.evaluate(() => document.body.innerText);
  const title = await page.title();
  
  console.log('\n--- Result Verification ---');
  console.log('Title:', title);
  if (text.includes('Unfortunately, CrashMania Social Casino is not available')) {
    console.log('STATUS: FAILED (Region block is still active!)');
    console.log('Body Text snippet:', text.slice(0, 300).trim().replace(/\n/g, ' '));
  } else {
    console.log('STATUS: SUCCESS! Bypassed region block.');
    console.log('Body Text length:', text.trim().length);
    console.log('Body Text snippet:', text.slice(0, 300).trim().replace(/\n/g, ' '));
  }

  // Clear popup overlays
  await page.evaluate(() => {
    // Hide standard popup wrappers
    document.querySelectorAll('[class*="modal"], [class*="popup"], [class*="overlay"], [role="dialog"], #intercom-container').forEach(el => {
      el.style.setProperty('display', 'none', 'important');
    });
  });

  await page.screenshot({ path: OUT_PATH });
  console.log(`Saved screenshot to ${OUT_PATH}`);
  await browser.close();
}

test().catch(console.error);
