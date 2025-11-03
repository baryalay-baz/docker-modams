// /wwwroot/js/pages/transfers-index.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const CFG = window.TRANSFERS_PAGE || {};

    const SEL = {
        store: "#ddStores",
        dtFrom: "#dtFrom",
        dtTo: "#dtTo",
        btnApply: "#btnApplyFilters",
        txtSearch: "#txtSearchTransferByAsset",
        btnSearch: "#btnSearchTransferByAsset",

        outgoingTable: "#tblTransfersOut",
        incomingTable: "#tblTransfersIn",

        spnNew: "#spnNew",
        pieOut: "#pieChartOutgoing",
        pieIn: "#pieChartIncoming",
    };

    const NS = ".transfersIndex";
    let outApi = null;
    let inApi = null;

    const STATE = {
        filterActive: false
    };

    function init() {
        if (!U) { console.error("[AMS] utils not loaded."); return; }

        U.hideMenu?.();
        initSelect2();
        initPickers();
        bindEvents();

        initOutgoingTable();
        initIncomingTable();

        renderOutgoingPie();
        renderIncomingPie();

        if (hasAnyDate()) {
            applyDateFilter();
            setFilterToggle(true);
        } else {
            setFilterToggle(false);
        }
    }

    /* ---------------- Events ---------------- */
    function bindEvents() {
        const $doc = $(document);

        $doc.off("change" + NS, SEL.store).on("change" + NS, SEL.store, function () {
            const id = $(this).val();
            if (!id) return;
            window.location.href = `/Users/Transfers/Index/${encodeURIComponent(id)}`;
        });

        $doc.off("click" + NS, SEL.btnApply).on("click" + NS, SEL.btnApply, function (e) {
            e.preventDefault();

            if (STATE.filterActive) {
                clearDateFilter();
                setFilterToggle(false);
                return;
            }

            if (!hasAnyDate()) {
                U.Notify?.("warning", "Please enter From and/or To date first.");
                return;
            }
            applyDateFilter();
            setFilterToggle(true);
        });

        $doc.off("click" + NS, SEL.btnSearch).on("click" + NS, SEL.btnSearch, function (e) {
            e.preventDefault();
            runGlobalSearch();
        });
        $doc.off("keydown" + NS, SEL.txtSearch).on("keydown" + NS, SEL.txtSearch, function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                runGlobalSearch();
            }
        });

        // Fallback delegated delete
        $doc.off("click" + NS, "[data-action='delete-transfer']").on("click" + NS, "[data-action='delete-transfer']", function () {
            const id = this.getAttribute("data-id");
            if (!id) return;
            openDeleteConfirm(id);
        });
    }

    /* ---------------- Widgets ---------------- */
    function initSelect2() {
        const $sel = $(SEL.store);
        if (!$sel.length) return;
        if ($.fn.select2 && !$sel.data("select2")) {
            $sel.select2({ width: "style" });
        }
    }
    function initPickers() {
        if ($.fn.datepicker) {
            $(SEL.dtFrom + "," + SEL.dtTo).datepicker({
                autoclose: true,
                format: "dd-MM-yyyy",
                todayHighlight: true
            });
        } else {
            console.warn("[AMS] bootstrap-datepicker not found; skipping date pickers.");
        }
    }

    /* ---------------- Helpers ---------------- */
    function hasAnyDate() {
        const f = ($(SEL.dtFrom).val() || "").trim();
        const t = ($(SEL.dtTo).val() || "").trim();
        return !!(f || t);
    }
    function setFilterToggle(active) {
        STATE.filterActive = !!active;
        const $btn = $(SEL.btnApply);
        if (!$btn.length) return;

        if (STATE.filterActive) {
            // Show “Remove filter”
            $btn
                .removeClass("btn-outline-primary")
                .addClass("btn-outline-danger")
                .html("<i class='fe fe-x-circle'></i><span class='ms-2'>Remove filter</span>")
                .attr("title", "Remove filter");
        } else {
            // Back to “Apply”
            $btn
                .removeClass("btn-outline-danger")
                .addClass("btn-outline-primary")
                .html("<i class='fe fe-filter'></i><span class='ms-2'>Apply</span>")
                .attr("title", "Apply");
        }
    }

    /* ---------------- Resolve row/id/status ---------------- */
    function resolveRowCtx(ctx, tableSel) {
        let $tr = $(ctx?.tr || []);
        if (!$tr.length && ctx?.event) $tr = $(ctx.event.target).closest("tr");
        if (!$tr.length) {
            try {
                const api = $(tableSel).DataTable();
                $tr = api.row({ selected: true }).nodes().to$();
            } catch { /* ignore */ }
        }
        if (!$tr.length) $tr = $(`${tableSel} tbody tr:hover`).first();
        const id = ctx?.id || $tr.data("id");
        const statusId = Number($tr.data("status-id")) || 0;
        return { $tr, id, statusId };
    }

    /* ---------------- Tables: Outgoing ---------------- */
    function initOutgoingTable() {
        const isSo = U.getCurrentLanguage?.() === "so";
        const S = CFG.strings || {};
        const isAuthorized = String(CFG.isAuthorized) === "true";

        const tPreview = isSo ? "Hordhaca Wareejinta" : (S.previewTransfer || "Preview Transfer");
        const tEdit = isSo ? "Wax ka beddel Wareejinta" : (S.editTransfer || "Edit Transfer");
        const tDelete = isSo ? "Tirtir Wareejinta" : (S.deleteTransfer || "Delete Transfer");

        outApi = U.makeDataTable(SEL.outgoingTable, "1", 10, {
            enable: true,
            paddingPx: 160,
            buttons: [
                {
                    key: "info",
                    title: tPreview,
                    iconHtml: "<i class='fe fe-eye'></i>",
                    onClick: (ctx) => {
                        const R = resolveRowCtx(ctx, SEL.outgoingTable);
                        if (R.id) window.location = `/Users/Transfers/PreviewTransfer/${R.id}`;
                    }
                },
                {
                    key: "edit",
                    title: tEdit,
                    iconHtml: "<i class='fe fe-edit'></i>",
                    className: "act-edit",
                    onClick: (ctx) => {
                        const R = resolveRowCtx(ctx, SEL.outgoingTable);
                        if (!R.id) return;
                        if (!isAuthorized) {
                            U.Notify?.("error", isSo ? "Lama oggola in wax laga beddelo." : "You are not authorized to edit this transfer.");
                            return;
                        }
                        if (R.statusId !== 1) return; // only Pending
                        window.location = `/Users/Transfers/EditTransfer/${R.id}`;
                    }
                },
                {
                    key: "delete",
                    title: tDelete,
                    iconHtml: "<i class='fe fe-trash'></i>",
                    className: "btn-danger act-delete",
                    onClick: (ctx) => {
                        const R = resolveRowCtx(ctx, SEL.outgoingTable);
                        if (!R.id) return;
                        if (R.statusId !== 1) return; // only Pending
                        openDeleteConfirm(R.id);
                    }
                }
            ]
        });

        enforceOutgoingActionVisibility(outApi, isAuthorized);
    }
    function enforceOutgoingActionVisibility(api, isAuthorized) {
        if (!api || typeof api.rows !== "function") return;

        function apply() {
            const $rows = api.rows({ page: "current" }).nodes().to$();
            $rows.each(function () {
                const $tr = $(this);
                const statusId = Number($tr.data("status-id")) || 0;
                const allow = (statusId === 1) && !!isAuthorized;

                const $actions = $tr.children(".row-actions");
                if ($actions.length) {
                    $actions.find(".act-edit, .act-delete").toggle(allow);
                }
            });
        }

        apply();
        api.off(".pendingMask").on("draw.pendingMask", apply);
    }

    /* ---------------- Tables: Incoming ---------------- */
    function initIncomingTable() {
        const isSo = U.getCurrentLanguage?.() === "so";
        const S = CFG.strings || {};
        const tPreview = isSo ? "Hordhaca Wareejinta" : (S.previewTransfer || "Preview Transfer");

        inApi = U.makeDataTable(SEL.incomingTable, "1", 10, {
            enable: true,
            paddingPx: 140,
            buttons: [
                {
                    key: "info",
                    title: tPreview,
                    iconHtml: "<i class='fe fe-eye'></i>",
                    onClick: (ctx) => {
                        const R = resolveRowCtx(ctx, SEL.incomingTable);
                        if (R.id) window.location = `/Users/Transfers/PreviewTransfer/${R.id}`;
                    }
                }
            ]
        });
    }

    /* ---------------- Filters & Search ---------------- */
    function runGlobalSearch() {
        const q = ($(SEL.txtSearch).val() || "").trim();
        try { outApi?.search(q).draw(); } catch { }
        try { inApi?.search(q).draw(); } catch { }
    }
    function parseDmyFlexible(s) {
        if (!s) return null;
        const t = Date.parse(s);
        if (!Number.isNaN(t)) return new Date(t);

        const m = String(s).match(/^(\d{2})[-/](\d{2}|\w{3})[-/](\d{4})$/i);
        if (!m) return null;
        const d = parseInt(m[1], 10);
        const mm = m[2];
        const y = parseInt(m[3], 10);
        let month = NaN;

        if (/^\d{2}$/.test(mm)) month = parseInt(mm, 10) - 1;
        else {
            const idx = ["jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec"].indexOf(mm.toLowerCase());
            month = idx >= 0 ? idx : NaN;
        }
        if (Number.isNaN(month)) return null;
        return new Date(y, month, d);
    }
    function applyDateFilter() {
        const fromStr = ($(SEL.dtFrom).val() || "").trim();
        const toStr = ($(SEL.dtTo).val() || "").trim();
        const from = parseDmyFlexible(fromStr);
        const to = parseDmyFlexible(toStr);

        // Remove prior filter (by our tag) to avoid stacking
        $.fn.dataTable.ext.search = ($.fn.dataTable.ext.search || []).filter(f => f._amsTag !== "trDate");

        const f = function (settings, data) {
            const id = settings?.nTable?.id;
            const outId = stripHash(SEL.outgoingTable);
            const inId = stripHash(SEL.incomingTable);
            if (id !== outId && id !== inId) return true;

            // date is column index 1 in both tables
            const cell = data[1] || "";
            const dt = parseDmyFlexible(cell);
            if (!from && !to) return true;
            if (!dt) return false;
            if (from && dt < from) return false;
            if (to && dt > to) return false;
            return true;
        };
        f._amsTag = "trDate";
        $.fn.dataTable.ext.search.push(f);

        try { outApi?.draw(); } catch { }
        try { inApi?.draw(); } catch { }
    }
    function clearDateFilter() {
        // Remove our tagged filter
        $.fn.dataTable.ext.search = ($.fn.dataTable.ext.search || []).filter(f => f._amsTag !== "trDate");

        // Clear inputs
        $(SEL.dtFrom).val("");
        $(SEL.dtTo).val("");

        // Redraw tables
        try { outApi?.draw(); } catch { }
        try { inApi?.draw(); } catch { }
    }
    function stripHash(sel) { return sel && sel[0] === "#" ? sel.slice(1) : sel; }

    /* ---------------- Charts (optional placeholders) ---------------- */
    function renderOutgoingPie() {
        const canvas = $(SEL.pieOut)[0];
        if (!canvas || typeof Chart === "undefined") return;
    }
    function renderIncomingPie() {
        const canvas = $(SEL.pieIn)[0];
        if (!canvas || typeof Chart === "undefined") return;
    }

    /* ---------------- Confirm ---------------- */
    function openDeleteConfirm(id) {
        const S = CFG.strings || {};
        const options = {
            actionUrl: `/Users/Transfers/DeleteTransfer/${id}`,
            title: S.deleteTransfer || "Delete Transfer",
            message: S.deleteConfirmationMessage || "Are you sure you want to delete this transfer?",
            btnConfirmText: S.confirm || "Confirm",
            btnCancelText: S.cancel || "Cancel",
            onConfirm: () => {
                window.location.href = `/Users/Transfers/DeleteTransfer/${id}`;
            }
        };

        if (typeof window.openConfirmation === "function") {
            try {
                window.openConfirmation(options);
                return;
            } catch (e) { /* fallback below */ }
        }

        if (window.confirm(options.message)) {
            window.location.href = options.actionUrl;
        }
    }

    AMS.pages?.register?.("Transfers/Index", init);

})(jQuery, window, document);
