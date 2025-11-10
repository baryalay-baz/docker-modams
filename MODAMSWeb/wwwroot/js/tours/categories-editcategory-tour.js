// /wwwroot/js/tours/categories-editcategory-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Categories/EditCategory tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Categories/EditCategory"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="cat.edit.header"]',
                popover: {
                    title: t("Edit Category", "Tafatir Qayb"),
                    description: t(
                        "You can modify the category details here.",
                        "Halkan waxaad ka beddeli kartaa faahfaahinta qaybta."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="cat.edit.section"]',
                popover: {
                    title: t("Category Details", "Faahfaahinta Qaybta"),
                    description: t(
                        "Fields below define the category.",
                        "Goobaha hoose ayaa qeexaya qaybta."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="cat.edit.name"]',
                popover: {
                    title: t("Category Name", "Magaca Qaybta"),
                    description: t(
                        "Changing the name will regenerate the code preview.",
                        "Markaad beddesho magaca, koodhka ayaa dib loo soo saari doonaa."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="cat.edit.code"]',
                popover: {
                    title: t("Category Code", "Koodhka Qaybta"),
                    description: t(
                        "Read-only; derived from the name (prefix 'MOD-' + first 4 letters).",
                        "Kaliya akhris; waxa laga soo qaatay magaca (hore 'MOD-' + 4 xaraf ee ugu horreeya)."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="cat.edit.nameSo"]',
                popover: {
                    title: t("Somali Name", "Magaca Af-Somali"),
                    description: t(
                        "Provide the Somali translation for the category.",
                        "Geli tarjumaadda Af-Somaliga ee qaybta."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="cat.edit.submit"]',
                popover: {
                    title: t("Save Changes", "Kaydi Isbeddellada"),
                    description: t(
                        "Click to save your edits.",
                        "Guji si aad u kaydiso isbeddellada."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="cat.edit.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t(
                        "Go back without saving.",
                        "Ku noqo adiga oo aan kaydin."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
