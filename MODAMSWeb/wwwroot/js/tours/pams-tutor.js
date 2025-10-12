window.addEventListener("load", () => {
    console.log("PAMS Tutor initialized ✅");

    // 🌍 Bilingual tip dictionary
    const tips = {
        "ca.category": {
            en: "Select the main category that best represents this asset.",
            so: "Dooro qaybta ugu weyn ee si fiican u metelaysa hantidan."
        },
        "ca.subcategory": {
            en: "Choose a subcategory to classify the asset more precisely.",
            so: "Dooro qayb-hoosaad si aad hantidan ugu kala saarto si faahfaahsan."
        },
        "ca.name": {
            en: "Enter the official name or title of the asset.",
            so: "Geli magaca ama cinwaanka rasmiga ah ee hantidan."
        },
        "ca.make": {
            en: "Specify the manufacturer or brand of this asset.",
            so: "Sheeg shirkadda ama calaamadda soo saartay hantidan."
        },
        "ca.model": {
            en: "Enter the specific model or type number of the asset.",
            so: "Geli nambarka ama nooca gaarka ah ee hantidan."
        },
        "ca.year": {
            en: "Provide the year the asset was manufactured or produced.",
            so: "Sheeg sannadka hantidan la soo saaray ama la sameeyay."
        },
        "ca.vehicleinfo": {
            en: "If the asset is a vehicle, fill in the Engine, Chassis, and Plate details.",
            so: "Haddii hantidu tahay gaadhi, geli faahfaahinta matoorka, shassiska iyo taarikada."
        },
        "ca.country": {
            en: "Enter the country where this asset was manufactured.",
            so: "Geli dalka lagu soo saaray hantidan."
        },
        "ca.serial": {
            en: "Provide the unique serial number of the asset, if available.",
            so: "Geli lambarka gaarka ah ee hantidan haddii uu jiro."
        },
        "ca.barcode": {
            en: "Scan or enter the barcode assigned to this asset.",
            so: "Geli ama iskaani koodhka baarka ee hantidan loo qoondeeyay."
        },
        "ca.specifications": {
            en: "Write any key specifications or important technical details here.",
            so: "Halkan ku qor faahfaahinta farsamo ee muhiimka ah ee hantidan."
        },
        "ca.cost": {
            en: "Enter the total cost or purchase price of the asset.",
            so: "Geli qiimaha guud ama lacagta lagu iibsaday hantidan."
        },
        "ca.purchasedate": {
            en: "Select the date when the asset was purchased.",
            so: "Dooro taariikhda la iibsaday hantidan."
        },
        "ca.receiptdate": {
            en: "Enter the date when the asset was received by your office.",
            so: "Geli taariikhda xafiisku hantidan helay."
        },
        "ca.po": {
            en: "Enter the Purchase Order (PO) number linked to this asset.",
            so: "Geli lambarka amarka iibsiga (PO) ee la xiriira hantidan."
        },
        "ca.procuredby": {
            en: "Select the entity that procured this asset, such as UNOPS or EU.",
            so: "Dooro hay’adda ama cidda soo iibsatay hantidan, sida UNOPS ama EU."
        },
        "ca.donor": {
            en: "Select the donor who funded or contributed this asset.",
            so: "Dooro deeq bixiyaha maalgeliyay ama bixiyay hantidan."
        },
        "ca.condition": {
            en: "Select the current condition of the asset (e.g., New, Good, Damaged).",
            so: "Dooro xaaladda hadda ee hantidan (tusaale Cusub, Wanaagsan, Jaban)."
        },
        "ca.remarks": {
            en: "Add any relevant remarks or comments about the asset.",
            so: "Ku dar faallooyin ama qoraallo la xiriira hantidan."
        },
        "ca.submit": {
            en: "When finished, click here to save the asset record.",
            so: "Markaad dhammayso, riix halkan si aad u kaydiso xogta hantidan."
        }
    };

    // 🌐 Get language
    function getLang() {
        try {
            if (typeof getCurrentLanguage === "function") {
                return getCurrentLanguage() || "en";
            }
            return "en";
        } catch {
            return "en";
        }
    }

    // 🧩 Tooltip container
    const tipBox = document.createElement("div");
    tipBox.id = "pams-tutor-tip";
    Object.assign(tipBox.style, {
        position: "absolute",
        background: "#ffffff",
        border: "1px solid #ccc",
        borderRadius: "8px",
        padding: "8px 12px",
        fontSize: "13px",
        color: "#333",
        boxShadow: "0 3px 8px rgba(0,0,0,0.15)",
        maxWidth: "260px",
        zIndex: "99999",
        display: "none",
        transition: "opacity 0.25s ease"
    });
    document.body.appendChild(tipBox);

    let currentEl = null;
    let hideTimeout;

    // ✨ Show tooltip BELOW control
    function showTip(el, text) {
        const rect = el.getBoundingClientRect();
        const scrollY = window.scrollY || document.documentElement.scrollTop;
        const scrollX = window.scrollX || document.documentElement.scrollLeft;

        tipBox.innerText = text;
        tipBox.style.display = "block";
        tipBox.style.opacity = 1;

        const gapBelow = 6;
        const top = scrollY + rect.bottom + gapBelow; // ✅ below the field
        let left = scrollX + rect.left;

        if (left + tipBox.offsetWidth > window.innerWidth - 20) {
            left = window.innerWidth - tipBox.offsetWidth - 20;
        }

        tipBox.style.top = `${top}px`;
        tipBox.style.left = `${left}px`;
    }

    function hideTip() {
        tipBox.style.opacity = 0;
        setTimeout(() => (tipBox.style.display = "none"), 200);
    }

    // ✨ Auto reposition on scroll/resize
    window.addEventListener("scroll", () => {
        if (currentEl) {
            const key = currentEl.dataset.tour;
            const lang = getLang();
            const msg = (tips[key] && tips[key][lang]) || (tips[key] && tips[key].en);
            if (msg) showTip(currentEl, msg);
        }
    });
    window.addEventListener("resize", () => {
        if (currentEl) {
            const key = currentEl.dataset.tour;
            const lang = getLang();
            const msg = (tips[key] && tips[key][lang]) || (tips[key] && tips[key].en);
            if (msg) showTip(currentEl, msg);
        }
    });

    // 🧭 Focus/blur on actual controls
    document.addEventListener("focusin", e => {
        const el = e.target.closest("[data-tour]");
        if (!el) return;
        clearTimeout(hideTimeout);
        currentEl = el;
        const key = el.dataset.tour;
        const lang = getLang();
        const msg = (tips[key] && tips[key][lang]) || (tips[key] && tips[key].en);
        if (msg) showTip(el, msg);
    });

    document.addEventListener("focusout", e => {
        const el = e.target.closest("[data-tour]");
        if (!el) return;
        hideTimeout = setTimeout(() => hideTip(), 200);
    });
});
