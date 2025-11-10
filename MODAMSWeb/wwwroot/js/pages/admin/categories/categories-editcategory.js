// /wwwroot/js/pages/admin/categories/categories-editcategory.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        $("#btnSubmit").on("click", onSubmit);

        // Keep code-generation parity with CreateCategory
        $("#CategoryName")
            .on("input", generateCategoryCode)
            .on("change", generateCategoryCode);
    }

    function onSubmit(e) {
        e.preventDefault();
        const $form = $("#frmCategory");
        if ($form.length) $form.trigger("submit");
    }

    function generateCategoryCode() {
        const $name = $("#CategoryName");
        const $code = $("#CategoryCode");
        if (!$name.length || !$code.length) return;

        const raw = String($name.val() || "").trim();
        if (!raw) {
            // keep current code if empty? Decide: here we clear to be explicit.
            $code.val("");
            return;
        }

        const lettersOnly = raw.replace(/[^a-zA-Z]/g, "");
        let part = lettersOnly.substring(0, 4).toUpperCase();
        part = part.padEnd(4, "X"); // always 4 chars
        $code.val(`MOD-${part}`);
    }

    AMS.pages?.register?.("Categories/EditCategory", init);
})(window, document, jQuery);
