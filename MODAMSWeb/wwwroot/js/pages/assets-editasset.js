// wwwroot/js/pages/assets-editasset.js
(function (window, $) {
    "use strict";

    const CFG = window.AMS_EDIT_ASSET || {};
    const U = window.AMS?.util || {};

    const SEL = {
        frm: "#frmAsset",
        btnSubmit: "#btnSubmit",
        category: "#CategoryId",
        subCategory: "#SubCategoryId",
        vehicleBlock: "#dvVehicleIdentification",
        engine: "#Engine",
        chasis: "#Chasis",
        plate: "#Plate",
        select2: ".select2",
        datePickers: ".picker"
    };

    const VEHICLE_CATEGORY_ID = "16";

    // pull preloaded lists from Razor (same pattern as CreateAsset)
    const STATE = {
        categories: window.AMS?.pageData?.categories || [],
        subCategories: window.AMS?.pageData?.subCategories || []
    };

    // ---------- init ----------
    function init(/*ctx*/) {
        U.hideMenu?.();

        //// init Select2 for everything EXCEPT subCategory (we'll init that after we inject its options)
        //if ($.fn.select2) {
        //    $(SEL.select2).not(SEL.subCategory).each(function () {
        //        const $el = $(this);
        //        if ($el.data("select2")) $el.select2("destroy");
        //        $el.select2({ width: "resolve" });
        //    });
        //}

        // init datepickers
        if ($.fn.datepicker) {
            $(SEL.datePickers).datepicker({
                autoclose: true,
                format: "dd-MM-yyyy",
                todayHighlight: true
            });
        }

        // wire events
        $(SEL.btnSubmit).on("click", () => $(SEL.frm).submit());

        $(SEL.category).on("change", () => {
            applyVehicleVisibility();
            populateSubCategoryDropdown(); // rebuild subs for new category
        });

        // build dropdowns from STATE
        populateCategoryDropdown();
        populateSubCategoryDropdown();

        // set vehicle section state
        applyVehicleVisibility();
    }

    // ---------- category dropdown ----------
    function populateCategoryDropdown() {
        const $cat = $(SEL.category);
        const isSomali = (U.lang?.() || U.getCurrentLanguage?.() || "en") === "so";

        let html = `<option disabled selected>-${isSomali ? "Dooro Qayb" : "Select Category"
            }-</option>`;

        STATE.categories.forEach(c => {
            // expecting { Id, CategoryName, CategoryNameSo }
            const text = isSomali
                ? (c.CategoryNameSo ?? c.CategoryName)
                : c.CategoryName;

            html += `<option value="${c.Id}">${text}</option>`;
        });

        $cat.html(html);

        // restore saved category if editing
        if (CFG.categoryId != null) {
            $cat.val(String(CFG.categoryId));
        }

        // re-init Select2 for Category after replacing options
        if ($.fn.select2) {
            if ($cat.data("select2")) $cat.select2("destroy");
            $cat.select2({ width: "resolve" });
        }
    }

    // ---------- subCategory dropdown ----------
    function populateSubCategoryDropdown() {
        const $sub = $(SEL.subCategory);
        const isSomali = (U.lang?.() || U.getCurrentLanguage?.() || "en") === "so";

        const selectedCatId = String($(SEL.category).val() || "");

        const placeholderText = isSomali
            ? "Dooro Qayb-hoosaad"
            : "Select Sub-Category";

        // always start with placeholder
        let html = `<option value="" disabled selected>-${placeholderText}-</option>`;

        // filter subs by category
        STATE.subCategories
            .filter(sc => String(sc.CategoryId) === selectedCatId)
            .forEach(sc => {
                // expecting { Id, CategoryId, SubCategoryName, SubCategoryNameSo }
                const text = isSomali
                    ? (sc.SubCategoryNameSo ?? sc.SubCategoryName)
                    : sc.SubCategoryName;

                html += `<option value="${sc.Id}">${text}</option>`;
            });

        $sub.html(html);

        // restore saved subcategory (only if it belongs to current category)
        if (CFG.subCategoryId != null) {
            $sub.val(String(CFG.subCategoryId));
        } else {
            $sub.val("");
        }

        // init/re-init Select2 just for subcategory
        if ($.fn.select2) {
            if ($sub.data("select2")) $sub.select2("destroy");
            $sub.select2({
                width: "resolve",
                placeholder: placeholderText,
                allowClear: true
            });
        }
    }

    // ---------- vehicle block visibility ----------
    function applyVehicleVisibility() {
        const isVehicle = String($(SEL.category).val() || "") === VEHICLE_CATEGORY_ID;

        const $block = $(SEL.vehicleBlock);
        const $engine = $(SEL.engine);
        const $chasis = $(SEL.chasis);
        const $plate = $(SEL.plate);

        if (isVehicle) {
            // Vehicle selected
            $block
                .removeClass("vblock-disabled")   // normal look
                .show();                          // make sure it's visible

            // Enable editing
            $engine.prop("readonly", false);
            $chasis.prop("readonly", false);
            $plate.prop("readonly", false);

            // If fields are empty, load values from CFG (edit mode)
            if (!$engine.val()) $engine.val(CFG.engine || "");
            if (!$chasis.val()) $chasis.val(CFG.chasis || "");
            if (!$plate.val()) $plate.val(CFG.plate || "");

        } else {
            // Not a vehicle
            $block
                .addClass("vblock-disabled")  // greyed out
                .show();                      // KEEP IT VISIBLE

            // Put "-" in the fields
            $engine.val("-").prop("readonly", true);
            $chasis.val("-").prop("readonly", true);
            $plate.val("-").prop("readonly", true);
        }
    }


    // expose for debugging if you want in console
    window.AMS_EditAssetDebug = {
        populateCategoryDropdown,
        populateSubCategoryDropdown,
        applyVehicleVisibility
    };

    // hook into page-registry just like CreateAsset:
    // ViewData["TourPageKey"] for EditAsset view must be "Assets/EditAsset"
    window.AMS?.pages?.register?.("Assets/EditAsset", init);

})(window, window.jQuery);
