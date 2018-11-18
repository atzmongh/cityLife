// var selector = '.main-slider .slick-slide:not(.slick-cloned)';
//
// // Skip cloned elements
// $().fancybox({
//     selector : selector,
//     backFocus : false
// });

// Attach custom click event on cloned elements,
// trigger click event on corresponding link
// $(document).on('click', '.slick-cloned', function(e) {
//     $(selector)
//         .eq( ( $(e.currentTarget).attr("data-slick-index") || 0) % $(selector).length )
//         .trigger("click.fb-start", {
//             $trigger: $(this)
//         });
//
//     return false;
// });

var selector = '.main-slider .slick-slide:not(.slick-cloned)';

$('.icon-btn.fancybox').fancybox({
    selector : selector,
    backFocus : false,
    loop: true,
    toolbar: false,
    buttons: "close",
    preload: true,
    arrowLeft:
        '<button data-fancybox-prev class="fancybox-button fancybox-button--arrow_left" title="{{PREV}}">' +
        '</button>',

    arrowRight:
        '<button data-fancybox-next class="fancybox-button fancybox-button--arrow_right" title="{{NEXT}}">' +
        '</button>',
});