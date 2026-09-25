// Regenerates the landing page hero image and the social preview image.
//
//   1. Publish the app and serve publish/wwwroot on port 5055 with any static server
//      that falls back to index.html for unknown paths:
//        dotnet publish RoadScript.csproj -c Release -o publish
//        npx http-server publish/wwwroot -p 5055 --proxy "http://localhost:5055?"
//   2. node tools/images/capture.js http://localhost:5055
//
// Needs Node and Playwright with Chromium (npm install playwright). The WebP and PNG
// encoding uses Python with Pillow (pip install pillow).
const path = require('path');
const { execFileSync } = require('child_process');
const { chromium } = require('playwright');

const base = process.argv[2] || 'http://localhost:5055';
const imgDir = path.resolve(__dirname, '../../wwwroot/img');
const raw = path.join(__dirname, 'hero-raw.png');

(async () => {
  const browser = await chromium.launch();

  // Hero: the portfolio example's timeline at 2x
  const page = await browser.newPage({ viewport: { width: 1600, height: 1100 }, deviceScaleFactor: 2 });
  await page.goto(base + '/showcase/portfolio');
  await page.waitForSelector('.showcase-page .roadmap-container', { timeout: 60000 });
  await page.waitForTimeout(800);
  await page.locator('.showcase-scroller').screenshot({ path: raw });
  execFileSync('python3', ['-c', `
from PIL import Image
im = Image.open(${JSON.stringify(raw)}).convert('RGB')
for w, name, q in [(1600, 'showcase-portfolio.webp', 74), (800, 'showcase-portfolio-800.webp', 78)]:
    h = round(im.size[1] * w / im.size[0])
    im.resize((w, h), Image.LANCZOS).save(${JSON.stringify(imgDir)} + '/' + name, 'WEBP', quality=q, method=6)
    print(name, w, h)
`], { stdio: 'inherit' });

  // Social preview: 1200 x 630, built from the new hero image
  const og = await browser.newPage({ viewport: { width: 1200, height: 630 } });
  await og.goto('file://' + path.join(__dirname, 'og-image.html'));
  await og.waitForTimeout(500);
  const ogRaw = path.join(__dirname, 'og-raw.png');
  await og.screenshot({ path: ogRaw });
  execFileSync('python3', ['-c', `
from PIL import Image
Image.open(${JSON.stringify(ogRaw)}).convert('RGB').quantize(colors=256, method=Image.Quantize.FASTOCTREE, dither=Image.Dither.FLOYDSTEINBERG).save(${JSON.stringify(imgDir)} + '/og-image.png', optimize=True)
print('og-image.png 1200 630')
`], { stdio: 'inherit' });

  await browser.close();
  console.log('Update the img width and height in wwwroot/index.html if the hero size changed.');
})();
