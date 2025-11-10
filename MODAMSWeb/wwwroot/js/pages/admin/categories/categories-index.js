// /wwwroot/js/pages/categories-index.js
(function ($, w, d) {
    "use strict";

    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    const SEL = {
        categoriesTable: "#tblCategories",
        subcategoriesWrap: "#dvSubCategories",
        subcategoriesTable: "#tblSubCategories",
        newSubBtn: "#btnCreateSubCategory",
        hiddenCatId: "#hiddenCategoryId",
        lblCategory: "#lblCategory",
    };

    function init() {
        U.hideMenu?.();
        initDataTables();
        bindCategoryRowClick();
        initCreateSubcategoryLink();
    }

    function initDataTables() {
        // Hover actions (overlay pills) – no visible Action column needed.
        const catActions = {
            enable: true,
            paddingPx: 140,
            buttons: [
                {
                    key: "edit",
                    titleEn: "Edit Category",
                    titleSo: "Tafatir Qayb",
                    iconHtml: "<i class='fe fe-edit'></i>",
                    href: "/Admin/Categories/EditCategory/{id}",
                },
            ],
        };
        U.makeDataTable(SEL.categoriesTable, "1", 100, catActions);

        if ($(SEL.subcategoriesTable).length) {
            const subActions = {
                enable: true,
                paddingPx: 160,
                buttons: [
                    {
                        key: "edit",
                        titleEn: "Edit Sub-Category",
                        titleSo: "Tafatir Qayb-hoosaad",
                        iconHtml: "<i class='fe fe-edit'></i>",
                        href: "/Admin/Categories/EditSubCategory/{id}",
                    },
                ],
            };
            U.makeDataTable(SEL.subcategoriesTable, "1", 100, subActions);
        }
    }

    function bindCategoryRowClick() {
        const $tbl = $(SEL.categoriesTable);
        const api = $tbl.DataTable?.();

        $tbl.on("click", "tbody tr", async function () {
            const $tr = $(this);
            const already = $tr.hasClass("trShadow");

            if (already) {
                $tr.removeClass("trShadow");
                return;
            }

            if (api) $(api.rows().nodes()).removeClass("trShadow");
            else $tbl.find("tbody tr").removeClass("trShadow");

            $tr.addClass("trShadow");

            const catId = ($tr.data("id") ?? "").toString().trim();
            const catName = ($tr.find(".cn").text() || "").trim();

            if (!catName) return;

            $(SEL.lblCategory).text(catName);
            $(SEL.hiddenCatId).val(catId || "");
            enableCreateSubBtn();

            await loadSubCategoryData(catName);
        });
    }

    function initCreateSubcategoryLink() {
        const $btn = $(SEL.newSubBtn);
        if (!$btn.length) return;

        if (!$btn.data("baseHref")) {
            $btn.data("baseHref", $btn.attr("href") || "/Admin/Categories/CreateSubCategory");
        }

        $btn.on("click", function (e) {
            const id = ($(SEL.hiddenCatId).val() || "").toString().trim();
            if (!id) {
                e.preventDefault();
                return false;
            }
            const base = $btn.data("baseHref");
            $btn.attr("href", `${base}/${encodeURIComponent(id)}`);
            return true;
        });
    }

    function disableCreateSubBtn() {
        $(SEL.newSubBtn).addClass("disabled");
    }
    function enableCreateSubBtn() {
        $(SEL.newSubBtn).removeClass("disabled");
    }

    async function loadSubCategoryData(categoryName) {
        try {
            const url = `/Admin/Categories/GetSubCategories?categoryName=${encodeURIComponent(
                categoryName
            )}`;
            const data = await U.fetchJson(url);
            rebuildSubcategoriesTable(Array.isArray(data) ? data : []);
        } catch (err) {
            console.error("Error fetching subcategories:", err);
            U.showErrorMessageJs?.("Failed to load subcategories.");
            $(SEL.subcategoriesWrap).html(
                '<p class="text-danger">Failed to load subcategories.</p>'
            );
        }
    }

    function rebuildSubcategoriesTable(rows) {
        if (!rows.length) {
            $(SEL.subcategoriesWrap).html('<p class="text-muted">No records found.</p>');
            return;
        }

        // Build WITHOUT an Action column; rows carry data-id for hover-actions.
        let html = `
        <div class="toolbar d-flex align-items-center gap-2 flex-wrap mb-2">
            <a asp-area="Admin"
                asp-controller="Categories"
                asp-action="CreateSubCategory"
                class="btn btn-outline-primary"
                data-tour="cat.sub.new" title="${localize("NewSubCategory")}">
                <i class="fa fa-circle-plus" aria-hidden="true"></i>
                <span>${localize("NewSubCategory")}</span>
            </a>
        </div>
      <table id="tblSubCategories"
             class="table text-nowrap mb-0 table-bordered border-top border-bottom project-list-main"
             data-tour="cat.sub.table">
        <thead class="table-head bg-primary-gradient ms-auto divShadow">
          <tr>
            <th class="text-white bg-transparent border-bottom-0 w-5">#</th>
            <th class="text-white bg-transparent border-bottom-0">${escapeHtml(
            localize("SubCategoryName")
        )}</th>
            <th class="text-white bg-transparent border-bottom-0">${escapeHtml(
            localize("SubCategoryNameSomali")
        )}</th>
          </tr>
        </thead>
        <tbody>
    `;

        rows.forEach((e, i) => {
            const n = String(i + 1).padStart(2, "0");
            html += `
        <tr data-id="${escapeHtml(e.id)}">
          <td class="text-muted fs-15 fw-semibold text-center">${n}</td>
          <td class="text-muted fs-15 fw-semibold">${escapeHtml(e.subCategoryName)}</td>
          <td class="text-muted fs-15 fw-semibold">${escapeHtml(e.subCategoryNameSo)}</td>
        </tr>`;
        });

        html += `</tbody></table>`;

        $(SEL.subcategoriesWrap).html(html);

        const subActions = {
            enable: true,
            paddingPx: 160,
            buttons: [
                {
                    key: "edit",
                    titleEn: "Edit Sub-Category",
                    titleSo: "Tafatir Qayb-hoosaad",
                    iconHtml: "<i class='fe fe-edit'></i>",
                    href: "/Admin/Categories/EditSubCategory/{id}",
                },
            ],
        };
        U.makeDataTable("#tblSubCategories", "1", 100, subActions);
    }

    function escapeHtml(s) {
        return U.escapeHtml ? U.escapeHtml(s) : String(s ?? "");
    }
    function localize(key) {
        const so = U.getCurrentLanguage?.() === "so";
        const map = {
            SubCategoryName: so ? "Magaca Qayb-hoosaad" : "Sub-Category Name",
            SubCategoryNameSomali: so ? "Magaca Qayb-hoosaad (Somali)" : "Sub-Category (Somali)",
            NewSubCategory: so ? "Qayb-hoosaad Cusub" : "New Sub-Category",
        };
        return map[key] || key;
    }

    AMS.pages?.register?.("Categories/Index", init);

})(jQuery, window, document);
