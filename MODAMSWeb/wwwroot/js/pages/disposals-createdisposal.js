// /wwwroot/js/pages/disposals-createdisposal.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    const CFG = window.CREATE_DISPOSAL || {};
    const isSomali = String(CFG.isSomali).toLowerCase() === "true";

    const SEL = {
        date: ".picker",
        assetsTable: "#tblAssets",
        submitBtn: "#btnSubmit",
        form: "#frmDisposal",
        hiddenAssetId: "#Disposal_AssetId",
        cardBodyFile: "#card-body-file",
        dropifyInput: ".dropify",
        // asset details spans
        spnMake: "#spnMake",
        spnModel: "#spnModel",
        spnAssetName: "#spnAssetName",
        spnIdentification: "#spnIdentification",
        assetDetails: "#AssetDetails"
    };

    function init() {
        (window.hideMenu || U.hideMenu || function () { })();

        initDatepicker();
        // your site already loads/select2 globally for this page; no changes here
        try { (window.makeDataTable || U.makeDataTable)(SEL.assetsTable, "1"); } catch { }

        initDropify();
        bindSubmit();
        // keep global selectAsset() but also provide a safe local fallback
        if (!window.selectAsset) window.selectAsset = localSelectAsset;
    }

    function initDatepicker() {
        if (!$.fn.datepicker) return;
        $(SEL.date).datepicker({
            autoclose: true,
            format: "dd-MM-yyyy",
            todayHighlight: true
        });
        // Pre-set and then clear (exactly like your inline)
        const currentDate = new Date();
        $(SEL.date).datepicker("setDate", currentDate).val("");
    }

    function initDropify() {
        // Re-init with localized messages
        const messages = isSomali ? {
            default: "Jiid oo dhig faylka ama guji",
            replace: "Jiid oo dhig si aad u beddesho faylka",
            remove: "Tirtir",
            error: "Khalad ayaa dhacay"
        } : {
            default: "Drag and drop a file or click",
            replace: "Drag and drop or click to replace",
            remove: "Remove",
            error: "Oops, something wrong happened."
        };

        try {
            $(SEL.dropifyInput).dropify({ messages });
        } catch { /* dropify optional */ }
    }

    function bindSubmit() {
        $(document).off("click.cd", SEL.submitBtn).on("click.cd", SEL.submitBtn, function (e) {
            e.preventDefault();
            const id = $(SEL.hiddenAssetId).val();
            if (!id || String(id) === "0") {
                const notifFn = window.notif || U.Notify;
                if (typeof notifFn === "function") {
                    notifFn({
                        type: "error",
                        msg: CFG.i18n?.noAssetErrorHtml || "<b>Error: </b>No asset selected!",
                        position: "center",
                        width: 500,
                        height: 60,
                        autohide: false
                    });
                } else {
                    alert(isSomali ? "Hanti lama dooran" : "No asset selected!");
                }
                return;
            }
            $(SEL.form)[0]?.submit();
        });
    }

    // Fallback for select button onclick in the table
    function localSelectAsset(id, btn, e) {
        if (e && typeof e.preventDefault === "function") e.preventDefault();

        // Enable all, then disable current
        $(".selectionButton").prop("disabled", false);
        $(btn).prop("disabled", true);

        // assets array is not directly serialized on page; we read row values instead (stable)
        // Row structure: [selectBtn, Category, Make, Model, AssetName, Identification, Barcode]
        const $tr = $(btn).closest("tr");
        const make = $tr.children().eq(2).text().trim();
        const model = $tr.children().eq(3).text().trim();
        const assetName = $tr.children().eq(4).text().trim();
        const identification = $tr.children().eq(5).text().trim();

        $(SEL.spnMake).text(make);
        $(SEL.spnModel).text(model);
        $(SEL.spnAssetName).text(assetName);
        $(SEL.spnIdentification).text(identification);

        $(SEL.hiddenAssetId).val(id);

        // collapse and reveal details
        try { $("#collapse8").collapse("hide"); } catch { }
        $(SEL.assetDetails).show();

        // increase image panel height like your inline code
        $(SEL.cardBodyFile).css("height", "575px");
    }

    // Page registry (compatible with your site pattern)
    AMS.pages = AMS.pages || {};
    AMS.pages.register = AMS.pages.register || function (key, fn) {
        document.addEventListener("DOMContentLoaded", fn);
    };
    AMS.pages.register("Disposals/CreateDisposal", init);

})(jQuery, window, document);
