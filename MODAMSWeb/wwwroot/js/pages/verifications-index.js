// /wwwroot/js/pages/verifications-index.js
(function ($, window, document) {
    "use strict";

    // --- Namespace safety ---
    const AMS = window.AMS || (window.AMS = {});
    const PAGES = AMS.pages || (AMS.pages = { register: function () { }, start: function () { } });
    const U = AMS.util || (AMS.util = {});

    // ---- Selectors / constants ----
    const SEL = {
        table: "#tblSchedule",
        chartHost: "#verificationBarChart",
        delBtn: ".js-del-schedule" // single source of truth
    };

    // Small helper: language + config
    function getLang() {
        try {
            return (window.getCurrentLanguage && window.getCurrentLanguage())
                || document.documentElement.getAttribute("lang")
                || "en";
        } catch { return "en"; }
    }

    function resolveConfig() {
        const cfg = window.VERIFICATIONS_CONFIG || {};
        const host = document.querySelector(SEL.chartHost);

        if (host) {
            if (!cfg.isSomali && host.dataset.isSomali) {
                cfg.isSomali = host.dataset.isSomali === "true";
            }
            if (!cfg.localizedStatuses && host.dataset.localizedStatuses) {
                try { cfg.localizedStatuses = JSON.parse(host.dataset.localizedStatuses); } catch { }
            }
            if (!cfg.chartData && host.dataset.chart) {
                try { cfg.chartData = JSON.parse(host.dataset.chart); } catch { }
            }
        }

        cfg.isSomali ??= (getLang() === "so");
        cfg.localizedStatuses ??= { Pending: "Pending", Ongoing: "Ongoing", Completed: "Completed" };
        cfg.chartData ??= [];

        return cfg;
    }

    // ---- Chart ----
    function loadMetricsChart() {
        if (!window.echarts) {
            console.warn("[AMS] ECharts is not loaded; skipping verification chart.");
            return;
        }

        const { isSomali, localizedStatuses, chartData } = resolveConfig();

        const statusLabels = (chartData || []).map(item =>
            isSomali ? (localizedStatuses[item.scheduleStatus] || item.scheduleStatus) : item.scheduleStatus
        );
        const scheduleCounts = (chartData || []).map(item => item.scheduleCount);

        const el = document.querySelector(SEL.chartHost);
        if (!el) {
            console.warn("[AMS] Chart host not found:", SEL.chartHost);
            return;
        }

        const chart = echarts.init(el);
        chart.setOption({
            grid: { top: 30, left: 70, right: 50, bottom: 80 },
            tooltip: { trigger: "axis", axisPointer: { type: "shadow" } },
            xAxis: {
                type: "category",
                data: statusLabels,
                axisLabel: { interval: 0, rotate: 30 }
            },
            yAxis: {
                type: "value",
                name: isSomali ? "Tirada Jadwalada" : "Number of Schedules"
            },
            series: [{
                name: "Schedules",
                type: "bar",
                data: scheduleCounts,
                label: { show: true, position: "top", formatter: "{c}" }
            }]
        });

        window.addEventListener("resize", () => chart.resize());
    }

    // ---- DataTable ----
    function initTable() {
        if (!U || typeof U.makeDataTable !== "function") {
            console.warn("[AMS] U.makeDataTable missing; skipping DataTable init.");
            return;
        }
        // type=1 => basic table (no export buttons)
        U.makeDataTable(SEL.table, "1");
    }

    // ---- Delete schedule (confirmation modal + fallback) ----
    function bindDelete() {
        $(document).off("click.viDel", SEL.delBtn).on("click.viDel", SEL.delBtn, function (e) {
            e.preventDefault();

            // respect disabled state (attr or class)
            if (this.disabled || this.classList.contains("disabled")) return;

            const id = this.getAttribute("data-schedule-id");
            if (!id) {
                console.warn("[AMS] Could not resolve schedule id for delete.");
                return;
            }

            const { isSomali } = resolveConfig();
            const actionUrl = `/Users/Verifications/DeleteSchedule/${encodeURIComponent(id)}`;

            const options = {
                actionUrl,
                title: isSomali ? "Tirtir Jadwalka" : "Delete Schedule",
                message: isSomali
                    ? "Ma hubtaa inaad tirtirto jadwalkan? Ficilkan lama celin karo."
                    : "Are you sure you want to delete this schedule? This action cannot be undone.",
                btnConfirmText: isSomali ? "Xaqiiji" : "Confirm",
                btnCancelText: isSomali ? "Jooji" : "Cancel"
            };

            if (typeof window.openConfirmation === "function") {
                window.openConfirmation(options);
                return;
            }

            // Fallback: safe POST with CSRF, then reload
            U.fetchJson(actionUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    ...(U.csrfHeader?.() || {})
                },
                body: "{}"
            })
                .then(() => window.location.reload())
                .catch(err => {
                    console.error("[AMS] Delete failed:", err);
                    if (typeof window.notif === "function") {
                        window.notif({
                            type: "error",
                            msg: isSomali ? "<b>Khalad:</b> Tirtirka wuu fashilmay." : "<b>Error:</b> Delete failed.",
                            position: "center", width: 480, height: 60, autohide: true
                        });
                    }
                });
        });
    }

    // ---- Bootstrap helpers (tooltips/popovers) ----
    function initBootstrapUI() {
        const hasBootstrap = !!window.bootstrap;
        if (!hasBootstrap) return;

        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            try { new bootstrap.Tooltip(el); } catch { }
        });

        document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => {
            try { new bootstrap.Popover(el, { trigger: "hover", html: true, container: "body" }); } catch { }
        });
    }

    function init() {
        try { U.hideMenu?.(); } catch { }
        initTable();
        initBootstrapUI();
        loadMetricsChart();
        bindDelete();
    }

    // Register with your page system (your pattern)
    PAGES.register && PAGES.register("Verifications/Index", init);

})(jQuery, window, document);
