// wwwroot/js/pages/assets-editasset.js
// PAMS • Assets/EditAsset page logic (jQuery)
// Expects window.PAMS_EDIT_ASSET to be defined in the view (values from Razor)

(function (window, $) {
    "use strict";

    const CFG = window.PAMS_EDIT_ASSET || {};
    // expected: { engine, chasis, plate, categoryId, subCategoryId }

    const Utils = window.PAMS?.util || {};

    // ---- Selectors
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

    // ---- Constants
    const VEHICLE_CATEGORY_ID = "16"; // if CategoryId == 16 => show vehicle identification

    // ---- In-memory state
    const STATE = {
        categories: [],      // [{ Id, CategoryCode, CategoryName, CategoryNameSo }]
        subCategories: []    // [{ Id, CategoryId, SubCategoryName, SubCategoryNameSo }]
    };

    // ---- Init
    Utils.ready(function () {
        Utils.hideMenu?.();

        // Init global plugins (do NOT init #SubCategoryId here; we re-init it when we rebuild options)
        if ($.fn.select2) $(SEL.select2).not(SEL.subCategory).select2();
        if ($.fn.datepicker) {
            $(SEL.datePickers).datepicker({
                autoclose: true,
                format: "dd-MM-yyyy",
                todayHighlight: true
            });
        }

        // Events
        $(SEL.btnSubmit).on("click", () => $(SEL.frm).submit());
        $(SEL.category).on("change", () => {
            checkIfVehiclesSelected();
            populateCmbSubCategory(); // rebuild subs for the newly picked category
        });

        // Load pipeline
        loadCategories();

        // Initial vehicle section visibility
        checkIfVehiclesSelected();
    });

    // ---- Vehicle toggling
    function checkIfVehiclesSelected() {
        const isVehicle = String($(SEL.category).val() || "") === VEHICLE_CATEGORY_ID;

        if (isVehicle) {
            $(SEL.vehicleBlock).show();
            if (!$(SEL.engine).val()) $(SEL.engine).val(CFG.engine || "");
            if (!$(SEL.chasis).val()) $(SEL.chasis).val(CFG.chasis || "");
            if (!$(SEL.plate).val()) $(SEL.plate).val(CFG.plate || "");
        } else {
            $(SEL.engine).val("-");
            $(SEL.chasis).val("-");
            $(SEL.plate).val("-");
            $(SEL.vehicleBlock).hide();
        }
    }

    // ---- Helpers: normalize arrays from responses
    function toArrayFromResponse(response, label) {
        // Accept raw array
        if (Array.isArray(response)) return response;

        // Accept { success, data: ... }
        if (response && "data" in response) {
            const d = response.data;

            if (Array.isArray(d)) return d;

            if (typeof d === "string") {
                const parsed = Utils.tryParseJson?.(d, label) || { status: "error" };
                if (parsed.status === "success" && Array.isArray(parsed.data)) return parsed.data;
            }
        }

        return []; // fallback
    }

    // ---- AJAX: Categories
    function loadCategories() {
        $.ajax({
            url: "/Users/Assets/GetCategories",
            dataType: "json",
            success: function (response) {
                const arr = toArrayFromResponse(response, "Categories");
                if (arr.length) {
                    STATE.categories = arr;
                    populateCmbCategory();
                    loadSubCategories();
                } else {
                    Utils.showErrorMessageJs?.(response?.message || "Failed to load categories.");
                    Utils.log?.("GetCategories unexpected shape:", response);
                }
            },
            error: function (_xhr, _status, error) {
                Utils.showErrorMessageJs?.(`Error loading categories data: ${error}`);
            }
        });
    }

    // ---- AJAX: SubCategories
    function loadSubCategories(id) {
        const data = id ? { id } : {};
        $.ajax({
            url: "/Users/Assets/GetSubCategories",
            dataType: "json",
            data,
            success: function (response) {
                const arr = toArrayFromResponse(response, "SubCategories");
                if (arr.length) {
                    STATE.subCategories = arr;
                    populateCmbSubCategory();
                } else {
                    Utils.showErrorMessageJs?.(response?.message || "Failed to load sub-categories.");
                    Utils.log?.("GetSubCategories unexpected shape:", response);
                }
            },
            error: function (_xhr, _status, error) {
                Utils.showErrorMessageJs?.(`Error loading sub-categories data: ${error}`);
            }
        });
    }

    // ---- Build Category <select> from STATE.categories
    function populateCmbCategory() {
        const isSomali = (Utils.lang?.() || Utils.getCurrentLanguage?.() || "en") === "so";

        let html = `<option selected disabled>-${isSomali ? "Dooro Qayb" : "Select Category"}-</option>`;
        for (const e of STATE.categories) {
            const text = isSomali ? (e.CategoryNameSo ?? e.CategoryName) : e.CategoryName;
            html += `<option value="${e.Id}">${text}</option>`;
        }

        const $cat = $(SEL.category);
        $cat.html(html);

        // Restore chosen category if provided (after options exist)
        if (CFG.categoryId != null) {
            $cat.val(String(CFG.categoryId));
        }

        // Reflect vehicle section visibility
        checkIfVehiclesSelected();
    }

    // ---- Build SubCategory <select> filtered by current category
    function populateCmbSubCategory() {
        const isSomali = (Utils.lang?.() || Utils.getCurrentLanguage?.() || "en") === "so";
        const selectedCatId = String($(SEL.category).val() || "");

        // Filter rows for the selected category
        const filtered = STATE.subCategories.filter(r => String(r.CategoryId) === selectedCatId);

        const placeholderText = isSomali ? "Dooro Qayb-hoosaad" : "Select Sub-Category";
        let html = `<option value="" disabled selected>-${placeholderText}-</option>`;
        for (const e of filtered) {
            const text = isSomali ? (e.SubCategoryNameSo ?? e.SubCategoryName) : e.SubCategoryName;
            html += `<option value="${e.Id}">${text}</option>`;
        }

        const $sub = $(SEL.subCategory);

        // Replace options
        $sub.html(html);

        // Re-init Select2 for THIS element only
        if ($.fn.select2) {
            if ($sub.data("select2")) $sub.select2("destroy");
            $sub.select2({ placeholder: placeholderText, allowClear: true });
        }

        // Reset or reapply known value (edit mode)
        if (CFG.subCategoryId != null) {
            $sub.val(String(CFG.subCategoryId)).trigger("change");
        } else {
            $sub.val("").trigger("change");
        }
    }

    // ---- Optional: expose some functions for debugging
    window.PAMS_EditAsset = {
        reloadCategories: loadCategories,
        populateCmbCategory,
        populateCmbSubCategory,
        checkIfVehiclesSelected
    };

})(window, window.jQuery);
