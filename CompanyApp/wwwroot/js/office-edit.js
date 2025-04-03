function loadEditForm(officeId) {
    $.get('/Office/Edit/' + officeId, function (data) {
        $('#editOfficeFormContainer').html(data);
    });
}

$(document).ready(function () {
    $(document).on('submit', '#edit-office-form', function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            type: 'POST',
            url: form.attr('action'),
            data: form.serialize(),
            success: function (response) {
                $('#editOfficeModal').modal('hide');
                location.reload(); // Обновление страницы после сохранения
            },
            error: function (xhr) {
                $('#editOfficeFormContainer').html(xhr.responseText); // Обновление формы с ошибками
            }
        });
    });
});
