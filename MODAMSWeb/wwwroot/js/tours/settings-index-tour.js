// wwwroot/js/tours/settings-index-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (window.getCurrentLanguage?.() || window.AMS?.bridge?.cultureTwoLetter || 'en').toLowerCase();
    if (!driverFactory) {
        console.error('Driver.js not found for Settings/Index tour.');
        return;
    }
    const t = (en, so) => (lang === 'so' ? so : en);

    // Clean, stable, headers-only tour. No CSS patches, no anchors, no auto-open.
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY['Settings/Index'] = {
        version: 'v10-clean-headers-only',
        steps: [
            {
                element: '#SelectedMonth',
                popover: {
                    title: t('Month Filter', 'Shaandhaynta Bisha'),
                    description: t('Choose the month to scope logs and history.', 'Dooro bisha si aad u koobto diiwaannada iyo taariikhda.'),
                    side: 'bottom'
                }
            },
            {
                element: '#SelectedYear',
                popover: {
                    title: t('Year Filter', 'Shaandhaynta Sannadka'),
                    description: t('Limit results to a specific year.', 'Ku koob natiijooyinka sannad goonni ah.'),
                    side: 'bottom'
                }
            },
            {
                element: '#heading2 .js-acc-toggle',
                popover: {
                    title: t('Login History (Collapsed)', 'Taariikhda Gelitaanka (Xiran)'),
                    description: t('This panel lists user sign-ins for the selected period. It stays collapsed during the tour.', 'Qaybtan waxay muujisaa gelitaanka isticmaalayaasha muddada la doortay. Inta socdaalka lagu jiro wuu xiran yahay.'),
                    side: 'bottom'
                }
            },
            {
                element: '#heading3 .js-acc-toggle',
                popover: {
                    title: t('Audit Log (Collapsed)', 'Diiwaanka Kormeerka (Xiran)'),
                    description: t('Shows who changed what and when. Kept closed for a smooth tour.', 'Waxay muujisaa yaa wax beddelay, waxa la beddelay iyo goorta. Si joogto ah ayaa loo xidhaa inta socdaalka lagu jiro.'),
                    side: 'bottom'
                }
            },
            {
                element: '#heading4 .js-acc-toggle',
                popover: {
                    title: t('Deleted Assets (Collapsed)', 'Hantida La Tirtiray (Xiran)'),
                    description: t('Contains items removed from active inventory, with recovery options.', 'Waxay ka kooban tahay walxo laga saaray kaydka firfircoon, oo leh ikhtiyaarro soo celin.'),
                    side: 'bottom'
                }
            }
        ]
    };
})();