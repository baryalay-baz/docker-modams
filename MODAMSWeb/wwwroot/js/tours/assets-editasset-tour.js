(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = getCurrentLanguage(); // returns "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Assets/EditAsset tour.");
        return;
    }

    const t = (enText, soText) => (lang === 'so' ? soText : enText);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    window.AMS_TOUR_REGISTRY["Assets/EditAsset"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="ea.category"]',
                popover: {
                    title: t('Asset Category', 'Qaybta Hantida'),
                    description: t(
                        'Select the main category that best represents the asset.',
                        'Dooro qaybta ugu weyn ee si fiican u metelaysa hantida.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.subcategory"]',
                popover: {
                    title: t('Asset Subcategory', 'Qayb-hoosaadka Hantida'),
                    description: t(
                        'Choose a subcategory to further classify the asset.',
                        'Dooro qayb-hoosaad si aad u kala saarto hantida si faahfaahsan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.name"]',
                popover: {
                    title: t('Asset Name', 'Magaca Hantida'),
                    description: t(
                        'Enter the full name or title of the asset.',
                        'Geli magaca buuxa ama cinwaanka hantida.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.make"]',
                popover: {
                    title: t('Make / Manufacturer', 'Soo Saaraha'),
                    description: t(
                        'Specify the brand or manufacturer of the asset.',
                        'Sheeg calaamadda ama shirkadda soo saartay hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.model"]',
                popover: {
                    title: t('Model', 'Qaabka / Nooca'),
                    description: t(
                        'Provide the specific model or version of the asset.',
                        'Geli nooca ama nambarka qaabka hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.year"]',
                popover: {
                    title: t('Manufacture Year', 'Sannadka Soo Saaridda'),
                    description: t(
                        'Enter the year this asset was manufactured or released.',
                        'Geli sannadka la soo saaray ama la sii daayay hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.vehicleinfo"]',
                popover: {
                    title: t('Vehicle Identification', 'Aqoonsiga Gaadhiga'),
                    description: t(
                        'If this asset is a vehicle, fill in the Engine, Chassis, and Plate number details.',
                        'Haddii hantidan tahay gaadhi, geli faahfaahinta matoorka, shassiska iyo lambarka taarikada.'
                    ),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="ea.country"]',
                popover: {
                    title: t('Manufacturing Country', 'Dalka Soo Saaray'),
                    description: t(
                        'Mention the country where the asset was manufactured.',
                        'Sheeg dalka lagu soo saaray hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.serial"]',
                popover: {
                    title: t('Serial Number', 'Lambarka Silsiladda'),
                    description: t(
                        'Enter the asset’s unique serial number (if available).',
                        'Geli lambarka gaarka ah ee hantidan (haddii uu jiro).'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.barcode"]',
                popover: {
                    title: t('Barcode', 'Koodhka Baarka'),
                    description: t(
                        'Scan or enter the barcode assigned to this asset.',
                        'Iskaani ama geli koodhka baarka ee loo qoondeeyay hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.specifications"]',
                popover: {
                    title: t('Specifications', 'Faahfaahinta Hantida'),
                    description: t(
                        'Provide key specifications or details describing this asset.',
                        'Geli faahfaahinta muhiimka ah ee sharxaysa hantidan.'
                    ),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="ea.cost"]',
                popover: {
                    title: t('Cost', 'Qiimaha'),
                    description: t(
                        'Enter the total cost or purchase price of the asset.',
                        'Geli qiimaha guud ama lacagta lagu iibsaday hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.purchasedate"]',
                popover: {
                    title: t('Purchase Date', 'Taariikhda Iibsiga'),
                    description: t(
                        'Select the date when the asset was purchased.',
                        'Dooro taariikhda la iibsaday hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.receiptdate"]',
                popover: {
                    title: t('Receipt Date', 'Taariikhda Lagu Helay'),
                    description: t(
                        'Enter the date when the asset was received by the organization.',
                        'Geli taariikhda la helay hantidan ee ururku gacanta ku dhigay.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.po"]',
                popover: {
                    title: t('Purchase Order Number', 'Lambarka Amarka Iibsiga'),
                    description: t(
                        'Enter the Purchase Order number associated with this asset.',
                        'Geli lambarka amarka iibsiga ee la xiriira hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.procuredby"]',
                popover: {
                    title: t('Procured By', 'Waxaa Soo Iibsaday'),
                    description: t(
                        'Select the entity that procured this asset (e.g., UNOPS, EU, Somalia).',
                        'Dooro hay’adda ama cidda soo iibsatay hantidan (tusaale UNOPS, EU, Somalia).'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.donor"]',
                popover: {
                    title: t('Donor', 'Deeq-bixiye'),
                    description: t(
                        'Select the donor that funded or contributed this asset.',
                        'Dooro deeq bixiyaha maalgeliyay ama bixiyay hantidan.'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.condition"]',
                popover: {
                    title: t('Condition', 'Xaaladda Hantida'),
                    description: t(
                        'Specify the current condition of the asset (e.g., New, Good, Damaged).',
                        'Sheeg xaaladda hadda ee hantidan (tusaale Cusub, Wanaagsan, Jaban).'
                    ),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="ea.remarks"]',
                popover: {
                    title: t('Remarks', 'Faallooyin'),
                    description: t(
                        'Add any relevant notes or additional comments about this asset.',
                        'Ku dar qoraallo ama faallooyin muhiim ah oo la xiriira hantidan.'
                    ),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="ea.submit"]',
                popover: {
                    title: t('Save Asset', 'Kaydi Hantida'),
                    description: t(
                        'Once all fields are filled, click here to save the asset record.',
                        'Markaad buuxiso dhammaan xogta, riix halkan si aad u kaydiso hantida.'
                    ),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="ea.cancel"]',
                popover: {
                    title: t('Cancel Changes', 'Jooji Isbedelada'),
                    description: t(
                        'Cancel all the changes made.',
                        'Jooji dhammaan isbeddelladii la sameeyay.'
                    ),
                    side: 'top'
                }
            }
        ]
    };

    // ✅ Create driver instance correctly for this page
    const driverObj = driverFactory({
        animate: true,
        showProgress: true,
        steps: window.AMS_TOUR_REGISTRY["Assets/EditAsset"].steps,
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
