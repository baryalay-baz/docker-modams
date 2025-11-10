// ~/js/pages/layout.js 
(function (w, $) {
    "use strict";

    // --- Namespaces / deps
    const AMS = (w.AMS = w.AMS || {});
    const U = AMS.util || {};
    const B = AMS.bridge || {};

    // Soft asserts (don’t crash if utils missing in some pages)
    if (U.assert) {
        U.assert(!!$, "jQuery missing for layout");
        U.assert(!!U.formatTables, "AMS.util.formatTables not loaded before layout");
    }

    // Unified "ready" helper: prefer U.ready if present, otherwise DOMContentLoaded.
    const onReady = (fn) => {
        if (U.ready && typeof U.ready === "function") return U.ready(fn);
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", fn, { once: true });
        } else {
            fn();
        }
    };

    const SEL = {
        globalSearchInput: "#txtGlobalSearch",
        globalSearchBtn: "#btnGlobalSearch",
        notificationsWrap: "#dvNotifications",
        notificationPulse: "#notification-pulse",
        profileImage: "#profileImage",
        profileHeading: "#profile-heading",
        alertCount: "#alertCount",
    };

    // ===== Init =====
    onReady(() => {
        bindEvents();
        loadProfileData();
        safeFormatTables();
        loadNotificationsData();

        // Mobile sidebar safety
        w.addEventListener("resize", onResize);

        // Feather icons (guard presence)
        if (w.feather && typeof w.feather.replace === "function") {
            w.feather.replace();
        }
    });

    // ===== Events =====
    function bindEvents() {
        $(SEL.globalSearchInput).on("change", navigateGlobalSearch);
        $(SEL.globalSearchBtn).on("click", navigateGlobalSearch);
    }

    function onResize() {
        if (w.innerWidth < 768) {
            document.body.classList.remove("sidenav-toggled");
        }
    }

    function navigateGlobalSearch() {
        const q = ($(SEL.globalSearchInput).val() || "").toString();
        w.location.href = "/Users/Home/GlobalSearch/?barcode=" + encodeURIComponent(q);
    }

    // ===== Profile =====
    function loadProfileData() {
        $.ajax({
            url: "/Users/Home/GetProfileData",
            dataType: "json",
            success: function (response) {
                if (response.status === "success") {
                    displayProfile(response.data);
                } else {
                    U.showErrorMessageJs && U.showErrorMessageJs(response.message);
                }
            },
            error: function (_xhr, _status, error) {
                U.showErrorMessageJs &&
                    U.showErrorMessageJs(`Error loading profile data: ${error}`);
            },
        });
    }

    function displayProfile(data) {
        // Basic HTML-encode for safety
        const esc = (s) =>
            String(s ?? "")
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");

        const imgUrl = esc(data.imageUrl);
        const fullName = esc(data.fullName);

        const profileImageHtml = `<img src="${imgUrl}" alt="profile-user" class="avatar profile-user brround cover-image">`;
        const profileHeadingHtml = `${fullName} <i class="user-angle ms-1 fa fa-angle-down"></i>`;
        $(SEL.profileImage).html(profileImageHtml);
        $(SEL.profileHeading).html(profileHeadingHtml);
    }

    // ===== Alerts =====
    function getAlertCount() {
        $.ajax({
            url: "/Users/Alerts/GetAlertCount",
            dataType: "json",
            success: function (e) {
                $(SEL.alertCount).html(e);
            },
        });
    }

    // ===== Notifications =====
    function loadNotificationsData() {
        $.ajax({
            url: "/Users/Home/GetNotifications",
            dataType: "json",
            success: function (response) {
                if (response.status === "success") {
                    displayNotifications(response.data);
                } else if (response.status === "empty") {
                    $(SEL.notificationPulse).hide();
                    $(SEL.notificationsWrap).html(emptyNotificationsHtml());
                } else {
                    $(SEL.notificationPulse).hide();
                    U.showErrorMessageJs && U.showErrorMessageJs(response.message);
                }
            },
            error: function (_xhr, _status, error) {
                U.showErrorMessageJs &&
                    U.showErrorMessageJs(`Error loading Notifications data: ${error}`);
            },
        });
    }

    function displayNotifications(data) {
        if (!Array.isArray(data)) {
            U.showErrorMessageJs &&
                U.showErrorMessageJs("Notifications value is not valid.");
            return;
        }

        let isNew = false;
        let n = 0;
        let html = "";

        for (const e of data) {
            n++;
            const newBadge = e.isViewed
                ? ""
                : '&nbsp;<span class="badge bg-primary my-1 custom-badge text-white">New</span>';
            if (!e.isViewed) isNew = true;
            if (n > 4) break;

            const when = (U.formattedDate && U.formattedDate(e.dateTime)) || "";

            html += `
        <a class="dropdown-item" href="/Users/Home/NotificationDirector/${e.id}">
          <div class="notification-each d-flex">
            <div>
              <span class="notification-label mb-1">${shortenMessage(
                e.message
            )}</span>
              ${newBadge}
              <span class="notification-subtext text-muted">${when}</span>
            </div>
          </div>
        </a>`;
        }

        if (n > 0) {
            $(SEL.notificationsWrap).html(html);
            isNew ? $(SEL.notificationPulse).show() : $(SEL.notificationPulse).hide();
        } else {
            $(SEL.notificationPulse).hide();
            $(SEL.notificationsWrap).html(emptyNotificationsHtml());
        }
    }

    function emptyNotificationsHtml() {
        // TODO: localize if needed via server or AMS.bridge
        return `
      <a class="dropdown-item" href="#">
        <div class="notification-each d-flex">
          <div class="me-3 notifyimg brround"></div>
          <div class="text-center">
            <span class="notification-label mb-1">No notification available</span>
          </div>
        </div>
      </a>`;
    }

    function clearNotifications() {
        $.ajax({
            url: "/Users/Home/ClearNotifications",
            dataType: "json",
            success: function () {
                $(SEL.notificationsWrap).html(emptyNotificationsHtml());
                $(SEL.notificationPulse).hide();
            },
        });
    }

    function shortenMessage(message) {
        const idx = (message || "").indexOf(",");
        return idx !== -1 ? message.substring(0, idx) : message || "";
    }

    function safeFormatTables() {
        try {
            U.formatTables && U.formatTables();
        } catch (e) {
            console.warn("[layout] formatTables failed:", e);
        }
    }

    // ===== Public API =====
    AMS.layout = {
        refreshProfile: loadProfileData,
        reloadNotifications: loadNotificationsData,
        clearNotifications: clearNotifications,
        getAlertCount: getAlertCount,
    };
})(window, window.jQuery);

// ============================================================================
// Sidebar Popovers: robust init with Bootstrap 5 global, jQuery fallback,
// late DOM mutations support, and zero crashes if Bootstrap is missing.
// ============================================================================
(function initMenuPopovers() {
    "use strict";

    const selector = '.side-menu__item[data-bs-toggle="popover"]';

    // Check which API is available (Bootstrap 5 global vs jQuery plugin vs none)
    function detectPopoverMode() {
        if (window.bootstrap && typeof window.bootstrap.Popover === "function")
            return "bs5";
        if (window.jQuery && typeof window.jQuery.fn.popover === "function")
            return "jq";
        return null;
    }

    // Initialize a single element safely
    function initOne(el, mode) {
        if (!el || el.__popoverInit) return;
        el.__popoverInit = true;

        const opts = {
            container: "body",
            placement: "right",
            trigger: "hover focus",
            html: false,
            delay: { show: 120, hide: 60 },
        };

        if (mode === "bs5") {
            new window.bootstrap.Popover(el, opts);
            return;
        }
        if (mode === "jq") {
            window.jQuery(el).popover(opts);
            return;
        }
    }

    // Run once over current DOM
    function passInit(mode) {
        document.querySelectorAll(selector).forEach((el) => initOne(el, mode));
    }

    // Wait for Bootstrap presence (and handle delayed loads)
    function whenBootstrapReady(cb, tries = 30) {
        const mode = detectPopoverMode();
        if (mode) return cb(mode);
        if (tries <= 0) return cb(null);
        setTimeout(() => whenBootstrapReady(cb, tries - 1), 50);
    }

    // Observe late-added nodes (sidebar scripts that rebuild DOM, SPA nav, etc.)
    function observeMutations(mode) {
        if (!mode) return;
        const mo = new MutationObserver((records) => {
            for (const rec of records) {
                rec.addedNodes &&
                    rec.addedNodes.forEach((n) => {
                        if (n.nodeType !== 1) return;
                        if (n.matches && n.matches(selector)) initOne(n, mode);
                        if (n.querySelectorAll) {
                            n.querySelectorAll(selector).forEach((el) => initOne(el, mode));
                        }
                    });
            }
        });
        mo.observe(document.body, { childList: true, subtree: true });
        // Optional: clean up on unload
        window.addEventListener("beforeunload", () => mo.disconnect(), {
            once: true,
        });
    }

    // Kickoff after DOM ready and after sidemenu likely ran
    const start = () =>
        whenBootstrapReady((mode) => {
            if (!mode) {
                console.warn(
                    "[layout popovers] bootstrap.bundle.min.js (or jQuery plugin) not loaded before init. Skipping."
                );
                return;
            }
            // One late tick so sidemenu.js can finish building
            setTimeout(() => {
                passInit(mode);
                observeMutations(mode);
            }, 50);
        });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start, { once: true });
    } else {
        start();
    }
})();
