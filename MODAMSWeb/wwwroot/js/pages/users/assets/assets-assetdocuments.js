// /wwwroot/js/pages/users/assets/assets-assetdocuments.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        tblDocuments: "#tblDocuments",

        cardUpload: "#cardUpload",
        btnCancel: "#cancelbtn",
        fileInput: "#file",

        docTypeIdHidden: "#DocumentTypeId",
        lblDocumentName: "#lblDocumentName",

        docTypesDataDiv: "#dvDocumentTypesData"
    };

    function getCFG() {
        return window.AMS_ASSET_DOCS || {};
    }

    function getLanguage() {
        if (typeof U.getCurrentLanguage === "function") {
            return U.getCurrentLanguage();
        }
        if (typeof window.getCurrentLanguage === "function") {
            return window.getCurrentLanguage();
        }
        return "en";
    }

    // ============= INIT =============
    async function init(/*ctx*/) {
        U.hideMenu?.();

        await loadDocumentTypes();

        // make sure upload card starts in "locked" state
        setUploadCardDisabled(true);

        // cancel button inside upload card
        $(SEL.btnCancel).on("click", function (e) {
            e.preventDefault();

            // clear file input
            $(SEL.fileInput).val("");

            // reset hidden doc type + label too if you want it blank:
            $(SEL.docTypeIdHidden).val("0");
            $(SEL.lblDocumentName).html("");

            // go back to disabled/locked state
            setUploadCardDisabled(true);
        });

        // init DataTable for the docs table
        if (typeof U.makeDataTable === "function") {
            U.makeDataTable(SEL.tblDocuments, "1");
        } else if (typeof window.makeDataTable === "function") {
            window.makeDataTable(SEL.tblDocuments, "1");
        } else {
            console.warn("[AMS] makeDataTable not found for", SEL.tblDocuments);
        }
    }

    // ============= LOAD DOCUMENT TYPES =============
    async function loadDocumentTypes() {
        try {
            const response = await U.fetchJson("/Users/Assets/GetDocumentTypes");

            if (response.success) {
                $(SEL.docTypesDataDiv).html(response.data);
            } else {
                U.showErrorMessageJs?.(response.message || "Unable to load document types.");
            }
        } catch (err) {
            console.error("[AMS] loadDocumentTypes error:", err);
            U.showErrorMessageJs?.(
                "Error loading document types: " + (err && err.message ? err.message : err)
            );
        }
    }

    // ============= UPLOAD CARD STATE TOGGLER =============
    function setUploadCardDisabled(disabled) {
        const $card = $(SEL.cardUpload);
        if (disabled) {
            $card
                .removeClass("uploadcard-active")
                .addClass("uploadcard-disabled");
        } else {
            $card
                .removeClass("uploadcard-disabled")
                .addClass("uploadcard-active");
        }
    }

    // ============= SHOW UPLOAD CARD FOR A GIVEN ROW TYPE =============
    // called from onclick="showCardUpload(@dt.DocumentTypeId)"
    function showCardUpload(documentTypeId) {
        // visually "unlock" the panel
        setUploadCardDisabled(false);

        // update the labels / hidden field for this doc type
        setDocumentNameForUpload(documentTypeId);
    }

    // ============= DELETE DOCUMENT FLOW =============
    function deleteDocument(id) {
        const cfg = getCFG();

        const options = {
            actionUrl: `/Users/Assets/DeleteDocument/${id}`,
            title: cfg.deleteTitle || "Delete Document",
            message: cfg.deleteMessage || "Are you sure you want to delete this document?",
            btnConfirmText: cfg.btnConfirmText || "Confirm",
            btnCancelText: cfg.btnCancelText || "Cancel"
        };

        window.openConfirmation?.(options);
    }

    // ============= SYNC UPLOAD PANEL WITH DOC TYPE =============
    function setDocumentNameForUpload(documentTypeId) {
        const rawHtml = $(SEL.docTypesDataDiv).html();
        const langIsSomali = getLanguage() === "so";

        const result = (window.tryParseJson
            ? window.tryParseJson(rawHtml, "Document Types")
            : fallbackTryParseJson(rawHtml, "Document Types")
        );

        if (result.status === "success") {
            const list = result.data;
            list.forEach((item) => {
                if (String(item.Id) === String(documentTypeId)) {
                    // set hidden input
                    $(SEL.docTypeIdHidden).val(item.Id);

                    // set label user sees
                    $(SEL.lblDocumentName).html(
                        langIsSomali ? item.NameSo : item.Name
                    );
                }
            });
        } else {
            U.showErrorMessageJs?.(result.message || "Invalid document types list.");
        }
    }

    // fallback if utils.tryParseJson isn't around
    function fallbackTryParseJson(text, identifier) {
        try {
            return { status: "success", data: JSON.parse(text) };
        } catch (err) {
            return {
                status: "error",
                message: `${identifier} data is not in valid JSON format.`
            };
        }
    }

    // expose for Razor inline onclick handlers
    window.showCardUpload = showCardUpload;
    window.deleteDocument = deleteDocument;

    // register page init with page registry / tour
    AMS.pages?.register?.("Assets/AssetDocuments", init);

})(jQuery, window, document);
