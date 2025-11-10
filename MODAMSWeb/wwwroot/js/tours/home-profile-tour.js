// /wwwroot/js/tours/home-profile-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (window.getCurrentLanguage && window.getCurrentLanguage()) || "en";
    if (!driverFactory) { console.error("Driver.js failed to load for Home/Profile tour."); return; }
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Home/Profile"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="hp.title"]',
                popover: {
                    title: t("Profile", "Xogta Shaqsiyeed"),
                    description: t("This page shows your profile and lets you update your info.", "Boggan waxa uu muujinayaa xogtaada shaqsiyeed oo waxaad wax ka beddeli kartaa."),
                    side: "bottom"
                }
            },

            {
                element: '[data-tour="hp.avatar"]',
                popover: {
                    title: t("Profile Picture", "Sawirka Profile-ka"),
                    description: t("Your current profile image.", "Sawirkaaga hadda."),
                    side: "right"
                }
            },

            {
                element: '[data-tour="hp.btn.editpic"]',
                popover: {
                    title: t("Change Picture", "Bedel Sawirka"),
                    description: t("Open the menu to upload a new 128×128 image.", "Fur menu-ga si aad u geliso sawir cusub 128×128."),
                    side: "right"
                }
            },

            {
                element: '[data-tour="hp.editpic.menu"]',
                popover: {
                    title: t("Upload Picture", "Geli Sawir"),
                    description: t("Choose a file and click Upload.", "Dooro faylka oo guji Upload."),
                    side: "right"
                }
            },

            {
                element: '[data-tour="hp.btn.resetpwd"]',
                popover: {
                    title: t("Reset Password", "Dib-u-dejinta Furaha"),
                    description: t("Open the menu to confirm password reset.", "Fur menu-ga si aad u xaqiijiso dib-u-dejinta."),
                    side: "right"
                }
            },

            {
                element: '[data-tour="hp.tabs"]',
                popover: {
                    title: t("Tabs", "Tab-yada"),
                    description: t("Switch between viewing your info and editing it.", "U kala beddel muuqa xoggta iyo tafatirka."),
                    side: "bottom"
                }
            },

            {
                element: '[data-tour="hp.personalinfo"]',
                popover: {
                    title: t("Personal Info", "Xogta Shaqsiyeed"),
                    description: t("Your stored name, title, email, card number, and organization details.", "Magaca, jagada, iimaylka, lambarka kaarka, iyo faahfaahinta hay’adda."),
                    side: "top"
                }
            },

            {
                element: '[data-tour="hp.contact"]',
                popover: {
                    title: t("Contact", "Xiriir"),
                    description: t("Your phone and email for quick reference.", "Telefoonka iyo iimaylkaaga."),
                    side: "top"
                }
            },

            {
                element: '[data-tour="hp.edit.form"]',
                popover: {
                    title: t("Edit Profile", "Tafatir Profile-ka"),
                    description: t("Update your personal details and save changes.", "Cusboonaysii faahfaahintaada oo keydi."),
                    side: "top"
                }
            },

            {
                element: '[data-tour="hp.btn.save"]',
                popover: {
                    title: t("Save Changes", "Keydi Isbedelada"),
                    description: t("Click to save the updated profile information.", "Guji si aad u kaydiso xoggta la cusboonaysiiyay."),
                    side: "right"
                }
            }
        ]
    };
})();
