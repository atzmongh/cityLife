function mobileMenu(){
    $('.mobile-menu-toggle').on('click', function () {
        $(this).toggleClass('active');
        $('.main-sidebar').toggleClass('active');
        $('body').toggleClass('active-menu');
    })
}

jQuery(document).ready(function () {
    mobileMenu();
});