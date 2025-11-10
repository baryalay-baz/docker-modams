// ~/js/pages/admin/categories/categories-editsubcategory.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        $("#btnSubmit").on("click", onSubmit);

        // In Edit, we DO NOT auto-regenerate SubCategoryCode from the name.
        // Code stays as-is unless you explicitly change server logic.
    }

    function onSubmit(e) {
        e.preventDefault();
        const $form = $("#frmSubCategory");
        if ($form.length) $form.trigger("submit");
    }

    AMS.pages?.register?.("Categories/EditSubCategory", init);
})(window, document, jQuery);
