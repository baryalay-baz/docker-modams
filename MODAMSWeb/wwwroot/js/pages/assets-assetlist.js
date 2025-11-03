// /wwwroot/js/pages/assets-assetlist.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        category: "#ddlCategoryFilter",
        table: "#tblAssets"
    };

    function init(/*ctx*/) {
        if (!U) {
            console.error("[AMS] utils not loaded.");
            return;
        }

        U.hideMenu?.();

        initDataTable();
        initCategoryFilter();
        bindCategoryChange();
    }

    const NS = ".assetList";

    function bindCategoryChange() {
        const $doc = $(document);
        $doc.off("change" + NS, SEL.category);
        $doc.on("change" + NS, SEL.category, function () {
            const selectedVal = $(this).val();
            window.location.href = "/Users/Assets/AssetList/" + encodeURIComponent(selectedVal);
        });
    }

    function initCategoryFilter() {
        const $cat = $(SEL.category);
        if (!$cat.length) return;

        if ($.fn.select2) {
            if (!$cat.data("select2")) {
                $cat.select2({ width: "200px" });
            }
            const $select2Box = $cat.next(".select2-container");
            if ($select2Box.length) $select2Box.attr("data-tour", "al.filter");
        } else {
            $cat.attr("data-tour", "al.filter");
        }
    }

    function initDataTable() {
        const selector = SEL.table || "#tblAssets";

        // Localized tooltip text for the single action (Info)
        const isSo = (U.getCurrentLanguage?.() === "so");
        const tInfo = isSo ? "Xogta Hantida" : "Asset Information";

        // Only one button on this page (info)
        const actions = {
            enable: true,
            // padColumnIndex: undefined (default last visible col is Status; good)
            paddingPx: 140,
            buttons: [
                {
                    key: "info",
                    title: tInfo,
                    iconHtml: "<i class='fe fe-info'></i>",
                    href: "/Users/Assets/AssetInfo/{id}"
                }
            ]
        };

        // Mode "2" to keep exports visible, 10 per page
        U.makeDataTable(selector, "2", 10, actions);
    }

    AMS.pages?.register?.("Assets/AssetList", init);

})(jQuery, window, document);
