// function mainSliderHeight(){
//     var mainSliderHeight = $('.main-slider').height();
//     var slideHeight = $('.main-slider .slide img');
//
//     slideHeight.css({'min-height': mainSliderHeight + 'px'});
// }

jQuery(document).ready(function () {
    //main slider
    $('.main-slider').slick({
        infinite: true,
        dots: true,
        arrows: false,
        autoplay: true
    });

    //vertical slider
    $('.slider-for').slick({
        slidesToShow: 1,
        slidesToScroll: 1,
        infinite: true,
        arrows: true,
        fade: true,
        dots: true,
        asNavFor: '.slider-nav'
    });
    $('.slider-nav').slick({
        slidesToShow: 6,
        slidesToScroll: 6,
        infinite: true,
        arrows: false,
        dots: false,
        focusOnSelect: true,
        centerMode: true,
        centerPadding: '0',
        vertical: true,
        verticalSwiping: true,
        asNavFor: '.slider-for',
        responsive: [
            {
                breakpoint: 1367,
                settings: {
                    vertical: false,
                    verticalSwiping: false,
                    slidesToShow: 5,
                    slidesToScroll: 5
                }
            },
            {
                breakpoint: 768,
                settings: {
                    vertical: false,
                    verticalSwiping: false,
                    slidesToShow: 3,
                    slidesToScroll: 3
                }
            }
        ]
    });

    //mainSliderHeight();
});

jQuery(window).on('resize', function () {
    //mainSliderHeight();
});
