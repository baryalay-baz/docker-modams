// ~/js/pages/admin/employees/employees-index.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    function init() {
        U.hideMenu?.();

        // Build hover actions: Edit + Lock/Unlock (single toggle button)
        const actions = {
            enable: true,
            paddingPx: 160,
            buttons: [
                {
                    key: "edit",
                    titleEn: "Edit Employee",
                    titleSo: "Tafatir Shaqaale",
                    iconHtml: "<i class='fe fe-edit'></i>",
                    href: "/Admin/Employees/EditEmployee/{id}",
                    className: "btn-outline-primary"
                },
                {
                    key: "toggle",
                    titleEn: "Lock / Unlock",
                    titleSo: "Quful / Furo",
                    iconHtml: "<i class='fe fe-lock'></i>",
                    className: "btn-outline-danger",
                    onClick: ({ id, event }) => {
                        // Decide endpoint by row's data-active flag
                        const $tr = $(event.target).closest("tr");
                        const isActive = String($tr.data("active")) === "true";
                        const url = isActive
                            ? `/Admin/Employees/LockAccount/${id}`
                            : `/Admin/Employees/UnlockAccount/${id}`;
                        w.location = url;
                    }
                }
            ]
        };

        // Type "2" → export buttons visible (copy/excel/pdf); tweak as you like
        U.makeDataTable("#tblEmployees", "2", 10, actions);
    }

    AMS.pages?.register?.("Employees/Index", init);
})(window, document, jQuery);
