// wwwroot/js/asset-info.js
(function () {
    const TAB_META = {
        "#tab_1": "#a_tab_1",
        "#tab_2": "#a_tab_2",
        "#tab_3": "#a_tab_3"
    };

    function paneOf(el) {
        const pane = el?.closest?.(".tab-pane");
        return pane ? `#${pane.id}` : null;
    }

    // Force-show a Bootstrap tab (BS5/BS4/jQuery/manual fallback)
    function forceShowTab(linkSelector) {
        const link = document.querySelector(linkSelector);
        if (!link) return;

        // BS5
        if (window.bootstrap?.Tab) {
            try { bootstrap.Tab.getOrCreateInstance(link).show(); return; } catch (_) { }
        }
        // BS4 (jQuery)
        if (window.jQuery && jQuery.fn.tab) {
            try { jQuery(link).tab("show"); return; } catch (_) { }
        }
        // Manual fallback
        const targetSel = link.getAttribute("data-bs-target") || link.getAttribute("href");
        if (!targetSel) return;

        document.querySelectorAll(".nav .nav-link.active").forEach(a => a.classList.remove("active"));
        document.querySelectorAll(".tab-pane.active").forEach(p => p.classList.remove("active", "show"));

        link.classList.add("active");
        const pane = document.querySelector(targetSel);
        if (pane) pane.classList.add("active", "show");
    }

    function activatePaneForElement(el) {
        const pid = paneOf(el);
        if (!pid) return;
        const linkSel = TAB_META[pid];
        if (linkSel) forceShowTab(linkSel);
    }

    // expose
    window.PAMS_PAGE = window.PAMS_PAGE || {};
    window.PAMS_PAGE.AssetInfo = { TAB_META, paneOf, forceShowTab, activatePaneForElement };
})();
