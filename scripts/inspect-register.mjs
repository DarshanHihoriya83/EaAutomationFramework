import { chromium } from 'playwright';

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage();
await page.goto('http://eaapp.somee.com');
await page.waitForTimeout(2000);
const reg = page.locator("nav .container a");
const count = await reg.count();
for (let i = 0; i < count; i++) {
  console.log('nav link', i, await reg.nth(i).innerText());
}
await page.locator("nav a:has-text('Register')").first().click().catch(e => console.log('register click', e.message));
await page.waitForTimeout(2000);
const html = await page.locator('.register-wrapper').innerHTML().catch(() => 'no register-wrapper');
console.log(html.slice(0, 5000));
console.log('profile', await page.locator('nav [title="Manage"]').count());
console.log('logout', await page.locator("button:has-text('Logout')").count());
await browser.close();
