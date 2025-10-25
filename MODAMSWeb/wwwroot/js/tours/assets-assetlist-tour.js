(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = getCurrentLanguage(); // returns "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Assets/AssetList tour.");
        return;
    }

    const t = (enText, soText) => (lang === 'so' ? soText : enText);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    window.AMS_TOUR_REGISTRY["Assets/AssetList"] = {
        version: "v1",
        steps: [
            
            {
                element: '[data-tour="al.filter"]',
                popover: {
                    title: t('Filter', 'Shaandhee'),
                    description: t(
                        'Use the filters to view assets by category or subcategory.',
                        'Isticmaal shaandhaynta si aad u aragto hantida iyadoo loo kala saarayo qayb ama qayb-hoosaad.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="al.assetinfo"]',
                popover: {
                    title: t('Asset Information', 'Macluumaadka Hantida'),
                    description: t(
                        'View full details for this asset, including status, location, serial number, and more.',
                        'Halkani waxaad ka arki kartaa faahfaahinta hantidan — sida xaaladdeeda, goobta ay taallo, lambarka serial-ka, iyo wixii kaloo khuseeya.'
                    ),
                    side: 'bottom'
                }
            }


        ]
    };

    const driverObj = driverFactory({
        animate: true,
        showProgress: true,
        steps: window.AMS_TOUR_REGISTRY["Assets/AssetList"].steps,
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
    window.startAssetCreateTour = () => driverObj.drive();
})();
