// /wwwroot/js/tours/transfers-index-tour.js
(function () {
    "use strict";
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) { console.error("Driver.js not loaded for Transfers/Index."); return; }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    const steps = [
        {
            element: '[data-tour="tr.title"]',
            popover: {
                title: t("Transfers", "Wareejinta Hantida"),
                description: t("Review and manage all asset transfers.", "Dib u eeg oo maamul dhammaan wareejinnada."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="tr.breadcrumbs"]',
            popover: {
                title: t("Breadcrumbs", "Jid-raac"),
                description: t("Quick navigation back to sections.", "Ku noqosho degdeg ah qaybaha kale."),
                side: "bottom"
            }
        },

        // KPIs / Search card (shown only if present; loader filters missing nodes)
        {
            element: '[data-tour="tr.kpi.out"]',
            popover: {
                title: t("Transferred Out", "Laga Wareejiyay"),
                description: t("Outbound transfers this period.", "Wareejinnada ka baxay muddadan."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="tr.kpi.in"]',
            popover: {
                title: t("Received In", "La Soo Qaatay"),
                description: t("Inbound transfers received.", "Wareejinnada soo galay."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="tr.kpi.search"]',
            popover: {
                title: t("Search by Asset", "Raadi Hanti"),
                description: t("Scan or type an asset barcode.", "Geli ama iskaanno baar-kodka hantida."),
                side: "bottom"
            }
        },

        // Toolbar
        {
            element: '[data-tour="tr.create"]',
            popover: {
                title: t("Create Transfer", "Samee Wareejin"),
                description: t("Start a new transfer to another store.", "Ka bilow wareejin cusub bakhaar kale."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="tr.filters"]',
            popover: {
                title: t("Filters", "Shaandhayaal"),
                description: t("Filter by store and date range.", "Ku shaandhee bakhaar iyo taariikh."),
                side: "left"
            }
        },

        // OUTGOING table + controls (scoped to #tblTransfersOut wrapper)
        {
            element: '#tblTransfersOut',
            popover: {
                title: t("Outgoing Transfers", "Wareejinnada Ka Baxaya"),
                description: t("Number, date, from/to, items, status.", "Lambar, taariikh, laga/loo diray, tiro, xaalad."),
                side: "top"
            }
        },
        {
            element: '#tblTransfersOut_wrapper .dataTables_length',
            popover: {
                title: t("Page Size", "Tirada Safafka"),
                description: t("Change rows per page.", "Badal tirada safafka bogga."),
                side: "bottom"
            }
        },
        {
            element: '#tblTransfersOut_wrapper .dataTables_filter',
            popover: {
                title: t("Search (Outgoing)", "Raadi (Bixid)"),
                description: t("Instantly filter the outgoing table.", "Si dhaqso ah u shaandhee jadwalka bixidda."),
                side: "bottom"
            }
        },
        {
            element: '#tblTransfersOut_wrapper .dt-buttons, #tblTransfersOut_wrapper .ams-export-col',
            popover: {
                title: t("Export", "Dhoofin"),
                description: t("Copy / Excel / PDF.", "Koobi / Excel / PDF."),
                side: "left"
            }
        },
        {
            element: '#tblTransfersOut_wrapper .dataTables_paginate',
            popover: {
                title: t("Pagination", "Bogagga"),
                description: t("Navigate results pages.", "U gudub bogagga natiijooyinka."),
                side: "top"
            }
        },

        // Row actions (inline buttons) — only appears if your rows include them
        {
            element: '[data-tour="tr.row.actions"]',
            popover: {
                title: t("Row Actions", "Hawlaha Safka"),
                description: t("View, print, edit, or delete (permissions apply).", "Eeg, daabac, tafatir, ama tirtir (ogolaansho)."),
                side: "left"
            }
        },

        // INCOMING table + controls (scoped to #tblTransfersIn wrapper)
        {
            element: '#tblTransfersIn',
            popover: {
                title: t("Incoming Transfers", "Wareejinnada Soo Galaya"),
                description: t("Number, date, from/to, items, status.", "Lambar, taariikh, laga/loo diray, tiro, xaalad."),
                side: "top"
            }
        },
        {
            element: '#tblTransfersIn_wrapper .dataTables_length',
            popover: {
                title: t("Page Size (Incoming)", "Tirada Safafka (Soo-gal)"),
                description: t("Change rows per page.", "Badal tirada safafka bogga."),
                side: "bottom"
            }
        },
        {
            element: '#tblTransfersIn_wrapper .dataTables_filter',
            popover: {
                title: t("Search (Incoming)", "Raadi (Soo-gal)"),
                description: t("Instantly filter the incoming table.", "Si dhaqso ah u shaandhee jadwalka soo-galka."),
                side: "bottom"
            }
        },
        {
            element: '#tblTransfersIn_wrapper .dt-buttons, #tblTransfersIn_wrapper .ams-export-col',
            popover: {
                title: t("Export", "Dhoofin"),
                description: t("Copy / Excel / PDF.", "Koobi / Excel / PDF."),
                side: "left"
            }
        },
        {
            element: '#tblTransfersIn_wrapper .dataTables_paginate',
            popover: {
                title: t("Pagination", "Bogagga"),
                description: t("Navigate results pages.", "U gudub bogagga natiijooyinka."),
                side: "top"
            }
        },

        // Back button
        {
            element: '[data-tour="tr.back"]',
            popover: {
                title: t("Back", "Dib U Noqo"),
                description: t("Return to previous page.", "Ku noqo boggii hore."),
                side: "top"
            }
        }
    ];

    // Register for your tours-loader
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Transfers/Index"] = { version: "v1", steps };

    // Optional: manual start hook (e.g., from console)
    const driverObj = driverFactory({ animate: true, showProgress: true, steps });
    window.startTransfersIndexTour = () => driverObj.drive();
})();
