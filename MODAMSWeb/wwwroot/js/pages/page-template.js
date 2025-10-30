// /wwwroot/js/pages/<page>.js
(function (w, d) {
    "use strict";

    const AMS = w.AMS || (w.AMS = {});
    const U = AMS.util || (AMS.util = {});

    const SEL = {
        btnVerify: "#btnVerify",
        table: "#tblAssets",
    };

    const STATE = {
        loaded: false,
    };

    function onReady() {
        bind();
        initTable();
    }

    function bind() {
        const NS = ".vps";
        d.removeEventListener("click" + NS, noop); // pattern example
        d.addEventListener("click", function (e) {
            const t = e.target;
            if (t.matches(SEL.btnVerify)) {
                e.preventDefault();
                openVerify();
            }
        });
    }

    function initTable() {
        // your DataTable init here (guard if not present)
    }

    function openVerify() {
        // open modal, fetch data, render
    }

    function noop() { }

    // run
    if (d.readyState === "loading") d.addEventListener("DOMContentLoaded", onReady, { once: true });
    else onReady();

})(window, document);
