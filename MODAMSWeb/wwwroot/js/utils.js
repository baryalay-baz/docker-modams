// utils.js (classic script, NOT type="module")
// PAMS global utilities — namespaced + backward compatible

(function (w, $) {
    "use strict";

    // Ensure jQuery exists for jQuery-dependent helpers
    const hasJQ = !!$;

    // Namespace
    const P = w.PAMS = w.PAMS || {};
    const U = P.util = P.util || {};

    U.ready = function (fn) {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", fn, { once: true });
        } else {
            fn();
        }
    };

    U.pageKey = function () {
        // try primary key, then your fallback attr
        return (
            document.body.getAttribute("data-page") ||
            document.body.getAttribute("data-page-fallback") ||
            ""
        );
    };

    // ----- Bridge access (safe alias) -----
    U.bridge = function () {
        return (window.PAMS && window.PAMS.bridge) || {};
    };
       
    // (Optional) One-liner for CSRF headers (if you like calling per-request)
    U.csrfHeader = function () {
        const b = U.bridge();
        const token =
            b.antiForgery?.token ||
            document.querySelector('meta[name="request-verification-token"]')?.content ||
            "";
        const name = b.antiForgery?.header || "RequestVerificationToken";
        return token ? { [name]: token } : {};
    };
    U.debounce = function (fn, wait = 150) {
        let t = null;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), wait);
        };
    };

    // Logger (centralized)
    U.log = function (...args) { console.log("[PAMS]", ...args); };

    // ---------- JSON ----------
    U.tryParseJson = function (jsonString, identifier = "JSON") {
        try {
            return { status: "success", data: JSON.parse(jsonString) };
        } catch (error) {
            return { status: "error", message: `${identifier} data is not in valid JSON format.` };
        }
    };

    // ---------- Tables ----------
    U.formatTables = function () {
        if (!hasJQ) return;
        $("thead").addClass("bg-info-gradient ms-auto divShadow");
        $("th").addClass("text-white");
    };

    // ---------- Theme Mode ----------
    U.setMode = function () {
        // single source of truth: displayMode = "dark-mode" | "light-mode"
        const isDark = localStorage.getItem("displayMode") === "dark-mode";

        document.body.classList.toggle("dark-mode", isDark);
        document.body.classList.toggle("light-mode", !isDark);

        const footerImg = isDark
            ? "/assets/images/brand/EU_Horizontal3_small.png"
            : "/assets/images/brand/EU_Horizontal2_small.png";

        const footer = document.getElementById("footerImgEU");
        if (footer) footer.innerHTML = `<img src="${footerImg}">`;

        // Keep only one key that matters
        localStorage.setItem("displayMode", isDark ? "dark-mode" : "light-mode");
    };

    U._getCurrentLanguageCookie = function () {
        const cookieName = ".AspNetCore.Culture=";
        const part = document.cookie.split("; ").find(row => row.startsWith(cookieName));
        if (!part) return "en";
        const decoded = decodeURIComponent(part.slice(cookieName.length));
        const m = decoded.match(/(?:c|uic)=([A-Za-z-]+)/);
        return m ? m[1].split("-")[0] : "en";
    };

    // bridge-first helper
    U.lang = function () {
        const b = U.bridge();
        return b.cultureTwoLetter || U._getCurrentLanguageCookie() || "en";
    };
        
    // ---------- Language ----------
    U.getCurrentLanguage = U.lang;


    // ---------- DataTables Language ----------
    U.getDataTableLanguageOptions = function (languageCode) {
        if (languageCode === "so") {
            return {
                processing: "Fadlan sug...",
                lengthMenu: "_MENU_",
                zeroRecords: "Ma jiro wax rikoor ah oo la helay",
                info: "Muujinaya _START_ ilaa _END_ ee _TOTAL_ rikoorrada",
                infoEmpty: "Muujinaya 0 ilaa 0 ee 0 rikoorrada",
                infoFiltered: "(la sifeynayo _MAX_ rikoorrada)",
                emptyTable: "Ma jiraan wax xog ah oo la heli karo",
                loadingRecords: "Loading...",
                infoThousands: ",",
                search: "",
                searchPlaceholder: "Raadi...",
                paginate: {
                    first: "Ugu Horeeya",
                    previous: "Hore",
                    next: "Xiga",
                    last: "Ugu Dambeeya"
                },
                aria: {
                    sortAscending: ": riix si aad u kala soocdo kor u kaca",
                    sortDescending: ": riix si aad u kala soocdo hoos u dhaca"
                }
            };
        }
        // Default English — use modern keys
        return {
            search: "",
            searchPlaceholder: "Search..."
        };
    };

    // ---------- DataTables Init ----------
    U.makeDataTable = function (tableName, type = "1", recordsPerPage = 10) {
        if (!hasJQ || !$.fn.DataTable) return null;

        const currentLanguage = U.getCurrentLanguage();
        const languageOptions = U.getDataTableLanguageOptions(currentLanguage);

        const table = $(tableName).DataTable({
            buttons: ["copy", "excel", "pdf", "colvis"],
            responsive: false,
            pageLength: recordsPerPage,
            language: languageOptions,
            initComplete: function () {
                U.styleDataTableButtonsAndPagination(tableName);
            }
        });

        U.applyRowStyles();
        table.on("draw.dt", U.applyRowStyles);

        if (type === "2") {
            table.buttons().container().appendTo(`${tableName}_wrapper .col-md-6:eq(0)`);
        } else if (type === "3") {
            table.buttons().container().hide();
        }

        return table;
    };

    U.styleDataTableButtonsAndPagination = function (tableName) {
        if (!hasJQ) return;
        // Button cleanup
        $(`${tableName}_wrapper .btn-primary`)
            .removeClass("btn-primary")
            .addClass("bg-info")
            .css("color", "white");

        $(`${tableName}_wrapper .paginate_button`)
            .removeClass("btn-primary")
            .addClass("btn-info")
            .css("color", "white");
    };

    U.applyRowStyles = function () {
        if (!hasJQ) return;
        $(".even").addClass("bg-light-transparent");
        $(".odd").removeClass("bg-light-transparent");
    };

    // ---------- Dates ----------
    U.formattedDate = function (date) {
        if (typeof w.moment !== "function") return "";
        return w.moment(date).format("DD-MMM-YYYY");
    };

    // ---------- Sidebar Toggle ----------
    U.hideMenu = function () {
        const root = document.querySelector('.app') || document.body; // fallback just in case
        root.classList.toggle('sidenav-toggled');
    };

    // ---------- Notifications ----------
    U.Notify = function (type, message) {
        // notif() from notifIt.js — guard if missing
        if (typeof w.notif !== "function") {
            console.warn("notif() not found. Message:", type, message);
            return;
        }
        const notifType = type === "success" ? "primary" : "error";
        const notifTitle = type === "success" ? "Success" : "Error";

        w.notif({
            type: notifType,
            msg: `<b>${notifTitle}:</b> ${message}`,
            position: "center",
            width: 500,
            height: 60,
            autohide: true
        });
    };

    U.showErrorMessageJs = function (errorMessage) {
        if (!hasJQ) return;
        const isSomali = U.getCurrentLanguage() === "so";
        const sHtml = `
      <div class="notification-container">
        <div class="alert alert-danger alert-dismissible fade show p-0 mb-0" role="alert">
          <p class="py-3 px-5 mb-0 border-bottom border-bottom-danger-light">
            <span class="alert-inner--icon me-2"><i class="fe fe-slash"></i></span>
            <strong>${isSomali ? "Fariinta Khaladka" : "Error Message"}</strong>
          </p>
          <p class="py-3 px-5">${errorMessage}</p>
          <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
            <span aria-hidden="true">×</span>
          </button>
        </div>
      </div>`;
        $(".js-notification").html(sHtml);
    };

    U.showSuccessMessageJs = function (successMessage) {
        if (!hasJQ) return;
        const isSomali = U.getCurrentLanguage() === "so";
        const sHtml = `
      <div class="notification-container">
        <div class="alert alert-success alert-dismissible fade show p-0 mb-4 notification-message" role="alert">
          <p class="py-3 px-5 mb-0 border-bottom border-bottom-success-light">
            <span class="alert-inner--icon me-2"><i class="fe fe-thumbs-up"></i></span>
            <strong>${isSomali ? "Fariinta Guusha" : "Success Message"}</strong>
          </p>
          <p class="py-3 px-5">${successMessage}</p>
          <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
            <span aria-hidden="true">×</span>
          </button>
        </div>
      </div>`;
        $(".js-notification").html(sHtml);
    };

    // ---------- Numbers ----------
    U.formatNumber = function (x, opts) {
        const n = Number(x);
        if (!isFinite(n)) return "0";
        // defaults to 2 decimals; caller can override
        const o = Object.assign({ minimumFractionDigits: 2, maximumFractionDigits: 2 }, opts);
        return n.toLocaleString(undefined, o);
    };

    U.formatInt = function (x) {
        const n = Number(x);
        if (!isFinite(n)) return "0";
        return n.toLocaleString();
    };

    // ---------- Strings ----------
    U.escapeRegex = function (s) {
        return String(s).replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    };

    // ---------- Backward compatibility (old globals) ----------
    w.tryParseJson = U.tryParseJson;
    w.formatTables = U.formatTables;
    w.setMode = U.setMode;
    w.getCurrentLanguage = U.getCurrentLanguage;
    w.getDataTableLanguageOptions = U.getDataTableLanguageOptions;
    w.makeDataTable = U.makeDataTable;
    w.styleDataTableButtonsAndPagination = U.styleDataTableButtonsAndPagination;
    w.applyRowStyles = U.applyRowStyles;
    w.formattedDate = U.formattedDate;
    w.hideMenu = U.hideMenu;
    w.Notify = U.Notify;
    w.showErrorMessageJs = U.showErrorMessageJs;
    w.showSuccessMessageJs = U.showSuccessMessageJs;
    w.log = U.log;
    w.lang = U.lang;
    w.formatNumber = U.formatNumber;
    w.formatInt = U.formatInt;
    w.escapeRegex = U.escapeRegex;
    w.U = U;

})(window, window.jQuery);
