// /wwwroot/js/tours/verifications-previewschedule-tour.js
(function () {
    const driverFactory = window?.driver?.js?.driver;
    const lang = (typeof window.getCurrentLanguage === "function") ? window.getCurrentLanguage() : "en";
    if (!driverFactory) { console.error("Driver.js not loaded for Verifications/PreviewSchedule."); return; }
    const t = (en, so) => (lang === "so" ? so : en);

    /* ---------- Z-INDEX PATCH (modal above overlay) ---------- */
    // replace previous zPatch with this:
    (function zPatch() {
        const id = "ams-driver-modal-zpatch";
        if (document.getElementById(id)) return;
        const css = document.createElement("style");
        css.id = id;
        css.textContent = `
    /* DEFAULT bootstrap: modal ~1055, backdrop ~1050 */

    /* When we want modal UNDER the driver (so overlay greys the page including modal),
       and only the highlighted element shows through */
    .driver-under .modal-backdrop { z-index: 1050 !important; }
    .driver-under .modal         { z-index: 1055 !important; }

    /* (Optional) If you ever need modal ABOVE overlay again, switch to .driver-over */
    .driver-over  .modal-backdrop { z-index: 11010 !important; }
    .driver-over  .modal          { z-index: 11020 !important; }
  `;
        document.head.appendChild(css);
    })();


    /* ---------- Helpers ---------- */
    function waitFor(fn, timeout = 12000, interval = 120) {
        const start = Date.now();
        return new Promise((res, rej) => {
            (function tick() {
                try {
                    if (fn()) return res(true);
                    if (Date.now() - start > timeout) return rej(new Error("waitFor timeout"));
                    setTimeout(tick, interval);
                } catch (e) { rej(e); }
            })();
        });
    }

    function bs5Show(id) {
        const el = document.getElementById(id);
        if (!el) return false;
        const M = window.bootstrap?.Modal;
        if (!M) return false;
        const inst = M.getOrCreateInstance(el, { backdrop: "static", keyboard: false });
        inst.show();
        return true;
    }
    function bs4Show(id) {
        if (!window.jQuery?.fn?.modal) return false;
        const $el = jQuery("#" + id);
        if ($el.length === 0) return false;
        $el.modal({ backdrop: "static", keyboard: false, show: true });
        return true;
    }
    function rawShow(id) {
        const el = document.getElementById(id);
        if (!el) return false;
        el.style.display = "block";
        el.classList.add("show");
        el.removeAttribute("aria-hidden");
        if (!document.querySelector(".modal-backdrop")) {
            const bd = document.createElement("div");
            bd.className = "modal-backdrop fade show";
            document.body.appendChild(bd);
        }
        return true;
    }
    function ensureModalOpen(id) {
        const el = document.getElementById(id);
        if (!el || el.classList.contains("show")) return;
        if (bs5Show(id)) return;
        if (bs4Show(id)) return;
        rawShow(id);
    }
    function ensureModalClosed(id) {
        const el = document.getElementById(id);
        if (!el) return;
        if (window.bootstrap?.Modal) {
            window.bootstrap.Modal.getOrCreateInstance(el).hide();
        } else if (window.jQuery?.fn?.modal) {
            jQuery("#" + id).modal("hide");
        } else {
            el.classList.remove("show");
            el.style.display = "none";
            el.setAttribute("aria-hidden", "true");
            document.querySelector(".modal-backdrop")?.remove();
        }
    }

    // Try to run your page’s real click handler to load data
    async function openVerifyFlow() {
        const btn = document.getElementById("btnVerify");
        if (btn) {
            btn.click(); // fire your real handler (AJAX + show modal)
        } else {
            // fallback: force the modal open
            ensureModalOpen("mdlVerifyAssets");
        }

        // Wait until modal visible
        await waitFor(() => document.getElementById("mdlVerifyAssets")?.classList.contains("show"));

        // If your code shows an overlay while loading, wait for rows AFTER overlay is hidden
        // Overlay id from your view: #verificationOverlay (inside modal body)
        // First, wait for any rows to appear OR overlay to disappear
        await waitFor(() => {
            const rows = document.querySelectorAll("#tblAssets tbody tr").length;
            const overlay = document.querySelector("#mdlVerifyAssets #verificationOverlay");
            const overlayHidden = overlay ? (overlay.style.display === "none" || overlay.hidden) : true;
            return rows > 0 && overlayHidden;
        });

        // tiny render cushion
        await new Promise(r => setTimeout(r, 100));
    }

    /* ---------- Steps ---------- */
    const steps = [
        // Page
        {
            element: '[data-tour="vps.pagetitle"]',
            popover: {
                title: t("Verification Schedule (Preview)", "Jadwalka Hubinta (Muuqaal)"),
                description: t("Overview of this schedule: verify or complete here.", "Dulmar jadwalkan: halkan ka hubi ama ka dhameystir."),
                side: "bottom"
            }
        },
        {
            element: '[data-tour="vps.scheduleCard"]',
            popover: {
                title: t("Schedule Summary", "Soo Koobidda Jadwalka"),
                description: t("Department, type, dates, notes, and team.", "Waax, nooc, taariikho, qoraallo, iyo koox."),
                side: "top"
            }
        },
        { element: '[data-tour="vps.department"]', popover: { title: t("Department", "Waaxda"), description: t("Where verification occurs.", "Halka hubintu ka dhaceyso."), side: "top" } },
        { element: '[data-tour="vps.type"]', popover: { title: t("Verification Type", "Nooca Hubinta"), description: t("Physical check, docs review, etc.", "Hubin jireed, dib-u-eegis, iwm."), side: "top" } },
        { element: '[data-tour="vps.start"]', popover: { title: t("Start Date", "Taariikhda Bilowga"), description: t("Planned start.", "Bilow qorshaysan."), side: "top" } },
        { element: '[data-tour="vps.end"]', popover: { title: t("End Date", "Taariikhda Dhamaadka"), description: t("Planned completion.", "Dhameystir qorshaysan."), side: "top" } },
        { element: '[data-tour="vps.notes"]', popover: { title: t("Notes", "Qoraallo"), description: t("Context/instructions.", "Macluumaad/tilmaamo."), side: "left" } },
        {
            element: '[data-tour="vps.team"]',
            popover: {
                title: t("Verification Team", "Kooxda Hubinta"),
                description: t("Hover avatars to see member details.", "Dul istaag sawirrada si aad u aragto faahfaahin."),
                side: "left"
            }
        },
        {
            element: '[data-tour="vps.counts"]',
            popover: {
                title: t("Progress Counters", "Tirakoobka Horumarka"),
                description: t("To verify vs verified — should match before completion.", "La hubinayo vs la hubiyay — waa inay isu dhigmaan ka hor dhameystir."),
                side: "top"
            }
        },

        // Verify button (instruction)
        {
            element: '[data-tour="vps.verifyBtn"]',
            popover: {
                title: t("Verify Assets", "Hubi Hantida"),
                description: t("Clicking this opens the verification form (we’ll open it next).", "Riixitaankani wuxuu furaa foomka hubinta (waan fureynaa hadda)."),
                side: "right"
            }
        },

        // Utility step: open modal via YOUR handler and wait for data, then auto-advance
        {
            element: 'body',
            popover: {
                title: t("Opening verification form…", "Furista foomka hubinta…"),
                description: t("Loading the list of assets to verify…", "Liiska hantida la hubinayo ayaa soo degaya…"),
                side: "bottom"
            },
            onHighlighted: async () => {
                document.body.classList.add("driver-active");
                try {
                    await openVerifyFlow();
                } catch (e) {
                    console.warn("[Tour] openVerifyFlow failed:", e);
                }
                driver.moveNext();
            }
        },

        // Modal steps
        {
            element: '#tblAssets',
            popover: {
                title: t("Select an Asset", "Dooro Hanti"),
                description: t("To verify an asset, first select one from this list.", "Si aad u hubiso hanti, marka hore dooro mid liiskan ka mid ah."),
                side: "left"
            }
        },
        {
            element: '[data-tour="vps.modal.asset"]',
            popover: {
                title: t("Selected Asset Information", "Macluumaadka Hantida La Doortay"),
                description: t("Once selected, the asset name appears here.", "Marka la doorto, halkan ayuu magaca ka muuqan doonaa."),
                side: "right"
            }
        },
        {
            element: '[data-tour="vps.modal.result"]',
            popover: {
                title: t("Result", "Natiijo"),
                description: t("Pick the appropriate verification outcome.", "Dooro natiijada saxda ah ee hubinta."),
                side: "right"
            }
        },
        {
            element: '[data-tour="vps.modal.comments"]',
            popover: {
                title: t("Comments", "Faallo"),
                description: t("Add notes or issues found during verification.", "Ku dar qoraallo ama cilado la helay."),
                side: "right"
            }
        },
        {
            element: '[data-tour="vps.modal.photo"]',
            popover: {
                title: t("Upload Photo", "Soo Geli Sawir"),
                description: t("Upload a current picture of the asset as evidence.", "Soo geli sawir cusub oo hanti ah sida caddeyn."),
                side: "right"
            }
        },
        {
            element: '[data-tour="vps.modal.verifySubmit"]',
            popover: {
                title: t("Save Verification", "Kaydi Hubinta"),
                description: t("Save to record this verification entry.", "Kaydi si aad u diiwaangeliso hubintan."),
                side: "right"
            }
        },

        // Utility step: close modal and continue with the rest of the page
        {
            element: 'body',
            popover: {
                title: t("Wrapping up the form…", "Dhameystiridda foomka…"),
                description: t("We’ll close the form and show the remaining items on this page.", "Waxaan xireynaa foomka si aan u sii wadno halkan."),
                side: "bottom"
            },
            onHighlighted: () => {
                ensureModalClosed("mdlVerifyAssets");
                setTimeout(() => driver.moveNext(), 100);
            }
        },

        // Back to page items
        {
            element: '[data-tour="vps.chart.status"]',
            popover: {
                title: t("Status Chart", "Jaantuska Xaaladda"),
                description: t("Breakdown of verification results.", "Qaybinta natiijooyinka hubinta."),
                side: "left"
            }
        },
        {
            element: '[data-tour="vps.chart.progress"]',
            popover: {
                title: t("Daily Progress", "Horumarka Maalinlaha"),
                description: t("Progress over the schedule dates.", "Horumarka maalmaha jadwalka."),
                side: "left"
            }
        },
        {
            element: '[data-tour="vps.completeBtn"]',
            popover: {
                title: t("Complete Verification", "Dhameystir Hubinta"),
                description: t("Enabled when all assets are verified and status is Ongoing.", "Wuu shaqeeyaa marka dhammaan la hubiyo oo xaaladdu Socota tahay."),
                side: "left"
            }
        },
        {
            element: '[data-tour="vps.back"]',
            popover: {
                title: t("Back to Verifications", "Ku Noqo Hubinta"),
                description: t("Return to the schedules list.", "Ku noqo liiska jadwalada."),
                side: "top"
            }
        }
    ];

    /* ---------- Registry ---------- */
    window.AMS_TOUR_REGISTRY = window.AMS_TOUR_REGISTRY || {};
    window.AMS_TOUR_REGISTRY["Verifications/PreviewSchedule"] = {
        version: "v7-click-btn-wait-data-autoclose",
        steps
    };

    /* ---------- Driver ---------- */
    const driver = driverFactory({
        animate: true,
        showProgress: true,
        steps,
        onHighlightStarted: (_el, step) => {
            // ensure the overlay sits ABOVE the modal so greying works
            document.body.classList.add("driver-under");
            document.body.classList.remove("driver-over"); // just in case

            const text = step?.popover?.description;
            if (text && window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.play(text, lang);
        },
        onDestroyed: () => {
            if (window.AMS_TOUR_AUDIO) window.AMS_TOUR_AUDIO.stop?.();
            ensureModalClosed("mdlVerifyAssets");
            document.body.classList.remove("driver-under", "driver-over");
        }
    });

    window.startVerificationsPreviewScheduleTour = () => driver.drive();
})();
