/* wwwroot/js/stores-index.js */
(function (window, document, $) {
    "use strict";

    const U = window.AMS?.util || {};
    const SEL = {
        listHost: "#dvMainRow",
        search: "#txtSearchStore",
        select2: ".select2",
        reportModal: "#mdl-asset-report",
        btnReport: "#btnReport",
        frmReport: "#frmAssetReport"
    };
    const L = window.localizer || {
        lblPrimaryStore: "Primary Store",
        lblSecondaryStore: "Secondary Store",
        lblNoStoresAvailable: "No stores available",
        lblTotalAssets: "Total Assets",
        lblInitialCost: "Initial Cost",
        lblCurrentValue: "Current Value",
        lblMembers: "Members"
    };
    const isSO = (U.lang?.() || U.getCurrentLanguage?.() || "en") === "so";
    const STATE = {
        stores: [],
        employees: []
    };

    // ---------- Init ----------
    function init(/* ctx */) {
        if (!U) return console.error("[AMS] utils not loaded.");

        hydrateState();
        bindEvents();
        //initSelect2();
        renderCards(STATE.stores);
    }

    // ---------- State hydration ----------
    function hydrateState() {
        const d = window.data || {};

        STATE.stores = normalizeArray(d.storesData);
        STATE.employees = normalizeArray(d.storeEmployeesData);
    }
    function normalizeArray(data) {
        if (Array.isArray(data)) return data;
        if (typeof data === "string") {
            try { const v = JSON.parse(data); return Array.isArray(v) ? v : []; }
            catch { return []; }
        }
        return [];
    }

    // ---------- Events ----------
    function bindEvents() {
        const onSearch = U.debounce ? U.debounce(handleSearch, 150) : handleSearch;
        $(SEL.search).on("input", onSearch);

        // 1) Card click for navigation, but ignore clicks that originate from popover triggers or other interactive children
        $(document).on("click", ".js-store-card", function (e) {
            if ($(e.target).closest(".js-emp-pop, [data-bs-toggle='popover'], a[href], button, input, select, textarea").length) {
                return; // let the inner control handle it
            }
            const id = $(this).data("id");
            if (id != null) window.location.href = `/Users/Stores/StoreDetails/${id}`;
        });

        // 2) Explicitly prevent bubbling from popover triggers (safety net)
        $(document).on("click", ".js-emp-pop", function (e) {
            e.preventDefault();
            e.stopPropagation();
        });

        // Report submit
        $(SEL.btnReport).on("click", () => $(SEL.frmReport).trigger("submit"));
    }
    function handleSearch() {
        const kw = ($(SEL.search).val() || "").trim();

        if (!kw) {
            renderCards(STATE.stores);
            return;
        }

        const rx = new RegExp(U.escapeRegex?.(kw) || kw, "i");
        const filtered = STATE.stores.filter(s =>
            rx.test(s.name || "") ||
            rx.test(s.nameSo || "") ||
            rx.test(String(s.id || "")) ||
            rx.test(String(s.code || "")) ||
            rx.test(String(s.department || ""))
        );
        renderCards(filtered);
    }
    function initSelect2() {
        if ($.fn.select2) {
            $(SEL.select2).select2({ dropdownParent: $(SEL.reportModal) });
        }
    }

    // (Re)initialize Bootstrap 5 popovers for any avatar anchors
    function initPopovers() {
        // Dispose old ones first to avoid duplicates when re-rendering
        $('[data-bs-toggle="popover"]').each(function () {
            const inst = bootstrap.Popover.getInstance(this);
            if (inst) inst.dispose();
        });
        // Init fresh
        $('[data-bs-toggle="popover"]').each(function () {
            new bootstrap.Popover(this, {
                trigger: "hover focus",   // hover on desktop, focus for keyboard
                container: "body",
                placement: "right",
                html: true,
                sanitize: false           // if you trust the content; otherwise escape your strings
            });
        });
    }
    // ---------- Rendering ----------
    function renderCards(rows) {
        const host = document.querySelector(SEL.listHost);
        if (!host) return;

        if (!Array.isArray(rows) || rows.length === 0) {
            host.innerHTML = ""; // or show a friendly message
            return;
        }

        // Pick which card gets the tour labels; here: first in the current list
        const labelIndex = 0;
        host.innerHTML = rows.map((e, i) => storeCardHtml(e, i === labelIndex)).join("");

        // IMPORTANT: popovers must be initialized AFTER injecting the HTML
        initPopovers();
    }
    function storeCardHtml(e, isFirst) {
        const bgType = Number(e.storeType) === 1 ? "bg-primary" : "bg-info-transparent";

        const sStoreTypeTourLabel = isFirst ? " data-tour='stores.storetype'" : "";
        const sMemberListTourLabel = isFirst ? " data-tour='stores.memberlist'" : "";

        const storeTypeText = Number(e.storeType) === 1
            ? `<span${sStoreTypeTourLabel} class="text-primary text-bold">${L.lblPrimaryStore}</span>`
            : L.lblSecondaryStore;

        const name = isSO ? (e.nameSo || e.name) : e.name;
        const membersHtml = employeeListHtml(e.id);

        return `
      <div class="col-sm-12 col-md-12 col-lg-12 col-xl-4">
        <div class="card divShadow--xs divBackground js-store-card" data-id="${e.id}" style="cursor:pointer;">
          <div class="card-body">
            <div class="row">
              <div class="col-md-12">
                <div class="row">
                  <div class="col">
                    <div class="d-sm-flex align-items-center">
                      <div class="avatar mb-2 p-2 lh-1 mb-sm-0 avatar-md rounded-circle ${bgType} me-2">
                        <svg xmlns="http://www.w3.org/2000/svg" class="w-icn text-white" viewBox="0 0 24 24">
                          <path d="M4.2069702,12l5.1464844-5.1464844c0.1871338-0.1937866,0.1871338-0.5009155,0-0.6947021C9.1616211,5.9602051,8.8450928,5.9547119,8.6464844,6.1465454l-5.5,5.5c-0.000061,0-0.0001221,0.000061-0.0001221,0.0001221c-0.1951904,0.1951904-0.1951294,0.5117188,0.0001221,0.7068481l5.5,5.5C8.7401123,17.9474487,8.8673706,18.0001831,9,18c0.1325684,0,0.2597046-0.0526733,0.3533936-0.1464233c0.1953125-0.1952515,0.1953125-0.5118408,0.0001221-0.7070923L4.2069702,12z M20.8534546,11.6465454l-5.5-5.5c-0.1937256-0.1871948-0.5009155-0.1871948-0.6947021,0c-0.1986084,0.1918335-0.2041016,0.5083618-0.0122681,0.7069702L19.7930298,12l-5.1465454,5.1464844c-0.09375,0.09375-0.1464233,0.2208862-0.1464233,0.3534546C14.5,17.776062,14.723877,17.999939,15,18c0.1326294,0.0001221,0.2598267-0.0525513,0.3534546-0.1464844l5.5-5.5c0.000061-0.000061,0.0001221-0.000061,0.0001831-0.0001221C21.0487671,12.1581421,21.0487061,11.8416748,20.8534546,11.6465454z"></path>
                        </svg>
                      </div>
                      <div class="ms-1">
                        <h6 class="mb-1">
                          <a class="float-start text-bold" href="/Users/Stores/StoreDetails/${e.id}">${name}</a>
                        </h6>
                        <span class="text-muted pe-2 fs-11 float-start mt-1">${storeTypeText}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="col-md-12 mt-4">
                <div class="row align-items-center">
                  <div class="col">
                    <p class="m-0 mb-2">${L.lblMembers}</p>
                    <div${sMemberListTourLabel} class="avatar-list avatar-list-stacked">
                      ${membersHtml}
                    </div>
                  </div>
                  <div class="col-auto">
                    <p class="mb-0">
                      <span class="text-muted d-block">${L.lblTotalAssets}</span>
                      <span class="text-primary text-bold float-end">${U.formatInt?.(e.totalCount) ?? String(e.totalCount ?? "")}</span>
                    </p>
                  </div>
                </div>
              </div>

              <div class="col-md-12 mt-4">
                <div class="d-f-ai-c-jc-c">
                  <div class="row wp-100">
                    <div class="col-md-6">
                      <span class="text-muted d-block float-start">${L.lblInitialCost}</span><br />
                      <span class="text-primary text-bold float-start">${U.formatNumber?.(e.totalCost) ?? String(e.totalCost ?? "")}</span>
                    </div>
                    <div class="col-md-6">
                      <span class="text-muted d-block float-end">${L.lblCurrentValue}</span><br />
                      <span class="text-primary text-bold float-end">${U.formatNumber?.(e.depCost) ?? String(e.depCost ?? "")}</span>
                    </div>
                  </div>
                </div>
              </div>

            </div>
          </div>
        </div>
      </div>`;
    }
    // ---------- Helpers ----------
    function employeeListHtml(storeId) {
        if (!Array.isArray(STATE.employees) || STATE.employees.length === 0) {
            return `<span class="avatar bradius bg-primary">0</span>`;
        }

        let out = "";
        STATE.employees.forEach(emp => {
            if (Number(emp.storeId) === Number(storeId)) {
                const title = esc(emp.fullName || emp.name || "Employee");
                const email = esc(emp.email || "");
                const role = esc(emp.role || "");
                const content = `${email}${email && role ? " <br /> " : ""}${role}`;

                // anchor is the popover trigger
                out += `
          <a href="#" class="js-emp-pop"
             data-bs-container="body"
             data-bs-toggle="popover"
             data-bs-html="true"
             data-bs-placement="right"
             data-bs-trigger="hover focus"
             data-bs-popover-color="default"
             data-bs-content="${content}"
             data-bs-original-title="${title}"
             role="button" tabindex="0">
            <span><img src="${emp.imageUrl}" alt="profile-user" class="avatar bradius cover-image"></span>
          </a>`;
            }
        });
        return out || `<span class="avatar bradius bg-primary">0</span>`;
    }
    function esc(s) {
        return String(s)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    window.AMS?.pages?.register && window.AMS.pages.register("Stores/Index", init);
})(window, document, jQuery);