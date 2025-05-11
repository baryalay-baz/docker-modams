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
        makeDataTable("#tblStores", "1");

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

