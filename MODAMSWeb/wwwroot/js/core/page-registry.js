// /wwwroot/js/core/page-registry.js
(function () {
    window.AMS = window.AMS || {};
    const AMS = window.AMS;
    const pages = Object.create(null);

    AMS.pages = {
        register(key, initFn) { pages[key] = initFn; },
        start() {
            // Primary key comes from your layout: data-page="@ViewData["TourPageKey"]"
            const key = document.body.getAttribute("data-page") || "";

            // Optional fallback if you add data-page-fallback in layout
            const fallback = document.body.getAttribute("data-page-fallback") || "";

            if (!key && !fallback) {
                console.warn('[AMS] data-page is empty. Set ViewData["TourPageKey"] in the view.');
                return;
            }

            const init = pages[key] || pages[fallback];
            if (typeof init !== "function") {
                console.warn(`[AMS] No page init registered for "${key || fallback}". Did you call AMS.pages.register("<Controller>/<Action>", init)?`);
                return;
            }

            try {
                init({ bridge: AMS.bridge, util: AMS.util, key: key || fallback });
            } catch (err) {
                console.error(`[AMS] Error in ${key || fallback} init:`, err);
            }
        }
    };

    const ready = AMS.util?.ready || (fn =>
        document.readyState === "loading"
            ? document.addEventListener("DOMContentLoaded", fn, { once: true })
            : fn()
    );

    ready(() => AMS.pages.start());
})();
