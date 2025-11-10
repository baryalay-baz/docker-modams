(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        try { U.hideMenu?.(); } catch { }

        // Submit -> post the form
        $("#btnSubmit").on("click", function (e) {
            e.preventDefault();
            $("#frmDonor").trigger("submit");
        });

        // (Optional) quick UX: trim text inputs on blur
        $("#frmDonor input[type='text']").on("blur", function () {
            const v = (this.value || "").trim();
            if (this.value !== v) this.value = v;
        });
    }

    AMS.pages?.register?.("Donors/EditDonor", init);
})(window, document, jQuery);
