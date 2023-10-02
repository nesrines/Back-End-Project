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
                $('.quick-view-image').slick({
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    arrows: false,
                    dots: false,
                    fade: true,
                    asNavFor: '.quick-view-thumb',
                    speed: 400,
                });
                $('.quick-view-thumb').slick({
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    asNavFor: '.quick-view-image',
                    dots: false,
                    arrows: false,
                    focusOnSelect: true,
                    speed: 400,
                });
            });
    });

    $(document).on('click', '.basket-btn', function (e) {
        e.preventDefault();
        fetch($(this).attr('href'))
            .then(res => res.text())
            .then(data => $('.header-cart').html(data));
    });

    $(document).on('click', '.remove-btn', function (e) {
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

    $('#loadMoreBtn').click(function (e) {
        e.preventDefault();

        let pageIndex = $(this).attr('href').split('=')[1];
        let maxPages = $(this).data('maxpages');

        if (pageIndex > 0 && pageIndex <= maxPages) {
            fetch(($(this).attr('href') + '?pageIndex=' + pageIndex))
                .then(res => res.text())
                .then(data => $("#productContainer").append(data));

            $(this).attr('href', $(this).attr('href').replace('=' + pageIndex, '=' + (pageIndex + 1)))

            if (pageIndex == maxPages) $(this).remove();
        }
    });

    $(document).on('click', '.qty-btn', function () {
        var qtyBtn = $(this);
        var oldValue = $button.parent().find('input').val();

        if (qtyBtn.hasClass('inc')) var newVal = oldValue > 1 ? parseFloat(oldValue) + 1 : 1;
        else var newVal = oldValue > 1 ? parseFloat(oldValue) - 1 : 1;

        qtyBtn.parent().find('input').val(newVal);

        fetch('Basket/ChangeCount?id=' + $(this).parent().find('input').data('id') + '&count=' + qtyBtn.parent().find('input').val())
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

        fetch('Basket/ChangeCount?id=' + $(this).data('id') + '&count=' + $(this).val())
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