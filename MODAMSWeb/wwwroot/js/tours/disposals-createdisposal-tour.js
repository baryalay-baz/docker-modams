// /wwwroot/js/tours/disposals-createdisposal-tour.js
(function () {
    "use strict";
    const factory = window?.driver?.js?.driver;
    if (!factory) return;

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || window.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    const steps = [
        {
            element: '[data-tour="cd.title"]',
            popover: {
                title: t("Create Disposal", "Samee Khasaaro"),
                description: t("Add a new disposal record.", "Ku dar diiwaan khasaaro cusub."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="cd.breadcrumbs"]',
            popover: {
                title: t("Navigation", "Hagid"),
                description: t("Go back to dashboard or list.", "U noqo dashboard ama liiska."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="cd.storeName"]',
            popover: {
                title: t("Store", "Kayd"),
                description: t("Disposal will be recorded under this store.", "Khasaaraha waxa lagu diiwaangelinayaa kaydkan."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="cd.storeOwner"]',
            popover: {
                title: t("Store Owner", "Milkiilaha Kaydka"),
                description: t("Informational only.", "Xog keliya."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="cd.date"]',
            popover: {
                title: t("Date", "Taariikh"),
                description: t("Select the disposal date.", "Doorto taariikhda khasaaraha."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="cd.type"]',
            popover: {
                title: t("Disposal Type", "Nooca Khasaaraha"),
                description: t("Choose how the asset is disposed.", "Dooro sida hantidu u khasaarayso."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="cd.notes"]',
            popover: {
                title: t("Notes", "Qoraallo"),
                description: t("Optional remarks.", "Faallooyin ikhtiyaari ah."),
                side: "top"
            }
        },

        {
            element: '[data-tour="cd.assetPickerToggle"]',
            popover: {
                title: t("Select Asset", "Dooro Hanti"),
                description: t("Open and choose the asset from the table.", "Fur oo ka dooro miiska."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="cd.assetTable"]',
            popover: {
                title: t("Assets Table", "Miiska Hantida"),
                description: t("Click the Select button to pick one asset.", "Guji ‘select’ si aad u doorato hanti."),
                side: "top"
            }
        },
        {
            element: '[data-tour="cd.assetDetails"]',
            popover: {
                title: t("Selected Asset", "Hantida La Doortay"),
                description: t("Details appear here after selection.", "Faahfaahinta halkan ayey ka muuqan doontaa."),
                side: "top"
            }
        },

        {
            element: '[data-tour="cd.imageInput"]',
            popover: {
                title: t("Attach Image", "Ku Lifaaq Sawir"),
                description: t("Upload a supporting picture if available.", "Haddii uu jiro, geli sawir taageera."),
                side: "left"
            }
        },

        {
            element: '[data-tour="cd.save"]',
            popover: {
                title: t("Save Disposal", "Kaydi Khasaaraha"),
                description: t("You must select an asset before saving.", "Waa in aad doorataa hanti ka hor kaydinta."),
                side: "top"
            }
        },
        {
            element: '[data-tour="cd.cancel"]',
            popover: {
                title: t("Cancel", "Jooji"),
                description: t("Leave without saving.", "Ka bax adigoon kaydin."),
                side: "top"
            }
        }
    ];

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Disposals/CreateDisposal"] = { version: "v1", steps };

    // Optional helper
    const driver = factory({ animate: true, showProgress: true, steps });
    window.startCreateDisposalTour = () => driver.drive();
})();
