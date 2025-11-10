(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Donors/EditDonor tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Donors/EditDonor"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="don.edit.header"]',
                popover: {
                    title: t("Edit Donor", "Wax ka beddel Deeq-bixiye"),
                    description: t(
                        "Update donor information used across projects and reports.",
                        "Cusbooneysii xogta deeq-bixiyaha ee laga adeegsanayo mashaariicda iyo warbixinada."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="don.edit.breadcrumbs"]',
                popover: {
                    title: t("Navigation", "Hagitaan"),
                    description: t(
                        "Use breadcrumbs to go back to the donors list or dashboard.",
                        "Isticmaal breadcrumbs si aad ugu noqoto liiska deeq-bixiyeyaasha ama dashboard-ka."
                    ),
                    side: "left"
                }
            },
            {
                element: '[data-tour="don.edit.section"]',
                popover: {
                    title: t("Details", "Faahfaahin"),
                    description: t(
                        "Confirm and modify the donor’s core details below.",
                        "Hubi oo wax ka beddel faahfaahinta aasaasiga ah ee deeq-bixiyaha."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="don.edit.form"]',
                popover: {
                    title: t("Form", "Foomka"),
                    description: t(
                        "Fields marked as required must be completed before saving.",
                        "Goobaha qasab ah waa in la buuxiyaa ka hor inta aan la kaydin."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="don.edit.code"]',
                popover: {
                    title: t("Donor Code", "Koodhka Deeq-bixiyaha"),
                    description: t(
                        "Short unique code (e.g., EU, WB, SIDA) used in references.",
                        "Koodh gaaban oo gaar ah (tusaale: EU, WB, SIDA) oo laga adeegsado tixraacyada."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="don.edit.name"]',
                popover: {
                    title: t("Donor Name", "Magaca Deeq-bixiyaha"),
                    description: t(
                        "Official name appearing on documents and reports.",
                        "Magaca rasmiga ah ee ka muuqanaya dukumentiyada iyo warbixinada."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="don.edit.submit"]',
                popover: {
                    title: t("Save Changes", "Kaydi Isbeddelada"),
                    description: t(
                        "Click to update and save the donor.",
                        "Guji si aad u cusbooneysiiso uguna kaydiso deeq-bixiyaha."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="don.edit.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t(
                        "Return to the donors list without saving changes.",
                        "Ku noqo liiska adigoon kaydin isbeddelada."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
