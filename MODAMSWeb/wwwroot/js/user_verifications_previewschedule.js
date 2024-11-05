//Script for Verifications/PreviewSchedule.cshtml
//Developed by Baryalay Baz
//5-Oct-2024
var currentPage_tblAssets = 0;

const loadBarchart = (data) => {

    var results = data.map(x => x.result);
    var recordCounts = data.map(x => x.verificationRecordCount);

    var barColors = [getRandomColor(), getRandomColor(), getRandomColor(), getRandomColor(), getRandomColor()];

    var myChart = echarts.init(document.getElementById('verificationChart'));

    var option = {
        title: {
            text: ''
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'shadow'
            }
        },
        grid: {
            left: '30%',
        },
        xAxis: {
            type: 'value',
            minInterval: 1
        },
        yAxis: {
            type: 'category',
            data: results,
            axisLabel: {
                fontSize: 12,
                interval: 0, 
                formatter: function (value) {
                    return value;
                }
            }
        },
        series: [{
            data: recordCounts.map((count, index) => ({
                value: count,
                itemStyle: {
                    color: barColors[index % barColors.length]  // Assign a different color from the array
                }
            })),
            type: 'bar',
            label: {
                show: true,        
                position: 'right', 
                formatter: '{c}'   
            }
        }]
    };

    myChart.setOption(option);

    myChart.on('click', function (params) {
        let clickedResult = params.name;
        handleBarClick(clickedResult);
    });
};
const loadProgressChart = (data) => {
    data = JSON.parse(data); // Make sure the data is parsed from JSON

    var chart = echarts.init(document.getElementById('progressChart'));

    var dates = data.map(item => item.formattedDate);

    var planProgress = data.map(item => item.planProgress); 
    var achievedProgress = data.map(item => item.progress); 

    var option = {
        title: {
            text: 'Planned vs. Achieved Progress'
        },
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            data: ['Planned Progress', 'Achieved Progress']
        },
        xAxis: {
            type: 'category',
            boundaryGap: false,
            data: dates 
        },
        yAxis: {
            type: 'value'
        },
        series: [
            {
                name: 'Planned Progress',
                type: 'line',
                data: planProgress,
                smooth: true, 
                lineStyle: {
                    width: 2
                }
            },
            {
                name: 'Achieved Progress',
                type: 'line',
                data: achievedProgress,
                smooth: true,
                lineStyle: {
                    width: 2
                },
                label: {
                    show: true,
                    position: 'top', 
                    formatter: function (params) {
                        return params.data !== null ? params.data : '';
                    }
                }
            }
        ]
    };
    chart.setOption(option);
}
const getRandomColor = () => {
    const moderateLightColorPalette = [
        '#A5D6A7', '#90CAF9', '#F48FB1', '#FFCC80', '#CE93D8', '#FFF59D', '#80CBC4',
        '#FFAB91', '#81D4FA', '#B39DDB', '#FFE082', '#FFCDD2'];

    const randomIndex = Math.floor(Math.random() * moderateLightColorPalette.length);
    return moderateLightColorPalette[randomIndex];
};
const handleBarClick = (clickedResult) => {
    $('#mdlVerified').modal('show');
    loadVerifiedAssetsTable(clickedResult);
};
const loadVerifiedAssetsTable = (result) => {
    let sData = $("#dvAssetsData").html();

    var Data = JSON.parse(sData);
    if (Data != null) {
        let sHtml = '';
        let nCounter = 0;

        let filteredData = Data.filter(e => e.result === result && e.verifiedBy !== "Not Verified")

        filteredData.forEach(e => {
            nCounter++;

            var sVerifyText = '<i class="fa fa-check-circle" style="font-size: 1rem; color: green;"></i> ' + e.verifiedBy;
            sHtml += `
                        <tr>
                            <td class="text-black bg-transparent border-bottom-0 w-2">${nCounter}</td>
                            <td class="text-black bg-transparent border-bottom-0 w-10">${e.make}</td>
                            <td class="text-black bg-transparent border-bottom-0 w-10">${e.model}</td>
                            <td class="text-black bg-transparent border-bottom-0 w-10">${e.name}</td>
                            <td class="text-black bg-transparent border-bottom-0 w-10">${e.serialNo}</td>
                            <td class="text-black bg-transparent border-bottom-0 w-10">${e.result}</td>
                            <td class="text-black bg-transparent border-bottom-0 w-10">${sVerifyText}</td>
                        </tr>
                    `;
        });

        if ($.fn.DataTable.isDataTable('#tblVerifiedAssets')) {
            $('#tblVerifiedAssets').DataTable().clear().destroy();
        }

        $("#table-body-verified").html("").html(sHtml);
        makeDataTable("#tblVerifiedAssets", "1");
        $("#lblTitle").text("").text(result);

    }
};
const submitVerificationForm = () => {
    var formData = new FormData($("#frmVerify")[0]);

    if ($.fn.DataTable.isDataTable("#tblAssets")) {
        currentPage_tblAssets = $('#tblAssets').DataTable().page();
    }

    $.ajax({
        url: '/Users/Verifications/VerifyAsset',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        beforeSend: function () {
            $("#verificationOverlay").show();
            //$("#frmVerify").find("input, select, button").prop("disabled", true);
        },
        success: function (response) {
            if (response.success) {
                showSuccessMessage(response.message);
            } else {
                showErrorMessage("Verification failed: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            if (xhr.status === 403) {
                showErrorMessage("You do not have permission to perform this action.");
            } else {
                var errorMessage = "An error occurred: Status - " + xhr.status + ", " + status + ", Error - " + error;
                if (xhr.responseText) {
                    errorMessage += "\nServer Response: " + xhr.responseText;
                }
                showErrorMessage(errorMessage);
            }
        },
        complete: function () {
            // Hide the overlay and spinner
            //$("#verificationOverlay").hide();
            //$("#frmVerify").find("input, select, button").prop("disabled", false);
            loadAssetsData();
            clearForm();
        }
    });
};
const isFormValid = () => {
    let isValid = true;

    $("span.text-danger").text("");
    if ($("#VerificationRecord_AssetId").val() == "0") {
        $("#spnAssetId").text("Please select an asset to verify!");
        isValid = false;
    }

    if ($("#VerificationRecord_Result").val() === "-1" || $("#VerificationRecord_Result").val() === null) {
        $("#spnResult").text("Please select a result option!");
        isValid = false;
    }
    if ($("#VerificationRecord_Comments").val().trim() === "") {
        $("#spnComments").text("Please provide verification comments!");
        isValid = false;
    }
    if ($("#file").val() === "") {
        $("#spnFile").text("Please upload a recent picture of the asset!");
        isValid = false;
    }

    return isValid;
};
const updateAssetsTable = (verificationRecordId) => {
    var assetId = $("#tempAssetId").val();
    var tdSelector = "#td-" + assetId;
    sButton = `<i class="fa fa-check-circle text-success delete-icon" style="font-size: 1rem;" data-id="${verificationRecordId}" data-assetid="${assetId}"></i> Verified`;
    $(tdSelector).html("").html(sButton);
    bindHoverAndClickEvents();
};
const showModal = () => {
    $('#mdlVerifyAssets').modal('show');
};
const selectRecord = (id) => {
    $("#tblAssets tbody tr").removeClass("bg-light-transparent text-bold");

    const trSelector = "#tr-" + id;

    $(trSelector).addClass("bg-light-transparent text-bold");
    $("#VerificationRecord_AssetId").val(id);

    const assetName = $(trSelector).find('td:nth-child(3)').text() + " - " + $(trSelector).find('td:nth-child(4)').text();

    $("#AssetName").val(assetName);
    $("#VerificationRecord_AssetId").val("0").val(id);
    $("#tempAssetId").val("0").val(id);
};
const clearForm = () => {
    //$("#VerificationRecord_AssetId").val(0);
    $("#AssetName").val("-select an asset-");
    $("#VerificationRecord_Result").val(null).trigger('change'); // Reset select2
    $("textarea[asp-for='VerificationRecord.Comments']").val("");
    $(".validation").text("");
    $("#frmVerify")[0].reset(); // Reset form
};
const showErrorMessage = (errorMessage) => {
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

    $(".modal-notification").html("").html(sHtml);
};
const showSuccessMessage = (successMessage) => {
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
    $(".modal-notification").html("").html(sHtml);
};
const refreshPage = () => {
    location.reload(true);
}
const loadAssetsData = () => {
    let scheduleId = $("#VerificationRecord_VerificationScheduleId").val();

    var postData = { id: scheduleId };

    $.ajax({
        url: '/Users/Verifications/GetScheduleAssets',
        datatype: "json",
        data: postData,
        beforeSend: function () {
            $("#verificationOverlay").show();
        },
        success: function (response) {
            if (response.success) {
                // Directly pass the assets data to loadAssets
                loadAssets(JSON.stringify(response.assets));
            } else {
                showErrorMessage("Error: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = "An error occurred: Status - " + xhr.status + ", " + status + ", Error - " + error;
            if (xhr.responseText) {
                errorMessage += "\nServer Response: " + xhr.responseText;
            }
            showErrorMessage(errorMessage);
        },
        complete: function () {
            $("#verificationOverlay").hide();
        }
    });
};
const loadAssets = (data) => {
    var Data = JSON.parse(data);
    var sHtml = '';

    let assetsToVerify = $('#txtNumberOfAssetsToVerify').val();
    let verifiedCount = 0;
    if ($.fn.DataTable.isDataTable("#tblAssets")) {
        $("#tblAssets").DataTable().clear().destroy(); // Destroy the existing table before rebuilding it
    }

    if (Data != null) {
        Data.forEach(e => {
            if (e.isSelected) {
                sButton = `<i class="fa fa-check-circle text-success delete-icon" id="icon-${e.id}" style="font-size: 1rem;" data-id="${e.verificationRecordId}" data-assetId="${e.id}"></i> Verified`;
                verifiedCount++;
            } else {
                sButton = `<button class="btn btn-outline-info btn-sm" onclick="return selectRecord(${e.id});">Select</button>`;
            }
            sHtml += `
                <tr id="tr-${e.id}">
                    <td id="td-${e.id}" class="text-black bg-transparent border-bottom-0 w-2">${sButton}</td>
                    <td class="text-black bg-transparent border-bottom-0 w-10">${e.model}</td>
                    <td class="text-black bg-transparent border-bottom-0 w-10">${e.name}</td>
                    <td class="text-black bg-transparent border-bottom-0 w-30">${e.serialNo}</td>
                    <td class="text-black bg-transparent border-bottom-0 w-10">${e.barcode}</td>
                </tr>
            `;
        });
        $("#table-body").html("").html(sHtml);

    }

    bindHoverAndClickEvents(); // Add hover and click event handling after the table is generated


    var table = makeDataTable("#tblAssets", "1", 5);

    if (table) {
        table.page(currentPage_tblAssets).draw(false); // Restore the DataTable to the previously stored page
    } else {
        console.error('DataTable is not initialized correctly.');
    }
    if (assetsToVerify == verifiedCount) {
        $("#btnVerifyAsset").addClass("disabled");
    } else {
        $("#btnVerifyAsset").removeClass("disabled");
    }
};
const bindHoverAndClickEvents = () => {
    
    $(".delete-icon").hover(
        function () {
            
            $(this).removeClass("fa-check-circle text-success").addClass("fa-trash text-secondary").css({
                "cursor": "pointer"
            });
        },
        function () {
            $(this).removeClass("fa-trash text-secondary").addClass("fa-check-circle text-success").css({
                "color": "green",
                "cursor": "default"
            });
        }
    );

    $(".delete-icon").on("click", function () {
        var verificationRecordId = $(this).data("id");
        var assetId = $(this).data("assetid");

        if (confirm("Are you sure you want to delete this Verification Record?")) {
            deleteVerificationRecord(verificationRecordId, assetId);
        }
    });
};
const deleteVerificationRecord = (verificationRecordId, assetId) => {
    let sButton = `<button class="btn btn-outline-info btn-sm" onclick="return selectRecord(${assetId});">Select</button>`;
    let sTd = "#td-" + assetId;

    $.ajax({
        url: '/Users/Verifications/DeleteVerificationRecord',
        type: 'POST',
        data: { id: verificationRecordId },
        beforeSend: function () {
            $("#verificationOverlay").show();
        },
        success: function (response) {
            if (response.success) {
                // change the deleted row from the table                
                $(sTd).html("").html(sButton);
                showSuccessMessage("Verification Record deleted successfully.");
            } else {
                showErrorMessage("Failed to delete record: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            if (xhr.status === 403) {
                showErrorMessage("You do not have permission to perform this action.");
            } else {
                var errorMessage = "An error occurred: Status - " + xhr.status + ", " + status + ", Error - " + error;
                if (xhr.responseText) {
                    errorMessage += "\nServer Response: " + xhr.responseText;
                }
                showErrorMessage(errorMessage);
            }
        },
        complete: function () {
            // Hide the overlay and spinner
            $("#verificationOverlay").hide();
            $("#btnVerifyAsset").removeClass("disabled");
            //$("#frmVerify").find("input, select, button").prop("disabled", false);
        }
    });
};
const completeSchedule = () => {
    var scheduleId = $("#Id").val();
    var postData = { id: scheduleId };
    $.ajax({
        url: '/Users/Verifications/CompleteVerificationSchedule',
        datatype: "json",
        data: postData,
        beforeSend: function () {
            $("#verificationOverlay").show();
        },
        success: function (response) {
            if (response.result == true) {
                showSuccessMessage("Schedule successfuly set to completed!");
            } else {
                showErrorMessage("Failed to delete record: " + response.message);
            }
        },
        error: function (xhr, status, error) {
            if (xhr.status === 403) {
                showErrorMessage("You do not have permission to perform this action.");
            } else {
                var errorMessage = "An error occurred: Status - " + xhr.status + ", " + status + ", Error - " + error;
                if (xhr.responseText) {
                    errorMessage += "\nServer Response: " + xhr.responseText;
                }
                showErrorMessage(errorMessage);
            }
        },
        complete: function () {
            //refreshPage();
        }
    });
};