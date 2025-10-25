(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = getCurrentLanguage(); // returns "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Stores/Index tour.");
        return;
    }

    const t = (enText, soText) => (lang === 'so' ? soText : enText);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    window.AMS_TOUR_REGISTRY["Home/GlobalSearch"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="gs.assetpicture"]',
                popover: {
                    title: t('Asset Picture', 'Sawirka Hantida'),
                    description: t(
                        'The official image representing the asset.',
                        'Sawirka rasmiga ah ee muujinaya hantida.'
                    ),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="gs.department"]',
                popover: {
                    title: t('Department / Store', 'Waaxda / Bakhaarka'),
                    description: t(
                        'The department or store responsible for the asset.',
                        'Waaxda ama bakhaarka mas’uulka ka ah hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="gs.category"]',
                popover: {
                    title: t('Asset Category', 'Qaybta Hantida'),
                    description: t(
                        'The main classification group of the asset.',
                        'Qaybta ugu weyn ee hantida lagu kala saaro.'
                    ),
                    side: 'left'
                }
            },
            {
                element: '[data-tour="gs.subcategory"]',
                popover: {
                    title: t('Asset Subcategory', 'Qaybta Hoose ee Hantida'),
                    description: t(
                        'A more specific classification within the asset category.',
                        'Kala sooc faahfaahsan oo hoos yimaada qaybta hantida.'
                    ),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="gs.assetname"]',
                popover: {
                    title: t('Asset Name', 'Magaca Hantida'),
                    description: t(
                        'The official name or title of the asset.',
                        'Magaca ama cinwaanka rasmiga ah ee hantida.'
                    ),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="gs.make"]',
                popover: {
                    title: t('Asset Make (Brand)', 'Soo-saare / Summad'),
                    description: t(
                        'The company or brand that manufactured the asset.',
                        'Shirkadda ama summadda soo saartay hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="gs.model"]',
                popover: {
                    title: t('Asset Model', 'Moodelka Hantida'),
                    description: t(
                        'The model or version of the asset.',
                        'Nooca ama moodelka hantidan.'
                    ),
                    side: 'left'
                }
            },
            {
                element: '[data-tour="gs.specs"]',
                popover: {
                    title: t('Asset Specifications', 'Sifooyinka Hantida'),
                    description: t(
                        'The technical details and features of the asset.',
                        'Faahfaahinta farsamo iyo sifooyinka hantidan.'
                    ),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="gs.assetinfo"]',
                popover: {
                    title: t('Asset Information', 'Xogta Hantida'),
                    description: t(
                        'Complete information about the asset.',
                        'Xogta dhamaystiran ee ku saabsan hantidan.'
                    ),
                    side: 'right'
                }
            }
        ]
    };

    // ✅ Create driver instance
    const driverObj = driverFactory({
        animate: true,
        showProgress: true,
        steps: window.AMS_TOUR_REGISTRY["Home/GlobalSearch"].steps,

        // 🔊 Audio playback hook for each step
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

    // 🔘 Optional manual start
    window.startStoresTour = () => driverObj.drive();
})();
