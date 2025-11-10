/* /js/pages/users/home/home-index.js */
(function ($, window, document) {
    "use strict";

    // Use your utils namespace (alias)
    const U = window.AMS?.util || {};

    // -------------------------
    // News Ticker (duplicate for seamless scroll)
    // -------------------------
    function initNewsTicker() {
        const $track = $("#newsTicker");
        if ($track.length === 0) return;
        if ($track.data("cloned")) return; // prevent double-clone on partial reloads
        $track.html($track.html() + $track.html());
        $track.data("cloned", true);
    }

    // -------------------------
    // Clickable table rows
    // -------------------------
    function initClickableRows() {
        $(document).on("click", ".clickable-row", function () {
            const href = $(this).data("href");
            if (href) window.location = href;
        });
    }

    // -------------------------
    // DataTable helper (prefer utils, fallback to legacy global)
    // -------------------------
    function initDataTable() {
        const make = U.makeDataTable || window.makeDataTable;
        if (typeof make === "function") {
            make("#tblCategories", 3);
        } else {
            U.log?.("DataTable init skipped: makeDataTable not found");
        }
    }

    // -------------------------
    // C3 Pie (same output, with guards)
    // -------------------------
    let chart = null;

    function renderPie() {
        U.assert?.(!!window.c3, "c3 missing for home-index pie");

        const rows = (window.dashData && window.dashData.categoryAssets) || [];
        if (!Array.isArray(rows) || rows.length === 0) return;

        // Build data object & labels
        const dataObj = {};
        const labels = [];
        rows.forEach(e => {
            labels.push(e.categoryName);
            dataObj[e.categoryName] = e.totalAssets;
        });

        // Destroy previous chart if any
        if (chart && chart.destroy) {
            try { chart.destroy(); } catch (_) { /* ignore */ }
        }

        // Generate chart
        chart = c3.generate({
            bindto: "#chart",
            data: {
                json: [dataObj],
                keys: { value: labels },
                type: "pie"
            },
            pie: { label: { show: true } },
            legend: { show: false },
            color: {
                pattern: [
                    "#1f77b4", "#aec7e8", "#ff7f0e", "#ffbb78",
                    "#2ca02c", "#98df8a", "#d62728", "#ff9896",
                    "#9467bd", "#c5b0d5"
                ]
            },
            size: { height: 380 },
            transition: { duration: 300 },
            tooltip: {
                grouped: false,
                format: {
                    value: function (value, ratio/*, id*/) {
                        const pct = (ratio * 100).toFixed(1);
                        return value + " (" + pct + "%)";
                    }
                }
            }
        });

        buildLegend(labels);
    }
    function buildLegend(labels) {
        const host = document.getElementById("chart");
        if (!host || !chart) return;

        const legendId = "categoryLegend";
        let legend = document.getElementById(legendId);
        if (!legend) {
            legend = document.createElement("div");
            legend.id = legendId;
            legend.className = "legend w-100 text-center p-2";
            host.insertAdjacentElement("afterend", legend);
        } else {
            legend.innerHTML = "";
        }

        labels.forEach(id => {
            const span = document.createElement("span");
            span.setAttribute("data-id", id);
            span.textContent = id;

            // style to match your original
            span.style.display = "inline-block";
            span.style.padding = "3px 6px";
            span.style.margin = "0 4px 4px 0";
            span.style.color = "#fff";
            span.style.borderRadius = "6px";
            span.style.backgroundColor = chart.color(id);

            span.addEventListener("mouseover", () => chart.focus(id));
            span.addEventListener("mouseout", () => chart.revert());
            span.addEventListener("click", () => chart.toggle(id));

            legend.appendChild(span);
        });
    }

    // -------------------------
    // Init (use utils-ready, not jQuery ready)
    // -------------------------
    function init(/* ctx */) {
        if (!U) return console.error("[AMS] utils not loaded.");
       
        initNewsTicker();
        initClickableRows();
        initDataTable();
        renderPie();
    };

    window.AMS?.pages?.register && window.AMS.pages.register("Home/Index", init);

})(jQuery, window, document);