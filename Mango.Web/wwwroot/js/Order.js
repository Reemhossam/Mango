﻿var dataTable;
$(document).ready(function () {
    loadDataTable();
});
function loadDataTable() {
    dataTable = $("#tblData").DataTable({
        "order":[[0,"desc"]], // عشان نرتب تنازلي
        "ajax": { url: "/order/GetAll" },
        "columns": [
            { data: 'orderHeaderId', "width": "5%" },
            { data: 'email', "width": "25%" },
            { data: 'name', "width": "15%" },
            { data: 'phone', "width": "15%" },
            { data: 'status', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'orderHeaderId',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2">
                    <i class="bi bi-pencil-square"></i>
                    </a>
                    </div>`
                },
                "width":"10%"
            }
        ]
    });
}