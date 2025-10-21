// /wwwroot/js/pages/store-details.js
(function ($, window, document) {
    "use strict";

    // Run after DOM is ready
    $(function () {
        // --- clickable rows (idempotent) ---
        $(document)
            .off("click.pamsRow")
            .on("click.pamsRow", ".clickable-row", function () {
                var href = $(this).data("href");
                if (href) window.location = href;
            });

        // --- optionally hide/toggle menu on load (keep if you actually want this) ---
        var hideMenuFn =
            (window.PAMS && window.PAMS.util && window.PAMS.util.hideMenu) ||
            (window.U && window.U.hideMenu) ||
            window.hideMenu;
        if (typeof hideMenuFn === "function") {
            // uncomment if you want to auto-toggle on page load:
            // hideMenuFn();
        }

        // --- chart ---
        var chart = null;

        function renderPie() {
            var dataRoot = window.storeDetailsData || {};
            var rows = dataRoot.subcategoryAssets || [];
            if (!Array.isArray(rows) || rows.length === 0) return;
            if (!window.c3 || !document.getElementById("chart")) return;

            // camelCase/PascalCase helper
            function get(o, k1, k2) {
                return o && o[k1] != null ? o[k1] : (o ? o[k2] : undefined);
            }

            var dataObj = {};
            var labels = [];

            rows.forEach(function (e) {
                var label = get(e, "subCategoryName", "SubCategoryName");
                var value = +get(e, "totalAssets", "TotalAssets");
                if (!label) return;
                labels.push(label);
                dataObj[label] = value;
            });

            // Destroy previous chart if any
            if (chart && typeof chart.destroy === "function") {
                try { chart.destroy(); } catch (e) { /* ignore */ }
            }

            chart = c3.generate({
                bindto: "#chart",
                data: { json: [dataObj], keys: { value: labels }, type: "pie" },
                pie: { label: { show: true } },
                legend: { show: false }, // custom legend below
                color: {
                    pattern: [
                        "#1f77b4", "#aec7e8", "#ff7f0e", "#ffbb78",
                        "#2ca02c", "#98df8a", "#d62728", "#ff9896",
                        "#9467bd", "#c5b0d5", "#8c564b", "#c49c94",
                        "#e377c2", "#f7b6d2", "#7f7f7f", "#c7c7c7",
                        "#bcbd22", "#dbdb8d", "#17becf", "#9edae5"
                    ]
                },
                size: { height: 380 },
                transition: { duration: 300 },
                tooltip: {
                    grouped: false,
                    format: {
                        value: function (v, ratio) { return v + " (" + (ratio * 100).toFixed(1) + "%)"; }
                    }
                }
            });

            buildLegend(labels);
        }

        function buildLegend(labels) {
            var host = document.getElementById("chart");
            if (!host || !chart) return;

            var legendId = "storeLegend";
            var legend = document.getElementById(legendId);
            if (!legend) {
                legend = document.createElement("div");
                legend.id = legendId;
                legend.className = "legend w-100 text-center p-2";
                host.insertAdjacentElement("afterend", legend);
            } else {
                legend.innerHTML = "";
            }

            labels.forEach(function (id) {
                var span = document.createElement("span");
                span.setAttribute("data-id", id);
                span.textContent = id;

                // pill styles
                span.style.display = "inline-block";
                span.style.padding = "3px 6px";
                span.style.margin = "0 2px 2px 0";
                span.style.color = "#fff";
                span.style.borderRadius = "6px";
                span.style.backgroundColor = chart.color(id);
                span.style.cursor = "pointer";
                span.style.userSelect = "none";

                // interactions
                span.addEventListener("mouseover", function () { chart.focus(id); });
                span.addEventListener("mouseout", function () { chart.revert(); });
                span.addEventListener("click", function () { chart.toggle(id); });

                legend.appendChild(span);
            });
        }

        // kickoff
        renderPie();
    });

})(jQuery, window, document);
