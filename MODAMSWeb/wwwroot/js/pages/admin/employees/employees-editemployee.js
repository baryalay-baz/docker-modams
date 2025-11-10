// ~/js/pages/admin/employees/employees-editemployee.js
(function (w, d, $) {
    "use strict";
    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        // Enhance selects (idempotent)

        U.select2?.({ root: "#frmEmployee", dropdownParent: document.body });


        // Submit
        $("#btnSubmit").on("click", function (e) {
            e.preventDefault();
            $("#frmEmployee").trigger("submit");
        });
    }

    AMS.pages?.register?.("Employees/EditEmployee", init);
})(window, document, jQuery);
