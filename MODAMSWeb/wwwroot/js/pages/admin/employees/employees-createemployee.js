// ~/js/pages/admin/employees/employees-createemployee.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        // Initialize all .select2 inside the form, with dropdown attached to <body>
        U.initSelect2({ root: "#frmEmployee", dropdownParent: document.body });

        bindSubmit();
        attachAvatarPreview();
    }

    function bindSubmit() {
        $("#btnSubmit").on("click", function (e) {
            e.preventDefault();
            $("#frmEmployee").trigger("submit");
        });
    }

    // Simple initials avatar from email (no external calls)
    function attachAvatarPreview() {
        const $email = $("#Employee_Email");
        const $target = $("#profile-image");
        if (!$email.length || !$target.length) return;

        const render = U.debounce(() => {
            const val = ($email.val() || "").toString().trim();
            if (!val) {
                $target.empty();
                return;
            }
            const initials = val.split("@")[0].slice(0, 2).toUpperCase();
            const html = `
        <div class="d-flex align-items-center justify-content-center"
             style="width:140px;height:140px;border-radius:50%;
                    border:1px solid rgba(0,0,0,.08);
                    background: rgba(0,0,0,.03); font-weight:700; font-size:42px;">
          ${U.escapeHtml(initials)}
        </div>`;
            $target.html(html);
        }, 150);

        $email.on("input change", render);
        render();
    }

    AMS.pages?.register?.("Employees/CreateEmployee", init);
})(window, document, jQuery);
