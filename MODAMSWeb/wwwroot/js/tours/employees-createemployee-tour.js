// ~/js/tours/employees-createemployee-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Employees/CreateEmployee tour.");
        return;
    }

    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Employees/CreateEmployee"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="emp.create.header"]',
                popover: {
                    title: t("Create Employee", "Abuur Shaqaale"),
                    description: t("Add a new employee to the system.", "Ku dar shaqaale cusub nidaamka."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.create.section"]',
                popover: {
                    title: t("Details", "Faahfaahin"),
                    description: t("Provide identity and assignment details.", "Geli faahfaahinta aqoonsiga iyo xilka."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.create.form"]',
                popover: {
                    title: t("Form", "Foomka"),
                    description: t("All mandatory fields must be filled.", "Dhamaan meelaha qasab ah waa in la buuxiyaa."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.fullname"]',
                popover: {
                    title: t("Full Name", "Magaca Buuxa"),
                    description: t("Enter the employee's legal name.", "Geli magaca rasmiga ah ee shaqaalaha."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.jobtitle"]',
                popover: {
                    title: t("Job Title", "Jagada Shaqada"),
                    description: t("State the employee's role title.", "Geli ciwaanka shaqada ee shaqaalaha."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.email"]',
                popover: {
                    title: t("Email", "Iimeyl"),
                    description: t("Used for account and notifications.", "Waxaa loo adeegsadaa akoonka iyo ogeysiisyada."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.phone"]',
                popover: {
                    title: t("Phone", "Taleefoon"),
                    description: t("Primary contact number.", "Lambarka xiriirka koowaad."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.card"]',
                popover: {
                    title: t("Card Number", "Lambarka Kaarka"),
                    description: t("Physical card / badge number if applicable.", "Lambarka kaarka / aqoonsiga haddii uu jiro."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.supervisor"]',
                popover: {
                    title: t("Supervisor", "Kormeeraha"),
                    description: t("Select the employee's direct supervisor.", "Dooro kormeeraha tooska ah ee shaqaalaha."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.role"]',
                popover: {
                    title: t("Initial Role", "Doorka Bilowga"),
                    description: t("Assign the initial system role.", "U qoondee doorka hore ee nidaamka."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.create.avatar"]',
                popover: {
                    title: t("Avatar Preview", "Muuqaalka Sawirka"),
                    description: t("A simple initials preview based on email.", "Muuqaal fudud oo xarfo ah oo ku saleysan iimeylka."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="emp.create.submit"]',
                popover: {
                    title: t("Save", "Kaydi"),
                    description: t("Submit to create the employee.", "Gudbi si aad u abuurto shaqaalaha."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="emp.create.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t("Return to the list without saving.", "Ku noqo liiska adiga oo aan kaydin."),
                    side: "top"
                }
            }
        ]
    };
})();
