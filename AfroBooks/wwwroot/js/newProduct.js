var dataTable

$(document).ready(function () {
    return dataTable = $('#tblData').DataTable({
        ajax: '/Admin/Product/GetAll',
        columns: [
            { data: 'title' },
            { data: 'author' },
            { data: 'isbn' },
            { data: 'productCategory.name' },
            {
                data: 'id',
                render: function (data) {
                    return `
                     <div>
                        <a href="/Admin/Product/Upsert?id=${data}" class="btn btn-secondary" style="width:fit-content">
                            <i class="bi bi-pencil-square"></i>
                        </a>
                        <a onClick="DeleteProduct('/Admin/Product/Delete?id=${data}')" class="btn btn-danger" style="width:fit-content">
                            <i class="bi bi-trash"></i>
                        </a>
                        
                    </div>
                    `
                }
            },
        ],
    });
});

function DeleteProduct(url) {
    Swal.fire({
        title: 'Are you sure you want to delete the product?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                success: function (data) {
                    if (data.state) {
                        Swal.fire(
                            'Deleted!',
                            data.msg,
                            'success'
                        );
                        dataTable.ajax.reload();
                    }
                    else {
                        Swal.fire(
                            'Wrong!',
                            data.msg,
                            'error'
                        );
                    }
                }
            })
        }
    })
}