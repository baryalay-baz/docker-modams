// ~/js/tours/donors-index-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Donors/Index tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Donors/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="donors.header"]',
                popover: {
                    title: t("Donors", "Deeq-bixiyeyaasha"),
                    description: t("This page lists and manages donors.", "Boggan waxa uu muujinayaa oo maamulaa deeq-bixiyeyaasha."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="donors.breadcrumbs"]',
                popover: {
                    title: t("Breadcrumbs", "Jidka Bogga"),
                    description: t("Use these links to navigate back to Dashboard.", "Isticmaal isku-xirayaasha si aad ugu noqoto Dashboard-ka."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="donors.create"]',
                popover: {
                    title: t("Create Donor", "Abuur Deeq-bixiye"),
                    description: t("Add a new donor record.", "Ku dar diiwaan deeq-bixiye cusub."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="donors.table.container"]',
                popover: {
                    title: t("Donor List", "Liiska Deeq-bixiyeyaasha"),
                    description: t("Hover a row to reveal quick actions.", "Ku dul-wareeji saf si aad u aragto ficillada degdega ah."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="donors.table"]',
                popover: {
                    title: t("Columns", "Tiirar"),
                    description: t("Code and Name identify each donor.", "Koodhka iyo Magacu waxay aqoonsadaan deeq-bixiyaha."),
                    side: "top"
                }
            },
            // DataTables live controls (target the actual wrapper)
            {
                element: '#tblDonors_wrapper .dataTables_filter',
                popover: {
                    title: t("Search", "Raadi"),
                    description: t("Filter donors by code or name.", "Shaandhee deeq-bixiyeyaasha kood ama magac."),
                    side: "left"
                }
            },
            {
                element: '#tblDonors_wrapper .dt-buttons',
                popover: {
                    title: t("Export", "Dhoofin"),
                    description: t("Copy, Excel, or PDF the current table.", "Nuqul, Excel, ama PDF ka samee shaxda hadda."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="donors.back"]',
                popover: {
                    title: t("Back", "Dib u Noqo"),
                    description: t("Return to the previous page.", "Ku noqo boggii hore."),
                    side: "top"
                }
            }
        ]
    };
})();
