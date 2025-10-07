// wwwroot/js/tours/home-index-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = getCurrentLanguage(); // returns "en" or "so"

    if (!driverFactory) {
        console.error("Driver.js failed to load for Home/Index tour.");
        return;
    }

    // 🌍 Translation helper
    const t = (enText, soText) => (lang === 'so' ? soText : enText);

    // 🧠 Make sure the registry exists globally
    window.PAMS_TOUR_REGISTRY = window.PAMS_TOUR_REGISTRY || {};

    // 🏡 Register the Home/Index tour
    window.PAMS_TOUR_REGISTRY["Home/Index"] = {
        version: "v1",
        steps: [
            // === 🧭 HEADER SECTION ===
            {
                element: '[data-tour="header.logo"]',
                popover: {
                    title: t('Logo', 'Astaanta Nidaamka'),
                    description: t('Click here anytime to return to the Dashboard.', 'Riix halkan si aad mar kasta ugu laabato Dashboard-ka.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="header.sidebar-toggle"]',
                popover: {
                    title: t('Sidebar Toggle', 'Fur/Furitaanka Menu-ga Dhinaca'),
                    description: t('Click this to show or hide the sidebar menu.', 'Riix si aad u muujiso ama u qariso menu-ga dhinaca.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="header.search"]',
                popover: {
                    title: t('Global Search', 'Raadin Guud'),
                    description: t('Scan or type a barcode here to quickly find assets.', 'Halkan ka baar ama geli barcode si degdeg ah loo helo hanti.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="header.search-btn"]',
                popover: {
                    title: t('Search Button', 'Badhanka Raadinta'),
                    description: t('Click to start your search.', 'Riix si aad u bilowdo raadinta.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="header.language"]',
                popover: {
                    title: t('Language Selector', 'Doorashada Luqadda'),
                    description: t('Switch between English and Somali.', 'Dooro luqadda English ama Somali.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="header.shortcuts"]',
                popover: {
                    title: t('Shortcuts', 'Jidka Degdegga ah'),
                    description: t('Access frequently used pages quickly.', 'Si degdeg ah u gal bogagga inta badan la isticmaalo.'),
                    side: 'bottom'
                },
                onNextClick: () => {
                    const el = document.querySelector('[data-tour="header.shortcuts"]');
                    el?.click();
                }
            },
            {
                element: '[data-tour="header.alerts"]',
                popover: {
                    title: t('Alerts', 'Digniino'),
                    description: t('Check system alerts or issues here.', 'Halkan ka eeg digniinaha nidaamka ama arrimaha jira.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="header.fullscreen"]',
                popover: {
                    title: t('Fullscreen Mode', 'Shaashad Buuxda'),
                    description: t('Click to toggle fullscreen view.', 'Riix si aad u bedesho muuqaalka shaashadda buuxda.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="header.notifications"]',
                popover: {
                    title: t('Notifications', 'Ogeysiisyada'),
                    description: t('View new notifications here.', 'Halkan ka eeg ogeysiisyada cusub.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="header.profile"]',
                popover: {
                    title: t('User Menu', 'Menu-ga Isticmaalaha'),
                    description: t('Access your profile, settings, and logout.', 'Halkan ka gal profile-kaaga, dejinta iyo bixista.'),
                    side: 'bottom'
                }
            },

            // === 📚 SIDEBAR SECTION ===
            {
                element: '[data-tour="sidebar.logo"]',
                popover: {
                    title: t('Sidebar Logo', 'Astaanta Dhinaca'),
                    description: t('Click this logo to return to the Dashboard.', 'Riix astaantan si aad ugu laabato Dashboard-ka.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.rolename"]',
                popover: {
                    title: t('Role Name', 'Doorka Isticmaalaha'),
                    description: t('Role Name of the logged in user.', 'Doorka isticmaalahaan uu ku jiro nidaamka.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.departmentname"]',
                popover: {
                    title: t('Department Name', 'Magaca Waaxda'),
                    description: t('Logged-in user\'s linked department', 'Waaxda uu isticmaalahaan la xiriiro.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.dashboard"]',
                popover: {
                    title: t('Dashboard', 'Dashboard-ka'),
                    description: t('This brings you back to the Dashboard view.', 'Tani waxay kuu celinaysaa bogga Dashboard-ka.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.stores"]',
                popover: {
                    title: t('Stores', 'Kaydka'),
                    description: t('Go to the Stores module to manage asset storage locations.', 'Tag qaybta Kaydka si aad u maamusho meelaha hantida la dhigo.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.transfers"]',
                popover: {
                    title: t('Transfers', 'Wareejinta'),
                    description: t('Access asset transfer operations between locations.', 'Maamul wareejinta hantida meelaha kala duwan.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.disposals"]',
                popover: {
                    title: t('Disposals', 'Qashin Bixinta'),
                    description: t('Manage asset disposal processes here.', 'Halkan ka maamul habka qashin bixinta hantida.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.handovers"]',
                popover: {
                    title: t('Handovers', 'Wareejinta Mas’uuliyadda'),
                    description: t('Track and manage asset handovers. (Under construction)', 'La soco oo maamul wareejinta mas’uuliyadda. (Dhisme)'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.verifications"]',
                popover: {
                    title: t('Verifications', 'Hubinta'),
                    description: t('Open the Verifications module to verify asset records.', 'Fur qaybta Hubinta si aad u xaqiijiso diiwaanka hantida.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.reports"]',
                popover: {
                    title: t('Reports', 'Warbixinnada'),
                    description: t('View or generate asset management reports.', 'Daawo ama samee warbixino ku saabsan maamulka hantida.'),
                    side: 'right'
                }
            },
            {
                element: '[data-tour="sidebar.settings"]',
                popover: {
                    title: t('Settings', 'Dejinta'),
                    description: t('Administrative settings for system configuration.', 'Dejinta maamulka ee nidaamka.'),
                    side: 'right'
                }
            },

            // === 📊 DASHBOARD SECTION ===
            {
                element: '[data-tour="dashboard.header"]',
                popover: {
                    title: t('Dashboard Overview', 'Dulmar Dashboard'),
                    description: t('This is your main dashboard. It shows quick stats and navigation tiles.', 'Tani waa Dashboard-ka ugu weyn oo muujinaya tirokoobyo degdeg ah iyo marinno muhiim ah.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="dashboard.tiles"]',
                popover: {
                    title: t('Quick Navigation Tiles', 'Meelaha Degdegga ah'),
                    description: t('Use these tiles to jump to Stores, Transfers, Disposal, and Reports.', 'Ka faa’iidayso meelahan si aad u gasho qaybo sida Kaydka, Wareejinta, Qashinka iyo Warbixinta.'),
                    side: 'bottom'
                }
            },
            {
                element: '[data-tour="dashboard.table"]',
                popover: {
                    title: t('Category Table', 'Jadwalka Qaybaha'),
                    description: t('Shows assets by category with counts and costs. Click a category to drill down.', 'Waxay muujinaysaa hantida qaybaheeda, tirada iyo qiimaha. Riix qayb si aad u faahfaahiso.'),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="dashboard.chart"]',
                popover: {
                    title: t('Distribution Chart', 'Jaantuska Qaybaha'),
                    description: t('Pie chart showing asset distribution across categories.', 'Jaantus muujinaya sida hantida ugu qaybsan tahay qaybaha kala duwan.'),
                    side: 'top'
                }
            },
            {
                element: '[data-tour="dashboard.summary"]',
                popover: {
                    title: t('Summary Figures', 'Tirokoobyada Guud'),
                    description: t('A quick snapshot of total assets, costs, current value, and store count.', 'Dulmar kooban oo ku saabsan hantida guud, kharashaadka, qiimaha hadda iyo tirada kaydka.'),
                    side: 'top'
                }
            }
        ]
    };
})();
