function tryParseJson(jsonString, identifier) {
    try {
        const parsed = JSON.parse(jsonString);
        return { status: 'success', data: parsed};        
    }
    catch (error) {
        return { status: 'error', message: `${identifier} data is not in valid JSON format.` };
    }
}

const formatTables = () => {

    $('thead').addClass("bg-info-gradient ms-auto divShadow");
    $('th').addClass("text-white");

    //$('.even').addClass("bg-light-transparent");
    //$('.odd').removeClass("bg-light-transparent");
}
const setMode = () => {
    if (localStorage.getItem("displayMode") == "dark-mode") {
        $('body').addClass('dark-mode');
        $('body').removeClass('light-mode');
        //$('body').removeClass('bg-img1');
        //$('body').removeClass('bg-img2');
        //$('body').removeClass('bg-img3');
        //$('body').removeClass('bg-img4');
        $("#footerImgEU").html("").html('<img src="/assets/images/brand/EU_Horizontal3_small.png">');


        localStorage.setItem('darkMode', true);
        localStorage.removeItem('lightMode');
        localStorage.setItem('displayMode', "dark-mode")
    } else {
        $('body').removeClass('dark-mode');
        $('body').addClass('light-mode');
        //$('body').removeClass('bg-img1');
        //$('body').removeClass('bg-img2');
        //$('body').removeClass('bg-img3');
        //$('body').removeClass('bg-img4');
        $("#footerImgEU").html("").html('<img src="/assets/images/brand/EU_Horizontal2_small.png">');

        localStorage.setItem('lightMode', true);
        localStorage.removeItem('darkMode');

    }
}
const makeDataTable = (tableName, type = "1", recordsPerPage = 10) => {
    // Initialize the DataTable
    var table = $(tableName).DataTable({
        buttons: ['copy', 'excel', 'pdf', 'colvis'],
        responsive: false,
        pageLength: recordsPerPage, // Set the number of records per page
        language: {
            searchPlaceholder: 'Search...',
            sSearch: '',
        },
        initComplete: function () {
            // Apply custom button and pagination styles
            styleDataTableButtonsAndPagination(tableName);
        }
    });

    // Apply row background styles
    applyRowStyles();

    // Event listeners for table redraw (on pagination and ordering)
    table.on('page.dt order.dt', function () {
        applyRowStyles();
    });

    // Handle additional button settings based on type
    if (type == "2") {
        table.buttons().container()
            .appendTo(tableName + '_wrapper .col-md-6:eq(0)');
    } else if (type == "3") {
        table.buttons().container().hide();
    }

    // Return the initialized table for further use
    return table;
};

// Helper function to style buttons and pagination
const styleDataTableButtonsAndPagination = (tableName) => {
    // Change color of specific buttons
    $(tableName + '_wrapper .btn-primary')
        .removeClass('btn-primary')
        .addClass('bg-info')
        .css('color', 'white');

    // Change color of pagination buttons
    $(tableName + '_wrapper .paginate_button')
        .removeClass('btn-primary')
        .addClass('bg-info')
        .css('color', 'white');
};
// Helper function to style row backgrounds
const applyRowStyles = () => {
    $('.even').addClass("bg-light-transparent");
    $('.odd').removeClass("bg-light-transparent");
};

const formattedDate = (date) => {
    var dateObject = moment(date);

    // Format the date to dd-MMMM-yyyy
    var formattedDate = dateObject.format('DD-MMM-YYYY');
    return formattedDate;
}
const hideMenu = () => {
    $("#hideMenu").click();
}
const Notify = (type, message) => {
    if (type == "success") {
        notif({
            type: "primary",
            msg: "<b>Success: </b> " + message,
            position: "center",
            width: 500,
            height: 60,
            autohide: true
        });
    } else {
        notif({
            type: "error",
            msg: "<b>Error: </b> " + message,
            position: "center",
            width: 500,
            height: 60,
            autohide: true
        });
    }
}

const showErrorMessageJs = (errorMessage) => {
    var sHtml = `
                <div class="notification-container">
                    <div class="alert alert-danger alert-dismissible fade show p-0 mb-0" role="alert">
                        <p class="py-3 px-5 mb-0 border-bottom border-bottom-danger-light">
                            <span class="alert-inner--icon me-2"><i class="fe fe-slash"></i></span>
                            <strong>Error Message</strong>
                        </p>
                        <p class="py-3 px-5">${errorMessage}</p>
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
                            <span aria-hidden="true">×</span>
                        </button>
                    </div>
                </div>`;

    $(".js-notification").html("").html(sHtml);
};
const showSuccessMessageJs = (successMessage) => {
    var sHtml = `
                <div class="notification-container">
                    <div class="alert alert-success alert-dismissible fade show p-0 mb-4 notification-message" role="alert">
                        <p class="py-3 px-5 mb-0 border-bottom border-bottom-success-light">
                            <span class="alert-inner--icon me-2"><i class="fe fe-thumbs-up"></i></span>
                            <strong>Success Message</strong>
                        </p>
                        <p class="py-3 px-5">${successMessage}</p>
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
                            <span aria-hidden="true">×</span>
                        </button>
                    </div>
                </div>`;
    $(".js-notification").html("").html(sHtml);
};