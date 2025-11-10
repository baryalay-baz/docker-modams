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
                element: '[data-tour="emp.header"]',
                popover: {
                    title: t("Employees", "Shaqaalaha"),
                    description: t("Browse and manage employee records.", "Baadh oo maamul diiwaannada shaqaalaha."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.new"]',
                popover: {
                    title: t("New Employee", "Shaqaale Cusub"),
                    description: t("Register a new employee.", "Diiwaangeli shaqaale cusub."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.table"]',
                popover: {
                    title: t("Employee List", "Liiska Shaqaalaha"),
                    description: t(
                        "Use search, paging, and export. Hover a row to reveal quick actions (Edit, Lock/Unlock).",
                        "Isticmaal raadinta, bogagga, iyo dhoofinta. Dul istaag saf si aad u aragto falalka degdegga ah (Tafatir, Quful/Furo)."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
