// /js/utils.js
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
        const finalOptions = Object.assign(
            {
                method: "GET",
                headers: {
                    "Accept": "application/json",
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
    U.log = function (...args) { console.log("[AMS]", ...args); };
    U.logWarn = function (...args) { console.warn("[AMS]", ...args); };
    U.logError = function (...args) { console.error("[AMS]", ...args); };

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

    //----------- Select2 Initialization ----------
    U.initSelect2 = function initSelect2(opts = {}) {
        if (!window.jQuery || !jQuery.fn || !jQuery.fn.select2) {
            console.error("[AMS] Select2 JS not loaded.");
            return;
        }
        const $root = (opts.root && (opts.root.jquery ? opts.root : jQuery(opts.root))) || jQuery(document);
        const selector = opts.selector || ".select2";
        const dropdownParent = opts.dropdownParent ? (opts.dropdownParent.jquery ? opts.dropdownParent : jQuery(opts.dropdownParent)) : jQuery(document.body);
        const width = opts.width || "100%";
        const allowClear = opts.allowClear !== false;

        $root.find(selector).each(function () {
            const $el = jQuery(this);

            // If already enhanced, destroy then remove any orphaned containers
            if ($el.hasClass("select2-hidden-accessible")) {
                try { $el.select2("destroy"); } catch (_) { }
                $el.siblings(".select2-container").remove();
            }

            // Init
            $el.select2({
                width,
                dropdownParent,
                placeholder: $el.attr("data-placeholder") || $el.data("placeholder") || "",
                allowClear
            });

            // Ensure placeholder shows if empty or "0"
            const v = ($el.val() ?? "").toString();
            if (v === "" || v === "0") {
                $el.val(null).trigger("change");
            }
        });
    };
    U.initOneSelect2 = function initOneSelect2(el, opts = {}) {
        return U.initSelect2({ ...opts, root: el, selector: el });
    };

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

    // === Hover Actions helper: tiny template "/assets/edit/{id}"
    U._interpolateHref = function (tpl, row) {
        if (!tpl) return null;
        return tpl.replace(/\{([^}]+)\}/g, (_, k) => String(row?.[k] ?? ""));
    };

    // ---------- INTERNAL: one-time global CSS for hover-actions ----------
    function ensureGlobalHoverActionsStyle() {
        const id = "ams-hover-actions-style";
        if (document.getElementById(id)) return;

        const style = document.createElement("style");
        style.id = id;
        style.textContent = `
/* Keep hover action buttons visible and colored even when DT/Bootstrap apply :hover styles */
.table--hover-actions .row-actions{
  position:absolute; right:.5rem; top:50%; transform:translateY(-50%);
  display:flex; gap:.5rem; background:rgba(255,255,255,.85);
  border-radius:2rem; padding:.25rem .4rem; box-shadow:0 6px 18px rgba(0,0,0,.12);
  opacity:0; pointer-events:none; transition:opacity .15s ease;
}
.table--hover-actions tbody tr:hover .row-actions,
.table--hover-actions tbody tr.show-actions .row-actions{opacity:1; pointer-events:auto;}
.table--hover-actions tbody tr{position:relative;}
.table--hover-actions td.with-actions-pad{padding-inline-end:128px}

/* Button visual: lock/edit/etc must not turn white on hover */
.table--hover-actions .row-actions .btn{
  color:var(--bs-primary) !important;
  border-color:rgba(13,110,253,.35) !important;
  background-color:#fff !important;
}
.table--hover-actions .row-actions .btn:hover{
  color:var(--bs-primary) !important;
  background-color:#fff !important;
  border-color:rgba(13,110,253,.55) !important;
  filter:brightness(1.02);
}

/* Icon inside keep current color */
.table--hover-actions .row-actions .btn i,
.table--hover-actions .row-actions .btn svg{
  color:inherit !important; fill:currentColor;
}`;
        document.head.appendChild(style);
    }

    U.makeDataTable = function (tableRef, type = "1", recordsPerPage = 10, actionsConfig) {
        if (!w.jQuery || !$.fn || !$.fn.DataTable) {
            console.error("[AMS] DataTables not loaded.");
            return null;
        }

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

        const tMode = String(type);
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

        function wireSearchDelegated(dtApi) {
            const $wrapper = $(`${sel}_wrapper`);
            if (!$wrapper.length) {
                console.warn("[AMS] wireSearchDelegated: wrapper not found for", sel);
                return;
            }

            $wrapper.data("dtApi", dtApi);

            $wrapper.off(
                "input.amsDelegatedSearch keyup.amsDelegatedSearch",
                ".dataTables_filter input[type='search']"
            );

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
        function wireLengthDelegated(dtApi) {
            const $wrapper = $(`${sel}_wrapper`);
            if (!$wrapper.length) {
                console.warn("[AMS] wireLengthDelegated: wrapper not found for", sel);
                return;
            }

            $wrapper.data("dtApi", dtApi);

            $wrapper.off("change.amsDelegatedLength", ".dataTables_length select");
            $wrapper.on(
                "change.amsDelegatedLength",
                ".dataTables_length select",
                function () {
                    const apiFromWrapper = $wrapper.data("dtApi");
                    if (!apiFromWrapper) return;

                    const newLen = parseInt($(this).val(), 10);
                    if (!isNaN(newLen)) {
                        apiFromWrapper.page.len(newLen).draw(false);
                    }
                }
            );
        }

        // === Hover Actions: helpers (scoped to this call) ======================
        const wantsActions =
            actionsConfig &&
            actionsConfig.enable !== false &&
            Array.isArray(actionsConfig.buttons) &&
            actionsConfig.buttons.length > 0;

        // Inject global CSS once for hover actions
        if (wantsActions) ensureGlobalHoverActionsStyle();

        function ensurePaddingOverrideIfAny() {
            if (!wantsActions) return;
            if (!Number.isFinite(actionsConfig.paddingPx)) return;
            const px = Math.max(100, actionsConfig.paddingPx | 0);
            const id = "pad-" + Math.random().toString(36).slice(2);
            const style = document.createElement("style");
            style.textContent =
                `.table--hover-actions[data-pad="${id}"] td.with-actions-pad{padding-inline-end:${px}px}`;
            document.head.appendChild(style);
            $tbl.attr("data-pad", id);
        }

        function buildRowActionsHtml(buttons) {
            const isSo = (U.getCurrentLanguage?.() === "so");

            // small helper: map bootstrap-ish variants to hex if user passes {variant:"danger"}
            const VARIANT_MAP = {
                primary: "#0d6efd",
                secondary: "#6c757d",
                success: "#198754",
                danger: "#dc3545",
                warning: "#ffc107",
                info: "#0dcaf0",
                dark: "#212529",
                purple: "#6f42c1",
                orange: "#fd7e14",
                teal: "#20c997"
            };

            return `
      <div class="row-actions">
        ${buttons.map((b) => {
                // Titles
                const rawTitle = isSo ? (b.titleSo || b.titleEn || b.title || "")
                    : (b.titleEn || b.titleSo || b.title || "");
                const title = U.escapeHtml(rawTitle);
                const icon = b.iconHtml || "";
                const extra = b.className ? " " + U.escapeHtml(b.className) : "";

                // Per-button color control:
                // Accept either explicit color/tint or a variant key (danger, success, etc.)
                const variant = (b.variant || "").toLowerCase().trim();
                const base = b.color || (variant && VARIANT_MAP[variant]) || ""; // hex/rgb
                const tint = (typeof b.tint === "string")
                    ? b.tint
                    : (base ? "rgba(" + hexToRgb(base) + ", .12)" : "transparent");

                // Inline CSS variables to beat specificity wars
                const styleVars = [
                    base ? `--btn-color:${base}` : "",
                    `--btn-tint:${tint}`
                ].filter(Boolean).join("; ");

                return `<button type="button"
                        class="btn btn-sm act-${U.escapeHtml(b.key)}${extra}"
                        data-bs-toggle="tooltip"
                        data-bs-placement="top"
                        data-bs-trigger="hover focus"
                        title="${title}"
                        data-action="${U.escapeHtml(b.key)}"
                        style="${styleVars}">
                        ${icon || title}
                    </button>`;
            }).join("")}
      </div>`;
        }

        /* helper local to U.makeDataTable scope */
        function hexToRgb(hex) {
            // supports #rgb, #rrggbb
            let c = hex.replace("#", "").trim();
            if (c.length === 3) c = c.split("").map(ch => ch + ch).join("");
            const n = parseInt(c, 16);
            if (Number.isNaN(n) || c.length !== 6) return "220,53,69"; // fallback = danger
            const r = (n >> 16) & 255, g = (n >> 8) & 255, b = n & 255;
            return `${r},${g},${b}`;
        }


        function applyHoverActions(dtApi) {
            if (!wantsActions) return;

            $tbl.addClass("table--hover-actions");

            const rows = dtApi.rows({ page: "current" }).nodes().to$();

            rows.each(function () {
                const $tr = $(this);
                // Add actions only once per row
                if (!$tr.children(".row-actions").length) {
                    $tr.append(buildRowActionsHtml(actionsConfig.buttons));

                    // Initialize Bootstrap tooltips for new buttons (if Bootstrap is present)
                    if (window.bootstrap?.Tooltip) {
                        $tr.find('.row-actions [data-bs-toggle="tooltip"]').each(function () {
                            if (!this._amsTip) {
                                this._amsTip = new bootstrap.Tooltip(this, { container: 'body', trigger: 'hover' });
                            }
                        });
                    }
                }
            });

            // Delegated click handling
            $tbl.off(".amsRowActions").on("click.amsRowActions", "tbody .row-actions [data-action]", function (e) {
                e.stopPropagation();

                const $btn = $(this);
                const $tr = $btn.closest("tr");
                const api = $tbl.DataTable();
                const row = api.row($tr).data();          // still handy if you need other values
                const id = $tr.data("id");               // <-- primary source

                const key = $btn.data("action");
                const cfg = actionsConfig.buttons.find(b => b.key === key);
                if (!cfg) return;

                // Safety: if no data-id on the row, try to fallback (hidden ID column/object rows)
                let ctxId = id;
                if (ctxId == null) {
                    if (Number.isInteger(actionsConfig.idColumnIndex) && Array.isArray(row)) {
                        const raw = row[actionsConfig.idColumnIndex];
                        ctxId = (raw && raw.nodeType === 1) ? $(raw).text().trim() : raw;
                    } else if (row && typeof row === "object" && !Array.isArray(row)) {
                        ctxId = row.Id ?? row.id ?? null;
                    }
                }

                if (typeof cfg.onClick === "function") {
                    return cfg.onClick({ id: ctxId, row, event: e, api });
                }

                if (cfg.href) {
                    const url = (ctxId != null && String(cfg.href).includes("{id}"))
                        ? cfg.href.replace("{id}", encodeURIComponent(ctxId))
                        : cfg.href;
                    if (url) window.location = url;
                }
            });

            // Touch: tap row to show pills briefly
            $tbl.on("click.amsRowActions", "tbody tr", function (e) {
                if ($(e.target).closest(".row-actions").length) return;
                const $tr = $(this);
                $tr.toggleClass("show-actions");
                if ($tr.hasClass("show-actions")) {
                    setTimeout(() => $tr.removeClass("show-actions"), 3000);
                }
            });
        }

        // ======================================================================

        // CASE 1: table already initialized
        if ($.fn.DataTable.isDataTable($tbl)) {
            const api = $tbl.DataTable();
            try {
                if (tMode === "1") {
                    $(api.buttons?.().container?.()).hide();
                } else {
                    $(api.buttons?.().container?.()).show();
                }
                U.styleDataTableButtonsAndPagination?.(sel, tMode);

                wireSearchDelegated(api);
                wireLengthDelegated(api);
                ensurePaddingOverrideIfAny();   // <-- added

                api.off("draw._amsHover").on("draw._amsHover", function () {
                    applyHoverActions(api);
                });
                applyHoverActions(api);

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
                    const dtApi = this.api();

                    if (tMode === "1") {
                        $(dtApi.buttons().container()).hide();
                    } else {
                        $(dtApi.buttons().container()).show();
                    }

                    U.styleDataTableButtonsAndPagination?.(sel, tMode);
                    wireSearchDelegated(dtApi);
                    wireLengthDelegated(dtApi);
                    ensurePaddingOverrideIfAny();   // <-- added

                    dtApi.off("draw._amsHover").on("draw._amsHover", function () {
                        applyHoverActions(dtApi);
                    });
                    applyHoverActions(dtApi);
                } catch (err) {
                    console.error("[AMS] initComplete error:", err);
                }
            }
        });

        try {
            U.applyRowStyles?.();
            api.on("draw.dt", U.applyRowStyles);
        } catch { }

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

        let $topRow = $wrapper.find('> .row').first();
        let $lenWrap = $topRow.find('.dataTables_length');
        let $filterWrap = $topRow.find('.dataTables_filter');
        let $buttonsWrap = $wrapper.find('.dt-buttons');

        if ($topRow.hasClass('ams-styled')) {
            if (tMode === "3") {
                $topRow.find('.ams-export-col').hide();
            } else {
                $topRow.find('.ams-export-col').show();
            }
            return;
        }

        // 1) length
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

            $label.empty().append($select)
                .addClass('mb-0 d-flex align-items-center')
                .css({ marginBottom: 0 });

            $lenWrap
                .addClass('mb-2 mb-md-0 d-flex align-items-center')
                .css({ marginBottom: 0 });
        }

        // 2) export buttons
        let $btnGroup = null;
        if (tMode !== "3") {
            $btnGroup = $('<div class="btn-group" role="group" aria-label="Export buttons"></div>');

            if ($buttonsWrap.length) {
                $buttonsWrap.children('button, a').each(function () {
                    const $btn = $(this);
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

        // 3) search
        if ($filterWrap.length) {
            const $label = $filterWrap.find('label');
            const $searchInput = $label.find('input[type="search"]');

            $label.contents().filter(function () { return this.nodeType === 3; }).remove();

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

        // 4) rebuild top row
        const $lenFinal = $lenWrap || $('<div class="dataTables_length"></div>');
        const $filterFinal = $filterWrap || $('<div class="dataTables_filter"></div>');

        $topRow.empty();

        const $colLeft = $('<div class="col-sm-12 col-md-auto d-flex align-items-center mb-2 mb-md-0"></div>');
        $colLeft.append($lenFinal);

        let $colMid = null;
        if ($btnGroup && $btnGroup.children().length) {
            $colMid = $('<div class="col-sm-12 col-md-auto d-flex align-items-center mb-2 mb-md-0 ams-export-col"></div>');
            $colMid.append($btnGroup);
        }

        const $colRight = $('<div class="col-sm-12 col-md d-flex justify-content-md-end align-items-center mb-2 mb-md-0"></div>');
        $colRight.append($filterFinal);

        $topRow.append($colLeft);
        if ($colMid) $topRow.append($colMid);
        $topRow
            .append($colRight)
            .addClass('ams-styled align-items-start')
            .css({
                borderBottom: '1px solid #e9ecef',
                paddingBottom: '.5rem',
                marginBottom: '.5rem'
            });

        if (tMode === "3") {
            $topRow.find('.ams-export-col').hide();
        }

        $buttonsWrap.remove();

        // 5) bottom
        const $bottomRow = $wrapper.find('> .row').eq(1);
        const $infoWrap = $bottomRow.find('.dataTables_info');
        const $pageWrap = $bottomRow.find('.dataTables_paginate');

        $infoWrap
            .addClass('mb-0 small text-muted d-flex align-items-center')
            .css({ marginBottom: 0 });

        $pageWrap
            .addClass('mb-0 d-flex align-items-center justify-content-md-end ms-auto')
            .css({ marginBottom: 0 });

        $pageWrap.find('.paginate_button').css({ background: 'transparent' });
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
