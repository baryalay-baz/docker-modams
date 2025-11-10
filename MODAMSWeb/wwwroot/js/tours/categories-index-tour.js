// /wwwroot/js/tours/categories-index-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Categories/Index tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    // Ensure registry object exists
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    window.AMS_TOUR_REGISTRY["Categories/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="cat.header"]',
                popover: {
                    title: t("Manage Categories", "Maamul Qaybaha"),
                    description: t(
                        "This page lets you manage high-level categories and their sub-categories.",
                        "Boggan waxaad ka maamuli kartaa qaybaha waaweyn iyo qayb-hoosaadyadooda."
                    ),
                    side: "bottom",
                },
            },
            {
                element: '[data-tour="cat.new"]',
                popover: {
                    title: t("New Category", "Qayb Cusub"),
                    description: t(
                        "Click to create a new top-level category.",
                        "Guji si aad u abuurto qayb cusub."
                    ),
                    side: "bottom",
                },
            },
            {
                element: '[data-tour="cat.table"]',
                popover: {
                    title: t("Categories List", "Liiska Qaybaha"),
                    description: t(
                        "Click a row to select it. The right panel will load its sub-categories.",
                        "Guji saf si aad u doorato. Gudiga midig wuxuu soo bandhigi doonaa qayb-hoosaadyada."
                    ),
                    side: "right",
                },
            },
            {
                element: '[data-tour="cat.sub.title"]',
                popover: {
                    title: t("Sub-Categories", "Qayb-hoosaadyo"),
                    description: t(
                        "Shows the sub-categories for the selected category.",
                        "Waxay muujinaysaa qayb-hoosaadyada qaybta la doortay."
                    ),
                    side: "bottom",
                },
            },
            {
                element: '[data-tour="cat.sub.new"]',
                popover: {
                    title: t("New Sub-Category", "Qayb-hoosaad Cusub"),
                    description: t(
                        "After selecting a category on the left, use this to add a sub-category.",
                        "Marka aad doorato qayb bidixda, isticmaal tan si aad ugu darto qayb-hoosaad."
                    ),
                    side: "bottom",
                },
            },
            {
                element: '[data-tour="cat.sub.table"]',
                popover: {
                    title: t("Sub-Categories List", "Liiska Qayb-hoosaadyada"),
                    description: t(
                        "Hover the row to reveal action buttons (Edit, etc.).",
                        "Marka aad dul istaagto safka, badhamada falalka (Tafatir, iwm) ayaa soo bixi doona."
                    ),
                    side: "left",
                },
            },
            {
                element: '[data-tour="cat.sub.action"]',
                popover: {
                    title: t("Row Actions", "Ficillada Safka"),
                    description: t(
                        "Use these buttons to edit the selected sub-category.",
                        "Isticmaal badhamadan si aad u tafatirto qayb-hoosaadka la xushay."
                    ),
                    side: "top",
                },
            },
        ],
    };
})();
