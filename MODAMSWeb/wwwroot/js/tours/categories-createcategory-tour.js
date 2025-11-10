// /wwwroot/js/tours/categories-createcategory-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Categories/CreateCategory tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Categories/CreateCategory"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="cat.create.header"]',
                popover: {
                    title: t("Create a New Category", "Abuur Qayb Cusub"),
                    description: t(
                        "Use this page to add a new top-level category.",
                        "Boggan ku dar qayb heer-sare ah oo cusub."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="cat.create.section"]',
                popover: {
                    title: t("Category Details", "Faahfaahinta Qaybta"),
                    description: t(
                        "Fill in the required fields below.",
                        "Buuxi meelaha loo baahan yahay ee hoose."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="cat.create.name"]',
                popover: {
                    title: t("Category Name", "Magaca Qaybta"),
                    description: t(
                        "Type the category name. The code will be generated automatically.",
                        "Geli magaca qaybta. Koodhku si toos ah ayuu u samaysmi doonaa."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="cat.create.code"]',
                popover: {
                    title: t("Category Code", "Koodhka Qaybta"),
                    description: t(
                        "Read-only. It’s derived from the category name (prefix 'MOD-' + first 4 letters).",
                        "Kaliya akhris. Waxaa laga soo goostaa magaca qaybta (hore 'MOD-' + 4 xaraf ee ugu horeeya)."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="cat.create.nameSo"]',
                popover: {
                    title: t("Somali Name", "Magaca Af-Somali"),
                    description: t(
                        "Provide the Somali translation of the category name.",
                        "Geli tarjumaadda Af-Somaliga ee magaca qaybta."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="cat.create.submit"]',
                popover: {
                    title: t("Save", "Kaydi"),
                    description: t(
                        "Click to save the new category.",
                        "Guji si aad u kaydiso qaybta cusub."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="cat.create.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t(
                        "Return to the categories list without saving.",
                        "Ku noqo liiska qaybaha adiga oo aan kaydin."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
