window.addEventListener("load", () => {
    // Detect language
    function getLang() {
        try {
            if (typeof getCurrentLanguage === "function") {
                return getCurrentLanguage() || "en";
            }
        } catch { }
        return "en";
    }

    // Create tooltip container
    const tipBox = document.createElement("div");
    tipBox.id = "pams-tutor-tip";
    tipBox.className = "pams-tutor-tip";
    document.body.appendChild(tipBox);

    let currentEl = null;

    // Show tooltip below focused input
    function showTip(el, text) {
        const rect = el.getBoundingClientRect();
        tipBox.innerHTML = `<i data-feather="info" class="pams-tutor-icon"></i><span>${text}</span>`;
        tipBox.style.display = "block";
        tipBox.classList.add("visible");

        const top = window.scrollY + rect.bottom + 8;
        const left = window.scrollX + rect.left;
        tipBox.style.top = `${top}px`;
        tipBox.style.left = `${left}px`;

        currentEl = el;

        // Replace icon dynamically (important if tooltip reused)
        if (window.feather) feather.replace();
    }

    // Hide tooltip
    function hideTip() {
        tipBox.classList.remove("visible");
        setTimeout(() => {
            if (!tipBox.classList.contains("visible")) {
                tipBox.style.display = "none";
            }
        }, 200);
        currentEl = null;
    }

    // Reposition tooltip on scroll/resize
    function positionTip() {
        if (!currentEl) return;
        const rect = currentEl.getBoundingClientRect();
        const top = window.scrollY + rect.bottom + 8;
        const left = window.scrollX + rect.left;
        tipBox.style.top = `${top}px`;
        tipBox.style.left = `${left}px`;
    }

    window.addEventListener("scroll", positionTip);
    window.addEventListener("resize", positionTip);

    // Event listeners for focus
    document.addEventListener("focusin", e => {
        const el = e.target.closest("[data-tour]");
        if (!el) return;

        const key = el.dataset.tour;
        const lang = getLang();

        const msg =
            (window.PAMS_TIPS?.[key] && window.PAMS_TIPS[key][lang]) ||
            (window.PAMS_TIPS?.[key] && window.PAMS_TIPS[key].en);

        if (msg) showTip(el, msg);
    });

    document.addEventListener("focusout", e => {
        const el = e.target.closest("[data-tour]");
        if (!el) return;
        hideTip();
    });
});
