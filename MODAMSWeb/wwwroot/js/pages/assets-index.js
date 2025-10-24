// /wwwroot/js/pages/assets-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        subCategory: "#SubCategoryId",
        table: "#tblAssets"
    };

    function init(/*ctx*/) {
        if (!U) {
            console.error("[AMS] utils not loaded.");
            return;
        }
        U.hideMenu?.();

        initDataTable();
        initSelect2();
        bindSubCategoryChange();
    }

    const NS = ".assetsIndex";
    function bindSubCategoryChange() {
        const $doc = $(document);

        // idempotent: remove old handlers for this page
        $doc.off("change" + NS, SEL.subCategory);

        // bind
        $doc.on("change" + NS, SEL.subCategory, function () {
            const storeId = window.data?.storeId;
            if (!storeId || String(storeId).trim() === "") {
                console.warn("[AMS] storeId not found on window.data");
                return;
            }

            const selected = $(this).val();
            // Build URL with id in the path, only subCategoryId in query
            const url = new URL(
                `/Users/Assets/Index/${encodeURIComponent(storeId)}`,
                window.location.origin
            );

            if (selected != null && String(selected).trim() !== "") {
                url.searchParams.set("subCategoryId", String(selected));
            } else {
                url.searchParams.delete("subCategoryId");
            }

            window.location.href = url.toString();
        });
    }
    function initSelect2() {
        const $sub = $(SEL.subCategory);
        if (!$sub.length) return;

        if ($.fn.select2) {
            // avoid re-initialization if partials reload
            if ($sub.data("select2")) return;
            $sub.select2({ width: "resolve" });
        } else {
            U.log?.("Select2 not present; skipping.");
        }
    }
    function initDataTable() {
        const selector = SEL?.table || "#tblAssets";
        U.makeDataTable(selector, "2", 10);
    }

    // Register with your page system
    AMS.pages?.register?.("Assets/Index", init);

})(jQuery, window, document);
