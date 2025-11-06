// /wwwroot/js/pages/transfers-receivedassets.js
(function ($, window, document) {
    "use strict";

    const AMS = window.AMS || (window.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        table: "#tblTransfers",
        wrap: "#dvTable-Responsive"
    };

    function init() {
        U.hideMenu?.();
        initTable();
    }

    function initTable() {
        U.makeDataTable(SEL.table, "2", 10, {
            enable: true,
            paddingPx: 140
            // No row actions here. Add later if needed.
        });

        // Keep the responsive container sane
        const $wrap = $(SEL.wrap);
        if ($wrap.length) $wrap.css("overflow", "auto");
    }

    AMS.pages?.register?.("Transfers/ReceivedAssets", init);

})(jQuery, window, document);
