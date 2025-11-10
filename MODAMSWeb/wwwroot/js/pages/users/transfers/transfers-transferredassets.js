// /wwwroot/js/pages/users/transfers/transfers-transferredassets.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        table: "#tblTransfers"
    };

    function init() {
        U.hideMenu?.();

        initTable();
    }

    function initTable() {
        // Column “2” paging preset like your other pages; page length 10; padding for DT controls
        U.makeDataTable(SEL.table, "2", 10, {
            enable: true,
            paddingPx: 140
        });

        // Optional: make the table responsive container keep height sane on small screens
        const $wrap = $("#dvTable-Responsive");
        if ($wrap.length) $wrap.css("overflow", "auto");
    }

    AMS.pages?.register?.("Transfers/TransferredAssets", init);

})(jQuery, window, document);
