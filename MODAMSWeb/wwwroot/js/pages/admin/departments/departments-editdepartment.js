// ~/js/pages/admin/departments/departments-editdepartment.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});
    const PAGES = (AMS.pages = AMS.pages || { register() { }, start() { } });

    function init() {
        try { U.hideMenu?.(); } catch { }

        // Idempotent Select2 init scoped to the form
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

    PAGES.register && PAGES.register("Departments/EditDepartment", init);
})(window, document, jQuery);
