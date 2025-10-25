// /wwwroot/js/pages/assets-assetlist.js
(function ($, window, document) {
    "use strict";

    // namespaces / dependencies
    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    // ---- selectors for this page ----
    const SEL = {
        category: "#ddlCategoryFilter",
        table: "#tblAssets"
    };

    // ---- init entrypoint ----
    function init(/*ctx*/) {
        if (!U) {
            console.error("[AMS] utils not loaded.");
            return;
        }

        // hide side menu on load
        U.hideMenu?.();

        initDataTable();
        initCategoryFilter(); // <-- new
        bindCategoryChange();
    }

    // ---- events ----
    const NS = ".assetList";

    function bindCategoryChange() {
        const $doc = $(document);

        // idempotent: remove any previous handler
        $doc.off("change" + NS, SEL.category);

        // re-bind
        $doc.on("change" + NS, SEL.category, function () {
            const selectedVal = $(this).val();
            // Original behavior preserved:
            // /Users/Assets/AssetList/{CategoryId}
            window.location.href = "/Users/Assets/AssetList/" + encodeURIComponent(selectedVal);
        });
    }

    // ---- category filter / select2 / tour hookup ----
    function initCategoryFilter() {
        const $cat = $(SEL.category);
        if (!$cat.length) {
            console.warn("[AMS] Category filter dropdown not found:", SEL.category);
            return;
        }

        // If Select2 is available, enhance it.
        if ($.fn.select2) {
            // Avoid double init
            if (!$cat.data("select2")) {
                $cat.select2({
                    // You can tune this. If you wrapped it in a flex item with fixed width,
                    // use '100%'. If not wrapped and you want a fixed width, use e.g. '200px'.
                    width: "200px"
                });
            }

            // After Select2 initializes, it injects a sibling .select2-container.
            // We'll tag that for the tour.
            const $select2Box = $cat.next(".select2-container");

            if ($select2Box.length) {
                // Give it a stable ID so tour can point to it
                $select2Box.attr("data-tour", "al.filter");
            } else {
                console.warn("[AMS] Could not find .select2-container for category filter.");
            }
        } else {
            // No Select2 on page – fallback:
            // Just tag the native <select> itself as the tour target.
            $cat.attr("data-tour", "al.filter");
        }
    }

    // ---- datatable ----
    function initDataTable() {
        // same as your old makeDataTable("#tblAssets", "2", 10);
        // using your AMS.util helper
        const selector = SEL.table || "#tblAssets";
        U.makeDataTable(selector, "2", 10);
    }

    // ---- register page with page system ----
    AMS.pages?.register?.("Assets/AssetList", init);

})(jQuery, window, document);
