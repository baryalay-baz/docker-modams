// /wwwroot/js/pages/users/disposals/disposals-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const CFG = window.DISPOSALS_PAGE || {};

    const SEL = {
        table: "#tblDisposals",
        dtFrom: "#dtFrom",
        dtTo: "#dtTo",
        btnApply: "#btnApplyFilters",
        pie: "#pieChartDisposals",

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
    let dtApi = null;

    AMS.pages?.register?.("Disposals/Index", init);

    function init() {
        U.hideMenu?.();
        initPickers();
        initTable();
        initChart();
        bindEvents();
        window.previewDisposal = previewDisposal;
        window.deleteDisposal = deleteDisposal;
    }

    /* ---------------- Pickers ---------------- */
    function initPickers() {
        if (!$.fn.datepicker) return;
        const opts = {
            autoclose: true,
            todayHighlight: true,
            format: "yyyy-mm-dd",   // ISO output
            orientation: "bottom auto",
            container: "body"
        };
        $(SEL.dtFrom).datepicker(opts);
        $(SEL.dtTo).datepicker(opts);
    }

    /* ---------------- DataTable with actions ---------------- */
    function initTable() {
        const lang = (U.getCurrentLanguage?.() || "en").toLowerCase();
        const t = (en, so) => (lang === "so" ? so : en);
        const canEdit = !!CFG.isAuthorized;

        const buttons = [
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
                    if (!canEdit) return;
                    const { id } = resolveRowCtx(ctx, SEL.table);
                    if (id) window.location.assign(`/Users/Disposals/EditDisposal/${id}`);
                }
            },
            {
                key: "delete",
                title: t("Delete Disposal", "Tirtir Ka-Takhalus"),
                iconHtml: "<i class='fe fe-trash'></i>",
                className: "btn-danger act-delete",
                onClick: (ctx) => {
                    if (!canEdit) return;
                    const { id } = resolveRowCtx(ctx, SEL.table);
                    if (id) deleteDisposal(id);
                }
            }
        ];

        const effective = canEdit ? buttons : buttons.filter(b => b.key === "preview");

        dtApi = U.makeDataTable(SEL.table, "1", 10, {
            enable: true,
            paddingPx: 160,
            buttons: effective
        });
    }

    function resolveRowCtx(ctx, tableSel) {
        let $tr = $(ctx?.tr || []);
        if (!$tr.length && ctx?.event) $tr = $(ctx.event.target).closest("tr");
        if (!$tr.length) {
            try { $tr = $(tableSel).DataTable().row({ selected: true }).nodes().to$(); } catch { }
        }
        if (!$tr.length) $tr = $(`${tableSel} tbody tr:hover`).first();
        const id = $tr.data("id");
        const iso = $tr.data("date"); // yyyy-MM-dd
        return { $tr, id, iso };
    }

    /* ---------------- Date Filter ---------------- */
    function bindEvents() {
        $(document)
            .off("click" + NS, SEL.btnApply)
            .on("click" + NS, SEL.btnApply, function (e) {
                e.preventDefault();
                applyDateFilter();
            });
    }

    function dmyToIsoNum(s) {
        // accepts dd-MM-yyyy or dd-MMM-yyyy
        if (!s) return null;
        const m = String(s).match(/^(\d{2})[-/](\d{2}|\w{3})[-/](\d{4})$/i);
        if (!m) return null;
        const d = m[1];
        const mid = m[2];
        const y = m[3];
        let mm = null;

        if (/^\d{2}$/.test(mid)) {
            mm = mid;
        } else {
            const map = { jan: "01", feb: "02", mar: "03", apr: "04", may: "05", jun: "06", jul: "07", aug: "08", sep: "09", oct: "10", nov: "11", dec: "12" };
            const k = mid.toLowerCase();
            if (!(k in map)) return null;
            mm = map[k];
        }
        return Number(`${y}${mm}${d}`); // yyyymmdd as number
    }

    function isoAttrToNum(iso) {
        // iso = yyyy-MM-dd
        if (!iso || !/^\d{4}-\d{2}-\d{2}$/.test(iso)) return null;
        return Number(iso.replaceAll("-", "")); // yyyymmdd
    }

    function isoToNum(iso) {
        // expects yyyy-mm-dd
        if (!iso || !/^\d{4}-\d{2}-\d{2}$/.test(iso)) return null;
        return Number(iso.replaceAll("-", "")); // yyyymmdd
    }

    function applyDateFilter() {
        const fromNum = isoToNum(($(SEL.dtFrom).val() || "").trim());
        const toNum = isoToNum(($(SEL.dtTo).val() || "").trim());

        // remove prior filter
        $.fn.dataTable.ext.search = ($.fn.dataTable.ext.search || []).filter(f => f._amsTag !== "dsDate");

        const table = document.querySelector(SEL.table);
        if (!table) return;
        const targetId = table.id;

        if (!fromNum && !toNum) { try { dtApi?.draw(); } catch { } return; }

        const pred = function (settings, data, dataIndex) {
            if (settings?.nTable?.id !== targetId) return true;

            // read stable ISO from the row attribute
            const node = settings.aoData[dataIndex]?.nTr;
            const rowNum = isoToNum(node?.getAttribute("data-date"));
            if (rowNum == null) return false;

            if (fromNum && rowNum < fromNum) return false;
            if (toNum && rowNum > toNum) return false;
            return true;
        };
        pred._amsTag = "dsDate";
        $.fn.dataTable.ext.search.push(pred);

        try { dtApi?.draw(); } catch { }
    }

    /* ---------------- Chart ---------------- */
    function initChart() {
        const canvas = document.querySelector(SEL.pie);
        if (!canvas || typeof Chart === "undefined") return;

        const labels = Array.isArray(CFG.labels) ? CFG.labels : [];
        const values = Array.isArray(CFG.values) ? CFG.values : [];
        if (!labels.length || !values.length || labels.length !== values.length) return;

        new Chart(canvas.getContext("2d"), {
            type: "doughnut",
            data: {
                labels,
                datasets: [{
                    data: values,
                    borderWidth: 0,
                    backgroundColor: genColors(values.length)
                }]
            },
            options: {
                cutout: "62%",
                responsive: true,
                plugins: { legend: { position: "bottom" } }
            }
        });
    }

    function genColors(n) {
        const out = [];
        for (let i = 0; i < n; i++) out.push(`hsl(${Math.round(360 * i / Math.max(n, 1))} 70% 55% / 0.88)`);
        return out;
    }

    /* ---------------- Preview (uses U.fetchJson) ---------------- */
    async function previewDisposal(disposalId) {
        try {
            const dto = await U.fetchJson(`/Users/Disposals/GetPreview/${encodeURIComponent(disposalId)}`);
            setVal(SEL.deptName, dto.departmentName);
            setVal(SEL.disposalType, dto.disposalType);
            setVal(SEL.subCategoryName, dto.subCategoryName);
            setVal(SEL.assetName, dto.assetName);
            setVal(SEL.identification, dto.identification);
            setVal(SEL.notes, dto.disposalNotes);

            const imgEl = document.querySelector(SEL.img);
            if (imgEl) imgEl.src = dto.imageUrl || "/assets/images/placeholders/pictureplaceholder.jpg";

            const modalEl = document.querySelector(SEL.modalId);
            if (modalEl && window.bootstrap?.Modal) new bootstrap.Modal(modalEl).show();
        } catch (err) {
            console.error("previewDisposal error:", err);
            window.showErrorMessageJs?.(String(err.message || err));
        }
    }

    function setVal(sel, v) {
        const el = document.querySelector(sel);
        if (!el) return;
        if ("value" in el) el.value = v ?? "";
        else el.textContent = v ?? "";
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
            try { window.openConfirmation(options); return; } catch { }
        }
        if (window.confirm(options.message)) window.location.assign(options.actionUrl);
    }

})(jQuery, window, document);
