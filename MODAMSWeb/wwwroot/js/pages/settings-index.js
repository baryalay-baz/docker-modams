// /wwwroot/js/pages/settings-index.js
(function (w, d, $) {
    "use strict";

    const AMS = w.AMS || (w.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const PAGE_KEY = "Settings/Index";
    const NS = ".settingsIndex";

    const SEL = {
        // FIX: Razor IDs are PascalCase
        cmbMonth: "#SelectedMonth",
        cmbYear: "#SelectedYear",

        // tables
        loginHistoryTable: "#tblLoginHistory",
        auditLogTable: "#tblAuditLog",
        deletedAssetsTable: "#tblDeletedAssets",

        // accordions
        accHeader: ".js-acc-header",
        accToggle: ".js-acc-toggle",
        accIcon: ".js-acc-icon"     
    };

    function go(monthVal, yearVal) {
        const m = monthVal ?? "";
        const y = yearVal ?? "";
        w.location.href = `/Admin/Settings/Index?nMonth=${encodeURIComponent(m)}&nYear=${encodeURIComponent(y)}`;
    }

    function bindFilters() {
        U.off(NS, "change", SEL.cmbMonth);
        U.off(NS, "change", SEL.cmbYear);

        U.on(NS, "change", SEL.cmbMonth, function () {
            go($(this).val(), $(SEL.cmbYear).val());
        });
        U.on(NS, "change", SEL.cmbYear, function () {
            go($(SEL.cmbMonth).val(), $(this).val());
        });
    }

    function bindAccordions() {
        // Make whole header clickable
        U.off(NS, "click", SEL.accHeader);
        U.on(NS, "click", SEL.accHeader, function (e) {
            // Only trigger if click wasn't on a link already to avoid double toggles
            if (!$(e.target).closest(SEL.accToggle).length) {
                $(this).find(SEL.accToggle).trigger("click");
            }
        });

        // Toggle +/− on collapse events
        const $collapses = $(".collapse[id^='collapse']");
        $collapses.each(function () {
            const $c = $(this);
            const $header = $(`[aria-controls='${$c.attr("id")}']`).closest(".card-header");
            const $icon = $header.find(SEL.accIcon);

            // initial state
            if ($c.hasClass("show")) {
                $icon.removeClass("fe-plus-circle").addClass("fe-minus-circle");
            } else {
                $icon.removeClass("fe-minus-circle").addClass("fe-plus-circle");
            }

            $c.off("show.bs.collapse" + NS).on("show.bs.collapse" + NS, function () {
                $icon.removeClass("fe-plus-circle").addClass("fe-minus-circle");
            });
            $c.off("hide.bs.collapse" + NS).on("hide.bs.collapse" + NS, function () {
                $icon.removeClass("fe-minus-circle").addClass("fe-plus-circle");
            });
        });
    }

    function initTables() {
        const make = U.makeDataTable || w.makeDataTable;
        if (typeof make !== "function") {
            U.logWarn?.("DataTable init skipped: makeDataTable not found");
            return;
        }
        if (d.querySelector(SEL.loginHistoryTable)) make(SEL.loginHistoryTable, "1");
        if (d.querySelector(SEL.auditLogTable)) make(SEL.auditLogTable, "1");
        if (d.querySelector(SEL.deletedAssetsTable)) make(SEL.deletedAssetsTable, "1");
    }

    function init() {
        U.hideMenu?.();
        bindFilters();
        bindAccordions();
        initTables();
    }

    if (AMS.pages && typeof AMS.pages.register === "function") {
        AMS.pages.register(PAGE_KEY, init);
    } else {
        U.ready(function () {
            const bodyPage = U.pageKey?.() || "";
            if (!bodyPage || bodyPage === PAGE_KEY) init();
        });
    }
})(window, document, window.jQuery);
