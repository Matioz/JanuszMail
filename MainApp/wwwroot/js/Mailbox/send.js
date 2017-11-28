var actionName = "";
$(".async-button").click(function (event) {
    actionName = $(this).attr("formaction");
});
$(".async-form").submit(function (event) {
    $("#modalButton").click();
    event.preventDefault();
    var url = "MailBox/" + actionName;

    var successMessage = "";
    if (actionName == "Send") {
        successMessage = "Message has been sent";
    }
    else if (actionName == "SaveDraft") {
        successMessage = "Message has been saved as draft";
    }
    var fd = new FormData();
    fd.append('__RequestVerificationToken', $('[name=__RequestVerificationToken]').val());
    fd.append('Subject', $('[name=Subject]').val());
    fd.append('Recipient', $('[name=Recipient]').val());
    fd.append('Body', $('[name=Body]').val());
    fd.append('ID', $('[name=ID]').val());
    var file_data = $('input[type="file"]')[0].files; // for multiple files
    for (var i = 0; i < file_data.length; i++) {
        fd.append("Attachments", file_data[i]);
    }
    $.ajax({
        url: url,
        type: 'POST',
        data: fd,
        processData: false,
        contentType: false,
        ajaxSend: $('#statusBody').html('<div class="text-center block">Please wait <img src="images/ajax-loader.gif" alt="Processing"/></div>'),
        success:
            function (data) {
                if (data == true) {
                    $('#statusBody').html('<p class="text-success">' + successMessage + '</p>');
                }
                else {
                    $('#statusBody').html('<p class="text-danger"> Operation failed</p>');
                }
            },
        error: function (obj) {
            $('#statusBody').html("Something happened");
        }
    });
});