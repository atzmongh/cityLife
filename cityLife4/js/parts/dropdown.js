function dropdown() {
    var dropdown = $('.dropdown');

    $('.dropdown-toggle').on('click', function (e) {
        dropdown.removeClass('open');
        $(this).next().addClass('open');
        e.preventDefault();
    })

    $('body').on('mouseup', function(e) {
        if(!dropdown.is(e.target) && dropdown.has(e.target).length === 0) {
            dropdown.removeClass('open');
        }
        e.preventDefault();
    });
}

jQuery(document).ready(function () {
    dropdown();
});

jQuery(window).on('resize', function () {
    dropdown();
});