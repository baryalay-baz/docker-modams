//  /js/pages/reporting-index.js
(function ($, window, document) {
    "use strict";

    // Use your utils namespace (alias)
    const AMS = window.AMS || {};
    const U = AMS?.util || {};

    const CFG = window.REPORTING || {};
    const SEL = {
        catalog: "#rp-catalog",
        search: "#rp-search",
        panelTitle: "#rp-panel-title",
        presetBtn: ".rp-preset",
        generate: "#rp-generate",
        actionMenu: "#rp-action-menu",
        forms: ".rp-form",
        select2: ".select2",
        previewFrame: "#rpPreviewFrame",
        empty: "#rpEmpty"
    };

    let currentReport = "asset";
    let currentPreset = "all";

    function init() {
        U.hideMenu();

        initSelect2();
        bindCatalog();
        bindPresets();
        bindActions();
        bindSearch();

        // Apply role-aware defaults once
        applyRoleDefaults();

        setActiveTile(currentReport);

        // Initial validity check
        updateGenerateState();
    }

    function initSelect2() {
        try {
            $(SEL.select2).each(function () {
                $(this).select2({
                    width: "100%",
                    dropdownParent: $(".rp-wrapper")
                });
            });
        } catch { /* ignore */ }

        // Re-validate on any change
        $(document).on("change input", `${SEL.forms} select`, updateGenerateState);
    }

    // ===== Catalog (tiles) =====
    function bindCatalog() {
        $(document)
            .off("click.rp.tile", `${SEL.catalog} .rp-tile`)
            .on("click.rp.tile", `${SEL.catalog} .rp-tile`, function () {
                const report = $(this).data("report");
                if (!report || report === "verification") return;

                currentReport = report;
                switchReport(report);
                setActiveTile(report); // NEW: visual + ARIA
            });
    }

    // NEW: centralize selected-state styling + ARIA
    function setActiveTile(report) {
        const $tiles = $(`${SEL.catalog} .rp-tile`);
        $tiles.removeClass("is-active").attr("aria-pressed", "false");
        const $active = $tiles.filter(`[data-report="${report}"]`);
        $active.addClass("is-active").attr("aria-pressed", "true");

        // Ensure the active tile is visible if search is filtering
        $active.closest(".col-6").removeClass("d-none");
    }

    function switchReport(report) {
        const titleMap = {
            asset: "Assets",
            transfer: "Transfers",
            disposal: "Disposals",
            verification: "Verifications"
        };
        $(SEL.panelTitle).text(titleMap[report] || "Report");

        $(SEL.forms).addClass("d-none").filter(`[data-report="${report}"]`).removeClass("d-none");

        // Reset preset visuals (keep currentPreset value)
        $(SEL.presetBtn).removeClass("active");

        // Re-apply required fields for the existing preset on the new form
        enforcePresetRequirements();

        // Update button state
        updateGenerateState();
    }

    // ===== Presets =====
    function bindPresets() {
        $(document)
            .off("click.rp.preset", SEL.presetBtn)
            .on("click.rp.preset", SEL.presetBtn, function () {
                const $btn = $(this);
                currentPreset = $btn.data("preset") || "all";

                $(SEL.presetBtn).removeClass("active");
                $btn.addClass("active");

                // Force fields per preset
                enforcePresetRequirements();
                updateGenerateState();
            });
    }

    function enforcePresetRequirements() {
        const $form = $(`.rp-form[data-report="${currentReport}"]`);
        if (!$form.length) return;

        // Clear required state
        $form.find("select").each(function () {
            $(this).prop("required", false).removeClass("is-required is-invalid").attr("aria-invalid", "false");
            // If we previously forced blank, put them back to "All"
            const allVal = $(this).data("all");
            if (allVal !== undefined && (this.dataset.forcedBlank === "1")) {
                $(this).val(String(allVal)).trigger("change");
                delete this.dataset.forcedBlank;
            }
        });

        // Apply the simple Tier-1 rules:
        // - "all": nothing required
        // - "byStore": require store selects of the active report
        // - "byStatus": require status selects of the active report
        if (currentPreset === "byStore") {
            requireIfExists($form, ["AssetStoreId", "StoreFromId", "StoreToId", "DisposalStoreId"]);
        } else if (currentPreset === "byStatus") {
            requireIfExists($form, ["AssetStatusId", "TransferStatusId"]);
        }
    }

    function requireIfExists($form, names) {
        names.forEach(n => {
            const $sel = $form.find(`[name="${n}"]`);
            if ($sel.length) {
                $sel.prop("required", true).addClass("is-required");
                const allVal = String($sel.data("all") ?? "");
                if ($sel.val() === allVal) {
                    // nudge user off "All" by blanking; Select2 will show empty
                    $sel.val("").trigger("change");
                    $sel[0].dataset.forcedBlank = "1";
                }
            }
        });
    }

    // ===== Actions =====
    function bindActions() {
        // Preview inline (iframe)
        $(document)
            .off("click.rp.generate", SEL.generate)
            .on("click.rp.generate", SEL.generate, function (e) {
                e.preventDefault();
                const $form = currentForm();
                if (!$form.length) return;

                // safety
                if (!isFormValid($form)) return;

                // Ensure target is iframe (inline preview)
                $form.attr("target", "rpPreviewFrame");
                $form.find('input[name="Mode"]').val("Window"); // your viewer can ignore/handle

                // show preview pane
                $(SEL.empty).hide();
                $(SEL.previewFrame).show();

                $form[0].submit();
            });

        // Menu actions
        $(document)
            .off("click.rp.menu", `${SEL.actionMenu} [data-action]`)
            .on("click.rp.menu", `${SEL.actionMenu} [data-action]`, function (e) {
                e.preventDefault();
                const action = $(this).data("action");
                const $form = currentForm();
                if (!$form.length) return;

                if (action === "export-pdf") {
                    // export in new tab
                    $form.attr("target", "_blank");
                    $form.find('input[name="Mode"]').val("Pdf");
                    $form[0].submit();
                    // restore
                    $form.attr("target", "rpPreviewFrame");
                    $form.find('input[name="Mode"]').val("Window");
                } else if (action === "window") {
                    // Open normal page in new tab
                    $form.attr("target", "_blank");
                    $form.find('input[name="Mode"]').val("Window");
                    $form[0].submit();
                    $form.attr("target", "rpPreviewFrame");
                }
            });
    }

    // ===== Search tiles =====
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

    // ===== Role-aware defaults =====
    function applyRoleDefaults() {
        const id = parseInt(CFG.defaultStoreId || 0, 10);
        if (!id) return;

        // For any store-related select, if it's currently "All"/0, set to user default.
        const storeNames = ["AssetStoreId", "StoreFromId", "StoreToId", "DisposalStoreId"];
        storeNames.forEach(n => {
            const $selAll = $(`[name="${n}"]`);
            if ($selAll.length) {
                $selAll.each(function () {
                    const $s = $(this);
                    const v = String($s.val() ?? "");
                    if (v === "0" || v === "") {
                        // only set if option exists
                        if ($s.find(`option[value="${id}"]`).length) {
                            $s.val(String(id)).trigger("change");
                        }
                    }
                });
            }
        });
    }

    // ===== Validity / Button state =====
    function currentForm() {
        return $(`.rp-form[data-report="${currentReport}"]`);
    }

    function isFormValid($form) {
        let ok = true;
        $form.find("select[required]").each(function () {
            const v = String($(this).val() ?? "").trim();
            const bad = !v || v === "0";
            $(this).toggleClass("is-invalid", bad).attr("aria-invalid", bad ? "true" : "false");
            if (bad) ok = false;
        });
        return ok;
    }

    function updateGenerateState() {
        const $form = currentForm();
        const ok = isFormValid($form);
        $(SEL.generate).prop("disabled", !ok);
    }

    // boot
    //document.addEventListener("DOMContentLoaded", init);
    window.AMS.pages.register("Reporting/Index", init);

})(jQuery, window, document);
