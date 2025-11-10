// /wwwroot/js/pages/users/disposals/disposals-editdisposal.js
(function ($, window, document) {
    "use strict";

    // ===== Namespaces =====
    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});
    AMS.pages = AMS.pages || {};

    // ===== Config =====
    const CFG = window.EDIT_DISPOSAL || {};
    const isSomali = String(CFG.isSomali).toLowerCase() === "true";

    const SEL = {
        date: ".picker",
        form: "#frmDisposal",
        submitBtn: "#btnSubmit",
        submitPicBtn: "#btnSubmitPic",
        deleteBtn: "#btnDeleteDisposal",
        cancelBtn: "#btnCancel",

        // assets
        table: "#tblAssets",
        assetDetails: "#AssetDetails",
        hiddenAssetId: "#Disposal_AssetId",
        selectBtn: ".selectionButton",

        // detail spans
        spnMake: "#spnMake",
        spnModel: "#spnModel",
        spnAssetName: "#spnAssetName",
        spnIdentification: "#spnIdentification",

        // image / file
        imageContainer: ".image-container",
        uploadBtn: "#btnUpload-Image",
        dropZone: ".dropify",
        imageLoader: "#dvImageLoader",
        cardBodyFile: "#card-body-file", // optional if exists (Create view has it)
    };
    function init() {
        // Optional sitewide helpers if present
        (window.hideMenu || U.hideMenu || function () { })();

        initDatepicker();
        initDataTable();
        initDropify();

        wirePrimaryActions();
        wireSelectAsset();

        // If there’s a preselected asset, reflect it in UI
        reflectExistingSelection();
    }
    function initDatepicker() {
        if (!$.fn.datepicker) return;
        $(SEL.date).datepicker({
            autoclose: true,
            format: "dd-MM-yyyy",
            todayHighlight: true
        });
    }
    function initDataTable() {
        try {
            (window.makeDataTable || U.makeDataTable)(SEL.table, "1");
        } catch {
            // non-fatal
        }
    }
    function initDropify() {
        if (!$.fn.dropify) return;
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
        try { $(SEL.dropZone).dropify({ messages }); } catch { }
    }
    function wirePrimaryActions() {
        // Save (main)
        $(document).off("click.ed.save", SEL.submitBtn).on("click.ed.save", SEL.submitBtn, function (e) {
            e.preventDefault();
            const id = $(SEL.hiddenAssetId).val();
            if (!id || String(id) === "0") {
                const notify = window.notif || U.Notify;
                if (typeof notify === "function") {
                    notify({
                        type: "error",
                        msg: CFG.i18n?.noAssetErrorHtml || "<b>Error: </b>No asset selected!",
                        position: "center",
                        width: 500,
                        height: 60,
                        autohide: false
                    });
                } else {
                    alert(isSomali ? "Hanti lama dooran!" : "No asset selected!");
                }
                return;
            }
            $(SEL.form)[0]?.submit();
        });

        // Save (image area)
        $(document).off("click.ed.savepic", SEL.submitPicBtn).on("click.ed.savepic", SEL.submitPicBtn, function (e) {
            e.preventDefault();
            $(SEL.form)[0]?.submit();
        });

        // Delete
        $(document).off("click.ed.del", SEL.deleteBtn).on("click.ed.del", SEL.deleteBtn, function (e) {
            e.preventDefault();

            const options = {
                actionUrl: CFG.deleteUrl,
                title: CFG.i18n?.deleteTitle || (isSomali ? "Tirtir Khasaaraha" : "Delete Disposal"),
                message: CFG.i18n?.deleteMsg || (isSomali ? "Ma hubtaa in aad tirtirayso?" : "Are you sure you want to delete this?"),
                btnConfirmText: CFG.i18n?.confirm || (isSomali ? "Xaqiiji" : "Confirm"),
                btnCancelText: CFG.i18n?.cancel || (isSomali ? "Jooji" : "Cancel")
            };

            if (typeof window.openConfirmation === "function") {
                window.openConfirmation(options);
            } else {
                // hard fallback
                if (confirm(options.message)) {
                    // simple form post
                    const form = document.createElement("form");
                    form.method = "post";
                    form.action = options.actionUrl;
                    document.body.appendChild(form);
                    form.submit();
                }
            }
        });

        // Image loader toggles (replaces inline functions)
        $(document).on("click", "#btnUpload-Image", function (e) {
            e.preventDefault();
            showImageLoader();
        });
        $(document).on("click", '.change-picture-link', function (e) {
            e.preventDefault();
            showImageLoader();
        });
        $(document).on("click", 'a[href="#"][data-cancel-image], a.btn.btn-outline-secondary', function (e) {
            // if you add data-cancel-image attr to your cancel link, this will catch it
            e.preventDefault();
            cancelImageLoad();
        });
    }
    function showImageLoader() {
        $(SEL.imageContainer).hide();
        $(SEL.uploadBtn).hide();
        $(SEL.imageLoader).show();
    }
    function cancelImageLoad() {
        $(SEL.imageContainer).show();
        $(SEL.uploadBtn).show();
        $(SEL.imageLoader).hide();
    }
    function wireSelectAsset() {
        // Provide a stable selectAsset API used by onclick in table
        window.selectAsset = function (id, btn, e) {
            if (e && typeof e.preventDefault === "function") e.preventDefault();

            // enable all, then disable current
            $(SEL.selectBtn).prop("disabled", false);
            $(btn).prop("disabled", true);

            // Extract row values directly to avoid serializing huge arrays into the page
            const $tr = $(btn).closest("tr");
            // Columns: [selectBtn, Category, Make, Model, AssetName, Identification, Barcode]
            const make = $tr.children().eq(2).text().trim();
            const model = $tr.children().eq(3).text().trim();
            const assetName = $tr.children().eq(4).text().trim();
            const identification = $tr.children().eq(5).text().trim();

            $(SEL.spnMake).text(make);
            $(SEL.spnModel).text(model);
            $(SEL.spnAssetName).text(assetName);
            $(SEL.spnIdentification).text(identification);

            $(SEL.hiddenAssetId).val(id);

            try { $("#collapse8").collapse("hide"); } catch { }
            $(SEL.assetDetails).show();

            // Optional visual consistency with Create page
            if ($(SEL.cardBodyFile).length) {
                $(SEL.cardBodyFile).css("height", "575px");
            }
        };
    }
    function reflectExistingSelection() {
        const selectedId = String(CFG.selectedAssetId || "").trim();
        if (!selectedId || selectedId === "0") return;

        // Try to disable the corresponding "select" button row if present
        $(SEL.table).find("tbody tr").each(function () {
            const $tr = $(this);
            // heuristic: the onclick has selectAsset(assetId,...). We can read it from the button attribute.
            const $btn = $tr.find(SEL.selectBtn);
            const onclick = String($btn.attr("onclick") || "");
            // crude parse: selectAsset(123,
            const match = onclick.match(/selectAsset\((\d+)/);
            if (match && match[1] === selectedId) {
                $btn.prop("disabled", true);
                return false; // break
            }
        });

        // If details section exists but empty, leave your server-rendered values as-is
    }

    // Register page
    AMS.pages.register("Disposals/EditDisposal", init);

})(jQuery, window, document);
