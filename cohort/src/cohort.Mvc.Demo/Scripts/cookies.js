$(function () {
    if (!$.cookie('cohort_landing_page')) {
        $.cookie('cohort_landing_page', window.location.pathname, { expires: 7, path: '/' });
    }
    if (!$.cookie('cohort_referer_url')) {
        $.cookie('cohort_referer_url', document.referrer, { expires: 7, path: '/' });
    }
});