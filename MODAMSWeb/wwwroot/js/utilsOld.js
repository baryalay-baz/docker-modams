// Parse JSON with error handling
function tryParseJson(jsonString, identifier) {
    try {
        const parsed = JSON.parse(jsonString);
        return { status: 'success', data: parsed };
    } catch (error) {
        return { status: 'error', message: `${identifier} data is not in valid JSON format.` };
    }
}

// Apply styling to tables
const formatTables = () => {
    $('thead').addClass("bg-info-gradient ms-auto divShadow");
    $('th').addClass("text-white");
};

// Toggle dark/light mode
const setMode = () => {
    const isDark = localStorage.getItem("displayMode") === "dark-mode";
    $('body').toggleClass('dark-mode', isDark);
    $('body').toggleClass('light-mode', !isDark);

    const footerImg = isDark
        ? '/assets/images/brand/EU_Horizontal3_small.png'
        : '/assets/images/brand/EU_Horizontal2_small.png';
    $("#footerImgEU").html(`<img src="${footerImg}">`);

    localStorage.setItem(isDark ? 'darkMode' : 'lightMode', true);
    localStorage.removeItem(isDark ? 'lightMode' : 'darkMode');
    localStorage.setItem('displayMode', isDark ? 'dark-mode' : 'light-mode');
};

// Detect current language from ASP.NET Core localization cookie
function getCurrentLanguage() {
    const cookieName = '.AspNetCore.Culture';
    const cookieValue = document.cookie
        .split('; ')
        .find(row => row.startsWith(cookieName + '='));

    if (cookieValue) {
        const encodedValue = cookieValue.split('=')[1];
        const decodedValue = decodeURIComponent(encodedValue);
        const match = decodedValue.match(/c=(\w+)/);
        return match ? match[1] : 'en';
    }
    return 'en';
}

// Return DataTable language config for given language
const getDataTableLanguageOptions = (languageCode) => {
    if (languageCode === 'so') {
        return {
            processing: "Fadlan sug...",
            lengthMenu: "_MENU_",
            zeroRecords: "Ma jiro wax rikoor ah oo la helay",
            info: "Muujinaya _START_ ilaa _END_ ee _TOTAL_ rikoorrada",
            infoEmpty: "Muujinaya 0 ilaa 0 ee 0 rikoorrada",
            infoFiltered: "(la sifeynayo _MAX_ rikoorrada)",
            infoPostFix: "",
            search: "",
            searchPlaceholder: "Raadi...",
            url: "",
            emptyTable: "Ma jiraan wax xog ah oo la heli karo",
            loadingRecords: "Loading...",
            infoThousands: ",",
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
    // Default (English)
    return {
        searchPlaceholder: 'Search...',
        sSearch: ''
    };
};

// Initialize DataTable with optional language and button settings
const makeDataTable = (tableName, type = "1", recordsPerPage = 10) => {
    const currentLanguage = getCurrentLanguage();
    const languageOptions = getDataTableLanguageOptions(currentLanguage);

    const table = $(tableName).DataTable({
        buttons: ['copy', 'excel', 'pdf', 'colvis'],
        responsive: false,
        pageLength: recordsPerPage,
        language: languageOptions,
        initComplete: () => styleDataTableButtonsAndPagination(tableName)
    });

    applyRowStyles();
    

    table.on('draw.dt', () => applyRowStyles());

    if (type === "2") {
        table.buttons().container().appendTo(`${tableName}_wrapper .col-md-6:eq(0)`);
    } else if (type === "3") {
        table.buttons().container().hide();
    }
    //styleDataTableButtonsAndPagination(tableName);
    return table;
};

// Style DataTable buttons & pagination
const styleDataTableButtonsAndPagination = (tableName) => {
    $(`${tableName}_wrapper .btn-primary`)
        .removeClass('btn-primary')
        .addClass('bg-info')
        .css('color', 'white');

    $(`${tableName}_wrapper .paginate_button`)
        .removeClass('btn-primary')
        .addClass('btn-info')
        .css('color', 'white');
};

// Style even/odd rows
const applyRowStyles = () => {
    $('.even').addClass("bg-light-transparent");
    $('.odd').removeClass("bg-light-transparent");
};

// Format date as dd-MMM-yyyy
const formattedDate = (date) => moment(date).format('DD-MMM-YYYY');

// Simulate hiding a menu
const hideMenu = () => {
    if (document.body.classList.contains('sidenav-toggled')) {
        document.body.classList.remove('sidenav-toggled'); // close
    } else {
        document.body.classList.add('sidenav-toggled'); // open
    }
};

// Notification wrapper
const Notify = (type, message) => {
    const notifType = type === "success" ? "primary" : "error";
    const notifTitle = type === "success" ? "Success" : "Error";

    notif({
        type: notifType,
        msg: `<b>${notifTitle}: </b> ${message}`,
        position: "center",
        width: 500,
        height: 60,
        autohide: true
    });
};

// Show error message inside a notification container
const showErrorMessageJs = (errorMessage) => {
    const isSomali = getCurrentLanguage() === "so";
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

// Show success message inside a notification container
const showSuccessMessageJs = (successMessage) => {
    const isSomali = getCurrentLanguage()==="so";

    const sHtml = `
        <div class="notification-container">
            <div class="alert alert-success alert-dismissible fade show p-0 mb-4 notification-message" role="alert">
                <p class="py-3 px-5 mb-0 border-bottom border-bottom-success-light">
                    <span class="alert-inner--icon me-2"><i class="fe fe-thumbs-up"></i></span>
                    <strong>${isSomali ? "Fariinta Guusha" :"Success Message"}</strong>
                </p>
                <p class="py-3 px-5">${successMessage}</p>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">×</span>
                </button>
            </div>
        </div>`;
    $(".js-notification").html(sHtml);
};
