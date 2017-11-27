var actionName = "";

$(".async-button").click(function (event) {
    var formaction = $(this).attr("formaction")
    actionName = formaction;
    if (formaction == "Delete") {
        $(this).addClass("to-remove");
    } else if (formaction == "MarkRead") {
        $(this).attr("formaction", "MarkUnread");
        $(this).removeClass("btn-warning");
        $(this).addClass("btn-info");
        $(this).children().removeClass("fa-envelope");
        $(this).children().addClass("fa-envelope-open");
    } else if (formaction == "MarkUnread") {
        $(this).attr("formaction", "MarkRead");
        $(this).addClass("btn-warning");
        $(this).removeClass("btn-info");
        $(this).children().removeClass("fa-envelope-open");
        $(this).children().addClass("fa-envelope");
    }
});

$(".async-form").submit(function (event) {
    event.preventDefault();
    var urlForm = "MailBox/" + actionName;
    if (actionName == "Delete") {
        var deleteButton = $("button.to-remove");
        deleteButton.parentsUntil("tbody").addClass("to-remove");
        deleteButton.parentsUntil("table").remove(".to-remove");
    }
    $.ajax({
        url: urlForm,
        type: 'POST',
        data: $(this).serialize(),
        success:
            function (data) {
                if (data == false) {
                    $('#statusBody').html("Operation failed");
                    $("#modalButton").click();
                }
            },
        error: function (obj) {
            $('#statusBody').html("Something happened");
            $("#modalButton").click();
        }
    });
});

$("a.allow-partial").click(function (event) {
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

$("button.async-reload-folder").click(function (event) {
    event.preventDefault();
    var currentContent = $('#MailList').html();
    var folderName = $(this).attr("folder-name");
    $.ajax({
        url: "MailBox/UpdateCachedMails",
        data: { folder: folderName },
        contentType: 'text/html; charset=utf-8',
        type: 'GET',
        success: function (data) {
            $.ajax({
                url: "MailBox/ShowMails",
                data: { folder: folderName },
                type: 'GET',
                success:
                    function (data) {
                        $('#MailList').html(data);
                    },
                error: function (obj) {
                    $('#MailList').html(currentContent);
                    $('#statusBody').html("Something happened");
                    $("#modalButton").click();
                }
            })
        },
        error: function (obj) {
            $('#MailList').html(currentContent);
            $('#statusBody').html("Something happened");
            $("#modalButton").click();
        }
    });
});