// /wwwroot/js/pages/assets-createasset.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    // in-memory state from Razor-injected pageData
    const state = {
        categories: window.AMS?.pageData?.categories || [],
        subCategories: window.AMS?.pageData?.subCategories || [],
    };

    const SEL = {
        category: "#CategoryId",
        subCategory: "#SubCategoryId",
        vehicleBlock: "#dvVehicleIdentification",
        engine: "#Engine",
        chasis: "#Chasis",
        plate: "#Plate",
        barcode: "#Barcode",
        form: "#frmAsset",
        submitBtn: "#btnSubmit",
        pickers: ".picker",
    };

    // ============= INIT =============
    async function init(/*ctx*/) {
        if (!U) {
            console.error("[AMS] utils not loaded.");
            return;
        }

        U.hideMenu?.();

        // submit button
        $(SEL.submitBtn).on("click", function () {
            $(SEL.form).submit();
        });

        // category change -> repopulate subcategory, refresh select2, update barcode
        $(SEL.category).on("change", async function () {
            handleCategoryChanged();
            await generateBarcode();
        });

        // subcategory change -> update barcode
        $(SEL.subCategory).on("change", async function () {
            await generateBarcode();
        });

        // datepickers
        initDatePickers();

        // hydrate the dropdowns from state (no AJAX now)
        populateCategoryDropdown();
        populateSubCategoryDropdown();     // placeholder only at first load
        ensureSelect2Initialized();        // first-time init for both selects

        // show/hide vehicle block based on current category
        applyVehicleVisibility();
    }

    // ============= DROPDOWN POPULATION =============
    function populateCategoryDropdown() {
        const $cat = $(SEL.category);
        const isSomali = (U.getCurrentLanguage?.() || "en") === "so";

        let html = `<option selected disabled>-${isSomali ? "Dooro Qayb" : "Select Category"
            }-</option>`;

        state.categories.forEach(c => {
            // expect c.Id, c.CategoryName, c.CategoryNameSo
            html += `<option value="${c.Id}">${isSomali ? c.CategoryNameSo : c.CategoryName
                }</option>`;
        });

        $cat.html(html);
    }

    function populateSubCategoryDropdown() {
        const $sub = $(SEL.subCategory);
        const isSomali = (U.getCurrentLanguage?.() || "en") === "so";
        const selectedCategoryId = $(SEL.category).val();

        // always start with placeholder
        let html = `<option disabled selected>-${isSomali ? "Dooro Qayb-hoosaad" : "Select Sub-Category"
            }-</option>`;

        if (selectedCategoryId) {
            state.subCategories
                .filter(sc => String(sc.CategoryId) === String(selectedCategoryId))
                .forEach(sc => {
                    // expect sc.Id, sc.CategoryId, sc.SubCategoryName, sc.SubCategoryNameSo
                    html += `<option value="${sc.Id}">${isSomali ? sc.SubCategoryNameSo : sc.SubCategoryName
                        }</option>`;
                });
        }

        $sub.html(html);
    }

    // user picked a category
    function handleCategoryChanged() {
        applyVehicleVisibility();
        populateSubCategoryDropdown();
        refreshSubCategorySelect2(); 
    }

    // ============= VEHICLE SECTION VISIBILITY =============
    function applyVehicleVisibility() {
        const isVehicle = String($(SEL.category).val() || "") === "16";

        const $block = $(SEL.vehicleBlock);
        const $engine = $(SEL.engine);
        const $chasis = $(SEL.chasis);
        const $plate = $(SEL.plate);

        if (isVehicle) {
            // --- Vehicle selected ---

            // visual state
            $block
                .removeClass("vblock-disabled")
                .show();

            // inputs become editable
            $engine.prop("readonly", false);
            $chasis.prop("readonly", false);
            $plate.prop("readonly", false);

            // if user previously had "-" because it was non-vehicle,
            // we can clear it to be nice (optional, but UX-friendly):
            if ($engine.val() === "-") $engine.val("");
            if ($chasis.val() === "-") $chasis.val("");
            if ($plate.val() === "-") $plate.val("");

        } else {
            // --- NOT a vehicle ---

            // visual state: keep it visible but "disabled"
            $block
                .addClass("vblock-disabled")
                .show();

            // lock inputs and set values to "-"
            $engine.val("-").prop("readonly", true);
            $chasis.val("-").prop("readonly", true);
            $plate.val("-").prop("readonly", true);
        }
    }


    // ============= BARCODE GEN =============
    async function generateBarcode() {
        const categoryId = $(SEL.category).val();
        const subCategoryId = $(SEL.subCategory).val();

        if (!categoryId || !subCategoryId) {
            applyVehicleVisibility();
            return;
        }

        const catPadded = String(categoryId).padStart(3, "0");
        const subPadded = String(subCategoryId).padStart(3, "0");

        const lastAssetId = await getLastAssetId();
        const newAssetId = lastAssetId + 1;
        const barcode = `MOD-${catPadded}-${subPadded}-${newAssetId}`;

        $(SEL.barcode).val(barcode);

        applyVehicleVisibility();
    }

    async function getLastAssetId() {
        try {
            const lastAssetId = await U.fetchJson("/Users/Assets/GetLastAssetId");
            const n = Number(lastAssetId);

            if (!Number.isFinite(n)) {
                console.error("[AMS] getLastAssetId: response was not a number", lastAssetId);
                return 0;
            }
            return n;
        } catch (err) {
            console.error("[AMS] Error fetching last asset ID:", err);
            return 0;
        }
    }

    // ============= UI HELPERS =============
    function initDatePickers() {
        if ($.fn && $.fn.datepicker) {
            $(SEL.pickers).datepicker({
                autoclose: true,
                format: "dd-MM-yyyy",
                todayHighlight: true,
            });
        } else {
            console.warn("[AMS] bootstrap-datepicker not found; skipping .picker datepicker init");
        }
    }

    // first-time init for BOTH selects
    function ensureSelect2Initialized() {
        if (!$.fn.select2) {
            U.log?.("[AMS] Select2 not present; skipping init.");
            return;
        }

        $(".select2").each(function () {
            const $el = $(this);

            if ($el.data("select2")) {
                $el.select2("destroy");
            }

            $el.select2({
                width: "resolve",
            });
        });
    }

    // re-init ONLY subcategory select2 after repopulating its <option>s
    function refreshSubCategorySelect2() {
        if (!$.fn.select2) {
            U.log?.("[AMS] Select2 not present; skipping refreshSubCategorySelect2.");
            return;
        }

        const $sub = $(SEL.subCategory);

        if ($sub.data("select2")) {
            $sub.select2("destroy");
        }

        $sub.select2({
            width: "resolve",
        });
    }

    // ============= REGISTER PAGE =============
    AMS.pages?.register?.("Assets/CreateAsset", init);

})(jQuery, window, document);
