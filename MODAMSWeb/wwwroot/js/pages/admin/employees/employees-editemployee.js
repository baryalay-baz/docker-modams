// ~/js/pages/admin/employees/employees-editemployee.js
(function (w, d, $) {
    "use strict";
    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        // Enhance selects (idempotent)
        if ($.fn.select2) {
            $(".select2").each(function () {
                if (!$(this).data("select2")) $(this).select2({ width: "resolve" });
            });
        } else {
            U.log?.("Select2 not present; skipping.");
        }

        // Submit
        $("#btnSubmit").on("click", function (e) {
            e.preventDefault();
            $("#frmEmployee").trigger("submit");
        });
    }

    AMS.pages?.register?.("Employees/EditEmployee", init);
})(window, document, jQuery);
