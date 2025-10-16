(function ($, window, document) {
    "use strict";

    hideMenu(); // in utils.js

    function initClickableRows() {
        $(document).on("click", ".clickable-row", function () {
            var href = $(this).data("href");
            if (href) window.location = href;
        });
    }

    var chart = null;

    function renderPie() {
        var rows = (window.storeDetailsData && window.storeDetailsData.subcategoryAssets) || [];
        if (!Array.isArray(rows) || rows.length === 0) return;

        // camelCase/PascalCase keys
        function get(o, k1, k2) { return o[k1] != null ? o[k1] : o[k2]; }

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
        if (chart && chart.destroy) {
            try { chart.destroy(); } catch (e) { }
        }

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
            legend: {
                show: false   // to render custom legend
            },
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
                    value: function (value, ratio/*, id*/) {
                        var pct = (ratio * 100).toFixed(1);
                        return value + " (" + pct + "%)";
                    }
                }
            }
        });

        buildLegend(labels);
    }

    function buildLegend(labels) {
        var host = document.getElementById("chart");
        if (!host) return;

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

            // pills 
            span.style.display = "inline-block";
            span.style.padding = "3px 6px";
            span.style.margin = "0 2px 2px 0";   // tight spacing
            span.style.color = "#fff";
            span.style.borderRadius = "6px";
            span.style.backgroundColor = chart.color(id);
            span.style.cursor = "pointer";

            // interactions (focus/revert/toggle)
            span.addEventListener("mouseover", function () { chart.focus(id); });
            span.addEventListener("mouseout", function () { chart.revert(); });
            span.addEventListener("click", function () { chart.toggle(id); });

            legend.appendChild(span);
        });
    }
    $(function () {
        initClickableRows();
        //makeDataTable("#tblCategories", "1");
        renderPie();
    });

})(jQuery, window, document);
