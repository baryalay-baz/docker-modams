$(document).ready(() => {
    //$("#btnCards").on("click", () => {
    //    $("#btnCards").removeClass("btn-outline-primary").removeClass("btn-primary").addClass("btn-primary");
    //    $("#btnList").removeClass("btn-primary").removeClass("btn-outline-primary").addClass("btn-outline-primary");
    //    ("#txtSearchStore").val("a");
    //    searchStore();
    //});
    //$("#btnList").on("click", () => {
    //    $("#btnCards").removeClass("btn-outline-primary").removeClass("btn-primary").addClass("btn-outline-primary");
    //    $("#btnList").removeClass("btn-primary").removeClass("btn-outline-primary").addClass("btn-primary");
    //    storeList();
    //});
    $("#txtSearchStore").on("input", () => {
        searchStore();
    });
});
const storeList = () => {
    var sData = $("#dvStoresData").html();
    if (sData != "No Records Found!") {
        var Data = JSON.parse(sData);
        var nCounter = 0;
        var sCounter = '';
        console.log(Data);
        var sHtml = `<div class="table-responsive">
            <table id="tblStores" class="table text-nowrap mb-0 table-bordered border-top border-bottom project-list-main">
                <thead class="bg-primary-gradient ms-auto divShadow">
                    <tr>
                        <th class="text-white bg-transparent border-bottom-0 w-5" width="10%">S.No</th>
                        <th class="text-white bg-transparent border-bottom-0">Store Name</th>
                        <th class="text-white bg-transparent border-bottom-0">Store Owner</th>
                        <th class="text-white bg-transparent border-bottom-0">Total Cost</th>
                        <th class="text-white bg-transparent border-bottom-0">Current Value</th>
                        <th class="text-white bg-transparent border-bottom-0 no-btn">Action</th>
                    </tr>
                </thead>
                <tbody class="table-body">`
        Data.forEach((e) => {
            sCounter = (nCounter < 10) ? '0' + nCounter.toString() : nCounter.toString();

            sHtml += `<tr>
                    <td class="text-muted text-center fs-15 fw-semibold">`+ sCounter + `.</td>
                    <td class="text-muted fs-15 fw-semibold">` + e.name + `</td>
                    <td class="text-muted fs-15 fw-semibold">` + e.storeOwner + `</td>
                    <td class="text-muted fs-15 fw-semibold">` + e.totalCost + `</td>
                    <td class="text-muted fs-15 fw-semibold">` + e.depCost + `</td>
                    <td class="text-muted fs-15 fw-semibold"><a href="/StoreDetails/` + e.id + `" class="btn btn-outline-link">Details</a></td>
                    </tr>`;
        });
        sHtml += `</tbody></table></div>`;
        $("#dvCardBody").html("").html(sHtml);
        $('#tblStores').DataTable({
            language: {
                searchPlaceholder: 'Search...',
                sSearch: '',
            }
        });
    }
}
const getEmployeelist = (id) => {
    var sHtml = '';
    var sData = $("#dvStoreEmployeesData").html();
    if (sData != [] || sData != "") {
        Data = JSON.parse(sData);
        if (Data.length > 0) {
            Data.forEach((e) => {
                if (e.storeId == id) {
                    sHtml += '<span><img src="' + e.imageUrl + '" alt="profile-user" class="avatar bradius cover-image"></span>';
                }
            });
        }
        if (sHtml == '')
            sHtml += '<span class="avatar bradius bg-primary">0</span>';
    }
    return sHtml;
}
const searchStore = () => {
    var sData = $("#dvStoresData").html();
    var Data = JSON.parse(sData);

    var searchKeyword = $("#txtSearchStore").val();
    var filteredData = $.grep(Data, function (item) {
        var regex = new RegExp(searchKeyword, "i");
        return regex.test(item.name);
    });
    if (filteredData.length > 0) {
        var sHtml = '';
        var bgType = '';
        var sStoreType = '';
        filteredData.forEach((e) => {
            if (e.storeType == 1) {
                bgType = 'bg-primary';
                sStoreType = '<span class="text-primary text-bold">Primary Store</span>';
            } else {
                bgType = 'bg-info-transparent';
                sStoreType = "Secondary Store";
            }
            var sEmployeeHtml = getEmployeelist(e.id);

            sHtml += `<div class="col-sm-12 col-md-12 col-lg-12 col-xl-4">
                        <div class="card divShadow">
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-md-12">
                                        <div class="row">
                                            <div class="col">
                                                <div class="d-sm-flex align-items-center">
                                                    <div class="avatar mb-2 p-2 lh-1 mb-sm-0 avatar-md rounded-circle ` + bgType + ` me-2">
                                                        <svg xmlns="http://www.w3.org/2000/svg" class="w-icn text-white" enable-background="new 0 0 24 24" viewBox="0 0 24 24"><path d="M4.2069702,12l5.1464844-5.1464844c0.1871338-0.1937866,0.1871338-0.5009155,0-0.6947021C9.1616211,5.9602051,8.8450928,5.9547119,8.6464844,6.1465454l-5.5,5.5c-0.000061,0-0.0001221,0.000061-0.0001221,0.0001221c-0.1951904,0.1951904-0.1951294,0.5117188,0.0001221,0.7068481l5.5,5.5C8.7401123,17.9474487,8.8673706,18.0001831,9,18c0.1325684,0,0.2597046-0.0526733,0.3533936-0.1464233c0.1953125-0.1952515,0.1953125-0.5118408,0.0001221-0.7070923L4.2069702,12z M20.8534546,11.6465454l-5.5-5.5c-0.1937256-0.1871948-0.5009155-0.1871948-0.6947021,0c-0.1986084,0.1918335-0.2041016,0.5083618-0.0122681,0.7069702L19.7930298,12l-5.1465454,5.1464844c-0.09375,0.09375-0.1464233,0.2208862-0.1464233,0.3534546C14.5,17.776062,14.723877,17.999939,15,18c0.1326294,0.0001221,0.2598267-0.0525513,0.3534546-0.1464844l5.5-5.5c0.000061-0.000061,0.0001221-0.000061,0.0001831-0.0001221C21.0487671,12.1581421,21.0487061,11.8416748,20.8534546,11.6465454z"></path></svg>
                                                    </div>
                                                    <div class="ms-1">
                                                                        <h6 class="mb-1"> <a class="float-start text-bold" href="/Users/Stores/StoreDetails/` + e.id + `">` + e.name + `</a></h6>
                                                        <span class="text-muted pe-2 fs-11 float-start mt-1">` + sStoreType + `</span>
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                    <div class="col-md-12 mt-4">
                                        <div class="row align-items-center">
                                            <div class="col">
                                                <p class="m-0 mb-2">Members</p>
                                                <div class="avatar-list avatar-list-stacked">` + sEmployeeHtml + `</div>
                                            </div>
                                            <div class="col-auto">
                                                <p class="mb-0">
                                                    <span class="text-muted d-block">Total Assets</span>
                                                    <span class="text-primary text-bold float-end">` + e.totalCount + `</span>
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-12 mt-4">
                                        <div class="d-f-ai-c-jc-c">
                                            <div class="row wp-100">
                                                <div class="col-md-6">
                                                    <span class="text-muted d-block float-start">Initial Cost</span><br />
                                                            <span class="text-primary text-bold float-start">` + Math.round(e.totalCost, 2).toLocaleString() + `</span>
                                                </div>
                                                <div class="col-md-6">
                                                    <span class="text-muted d-block float-end">Current Value</span><br />
                                                    <span class="text-primary text-bold float-end">` + Math.round(e.depCost, 2).toLocaleString() + `</span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>`;
        });
        $("#dvMainRow").html("").html(sHtml);
    }
}