// /wwwroot/js/tours/home-settings-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (window.getCurrentLanguage && window.getCurrentLanguage()) || "en";
    if (!driverFactory) { console.error("Driver.js failed to load for Home/Settings tour."); return; }
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Home/Settings"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="hs.title"]',
                popover: {
                    title: t("Account Settings", "Dejinta Akoonka"),
                    description: t("Manage password reset and Two-Factor Authentication (2FA).",
                        "Maamul dib-u-dejinta erayga sirta iyo laba-tallaabo xaqiijin (2FA)."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="hs.reset.heading"]',
                popover: {
                    title: t("Reset Password", "Dib-u-deji Erayga Sirta"),
                    description: t("Use the menu to confirm and trigger a password reset.",
                        "Isticmaal menu-ga si aad u xaqiijiso una bilaawdo dib-u-dejinta."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="hs.reset.dropdown"]',
                popover: {
                    title: t("Open Reset Menu", "Fur Menu-ga Dib-u-dejinta"),
                    description: t("Tap to reveal the confirmation and reset action.",
                        "Guji si aad u aragto xaqiijinta iyo ficilka dib-u-dejinta."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="hs.reset.confirm"]',
                popover: {
                    title: t("Confirm Reset", "Xaqiiji Dib-u-dejinta"),
                    description: t("Proceed only if you’re sure—you’ll set a new password afterwards.",
                        "Sii wad keliya haddii aad hubto—waxaad dejin doontaa eray sir cusub marka xigta."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="hs.2fa.heading"]',
                popover: {
                    title: t("Two-Factor Authentication (2FA)", "Laba-Tallaabo Xaqiijin (2FA)"),
                    description: t("Adds an extra verification step to protect your account.",
                        "Waxay ku daraysaa tallaabo xaqiijin dheeraad ah si ay u ilaaliso akoonkaaga."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="hs.2fa.link"]',
                popover: {
                    title: t("Set Up 2FA", "Deji 2FA"),
                    description: t("Open the security page to enable or manage your 2FA.",
                        "Fur bogga amniga si aad u hawlgeliso ama u maamusho 2FA-gaaga."),
                    side: "right"
                }
            }
        ]
    };
})();
