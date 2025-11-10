// /wwwroot/js/pages/admin/donors/donors-index.js
(function (w, d, $) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const PAGES = AMS.pages || (AMS.pages = { register() { }, start() { } });
    const U = (AMS.util = AMS.util || {});

    function init() {
        try { U.hideMenu?.(); } catch { }

        const actions = {
            enable: true,
            paddingPx: 140,
            buttons: [
                {
                    key: "edit",
                    titleEn: "Edit Donor",
                    titleSo: "Wax ka beddel Deeq-bixiye",
                    iconHtml: "<i class='fe fe-edit'></i>",
                    href: "/Admin/Donors/EditDonor/{id}",
                    variant: "info" // options: primary|info|success|warning|danger|secondary etc.
                }
                // If you later add delete or details, just drop them here.
                // { key:"delete", titleEn:"Delete", titleSo:"Tirtir", iconHtml:"<i class='fe fe-trash'></i>", variant:"danger", onClick: ... }
            ]
        };
        U.makeDataTable("#tblDonors", "2", 10, actions);
        if (w.bootstrap?.Tooltip) {
            d.querySelectorAll("[data-bs-toggle='tooltip']").forEach(el => {
                try { new bootstrap.Tooltip(el, { container: "body" }); } catch { }
            });
        }
    }

    PAGES.register && PAGES.register("Donors/Index", init);
})(window, document, jQuery);
