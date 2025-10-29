// /wwwroot/js/tours/assets-assetdocuments-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (typeof window.getCurrentLanguage === "function"
        ? window.getCurrentLanguage()
        : "en"
    ); // "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Assets/AssetDocuments tour.");
        return;
    }

    // bilingual text helper
    const t = (enText, soText) => (lang === "so" ? soText : enText);

    // make sure registry exists
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};

    window.AMS_TOUR_REGISTRY["Assets/AssetDocuments"] = {
        version: "v1",
        steps: [
            {
                // Page title: tells user what screen this is
                element: '[data-tour="ad.pagetitle"]',
                popover: {
                    title: t("Asset Documents Page", "Bogga Dukumentiyada Hantida"),
                    description: t(
                        "This page shows and manages all documents linked to the selected asset.",
                        "Boggan waxa uu muujinayaa dhammaan dukumentiyada ku lifaaqan hantidan."
                    ),
                    side: "bottom"
                }
            },
            {
                // Asset header (which asset we're editing)
                element: '[data-tour="ad.assetinfo"]',
                popover: {
                    title: t("Asset Information", "Macluumaadka Hantida"),
                    description: t(
                        "Here you see which asset you're working on (name / make / model).",
                        "Halkaan waxaad ka arki kartaa hantida aad hadda la shaqaynayso (magac / nooc / moodel)."
                    ),
                    side: "bottom"
                }
            },
            {
                // Documents table
                element: '[data-tour="ad.table"]',
                popover: {
                    title: t("Documents List", "Liiska Dukumentiyada"),
                    description: t(
                        "Every row is a required document type for this asset. You can download existing files or upload a new one.",
                        "Saf kasta waxa uu u taagan yahay dukumenti loo baahan yahay. Waxaad ka soo dejisan kartaa fayl jira ama waad geli karin mid cusub."
                    ),
                    side: "top"
                }
            },
            {
                // Upload button
                element: '[data-tour="ad.uploadbtn"]',
                popover: {
                    title: t("Upload a File", "Soo Geli Fayl"),
                    description: t(
                        "Click here to attach the correct document for this row. After you click, the upload form will open on the right.",
                        "Riix halkan si aad u geliso faylka saxda ah ee safkan. Marka aad riixdo, foomka soo gelinta waxa uu ka muuqan doonaa dhinaca midig."
                    ),
                    side: "left"
                }
            },
            {
                // Upload card / right side form
                element: '[data-tour="ad.uploadcard"]',
                popover: {
                    title: t("Upload Panel", "Foomka Soo Gelinta"),
                    description: t(
                        "This panel lets you choose a file and upload it. The system automatically links the file to the selected document type.",
                        "Halkaan waxaad ka dooranaysaa faylka aad rabto inaad geliso. Nidaamku si toos ah ayuu ugu xidhayaa nooca dukumentiga la doortay."
                    ),
                    side: "left"
                }
            },
            {
                // Back button at the bottom
                element: '[data-tour="ad.back"]',
                popover: {
                    title: t("Go Back", "Ku Noqo"),
                    description: t(
                        "Use this button to go back to the list of assets in this store.",
                        "Isticmaal badhankan si aad ugu laabato liiska hantida dukaankan / bakhaarkan."
                    ),
                    side: "top"
                }
            }
        ]
    };

    // Driver instance with audio hooks
    const driverObj = driverFactory({
        animate: true,
        showProgress: true,
        steps: window.AMS_TOUR_REGISTRY["Assets/AssetDocuments"].steps,
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

    // Optional manual start if you ever want to trigger it yourself
    window.startAssetDocumentsTour = () => driverObj.drive();
})();
