const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer');

const COOKIES_PATH = path.join(__dirname, '../vitaliiamaz_cookies');
const OUT_DIR = path.join(__dirname, '../raw/index_screenshots');

// Ensure output directory exists
if (!fs.existsSync(OUT_DIR)) {
  fs.mkdirSync(OUT_DIR, { recursive: true });
}

// Pages to capture
const PAGES = [
  { name: '01_homepage', url: 'https://crashmania.com/' },
  { name: '02_lobby_gold', url: 'https://game.crashmania.com/lobby', authenticated: true },
  { name: '03_crash_game_1002', url: 'https://game.crashmania.com/game/1002', authenticated: true },
  { name: '04_store', url: 'https://game.crashmania.com/store', authenticated: true },
  { name: '05_gifts', url: 'https://game.crashmania.com/gifts', authenticated: true },
  { name: '06_account', url: 'https://game.crashmania.com/account', authenticated: true }
];

// Close popups, overlays, intercom, and FTUE modals
async function clearPopups(page) {
  console.log('  -> Aggressively clearing popups and modal overlays...');
  await page.evaluate(() => {
    // 1. Try to click close/dismiss buttons
    const selectors = [
      'button[class*="close" i]',
      'div[class*="close" i]',
      'span[class*="close" i]',
      'a[class*="close" i]',
      '[aria-label="Close" i]',
      '[aria-label="close" i]',
      '.modal-close',
      '.close-btn',
      '.close-button',
      '#close-button',
      'button:has-text("Close")',
      'button:has-text("Dismiss")',
      'button:has-text("Agree")',
      'button:has-text("Accept")'
    ];

    for (const selector of selectors) {
      try {
        const els = document.querySelectorAll(selector);
        els.forEach(el => el.click());
      } catch(e) {}
    }

    // 2. Hide overlay elements (modals, dialogs, cookie banners)
    const overlayKeywords = [
      'modal', 'popup', 'overlay', 'dialog', 'banner', 'welcome-gift', 'wheel-modal',
      'cookie', 'terms', 'privacy', 'notification', 'newsletter', 'intercom', 'ftue'
    ];

    const allElements = document.querySelectorAll('div, section, iframe, aside, dialog');
    allElements.forEach(el => {
      try {
        const id = el.id ? el.id.toLowerCase() : '';
        const className = typeof el.className === 'string' ? el.className.toLowerCase() : '';
        const role = el.getAttribute('role') ? el.getAttribute('role').toLowerCase() : '';
        
        const matchesKeyword = overlayKeywords.some(kw => id.includes(kw) || className.includes(kw));
        const style = window.getComputedStyle(el);
        const zIndex = parseInt(style.zIndex, 10);
        
        if (matchesKeyword || role === 'dialog' || (!isNaN(zIndex) && zIndex > 99)) {
          if (el.id !== 'app' && el.id !== 'root' && !className.includes('layout') && !className.includes('wrapper')) {
            el.style.setProperty('display', 'none', 'important');
            el.style.setProperty('visibility', 'hidden', 'important');
            el.style.setProperty('opacity', '0', 'important');
            el.style.setProperty('pointer-events', 'none', 'important');
          }
        }
      } catch(e) {}
    });

    // 3. Remove Intercom container completely
    document.querySelectorAll('#intercom-container, .intercom-namespace, iframe[name*="intercom"]').forEach(el => {
      try { el.remove(); } catch(e) {}
    });

    // 4. Force body scrollability and remove filters
    document.body.style.setProperty('overflow', 'auto', 'important');
    document.documentElement.style.setProperty('overflow', 'auto', 'important');
    document.body.style.setProperty('filter', 'none', 'important');
    
    const appEl = document.getElementById('app') || document.getElementById('root');
    if (appEl) {
      appEl.style.setProperty('filter', 'none', 'important');
    }
  });
}

// Simulate mouse clicks on the sides of the screen to close FTUE backdrops
async function simulateSideClicks(page, width, height) {
  console.log(`  -> Simulating side clicks for FTUE dismissal (${width}x${height})...`);
  try {
    // Click left side
    await page.mouse.click(10, 10);
    await new Promise(resolve => setTimeout(resolve, 300));
    await page.mouse.click(10, Math.floor(height / 2));
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Click right side
    await page.mouse.click(width - 10, 10);
    await new Promise(resolve => setTimeout(resolve, 300));
    await page.mouse.click(width - 10, Math.floor(height / 2));
    await new Promise(resolve => setTimeout(resolve, 300));
  } catch (err) {
    console.error('  -> Side clicks failed:', err.message);
  }
}

async function capture() {
  let cookies = [];
  if (fs.existsSync(COOKIES_PATH)) {
    try {
      cookies = JSON.parse(fs.readFileSync(COOKIES_PATH, 'utf8'));
    } catch (e) {
      console.error('Failed to parse cookies:', e);
    }
  }

  console.log('Launching browser in NON-HEADLESS mode (will open on screen)...');
  const browser = await puppeteer.launch({
    headless: false,
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  const context = browser.defaultBrowserContext();
  await context.overridePermissions('https://crashmania.com', ['geolocation']);
  await context.overridePermissions('https://game.crashmania.com', ['geolocation']);

  const page = await browser.newPage();
  await page.emulateTimezone('America/New_York');
  await page.setGeolocation({ latitude: 40.7895, longitude: -74.0565 });
  await page.setUserAgent('Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36');
  await page.setExtraHTTPHeaders({
    'Accept-Language': 'en-US,en;q=0.9'
  });

  if (cookies.length > 0) {
    await page.setCookie(...cookies);
  }

  for (const target of PAGES) {
    console.log(`\n--- Capturing: ${target.name} (${target.url}) ---`);
    
    // 1. Capture Mobile Layout
    console.log('Capturing mobile view (390x844)...');
    await page.setViewport({ width: 390, height: 844, isMobile: true, hasTouch: true });
    try {
      await page.goto(target.url, { waitUntil: 'networkidle2', timeout: 35000 });
      await new Promise(resolve => setTimeout(resolve, 8000));
      
      // Clear popups, simulate side clicks, and wait
      await clearPopups(page);
      await simulateSideClicks(page, 390, 844);
      await new Promise(resolve => setTimeout(resolve, 5000)); // 5s delay
      await clearPopups(page);
      
      const mobilePath = path.join(OUT_DIR, `${target.name}_mobile.png`);
      await page.screenshot({ path: mobilePath, fullPage: false });
      console.log(`✓ Saved mobile screenshot to ${mobilePath}`);
    } catch (err) {
      console.error(`✗ Mobile capture failed for ${target.name}:`, err.message);
    }

    // 2. Capture Desktop Layout
    console.log('Capturing desktop view (1440x900)...');
    await page.setViewport({ width: 1440, height: 900, isMobile: false, hasTouch: false });
    try {
      await page.goto(target.url, { waitUntil: 'networkidle2', timeout: 35000 });
      await new Promise(resolve => setTimeout(resolve, 8000));
      
      // Clear popups, simulate side clicks, and wait
      await clearPopups(page);
      await simulateSideClicks(page, 1440, 900);
      await new Promise(resolve => setTimeout(resolve, 5000)); // 5s delay
      await clearPopups(page);
      
      const desktopPath = path.join(OUT_DIR, `${target.name}_desktop.png`);
      await page.screenshot({ path: desktopPath, fullPage: false });
      console.log(`✓ Saved desktop screenshot to ${desktopPath}`);
    } catch (err) {
      console.error(`✗ Desktop capture failed for ${target.name}:`, err.message);
    }
  }

  await browser.close();
  console.log('\n=== All screenshots captured successfully in non-headless mode ===');
}

capture().catch(err => {
  console.error('Fatal error during capture:', err);
  process.exit(1);
});
