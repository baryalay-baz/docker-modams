// /wwwroot/js/tours/transfers-edittransfer-tour.js
(function () {
    "use strict";
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) { console.error("Driver.js not loaded for Transfers/EditTransfer."); return; }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    // Best effort: ensure assets accordion is open before pointing at the table
    function ensureAssetsOpen() {
        const toggle = document.querySelector('[data-tour="et.assets.toggle"]');
        const panel = document.getElementById("collapse8");
        if (toggle && panel && !panel.classList.contains("show")) {
            // Bootstrap-aware toggle (works even if BS isn't present; fallback is a click)
            try { toggle.click(); } catch { /* noop */ }
        }
    }

    const steps = [
        // Header & breadcrumbs
        {
            element: '[data-tour="et.title"]',
            popover: {
                title: t("Edit Transfer", "Tafatir Wareejin"),
                description: t("Review and change transfer details.", "Dib u eeg oo beddel faahfaahinta wareejinta."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="et.breadcrumbs"]',
            popover: {
                title: t("Breadcrumbs", "Jid-raac"),
                description: t("Jump back to dashboard or list.", "Ku laabo dashboard ama liiska."),
                side: "bottom"
            }
        },

        // Form overview
        {
            element: '[data-tour="et.header"]',
            popover: {
                title: t("Form Overview", "Guudmar Foomka"),
                description: t("Origin store & number are read-only. You can adjust date, destination, assets, and notes.",
                    "Bakhaarka asalka ah iyo lambarku waa akhris-keli. Waad beddeli kartaa taariikhda, meesha loo wareejinayo, hantida, iyo qoraallada."),
                side: "bottom"
            }
        },

        // Core fields
        {
            element: '[data-tour="et.from"]',
            popover: {
                title: t("From Store", "Laga Wareejinayo"),
                description: t("The originating store of this transfer.", "Bakhaarka laga wareejinayo."),
                side: "right"
            }
        },
        {
            element: '[data-tour="et.number"]',
            popover: {
                title: t("Transfer Number", "Lambarka Wareejinta"),
                description: t("Auto-generated identifier (read-only).", "Aqoonsi si toos ah loo sameeyay (akhris-keli)."),
                side: "left"
            }
        },
        {
            element: '[data-tour="et.date"]',
            popover: {
                title: t("Transfer Date", "Taariikhda Wareejinta"),
                description: t("Adjust the effective date if needed.", "Haddii loo baahdo, beddel taariikhda dhaqan-gelinta."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="et.store"]',
            popover: {
                title: t("To Store", "Loogu Wareejinayo"),
                description: t("Select or correct the destination store.", "Dooro ama sax bakhaarka loo wareejinayo."),
                side: "bottom"
            }
        },

        // Assets selection
        {
            element: '[data-tour="et.assets.section"]',
            popover: {
                title: t("Assets", "Hantida"),
                description: t("Expand the section to add/remove items.", "Furo qaybta si aad wax ugu darto/uga saarto."),
                side: "top"
            }
        },
        {
            element: '[data-tour="et.assets.toggle"]',
            popover: {
                title: t("Select Assets", "Dooro Hantida"),
                description: t("Click to expand the list of available assets.", "Guji si aad u ballaariso liiska hantida la heli karo."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="et.assets.table"]',
            popover: {
                title: t("Assets Table", "Jadwalka Hantida"),
                description: t("Tick items to include in the transfer; untick to remove.", "Calaamadee si loogu daro wareejinta; ka saar calaamadda si looga saaro."),
                side: "top"
            }
        },
        {
            element: '[data-tour="et.assets.checkbox"]',
            popover: {
                title: t("Select/Deselect", "Dooro/Ka Saar"),
                description: t("Use the checkbox on each row.", "Isticmaal sanduuqa saxda ee saf kasta."),
                side: "right"
            }
        },
        {
            element: '[data-tour="et.assets.count"]',
            popover: {
                title: t("Selection Counter", "Tirada Xulashada"),
                description: t("Live total of selected items.", "Wadarta waqtiga-dhabta ee xulashada."),
                side: "left"
            }
        },

        // Notes
        {
            element: '[data-tour="et.notes"]',
            popover: {
                title: t("Notes", "Qoraallo"),
                description: t("Add purpose, handover details, or remarks.", "Ku dar ujeeddo, faahfaahinta wareejinta, ama faallo."),
                side: "top"
            }
        },

        // Actions (save/delete appear only when status == Pending)
        {
            element: '[data-tour="et.save"]',
            popover: {
                title: t("Save Changes", "Kaydi Isbeddellada"),
                description: t("Validate and submit updates.", "Xaqiiji oo gudbi cusboonaysiinta."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="et.delete"]',
            popover: {
                title: t("Delete Transfer", "Tirtir Wareejinta"),
                description: t("Opens a confirmation dialog. Only pending transfers can be deleted.",
                    "Fura sanduuqa xaqiijinta. Kaliya wareejinnada sugaya ayaa la tirtiri karaa."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="et.preview"]',
            popover: {
                title: t("Preview", "Horgaalid"),
                description: t("Open a printable preview of this transfer.", "Fur muuqaal la daabici karo oo wareejintan ah."),
                side: "left"
            }
        },

        // Sidebar selection
        {
            element: '[data-tour="et.selection.card"]',
            popover: {
                title: t("Your Selection", "Xulashadaada"),
                description: t("Monitors items you’ve chosen.", "La soco hantida aad dooratay."),
                side: "left"
            }
        },
        {
            element: '[data-tour="et.selection.table"]',
            popover: {
                title: t("Selected Items", "Waxyaabaha La Doortay"),
                description: t("Updates as you tick and untick rows.", "Wuu cusboonaysiiyaa markaad calaamadeyso/ka saarto."),
                side: "left"
            }
        },

        // Back
        {
            element: '[data-tour="et.back"]',
            popover: {
                title: t("Back", "Dib U Noqo"),
                description: t("Return to the list without further edits.", "Ku noqo liiska adigoon wax kale beddelin."),
                side: "top"
            }
        }
    ];

    // Register for tours-loader.js
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Transfers/EditTransfer"] = { version: "v1", steps };

    // Optional manual start for debugging
    const driverObj = driverFactory({ animate: true, showProgress: true, steps });
    window.startTransfersEditTour = () => {
        ensureAssetsOpen();
        driverObj.drive();
    };
})();
