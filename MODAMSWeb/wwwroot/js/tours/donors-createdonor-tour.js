(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Donors/CreateDonor tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Donors/CreateDonor"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="don.create.header"]',
                popover: {
                    title: t("Create Donor", "Abuur Deeq-bixiye"),
                    description: t(
                        "Add a new donor profile used across funding and reports.",
                        "Ku dar deeq-bixiye cusub oo laga adeegsan doono maalgelinta iyo warbixinada."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="don.create.breadcrumbs"]',
                popover: {
                    title: t("Navigation", "Hagitaan"),
                    description: t(
                        "Use breadcrumbs to go back to the dashboard or donors list.",
                        "Isticmaal breadcrumbs si aad ugu noqoto dashboard-ka ama liiska deeq-bixiyeyaasha."
                    ),
                    side: "left"
                }
            },
            {
                element: '[data-tour="don.create.section"]',
                popover: {
                    title: t("Details", "Faahfaahin"),
                    description: t(
                        "Basic identification fields for the donor.",
                        "Goobaha aqoonsiga aasaasiga ah ee deeq-bixiyaha."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="don.create.form"]',
                popover: {
                    title: t("Form", "Foomka"),
                    description: t(
                        "Fill the required fields and submit to save.",
                        "Buuxi meelaha qasab ah kadibna gudbi si loo kaydiyo."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="don.create.code"]',
                popover: {
                    title: t("Donor Code", "Koodhka Deeq-bixiyaha"),
                    description: t(
                        "A short unique code (e.g., EU, WB, SIDA).",
                        "Koodh gaaban oo gaar ah (tusaale: EU, WB, SIDA)."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="don.create.name"]',
                popover: {
                    title: t("Donor Name", "Magaca Deeq-bixiyaha"),
                    description: t(
                        "Official donor name used in documents and reports.",
                        "Magaca rasmiga ah ee deeq-bixiyaha ee dukumentiyada iyo warbixinada."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="don.create.submit"]',
                popover: {
                    title: t("Save", "Kaydi"),
                    description: t(
                        "Click to create the donor.",
                        "Guji si aad u abuurto deeq-bixiyaha."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="don.create.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t(
                        "Return to the donors list without saving.",
                        "Ku noqo liiska adigoon kaydin."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
