// ~/js/tours/departments-createdepartment-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Departments/CreateDepartment tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Departments/CreateDepartment"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="dept.create.header"]',
                popover: {
                    title: t("Create Department", "Abuur Waax"),
                    description: t("Add a new department record.", "Ku dar diiwaan waax cusub."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="dept.create.section"]',
                popover: {
                    title: t("Details", "Faahfaahin"),
                    description: t("Fill the name in English and Somali.", "Buuxi magaca English iyo Somali."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="dept.create.form"]',
                popover: {
                    title: t("Form", "Foom"),
                    description: t("All required fields must be completed.", "Goobaha qasab ah waa in la buuxiyaa."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.create.name"]',
                popover: {
                    title: t("Department Name (EN)", "Magaca Waaxda (EN)"),
                    description: t("Official English name used in reports.", "Magaca rasmiga ah ee English-ka ee warbixinada."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.create.nameSo"]',
                popover: {
                    title: t("Department Name (SO)", "Magaca Waaxda (SO)"),
                    description: t("Somali label shown to local users.", "Magaca Somali ee lagu tusi doono isticmaaleyaasha maxalliga ah."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.create.upper"]',
                popover: {
                    title: t("Upper-Level Department", "Waaxda Sare"),
                    description: t(
                        "Optionally link this department to a parent for the org chart.",
                        "Haddii loo baahdo, ku xiro waalid si loogu muujiyo shaxda ururka."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="dept.create.sidebar"]',
                popover: {
                    title: t("Side Panel", "Guddi Dhinac"),
                    description: t("Reserved for future info/preview widgets.", "Loogu talagalay xog/sawirro mustaqbalka."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="dept.create.submit"]',
                popover: {
                    title: t("Save Department", "Kaydi Waaxda"),
                    description: t("Submit the form to create the department.", "Gudbi si aad u abuurto waaxda."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="dept.create.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t("Return to the department list without saving.", "Ku noqo liiska waaxyaha adoonsa kaydin."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="dept.create.breadcrumbs"]',
                popover: {
                    title: t("Navigation", "Hagitaan"),
                    description: t("Use breadcrumbs to jump back to lists.", "Isticmaal breadcrumbs si aad ugu noqoto liisaska."),
                    side: "left"
                }
            }
        ]
    };
})();
