// /wwwroot/js/tours/disposals-editdisposal-tour.js
(function () {
    "use strict";
    const factory = window?.driver?.js?.driver;
    if (!factory) return;

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || window.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    const steps = [
        {
            element: '[data-tour="ed.title"]',
            popover: {
                title: t("Edit Disposal", "Fur Khasaaraha"),
                description: t("Update the disposal record.", "Cusbooneysii diiwaanka khasaaraha."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="ed.breadcrumbs"]',
            popover: {
                title: t("Navigation", "Hagid"),
                description: t("Go back to dashboard or list.", "U noqo dashboard ama liiska."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="ed.storeName"]',
            popover: {
                title: t("Store", "Kayd"),
                description: t("This disposal belongs to this store.", "Khasaarahani wuxuu ku diiwaan gashan yahay kaydkan."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="ed.storeOwner"]',
            popover: {
                title: t("Store Owner", "Milkiilaha Kaydka"),
                description: t("Informational only.", "Xog keliya."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="ed.date"]',
            popover: {
                title: t("Date", "Taariikh"),
                description: t("Edit the disposal date.", "Beddel taariikhda khasaaraha."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="ed.type"]',
            popover: {
                title: t("Disposal Type", "Nooca Khasaaraha"),
                description: t("Select how the asset is disposed.", "Dooro sida hantidu u khasaarayso."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="ed.notes"]',
            popover: {
                title: t("Notes", "Qoraallo"),
                description: t("Optional remarks.", "Faallooyin ikhtiyaari ah."),
                side: "top"
            }
        },

        {
            element: '[data-tour="ed.assetPickerToggle"]',
            popover: {
                title: t("Select Asset", "Dooro Hanti"),
                description: t("Open the table to choose an asset.", "Fur miiska si aad u doorato hanti."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="ed.assetTable"]',
            popover: {
                title: t("Assets Table", "Miiska Hantida"),
                description: t("Click ‘select’ on the asset row.", "Guji ‘select’ safka hantida."),
                side: "top"
            }
        },

        {
            element: '[data-tour="ed.assetDetails"]',
            popover: {
                title: t("Selected Asset", "Hantida La Doortay"),
                description: t("Details appear here.", "Faahfaahinta halkan ayay ka muuqan."),
                side: "top"
            }
        },

        {
            element: '[data-tour="ed.imageInput"]',
            popover: {
                title: t("Attach / Change Image", "Ku Lifaaq / Beddel Sawir"),
                description: t("Upload or replace supporting image.", "Soo geli ama beddel sawirka taageerada."),
                side: "left"
            }
        },

        {
            element: '[data-tour="ed.save"]',
            popover: {
                title: t("Save Changes", "Kaydi Isbedellada"),
                description: t("You must select an asset before saving.", "Waa in aad doorataa hanti ka hor kaydinta."),
                side: "top"
            }
        },

        {
            element: '[data-tour="ed.delete"]',
            popover: {
                title: t("Delete Disposal", "Tirtir Khasaaraha"),
                description: t("Opens a confirmation dialog.", "Waxay furtaa daaqad xaqiijin."),
                side: "top"
            }
        },

        {
            element: '[data-tour="ed.cancel"]',
            popover: {
                title: t("Cancel", "Jooji"),
                description: t("Leave without saving.", "Ka bax adigoon kaydin."),
                side: "top"
            }
        }
    ];

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Disposals/EditDisposal"] = { version: "v1", steps };

    const driver = factory({ animate: true, showProgress: true, steps });
    window.startEditDisposalTour = () => driver.drive();
})();
