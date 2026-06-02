// Minimal no-op service worker — required for PWA installability.
// No caching in v1; full offline support is a separate effort.
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', (e) => e.waitUntil(self.clients.claim()));
self.addEventListener('fetch', () => { /* network-first; no caching in v1 */ });
