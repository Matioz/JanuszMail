var actionName = "";
$(".async-button").click(function (event) {
    actionName = $(this).attr("formaction");
});
$(".async-form").submit(function (event) {
    $("#modalButton").click();
    event.preventDefault();
    var url = $(this).attr("action") + "/" + actionName;
    $.ajax({
        url: url,
        type: 'POST',
        data: $(this).serialize(),
        ajaxSend: $('#statusBody').html('<div class="text-center block">Please wait <img src="images/ajax-loader.gif" alt="Processing"/></div>'),
        success:
            function (data) {
                if (data == true) {
                    $('#statusBody').html('<p class="text-success"> Message  sent</p>');
                }
                else {
                    $('#statusBody').html('<p class="text-danger"> Sending failed</p>');
                }
            },
        error: function (obj) {
            $('#statusBody').html("Something happened");
        }
    });
});