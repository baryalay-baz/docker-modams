/* wwwroot/js/home-index.js */
(function ($, window, document) {
    "use strict";

    // -------------------------
    // News Ticker (duplicate for seamless scroll)
    // -------------------------
    function initNewsTicker() {
        var $track = $("#newsTicker");
        if ($track.length === 0) return;
        if ($track.data("cloned")) return;      // prevent double-clone on partial reloads
        $track.html($track.html() + $track.html());
        $track.data("cloned", true);
    }

    // -------------------------
    // Clickable table rows
    // -------------------------
    function initClickableRows() {
        $(document).on("click", ".clickable-row", function () {
            var href = $(this).data("href");
            if (href) window.location = href;
        });
    }

    // -------------------------
    // Your DataTable helper (unchanged)
    // -------------------------
    function initDataTable() {
        if (typeof makeDataTable === "function") {
            makeDataTable("#tblCategories", "1");
        }
    }

    // -------------------------
    // C3 Pie (exactly like your initial one)
    // -------------------------
    var chart = null;

    function renderPie() {
        var rows = (window.dashData && window.dashData.categoryAssets) || [];
        if (!Array.isArray(rows) || rows.length === 0) return;

        // Build the same data shape you had
        var dataObj = {};
        var labels = [];
        rows.forEach(function (e) {
            labels.push(e.categoryName);
            dataObj[e.categoryName] = e.totalAssets;
        });

        // Destroy previous chart if any (safe)
        if (chart && chart.destroy) {
            try { chart.destroy(); } catch (e) { }
        }

        // Generate chart — legend hidden (we’ll build our own like your original)
        chart = c3.generate({
            bindto: "#chart",
            data: {
                json: [dataObj],
                keys: { value: labels },
                type: "pie"
            },
            pie: {
                label: { show: true }
            },
            legend: { show: false },
            color: {
                pattern: [
                    "#1f77b4", "#aec7e8", "#ff7f0e", "#ffbb78",
                    "#2ca02c", "#98df8a", "#d62728", "#ff9896",
                    "#9467bd", "#c5b0d5"
                ]
            },
            size: { height: 380 },            // keep the size you liked
            transition: { duration: 300 },
            tooltip: {
                grouped: false,
                format: {
                    value: function (value, ratio/*, id*/) {
                        var pct = (ratio * 100).toFixed(1);
                        return value + " (" + pct + "%)";
                    }
                }
            }
        });

        // Build a simple custom legend like your original code
        buildLegend(labels);
    }

    function buildLegend(labels) {
        var host = document.getElementById("chart");
        if (!host) return;

        // Create (or reuse) a sibling legend div immediately after #chart
        var legendId = "categoryLegend";
        var legend = document.getElementById(legendId);
        if (!legend) {
            legend = document.createElement("div");
            legend.id = legendId;
            legend.className = "legend w-100 text-center p-2"; // same classes you used
            host.insertAdjacentElement("afterend", legend);
        } else {
            legend.innerHTML = ""; // reset
        }

        // Build items
        labels.forEach(function (id) {
            var span = document.createElement("span");
            span.setAttribute("data-id", id);
            span.textContent = id;

            // basic styling like your original (background per series color)
            span.style.display = "inline-block";
            span.style.padding = "3px 6px";
            span.style.margin = "0 4px 4px 0";
            span.style.color = "#fff";
            span.style.borderRadius = "6px";
            span.style.backgroundColor = chart.color(id); // <- c3 series color

            // c3 interactions (focus/revert/toggle)
            span.addEventListener("mouseover", function () { chart.focus(id); });
            span.addEventListener("mouseout", function () { chart.revert(); });
            span.addEventListener("click", function () { chart.toggle(id); });

            legend.appendChild(span);
        });
    }

    // -------------------------
    // Init (document ready)
    // -------------------------
    $(function () {
        initNewsTicker();
        initClickableRows();
        initDataTable();
        renderPie();
    });

})(jQuery, window, document);
