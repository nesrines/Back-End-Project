$(document).ready(function () {

    $(document).on('click', '.delete-img-btn', function (e) {
        e.preventDefault();
        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => $('#productImages').html(data));
    });

    $(document).on('click', '.reset-btn', function (e) {
        e.preventDefault();
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, reset it!'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch($(this).attr('href'))
                    .then(res => res.text())
                    .then(data => $('#userContainer').html(data));

                Swal.fire(
                    'Reset!',
                    'User password has been reset.',
                    'success'
                )
            }
        });
    });

    $(document).on('click', '.activation-btn', function (e) {
        e.preventDefault();
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Change Active Status'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch($(this).attr('href'))
                    .then(res => res.text())
                    .then(data => $('#userContainer').html(data));

                Swal.fire(
                    'Changed!',
                    'User Active Status has been changed.',
                    'success'
                )
            }
        });
    });
})