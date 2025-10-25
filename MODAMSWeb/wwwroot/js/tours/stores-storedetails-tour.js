// wwwroot/js/tours/stores-storedetails-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = getCurrentLanguage(); // returns "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Stores/StoreDetails tour.");
        return;
    }

    // Translation helper
    const t = (enText, soText) => (lang === 'so' ? soText : enText);

    // Make sure the registry exists globally
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    // Register the Stores/StoreDetails tour
    window.AMS_TOUR_REGISTRY["Stores/StoreDetails"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="sd.departmentname"]',
                popover: {
                    title: t('Store / Department Name', 'Magaca Bakhaarka / Waaxda'),
                    description: t(
                        'Displays the name of the Store or Department.',
                        'Waxay muujinaysaa magaca Bakhaarka ama Waaxda.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.storeowner"]',
                popover: {
                    title: t('Store Owner', 'Milkiilaha Bakhaarka'),
                    description: t(
                        'Displays the name of the Store Owner.',
                        'Waxay muujinaysaa magaca Milkiilaha Bakhaarka.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.registered"]',
                popover: {
                    title: t('Registered Assets', 'Hantida Diiwaangashan'),
                    description: t(
                        'Shows all assets registered in this store. Click to view the list.',
                        'Waxay muujinaysaa hantida oo dhan ee bakhaarkan lagu diiwaangeliyey. Guji si aad u aragto liiska.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.received"]',
                popover: {
                    title: t('Received Assets', 'Hantida La Helay'),
                    description: t(
                        'Shows all assets received from another store through the Asset Transfer process. Click to view the list.',
                        'Waxay muujinaysaa hantida laga soo wareejiyey bakhaar kale iyada oo loo marayo habka Wareejinta Hantida. Guji si aad u aragto liiska.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.transfered"]',
                popover: {
                    title: t('Transferred Assets', 'Hantida La Wareejiyey'),
                    description: t(
                        'Shows all assets transferred to another store through the Asset Transfer process. Click to view the list.',
                        'Waxay muujinaysaa hantida loo wareejiyey bakhaar kale iyada oo loo marayo habka Wareejinta Hantida. Guji si aad u aragto liiska.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.handover"]',
                popover: {
                    title: t('Handed Over Assets', 'Hantida La Dhiibay'),
                    description: t(
                        'Shows all assets handed over to employees through the Asset Handover process. Click to view the list.',
                        'Waxay muujinaysaa hantida loo dhiibay shaqaalaha iyada oo loo marayo habka Dhiibista Hantida. Guji si aad u aragto liiska.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.disposal"]',
                popover: {
                    title: t('Disposed Assets', 'Hantida La Tuuray'),
                    description: t(
                        'Shows all assets disposed of through the Asset Disposal process. Click to view the list.',
                        'Waxay muujinaysaa hantida laga saaray nidaamka iyada oo loo marayo habka Tuurista Hantida. Guji si aad u aragto liiska.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.balance"]',
                popover: {
                    title: t('Asset Balance', 'Hantida Hadda Jirta'),
                    description: t(
                        'Displays the total assets currently available in this store.',
                        'Waxay muujinaysaa hantida guud ee hadda ku jirta bakhaarkan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.register"]',
                popover: {
                    title: t('Register New Asset', 'Diiwaangeli Hanti Cusub'),
                    description: t(
                        'Takes you to the Asset Registration form.',
                        'Waxay ku geyneysaa foomka Diiwaangelinta Hantida.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.summary"]',
                popover: {
                    title: t('Asset Summary', 'Soo Koobidda Hantida'),
                    description: t(
                        'Displays a summary of all assets in this store, grouped by Category and Subcategory.',
                        'Waxay muujinaysaa soo koobidda hantida bakhaarkan, iyadoo loo kala saaray Qayb iyo Hoos-Qayb.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="sd.chart"]',
                popover: {
                    title: t('Chart by Subcategory', 'Jaantuska Hoos-Qaybta'),
                    description: t(
                        'Displays a visual chart of assets by Subcategory.',
                        'Waxay muujinaysaa jaantuska hantida iyadoo loo kala saaray Hoos-Qaybaha.'
                    ),
                    side: 'bottom'
                }
            }
        ]
    };
})();
