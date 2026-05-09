// Barber Hub service worker — handles Web Push and basic offline fallback.
const CACHE_NAME = 'bh-cache-v1';
const OFFLINE_URL = '/offline.html';

// Cache the offline fallback page on install
self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => cache.addAll([OFFLINE_URL])).catch(() => {})
    );
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE_NAME).map(k => caches.delete(k)))
        ).then(() => self.clients.claim())
    );
});

// Network-first for navigation; if offline, fall back to /offline.html
self.addEventListener('fetch', (event) => {
    const req = event.request;
    if (req.mode === 'navigate') {
        event.respondWith(
            fetch(req).catch(() =>
                caches.match(OFFLINE_URL).then(r => r || new Response('Offline.', {
                    status: 503, headers: { 'Content-Type': 'text/plain' }
                }))
            )
        );
    }
});

// Push notification — fired by the server's WebPush.SendNotificationAsync
self.addEventListener('push', (event) => {
    let data = { title: 'Barber Hub', body: 'You have a new notification.' };
    try {
        if (event.data) data = event.data.json();
    } catch {
        if (event.data) data.body = event.data.text();
    }

    const title = data.title || 'Barber Hub';
    const options = {
        body: data.body || '',
        icon: '/icons/icon-192.png',
        badge: '/icons/icon-192.png',
        tag: data.tag,
        renotify: true,
        data: { url: data.url || '/' }
    };
    event.waitUntil(self.registration.showNotification(title, options));
});

// Tap on a notification → focus the app on the right URL
self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const url = (event.notification.data && event.notification.data.url) || '/';
    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
            for (const client of clientList) {
                if ('focus' in client) {
                    client.navigate(url);
                    return client.focus();
                }
            }
            if (self.clients.openWindow) return self.clients.openWindow(url);
        })
    );
});
