var dataTable;

document.addEventListener('DOMContentLoaded', function () {
    var url = window.location.search;
    console.log(url);
    // 修改：簡化狀態判斷邏輯，使用 switch 更容易維護
    const status = new URLSearchParams(url).get('status') || 'all';
    loadDataTable(status);
});

// 修改：加入 status 參數
function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            // 修改：確保 status 參數正確傳遞
            "url": `/admin/order/getall?status=${status}`,
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "20%" },
            { "data": "applicationUser.email", "width": "20%" },
            { "data": "orderStatus", "width": "10%" },
            {
                "data": "orderTotal",
                "width": "10%",
                // 新增：金額格式化
                "render": function (data) {
                    return `$${parseFloat(data).toFixed(0)}`;
                }
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data}" 
                            class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> 詳細
                            </a>
                        </div>
                    `
                },
                "width": "15%"
            }
        ],
        // 新增：中文化設定
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
        }
    });
}