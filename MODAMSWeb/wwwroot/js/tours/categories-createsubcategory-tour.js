// /wwwroot/js/tours/categories-createsubcategory-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Categories/CreateSubCategory tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Categories/CreateSubCategory"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="sub.create.header"]',
                popover: {
                    title: t("Create Sub-Category", "Abuur Qayb-hoosaad"),
                    description: t(
                        "Add a new sub-category under the selected category.",
                        "Ku dar qayb-hoosaad cusub hoosta qaybta la xushay."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="sub.create.section"]',
                popover: {
                    title: t("Sub-Category Details", "Faahfaahinta Qayb-hoosaadka"),
                    description: t(
                        "Complete the fields below.",
                        "Buuxi goobaha hoose."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="sub.create.categoryName"]',
                popover: {
                    title: t("Parent Category", "Qaybta Waalidka"),
                    description: t(
                        "This is the parent category name.",
                        "Tani waa magaca qaybta waalidka."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.create.code"]',
                popover: {
                    title: t("Sub-Category Code", "Koodhka Qayb-hoosaadka"),
                    description: t(
                        "Read-only; derived from the parent category code plus the first 4 letters of the sub-category name.",
                        "Kaliya akhris; waxa laga soo qaatay koodhka qaybta waalidka iyo 4-ta xaraf ee ugu horreeya ee magaca qayb-hoosaadka."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.create.nameEn"]',
                popover: {
                    title: t("Name (English)", "Magaca (Ingiriisi)"),
                    description: t(
                        "Type the sub-category name in English.",
                        "Geli magaca qayb-hoosaadka ee Ingiriisiga."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.create.nameSo"]',
                popover: {
                    title: t("Name (Somali)", "Magaca (Af-Somali)"),
                    description: t(
                        "Provide the Somali translation.",
                        "Geli tarjumaadda Af-Somaliga."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.create.lifespan"]',
                popover: {
                    title: t("Lifespan (years)", "Muddada nolosha (sano)"),
                    description: t(
                        "Expected useful life for depreciation and planning.",
                        "Muddada nolosha ee la filayo si loogu adeegsado qiimo dhimid iyo qorshayn."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.create.submit"]',
                popover: {
                    title: t("Save", "Kaydi"),
                    description: t(
                        "Click to save the new sub-category.",
                        "Guji si aad u kaydiso qayb-hoosaadka cusub."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="sub.create.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t(
                        "Return without saving.",
                        "Ku noqo adiga oo aan kaydin."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
