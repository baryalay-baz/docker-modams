// /wwwroot/js/pages/disposals-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const CFG = window.DISPOSALS_PAGE || {}; // { isAuthorized, strings, labels, values }

    const SEL = {
        table: "#tblDisposals",
        wrap: ".project-list-table-container .export-table",
        pie: "#pieChartDisposals",

        // Preview modal
        modalId: "#mdlPreviewDisposal",
        deptName: "#txtDepartmentName",
        disposalType: "#txtDisposalType",
        subCategoryName: "#txtSubCategoryName",
        assetName: "#txtAssetName",
        identification: "#txtIdentification",
        notes: "#txtDisposalNotes",
        img: "#imgDisposalPreview"
    };

    const NS = ".disposalsIndex";

    function init() {
        U.hideMenu?.();
        initTableWithActions();
        initChart();
        bindDelegates();

        // Back-compat for any leftover inline handlers in Razor
        window.previewDisposal = previewDisposal;
        window.deleteDisposal = deleteDisposal;
    }

    /* ---------------- DataTable with ACTION BUTTONS ---------------- */
    function initTableWithActions() {
        const lang = (U.getCurrentLanguage?.() || "en").toLowerCase();
        const canEdit = !!CFG.isAuthorized;
        const t = (en, so) => (lang === "so" ? so : en);

        const baseButtons = [
            {
                key: "preview",
                title: t("Preview Disposal", "Horgaalid Ka-Takhalus"),
                iconHtml: "<i class='fe fe-eye'></i>",
                onClick: (ctx) => {
                    const { id } = resolveRowCtx(ctx, SEL.table);
                    if (id) previewDisposal(id);
                }
            },
            {
                key: "edit",
                title: t("Edit Disposal", "Wax ka beddel Ka-Takhalus"),
                iconHtml: "<i class='fe fe-edit'></i>",
                className: "act-edit",
                onClick: (ctx) => {
                    const { id } = resolveRowCtx(ctx, SEL.table);
                    if (id && canEdit) window.location.assign(`/Users/Disposals/EditDisposal/${id}`);
                }
            },
            {
                key: "delete",
                title: t("Delete Disposal", "Tirtir Ka-Takhalus"),
                iconHtml: "<i class='fe fe-trash'></i>",
                className: "btn-danger act-delete",
                onClick: (ctx) => {
                    const { id } = resolveRowCtx(ctx, SEL.table);
                    if (id && canEdit) deleteDisposal(id);
                }
            }
        ];

        const effectiveButtons = canEdit ? baseButtons : baseButtons.filter(b => b.key === "preview");

        U.makeDataTable(SEL.table, "1", 5, {
            enable: true,
            paddingPx: 160,
            buttons: effectiveButtons
        });

        const $wrap = $(SEL.wrap);
        if ($wrap.length) $wrap.css("overflow", "auto");
    }

    function resolveRowCtx(ctx, tableSel) {
        let $tr = $(ctx?.tr || []);
        if (!$tr.length && ctx?.event) $tr = $(ctx.event.target).closest("tr");
        if (!$tr.length) {
            try {
                const api = $(tableSel).DataTable();
                $tr = api.row({ selected: true }).nodes().to$();
            } catch { /* noop */ }
        }
        if (!$tr.length) $tr = $(`${tableSel} tbody tr:hover`).first();
        const id = $tr.data("id"); // REQUIRE: <tr data-id="...">
        return { $tr, id };
    }

    /* ---------------- Chart ---------------- */
    function initChart() {
        const canvas = document.querySelector(SEL.pie);
        if (!canvas || typeof Chart === "undefined") return;

        const labels = Array.isArray(CFG.labels) ? CFG.labels : [];
        const values = Array.isArray(CFG.values) ? CFG.values : [];
        if (!labels.length || !values.length || labels.length !== values.length) return;

        const ctx = canvas.getContext("2d");
        new Chart(ctx, {
            type: "pie",
            data: {
                labels,
                datasets: [{
                    data: values,
                    backgroundColor: makeColors(values.length)
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { position: "bottom" }, tooltip: { enabled: true } }
            }
        });
    }

    function makeColors(n) {
        const arr = [];
        for (let i = 0; i < n; i++) {
            const hue = Math.round((360 / Math.max(n, 1)) * i);
            arr.push(`hsl(${hue} 70% 55% / 0.85)`);
        }
        return arr;
    }

    /* ---------------- Delegates (optional if you drop inline handlers) ---------------- */
    function bindDelegates() {
        const $doc = $(document);
        $doc.off("click" + NS, "[data-action='preview-disposal']")
            .on("click" + NS, "[data-action='preview-disposal']", function () {
                const id = this.getAttribute("data-id");
                if (id) previewDisposal(id);
            });

        $doc.off("click" + NS, "[data-action='delete-disposal']")
            .on("click" + NS, "[data-action='delete-disposal']", function () {
                const id = this.getAttribute("data-id");
                if (id) deleteDisposal(id);
            });
    }

    /* ---------------- Preview (modal) via U.fetchJson ---------------- */
    async function previewDisposal(disposalId) {
        try {
            const dto = await U.fetchJson(`/Users/Disposals/GetPreview/${encodeURIComponent(disposalId)}`);
            // Populate fields
            setVal(SEL.deptName, dto.departmentName);
            setVal(SEL.disposalType, dto.disposalType);
            setVal(SEL.subCategoryName, dto.subCategoryName);
            setVal(SEL.assetName, dto.assetName);
            setVal(SEL.identification, dto.identification);
            setVal(SEL.notes, dto.disposalNotes);

            // Image
            const imgEl = document.querySelector(SEL.img);
            if (imgEl) imgEl.src = dto.imageUrl || "/assets/images/placeholders/pictureplaceholder.jpg";

            // Show modal
            const modalEl = document.querySelector(SEL.modalId);
            if (modalEl && window.bootstrap?.Modal) new bootstrap.Modal(modalEl).show();
        } catch (err) {
            console.error("previewDisposal error:", err);
            // Your project-wide error toaster (keep it consistent)
            if (typeof window.showErrorMessageJs === "function") {
                window.showErrorMessageJs(String(err.message || err));
            } else {
                alert(String(err.message || err));
            }
        }
    }

    function setVal(selector, value) {
        const el = document.querySelector(selector);
        if (!el) return;
        if ("value" in el) el.value = value ?? "";
        else el.textContent = value ?? "";
    }

    /* ---------------- Delete ---------------- */
    function deleteDisposal(id) {
        const S = CFG.strings || {};
        const options = {
            actionUrl: `/Users/Disposals/DeleteDisposal/${encodeURIComponent(id)}`,
            title: S.deleteTitle || "Delete Disposal",
            message: S.deleteMessage || "Are you sure you want to delete this disposal?",
            btnConfirmText: S.confirm || "Confirm",
            btnCancelText: S.cancel || "Cancel",
            onConfirm: () => window.location.assign(`/Users/Disposals/DeleteDisposal/${encodeURIComponent(id)}`)
        };

        if (typeof window.openConfirmation === "function") {
            try { window.openConfirmation(options); return; } catch { /* fallback */ }
        }
        if (window.confirm(options.message)) window.location.assign(options.actionUrl);
    }

    AMS.pages?.register?.("Disposals/Index", init);

})(jQuery, window, document);
