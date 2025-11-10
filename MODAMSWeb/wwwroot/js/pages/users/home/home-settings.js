// /wwwroot/js/pages/users/home/home-settings.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const PAGES = AMS.pages || (AMS.pages = { register() { }, start() { } });

    function initBootstrapUI() {
        if (!window.bootstrap) return;
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => { try { new bootstrap.Tooltip(el); } catch { } });
        document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => { try { new bootstrap.Popover(el, { trigger: "hover", html: true, container: "body" }); } catch { } });
    }

    function init() {
        try { U.hideMenu?.(); } catch { }
        initBootstrapUI();
        // Dropdown behavior handled by Bootstrap bundle already included in layout.
    }

    PAGES.register && PAGES.register("Home/Settings", init);

})(jQuery, window, document);
