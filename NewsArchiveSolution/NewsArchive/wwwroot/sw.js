const VERSION = "meridian-v1";
const STATIC_CACHE = VERSION + "-static";
const OFFLINE_URL = "/offline.html";
const PRECACHE = [
    OFFLINE_URL,
    "/js/offline.js",
    "/favicon.ico",
    "/images/brand/icon-192.png",
    "/images/brand/logo-mark-ink.svg",
    "/images/brand/logo-mark-light.svg",
    "/lib/bootstrap/dist/css/bootstrap.min.css",
    "/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
    "/lib/jquery/dist/jquery.min.js"
];
const STATIC_PREFIXES = ["/css/", "/js/", "/lib/", "/images/brand/", "/images/articles/", "/images/avatars/", "/uploads/"];
const PRIVATE_PREFIXES = ["/api/", "/Admin", "/Auth", "/Member", "/history", "/bookmarks", "/newsletter", "/swagger", "/health"];

self.addEventListener("install", function (event) {
    event.waitUntil(caches.open(STATIC_CACHE).then(function (cache) {
        return cache.addAll(PRECACHE);
    }).then(function () {
        return self.skipWaiting();
    }));
});

self.addEventListener("activate", function (event) {
    event.waitUntil(caches.keys().then(function (keys) {
        return Promise.all(keys.filter(function (key) {
            return key.indexOf(VERSION) !== 0;
        }).map(function (key) {
            return caches.delete(key);
        }));
    }).then(function () {
        return self.clients.claim();
    }));
});

function isStatic(url) {
    return STATIC_PREFIXES.some(function (prefix) { return url.pathname.indexOf(prefix) === 0; });
}

function isPrivate(url) {
    return PRIVATE_PREFIXES.some(function (prefix) { return url.pathname.indexOf(prefix) === 0; });
}

self.addEventListener("fetch", function (event) {
    const request = event.request;
    if (request.method !== "GET") return;

    const url = new URL(request.url);
    if (url.origin !== self.location.origin || isPrivate(url)) return;

    if (request.mode === "navigate") {
        event.respondWith(fetch(request).catch(function () {
            return caches.match(OFFLINE_URL);
        }));
        return;
    }

    if (isStatic(url)) {
        event.respondWith(caches.open(STATIC_CACHE).then(function (cache) {
            return cache.match(request).then(function (cached) {
                const network = fetch(request).then(function (response) {
                    if (response.ok && response.type === "basic") cache.put(request, response.clone());
                    return response;
                }).catch(function () {
                    return cached;
                });
                return cached || network;
            });
        }));
    }
});
