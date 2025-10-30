// /wwwroot/js/tours/verifications-editschedule-tour.js
(function () {
    "use strict";

    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) { console.error("Driver.js not loaded for Verifications/EditSchedule."); return; }

    const lang = (typeof window.getCurrentLanguage === "function")
        ? window.getCurrentLanguage()
        : "en";
    const t = (en, so) => (lang === "so" ? so : en);

    // Keep overlay nicely above page
    (function zPatch() {
        const id = "ams-driver-zpatch";
        if (document.getElementById(id)) return;
        const css = document.createElement("style");
        css.id = id;
        css.textContent = `
      .driver-popover{ max-width: 420px; }
      .pams-tour-popover{ }
      .driver-under .modal-backdrop { z-index: 1050 !important; }
      .driver-under .modal { z-index: 1055 !important; }
    `;
        document.head.appendChild(css);
    })();

    const steps = [
        // Header
        {
            element: '[data-tour="ves.pagetitle"]',
            popover: {
                title: t("Edit Verification Schedule", "Beddel Jadwalka Hubinta"),
                description: t("Modify schedule details, team, and dates here.", "Halkan ka beddel faahfaahinta jadwalka, kooxda, iyo taariikhaha."),
                side: "bottom", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.headerCard"]',
            popover: {
                title: t("Page Header", "Cinwaanka Bogga"),
                description: t("High-level context for the schedule you’re editing.", "Dulmar guud oo ku saabsan jadwalka aad wax ka beddelayso."),
                side: "top", className: "pams-tour-popover"
            }
        },

        // Left form
        {
            element: '[data-tour="ves.department"]',
            popover: {
                title: t("Department", "Waaxda"),
                description: t("Choose the department (store) this schedule belongs to.", "Dooro waaxda/dukanka jadwalkani khuseeyo."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.type"]',
            popover: {
                title: t("Verification Type", "Nooca Hubinta"),
                description: t("Full = all assets; Custom = you enter the number to verify.", "Full = dhammaan; Custom = adiga ayaad dhigaysaa tirada la hubinayo."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.numInStore"]',
            popover: {
                title: t("Assets in Store", "Hantida Dukaanka"),
                description: t("Read-only count of assets in the selected department.", "Tirada guud ee hantida waaxda la doortay (akhris-kaliya)."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.numToVerify"]',
            popover: {
                title: t("Assets to Verify", "Hantida la Hubinayo"),
                description: t("Editable only for Custom verification; must be between 1 and the store’s total.", "Waxaa la beddeli karaa marka Custom; waa inuu ahaadaa inta u dhexeysa 1 iyo tirada guud."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.start"]',
            popover: {
                title: t("Start Date", "Taariikhda Bilowga"),
                description: t("When verification begins.", "Goorta hubintu bilaabaneyso."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.end"]',
            popover: {
                title: t("End Date", "Taariikhda Dhameyska"),
                description: t("Planned completion date.", "Taariikhda la qorsheeyay ee dhameystirka."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.notes"]',
            popover: {
                title: t("Notes", "Qoraallo"),
                description: t("Add instructions or context for the verification team.", "Ku dar tilmaamo ama macluumaad kooxda."),
                side: "left", className: "pams-tour-popover"
            }
        },

        // Team panel
        {
            element: '[data-tour="ves.team.card"]',
            popover: {
                title: t("Team Members", "Xubnaha Kooxda"),
                description: t("Manage the team assigned to this schedule.", "Maamul kooxda loo xilsaaray jadwalkan."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.team.table"]',
            popover: {
                title: t("Selected Members", "Xubnaha la Doortay"),
                description: t("Members you add appear here; remove with the (x).", "Kuwii aad dartay halkan ayay ka muuqdaan; (x) ayaad kaga saari."),
                side: "top", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.team.employee"]',
            popover: {
                title: t("Choose Employee", "Dooro Shaqaale"),
                description: t("Pick a person to add to the team.", "Dooro qof aad ku dartid kooxda."),
                side: "left", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.team.role"]',
            popover: {
                title: t("Assign Role", "Sii Doorka"),
                description: t("Max one Team Leader; others are Team Members.", "Hal Hoggaamiye Koox; inta kale waa Xubno Koox."),
                side: "left", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.team.add"]',
            popover: {
                title: t("Add to Team", "Ku dar Kooxda"),
                description: t("Click to append the selected person + role.", "Riix si aad u darto shaqaalaha + doorka."),
                side: "left", className: "pams-tour-popover"
            }
        },

        // Footer actions (exclude confirmation modal from tour)
        {
            element: '[data-tour="ves.submit"]',
            popover: {
                title: t("Save Schedule", "Kaydi Jadwalka"),
                description: t("Requires a valid number and at least one team member.", "Waxay u baahan tahay tiro sax ah iyo ugu yaraan hal xubin."),
                side: "right", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.preview"]',
            popover: {
                title: t("Preview", "Eeg Hordhac"),
                description: t("Open the schedule preview, including charts.", "Fur hordhaca jadwalka, oo ay ku jiraan jaantusyada."),
                side: "right", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.delete"]',
            popover: {
                title: t("Delete Schedule", "Tirtir Jadwalka"),
                description: t("Opens a confirmation dialog. This step doesn’t open it in the tour.", "Waxay furtaa xaqiijin; tallaabadani ma fureyso intaan socdaalka ku jiro."),
                side: "right", className: "pams-tour-popover"
            }
        },
        {
            element: '[data-tour="ves.cancel"]',
            popover: {
                title: t("Cancel", "Jooji"),
                description: t("Go back without saving changes.", "Ku noqo adigoon wax kaydin."),
                side: "right", className: "pams-tour-popover"
            }
        }
    ];

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Verifications/EditSchedule"] = {
        version: "v1",
        steps
    };

    const driver = driverFactory({
        animate: true,
        showProgress: true,
        steps,
        onHighlightStarted: (_el, step) => {
            document.body.classList.add("driver-under");
            const txt = step?.popover?.description;
            if (txt && window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.play(txt, lang);
        },
        onDeselected: () => { if (window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.stop?.(); },
        onDestroyed: () => { if (window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.stop?.(); document.body.classList.remove("driver-under"); }
    });

    // Optional manual trigger
    window.startEditScheduleTour = () => driver.drive();
})();
