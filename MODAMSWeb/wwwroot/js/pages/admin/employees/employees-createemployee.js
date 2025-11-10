// ~/js/pages/admin/employees/employees-createemployee.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        // Enhance selects
        if ($.fn.select2) {
            $(".select2").each(function () {
                if (!$(this).data("select2")) $(this).select2({ width: "resolve" });
            });
        }

        // Submit handler
        $("#btnSubmit").on("click", function (e) {
            e.preventDefault();
            $("#frmEmployee").trigger("submit");
        });

        // Optional: build a lightweight avatar preview if Email present
        attachAvatarPreview();
    }

    function attachAvatarPreview() {
        const $email = $("#Employee_Email");
        const $target = $("#profile-image");
        if (!$email.length || !$target.length) return;

        const render = U.debounce(() => {
            const val = ($email.val() || "").toString().trim();
            if (!val) {
                $target.html("");
                return;
            }
            // Gravatar-style placeholder (no external calls) — just initials
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
