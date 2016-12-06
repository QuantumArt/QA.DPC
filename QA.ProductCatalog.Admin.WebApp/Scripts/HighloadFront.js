function updateTasks() {
    var url = window.location.href;

    $.getJSON(url + '/GetSettings?url=sync/settings', function (json) {

        clearTasks();

        $.each(json, function (i, task) {
            addTask(task, i);
        });

    })

    setTimeout(function () {
        updateTasks();
    }, 5000);
}

function IndexChanel(language, state, id) {
    var url = window.location.href;

    $.post(url + '/IndexChanel?url=sync/' + language + '/' + state + '/reset', function (data) {
        updateTasks();
    });
}

function clearTasks() {
    $("#tasks")
        .find('tbody').empty();
}

function addTask(task, id) {
    $("#tasks")
        .find('tbody')
        .append(
            '<tr><td>' +
                task.ChannelLanguage +
            '</td><td>' +
                task.ChannelState +
            '</td><td>' +
                task.TaskProgress +
            '</td><td>' +
                getTaskStateDescription(task.TaskState) +
              '</td><td>' +
                '<input Id="index' + id +'" ' + task.TaskId + ' type="button" value="индексировать" />' +
            '</td></tr>');

    $('#index' + id).click(function () {
        IndexChanel(task.ChannelLanguage, task.ChannelState, 0);
    });
}

function getTaskStateDescription(state) {
    if (state == null) {
        return null;
    }
    else if (state == 1) {
        return 'New';
    }
    else if (state == 2) {
        return 'Running';
    }
    else if (state == 3) {
        return 'Done';
    }
    else if (state == 4) {
        return 'Failed';
    }
    else if (state == 5) {
        return 'Cancelled';
    }

    return null;
}