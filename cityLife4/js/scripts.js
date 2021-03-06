$( function() {
    var dateFormat = "d MM, yy",
        from = $( "#fromDate" )
            .datepicker({
                defaultDate: "+1w",
                numberOfMonths: 1,
                dateFormat: "d MM, yy",
                firstDay: 1
            })
            .on( "change", function() {
                to.datepicker( "option", "minDate", getDate( this ) );
            }),
        to = $( "#toDate" ).datepicker({
            defaultDate: "+1w",
            numberOfMonths: 1,
            dateFormat: "d MM, yy",
            firstDay: 1
        })
            .on( "change", function() {
                from.datepicker( "option", "maxDate", getDate( this ) );
            });

    function getDate( element ) {
        var date;
        try {
            date = $.datepicker.parseDate( dateFormat, element.value );
        } catch( error ) {
            date = null;
        }

        return date;
    }
} );

//$('.ui-datepicker-calendar').insertAfter('.datepicker-note');
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
function focusAnim(){
    jQuery('input, textarea').each(function (i, el) {
        if(!jQuery(el).is('[type=checkbox]') && !jQuery(el).is('[type=radio]')){
            if (jQuery(el).val().trim() !== '' ) {
                jQuery(this).parents('.field-box').addClass('focus')
            } else {
                jQuery(this).parents('.field-box').removeClass('focus')
            }

            jQuery(el).focusout(function () {
                if (jQuery(el).val().trim() !== '' ){
                    jQuery(this).parents('.field-box').addClass('focus')
                } else {
                    jQuery(this).parents('.field-box').removeClass('focus')
                }
            });

            jQuery(el).change(function () {
                if (jQuery(el).val().trim() !== '' ){
                    jQuery(this).parents('.field-box').addClass('focus')
                } else {
                    jQuery(this).parents('.field-box').removeClass('focus')
                }
            });

            jQuery(el).focus(function () {
                jQuery(this).parents('.field-box').addClass('focus');
            });
        }
    });
}


jQuery(document).ready(function () {
    focusAnim();
});

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
$(document).ready(function() {
    $('select').select2({
        minimumResultsForSearch: Infinity
    });
});
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
        slidesToScroll: 1,
        infinite: true,
        arrows: false,
        dots: false,
        focusOnSelect: true,
        vertical: true,
        asNavFor: '.slider-for'
    });

    //mainSliderHeight();
});

jQuery(window).on('resize', function () {
    //mainSliderHeight();
});
