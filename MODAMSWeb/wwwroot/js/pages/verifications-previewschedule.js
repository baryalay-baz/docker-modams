// /wwwroot/js/pages/verifications-previewschedule.js
// Baz • 2025-10-30 (aligned with utils.js)

(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const PAGES = AMS.pages || (AMS.pages = { register() { }, start() { } });

    const SEL = {
        verifyBtn: "#btnVerify",
        verifyAssetBtn: "#btnVerifyAsset",
        completeBtn: "#btnComplete",
        completeForm: "#frmComplete",
        modalVerify: "#mdlVerifyAssets",
        modalVerified: "#mdlVerified",
        overlay: "#verificationOverlay",
        assetsTable: "#tblAssets",
        assetsTBody: "#table-body",
        verifiedTable: "#tblVerifiedAssets",
        verifiedTBody: "#table-body-verified",
        sectionAssetsData: "#dvAssetsData",
        assetName: "#AssetName",
        assetIdHidden: "#VerificationRecord_AssetId",
        verifyForm: "#frmVerify",
        progressChart: "#progressChart",
        verificationChart: "#verificationChart",
        numberToVerify: "#txtNumberOfAssetsToVerify",
        lblTitle: "#lblTitle",
        modalNotice: ".modal-notification"
    };
    const RESULT = "#VerificationRecord_Result";
    const FILE = "#file";
    const PHOTO_ROW = "#rowPhoto";

    const isSomali = (U.getCurrentLanguage?.() || "en") === "so";

    const localizedResult = {
        "Verified (In Good Condition)": "La Xaqiijiyay (Xaalad Wanaagsan)",
        "Verified (With Issues)": "La Xaqiijiyay (Dhibaatooyin Jira)",
        "Verified (Out of Service)": "La Xaqiijiyay (Ka Baxsan Adeeg)",
        "Verified (Damaged)": "La Xaqiijiyay (Waxyeello Leh)",
        "Not Verified (Missing)": "Lama Xaqiijin (Maqan)"
    };
    const unLocalizedResult = Object.fromEntries(
        Object.entries(localizedResult).map(([en, so]) => [so, en])
    );

    let currentPage_tblAssets = 0;

    // ---------- helpers ----------
    function showOverlay(on) { const el = document.querySelector(SEL.overlay); if (el) el.style.display = on ? "block" : "none"; }
    function tryParse(raw, label) { return U.tryParseJson ? U.tryParseJson(raw, label) : (function () { try { return { status: "success", data: JSON.parse(raw) }; } catch { return { status: "error", message: `${label} invalid JSON` }; } })(); }
    function makeDT(sel, type = "1", len = 10) { return U.makeDataTable ? U.makeDataTable(sel, type, len) : null; }
    function notifyInModal(type, msg) {
        const $c = $(SEL.modalNotice);
        if ($c.length) {
            const safe = U.escapeHtml ? U.escapeHtml(msg) : msg;
            const ok = type === "success";
            const title = ok ? (isSomali ? "Fariinta Guusha" : "Success Message")
                : (isSomali ? "Fariinta Khaladka" : "Error Message");
            const cls = ok ? "success" : "danger";
            const html = `
        <div class="notification-container">
          <div class="alert alert-${cls} alert-dismissible fade show p-0 mb-4" role="alert">
            <p class="py-3 px-5 mb-0 border-bottom border-bottom-${cls}-light">
              <span class="alert-inner--icon me-2"><i class="fe ${ok ? "fe-thumbs-up" : "fe-slash"}"></i></span>
              <strong>${title}</strong>
            </p>
            <p class="py-3 px-5">${safe}</p>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
              <span aria-hidden="true">×</span>
            </button>
          </div>
        </div>`;
            $c.html(html);
            return;
        }
        // fallback to global
        (ok => ok ? U.showSuccessMessageJs?.(msg) : U.showErrorMessageJs?.(msg))(type === "success");
    }
    function openBsModal(selector) {
        const el = document.querySelector(selector);
        if (!el || !window.bootstrap) return null;
        const m = bootstrap.Modal.getOrCreateInstance(el, { keyboard: false, backdrop: "static" });
        m.show();
        return m;
    }
    function isMissingSelected() {
        // Value is EN string even when UI text is localized (good!)
        return $(RESULT).val() === "Not Verified (Missing)";
    }
    function togglePhotoRequirement() {
        const isMissing = isMissingSelected();
        const $file = $(FILE);

        if (isMissing) {
            // not required, disable & hide
            $file.prop("required", false).prop("disabled", true);
            $("#spnFile").text("");

            // Clear Dropify preview/file if present
            const dr = $file.data("dropify");
            if (dr) {
                try { dr.clearElement(); } catch { $file.val(""); }
            } else {
                $file.val("");
            }

            $(PHOTO_ROW).hide();
        } else {
            // required, enable & show
            $file.prop("disabled", false).prop("required", true);
            $(PHOTO_ROW).show();
        }
    }



    // ---------- charts ----------
    function loadBarchart(data) {
        if (!window.echarts) return;
        const el = document.querySelector(SEL.verificationChart); if (!el) return;

        const results = (data || []).map(x => isSomali ? (localizedResult[x.result] || x.result) : x.result);
        const counts = (data || []).map(x => x.verificationRecordCount);
        const palette = ["#A5D6A7", "#90CAF9", "#F48FB1", "#FFCC80", "#CE93D8", "#FFF59D", "#80CBC4", "#FFAB91", "#81D4FA", "#B39DDB", "#FFE082", "#FFCDD2"];

        const chart = echarts.init(el);
        chart.setOption({
            tooltip: { trigger: "axis", axisPointer: { type: "shadow" } },
            grid: { left: "30%" },
            xAxis: { type: "value", minInterval: 1 },
            yAxis: { type: "category", data: results, axisLabel: { interval: 0, fontSize: 12 } },
            series: [{
                type: "bar",
                data: counts.map((c, i) => ({ value: c, itemStyle: { color: palette[i % palette.length] } })),
                label: { show: true, position: "right", formatter: "{c}" }
            }]
        });
        chart.on("click", p => handleBarClick(p.name));
        window.addEventListener("resize", () => chart.resize());
    }
    function toNum(v) { const n = Number(v); return Number.isFinite(n) ? n : null; }
    function loadProgressChart(data) {
        if (!window.echarts) return;
        const el = document.querySelector("#progressChart"); if (!el) return;

        const rows = (data || []).slice().sort((a, b) => new Date(a.formattedDate) - new Date(b.formattedDate));
        const dates = rows.map(r => r.formattedDate);
        const planned = rows.map(r => toNum(r.planProgress));
        const achieved = rows.map(r => toNum(r.progress));

        const isSo = (U.getCurrentLanguage?.() || "en") === "so";
        const title = isSo ? "Horumarka La Qorsheeyay vs. Horumarka La Gaadhay" : "Planned vs. Achieved Progress";
        const pLabel = isSo ? "Horumarka La Qorsheeyay" : "Planned Progress";
        const aLabel = isSo ? "Horumarka La Gaadhay" : "Achieved Progress";

        const chart = echarts.init(el);
        chart.setOption({
            title: { text: title },
            tooltip: { trigger: "axis" },
            legend: { data: [pLabel, aLabel] },
            grid: { left: 50, right: 20, top: 40, bottom: 40 },
            xAxis: { type: "category", boundaryGap: false, data: dates },
            yAxis: { type: "value", min: 0 },
            series: [
                { name: pLabel, type: "line", data: planned, smooth: true, connectNulls: true, showSymbol: false, lineStyle: { width: 2 } },
                {
                    name: aLabel, type: "line", data: achieved, smooth: true, connectNulls: true, showSymbol: true, lineStyle: { width: 2 },
                    label: { show: true, position: "top", formatter: (p) => (p.data == null ? "" : p.data) }
                }
            ]
        });
        window.addEventListener("resize", () => chart.resize());
    }
    function handleBarClick(clickedLabel) {
        const key = isSomali ? (unLocalizedResult[clickedLabel] || clickedLabel) : clickedLabel;
        const raw = document.querySelector(SEL.sectionAssetsData)?.textContent || "[]";
        const parsed = tryParse(raw, "Assets Data"); if (parsed.status !== "success") { U.showErrorMessageJs?.(parsed.message); return; }
        openBsModal(SEL.modalVerified);
        loadVerifiedAssetsTable(key, parsed.data);
    }
    async function refreshPreviewDataJS() {
        const scheduleId = String(window.VERIF_PREVIEW_CONFIG?.scheduleId || $("#VerificationRecord_VerificationScheduleId").val() || "").trim();
        if (!scheduleId) return;

        try {
            const bar = await U.fetchJson(`/Users/Verifications/GetBarChartData?id=${encodeURIComponent(scheduleId)}`);
            if (bar?.success && Array.isArray(bar.data)) {
                loadBarchart(bar.data); // re-render bar chart
            }

            const prog = await U.fetchJson(`/Users/Verifications/GetProgressData?id=${encodeURIComponent(scheduleId)}`);
            U.log("[DEBUG] progress payload:", prog);

            if (prog?.success && Array.isArray(prog.data)) {
                loadProgressChart(prog.data); // re-render progress chart
            }
        } catch (e) {
            console.warn("[AMS] refreshPreviewDataJS:", e);
        }
    }
    function wireModalCloseRefresh() {
        const el = document.querySelector("#mdlVerifyAssets");
        if (!el || !window.bootstrap) return;
        el.addEventListener("hidden.bs.modal", () => {
            // Re-pull assets list + charts to reflect recent verification(s)
            loadAssetsData();         // existing function you already have
            refreshPreviewDataJS();   // new function above
        });
    }
    function loadVerifiedAssetsTable(resultKey, allAssets) {
        const filtered = (allAssets || []).filter(e => e.result === resultKey && e.verifiedBy !== "Not Verified");
        let html = "", n = 0;
        for (const e of filtered) {
            n++;
            const badge = `<i class="fa fa-check-circle" style="font-size:1rem;color:green;"></i> ${e.verifiedBy}`;
            html += `
        <tr>
          <td class="text-black w-2">${n}</td>
          <td class="text-black w-10">${e.make || ""}</td>
          <td class="text-black w-10">${e.model || ""}</td>
          <td class="text-black w-10">${e.name || ""}</td>
          <td class="text-black w-10">${e.serialNo || ""}</td>
          <td class="text-black w-10">${badge}</td>
        </tr>`;
        }
        if ($.fn.DataTable && $.fn.DataTable.isDataTable(SEL.verifiedTable)) {
            $(SEL.verifiedTable).DataTable().clear().destroy();
        }
        $(SEL.verifiedTBody).html(html);
        makeDT(SEL.verifiedTable, "1");
        $(SEL.lblTitle).text(isSomali ? (localizedResult[resultKey] || resultKey) : resultKey);
    }

    // ---------- assets modal load/render ----------
    function showVerifyModal() { openBsModal(SEL.modalVerify); }
    function loadAssetsData() {
        const scheduleId = $("#VerificationRecord_VerificationScheduleId").val();
        showOverlay(true);
        $.ajax({
            url: "/Users/Verifications/GetScheduleAssets",
            data: { id: scheduleId },
            datatype: "json"
        })
            .done((res) => {
                if (res?.success) loadAssets(JSON.stringify(res.assets));
                else notifyInModal("error", "Error: " + (res?.message || "Unknown"));
            })
            .fail((xhr, status, error) => {
                let msg = `Error ${xhr.status} ${status}: ${error}`;
                if (xhr.responseText) msg += `\n${xhr.responseText}`;
                notifyInModal("error", msg);
            })
            .always(() => showOverlay(false));
    }
    function loadAssets(json) {
        const Data = JSON.parse(json || "[]");
        let html = "", verifiedCount = 0;

        if ($.fn.DataTable && $.fn.DataTable.isDataTable(SEL.assetsTable)) {
            $(SEL.assetsTable).DataTable().clear().destroy();
        }

        for (const e of Data) {
            let cell;
            if (e.isSelected) {
                cell = `<i class="fa fa-check-circle text-success delete-icon" id="icon-${e.id}"
                   style="font-size:1rem;" data-id="${e.verificationRecordId}" data-assetid="${e.id}"></i> ${isSomali ? "La Xaqiijiyay" : "Verified"}`;
                verifiedCount++;
            } else {
                cell = `<button class="btn btn-outline-info btn-sm" onclick="return AMS.pages.__verPrev.selectRecord(${e.id});">${isSomali ? "Dooro" : "Select"}</button>`;
            }
            html += `
        <tr id="tr-${e.id}">
          <td id="td-${e.id}" class="text-black w-2">${cell}</td>
          <td class="text-black w-10">${e.make || ""}</td>
          <td class="text-black w-10">${e.model || ""}</td>
          <td class="text-black w-30">${e.name || ""}</td>
          <td class="text-black w-10">${e.serialNo || ""}</td>
        </tr>`;
        }

        $(SEL.assetsTBody).html(html);
        bindHoverAndClickEvents();

        const dt = makeDT(SEL.assetsTable, "1", 5);
        if (dt && typeof dt.page === "function") dt.page(currentPage_tblAssets).draw(false);

        const toVerify = Number($(SEL.numberToVerify).val() || 0);
        if (toVerify === verifiedCount) $(SEL.verifyAssetBtn).addClass("disabled");
        else $(SEL.verifyAssetBtn).removeClass("disabled");
    }

    // expose for inline onclick
    AMS.pages.__verPrev = AMS.pages.__verPrev || {};
    AMS.pages.__verPrev.selectRecord = function (id) {
        $("#tblAssets tbody tr").removeClass("bg-light-transparent text-bold");
        const tr = `#tr-${id}`;
        $(tr).addClass("bg-light-transparent text-bold");
        const assetName = $(tr).find("td:nth-child(4)").text() + " - " + $(tr).find("td:nth-child(5)").text();
        $(SEL.assetName).val(assetName);
        $(SEL.assetIdHidden).val("0").val(id);
        $("#tempAssetId").val("0").val(id);
        $("#spnAssetId").text("");
        return false;
    };

    function bindHoverAndClickEvents() {
        $(".delete-icon").off("mouseenter mouseleave click");
        $(".delete-icon").hover(
            function () { $(this).removeClass("fa-check-circle text-success").addClass("fa-trash text-secondary").css({ cursor: "pointer" }); },
            function () { $(this).removeClass("fa-trash text-secondary").addClass("fa-check-circle text-success").css({ color: "green", cursor: "default" }); }
        );
        $(".delete-icon").on("click", function () {
            const verificationRecordId = $(this).data("id");
            const assetId = $(this).data("assetid");
            const q = isSomali ? "Ma hubtaa inaad tirtirto rikoodhka xaqiijinta?" : "Are you sure you want to delete this Verification Record?";
            if (confirm(q)) deleteVerificationRecord(verificationRecordId, assetId);
        });
    }
    function deleteVerificationRecord(verificationRecordId, assetId) {
        const tdSel = `#td-${assetId}`;
        const btn = `<button class="btn btn-outline-info btn-sm" onclick="return AMS.pages.__verPrev.selectRecord(${assetId});">${isSomali ? "Dooro" : "Select"}</button>`;

        showOverlay(true);
        $.ajax({
            url: "/Users/Verifications/DeleteVerificationRecord",
            type: "POST",
            data: { id: verificationRecordId }
        })
            .done((res) => {
                if (res?.success) {
                    $(tdSel).html(btn);
                    notifyInModal("success", isSomali ? "Rikoodhka si guul leh ayaa loo tirtiray." : "Verification Record deleted successfully.");
                } else {
                    notifyInModal("error", "Failed to delete record: " + (res?.message || "Unknown"));
                }
            })
            .fail((xhr, status, error) => {
                let msg = `Error ${xhr.status} ${status}: ${error}`;
                if (xhr.responseText) msg += `\n${xhr.responseText}`;
                notifyInModal("error", msg);
            })
            .always(() => {
                showOverlay(false);
                $(SEL.verifyAssetBtn).removeClass("disabled");
            });
    }

    // ---------- form ----------
    function isFormValid() {
        let ok = true;
        $("span.text-danger").text("");

        if ($(SEL.assetIdHidden).val() == "0") {
            $("#spnAssetId").text(isSomali ? "Fadlan dooro hanti!" : "Please select an asset to verify!");
            ok = false;
        }
        if ($("#VerificationRecord_Result").val() === "-1" || $("#VerificationRecord_Result").val() === null) {
            $("#spnResult").text(isSomali ? "Fadlan dooro natiijo!" : "Please select a result option!");
            ok = false;
        }
        if ($("#VerificationRecord_Comments").val().trim() === "") {
            $("#spnComments").text(isSomali ? "Fadlan geli faallo!" : "Please provide verification comments!");
            ok = false;
        }
        if (!isMissingSelected() && $(FILE).val() === "") {
            $("#spnFile").text(isSomali ? "Fadlan soo geli sawir cusub!" : "Please upload a recent picture of the asset!");
            ok = false;
        } else {
            $("#spnFile").text("");
        }
        return ok;
    }
    function submitVerificationForm() {
        const $form = $(SEL.verifyForm);
        const fd = new FormData($form[0]);

        if ($.fn.DataTable && $.fn.DataTable.isDataTable(SEL.assetsTable)) {
            currentPage_tblAssets = $(SEL.assetsTable).DataTable().page();
        }

        $.ajax({
            url: "/Users/Verifications/VerifyAsset",
            type: "POST",
            data: fd,
            contentType: false,
            processData: false,
            beforeSend: function () { showOverlay(true); }
        })
            .done(resp => {
                if (resp?.success) notifyInModal("success", resp.message || (isSomali ? "Xaqiijin waa lagu guulaystay." : "Verification succeeded."));
                else notifyInModal("error", "Verification failed: " + (resp?.message || "Unknown"));
            })
            .fail((xhr, status, error) => {
                if (xhr.status === 403) notifyInModal("error", isSomali ? "Oggolaansho ma lihid." : "You do not have permission to perform this action.");
                else {
                    let msg = `Error ${xhr.status} ${status}: ${error}`;
                    if (xhr.responseText) msg += `\n${xhr.responseText}`;
                    notifyInModal("error", msg);
                }
            })
            .always(() => {
                showOverlay(false);
                // refresh modal table & reset form
                loadAssetsData();
                clearForm();
            });
    }
    function clearForm() {
        $(SEL.assetName).val("-select an asset-");
        $("#VerificationRecord_Result").val(null).trigger("change");
        $("textarea[asp-for='VerificationRecord.Comments']").val("");
        $(".validation").text("");
        $(SEL.verifyForm)[0].reset();

        togglePhotoRequirement();
    }

    // ---------- events ----------
    function bindEvents() {
        $(document).off("click.verPrev.verify", SEL.verifyBtn)
            .on("click.verPrev.verify", SEL.verifyBtn, function () {
                loadAssetsData(); showVerifyModal();
            });

        $(document).off("click.verPrev.submit", SEL.verifyAssetBtn)
            .on("click.verPrev.submit", SEL.verifyAssetBtn, function (e) {
                e.preventDefault();
                if (isFormValid()) submitVerificationForm();
                else notifyInModal("error", isSomali ? "Fadlan sax khaladaadka!" : "Please correct the validation errors!");
            });

        $(document).off("click.verPrev.complete", SEL.completeBtn)
            .on("click.verPrev.complete", SEL.completeBtn, function (e) {
                e.preventDefault();
                $(SEL.completeForm).trigger("submit");
            });
        $(document).off("change.verPrev.result", RESULT)
            .on("change.verPrev.result", RESULT, togglePhotoRequirement);
    }

    // ---------- bootstrap UI ----------
    function initBootstrapUI() {
        if (!window.bootstrap) return;
        document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => {
            try { new bootstrap.Popover(el, { trigger: "hover", html: true, container: "body" }); } catch { }
        });
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
            try { new bootstrap.Tooltip(el); } catch { }
        });
    }

    // ---------- entry ----------
    function init() {
        try { U.hideMenu && U.hideMenu(); } catch { }

        const tH = $("textarea.form-control").outerHeight();
        if (tH) $(".avatar-list").outerHeight(tH);

        // charts via config blob from view
        const barData = window.VERIF_PREVIEW_CONFIG?.barChart;
        const progData = window.VERIF_PREVIEW_CONFIG?.progressChart;
        if (barData) loadBarchart(barData);
        if (progData) loadProgressChart(progData);

        wireModalCloseRefresh();

        initBootstrapUI();
        bindEvents();
        togglePhotoRequirement();
    }

    PAGES.register && PAGES.register("Verifications/PreviewSchedule", init);

})(jQuery, window, document);
