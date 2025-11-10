// /wwwroot/js/pages/users/assets/assets-assetpictures.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        fileInput: "#file"
    };

    function getCFG() {
        return window.AMS_ASSET_PICS || {};
    }

    // ============= INIT =============
    function init(/*ctx*/) {
        U.hideMenu?.();
    }

    // ============= DELETE PICTURE CONFIRMATION =============
    function deletePicture(id, assetId) {
        const cfg = getCFG();

        const options = {
            actionUrl: `/Users/Assets/DeletePicture/${id}`,
            title: cfg.deleteTitle || "Delete Picture",
            message: cfg.deleteMessage || "Are you sure you want to delete this picture?",
            btnConfirmText: cfg.btnConfirmText || "Confirm",
            btnCancelText: cfg.btnCancelText || "Cancel",

            data: `<input type="hidden" name="assetId" value="${assetId}" />`
        };

        window.openConfirmation?.(options);
    }

    window.deletePicture = deletePicture;

    AMS.pages?.register?.("Assets/AssetPictures", init);

})(jQuery, window, document);
