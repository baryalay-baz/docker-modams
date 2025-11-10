// /wwwroot/js/pages/users/home/home-profile.js
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
        // Nothing else needed; dropdowns handled by Bootstrap.
    }

    PAGES.register && PAGES.register("Home/Profile", init);

})(jQuery, window, document);
