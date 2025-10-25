// /wwwroot/js/pages/assets-assetinfo.js
(function ($, window, document) {
    "use strict";

    // ----- Namespaces / deps -----
    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const B = AMS.bridge || (AMS.bridge = {});

    // -----------------------------
    // TAB + TOUR METADATA / HELPERS
    // -----------------------------
    // Each tab-pane maps to its nav link + visual ordering index
    const TAB_META = {
        "#tab_1": { nav: "#a_tab_1", idx: 1 },
        "#tab_2": { nav: "#a_tab_2", idx: 2 },
        "#tab_3": { nav: "#a_tab_3", idx: 3 }
    };

    // Which .tab-pane is this element inside?
    function paneOf(el) {
        const pane = el?.closest?.(".tab-pane");
        return pane ? `#${pane.id}` : null;
    }

    // Brutally force-show a tab, works with BS5, BS4, or raw DOM
    function forceShowTab(linkSelector) {
        const link = document.querySelector(linkSelector);
        if (!link) return;

        // Try Bootstrap 5 Tab API
        if (window.bootstrap?.Tab) {
            try {
                window.bootstrap.Tab.getOrCreateInstance(link).show();
                return;
            } catch (_) { /* fall through */ }
        }

        // Try Bootstrap 4 (jQuery plugin)
        if (window.jQuery && jQuery.fn.tab) {
            try {
                jQuery(link).tab("show");
                return;
            } catch (_) { /* fall through */ }
        }

        // Manual fallback (in case neither API is available)
        const targetSel = link.getAttribute("data-bs-target") || link.getAttribute("href");
        if (!targetSel) return;

        // Remove all active/show classes
        document.querySelectorAll(".nav .nav-link.active").forEach(a => a.classList.remove("active"));
        document.querySelectorAll(".tab-pane.active").forEach(p => p.classList.remove("active", "show"));

        // Activate this link + its pane
        link.classList.add("active");
        const pane = document.querySelector(targetSel);
        if (pane) pane.classList.add("active", "show");
    }

    // The tour calls this to make sure the correct tab is visible
    // for whatever element it's trying to highlight.
    function activatePaneForElement(el) {
        if (!el) return;
        const pid = paneOf(el);
        if (!pid) return;
        const meta = TAB_META[pid];
        if (meta?.nav) {
            forceShowTab(meta.nav);
        }
    }

    // Resume helpers for the tour (session storage)
    const RESUME = {
        key: "PAMS_TOUR:Assets/AssetInfo:step",
        save(i) {
            try { sessionStorage.setItem(this.key, String(i)); } catch (_) { }
        },
        read() {
            try {
                const v = sessionStorage.getItem(this.key);
                return v ? parseInt(v, 10) : 0;
            } catch (_) {
                return 0;
            }
        },
        clear() {
            try { sessionStorage.removeItem(this.key); } catch (_) { }
        }
    };

    // ---------------------------------
    // PAGE-SPECIFIC DOM/UTILITY HELPERS
    // ---------------------------------

    function adjustMainContainerHeight() {
        const mainContainer = document.getElementById("dvMainContainer");
        if (!mainContainer) return;

        // auto size, then lock exact height so right column can match
        mainContainer.style.height = "auto";
        mainContainer.style.height = `${mainContainer.scrollHeight}px`;

        setCardBodyHeight();
    }

    function setCardBodyHeight() {
        const mainContainer = document.getElementById("dvMainContainer");
        const cardBody = document.getElementById("dvHistoryCard");
        if (mainContainer && cardBody) {
            // 80px offset is your original padding fudge
            cardBody.style.height = `${mainContainer.scrollHeight - 80}px`;
        }
    }

    // Reads which tab should be active from server -> bridge
    // B.assetInfo.tab is provided in Razor before this file is loaded
    // ("1" | "2" | "3" | ""/null)
    function setInitialTab() {
        const tabVal = B?.assetInfo?.tab;
        let desiredPane = "#tab_1"; // default if null/undefined/"1"

        if (tabVal === "2") desiredPane = "#tab_2";
        else if (tabVal === "3") desiredPane = "#tab_3";

        const meta = TAB_META[desiredPane];
        if (meta?.nav) {
            forceShowTab(meta.nav);
            return;
        }

        // Fallback if markup changes in the future:
        Object.keys(TAB_META).forEach(pid => {
            if (pid === desiredPane) {
                $(pid + "," + TAB_META[pid].nav).addClass("active show");
            } else {
                $(pid + "," + TAB_META[pid].nav).removeClass("active show");
            }
        });
    }

    // -------------------------
    // DELETE ASSET (global for Razor onclick)
    // -------------------------
    function deleteAssetPublic(id) {
        // localized strings are injected from Razor into AMS.bridge.assetInfo.deleteText
        const t = B?.assetInfo?.deleteText || {};
        const options = {
            actionUrl: `/Users/Assets/DeleteAsset/${id}`,
            title: t.title ?? "Delete Asset",
            message: t.message ?? "Are you sure?",
            btnConfirmText: t.confirm ?? "Confirm",
            btnCancelText: t.cancel ?? "Cancel"
        };

        if (typeof window.openConfirmation === "function") {
            window.openConfirmation(options);
        } else {
            console.warn("[AssetInfo] openConfirmation() not found.");
        }
    }

    // keep for <button onclick="deleteAsset('...')">
    window.deleteAsset = deleteAssetPublic;

    // -------------------------
    // PAGE INIT (runs via AMS.pages.register)
    // -------------------------
    function init(/* ctx: { bridge, util, key } */) {
        // Collapse/toggle the side menu like other pages
        U?.hideMenu?.();

        // Init DataTables only if table has rows
        const $tblDocs = $("#tblDocuments");
        if ($tblDocs.length > 0 && $tblDocs.find("tbody tr").length > 0) {
            const makeDT = U?.makeDataTable || window.makeDataTable;
            if (typeof makeDT === "function") {
                // Your Assets/Index uses makeDataTable(selector, "2", 10)
                // "2" means: custom placement/styling of DataTable buttons
                makeDT("#tblDocuments", "2", 10);
            } else {
                console.warn("[AssetInfo] DataTables helper not found; skipping table init.");
            }
        }

        // Sync heights so the history card scrollbar fits nicely
        adjustMainContainerHeight();
        window.addEventListener("resize", setCardBodyHeight, { passive: true });

        // Show the correct starting tab from TempData["tab"]
        setInitialTab();

        // Expose helpers for the tour system (assets-assetinfo-tour.js uses these)
        // NOTE: your tour loader looks for window.PAMS_PAGE.AssetInfo
        // so we publish under that name.
        window.PAMS_PAGE = window.PAMS_PAGE || {};
        window.PAMS_PAGE.AssetInfo = {
            forceShowTab,
            activatePaneForElement,
            RESUME
        };

        // (Optional) also expose for debugging if you want to poke from console
        window.AMS_PAGE = window.AMS_PAGE || {};
        window.AMS_PAGE.AssetInfo = {
            TAB_META,
            paneOf,
            forceShowTab,
            activatePaneForElement,
            RESUME,
            adjustMainContainerHeight,
            setCardBodyHeight,
            setInitialTab
        };
    }

    // Hook into your global page registry
    AMS.pages?.register?.("Assets/AssetInfo", init);

})(jQuery, window, document);
