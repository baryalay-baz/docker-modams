// /wwwroot/js/pages/admin/employees/employees-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = { table: "#tblEmployees" };

    function init() {
        U.hideMenu?.();
        initDataTable();
    }

    function initDataTable() {
        const selector = SEL.table;

        // Define overlay actions (like Assets page)
        const actions = {
            enable: true,
            paddingPx: 140,
            buttons: [
                {
                    key: "edit",
                    titleEn: "Edit Employee",
                    titleSo: "Wax ka beddel Shaqaalaha",
                    iconHtml: "<i class='fe fe-edit'></i>",
                    href: "/Admin/Employees/EditEmployee/{id}"
                },
                {
                    key: "lock",
                    titleEn: "Lock Account",
                    titleSo: "Xir Akoonka",
                    iconHtml: "<i class='bi bi-lock-fill' aria-hidden='true'></i>",
                    variant: "danger",
                    onClick: ({ id }) => {
                        if (!id) return;
                        window.location = `/Admin/Employees/LockAccount/${encodeURIComponent(id)}`;
                    }
                },
                {
                    key: "unlock",
                    titleEn: "Unlock Account",
                    titleSo: "Fur Akoonka",
                    iconHtml: "<i class='bi bi-unlock-fill' aria-hidden='true'></i>",
                    variant: "success",
                    onClick: ({ id }) => {
                        if (!id) return;
                        window.location = `/Admin/Employees/UnlockAccount/${encodeURIComponent(id)}`;
                    }
                }
            ]
        };

        const api = U.makeDataTable(selector, "2", 10, actions);

        if (!api) return;

        // Hide lock/unlock per-row based on data-active
        const adjustLockButtons = () => {
            const $tbl = $(selector);
            api.rows({ page: "current" }).every(function () {
                const node = this.node();
                const $tr = $(node);
                const isActive = $tr.data("active") === 1 || $tr.data("active") === "1";

                // find the overlay appended to this row
                const $overlay = $tr.children(".row-actions");
                if (!$overlay.length) return;

                const $lock = $overlay.find(".act-lock");
                const $unlock = $overlay.find(".act-unlock");

                if (isActive) {
                    $lock.show();
                    $unlock.hide();
                } else {
                    $lock.hide();
                    $unlock.show();
                }
            });
        };

        api.on("draw._employeesLockToggle", adjustLockButtons);
        adjustLockButtons();
    }

    AMS.pages?.register?.("Employees/Index", init);
})(jQuery, window, document);
