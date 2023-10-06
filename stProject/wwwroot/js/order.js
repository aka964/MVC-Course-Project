var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inproccess")) {
        loadDataTable("inproccess");
    } else {
        if (url.includes("complete")) {
            loadDataTable("complete");
        } else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            } else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                } else {
                    loadDataTable("all");
                }
            }
        }
    }
    
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/ordermanagement/getall?status=' + status },
        "columns": [
            { data: 'id', "width": "5%" },
            { data: 'name', "width": "18%" },
            { data: 'phoneNumber', "width": "10%" },
            { data: 'applicationUser.email', "width": "10%" },
            { data: 'orderStatus', "width": "8%" },
            { data: 'orderTotal', "width": "8%" },

            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a href="/admin/ordermanagement/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                        </div>`
                },
                "width": "25%"
            }
        ]
    });
}