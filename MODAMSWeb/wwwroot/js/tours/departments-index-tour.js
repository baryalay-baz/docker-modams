(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Departments/Index tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Departments/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="dept.index.header"]',
                popover: {
                    title: t("Departments", "Waaxyaha"),
                    description: t(
                        "Manage department records, owners, and users.",
                        "Maamul xogta waaxyaha, mulkiilayaasha iyo adeegsadayaasha."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="dept.index.breadcrumbs"]',
                popover: {
                    title: t("Navigation", "Hagitaan"),
                    description: t(
                        "Use breadcrumbs to move between dashboard and lists.",
                        "Isticmaal breadcrumbs si aad ugu kala wareegto dashboard-ka iyo liisaska."
                    ),
                    side: "left"
                }
            },
            {
                element: '[data-tour="dept.index.top-actions"]',
                popover: {
                    title: t("Quick Actions", "Ficillo Degdeg ah"),
                    description: t(
                        "Open the Organization Chart or access more actions.",
                        "Fur Shaxda Ururka ama gal ficillo dheeraad ah."
                    ),
                    side: "left"
                }
            },
            {
                element: '[data-tour="dept.index.create"]',
                popover: {
                    title: t("Create Department", "Abuur Waax"),
                    description: t(
                        "Add a new department to the system.",
                        "Ku dar waax cusub nidaamka."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="dept.index.tablewrap"]',
                popover: {
                    title: t("Departments Table", "Jadwalka Waaxyaha"),
                    description: t(
                        "Hover a row to reveal action pills (Edit, Store Users).",
                        "Ku dul soco saf si aad u aragto badhamada ficillada (Wax ka beddel, Adeegsadayaasha Dukaanka)."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="dept.index.table"]',
                popover: {
                    title: t("Table Controls", "Xakameynta Jadwalka"),
                    description: t(
                        "Use built-in DataTables search, paging, and export (if enabled).",
                        "Isticmaal raadinta, bogagga, iyo dhoofinta DataTables (haddii la daaray)."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="dept.index.back"]',
                popover: {
                    title: t("Back", "Dib u Noqo"),
                    description: t(
                        "Return to the dashboard.",
                        "Ku noqo dashboard-ka."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
