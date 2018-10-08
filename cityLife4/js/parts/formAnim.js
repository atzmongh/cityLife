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
