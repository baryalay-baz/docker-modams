// /wwwroot/js/tours/verifications-index-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (window.getCurrentLanguage && window.getCurrentLanguage()) || "en";
    if (!driverFactory) {
        console.error("Driver.js failed to load for Verifications/Index tour.");
        return;
    }

    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Verifications/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="vi.pagetitle"]',
                popover: {
                    title: t("Asset Verifications", "Hubinta Hantida"),
                    description: t("This page lists verification schedules and their status.", "Boggan waxa uu soo bandhigayaa jadwalada hubinta iyo xaaladooda."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="vi.breadcrumbs"]',
                popover: {
                    title: t("Navigation", "Hagid"),
                    description: t("Use breadcrumbs to return to the Dashboard.", "Isticmaal waddooyinka dusha si aad ugu noqoto Dasborka."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="vi.newschedule"]',
                popover: {
                    title: t("Create Schedule", "Abuur Jadwal"),
                    description: t("Start a new verification schedule for a department/store.", "Bilow jadwal hubin oo cusub waax/bakhaar."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="vi.table"]',
                popover: {
                    title: t("Schedules Table", "Jadwalada"),
                    description: t("Each row shows the department, team, period, and status.", "Saf kasta waxa uu muujinayaa waaxda, kooxda, muddada, iyo xaaladda."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="vi.col.team"]',
                popover: {
                    title: t("Team", "Kooxda"),
                    description: t("Hover avatars to see member role and email.", "Ku dul wareeji sawirrada si aad u aragto doorka iyo iimaylka."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="vi.col.status"]',
                popover: {
                    title: t("Status", "Xaaladda"),
                    description: t("Statuses help you track progress: Pending, Ongoing, Completed.", "Xaaladuhu waxay kaa caawinayaan inaad la socoto horumarka: Sugaya, Socota, Dhammaystiran."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="vi.table"]',
                popover: {
                    title: t("Row Actions (Hover)", "Ficillada Safka (Hover)"),
                    description: t(
                        "Hover a row to reveal action pills: Preview (always), Edit & Delete (only when Pending).",
                        "Ku dul wareeji saf si aad u aragto badhamada: Daawo (had iyo jeer), Tafatir & Tirtir (keliya marka ay Sugayaan)."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="vi.chart"]',
                popover: {
                    title: t("Status Overview", "Dulmar Xaalad"),
                    description: t("Bar chart summarises schedules by status.", "Jaantusku waxa uu soo koobayaa jadwalada iyadoo lagu kala saarayo xaalad."),
                    side: "left"
                }
            }
        ]
    };

    // Optional manual starter if you ever want it
    window.startVerificationsIndexTour = () => {
        const driver = driverFactory({
            animate: true,
            showProgress: true,
            steps: window.AMS_TOUR_REGISTRY["Verifications/Index"].steps
        });
        driver.drive();
    };
})();
