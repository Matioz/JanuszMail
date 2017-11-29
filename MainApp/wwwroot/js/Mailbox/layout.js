$(document).ready(function () {
    var currentFolder = "";
    $(".nav-folder").click(function (event) {
        if (currentFolder == "") {
            $("#searchButton").removeClass("disabled");
        } else {
            $('.nav-folder[folder-name="' + currentFolder + '"]').removeClass("bold");
        }
        currentFolder = $(this).attr("folder-name");
        $(this).addClass("bold");
        var currentContent = $('#MailList').html();
        $.ajax({
            url: 'MailBox/ShowMails',
            data: { folder: $(this).attr("folder-name") },
            type: 'GET',
            contentType: 'text/html; charset=utf-8',
            ajaxSend: $('#MailList').html('<div class="text-center block"><img src="images/ajax-loader.gif" alt="Loading mails"/></div>'),
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
    $("#searchButton").click(function (event) {
        if ($(this).hasClass("disabled")) {
            return;
        }
        if (currentFolder.length == 0) {
            alert("Choose folder first");
            return;
        }
        var subject = "";
        var sender = "";
        if ($("#searchBy").val() == "subject") {
            subject = $("#searchText").val();
        }
        else {
            sender = $("#searchText").val();
        }
        var currentContent = $('#MailList').html();
        $.ajax({
            url: 'MailBox/ShowMails',
            data: { folder: currentFolder, subject: subject, sender: sender },
            type: 'GET',
            ajaxSend: $('#MailList').html('<div class="text-center block"><img src="images/ajax-loader.gif" alt="Loading mails"/></div>'),
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
    $("a.nav-partial").click(function (event) {
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
    $("#searchText").autocomplete({

        source: function (request, response) {
            var subject = "";
            var sender = "";
            if ($("#searchBy").val() == "subject") {
                subject = request.term;
            }
            else {
                sender = request.term;
            }
            if (currentFolder == "") {
                return;
            }
            $.ajax({
                url: "MailBox/QuickSearch",
                dataType: "json",
                data: {
                    folder: currentFolder,
                    sender: sender,
                    subject: subject
                },
                success: function (data) {
                    response(data);
                },
                error: function (data) {
                    alert("error");
                    alert(data[0]);
                }
            });
        },
        open: function () {
            $("#ui-id-1").css('z-index', 1000);
            $("#ui-id-1").show();
            return false;
        }
    });
    $("button.async-reload-folders").click(function (event) {
        event.preventDefault();
        var currentContent = $('#sidebarFolders').html();
        $.ajax({
            url: "MailBox/ShowFolders",
            contentType: 'text/html; charset=utf-8',
            type: 'GET',
            success: function (data) {
                $('#sidebarFolders').html(data);
            },
            error: function (obj) {
                $('#sidebarFolders').html(currentContent);
                $('#statusBody').html("Something happened");
                $("#modalButton").click();
            }
        });
    });
});