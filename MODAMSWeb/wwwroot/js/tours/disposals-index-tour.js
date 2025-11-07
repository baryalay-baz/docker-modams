// /wwwroot/js/tours/disposals-index-tour.js
(function () {
    "use strict";
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) { console.error("Driver.js not loaded for Disposals/Index."); return; }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    const steps = [
        {
            element: '[data-tour="ds.title"]',
            popover: {
                title: t("Disposals", "Dafaacyada"),
                description: t("View and manage all asset disposals for this store.", "Eeg oo maamul dhammaan ka-takhalusyada hantida ee bakhaarkan."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ds.breadcrumbs"]',
            popover: {
                title: t("Breadcrumbs", "Jid-raac"),
                description: t("Quick navigation back to other sections.", "Ku noqosho degdeg ah qaybaha kale."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ds.kpis"]',
            popover: {
                title: t("Top Categories", "Qaybaha Sare"),
                description: t("At-a-glance counts for the most frequent disposal types.", "Tiro-kooban oo degdeg ah ee noocyada ka-takhaluska ugu badan."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ds.store"]',
            popover: {
                title: t("Current Store", "Bakhaarka Hadda"),
                description: t("All data on this page is scoped to this store.", "Dhammaan xogta boggan waxa lagu xaddiday bakhaarkan."),
                side: "bottom"
            }
        },
        // Toolbar & Filters
        {
            element: '[data-tour="ds.toolbar"]',
            popover: {
                title: t("Toolbar", "Qalabka Shaqada"),
                description: t("Create new disposal, filter by dates, and export.", "Samee ka-takhalus cusub, ku shaandhee taariikho, oo dhoofin."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ds.filters.from"]',
            popover: {
                title: t("From Date", "Taariikhda Bilowga"),
                description: t("Start of the date range (YYYY-MM-DD).", "Bilowga xilligga taariikhda (YYYY-MM-DD)."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ds.filters.to"]',
            popover: {
                title: t("To Date", "Taariikhda Dhammaadka"),
                description: t("End of the date range (YYYY-MM-DD).", "Dhammaadka xilligga taariikhda (YYYY-MM-DD)."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ds.filters.apply"]',
            popover: {
                title: t("Apply Filter", "Dalbo Shaandhaynta"),
                description: t("Apply the date range to the table below.", "Ku dabaq xilligga taariikhda jadwalka hoose."),
                side: "left"
            }
        },
        // Table & DataTables controls
        {
            element: '[data-tour="ds.table"]',
            popover: {
                title: t("Disposals Table", "Jadwalka Dafaacyada"),
                description: t("Date, type, asset name, and identification.", "Taariikh, nooc, magaca hanti, iyo aqoonsi."),
                side: "top"
            }
        },
        {
            element: '#tblDisposals_wrapper .dataTables_length',
            popover: {
                title: t("Page Size", "Tirada Safafka"),
                description: t("Change rows per page.", "Badal tirada safafka bogga."),
                side: "bottom"
            }
        },
        {
            element: '#tblDisposals_wrapper .dataTables_filter',
            popover: {
                title: t("Search", "Raadi"),
                description: t("Instant text search in the table.", "Raadin degdeg ah gudaha jadwalka."),
                side: "bottom"
            }
        },
        {
            element: '#tblDisposals_wrapper .dt-buttons',
            popover: {
                title: t("Export", "Dhoofin"),
                description: t("Copy / Excel / PDF.", "Koobi / Excel / PDF."),
                side: "left"
            }
        },
        {
            element: '#tblDisposals_wrapper .dataTables_paginate',
            popover: {
                title: t("Pagination", "Bogagga"),
                description: t("Navigate through results pages.", "U gudub bogagga natiijooyinka."),
                side: "top"
            }
        },
        // Chart
        {
            element: '[data-tour="ds.chart"]',
            popover: {
                title: t("By Disposal Type", "Nooca Ka-Takhaluska"),
                description: t("Breakdown of disposals by type.", "Kala qeybsanaanta ka-takhaluska iyadoo loo eegayo nooca."),
                side: "left"
            }
        }
    ];

    // Register for the loader
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Disposals/Index"] = { version: "v1", steps };

    // Optional: manual start for debugging
    const driverObj = driverFactory({ animate: true, showProgress: true, steps });
    window.startDisposalsIndexTour = () => driverObj.drive();
})();
