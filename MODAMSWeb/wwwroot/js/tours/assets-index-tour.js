(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = getCurrentLanguage(); // returns "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Assets/CreateAsset tour.");
        return;
    }

    const t = (enText, soText) => (lang === 'so' ? soText : enText);

    window.PAMS_TOUR_REGISTRY = window.PAMS_TOUR_REGISTRY || {};

    window.PAMS_TOUR_REGISTRY["Assets/Index"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="assets.storename"]',
                popover: {
                    title: t('Department / Store', 'Waaxda / Bakhaarka'),
                    description: t(
                        'Shows the department or store the asset belongs to.',
                        'Waxay muujinaysaa waaxda ama bakhaarka ay hantidu ka diiwaangashan tahay.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="assets.storeowner"]',
                popover: {
                    title: t('Store Owner', 'Mas’uulka Bakhaarka'),
                    description: t(
                        'Displays the name of the person responsible for the store.',
                        'Waxay muujinaysaa magaca qofka mas’uulka ka ah bakhaarka.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="assets.filter"]',
                popover: {
                    title: t('Filter', 'Shaandhee'),
                    description: t(
                        'Use filters to view assets by category or subcategory.',
                        'Isticmaal shaandheeyaha si aad u aragto hantida iyadoo lagu kala saarayo qayb ama qayb-hoosaad.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="assets.create"]',
                popover: {
                    title: t('Register New Asset', 'Diiwaangeli Hanti Cusub'),
                    description: t(
                        'Click this button to register a new asset in the system.',
                        'Guji badhankan si aad u diiwaangeliso hanti cusub nidaamka.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="assets.actions"]',
                popover: {
                    title: t('Asset Actions', 'Hawlaha Hantida'),
                    description: t(
                        'Access options such as Asset Info, Edit Asset, Documents, and Pictures.',
                        'Halkan waxaad ka heli kartaa fursadaha sida Macluumaadka Hantida, Tafatirka, Dukumentiyada iyo Sawirrada.'
                    ),
                    side: 'bottom'
                }
            }
        ]
    };

    // ✅ Create driver instance correctly for this page
    const driverObj = driverFactory({
        animate: true,
        showProgress: true,
        steps: window.PAMS_TOUR_REGISTRY["Assets/Index"].steps,
        onHighlightStarted: (element, step) => {
            const text = step?.popover?.description;
            if (text && window.PAMS_TOUR_AUDIO) {
                window.PAMS_TOUR_AUDIO.play(text, lang);
            }
        },
        onDeselected: () => {
            if (window.PAMS_TOUR_AUDIO) window.PAMS_TOUR_AUDIO.stop();
        },
        onDestroyed: () => {
            if (window.PAMS_TOUR_AUDIO) window.PAMS_TOUR_AUDIO.stop();
        }
    });

    // 🔘 Optional manual start
    window.startAssetCreateTour = () => driverObj.drive();
})();
