// /wwwroot/js/pages/reporting-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    AMS.pages = AMS.pages || {};
    AMS.pages.register = AMS.pages.register || function (key, fn) {
        document.addEventListener("DOMContentLoaded", fn);
    };

    const CFG = window.REPORTING || {};
    const isSomali = String(CFG.isSomali).toLowerCase() === "true";

    const SEL = {
        catalog: "#rp-catalog",
        search: "#rp-search",
        panelTitle: "#rp-panel-title",
        presetBtn: ".rp-preset",
        generate: "#rp-generate",
        actionMenu: "#rp-action-menu",
        forms: ".rp-form",
        select2: ".select2"
    };

    let currentReport = "asset";

    function init() {
        initSelect2();
        bindCatalog();
        bindPresets();
        bindActions();
        bindSearch();
    }

    function initSelect2() {
        try { $(SEL.select2).select2({ dropdownParent: $(".rp-wrapper") }); } catch { }
    }

    // Catalog
    function bindCatalog() {
        $(document)
            .off("click.rp.tile", `${SEL.catalog} .rp-tile`)
            .on("click.rp.tile", `${SEL.catalog} .rp-tile`, function () {
                const report = $(this).data("report");
                if (!report) return;
                switchReport(report);
            });
    }

    function switchReport(report) {
        currentReport = report;
        const titleMap = {
            asset: (CFG.i18n && CFG.i18n.assets) || "Assets",
            transfer: (CFG.i18n && CFG.i18n.transfers) || "Transfers",
            disposal: (CFG.i18n && CFG.i18n.disposals) || "Disposals",
            verification: (CFG.i18n && CFG.i18n.verifications) || "Verifications"
        };
        $(SEL.panelTitle).text(titleMap[report] || "Report");
        $(SEL.forms).addClass("d-none").filter(`[data-report="${report}"]`).removeClass("d-none");
        $(SEL.presetBtn).removeClass("active");
    }

    // Presets
    function bindPresets() {
        $(document)
            .off("click.rp.preset", SEL.presetBtn)
            .on("click.rp.preset", SEL.presetBtn, function () {
                const $btn = $(this);
                const preset = $btn.data("preset");
                if (!preset) return;
                applyPreset(currentReport, preset);
                $(SEL.presetBtn).removeClass("active");
                $btn.addClass("active");
            });
    }

    function applyPreset(report, preset) {
        const $form = $(`.rp-form[data-report="${report}"]`);
        if (!$form.length) return;

        const setSelect = (forName, val) => {
            const $sel = $form.find(`[name="${forName}"]`);
            if ($sel.length) $sel.val(String(val)).trigger("change");
        };

        if (preset === "all") {
            $form.find("select").each(function () {
                if ($(this).find('option[value="0"]').length) $(this).val("0").trigger("change");
            });
            return;
        }

        if (report === "asset") {
            if (preset === "byStore") setSelect("AssetStoreId", "0");
            else if (preset === "byStatus") setSelect("AssetStatusId", "0");
        } else if (report === "transfer") {
            if (preset === "byStore") { setSelect("StoreFromId", "0"); setSelect("StoreToId", "0"); }
            else if (preset === "byStatus") setSelect("TransferStatusId", "0");
        } else if (report === "disposal") {
            if (preset === "byStore") setSelect("DisposalStoreId", "0");
        }
    }

    // Actions
    function bindActions() {
        $(document)
            .off("click.rp.generate", SEL.generate)
            .on("click.rp.generate", SEL.generate, function (e) {
                e.preventDefault();
                submitCurrent("window");
            });

        $(document)
            .off("click.rp.menu", `${SEL.actionMenu} [data-action]`)
            .on("click.rp.menu", `${SEL.actionMenu} [data-action]`, function (e) {
                e.preventDefault();
                const action = $(this).data("action");
                submitCurrent(action);
            });
    }

    function submitCurrent(action) {
        const $form = $(`.rp-form[data-report="${currentReport}"]`);
        if (!$form.length) return;

        // Make a unique window name each time to avoid stale handles
        const winName = (action === "modal") ? "_blank"
            : `PrintWindow_${currentReport}_${Date.now()}_${Math.random().toString(36).slice(2)}`;

        $form.attr("target", winName);

        // If you support explicit export mode, append a temporary hidden input here
        // if (action === "export-pdf") {
        //   $('<input/>', { type:'hidden', name:'Export', value:'Pdf', class:'__tmp_export' }).appendTo($form);
        // }

        // IMPORTANT: Let the form submit open the popup (user gesture) → allowed by blockers
        $form.get(0).submit();

        // Cleanup temp fields if used
        // $form.find('.__tmp_export').remove();
    }

    // Search filter for tiles
    function bindSearch() {
        $(document)
            .off("input.rp.search", SEL.search)
            .on("input.rp.search", SEL.search, function () {
                const q = $(this).val().toString().toLowerCase().trim();
                const $tiles = $(`${SEL.catalog} .rp-tile`);
                if (!q) { $tiles.closest(".col-6").removeClass("d-none"); return; }
                $tiles.each(function () {
                    const text = $(this).text().toLowerCase();
                    $(this).closest(".col-6").toggleClass("d-none", !text.includes(q));
                });
            });
    }

    AMS.pages.register("Reporting/Index", init);

})(jQuery, window, document);
// /wwwroot/js/pages/reporting-index.v2.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    AMS.pages = AMS.pages || {};
    AMS.pages.register = AMS.pages.register || function (key, fn) {
        document.addEventListener("DOMContentLoaded", fn);
    };

    const CFG = window.REPORTING || {};
    const isSomali = String(CFG.isSomali).toLowerCase() === "true";

    const SEL = {
        catalog: "#rp-catalog",
        search: "#rp-search",
        panelTitle: "#rp-panel-title",
        presetBtn: ".rp-preset",
        generate: "#rp-generate",
        actionMenu: "#rp-action-menu",
        forms: ".rp-form",
        select2: ".select2"
    };

    let currentReport = "asset";

    function init() {
        initSelect2();
        bindCatalog();
        bindPresets();
        bindActions();
        bindSearch();
    }

    function initSelect2() {
        try { $(SEL.select2).select2({ dropdownParent: $(".rp-wrapper") }); } catch { }
    }

    // Catalog
    function bindCatalog() {
        $(document)
            .off("click.rp.tile", `${SEL.catalog} .rp-tile`)
            .on("click.rp.tile", `${SEL.catalog} .rp-tile`, function () {
                const report = $(this).data("report");
                if (!report) return;
                switchReport(report);
            });
    }

    function switchReport(report) {
        currentReport = report;
        const titleMap = {
            asset: (CFG.i18n && CFG.i18n.assets) || "Assets",
            transfer: (CFG.i18n && CFG.i18n.transfers) || "Transfers",
            disposal: (CFG.i18n && CFG.i18n.disposals) || "Disposals",
            verification: (CFG.i18n && CFG.i18n.verifications) || "Verifications"
        };
        $(SEL.panelTitle).text(titleMap[report] || "Report");
        $(SEL.forms).addClass("d-none").filter(`[data-report="${report}"]`).removeClass("d-none");
        $(SEL.presetBtn).removeClass("active");
    }

    // Presets
    function bindPresets() {
        $(document)
            .off("click.rp.preset", SEL.presetBtn)
            .on("click.rp.preset", SEL.presetBtn, function () {
                const $btn = $(this);
                const preset = $btn.data("preset");
                if (!preset) return;
                applyPreset(currentReport, preset);
                $(SEL.presetBtn).removeClass("active");
                $btn.addClass("active");
            });
    }

    function applyPreset(report, preset) {
        const $form = $(`.rp-form[data-report="${report}"]`);
        if (!$form.length) return;

        const setSelect = (forName, val) => {
            const $sel = $form.find(`[name="${forName}"]`);
            if ($sel.length) $sel.val(String(val)).trigger("change");
        };

        if (preset === "all") {
            $form.find("select").each(function () {
                if ($(this).find('option[value="0"]').length) $(this).val("0").trigger("change");
            });
            return;
        }

        if (report === "asset") {
            if (preset === "byStore") setSelect("AssetStoreId", "0");
            else if (preset === "byStatus") setSelect("AssetStatusId", "0");
        } else if (report === "transfer") {
            if (preset === "byStore") { setSelect("StoreFromId", "0"); setSelect("StoreToId", "0"); }
            else if (preset === "byStatus") setSelect("TransferStatusId", "0");
        } else if (report === "disposal") {
            if (preset === "byStore") setSelect("DisposalStoreId", "0");
        }
    }

    // Actions
    function bindActions() {
        $(document)
            .off("click.rp.generate", SEL.generate)
            .on("click.rp.generate", SEL.generate, function (e) {
                e.preventDefault();
                submitCurrent("window");
            });

        $(document)
            .off("click.rp.menu", `${SEL.actionMenu} [data-action]`)
            .on("click.rp.menu", `${SEL.actionMenu} [data-action]`, function (e) {
                e.preventDefault();
                const action = $(this).data("action");
                submitCurrent(action);
            });
    }

    function submitCurrent(action) {
        const $form = $(`.rp-form[data-report="${currentReport}"]`);
        if (!$form.length) return;

        // Make a unique window name each time to avoid stale handles
        const winName = (action === "modal") ? "_blank"
            : `PrintWindow_${currentReport}_${Date.now()}_${Math.random().toString(36).slice(2)}`;

        $form.attr("target", winName);

        // If you support explicit export mode, append a temporary hidden input here
        // if (action === "export-pdf") {
        //   $('<input/>', { type:'hidden', name:'Export', value:'Pdf', class:'__tmp_export' }).appendTo($form);
        // }

        // IMPORTANT: Let the form submit open the popup (user gesture) → allowed by blockers
        $form.get(0).submit();

        // Cleanup temp fields if used
        // $form.find('.__tmp_export').remove();
    }

    // Search filter for tiles
    function bindSearch() {
        $(document)
            .off("input.rp.search", SEL.search)
            .on("input.rp.search", SEL.search, function () {
                const q = $(this).val().toString().toLowerCase().trim();
                const $tiles = $(`${SEL.catalog} .rp-tile`);
                if (!q) { $tiles.closest(".col-6").removeClass("d-none"); return; }
                $tiles.each(function () {
                    const text = $(this).text().toLowerCase();
                    $(this).closest(".col-6").toggleClass("d-none", !text.includes(q));
                });
            });
    }

    AMS.pages.register("Reporting/Index", init);

})(jQuery, window, document);
