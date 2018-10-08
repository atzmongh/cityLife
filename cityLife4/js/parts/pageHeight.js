function minWrapperHeight(){
    //page height
    var mainWrapper = $('.main-wrapper'),
        windowHeight = $(window).outerHeight(),
        windowWidth = $(window).outerWidth();

    if(windowWidth > 767) {
        mainWrapper.css({'min-height': windowHeight + 'px'});
    }

    //main description height
    var mainWrapperHeight = parseInt(mainWrapper.css('minHeight')),
        mainSliderHeight = $('.main-slider').outerHeight(),
        mainDscr = $('.main-description-wrap'),
        mainDscrHeight = mainWrapperHeight - mainSliderHeight;

    if(windowWidth > 767) {
        mainDscr.css({'min-height': mainDscrHeight + 'px'});
    }
}

jQuery(document).ready(function () {
    minWrapperHeight();
});

jQuery(window).on('resize', function () {
    minWrapperHeight();
});