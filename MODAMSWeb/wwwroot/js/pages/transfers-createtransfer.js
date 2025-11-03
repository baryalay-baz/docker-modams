// /wwwroot/js/pages/transfers-createtransfer.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        form: "#frmTransfer",
        btnSubmit: "#btnSubmit",
        hiddenIds: "#selectedAssets",
        table: "#tblTransfers",
        tableBody: "#tblTransfers tbody",
        countSpan: "#spnSelection",
        listBody: "#tbodySelection",
        transferDate: "#Transfer_TransferDate"
    };

    const STATE = {
        isSo: (U.getCurrentLanguage?.() || "en") === "so",
        assets: Array.isArray(window.AMS?.pageData?.assets) ? window.AMS.pageData.assets : [],
        byId: new Map(),
        selectionIds: new Set(),
        submitting: false
    };

    // Map Model.Assets (PascalCase) -> stable shape
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

        // Build lookup & seed pre-selected
        for (const raw of STATE.assets) {
            const n = normalize(raw);
            if (!Number.isFinite(n.id)) continue;
            STATE.byId.set(n.id, n);
            if (n.isSelected) STATE.selectionIds.add(n.id);
        }

        // Widgets (idempotent)
        if ($.fn?.select2) $(".select2").select2({ width: "resolve" });
        if ($.fn?.datepicker) $(".picker," + SEL.transferDate).datepicker({
            autoclose: true, format: "dd-MM-yyyy", todayHighlight: true
        });
        U.makeDataTable?.(SEL.table, "1");

        // Render current selection & count
        renderSelectionPanel();
        updateCount();

        // Events
        bindEvents();
    }

    function bindEvents() {
        // Submit
        $(SEL.btnSubmit).on("click", onSubmit);

        // Delegated fallback (works even if you remove inline onclick later)
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
        <tr data-id="${U.escapeHtml(id)}">
          <td style="width:90px; text-align:center;">
            <img src="${U.escapeHtml(a.imageUrl)}" alt="" style="width:80px; height:auto;">
          </td>
          <td>
            <p class="mb-1"><span class="text-muted">${t("Category")}:</span> <strong>${U.escapeHtml(a.subCategory)}</strong></p>
            <p class="mb-1"><span class="text-muted">${t("AssetName")}:</span> <strong>${U.escapeHtml(a.assetName)}</strong></p>
            <p class="mb-0"><span class="text-muted">${t("SerialNo")}:</span> <strong>${U.escapeHtml(a.serialNumber)}</strong></p>
          </td>
        </tr>
      `);
        }
        $tbody.html(rows.join(""));
    }

    function updateCount() {
        $(SEL.countSpan).text(`(${STATE.selectionIds.size} ${STATE.isSo ? t("Selected_so") : t("Selected")})`);
    }

    async function onSubmit(e) {
        e.preventDefault();
        if (STATE.submitting) return;

        if (STATE.selectionIds.size === 0) {
            warnNoSelection();
            return;
        }

        // Pack comma-separated IDs for your controller
        $(SEL.hiddenIds).val(Array.from(STATE.selectionIds).join(","));

        STATE.submitting = true;
        $(SEL.btnSubmit).prop("disabled", true).addClass("disabled");
        try {
            $(SEL.form)[0].submit();
        } catch (err) {
            STATE.submitting = false;
            $(SEL.btnSubmit).prop("disabled", false).removeClass("disabled");
            U.showErrorMessageJs?.(err?.message || String(err));
        }
    }

    function warnNoSelection() {
        if (typeof window.notif === "function") {
            window.notif({
                type: "warning",
                msg: `<b>${STATE.isSo ? "Digniin" : "Warning"}:</b> ${STATE.isSo ? "Hanti lama xulin" : "No assets selected"}`,
                position: "center",
                width: 500,
                height: 60,
                autohide: false
            });
        } else {
            U.showErrorMessageJs?.(STATE.isSo ? "Hanti lama xulin." : "No assets selected.");
        }
    }

    // i18n micro-dict
    function t(key) {
        const en = {
            Category: "Category",
            AssetName: "Asset Name",
            SerialNo: "Serial No",
            Selected: "Selected",
            Selected_so: "la doortay"
        };
        return en[key] || key;
    }

    // Keep your inline onclick="selectAsset(assetId, this)" working
    window.selectAsset = function (assetId, el) {
        toggleSelection(Number(assetId), !!$(el).prop("checked"));
    };

    AMS.pages?.register?.("Transfers/CreateTransfer", init);

})(jQuery, window, document);
