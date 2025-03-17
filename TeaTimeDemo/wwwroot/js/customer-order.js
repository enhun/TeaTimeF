var dataTable;

document.addEventListener('DOMContentLoaded', function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Customer/Order/GetAll",
            "dataSrc": function (json) {
                return json.data;
            }
        },
        "columns": [
            { "data": "id", "width": "10%" },
            {
                "data": "orderDate",
                "width": "20%",
                "render": function (data) {
                    return new Date(data).toLocaleDateString();
                }
            },
            {
                "data": "orderStatus",
                "width": "15%",
                "render": function (data) {
                    let statusClass = "";
                    switch (data) {
                        case "待處理":
                            statusClass = "text-warning";
                            break;
                        case "處理中":
                            statusClass = "text-info";
                            break;
                        case "可取餐":
                            statusClass = "text-primary";
                            break;
                        case "已完成":
                            statusClass = "text-success";
                            break;
                        case "已取消":
                            statusClass = "text-danger";
                            break;
                    }
                    return `<span class="${statusClass}">${data}</span>`;
                }
            },
            {
                "data": "orderTotal",
                "width": "15%",
                "render": function (data) {
                    return `$${parseFloat(data).toFixed(0)}`;
                }
            },
            {
                "data": "id",
                "width": "15%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/Customer/Order/Details?orderId=${data}" 
                               class="btn btn-primary mx-2">
                                <i class="bi bi-info-circle"></i> 詳細
                            </a>
                        </div>
                    `;
                }
            }
        ],
        "language": {
            "processing": "處理中...",
            "loadingRecords": "載入中...",
            "lengthMenu": "顯示 _MENU_ 筆資料",
            "zeroRecords": "沒有符合的資料",
            "info": "顯示第 _START_ 至 _END_ 筆資料，共 _TOTAL_ 筆",
            "infoEmpty": "顯示第 0 至 0 筆資料，共 0 筆",
            "infoFiltered": "(從 _MAX_ 筆資料中過濾)",
            "search": "搜尋:",
            "paginate": {
                "first": "第一頁",
                "previous": "上一頁",
                "next": "下一頁",
                "last": "最後一頁"
            }
        },
        "order": [[0, "desc"]] // 預設按訂單ID降序排序
    });
}
