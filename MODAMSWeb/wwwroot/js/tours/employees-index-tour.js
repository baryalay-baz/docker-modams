// ~/js/tours/employees-index-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Employees/Index tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Employees/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="emp.index.header"]',
                popover: {
                    title: t("Employees", "Shaqaalaha"),
                    description: t("Browse and manage all employees.", "Daalaco oo maamul dhammaan shaqaalaha."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.index.breadcrumbs"]',
                popover: {
                    title: t("Breadcrumbs", "Jidka Bogga"),
                    description: t("Jump back to the Dashboard or parent pages.", "Ku noqo Dashboard-ka ama boggaga waalidka."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="emp.index.section"]',
                popover: {
                    title: t("Employee List", "Liiska Shaqaalaha"),
                    description: t("This section lists employees with key details.", "Qaybtan waxay muujisaa shaqaalaha iyo faahfaahin muhiim ah."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.index.create"]',
                popover: {
                    title: t("New Employee", "Shaqaale Cusub"),
                    description: t("Create a new employee record.", "Abuur diiwaan shaqaale cusub."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.index.table.container"]',
                popover: {
                    title: t("Table Area", "Aagga Shaxda"),
                    description: t("Hover a row to reveal quick actions (edit, lock, etc.).", "Ku dul-wareeji saf si aad u aragto ficillo degdeg ah (tifatir, quful, iwm)."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="emp.index.table"]',
                popover: {
                    title: t("Columns", "Tiirar"),
                    description: t("Full name, job title, role, department, and status.", "Magaca buuxa, jagada, doorka, waaxda, iyo xaaladda."),
                    side: "top"
                }
            },
            {
                // DataTables live search box
                element: '#tblEmployees_wrapper .dataTables_filter',
                popover: {
                    title: t("Search", "Raadi"),
                    description: t("Filter employees by name, title, or department.", "Shaandhee shaqaalaha magac, jago, ama waax."),
                    side: "left"
                }
            },
            {
                // DataTables export buttons (if enabled by your init)
                element: '#tblEmployees_wrapper .dt-buttons',
                popover: {
                    title: t("Export", "Dhoofin"),
                    description: t("Copy, Excel, or PDF the table contents.", "Nuqul, Excel, ama PDF ka samee waxa ku jira shaxda."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="emp.index.back"]',
                popover: {
                    title: t("Back", "Dib u Noqo"),
                    description: t("Return to the previous page.", "Ku noqo boggii hore."),
                    side: "top"
                }
            }
        ]
    };
})();
