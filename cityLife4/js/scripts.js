$( function() {
    var dateFormat = "d MM, yy",
        from = $( "#fromDate" )
            .datepicker({
                defaultDate: "+0d",
                minDate: timeNow,   //The timenow is defined in a script before this script, and it gives the fake time now
                numberOfMonths: 1, 
                dateFormat: "d MM, yy",
                firstDay: 1
            })
            .on("change", function () {
                var fromDate = getDate(this);   //get the "from date"
                var nextDay = fromDate.getDate() + 1;  //get the day component and add 1 to it.
                var nextDate = new Date(fromDate);   
                nextDate.setDate(nextDay);             //Add the day component to the "from date". Note that if from date is 31/12/2018, it will skip to 1/1/2019
                to.datepicker( "option", "minDate", nextDate);
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
$('.icon-btn.fancybox').fancybox({
    loop: true,
    toolbar: true,
    buttons: [
        "close"
    ],
    btnTpl: {
        close:
            '<button data-fancybox-close class="fancybox-button fancybox-button--close" title="{{CLOSE}}"></button>',
        arrowLeft:
            '<button data-fancybox-prev class="fancybox-button fancybox-button--arrow_left" title="{{PREV}}"></button>',
        arrowRight:
            '<button data-fancybox-next class="fancybox-button fancybox-button--arrow_right" title="{{NEXT}}"></button>'
    },
    infobar: false,
    arrowLeft: '<button data-fancybox-prev class="fancybox-button fancybox-button--arrow_left" title="{{PREV}}">' + '</button>',
    arrowRight: '<button data-fancybox-next class="fancybox-button fancybox-button--arrow_right" title="{{NEXT}}">' + '</button>'
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

    //vertical slider on booking
    $('.slider-for-simple').slick({
        slidesToShow: 1,
        slidesToScroll: 1,
        infinite: true,
        arrows: true,
        fade: true,
        dots: false,
        swipe: false,
        asNavFor: '.slider-nav-simple'
    });
    $('.slider-nav-simple').slick({
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
        asNavFor: '.slider-for-simple',
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

    //vertical slider on flat-description
    $('.slider-for-has-sub-slider').slick({
        slidesToShow: 1,
        slidesToScroll: 1,
        infinite: true,
        arrows: false,
        fade: true,
        dots: false,
        swipe: false,
        asNavFor: '.slider-nav-has-sub-slider'
    });
    $('.sub-slider').slick({
        slidesToShow: 1,
        slidesToScroll: 1,
        infinite: true,
        arrows: true,
        fade: true,
        dots: false,
        swipe: false,
    });
    $('.sub-slider').on("mousedown mouseup", function() {
        $('.slider-for.has-sub-slider').slick("slickGoTo", 1);
    });
    $('.slider-nav-has-sub-slider').slick({
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
        asNavFor: '.slider-for-has-sub-slider',
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
