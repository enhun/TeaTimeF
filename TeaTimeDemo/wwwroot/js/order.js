var dataTable;
document.addEventListener('DOMContentLoaded', function () {
    var url = window.location.search;
    const status = new URLSearchParams(url).get('status') || 'all';
    loadDataTable(status);

    // 添加篩選按鈕的點擊事件監聽器
    document.querySelectorAll('.list-group-item').forEach(function (item) {
        item.addEventListener('click', function () {
            const status = this.getAttribute('data-status');
            loadDataTable(status);
        });
    });
});

function loadDataTable(status) {
    if (dataTable) {
        dataTable.destroy();
    }
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": `/admin/order/getall?status=${status}`,
            "dataSrc": function (json) {
                return json.data || [];
            },
            "error": function (xhr, error, thrown) {
                console.error('DataTables 錯誤:', error);
                console.error('伺服器回應:', xhr.responseText);
                toastr.error('載入資料時發生錯誤，請檢查控制台了解詳情');
                return [];
            }
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "applicationUser",
                "width": "15%",
                "render": function (data) {
                    return data ? data.email : '';
                }
            },
            {
                "data": "orderStatus",
                "width": "10%",
                "render": function (data) {
                    return `<span>${data || ''}</span>`;
                }
            },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "width": "25%",
                "render": function (data) {
                    return `
                        <div class="btn-group" role="group">
                            <a href="/admin/order/details?orderId=${data}" 
                               class="btn btn-primary mx-2">
                                <i class="bi bi-pencil-square"></i> 詳細
                            </a>
                            <a onclick="deleteOrder(${data})"
                               class="btn btn-danger mx-2">
                                <i class="bi bi-trash"></i> 刪除
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
        }
    });
}

function deleteOrder(id) {
    Swal.fire({
        title: '確定要刪除這個訂單嗎?',
        text: "此操作無法復原!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: '確定刪除',
        cancelButtonText: '取消'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: `/Admin/Order/Delete`,
                type: 'DELETE',
                data: { id: id },
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                },
                error: function (xhr, status, error) {
                    console.error('刪除失敗:', error);
                    toastr.error('刪除訂單時發生錯誤');
                }
            });
        }
    });
}