// ~/js/tours/departments-editdepartment-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Departments/EditDepartment tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Departments/EditDepartment"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="dept.edit.header"]',
                popover: {
                    title: t("Edit Department", "Wax ka beddel Waax"),
                    description: t("Update the department details.", "Cusbooneysii faahfaahinta waaxda."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="dept.edit.section"]',
                popover: {
                    title: t("Details", "Faahfaahin"),
                    description: t("Maintain English and Somali names consistently.", "Joogteey magaca English iyo Somali."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="dept.edit.form"]',
                popover: {
                    title: t("Form", "Foom"),
                    description: t("Fields marked required must be filled.", "Goobaha qasab ah waa in la buuxiyaa."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.edit.name"]',
                popover: {
                    title: t("Department Name (EN)", "Magaca Waaxda (EN)"),
                    description: t("Shown in English reports.", "Waxa lagu isticmaalaa warbixinada English."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.edit.nameSo"]',
                popover: {
                    title: t("Department Name (SO)", "Magaca Waaxda (SO)"),
                    description: t("Shown to Somali users.", "Waxa lagu tusi doonaa isticmaaleyaasha Somali."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.edit.upper"]',
                popover: {
                    title: t("Upper-Level Department", "Waaxda Sare"),
                    description: t("If applicable, link to a parent for org chart.", "Haddii loo baahdo, ku xir waalid si shaxda ururku u saxnaato."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.edit.owner"]',
                popover: {
                    title: t("Department Owner", "Mulkiilaha Waaxda"),
                    description: t("Owner is read-only here; manage in Department Heads.", "Mulkiilaha halkan waa akhris-kaliya; maamul bogga Madaxa Waaxda."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.edit.sidebar"]',
                popover: {
                    title: t("Side Panel", "Guddi Dhinac"),
                    description: t("Reserved for previews or auxiliary info.", "Loogu talagalay muuqaallo/warbixinno dheeraad ah."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="dept.edit.submit"]',
                popover: {
                    title: t("Save Changes", "Kaydi Isbedellada"),
                    description: t("Submit to update the department.", "Gudbi si aad u cusbooneysiiso waaxda."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="dept.edit.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t("Return to the list without saving.", "Ku noqo liiska adigoon kaydin."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="dept.edit.breadcrumbs"]',
                popover: {
                    title: t("Navigation", "Hagitaan"),
                    description: t("Use breadcrumbs to jump between pages.", "Isticmaal breadcrumbs si aad ugu kala booddo bogagga."),
                    side: "left"
                }
            }
        ]
    };
})();
