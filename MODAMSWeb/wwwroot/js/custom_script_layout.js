$(document).ready(() => {
    setMode();
    loadProfileData();
    formatTables();

    $("#txtGlobalSearch").on("change", () => {
        window.location.href = "/Users/Home/GlobalSearch/?barcode=" + $("#txtGlobalSearch").val();
    });

    $("#btnGlobalSearch").on("click", () => {
        window.location.href = "/Users/Home/GlobalSearch/?barcode=" + $("#txtGlobalSearch").val();
    });
    loadNotificationsData();
});

const loadProfileData = () => {
    $.ajax({
        url: '/Users/Home/GetProfileData',
        datatype: "json",
        data: {},
        beforeSend: function () {
            // Optional: Add a loading indicator here if needed
        },
        success: function (response) {
            if (response.status == "success") {
                displayProfile(response.data);
            } else {
                showErrorMessageJs(response.message);
            }
        },
        error: function (xhr, status, error) {
            showErrorMessageJs(`Error loading profile data: ${error}`);
        }
    });
}
const displayProfile = (data) => {
    
    const profileImageHtml = `<img src="${data.imageUrl}" alt="profile-user" class="avatar profile-user brround cover-image">`;
    const profileHeadingHtml = `${data.fullName} <i class="user-angle ms-1 fa fa-angle-down"></i>`;

    $("#profileImage").html(profileImageHtml);
    $("#profile-heading").html(profileHeadingHtml);
}

const getAlertCount = () => {
    var postData = {};
    $.ajax({
        url: '/Users/Alerts/GetAlertCount',
        datatype: "json",
        data: postData,
        beforeSend: function () {
        },
        success: function (e) {
            $("#alertCount").html(e);
        },
        complete: function () {
            //loadProfile();
        }
    });
}

const loadNotificationsData = () => {
    var postData = {};
    $.ajax({
        url: '/Users/Home/GetNotifications',
        datatype: "json",
        data: postData,
        beforeSend: function () {
        },
        success: function (response) {
            if (response.status === "success") {
                displayNotifications(response.data);
            } else if (response.status === "empty") {
                $("#notification-pulse").hide();
            }
            else {
                $("#notification-pulse").hide();
                showErrorMessageJs(response.message);
            }
        },
        error: function (xhr, status, error) {
            showErrorMessageJs(`Error loading Notifications data: ${error}`);
        }
    });
};
const displayNotifications = (data) => {

    if (!Array.isArray(data)) {
        showErrorMessageJs('Notifications value is not valid.');
        return;
    }

    let isNewNotification = false;
    let nCounter = 0;
    let sHtml = "";
    let sbgClass = "";
    for (const e of data) {
        nCounter++;
        const sPulse = e.IsViewed ? '' : '<span class="pulse rounded-pill bg-success"></span>';
        if (!e.IsViewed) {
            sbgClass = "bg-light-transparent"
            isNewNotification = true;
        } else {
            sbgClass = "";
        }

        if (nCounter > 4) {
            break; // Exit the loop once nCounter exceeds 4
        }
        sHtml += `
                    <a class="dropdown-item ${sbgClass}" href="/Users/Home/NotificationDirector/${e.id}">
                        <div class="notification-each d-flex ">
                            <div class="avatar-container avatar-md me-2">
                                <img alt="avatar" class="rounded-circle" src="${e.imageUrl}">
                            </div>
                            <div>
                                <span class="notification-label mb-1">${shortenMessage(e.message)}</span>
                                        ${sPulse}
                                <span class="notification-subtext text-muted">${formattedDate(e.dateTime)}</span>
                            </div>
                        </div>
                    </a>`;
    }
    $("#dvNotifications").html(sHtml);
    const notificationPulse = $("#notification-pulse");
    if (nCounter > 0) {
        if (isNewNotification) {
            notificationPulse.show();
        } else {
            notificationPulse.hide();
        }
    } else {
        notificationPulse.hide();
        sHtml = `<a class="dropdown-item" href="#">
                                        <div class="notification-each d-flex">
                                            <div class="me-3 notifyimg brround">
                                            </div>
                                            <div>
                                                <span class="notification-label mb-1">No notification available</span>
                                            </div>
                                        </div>
                                    </a>`;
        $("#dvNotifications").html(sHtml);
    }

}

const clearNotifications = () => {
    var postData = {};
    $.ajax({
        url: '/Users/Home/ClearNotifications',
        datatype: "json",
        data: postData,
        beforeSend: function () {
        },
        success: function (e) {
            $("#dvNotifications").html(`<a class="dropdown-item" href="#">
                                                <div class="notification-each d-flex">
                                                    <div class="me-3 notifyimg brround">
                                                    </div>
                                                    <div>
                                                        <span class="notification-label mb-1">No notification available</span>
                                                    </div>
                                                </div>
                                            </a>`);
        },
        complete: function () {
            //loadNotificationsData();
        }
    });
}
const shortenMessage = (message) => {
    const index = message.indexOf(',');
    if (index !== -1) {
        message = message.substring(0, index);
    }
    return message;
}
