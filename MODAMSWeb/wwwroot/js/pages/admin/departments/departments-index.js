(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const PAGES = AMS.pages || (AMS.pages = { register() { }, start() { } });
    const U = AMS.util || (AMS.util = {});
    const SEL = { table: "#tblDepartments" };

    function initBootstrapUI() {
        if (!window.bootstrap) return;
        // Tooltips
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            try { new bootstrap.Tooltip(el); } catch { }
        });
        // Popovers (for stacked avatars)
        document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => {
            try { new bootstrap.Popover(el, { trigger: "hover", html: true, container: "body" }); } catch { }
        });
    }

    function initTable() {
        if (!U || typeof U.makeDataTable !== "function") return;

        const actionsConfig = {
            enable: true,
            paddingPx: 160,
            buttons: [
                {
                    key: "edit",
                    titleEn: "Edit Department",
                    titleSo: "Wax ka beddel Waaxda",
                    iconHtml: "<i class='fe fe-edit'></i>",
                    href: "/Admin/Departments/EditDepartment/{id}",
                    classes: "act-edit"
                },
                {
                    key: "users",
                    titleEn: "Store Users",
                    titleSo: "Adeegsadayaasha Dukaanka",
                    iconHtml: "<i class='fe fe-users'></i>",
                    href: "/Admin/Departments/DepartmentHeads/{id}",
                    classes: "act-info"
                }
            ]
        };

        const api = U.makeDataTable(SEL.table, "1", 10, actionsConfig);

        // Hide action pills for Ministry of Defense
        function applyRowFilters() {
            const $rows = api.rows({ page: "current" }).nodes().to$();
            $rows.each(function () {
                const $tr = $(this);
                const isMoD = $tr.attr("data-mod") === "1";
                if (isMoD) $tr.find(".row-actions").hide();
            });
        }

        if (api && api.on) {
            api.on("draw.dept", () => {
                initBootstrapUI();   // rebind after redraw (popovers)
                applyRowFilters();
            });
        }
        // first pass
        initBootstrapUI();
        applyRowFilters();
    }

    function init() {
        try { U.hideMenu?.(); } catch { }
        initTable();
    }

    PAGES.register && PAGES.register("Departments/Index", init);

})(jQuery, window, document);
