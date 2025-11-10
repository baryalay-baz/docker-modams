// /wwwroot/js/pages/users/assets/assets-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        subCategory: "#SubCategoryId",
        table: "#tblAssets"
    };

    function init() {
        if (!U) { console.error("[AMS] utils not loaded."); return; }
        U.hideMenu?.();

        initDataTable();
        initSelect2();
        bindSubCategoryChange();
    }

    const NS = ".assetsIndex";
    function bindSubCategoryChange() {
        const $doc = $(document);
        $doc.off("change" + NS, SEL.subCategory);
        $doc.on("change" + NS, SEL.subCategory, function () {
            const storeId = window.data?.storeId;
            if (!storeId || String(storeId).trim() === "") {
                console.warn("[AMS] storeId not found on window.data");
                return;
            }
            const selected = $(this).val();
            const url = new URL(`/Users/Assets/Index/${encodeURIComponent(storeId)}`, window.location.origin);
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
            if ($sub.data("select2")) return;
            $sub.select2({ width: "resolve" });
        } else {
            U.log?.("Select2 not present; skipping.");
        }
    }

    function initDataTable() {
        const selector = SEL?.table || "#tblAssets";
        const canEdit = !!window.data?.isAuthorized;

        const actions = {
            enable: true,
            paddingPx: 160,
            buttons: [
                { key: "info", titleEn: "Asset Information", titleSo: "Macluumaadka Hantida", iconHtml: "<i class='fe fe-info'></i>", href: "/Users/Assets/AssetInfo/{id}" },
                { key: "edit", titleEn: "Edit Asset", titleSo: "Wax ka beddel Hanti", iconHtml: "<i class='fe fe-edit'></i>", href: "/Users/Assets/EditAsset/{id}" },
                { key: "docs", titleEn: "Asset Documents", titleSo: "Dukumentiyada Hantida", iconHtml: "<i class='fe fe-file-text'></i>", href: "/Users/Assets/AssetDocuments/{id}" },
                { key: "pics", titleEn: "Asset Pictures", titleSo: "Sawirrada Hantida", iconHtml: "<i class='fe fe-image'></i>", href: "/Users/Assets/AssetPictures/{id}" }
            ]
        };

        // Show all buttons if canEdit; otherwise only the "info" button
        const effectiveActions = {
            ...actions,
            buttons: canEdit ? actions.buttons : actions.buttons.filter(b => b.key === "info")
        };

        U.makeDataTable(selector, "2", 10, effectiveActions);
    }

    AMS.pages?.register?.("Assets/Index", init);

})(jQuery, window, document);
