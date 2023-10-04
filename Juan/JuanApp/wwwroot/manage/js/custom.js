$(document).ready(function () {

    $(document).on('click', 'delete-img-btn', function (e) {
        e.preventDefault();
        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => $('#productImages').html(data));
    });
})