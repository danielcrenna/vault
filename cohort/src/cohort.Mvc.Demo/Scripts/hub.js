$(function () {
    
    // Proxy created on the fly
    var notification = $.connection.notification;

    notification.client.addMessage = function (message) {
        $.gritter.add({
            title: 'Message',
            text: message,
            sticky: false
        });
    };

    notification.client.alertApplicationMaintenance = function () {
        $.gritter.add({
            title: 'Application Maintenance!',
            text: 'This system will be going offline in 5 minutes for maintenance.  Please finish up your work.',
            sticky: true,
            class_name: 'my-sticky-class'
        });
    };

    $("#broadcast").click(function () {
        notification.server.send($('#msg').val());
    });

    $("#maintenance").click(function () {
        notification.server.applicationMaintenance();
    });

    // Start the connection
    $.connection.hub.start();
});