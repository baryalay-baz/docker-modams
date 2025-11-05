// wwwroot/js/tours/Transfers/PreviewTransfer-tour.js
(function () {
    // Driver.js factory (IIFE build from your CDN)
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js failed to load for Transfers/PreviewTransfer tour.");
        return;
    }

    // --- Language helper (en | so)
    const lang = (window.AMS?.bridge?.cultureTwoLetter || "en").toLowerCase();
    const t = (en, so) => (lang === "so" ? so : en);

    // --- Utilities
    const qs = (sel) => document.querySelector(sel);
    const qsa = (sel) => Array.from(document.querySelectorAll(sel));
    const ensure = (sel) => !!qs(sel);
    const skipIfMissing = (step) => ({
        onHighlightStarted: (_el, _step, opts) => {
            if (!qs(step.element)) opts?.driver?.moveNext();
        }
    });

    const tryExpandAssets = () => {
        const collapse = qs("#pvAssets");
        // If collapse exists and not expanded, click the toggle button
        if (collapse && !collapse.classList.contains("show")) {
            const btn = document.querySelector('[data-bs-target="#pvAssets"]');
            btn?.click();
        }
    };

    // --- Register the tour
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Transfers/PreviewTransfer"] = {
        version: "v2-inline-actions",
        steps: [
            // ===== Header & nav =====
            {
                element: '[data-tour="pv.header"]',
                popover: {
                    title: t("Preview Transfer", "Eeg Wareejinta"),
                    description: t(
                        "This page shows transfer status, next actions, and related assets.",
                        "Boggan wuxuu muujinayaa xaaladda wareejinta, falalka xiga, iyo hantida la xiriirta."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.breadcrumbs"]',
                popover: {
                    title: t("Navigation", "Hagid"),
                    description: t(
                        "Use breadcrumbs to jump back to Transfers or the Dashboard.",
                        "Isticmaal breadcrumbs si aad ugu laabato Wareejinta ama Dashboard-ka."
                    ),
                    side: "left"
                }
            },

            // ===== Status & KPIs =====
            {
                element: '[data-tour="pv.card"]',
                popover: {
                    title: t("Status & Key Facts", "Xaaladda & Xog Muhiim ah"),
                    description: t(
                        "At-a-glance status, date, and the most important figures for this transfer.",
                        "Halkan waxaad ka arki kartaa xaaladda, taariikhda, iyo tirooyinka ugu muhiimsan."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="pv.status"]',
                popover: {
                    title: t("Transfer Status", "Xaaladda Wareejinta"),
                    description: t(
                        "Green = Acknowledged; Red = Rejected; Yellow/Gray = In progress.",
                        "Cagaar = La xaqiijiyay; Casaan = La diiday; Huruud/Cawlan = Socota."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.assets.count"]',
                popover: {
                    title: t("Asset Count", "Tirada Hantida"),
                    description: t(
                        "Total number of assets included in this transfer.",
                        "Tirada guud ee hantida ku jirta wareejintan."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.stores.from"]',
                popover: {
                    title: t("Store From", "Bakhaarka Laga Soo Wareejiyay"),
                    description: t(
                        "Where the assets are coming from.",
                        "Meesha ay hantidu ka imanayaan."
                    ),
                    side: "bottom"
                }
            },
            {
                element: '[data-tour="pv.stores.to"]',
                popover: {
                    title: t("Store To", "Bakhaarka Loo Wareejinayo"),
                    description: t(
                        "Where the assets will be received.",
                        "Meesha hantidu lagu heli doono."
                    ),
                    side: "bottom"
                }
            },

            // ===== Timeline =====
            {
                element: '[data-tour="pv.timeline"]',
                popover: {
                    title: t("Lifecycle", "Marxaladaha"),
                    description: t(
                        "Created → Submitted for Acknowledgement → Acknowledged/Rejected.",
                        "La abuuray → Loo gudbiyay xaqiijin → La xaqiijiyay/La diiday."
                    ),
                    side: "top"
                }
            },

            // ===== Assets =====
            {
                element: '[data-tour="pv.assets.block"]',
                popover: {
                    title: t("Assets Overview", "Dulmar Hantida"),
                    description: t(
                        "Quick chips for the first items; expand for full details.",
                        "Waxaad si degdeg ah u arki kartaa kuwa ugu horreeya; ballaari si aad u aragto faahfaahinta oo dhan."
                    ),
                    side: "top"
                }
            },
            {
                element: '[data-tour="pv.assets.chips"]',
                popover: {
                    title: t("Quick Chips", "Tilmaamo Degdeg ah"),
                    description: t(
                        "Click a chip to open asset details in a new tab.",
                        "Guji chip si aad u furto faahfaahinta hantida bog cusub."
                    ),
                    side: "top"
                },
                onNextClick: () => tryExpandAssets()
            },
            {
                element: '[data-tour="#pvAssets"]', // anchor step to the collapse container
                popover: {
                    title: t("Asset Details", "Faahfaahinta Hantida"),
                    description: t(
                        "Expanded table with category, model, identification, and condition.",
                        "Jadwal faahfaahsan oo leh qayb, nooc, aqoonsi, iyo xaalad."
                    ),
                    side: "top"
                },
                // Make this robust even if ID differs; fall back to table selector
                ...{
                    onHighlightStarted: (_el, _step, opts) => {
                        const anchor = qs("#pvAssets") || qs('[data-tour="pv.assets.table"]');
                        if (!anchor) { opts?.driver?.moveNext(); return; }
                        tryExpandAssets();
                    }
                }
            },
            {
                element: '[data-tour="pv.assets.table"]',
                popover: {
                    title: t("Open Asset", "Fur Hanti"),
                    description: t(
                        "Click a row to open the asset profile in a new tab.",
                        "Guji saf si aad u furto bogga hantida bog cusub."
                    ),
                    side: "top"
                },
                ...skipIfMissing({ element: '[data-tour="pv.assets.table"]' })
            },

            // ===== Signatures =====
            {
                element: '[data-tour="pv.sign.sender"]',
                popover: {
                    title: t("Sender Signature", "Saxeexa Diraha"),
                    description: t(
                        "Visible after submission for acknowledgement.",
                        "Waxay soo baxdaa ka dib gudbinta si loo xaqiijiyo."
                    ),
                    side: "top"
                },
                ...skipIfMissing({ element: '[data-tour="pv.sign.sender"]' })
            },
            {
                element: '[data-tour="pv.sign.receiver"]',
                popover: {
                    title: t("Receiver Signature", "Saxeexa Qaataha"),
                    description: t(
                        "Visible after acknowledgement (or marked if rejected).",
                        "Waxay soo baxdaa marka la xaqiijiyo (ama la calaamadiyo haddii la diiday)."
                    ),
                    side: "top"
                },
                ...skipIfMissing({ element: '[data-tour="pv.sign.receiver"]' })
            },

            // ===== Actions (inline panel) =====
            {
                element: '[data-tour="pv.actions.panel"]',
                popover: {
                    title: t("Actions", "Falalka"),
                    description: t(
                        "Perform the next step here based on your role and the transfer state.",
                        "Halkan ka samee tallaabada xigta iyadoo lagu saleynayo doorkaaga iyo xaaladda wareejinta."
                    ),
                    side: "left"
                }
            },
            {
                element: '[data-tour="pv.actions.submit"]',
                popover: {
                    title: t("Submit for Acknowledgement", "Gudbi si Loo Xaqiijiyo"),
                    description: t(
                        "Sender submits to the receiver for acknowledgement.",
                        "Diruhu wareejinta ayuu u gudbiyaa qaataha si loo xaqiijiyo."
                    ),
                    side: "left"
                },
                ...skipIfMissing({ element: '[data-tour="pv.actions.submit"]' })
            },
            {
                element: '[data-tour="pv.actions.ack"]',
                popover: {
                    title: t("Acknowledge Transfer", "Xaqiiji Wareejinta"),
                    description: t(
                        "Receiver confirms receipt of assets.",
                        "Qaatahu wuxuu xaqiijiyaa helitaanka hantida."
                    ),
                    side: "left"
                },
                ...skipIfMissing({ element: '[data-tour="pv.actions.ack"]' })
            },
            {
                element: '[data-tour="pv.actions.reject"]',
                popover: {
                    title: t("Reject Transfer", "Diid Wareejinta"),
                    description: t(
                        "Receiver can reject if details are incorrect.",
                        "Qaatahu wuu diidi karaa haddii faahfaahinta khaldan tahay."
                    ),
                    side: "left"
                },
                ...skipIfMissing({ element: '[data-tour="pv.actions.reject"]' })
            },
            {
                element: '[data-tour="pv.actions.print"]',
                popover: {
                    title: t("Print Voucher", "Daabac Warqadda"),
                    description: t(
                        "Opens the official voucher (Telerik report) in a new window.",
                        "Waxay furaysaa warqadda rasmiga ah (Telerik report) bog cusub."
                    ),
                    side: "left"
                },
                ...skipIfMissing({ element: '[data-tour="pv.actions.print"]' })
            },

            // ===== Exit =====
            {
                element: '[data-tour="pv.back"]',
                popover: {
                    title: t("Back to Transfers", "U Noqo Wareejinta"),
                    description: t(
                        "Return to the transfers list.",
                        "Ku laabo liiska wareejinta."
                    ),
                    side: "bottom"
                }
            }
        ],

        // Optional hooks (kept minimal)
        onDestroyed: () => {
            // Collapse assets after tour ends if you expanded them
            const collapse = document.getElementById("pvAssets");
            if (collapse && collapse.classList.contains("show")) {
                const btn = document.querySelector('[data-bs-target="#pvAssets"]');
                btn?.click();
            }
        }
    };
})();
