// /wwwroot/js/pages/admin/categories/categories-createcategory.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        // Keep layout consistent with list pages
        U.hideMenu?.();

        // Wire actions
        $("#btnSubmit").on("click", onSubmit);
        $("#CategoryName")
            .on("input", generateCategoryCode)
            .on("change", generateCategoryCode)
            .trigger("change"); // generate once if prefills exist
    }

    function onSubmit(e) {
        e.preventDefault();
        const $form = $("#frmCategory");
        if (!$form.length) return;
        $form.trigger("submit");
    }

    function generateCategoryCode() {
        const $name = $("#CategoryName");
        const $code = $("#CategoryCode");
        if (!$name.length || !$code.length) return;

        const raw = String($name.val() || "").trim();
        if (!raw) {
            $code.val("");
            return;
        }

        // Keep logic consistent and predictable:
        // - Strip non-letters
        // - Take first 4 letters
        // - Pad to 4 with 'X' if shorter
        // - Prefix with "MOD-"
        const lettersOnly = raw.replace(/[^a-zA-Z]/g, "");
        let part = lettersOnly.substring(0, 4).toUpperCase();
        part = part.padEnd(4, "X"); // was inconsistent before; now always 4
        $code.val(`MOD-${part}`);
    }

    AMS.pages?.register?.("Categories/CreateCategory", init);
})(window, document, jQuery);
