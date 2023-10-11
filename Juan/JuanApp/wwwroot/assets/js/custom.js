$(document).ready(function () {
    let infoInput = $('#infoToaster').val();
    if (infoInput.length > 0) toastr["info"](infoInput);

    let successInput = $('#successToaster').val();
    if (successInput.length > 0) toastr["success"](successInput);

    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }

    $('.add-address-btn').click(function (e) {
        e.preventDefault();
        $('#addressForm').removeClass('d-none');
        $('#addressContainer').addClass('d-none');
    });

    $('.go-back-btn').click(function (e) {
        e.preventDefault();
        $('#addressForm').addClass('d-none');
        $('#addressContainer').removeClass('d-none');
    });

    $('.edit-btn').click(function (e) {
        e.preventDefault();
        $('#addressForm').removeClass('d-none');
        $('#addressContainer').addClass('d-none');

        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => $('#addressForm').html(data))
    });

    $('#searchInput').keyup(function (e) {
        e.preventDefault();
        let categoryId = $('#categoryId').val();
        let search = $('#searchInput').val().trim();
        if (search.length >= 3) {
            fetch('product/search?search=' + search + '&categoryId=' + categoryId)
                .then(res => res.text())
                .then(data => $('#searchBody').html(data));
        }
        else $('#searchBody').html('');
    });

    $(document).on('click', '.modal-btn', function (e) {
        e.preventDefault();
        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => {
                $('.modal-content').html(data);
                $('.product-large-slider').slick({
                    fade: true,
                    arrows: false,
                    asNavFor: '.pro-nav'
                });
                $('.pro-nav').slick({
                    slidesToShow: 4,
                    asNavFor: '.product-large-slider',
                    arrows: false,
                    focusOnSelect: true
                });
                $('.img-zoom').zoom();
            });
    });

    $(document).on('click', '.basket-btn', function (e) {
        e.preventDefault();
        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => {
                $('.minicart-content-box').html(data);

                fetch('/Basket/UpdateCount').then(res => res.text())
                    .then(data => $('.notification').html(data));
            });
    });

    $(document).on('click', '.minicart-remove', function (e) {
        e.preventDefault();

        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => {
                $('.minicart-content-box').html(data);

                fetch('/Basket/UpdateCount').then(res => res.text())
                    .then(data => $('.notification').html(data));

                if (location.pathname == '/Basket') {
                    fetch('/Basket/UpdateCart').then(res => res.text())
                        .then(data => $('#basketContainer').html(data));
                }
            });
    });

    $(document).on('click', '.qtybtn', function () {
        fetch(`/Basket/Count${$(this).hasClass('inc') ? 'Inc' : 'Dec'}` + $(this).parent().find('input').data('id'))
            .then(res => res.text())
            .then(data => {
                $('.minicart-content-box').html(data);
                if (location.pathname == '/Basket') {
                    fetch('/Basket/UpdateCart').then(res => res.text())
                        .then(data => $('#basketContainer').html(data));
                }
            });
    });
});