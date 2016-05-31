$(function () {
    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
        var verificationToken = $("meta[name='csrf_token']").attr('content');
        if (verificationToken) {
            jqXHR.setRequestHeader("X-Authenticity-Token", verificationToken);
        }
    });
    var token = $('meta[name=csrf_token]').attr('content');
    var field = $("<input type='hidden' name='csrf_token' value='" + token + "' />");
    var forms = $('form');
    field.appendTo(forms);
});