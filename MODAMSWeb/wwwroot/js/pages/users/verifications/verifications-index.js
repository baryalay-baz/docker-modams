// /wwwroot/js/pages/users/verifications/verifications-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const PAGES = AMS.pages || (AMS.pages = { register() { }, start() { } });
    const U = AMS.util || (AMS.util = {});
    const SEL = { table: "#tblSchedule", chartHost: "#verificationBarChart" };

    function getLang() {
        try { return (window.getCurrentLanguage && window.getCurrentLanguage()) || document.documentElement.lang || "en"; }
        catch { return "en"; }
    }

    function resolveConfig() {
        const cfg = window.VERIFICATIONS_CONFIG || {};
        cfg.isSomali ??= (getLang() === "so");
        cfg.localizedStatuses ??= { Pending: "Pending", Ongoing: "Ongoing", Completed: "Completed" };
        cfg.chartData ??= [];
        return cfg;
    }

    // ---- Chart (unchanged) ----
    function loadMetricsChart() {
        if (!window.echarts) return;
        const { isSomali, localizedStatuses, chartData } = resolveConfig();
        const el = document.querySelector(SEL.chartHost);
        if (!el) return;

        const labels = chartData.map(d => isSomali ? (localizedStatuses[d.scheduleStatus] || d.scheduleStatus) : d.scheduleStatus);
        const values = chartData.map(d => d.scheduleCount);

        const chart = echarts.init(el);
        chart.setOption({
            grid: { top: 30, left: 70, right: 50, bottom: 80 },
            tooltip: { trigger: "axis", axisPointer: { type: "shadow" } },
            xAxis: { type: "category", data: labels, axisLabel: { interval: 0, rotate: 30 } },
            yAxis: { type: "value", name: isSomali ? "Tirada Jadwalada" : "Number of Schedules" },
            series: [{ name: "Schedules", type: "bar", data: values, label: { show: true, position: "top", formatter: "{c}" } }]
        });
        window.addEventListener("resize", () => chart.resize());
    }

    // ---- DataTable + hover actions (Assets/Index pattern) ----
    function initTable() {
        if (!U || typeof U.makeDataTable !== "function") return;

        const { isSomali } = resolveConfig();

        const actionsConfig = {
            enable: true,
            paddingPx: 160,
            buttons: [
                {
                    key: "preview",
                    titleEn: "Preview Schedule",
                    titleSo: "Daawo Jadwalka",
                    iconHtml: "<i class='fe fe-eye'></i>",
                    href: "/Users/Verifications/PreviewSchedule/{id}"
                },
                {
                    key: "edit",
                    titleEn: "Edit Schedule",
                    titleSo: "Wax ka beddel Jadwal",
                    iconHtml: "<i class='fe fe-edit'></i>",
                    href: "/Users/Verifications/EditSchedule/{id}"
                },
                {
                    key: "delete",
                    titleEn: "Delete Schedule",
                    titleSo: "Tirtir Jadwalka",
                    iconHtml: "<i class='fe fe-trash'></i>",
                    onClick: ({ id }) => {
                        if (!id) return;
                        const opts = {
                            actionUrl: `/Users/Verifications/DeleteSchedule/${encodeURIComponent(id)}`,
                            title: isSomali ? "Tirtir Jadwalka" : "Delete Schedule",
                            message: isSomali
                                ? "Ma hubtaa inaad tirtirto jadwalkan? Ficilkan lama celin karo."
                                : "Are you sure you want to delete this schedule? This action cannot be undone.",
                            btnConfirmText: isSomali ? "Xaqiiji" : "Confirm",
                            btnCancelText: isSomali ? "Jooji" : "Cancel"
                        };
                        if (typeof window.openConfirmation === "function") return window.openConfirmation(opts);
                        U.fetchJson(opts.actionUrl, {
                            method: "POST",
                            headers: { "Content-Type": "application/json", ...(U.csrfHeader?.() || {}) },
                            body: "{}"
                        }).then(() => window.location.reload())
                            .catch(err => console.error("[AMS] Delete failed:", err));
                    }
                }
            ]
        };

        // mode "2" = show export buttons (match Assets/Index). Use "1" to hide.
        const api = U.makeDataTable(SEL.table, "2", 10, actionsConfig);

        // After pills are injected, hide Edit/Delete for non-Pending rows
        function applyRowVisibility() {
            const rows = api.rows({ page: "current" }).nodes().to$();
            rows.each(function () {
                const status = this.getAttribute("data-status");
                const $actions = $(this).children(".row-actions");
                if (!$actions.length) return;
                const showEditDelete = status === "Pending";
                $actions.find(".act-edit, .act-delete").toggle(showEditDelete);
            });
        }

        if (api && api.on) {
            api.on("draw.vi", applyRowVisibility);
            applyRowVisibility();
        }
    }

    function initBootstrapUI() {
        if (!window.bootstrap) return;
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => { try { new bootstrap.Tooltip(el); } catch { } });
        document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => { try { new bootstrap.Popover(el, { trigger: "hover", html: true, container: "body" }); } catch { } });
    }

    function init() {
        try { AMS.util.hideMenu?.(); } catch { }
        initTable();
        initBootstrapUI();
        loadMetricsChart();
    }

    PAGES.register && PAGES.register("Verifications/Index", init);
})(jQuery, window, document);
