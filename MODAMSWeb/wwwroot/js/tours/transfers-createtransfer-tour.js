// /wwwroot/js/tours/transfers-createtransfer-tour.js
(function () {
    "use strict";
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) { console.error("Driver.js not loaded for Transfers/CreateTransfer."); return; }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    const steps = [
        // Header & breadcrumbs
        {
            element: '[data-tour="ct.title"]',
            popover: {
                title: t("Create Transfer", "Abuur Wareejin"),
                description: t("Start a new asset transfer.", "Bilow wareejin hanti cusub."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ct.breadcrumbs"]',
            popover: {
                title: t("Breadcrumbs", "Jid-raac"),
                description: t("Navigate back to dashboard or transfers list.", "Ku noqo dashboard ama liiska wareejinta."),
                side: "bottom"
            }
        },

        // Main form intro
        {
            element: '[data-tour="ct.header"]',
            popover: {
                title: t("Form Overview", "Guudmar Foomka"),
                description: t("Fill the details, pick assets, then save.", "Buuxi faahfaahinta, dooro hantida, kadibna keydi."),
                side: "bottom"
            }
        },

        // Core fields
        {
            element: '[data-tour="ct.from"]',
            popover: {
                title: t("From Store", "Laga Wareejinayo"),
                description: t("Origin store is fixed based on your role.", "Bakhaarka laga wareejinayo waa go’an iyadoo doorkaaga lagu saleeyay."),
                side: "right"
            }
        },
        {
            element: '[data-tour="ct.number"]',
            popover: {
                title: t("Transfer Number", "Lambarka Wareejinta"),
                description: t("Auto-generated and read-only.", "Si toos ah loo sameeyay oo kaliya akhris."),
                side: "left"
            }
        },
        {
            element: '[data-tour="ct.date"]',
            popover: {
                title: t("Transfer Date", "Taariikhda Wareejinta"),
                description: t("Pick the effective transfer date.", "Dooro taariikhda wareejinta ee la dhaqan gelinayo."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ct.store"]',
            popover: {
                title: t("To Store", "Loogu Wareejinayo"),
                description: t("Choose the destination store.", "Dooro bakhaarka loo wareejinayo."),
                side: "bottom"
            }
        },

        // Assets selection
        {
            element: '[data-tour="ct.assets.toggle"]',
            popover: {
                title: t("Select Assets", "Dooro Hantida"),
                description: t("Expand to view available assets.", "Furo si aad u aragto hantida la heli karo."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ct.assets.table"]',
            popover: {
                title: t("Assets Table", "Jadwalka Hantida"),
                description: t("Tick items to include in this transfer.", "Calaamadee hantida aad rabto in wareejinta lagu daro."),
                side: "top"
            }
        },
        {
            element: '[data-tour="ct.assets.checkbox"]',
            popover: {
                title: t("Select/Deselect", "Dooro/Ka Saar"),
                description: t("Use the checkbox to add or remove an item.", "Isticmaal sanduuqa si aad u darto ama uga saarto hanti."),
                side: "right"
            }
        },
        {
            element: '[data-tour="ct.assets.count"]',
            popover: {
                title: t("Selection Counter", "Tirada Xulashada"),
                description: t("Live counter of selected assets.", "Tiro cusub oo toos ah oo hantida la xushay."),
                side: "left"
            }
        },

        // Notes
        {
            element: '[data-tour="ct.notes"]',
            popover: {
                title: t("Notes", "Qoraallo"),
                description: t("Explain the reason or any handover details.", "Sharax sababta ama faahfaahin kale."),
                side: "top"
            }
        },

        // Sidebar selection
        {
            element: '[data-tour="ct.selection.card"]',
            popover: {
                title: t("Your Selection", "Xulashadaada"),
                description: t("Review items you’ve picked.", "Dib u eeg hantida aad dooratay."),
                side: "left"
            }
        },
        {
            element: '[data-tour="ct.selection.table"]',
            popover: {
                title: t("Selected Items", "Waxyaabaha La Doortay"),
                description: t("This list updates as you tick/untick.", "Liiskani wuu cusboonaysiinayaa markaad calaamadeyso."),
                side: "left"
            }
        },

        // Actions
        {
            element: '[data-tour="ct.save"]',
            popover: {
                title: t("Save Transfer", "Kaydi Wareejinta"),
                description: t("Validate and submit the transfer.", "Xaqiiji oo gudbi wareejinta."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ct.cancel"]',
            popover: {
                title: t("Cancel", "Jooji"),
                description: t("Abort without saving changes.", "Ka bax adigoon wax keydin."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="ct.back"]',
            popover: {
                title: t("Back", "Dib U Noqo"),
                description: t("Return to the transfers list.", "Ku noqo liiska wareejinta."),
                side: "top"
            }
        }
    ];

    // Register for the tours-loader
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Transfers/CreateTransfer"] = { version: "v1", steps };

    // Optional manual hook
    const driverObj = driverFactory({ animate: true, showProgress: true, steps });
    window.startTransfersCreateTour = () => driverObj.drive();
})();
