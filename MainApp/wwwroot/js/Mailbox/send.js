var actionName = "";
$(".async-button").click(function (event) {
    actionName = $(this).attr("formaction");
});
$(".async-form").submit(function (event) {
    $("#modalButton").click();
    event.preventDefault();
    var url = $(this).attr("action") + "/" + actionName;

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