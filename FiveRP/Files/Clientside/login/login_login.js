jQuery(function($) {
    $("form").on('submit', function (e) {
        e.preventDefault();
        var user = $("#email").val();
        var pass = $("#password").val();
        resourceCall("onSubmit", user, pass);
    });
})