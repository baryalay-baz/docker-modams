(function () {
    const driverFactory = window?.driver?.js?.driver;
    if (!driverFactory) {
        console.error("Driver.js not loaded.");
        return;
    }

    function getPageKey() {
        return document.body.getAttribute("data-page") || "";
    }

    function filterExistingSteps(steps) {
        return steps.filter(s => {
            try { return document.querySelector(s.element); }
            catch { return false; }
        });
    }

    function startTourFor(pageKey) {
        const cfg = window.PAMS_TOUR_REGISTRY[pageKey];
        if (!cfg) return;

        const steps = filterExistingSteps(cfg.steps).map(s => ({
            ...s,
            popover: {
                ...(s.popover || {}),
                className: ((s.popover?.className || '') + ' pams-tour-popover').trim()
            }
        }));

        const driver = driverFactory({
            showProgress: true,
            overlayOpacity: 0.5,
            showButtons: ['previous', 'next', 'close'],
            steps
        });

        driver.drive();
    }

    document.addEventListener("DOMContentLoaded", () => {
        const pageKey = getPageKey();
        const fab = document.getElementById("PamsHelpFab");

        if (!pageKey || !fab) return;

        const scriptUrl = `/js/tours/${pageKey.replace(/\//g, '-').toLowerCase()}-tour.js`;
        const script = document.createElement("script");
        script.src = scriptUrl;

        script.onload = () => {
            console.log(`Tour script loaded: ${scriptUrl}`);

            if (window.PAMS_TOUR_REGISTRY?.[pageKey]) {
                fab.style.display = "inline-block";
                fab.onclick = () => startTourFor(pageKey);
            } else {
                console.warn(`Tour for ${pageKey} not found after script load.`);
            }
        };

        script.onerror = () => console.error(`Failed to load tour script: ${scriptUrl}`);
        document.body.appendChild(script);
    });

})();
