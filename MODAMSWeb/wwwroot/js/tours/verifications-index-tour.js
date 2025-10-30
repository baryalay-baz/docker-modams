// /wwwroot/js/tours/verifications-index-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (typeof window.getCurrentLanguage === "function"
        ? window.getCurrentLanguage()
        : "en"
    );

    if (!driverFactory) {
        console.error("Driver.js failed to load for Verifications/Index tour.");
        return;
    }

    // bilingual helper
    const t = (enText, soText) => (lang === "so" ? soText : enText);

    // Ensure registry
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    window.AMS_TOUR_REGISTRY["Verifications/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="vi.pagetitle"]',
                popover: {
                    title: t("Asset Verifications", "Hubinta Hantida"),
                    description: t(
                        "This screen is your command center for creating and tracking verification schedules.",
                        "Boggan waa halka aad ka abuurto oo ka maamuli karto jadwalada hubinta hantida."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="vi.newschedule"]',
                popover: {
                    title: t("Create a New Schedule", "Abuur Jadwal Cusub"),
                    description: t(
                        "Click here to start a new verification schedule. Permissions apply—if it's disabled, you probably don’t have rights.",
                        "Riix halkan si aad u bilowdo jadwal hubin oo cusub. Haddii uu naafo yahay, xuquuqda ayaad u baahan tahay."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="vi.table"]',
                popover: {
                    title: t("Schedules Table", "Jadwalada Hubinta"),
                    description: t(
                        "All schedules live here. Use the table to scan department, type, team, dates, status, and actions.",
                        "Dhammaan jadwaladu waxay ku jiraan halkan. Ka akhri waaxda, nooca, kooxda, taariikhaha, xaaladda iyo falalka."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="vi.teamcol"]',
                popover: {
                    title: t("Verification Team", "Kooxda Hubinta"),
                    description: t(
                        "This column shows who is on the verification team. Hover the avatars to see details.",
                        "Tiirkan waxa uu muujinayaa kooxda hubinta. Dul istaag sawirrada si aad u aragto faahfaahin."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="vi.statuscol"]',
                popover: {
                    title: t("Schedule Status", "Xaaladda Jadwalka"),
                    description: t(
                        "Keep an eye on status: Pending, Ongoing, or Completed. That’s your quick progress signal.",
                        "Halkan ka arag xaaladda: Sugaya, Socota, ama La Dhameeyay. Waa tilmaanta horumarka."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="vi.rowactions"]',
                popover: {
                    title: t("Row Actions", "Falalka Safka"),
                    description: t(
                        "Preview to see details, Edit while pending, or Delete if you need to scrap a schedule.",
                        "Isticmaal Muuqaal si aad faahfaahin u aragto, Tafatir marka uu sugayo, ama Tir si aad u baabi'iso jadwalka."
                    ),
                    side: "left"
                }
            },
            {
                element: '[data-tour="vi.chart"]',
                popover: {
                    title: t("Status Summary Chart", "Jaantuska Xaaladda"),
                    description: t(
                        "This chart summarizes schedules by status—handy for quick reporting and spotting bottlenecks.",
                        "Jaantuskani waxa uu soo koobayaa jadwalada sida ay xaalad ahaan u kala jiraan—si degdeg ah u ogow dib-u-dhacyo."
                    ),
                    side: "left"
                }
            }
        ]
    };

    // Driver instance with audio hooks (optional)
    const driverObj = driverFactory({
        animate: true,
        showProgress: true,
        steps: window.AMS_TOUR_REGISTRY["Verifications/Index"].steps,
        onHighlightStarted: (element, step) => {
            const text = step?.popover?.description;
            if (text && window.AMS_TOUR_AUDIO) {
                window.AMS_TOUR_AUDIO.play(text, lang);
            }
        },
        onDeselected: () => {
            if (window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.stop();
        },
        onDestroyed: () => {
            if (window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.stop();
        }
    });

    // Optional manual trigger if you ever need it
    window.startVerificationsIndexTour = () => driverObj.drive();
})();
