// /wwwroot/js/tours/assets-assetpictures-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (typeof window.getCurrentLanguage === "function"
        ? window.getCurrentLanguage()
        : "en"
    ); // "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Assets/AssetPictures tour.");
        return;
    }

    // bilingual helper
    const t = (enText, soText) => (lang === "so" ? soText : enText);

    // make sure global registry exists
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    // register steps for this page
    window.AMS_TOUR_REGISTRY["Assets/AssetPictures"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="ap.pagetitle"]',
                popover: {
                    title: t("Asset Pictures Page", "Bogga Sawirrada Hantida"),
                    description: t(
                        "This page shows all photos taken for this asset, and lets you upload new ones.",
                        "Boggan waxa uu muujinayaa dhammaan sawirrada laga qaaday hantidan, sidoo kale waxaad ku geli kartaa sawirro cusub."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="ap.assetinfo"]',
                popover: {
                    title: t("Asset Information", "Macluumaadka Hantida"),
                    description: t(
                        "This is the specific asset you're working with (name / make / model). All pictures below belong to this asset.",
                        "Tani waa hantida aad hadda ku shaqaynayso (magac / nooc / moodel). Dhammaan sawirrada hoos ku qoran waxay la xiriiraan hantidan."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="ap.uploadsection"]',
                popover: {
                    title: t("Upload New Picture", "Soo Geli Sawir Cusub"),
                    description: t(
                        "Choose an image file here and click Upload. The image will be stored and shown in the gallery below.",
                        "Dooro fayl sawir halkan ka dibna guji Upload. Sawirka waxa lagu kaydin doonaa oo waxa lagu tusi doonaa galeriga hoose."
                    ),
                    side: "left"
                }
            },
            {
                element: '[data-tour="ap.gallery"]',
                popover: {
                    title: t("Picture Gallery", "Galeriga Sawirrada"),
                    description: t(
                        "All saved pictures for this asset are displayed here. Use this to visually verify the item’s condition.",
                        "Dhammaan sawirrada kaydsan ee hantidan waxa lagu soo bandhigayaa halkan. Waxaad uga adeegsan kartaa inaad ka hubiso xaaladda shayga."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="ap.deletebtn"]',
                popover: {
                    title: t("Delete Picture", "Tirtir Sawir"),
                    description: t(
                        "Click Delete to remove a picture. You'll be asked to confirm before it's permanently removed.",
                        "Riix Delete si aad u tirtirto sawirka. Waxaa lagaa weydiin doonaa xaqiijin ka hor inta aan si joogto ah loo tirtirin."
                    ),
                    side: "left"
                }
            }
        ]
    };

    // optional direct-start handle (not used by FAB, but good for debugging)
    const driverObj = driverFactory({
        animate: true,
        showProgress: true,
        steps: window.AMS_TOUR_REGISTRY["Assets/AssetPictures"].steps,
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

    // for manual trigger in console if you ever need:
    window.startAssetPicturesTour = () => driverObj.drive();
})();
