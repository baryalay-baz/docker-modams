// /wwwroot/js/core/page-registry.js
(function () {
    window.PAMS = window.PAMS || {};
    const P = window.PAMS;
    const pages = Object.create(null);

    P.pages = {
        register(key, initFn) { pages[key] = initFn; },
        start() {
            // Primary key comes from your layout: data-page="@ViewData["TourPageKey"]"
            const key = document.body.getAttribute("data-page") || "";

            // Optional fallback if you add data-page-fallback in layout
            const fallback = document.body.getAttribute("data-page-fallback") || "";

            if (!key && !fallback) {
                console.warn('[PAMS] data-page is empty. Set ViewData["TourPageKey"] in the view.');
                return;
            }

            const init = pages[key] || pages[fallback];
            if (typeof init !== "function") {
                console.warn(`[PAMS] No page init registered for "${key || fallback}". Did you call PAMS.pages.register("<Controller>/<Action>", init)?`);
                return;
            }

            try {
                init({ bridge: P.bridge, util: P.util, key: key || fallback });
            } catch (err) {
                console.error(`[PAMS] Error in ${key || fallback} init:`, err);
            }
        }
    };

    const ready = P.util?.ready || (fn =>
        document.readyState === "loading"
            ? document.addEventListener("DOMContentLoaded", fn, { once: true })
            : fn()
    );
    ready(() => P.pages.start());
})();
