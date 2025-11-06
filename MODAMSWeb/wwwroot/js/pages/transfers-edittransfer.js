// /wwwroot/js/pages/transfers-edittransfer.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        form: "#frmTransfer",
        btnSubmit: "#btnSubmit",
        btnDelete: "#btnDeleteTransfer",
        hiddenIds: "#selectedAssets",
        table: "#tblTransfers",
        tableBody: "#tblTransfers tbody",
        countSpan: "#spnSelection",
        listBody: "#tbodySelection",
        transferDate: "#Transfer_TransferDate",
        select2: ".select2",
        datepickers: ".picker"
    };

    const PD = (window.AMS && window.AMS.pageData) || {};
    const I18N = Object.assign({
        Category: "Category",
        AssetName: "Asset Name",
        SerialNo: "Serial No",
        Selected: "Selected",
        Warning: "Warning",
        NoAssetsSelected: "No assets selected"
    }, PD.i18n || {});

    const STATE = {
        isSo: (U.getCurrentLanguage?.() || "en") === "so",
        assets: Array.isArray(PD.assets) ? PD.assets : [],
        byId: new Map(),
        selectionIds: new Set(),
        submitting: false,
        deleteOptions: PD.delete || null
    };

    function normalize(a) {
        return {
            id: Number(a.AssetId),
            assetName: a.AssetName || "",
            category: a.Category || "",
            subCategory: a.SubCategory || a.Category || "",
            serialNumber: a.SerialNumber || "",
            barcode: a.Barcode || "",
            imageUrl: a.ImageUrl || "",
            isSelected: !!a.IsSelected
        };
    }

    function init() {
        U.hideMenu?.();

        for (const raw of STATE.assets) {
            const n = normalize(raw);
            if (!Number.isFinite(n.id)) continue;
            STATE.byId.set(n.id, n);
            if (n.isSelected) STATE.selectionIds.add(n.id);
        }

        // Widgets (idempotent)
        if ($.fn?.select2) $(SEL.select2).select2({ width: "resolve" });
        if ($.fn?.datepicker) $(SEL.datepickers).datepicker({
            autoclose: true, format: "dd-MM-yyyy", todayHighlight: true
        });
        U.makeDataTable?.(SEL.table, "1");

        // Initial render
        renderSelectionPanel();
        updateCount();

        bindEvents();
    }

    function bindEvents() {
        $(SEL.btnSubmit).on("click", onSubmit);
        $(SEL.btnDelete).on("click", onDelete);

        // Delegated — works regardless of inline onclick
        $(document).on("change", `${SEL.tableBody} input[type='checkbox'][name$='IsSelected']`, function () {
            const $cb = $(this);
            const $tr = $cb.closest("tr");
            const id = Number($tr.find("input[type='hidden'][name$='AssetId']").val());
            if (!Number.isFinite(id)) return;
            toggleSelection(id, $cb.is(":checked"));
        });
    }

    function toggleSelection(id, checked) {
        if (checked) STATE.selectionIds.add(id);
        else STATE.selectionIds.delete(id);
        renderSelectionPanel();
        updateCount();
    }

    function renderSelectionPanel() {
        const $tbody = $(SEL.listBody);
        if (!$tbody.length) return;

        const rows = [];
        for (const id of STATE.selectionIds) {
            const a = STATE.byId.get(id);
            if (!a) continue;

            rows.push(`
        <tr data-id="${U.escapeHtml(String(id))}">
          <td style="width:90px; text-align:center;">
            ${a.imageUrl ? `<img src="${U.escapeHtml(a.imageUrl)}" alt="" style="width:80px; height:auto;">` : ""}
          </td>
          <td>
            <p class="mb-1"><span class="text-muted">${U.escapeHtml(I18N.Category)}:</span> <strong>${U.escapeHtml(a.subCategory)}</strong></p>
            <p class="mb-1"><span class="text-muted">${U.escapeHtml(I18N.AssetName)}:</span> <strong>${U.escapeHtml(a.assetName)}</strong></p>
            <p class="mb-0"><span class="text-muted">${U.escapeHtml(I18N.SerialNo)}:</span> <strong>${U.escapeHtml(a.serialNumber)}</strong></p>
          </td>
        </tr>
      `);
        }
        $tbody.html(rows.join(""));
    }

    function updateCount() {
        $(SEL.countSpan).text(`(${STATE.selectionIds.size} ${I18N.Selected})`);
    }

    async function onSubmit(e) {
        e.preventDefault();
        if (STATE.submitting) return;

        if (STATE.selectionIds.size === 0) {
            warnNoSelection();
            return;
        }

        $(SEL.hiddenIds).val(Array.from(STATE.selectionIds).join(","));

        STATE.submitting = true;
        $(SEL.btnSubmit).prop("disabled", true).addClass("disabled");
        try {
            $(SEL.form)[0].submit();
        } catch (err) {
            U.showErrorMessageJs?.(err?.message || String(err));
        } finally {
            STATE.submitting = false;
            $(SEL.btnSubmit).prop("disabled", false).removeClass("disabled");
        }
    }

    function onDelete(e) {
        e.preventDefault();
        if (!STATE.deleteOptions) return;

        // expects your global openConfirmation(...) from _Confirmation.cshtml
        if (typeof window.openConfirmation === "function") {
            window.openConfirmation(STATE.deleteOptions);
        } else {
            U.showErrorMessageJs?.("Confirmation dialog not available.");
        }
    }

    function warnNoSelection() {
        if (typeof window.notif === "function") {
            window.notif({
                type: "warning",
                msg: `<b>${U.escapeHtml(I18N.Warning)}:</b> ${U.escapeHtml(I18N.NoAssetsSelected)}`,
                position: "center",
                width: 500,
                height: 60,
                autohide: false
            });
        } else {
            U.showErrorMessageJs?.(I18N.NoAssetsSelected);
        }
    }

    // Keep inline onclick="return selectAsset(assetId,this)" functional
    window.selectAsset = function (assetId, el) {
        toggleSelection(Number(assetId), !!$(el).prop("checked"));
        return true;
    };

    AMS.pages?.register?.("Transfers/EditTransfer", init);

})(jQuery, window, document);
