// /wwwroot/js/pages/users/stores/store-details.js
(function ($, window, document) {
    "use strict";
    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    function init(/*ctx*/) {
        U.hideMenu();
        wireClickableRows();
        renderPie();
    }

    function wireClickableRows() {
        $(document)
            .off("click.subCategoryRow")
            .on("click.subCategoryRow", ".clickable-row", function () {
                const href = $(this).data("href");
                if (href) window.location = href;
            });
    }

    function renderPie() {
        const dataRoot = window.storeDetailsData || {};
        const rows = dataRoot.subcategoryAssets || [];
        if (!Array.isArray(rows) || rows.length === 0) return;
        if (!window.c3 || !document.getElementById("chart")) return;

        const get = (o, k1, k2) => (o && o[k1] != null ? o[k1] : o?.[k2]);
        const dataObj = {};
        const labels = [];

        rows.forEach(e => {
            const label = get(e, "subCategoryName", "SubCategoryName");
            const value = +get(e, "totalAssets", "TotalAssets");
            if (!label) return;
            labels.push(label);
            dataObj[label] = value;
        });

        // create chart...
        const chart = c3.generate({
            bindto: "#chart",
            data: { json: [dataObj], keys: { value: labels }, type: "pie" },
            pie: { label: { show: true } },
            legend: { show: false },
            size: { height: 380 },
            transition: { duration: 300 },
            tooltip: {
                grouped: false,
                format: { value: (v, r) => `${v} (${(r * 100).toFixed(1)}%)` }
            }
        });

        buildLegend(chart, labels);
    }
    function buildLegend(chart, labels) {
        const host = document.getElementById("chart");
        if (!host || !chart) return;
        let legend = document.getElementById("storeLegend");
        if (!legend) {
            legend = document.createElement("div");
            legend.id = "storeLegend";
            legend.className = "legend w-100 text-center p-2";
            host.insertAdjacentElement("afterend", legend);
        } else {
            legend.innerHTML = "";
        }
        labels.forEach(id => {
            const pill = document.createElement("span");
            pill.dataset.id = id;
            pill.textContent = id;
            Object.assign(pill.style, {
                display: "inline-block",
                padding: "3px 6px",
                margin: "0 2px 2px 0",
                color: "#fff",
                borderRadius: "6px",
                backgroundColor: chart.color(id),
                cursor: "pointer",
                userSelect: "none"
            });
            pill.addEventListener("mouseover", () => chart.focus(id));
            pill.addEventListener("mouseout", () => chart.revert());
            pill.addEventListener("click", () => chart.toggle(id));
            legend.appendChild(pill);
        });
    }

    // Register with your page system
    AMS.pages?.register && AMS.pages.register("Stores/StoreDetails", init);

})(jQuery, window, document);
