// /wwwroot/js/tours/verifications-createschedule-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (typeof window.getCurrentLanguage === "function") ? window.getCurrentLanguage() : "en";
    if (!driverFactory) { console.error("Driver.js not loaded for Verifications/CreateSchedule."); return; }
    const t = (en, so) => (lang === "so" ? so : en);

    // Overlay ordering so the page greys correctly during tour
    (function zPatch() {
        const id = "ams-driver-zpatch";
        if (document.getElementById(id)) return;
        const css = document.createElement("style"); css.id = id;
        css.textContent = `
      .driver-under .modal-backdrop { z-index:1050!important; }
      .driver-under .modal{ z-index:1055!important; }
    `;
        document.head.appendChild(css);
    })();

    const steps = [
        {
            element: '[data-tour="vcs.pagetitle"]',
            popover: {
                title: t("Create Verification Schedule", "Abuur Jadwalka Hubinta"),
                description: t("Use this form to create a verification schedule for the selected department.", "Foomkan ku abuur jadwal hubin waaxda la xushay."),
                side: "bottom"
            }
        },

        {
            element: '[data-tour="vcs.headerCard"]',
            popover: {
                title: t("Page Header", "Cinwaanka Bogga"),
                description: t("Quick summary and context for this task.", "Dulmar degdeg ah oo shaqadan ah."),
                side: "top"
            }
        },

        // Left form
        {
            element: '[data-tour="vcs.department"]',
            popover: {
                title: t("Department", "Waaxda"),
                description: t("Where the verification will take place.", "Halka hubintu ka dhaceyso."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.type"]',
            popover: {
                title: t("Verification Type", "Nooca Hubinta"),
                description: t("Full = all assets. Custom = specify your own number.", "Full = dhammaan; Custom = adigu dhig tirada."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.numInStore"]',
            popover: {
                title: t("Assets in Store", "Hantida Dukaanka"),
                description: t("Total assets currently in this store.", "Tirada guud ee hantida dukaankan."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.numToVerify"]',
            popover: {
                title: t("Assets to Verify", "Hantida la Hubinayo"),
                description: t("Auto-fills for Full; becomes editable for Custom.", "Si toos ah u buuxiya Full; Custom waa la beddeli karaa."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.start"]',
            popover: {
                title: t("Start Date", "Taariikhda Bilowga"),
                description: t("When verification begins.", "Goorta hubintu bilaabaneyso."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.end"]',
            popover: {
                title: t("End Date", "Taariikhda Dhameyska"),
                description: t("Planned completion date.", "Taariikhda la qorsheeyay ee dhameystirka."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.notes"]',
            popover: {
                title: t("Notes", "Qoraallo"),
                description: t("Add instructions or context for the team.", "Ku dar tilmaamo ama macluumaad kooxda."),
                side: "left"
            }
        },

        // Team column (no modal steps)
        {
            element: '[data-tour="vcs.team.card"]',
            popover: {
                title: t("Team Members", "Xubnaha Kooxda"),
                description: t("Build the team and assign roles.", "Dhiso kooxda oo sii doorkooda."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.team.table"]',
            popover: {
                title: t("Selected Members", "Xubnaha la Doortay"),
                description: t("Members you add appear here; remove with (x).", "Kuwii aad dartay halkan ayay ka muuqdaan; (x) ayaad kaga saari."),
                side: "top"
            }
        },
        {
            element: '[data-tour="vcs.team.employee"]',
            popover: {
                title: t("Choose Employee", "Dooro Shaqaale"),
                description: t("Pick a person to add to the team.", "Dooro qof aad ku dartid kooxda."),
                side: "left"
            }
        },
        {
            element: '[data-tour="vcs.team.role"]',
            popover: {
                title: t("Assign Role", "Sii Doorka"),
                description: t("Max one Team Leader; rest are Team Members.", "Hal Hoggaamiye Koox; inta kale Xubno Koox."),
                side: "left"
            }
        },
        {
            element: '[data-tour="vcs.team.add"]',
            popover: {
                title: t("Add to Team", "Ku dar Kooxda"),
                description: t("Click to append selected person + role.", "Riix si aad u darto shaqaalaha + doorka."),
                side: "left"
            }
        },

        // Footer actions
        {
            element: '[data-tour="vcs.submit"]',
            popover: {
                title: t("Save Schedule", "Kaydi Jadwalka"),
                description: t("Requires valid count and at least one team member.", "Waxay u baahan tahay tiro sax ah iyo ugu yaraan hal xubin."),
                side: "right"
            }
        },
        {
            element: '[data-tour="vcs.cancel"]',
            popover: {
                title: t("Cancel", "Jooji"),
                description: t("Go back without saving changes.", "Ku noqo adigoon wax kaydin."),
                side: "right"
            }
        }
    ];

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Verifications/CreateSchedule"] = {
        version: "v2-no-store-modal",
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
        onDestroyed: () => {
            if (window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.stop?.();
            document.body.classList.remove("driver-under");
        }
    });

    window.startCreateScheduleTour = () => driver.drive();
})();
