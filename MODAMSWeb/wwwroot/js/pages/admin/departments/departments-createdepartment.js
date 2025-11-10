// ~/js/pages/admin/departments/departments-createdepartment.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});
    const PAGES = (AMS.pages = AMS.pages || { register() { }, start() { } });

    function init() {
        try { U.hideMenu?.(); } catch { }

        // Robust, idempotent Select2 init under the form scope
        U.initSelect2?.({
            root: "#frmDepartment",
            selector: ".select2",
            dropdownParent: "#frmDepartment",
            width: "100%",
            allowClear: true
        });

        $("#btnSubmit").on("click", function (e) {
            e.preventDefault();
            $("#frmDepartment").trigger("submit");
        });
    }

    PAGES.register && PAGES.register("Departments/CreateDepartment", init);
})(window, document, jQuery);
