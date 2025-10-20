// wwwroot/js/layout.js

(function (w, $) {
    "use strict";

    // --- Dependencies / aliases
    const U = (w.PAMS && w.PAMS.util) || {};
    const B = (w.PAMS && w.PAMS.bridge) || {};

   
    U.assert && U.assert(!!$, "jQuery missing for layout");
    U.assert && U.assert(!!U.formatTables, "PAMS.util not loaded before layout");

    const SEL = {
        globalSearchInput: "#txtGlobalSearch",
        globalSearchBtn: "#btnGlobalSearch",
        notificationsWrap: "#dvNotifications",
        notificationPulse: "#notification-pulse",
        profileImage: "#profileImage",
        profileHeading: "#profile-heading",
        alertCount: "#alertCount"
    };

    // --- Init on DOM ready (use utils ready)
    U.ready(() => {
        // U.setMode?.(); // enable if you want theme switch on page load
        bindEvents();
        loadProfileData();
        U.formatTables();
        loadNotificationsData();

        // Mobile sidebar safety
        w.addEventListener("resize", onResize);

        // Feather icons (guard presence)
        if (w.feather && typeof w.feather.replace === "function") {
            w.feather.replace();
        }
    });

    // --- Event wiring
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
        const q = $(SEL.globalSearchInput).val() || "";
        w.location.href = "/Users/Home/GlobalSearch/?barcode=" + encodeURIComponent(q);
    }

    // --- Profile
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
                U.showErrorMessageJs && U.showErrorMessageJs(`Error loading profile data: ${error}`);
            }
        });
    }
    function displayProfile(data) {
        const profileImageHtml = `<img src="${data.imageUrl}" alt="profile-user" class="avatar profile-user brround cover-image">`;
        const profileHeadingHtml = `${data.fullName} <i class="user-angle ms-1 fa fa-angle-down"></i>`;
        $(SEL.profileImage).html(profileImageHtml);
        $(SEL.profileHeading).html(profileHeadingHtml);
    }

    // --- Alerts (kept private, expose on API if you want it)
    function getAlertCount() {
        $.ajax({
            url: "/Users/Alerts/GetAlertCount",
            dataType: "json",
            success: function (e) { $(SEL.alertCount).html(e); }
        });
    }

    // --- Notifications
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
                U.showErrorMessageJs && U.showErrorMessageJs(`Error loading Notifications data: ${error}`);
            }
        });
    }
    function displayNotifications(data) {
        if (!Array.isArray(data)) {
            U.showErrorMessageJs && U.showErrorMessageJs("Notifications value is not valid.");
            return;
        }

        let isNew = false;
        let n = 0;
        let html = "";

        for (const e of data) {
            n++;
            const newBadge = e.isViewed ? "" : '&nbsp;<span class="badge bg-primary my-1 custom-badge text-white">New</span>';
            if (!e.isViewed) isNew = true;
            if (n > 4) break;

            const when = (U.formattedDate && U.formattedDate(e.dateTime)) || "";

            html += `
        <a class="dropdown-item" href="/Users/Home/NotificationDirector/${e.id}">
          <div class="notification-each d-flex">
            <div>
              <span class="notification-label mb-1">${shortenMessage(e.message)}</span>
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
            }
        });
    }
    function shortenMessage(message) {
        const idx = (message || "").indexOf(",");
        return idx !== -1 ? message.substring(0, idx) : (message || "");
    }

    // --- Minimal public API (for debugging / reuse)
    w.PAMS = w.PAMS || {};
    w.PAMS.layout = {
        refreshProfile: loadProfileData,
        reloadNotifications: loadNotificationsData,
        clearNotifications: clearNotifications,
        getAlertCount: getAlertCount
    };

})(window, window.jQuery);
