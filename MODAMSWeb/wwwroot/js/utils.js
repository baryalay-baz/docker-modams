// utils.js (classic script, NOT type="module")
// AMS global utilities — namespaced + backward compatible

(function (w, $) {
    "use strict";

    // Ensure jQuery exists for jQuery-dependent helpers
    const hasJQ = !!$;

    // Namespace
    const AMS = (w.AMS = w.AMS || {});
    const U = (AMS.util = AMS.util || {});

    // ---------- Ready / Page Key ----------
    U.ready = function (fn) {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", fn, { once: true });
        } else {
            fn();
        }
    };
    U.pageKey = function () {
        return (
            document.body.getAttribute("data-page") ||
            document.body.getAttribute("data-page-fallback") ||
            ""
        );
    };

    // ---------- API Calls ----------
    U.fetchJson = async function (url, options = {}) {
        // merge defaults
        const finalOptions = Object.assign(
            {
                method: "GET",
                headers: {
                    "Accept": "application/json",
                    // automatically include anti-forgery token if available
                    ...U.csrfHeader?.()
                }
            },
            options
        );

        let res;
        try {
            res = await fetch(url, finalOptions);
        } catch (networkErr) {
            throw new Error("Network error contacting " + url + ": " + networkErr);
        }

        if (!res.ok) {
            throw new Error("Request to " + url + " failed with status " + res.status);
        }

        try {
            return await res.json();
        } catch (parseErr) {
            throw new Error("Invalid JSON from " + url + ": " + parseErr);
        }
    };


    // ---------- Assert (warn-once in prod) ----------
    U._assertWarned = U._assertWarned || new Set();
    U.assert = function (condition, message) {
        const env = (w.AMS && w.AMS.env) || "prod";
        if (condition) return true;

        if (env === "dev" || env === "development") {
            console.error("[ASSERT FAIL]", message);
            throw new Error(message || "Assertion failed");
        } else {
            if (!U._assertWarned.has(message)) {
                console.warn("[ASSERT]", message);
                U._assertWarned.add(message);
            }
            return false;
        }
    };

    // ---------- Bridge / CSRF ----------
    U.bridge = function () {
        return (w.AMS && w.AMS.bridge) || {};
    };
    U.csrfHeader = function () {
        const b = U.bridge();
        const name =
            b.antiForgery?.headerName ||
            b.antiForgery?.header ||
            "RequestVerificationToken";
        const token =
            b.antiForgery?.requestToken ||
            b.antiForgery?.token ||
            document.querySelector('meta[name="request-verification-token"]')
                ?.content ||
            document.querySelector('input[name="__RequestVerificationToken"]')
                ?.value ||
            "";
        return token ? { [name]: token } : {};
    };

    // ---------- Logger ----------
    U.log = function (...args) {
        console.log("[AMS]", ...args);
    };
    U.logWarn = function (...args) {
        console.warn("[AMS]", ...args);
    };
    U.logError = function (...args) {
        console.error("[AMS]", ...args);
    };

    // ---------- Small utils ----------
    U.debounce = function (fn, wait = 150) {
        let t = null;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), wait);
        };
    };
    U.escapeRegex = function (s) {
        return String(s).replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    };
    U.escapeHtml = function (s) {
        return String(s)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    };
    U.toNumber = (v, def = 0) => {
        const n = Number(v);
        return Number.isFinite(n) ? n : def;
    };
    U.toInt = (v, def = 0) => {
        const n = parseInt(v, 10);
        return Number.isFinite(n) ? n : def;
    };

    // ---------- PARSE JSON ----------
    U.tryParseJson = function (jsonString, identifier = "JSON") {
        try {
            return { status: "success", data: JSON.parse(jsonString) };
        } catch (error) {
            return {
                status: "error",
                message: `${identifier} data is not in valid JSON format.`,
            };
        }
    };

    // ---------- Event helpers (namespaced + idempotent) ----------
    U.on = function (ns, evt, sel, fn) {
        $(document).off(evt + ns, sel).on(evt + ns, sel, fn);
    };
    U.off = function (ns, evt, sel) {
        $(document).off(evt + ns, sel);
    };

    // ---------- Tables (scoped styling) ----------
    U.formatTables = function (rootSel = "table.dataTable") {
        if (!hasJQ) return;
        $("thead").addClass("bg-info-gradient ms-auto divShadow--xs");
        $("th").addClass("text-white");
    };

    // ---------- Theme Mode ----------
    U.setMode = function (mode) {
        // mode: "dark-mode" | "light-mode" | undefined (read from LS)
        const saved = localStorage.getItem("displayMode");
        const target = mode || saved || "light-mode";
        const isDark = target === "dark-mode";

        document.body.classList.toggle("dark-mode", isDark);
        document.body.classList.toggle("light-mode", !isDark);
        localStorage.setItem("displayMode", isDark ? "dark-mode" : "light-mode");

        const footer = document.getElementById("footerImgEU");
        if (footer) {
            const img = new Image();
            img.src = isDark
                ? "/assets/images/brand/EU_Horizontal3_small.png"
                : "/assets/images/brand/EU_Horizontal2_small.png";
            img.alt = "EU";
            footer.replaceChildren(img);
        }
    };
    U.toggleMode = function () {
        const cur = localStorage.getItem("displayMode") || "light-mode";
        U.setMode(cur === "dark-mode" ? "light-mode" : "dark-mode");
    };

    // ---------- Language ----------
    U._getCurrentLanguageCookie = function () {
        const cookieName = ".AspNetCore.Culture=";
        const part = document.cookie.split("; ").find((row) =>
            row.startsWith(cookieName)
        );
        if (!part) return "en";
        const decoded = decodeURIComponent(part.slice(cookieName.length));
        const m = decoded.match(/(?:c|uic)=([A-Za-z-]+)/);
        return m ? m[1].split("-")[0] : "en";
    };
    U.lang = function () {
        const b = U.bridge();
        return b.cultureTwoLetter || U._getCurrentLanguageCookie() || "en";
    };
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
                    last: "Ugu Dambeeya",
                },
                aria: {
                    sortAscending: ": riix si aad u kala soocdo kor u kaca",
                    sortDescending: ": riix si aad u kala soocdo hoos u dhaca",
                },
            };
        }
        // Default English — consistent modern keys
        return {
            processing: "Processing...",
            lengthMenu: "_MENU_",
            zeroRecords: "No matching records",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            infoEmpty: "Showing 0 to 0 of 0 entries",
            infoFiltered: "(filtered from _MAX_ total entries)",
            emptyTable: "No data available",
            loadingRecords: "Loading...",
            search: "",
            searchPlaceholder: "Search...",
            paginate: {
                first: "First",
                previous: "Prev",
                next: "Next",
                last: "Last",
            },
        };
    };
    U.makeDataTable = function (tableRef, type = "1", recordsPerPage = 10) {
        if (!w.jQuery || !$.fn || !$.fn.DataTable) {
            console.error("[AMS] DataTables not loaded.");
            return null;
        }

        // --- normalize tableRef -> selector string ---
        let sel = null;

        if (tableRef && tableRef.jquery) {
            sel =
                tableRef.selector ||
                (function () {
                    const el = tableRef[0];
                    if (!el) return null;
                    if (!el.id)
                        el.id = "tbl_" + Math.random().toString(36).slice(2, 8);
                    return "#" + el.id;
                })();
        } else if (tableRef instanceof Element) {
            if (!tableRef.id)
                tableRef.id = "tbl_" + Math.random().toString(36).slice(2, 8);
            sel = "#" + tableRef.id;
        } else if (typeof tableRef === "string") {
            sel = tableRef;
        }

        if (!sel) {
            console.error("[AMS] makeDataTable: invalid tableRef", tableRef);
            return null;
        }

        const $tbl = $(sel);
        if (!$tbl.length) {
            console.warn("[AMS] makeDataTable: table not found:", sel);
            return null;
        }

        const tMode = String(type); // "1","2","3"
        const currentLanguage = U.getCurrentLanguage?.() ?? "en";
        const languageOptions = U.getDataTableLanguageOptions?.(currentLanguage) ?? {};

        const domLayout =
            "<'row ams-dt-header align-items-center mb-2' " +
            "<'col-12 col-md-4 d-flex align-items-center mb-2 mb-md-0' l> " +
            "<'col-12 col-md-4 d-flex justify-content-center mb-2 mb-md-0' B> " +
            "<'col-12 col-md-4 d-flex justify-content-md-end' f> " +
            ">" +
            "t" +
            "<'row align-items-center mt-2' " +
            "<'col-12 col-md-6 d-flex align-items-center' i> " +
            "<'col-12 col-md-6 d-flex justify-content-md-end justify-content-start mt-2 mt-md-0' p> " +
            ">";

        // delegated search binder (survives DOM rebuild)
        function wireSearchDelegated(dtApi) {
            const $wrapper = $(`${sel}_wrapper`);
            if (!$wrapper.length) {
                console.warn("[AMS] wireSearchDelegated: wrapper not found for", sel);
                return;
            }

            // stash api reference on wrapper
            $wrapper.data("dtApi", dtApi);

            // remove old delegated handler (namespaced)
            $wrapper.off(
                "input.amsDelegatedSearch keyup.amsDelegatedSearch",
                ".dataTables_filter input[type='search']"
            );

            // delegate events from wrapper to the search input
            $wrapper.on(
                "input.amsDelegatedSearch keyup.amsDelegatedSearch",
                ".dataTables_filter input[type='search']",
                function () {
                    const apiFromWrapper = $wrapper.data("dtApi");
                    if (!apiFromWrapper) return;
                    const val = this.value ?? "";
                    apiFromWrapper.search(val).draw();
                }
            );
        }

        // CASE 1: table already initialized
        if ($.fn.DataTable.isDataTable($tbl)) {
            const api = $tbl.DataTable();
            try {
                // ✅ BUTTON VISIBILITY LOGIC (final rules)
                // type "1" => hide
                // type "2" => show
                // type "3" => show
                if (tMode === "1") {
                    $(api.buttons?.().container?.()).hide();
                } else {
                    $(api.buttons?.().container?.()).show();
                }

                // rebuild header with your styling
                U.styleDataTableButtonsAndPagination?.(sel, tMode);

                // reattach delegated search
                wireSearchDelegated(api);

                // adjust columns after DOM changes
                api.columns?.adjust?.().draw(false);
            } catch (err) {
                console.error("[AMS] reuse DataTable error:", err);
            }
            return api;
        }

        // CASE 2: fresh init
        const api = $tbl.DataTable({
            dom: domLayout,
            buttons: ["copy", "excel", "pdf"],
            responsive: false,
            pageLength: recordsPerPage,
            language: languageOptions,
            autoWidth: false,
            initComplete: function () {
                try {
                    // this === DataTable context
                    const dtApi = this.api();

                    // ✅ BUTTON VISIBILITY LOGIC (final rules)
                    // type "1" => hide
                    // type "2" => show
                    // type "3" => show
                    if (tMode === "1") {
                        $(dtApi.buttons().container()).hide();
                    } else {
                        $(dtApi.buttons().container()).show();
                    }

                    // apply your styling
                    U.styleDataTableButtonsAndPagination?.(sel, tMode);

                    // hook delegated search
                    wireSearchDelegated(dtApi);
                } catch (err) {
                    console.error("[AMS] initComplete error:", err);
                }
            }
        });

        // zebra / redraw styling
        try {
            U.applyRowStyles?.();
            api.on("draw.dt", U.applyRowStyles);
        } catch { }

        // fix columns if table starts hidden
        $(document).one(
            "shown.bs.tab shown.bs.collapse shown.bs.modal",
            function () {
                try {
                    api.columns.adjust().draw(false);
                } catch { }
            }
        );

        return api;
    };
    U.styleDataTableButtonsAndPagination = function (tableName, tMode) {
        if (!hasJQ) return;

        const $wrapper = $(`${tableName}_wrapper`);

        // --- grab original DT chunks ---
        let $topRow = $wrapper.find('> .row').first();         // header row DT created
        let $lenWrap = $topRow.find('.dataTables_length');     // dropdown
        let $filterWrap = $topRow.find('.dataTables_filter');  // search
        let $buttonsWrap = $wrapper.find('.dt-buttons');       // export buttons

        // Bail if we've already rebuilt this exact table header once
        if ($topRow.hasClass('ams-styled')) {
            // still enforce hide/show in case tMode changed
            if (tMode === "3") {
                $topRow.find('.ams-export-col').hide();
            } else {
                $topRow.find('.ams-export-col').show();
            }
            return;
        }

        // ========== 1. STYLE: Page length dropdown ==========
        if ($lenWrap.length) {
            const $label = $lenWrap.find('label');
            const $select = $label.find('select');

            $select
                .addClass('form-select form-select-sm')
                .removeClass('form-control form-control-sm')
                .css({
                    width: 'auto',
                    minWidth: '3.5rem',
                    paddingRight: '1.5rem',
                    lineHeight: '1.4',
                    height: '32px'
                });

            // remove "Show ... entries"
            $label
                .empty()
                .append($select)
                .addClass('mb-0 d-flex align-items-center')
                .css({ marginBottom: 0 });

            $lenWrap
                .addClass('mb-2 mb-md-0 d-flex align-items-center')
                .css({ marginBottom: 0 });
        }

        // ========== 2. STYLE: Export buttons (Copy / Excel / PDF / etc.) ==========
        // Only build if NOT mode "3"
        let $btnGroup = null;
        if (tMode !== "3") {
            $btnGroup = $('<div class="btn-group" role="group" aria-label="Export buttons"></div>');

            if ($buttonsWrap.length) {
                $buttonsWrap.children('button, a').each(function () {
                    const $btn = $(this);

                    // strip DT stock classes
                    $btn.removeClass(function (_i, cls) {
                        return (cls || "")
                            .split(" ")
                            .filter(c =>
                                c.startsWith('dt-') ||
                                c.startsWith('buttons-') ||
                                c === 'btn' ||
                                c === 'btn-sm' ||
                                c === 'btn-primary' ||
                                c === 'btn-secondary' ||
                                c === 'btn-success' ||
                                c === 'btn-outline-primary' ||
                                c === 'btn-outline-success' ||
                                c === 'btn-default-outline' ||
                                c === 'btn-outline-light'
                            )
                            .join(" ");
                    });

                    // apply theme button look
                    $btn
                        .addClass('btn btn-outline-light btn-sm')
                        .css({
                            borderRadius: '0',
                            lineHeight: '1.2',
                            height: '32px',
                            paddingTop: '0.25rem',
                            paddingBottom: '0.25rem'
                        });

                    $btnGroup.append($btn);
                });
            }
        }

        // ========== 3. STYLE: Search box ==========
        if ($filterWrap.length) {
            const $label = $filterWrap.find('label');
            const $searchInput = $label.find('input[type="search"]');

            // kill literal "Search:" (or "Raadi:") label text node
            $label
                .contents()
                .filter(function () {
                    return this.nodeType === 3; // text node
                })
                .remove();

            // figure out placeholder based on current language
            const langCode = U.getCurrentLanguage ? U.getCurrentLanguage() : "en";
            const dtLang = U.getDataTableLanguageOptions
                ? U.getDataTableLanguageOptions(langCode)
                : {};
            const placeholderText =
                dtLang.searchPlaceholder ||
                (langCode === "so" ? "Raadi..." : "Search...");

            $searchInput
                .addClass('form-control form-control-sm')
                .removeClass('form-control-lg form-control-md')
                .attr('placeholder', placeholderText)
                .css({
                    minWidth: '12rem',
                    height: '32px',
                    lineHeight: '32px'
                });

            $label
                .addClass('mb-0 d-flex align-items-center')
                .css({ marginBottom: 0 });

            $filterWrap
                .addClass('mb-2 mb-md-0 d-flex align-items-center')
                .css({ marginBottom: 0 });
        }

        // ========== 4. REBUILD TOP ROW into columns ==========
        // We’re gonna control the columns ourselves.

        const $lenFinal = $lenWrap || $('<div class="dataTables_length"></div>');
        const $filterFinal = $filterWrap || $('<div class="dataTables_filter"></div>');

        // wipe DT's first row so we can rebuild
        $topRow.empty();

        // left col: page length
        const $colLeft = $('<div class="col-sm-12 col-md-auto d-flex align-items-center mb-2 mb-md-0"></div>');
        $colLeft.append($lenFinal);

        // middle col: export buttons (only if tMode !== "3" and we actually have buttons)
        let $colMid = null;
        if ($btnGroup && $btnGroup.children().length) {
            $colMid = $('<div class="col-sm-12 col-md-auto d-flex align-items-center mb-2 mb-md-0 ams-export-col"></div>');
            $colMid.append($btnGroup);
        }

        // right col: search
        const $colRight = $('<div class="col-sm-12 col-md d-flex justify-content-md-end align-items-center mb-2 mb-md-0"></div>');
        $colRight.append($filterFinal);

        // stitch columns back in
        $topRow.append($colLeft);

        if ($colMid) {
            $topRow.append($colMid);
        }

        $topRow
            .append($colRight)
            .addClass('ams-styled align-items-start')
            .css({
                borderBottom: '1px solid #e9ecef',
                paddingBottom: '.5rem',
                marginBottom: '.5rem'
            });

        // if mode "3", make sure that export col is hidden (paranoia)
        if (tMode === "3") {
            $topRow.find('.ams-export-col').hide();
        }

        // we moved children, we don't need the old wrapper node anymore
        $buttonsWrap.remove();

        // ========== 5. BOTTOM (info + pagination) cleanup ==========
        const $bottomRow = $wrapper.find('> .row').eq(1);
        const $infoWrap = $bottomRow.find('.dataTables_info');
        const $pageWrap = $bottomRow.find('.dataTables_paginate');

        $infoWrap
            .addClass('mb-0 small text-muted d-flex align-items-center')
            .css({ marginBottom: 0 });

        $pageWrap
            .addClass('mb-0 d-flex align-items-center justify-content-md-end ms-auto')
            .css({ marginBottom: 0 });

        $pageWrap.find('.paginate_button').css({
            background: 'transparent'
        });
    };
    U.applyRowStyles = function () {
        if (!hasJQ) return;
        $(".even").addClass("bg-light-transparent");
        $(".odd").removeClass("bg-light-transparent");
    };

    // ---------- Dates ----------
    U.formattedDate = function (date) {
        if (typeof w.moment === "function")
            return w.moment(date).format("DD-MMM-YYYY");
        try {
            const d = new Date(date);
            if (isNaN(d)) return "";
            return d.toLocaleDateString(undefined, {
                day: "2-digit",
                month: "short",
                year: "numeric",
            });
        } catch {
            return "";
        }
    };

    // ---------- Sidebar Toggle ----------
    U.hideMenu = function () {
        const root = document.querySelector(".app") || document.body;
        root.classList.toggle("sidenav-toggled");
    };

    // ---------- Notifications ----------
    U.Notify = function (type, message) {
        if (typeof w.notif !== "function") {
            U.log?.("Notify skipped:", type, message);
            return;
        }
        const notifType = type === "success" ? "primary" : "error";
        const notifTitle = type === "success" ? "Success" : "Error";

        w.notif({
            type: notifType,
            msg: `<b>${notifTitle}:</b> ${U.escapeHtml(message)}`,
            position: "center",
            width: 500,
            height: 60,
            autohide: true,
        });
    };
    U.showErrorMessageJs = function (errorMessage) {
        if (!hasJQ) return;
        const isSomali = U.getCurrentLanguage() === "so";
        const safe = U.escapeHtml(errorMessage);
        const sHtml = `
      <div class="notification-container">
        <div class="alert alert-danger alert-dismissible fade show p-0 mb-0" role="alert">
          <p class="py-3 px-5 mb-0 border-bottom border-bottom-danger-light">
            <span class="alert-inner--icon me-2"><i class="fe fe-slash"></i></span>
            <strong>${isSomali ? "Fariinta Khaladka" : "Error Message"}</strong>
          </p>
          <p class="py-3 px-5">${safe}</p>
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
        const safe = U.escapeHtml(successMessage);
        const sHtml = `
      <div class="notification-container">
        <div class="alert alert-success alert-dismissible fade show p-0 mb-4 notification-message" role="alert">
          <p class="py-3 px-5 mb-0 border-bottom border-bottom-success-light">
            <span class="alert-inner--icon me-2"><i class="fe fe-thumbs-up"></i></span>
            <strong>${isSomali ? "Fariinta Guusha" : "Success Message"}</strong>
          </p>
          <p class="py-3 px-5">${safe}</p>
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
        const o = Object.assign(
            { minimumFractionDigits: 2, maximumFractionDigits: 2 },
            opts
        );
        return n.toLocaleString(undefined, o);
    };
    U.formatInt = function (x) {
        const n = Number(x);
        if (!isFinite(n)) return "0";
        return n.toLocaleString();
    };

    // ---------- Backward compatibility (old globals) ----------
    w.bridge = U.bridge;
    w.csrfHeader = U.csrfHeader;
    w.fetchJson = U.fetchJson;
    w.tryParseJson = U.tryParseJson;
    w.formatTables = U.formatTables;
    w.setMode = U.setMode;
    w.toggleMode = U.toggleMode;
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
    w.escapeHtml = U.escapeHtml;
    w.assert = U.assert;
    w.on = U.on;
    w.off = U.off;
    w.toNumber = U.toNumber;
    w.toInt = U.toInt;
    w.U = U;
})(window, window.jQuery);
