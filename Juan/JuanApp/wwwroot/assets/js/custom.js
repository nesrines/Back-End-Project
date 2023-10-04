$(document).ready(function () {
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
            .then(data => $('.header-cart').html(data));
    });

    $(document).on('click', '.minicart-remove', function (e) {
        e.preventDefault();

        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => {
                $('.header-cart').html(data);
                if (location.pathname == '/Basket') {
                    fetch('/Basket/UpdateCart')
                        .then(res => res.text())
                        .then(data => $('#basketContent').html(data));
                }
            });
    });

    $(document).on('click', '.qtybtn', function () {
        var qtyBtn = $(this);
        var value = $button.parent().find('input').val();

        if (qtyBtn.hasClass('inc')) qtyBtn.parent().find('input').val(value > 1 ? parseFloat(value) + 1 : 1);
        else qtyBtn.parent().find('input').val(value > 1 ? parseFloat(value) - 1 : 1);;

        fetch('/Basket/ChangeCount?id=' + $(this).parent().find('input').data('id') + '&count=' + qtyBtn.parent().find('input').val())
            .then(res => res.text())
            .then(data => {
                $('.header-cart').html(data);
                if (location.pathname == '/Basket') {
                    fetch('/Basket/UpdateCart')
                        .then(res => res.text())
                        .then(data => $('#basketContent').html(data));
                }
            });
    });

    $(document).on('change', '.pro-qty input', function (e) {
        e.preventDefault();

        fetch('/Basket/ChangeCount?id=' + $(this).data('id') + '&count=' + $(this).val())
            .then(res => res.text())
            .then(data => {
                $('.header-cart').html(data);
                if (location.pathname == '/Basket') {
                    fetch('/Basket/UpdateCart')
                        .then(res => res.text())
                        .then(data => $('#basketContent').html(data));
                }
            });
    });
});