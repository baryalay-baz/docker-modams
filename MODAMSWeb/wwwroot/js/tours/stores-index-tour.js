(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = getCurrentLanguage(); // returns "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Stores/Index tour.");
        return;
    }

    const t = (enText, soText) => (lang === 'so' ? soText : enText);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    window.AMS_TOUR_REGISTRY["Stores/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="stores.search"]',
                popover: {
                    title: t('Search for Store', 'Raadi Bakhaarka'),
                    description: t(
                        'Start typing a Department Name here to automatically filter and find the related store.',
                        'Ku qor magaca waaxda halkan si aad si toos ah ugu shaandhayso oo u hesho bakhaarka la xiriira.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="stores.assetreport"]',
                popover: {
                    title: t('Asset Report', 'Warbixinta Hantida'),
                    description: t(
                        'Click here to generate an Asset Report for this store.',
                        'Guji halkan si aad u abuurto Warbixinta Hantida ee bakhaarkan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="stores.storetype"]',
                popover: {
                    title: t('Store Type', 'Nooca Bakhaarka'),
                    description: t(
                        'Defines whether this store is a Primary or Secondary Store. Only authorized users can manage assets in a Primary Store.',
                        'Tani waxay qeexaysaa in bakhaarku yahay mid Aasaasi ah ama Labaad. Kaliya isticmaalayaasha la oggolaaday ayaa maamuli kara hantida ku jirta Bakhaarka Aasaasiga ah.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="stores.memberlist"]',
                popover: {
                    title: t('Member List', 'Liiska Xubnaha'),
                    description: t(
                        'View the list of members assigned to this store.',
                        'Daawo liiska xubnaha loo xilsaaray bakhaarkan.'
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
        steps: window.AMS_TOUR_REGISTRY["Stores/Index"].steps,

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
