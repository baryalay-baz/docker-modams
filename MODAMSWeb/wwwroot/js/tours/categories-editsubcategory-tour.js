// ~/js/tours/categories-editsubcategory-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Categories/EditSubCategory tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Categories/EditSubCategory"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="sub.edit.header"]',
                popover: {
                    title: t("Edit Sub-Category", "Tafatir Qayb-hoosaad"),
                    description: t(
                        "Modify the sub-category details here.",
                        "Halkan ka beddel faahfaahinta qayb-hoosaadka."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="sub.edit.section"]',
                popover: {
                    title: t("Details", "Faahfaahin"),
                    description: t(
                        "Review and update the fields below.",
                        "Dib u eeg oo cusboonaysii meelaha hoose."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="sub.edit.code"]',
                popover: {
                    title: t("Code", "Koodh"),
                    description: t(
                        "Read-only sub-category code.",
                        "Koodh qayb-hoosaad ah oo keliya akhris."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.edit.parentName"]',
                popover: {
                    title: t("Parent Category", "Qaybta Waalidka"),
                    description: t(
                        "The parent (top-level) category this belongs to.",
                        "Qaybta waalidka ee heer-sare ah ee tan leedahay."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.edit.nameEn"]',
                popover: {
                    title: t("Name (English)", "Magaca (Ingiriisi)"),
                    description: t(
                        "Edit the English name.",
                        "Tafatir magaca Ingiriisiga."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.edit.nameSo"]',
                popover: {
                    title: t("Name (Somali)", "Magaca (Af-Somali)"),
                    description: t(
                        "Edit the Somali translation.",
                        "Tafatir tarjumaadda Af-Soomaaliga."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.edit.lifespan"]',
                popover: {
                    title: t("Lifespan (years)", "Muddada nolosha (sano)"),
                    description: t(
                        "Update the expected useful life.",
                        "Cusboonaysii muddada nolosha la filayo."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="sub.edit.submit"]',
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
                element: '[data-tour="sub.edit.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t(
                        "Return to the list without saving.",
                        "Ku noqo liiska adiga oo aan kaydin."
                    ),
                    side: "top"
                }
            }
        ]
    };
})();
