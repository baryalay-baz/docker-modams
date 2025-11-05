(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (window.AMS?.bridge?.cultureTwoLetter || "en").toLowerCase() === "so" ? "so" : "en";
    if (!driverFactory) {
        console.error("Driver.js failed to load for Transfers/PreviewTransfer tour.");
        return;
    }
    const t = (en, so) => (lang === "so" ? so : en);

    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Transfers/PreviewTransfer"] = {
        version: "v1",
        steps: [
            {
                element: '[data-tour="pv.header"]',
                popover: {
                    title: t("Preview Transfer", "Hordhaca Wareejinta"),
                    description: t("Read-only preview of the transfer voucher.", "Aragti akhris-kaliya oo warqadda wareejinta ah."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.breadcrumbs"]',
                popover: {
                    title: t("Breadcrumbs", "Tilmaamaha Bogagga"),
                    description: t("Navigate back to Transfers or Dashboard.", "U laabo Wareejinta ama Dashboard-ka."),
                    side: "left"
                }
            },

            {
                element: '[data-tour="pv.title"]',
                popover: {
                    title: t("Voucher Title", "Cinwaanka Warqadda"),
                    description: t("Title of this voucher preview.", "Cinwaanka hordhacan."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.meta.number"]',
                popover: {
                    title: t("Transfer Number", "Lambarka Wareejinta"),
                    description: t("Unique identifier for this transfer.", "Lambar gaar ah oo lagu garto."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.meta.date"]',
                popover: {
                    title: t("Transfer Date", "Taariikhda Wareejinta"),
                    description: t("Date the transfer was created/submitted.", "Taariikhda la sameeyay/lagu gudbiyay."),
                    side: "left"
                }
            },
            {
                element: '[data-tour="pv.meta.stores"]',
                popover: {
                    title: t("Stores", "Bakhaarada"),
                    description: t("Origin and destination stores.", "Bakhaarka laga keenay iyo kan loo diray."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="pv.assets.count"]',
                popover: {
                    title: t("Number of Assets", "Tirada Hantida"),
                    description: t("How many assets are included.", "Tirada hantida ku jirta."),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.status"]',
                popover: {
                    title: t("Status", "Xaaladda"),
                    description: t("Pending / Submitted / Acknowledged / Rejected.", "Sugaya / La gudbiyay / La xaqiijiyay / La diiday."),
                    side: "bottom"
                }
            },

            {
                element: '[data-tour="pv.assets.table"]',
                popover: {
                    title: t("Assets List", "Liiska Hantida"),
                    description: t("Detailed list of moved assets.", "Liis faahfaahsan oo hantida wareegaysa."),
                    side: "top"
                }
            },

            {
                element: '[data-tour="pv.sign.sender"]',
                popover: {
                    title: t("Transfer By", "Ku Wareejiyay"),
                    description: t("Sender signature/barcode & date.", "Saxiixa/baar-koodhka diraha iyo taariikhda."),
                    side: "top"
                }
            },
            {
                element: '[data-tour="pv.sign.receiver"]',
                popover: {
                    title: t("Received By", "Qaatay"),
                    description: t("Receiver signature/barcode & date.", "Saxiixa/baar-koodhka qaataha iyo taariikhda."),
                    side: "top"
                }
            },

            // Action button first...
            {
                element: '[data-tour="pv.actions.fab"]',
                popover: {
                    title: t("Actions Panel", "Gudiga Falalka"),
                    description: t("Open actions for submit/ack/reject/print.", "Fur falalka gudbi/xaqiiji/diid/daabac."),
                    side: "left"
                },
                onNextClick: () => document.querySelector('[data-tour="pv.actions.fab"]')?.click()
            },

            // ...then the offcanvas container
            {
                element: '[data-tour="pv.actions.panel"]',
                popover: {
                    title: t("Offcanvas Actions", "Gudiga Dhinaca"),
                    description: t("Contextual actions for this transfer.", "Falal ku xiran xaaladda warqadda."),
                    side: "left"
                }
            },

            // Conditional actions
            {
                element: '[data-tour="pv.actions.submit"]',
                popover: {
                    title: t("Submit for Acknowledgement", "Gudbi si Loo Xaqiijiyo"),
                    description: t("Sender submits to receiver.", "Diruhu wuu gudbiyaa wareejinta."),
                    side: "top"
                },
                onHighlightStarted: (_el, step, opts) => { if (!document.querySelector(step.element)) opts.driver?.moveNext(); }
            },
            {
                element: '[data-tour="pv.actions.ack"]',
                popover: {
                    title: t("Acknowledge Transfer", "Xaqiiji Wareejinta"),
                    description: t("Receiver acknowledges receipt.", "Qaatahu wuu xaqiijiyaa."),
                    side: "top"
                },
                onHighlightStarted: (_el, step, opts) => { if (!document.querySelector(step.element)) opts.driver?.moveNext(); }
            },
            {
                element: '[data-tour="pv.actions.reject"]',
                popover: {
                    title: t("Reject Transfer", "Diid Wareejinta"),
                    description: t("Receiver can reject with reason.", "Qaatahu wuu diidi karaa."),
                    side: "top"
                },
                onHighlightStarted: (_el, step, opts) => { if (!document.querySelector(step.element)) opts.driver?.moveNext(); }
            },

            {
                element: '[data-tour="pv.actions.print"]',
                popover: {
                    title: t("Print Voucher", "Daabac Warqadda"),
                    description: t("Open a printable version in a new window.", "Fur nooca daabacaadda ee daaqad cusub."),
                    side: "left"
                }
            },

            // Back nav
            {
                element: '[data-tour="pv.back"]',
                popover: {
                    title: t("Back to Transfers", "Ku Noqo Wareejinta"),
                    description: t("Return to the Transfers list.", "Ku laabo liiska Wareejinta."),
                    side: "bottom"
                }
            }
        ]
    };
})();
