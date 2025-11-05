// ~/js/pages/transfers-preview.js
(function () {
    const $ = window.jQuery;

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    function init() {
        if (!U) { console.error("[AMS] utils not loaded."); return; }
        U.hideMenu();
        U.makeDataTable("#tblAssets", 1);
    }


    // Helper: submit a form programmatically and then lock the button
    function submitThenLock(formSel, btnSel) {
        const $form = $(formSel);
        const $btn = $(btnSel);
        if ($form.length === 0 || $btn.length === 0) return;

        $form[0].submit();

        // Lock UI after submission has been kicked off
        setTimeout(() => {
            $btn.addClass("btn-loading btn-icon disabled").prop("disabled", true);
        }, 0);
    }

    // Submit for Acknowledgement
    $(document).on("click", "#btnSubmitTransfer", function (e) {
        e.preventDefault();
        submitThenLock("#frmSubmitForAcknowledgement", "#btnSubmitTransfer");
    });

    // Acknowledge
    $(document).on("click", "#btnAcknowledgeTransfer", function (e) {
        e.preventDefault();
        submitThenLock("#frmAcknowledgeTransfer", "#btnAcknowledgeTransfer");
    });

    // Reject
    $(document).on("click", "#btnRejectTransfer", function (e) {
        e.preventDefault();
        submitThenLock("#frmRejectTransfer", "#btnRejectTransfer");
    });

    $(document).on("click", "#btnPrintVoucher", function (e) {
        e.preventDefault();

        const w = screen.availWidth, h = screen.availHeight;
        const printWin = window.open(
            "",
            "PrintWindow",
            `toolbar=no,menubar=no,scrollbars=no,resizable=no,location=no,status=no,width=${w},height=${h}`
        );

        if (printWin) {
            try { printWin.moveTo(0, 0); } catch { /* ignore */ }
            const $form = $("#frmPrintVoucher");
            $form.attr("target", "PrintWindow");
            $form[0].submit();
        } else {
            // Popup blocked: submit in the same tab
            $("#frmPrintVoucher")[0].submit();
        }
    });


    const txt = ($("#spnAction").text() || "").trim().toLowerCase();
    if (txt.includes("no action")) {
        // e.g., $("#dvAction").addClass("opacity-75");
    }

    AMS.pages?.register?.("Transfers/PreviewTransfer", init);

})();
