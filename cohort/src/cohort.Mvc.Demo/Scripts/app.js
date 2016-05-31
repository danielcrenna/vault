$(function() {

    $('#contact-submit').click(function() {
        $('#contact-form').submit();
    });

    $('#contact-modal').css({
        width: 'auto',
        'margin-left': function () {
            return -($(this).width() / 2) - 10;
        }
    });

});


