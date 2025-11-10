// /wwwroot/js/pages/admin/categories/categories-createsubcategory.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        $("#btnSubmit").on("click", onSubmit);

        $("#SubCategoryName")
            .on("input", generateSubCategoryCode)
            .on("change", generateSubCategoryCode);

        // initial generation if a name is prefilled (rare, but safe)
        generateSubCategoryCode();
    }

    function onSubmit(e) {
        e.preventDefault();
        const $form = $("#frmSubCategory");
        if ($form.length) $form.trigger("submit");
    }

    function generateSubCategoryCode() {
        const $name = $("#SubCategoryName");
        const $code = $("#SubCategoryCode");
        if (!$name.length || !$code.length) return;

        const base = ($code.data("base-code") || "").toString().trim(); // e.g., "MOD-ABCD"
        const raw = ($name.val() || "").toString().trim();

        if (!base) {
            // If the base is missing, don't invent it; keep whatever is there.
            return;
        }

        if (!raw) {
            // If no name, show only the base (or leave as-is). Choose base for clarity.
            $code.val(base);
            return;
        }

        // sanitize: letters only from the subcategory name
        const lettersOnly = raw.replace(/[^a-zA-Z]/g, "");
        let part = lettersOnly.substring(0, 4).toUpperCase();
        part = part.padEnd(4, "X"); // always 4 chars for consistency

        // If base already has a suffix (e.g., "MOD-ABCD-..."), strip to the base segment before appending.
        const baseCore = base.split("-").slice(0, 2).join("-"); // "MOD-ABCD"
        $code.val(`${baseCore}-${part}`);
    }

    AMS.pages?.register?.("Categories/CreateSubCategory", init);
})(window, document, jQuery);
