// ~/js/tours/employees-editemployee-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded for Employees/EditEmployee tour.");
        return;
    }
    const lang = (window.AMS?.util?.getCurrentLanguage?.() || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Employees/EditEmployee"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="emp.edit.header"]',
                popover: {
                    title: t("Edit Employee", "Tafatir Shaqaale"),
                    description: t("Modify existing employee details.", "Wax ka beddel faahfaahinta shaqaalaha."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.edit.section"]',
                popover: {
                    title: t("Details", "Faahfaahin"),
                    description: t("Update personal and assignment info.", "Cusboonaysii xogta shakhsiga iyo xilka."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="emp.edit.form"]',
                popover: {
                    title: t("Form", "Foomka"),
                    description: t("Ensure mandatory fields are valid.", "Hubi meelaha khasabka ah inay sax yihiin."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.fullname"]',
                popover: {
                    title: t("Full Name", "Magaca Buuxa"),
                    description: t("Correct the legal name if needed.", "Haddii loo baahdo, sax magaca rasmiga ah."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.jobtitle"]',
                popover: {
                    title: t("Job Title", "Jagada Shaqada"),
                    description: t("Keep the role title up to date.", "Cusboonaysii ciwaanka shaqada."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.email"]',
                popover: {
                    title: t("Email", "Iimeyl"),
                    description: t("Email is read-only for integrity.", "Iimeylku waa akhris-keli ah si loo ilaaliyo."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.phone"]',
                popover: {
                    title: t("Phone", "Taleefoon"),
                    description: t("Primary contact number.", "Lambarka xiriirka koowaad."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.card"]',
                popover: {
                    title: t("Card Number", "Lambarka Kaarka"),
                    description: t("Badge / ID card number.", "Lambarka aqoonsiga/kaarka."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.supervisor"]',
                popover: {
                    title: t("Supervisor", "Kormeeraha"),
                    description: t("Change supervisor when line changes.", "Bedel kormeeraha marka xariiqda shaqo is bedesho."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.role"]',
                popover: {
                    title: t("Role", "Door"),
                    description: t("Adjust user role as required.", "Hagaaji doorka isticmaalaha sida loogu baahan yahay."),
                    side: "right"
                }
            },
            {
                element: '[data-tour="emp.edit.avatar"]',
                popover: {
                    title: t("Photo", "Sawir"),
                    description: t("Existing profile photo preview.", "Muuqaalka sawirka hore ee profile-ka."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="emp.edit.submit"]',
                popover: {
                    title: t("Save", "Kaydi"),
                    description: t("Submit to persist changes.", "Gudbi si isbeddelada loo keydiyo."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="emp.edit.cancel"]',
                popover: {
                    title: t("Cancel", "Jooji"),
                    description: t("Go back without saving.", "Ku noqo adigoon kaydin."),
                    side: "top"
                }
            }
        ]
    };
})();
