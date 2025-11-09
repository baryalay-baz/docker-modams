(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (typeof getCurrentLanguage === "function") ? getCurrentLanguage() : "en";
    if (!driverFactory) {
        console.error("Driver.js failed to load for Reporting/Index tour.");
        return;
    }

    // i18n helper
    const t = (en, so) => (lang === "so" ? so : en);

    // Ensure a global registry exists
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    // Small helpers used by steps
    const click = (sel) => document.querySelector(sel)?.click();

    window.AMS_TOUR_REGISTRY["Reporting/Index"] = {
        version: "v1",
        steps: [
            // ====== Header / Breadcrumbs / Layout ======
            {
                element: '[data-tour="rp.title"]',
                popover: {
                    title: t("Reports", "Warbixinno"),
                    description: t(
                        "This page lets you build and preview Asset, Transfer, and Disposal reports.",
                        "Boggani wuxuu kuu oggolaanayaa inaad dhisto oo aad eegto warbixinaha Hantida, Wareejinta, iyo Qashin-bixinta."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="rp.breadcrumbs"]',
                popover: {
                    title: t("Breadcrumbs", "Tilsan-raad"),
                    description: t(
                        "Use these to jump back to the Dashboard.",
                        "U isticmaal si aad ugu noqoto Dashboard-ka."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="rp.layout"]',
                popover: {
                    title: t("Two-Pane Layout", "Qaabka Laba-Qeybood"),
                    description: t(
                        "Left: pick a report, presets, and filters. Right: the live preview.",
                        "Bidix: dooro warbixin, presets, iyo sifee. Midig: muuqaalka tijaabada (preview)."
                    ),
                    side: "top"
                }
            },

            // ====== Left Panel (Catalog / Search) ======
            {
                element: '[data-tour="rp.left"]',
                popover: {
                    title: t("Report Builder", "Dhise Warbixin"),
                    description: t(
                        "Everything you need to configure a report is here.",
                        "Wax walba oo aad ugu baahan tahay dejinta warbixinta waxay ku jiraan halkan."
                    ),
                    side: "right"
                }
            },
            {
                element: '[data-tour="rp.search"]',
                popover: {
                    title: t("Search Reports", "Raadi Warbixinno"),
                    description: t(
                        "Filter the catalog by typing a report name.",
                        "Ku shaandhee buug-yaraha adigoo qoreya magaca warbixinta."
                    ),
                    side: "bottom"
                }
            },

            // ====== Catalog Tiles ======
            {
                element: '[data-tour="rp.tile.asset"]',
                popover: {
                    title: t("Assets Report", "Warbixinta Hantida"),
                    description: t(
                        "Click to focus the Assets filters below.",
                        "Riix si diiradda loo saaro sifeeyeyaasha Hantida ee hoose."
                    ),
                    side: "top"
                },
                onNextClick: () => click('[data-tour="rp.tile.asset"]')
            },
            {
                element: '[data-tour="rp.tile.transfer"]',
                popover: {
                    title: t("Transfers Report", "Warbixinta Wareejinta"),
                    description: t(
                        "Shows movement of assets between stores.",
                        "Waxay muujisaa dhaq-dhaqaaqa hantida ee u dhexeeya bakhaarada."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="rp.tile.disposal"]',
                popover: {
                    title: t("Disposals Report", "Warbixinta Qashin-bixinta"),
                    description: t(
                        "Summarizes assets sent to disposal with type and date.",
                        "Waxay soo koobtaa hantida loo diro qashin-bixin nooc iyo taariikh ahaan."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="rp.tile.verification"]',
                popover: {
                    title: t("Verifications", "Hubinno"),
                    description: t(
                        "Planned module. The tile is disabled for now.",
                        "Qayb qorsheysan. Hadda waa naafo."
                    ),
                    side: "top"
                }
            },

            // ====== Presets / Panel Title / Actions ======
            {
                element: '[data-tour="rp.panel.title"]',
                popover: {
                    title: t("Current Report", "Warbixinta Hadda"),
                    description: t(
                        "This title changes to match the selected report tile.",
                        "Cinwaankan wuu isbeddelaa si uu ula jaanqaado warbixinta la doortay."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="rp.presets"]',
                popover: {
                    title: t("Presets", "Preset-yo"),
                    description: t(
                        "Quick starting points like All, By Store, or By Status.",
                        "Meelo bilow degdeg ah sida Dhammaan, Bakhaar ahaan, ama Xaalad ahaan."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="rp.actions"]',
                popover: {
                    title: t("Actions", "Falal"),
                    description: t(
                        "Preview in the pane or export/open via the menu. The Preview button enables once required filters are valid.",
                        "Ka eeg qaybta midig ama dhoofso/fur adigoo adeegsanaya menu-ga. Badhamka Muuji wuxuu shaqeeyaa marka sifeeyeyaasha looga baahan yahay ay sax yihiin."
                    ),
                    side: "left"
                }
            },

            // ====== Filters ======
            {
                element: '[data-tour="rp.filters"]',
                popover: {
                    title: t("Filters", "Sifeeyayaal"),
                    description: t(
                        "Choose Store/Status/Category/Condition/Donor. You can also set Date From/Date To.",
                        "Dooro Bakhaar/Xaalad/Qeyb/Xaalad Hanti/Deeq-bixiye. Waxa kale oo aad dejin kartaa Laga bilaabo/Ilā taariikh."
                    ),
                    side: "right"
                }
            },

            // ====== Preview Pane (Right) ======
            {
                element: '#rpPreviewFrame',
                popover: {
                    title: t("Live Preview", "Muuqaal Tijaabo"),
                    description: t(
                        "The report renders here after you click Preview.",
                        "Warbixintu waxay halkan ka muuqan doontaa markaad gujiso Muuji."
                    ),
                    side: "left"
                }
            },
            {
                element: '#rpEmpty',
                popover: {
                    title: t("Empty State", "Xaalad Madhan"),
                    description: t(
                        "Until you pick filters and click Preview, this message is shown.",
                        "Ilaa aad doorato sifeeyeyaasha oo aad gujiso Muuji, farriintan ayaa muuqanaysa."
                    ),
                    side: "left"
                }
            }
        ]
    };
})();
