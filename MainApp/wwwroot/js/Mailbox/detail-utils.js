var actionName = "";

$(".async-button").click(function (event) {
    actionName = $(this).attr("formaction");
});
$(".async-form").submit(function (event) {
    $("#modalButton").click();
    event.preventDefault();
    var operation = "";
    if (actionName == "MoveToFolder") {
        operation = "Moving to folder";
    }
    else {
        operation = "Deleting message";
    }
    var urlForm = $(this).attr("action") + "/" + actionName;

    var currentContent = $('#MailList').html();
    $.ajax({
        url: urlForm,
        type: 'POST',
        data: $(this).serialize(),
        ajaxSend: $('#statusBody').html('<div class="text-center block">Please wait <img src="images/ajax-loader.gif" alt="Processing"/></div>'),
        success:
            function (data) {
                if (data == true) {
                    $('#statusBody').html('<p class="text-success">' + operation + ' succeed</p>');
                    if (operation == "Deleting message") {
                        $.ajax({
                            url: 'MailBox/ShowMails',
                            data: { folder: $("#currentFolderName").val() },
                            type: 'GET',
                            contentType: 'text/html; charset=utf-8',
                            ajaxSend: $('#MailList').html('<div class="text-center block"><img src="images/ajax-loader.gif" alt="Loading mails"/></div>'),
                            success:
                                function (data) {
                                    $('#MailList').html(data);
                                },
                            error: function (obj) {
                                $('#MailList').html(currentContent);
                                $('#statusBody').html("Could not load mails from folder");
                                $("#modalButton").click();
                            }
                        });
                    }
                }
                else {
                    $('#statusBody').html('<p class="text-danger">' + operation + ' failed</p>');
                }
            },
        error: function (obj) {
            $('#MailList').html(currentContent);
            $('#statusBody').html("Something happened");
        }
    });
});
$("a.partial-button").click(function (event) {
    event.preventDefault();

    var currentContent = $('#MailList').html();
    $.ajax({
        url: $(this).attr("href"),
        type: 'GET',
        ajaxSend: $('#MailList').html('<div class="text-center block"><img src="images/ajax-loader.gif" alt="Loading"/></div>'),
        success:
            function (data) {
                $('#MailList').html(data);
            },
        error: function (obj) {
            $('#MailList').html(currentContent);
            $('#statusBody').html("Something happened");
            $("#modalButton").click();
        }
    });
});