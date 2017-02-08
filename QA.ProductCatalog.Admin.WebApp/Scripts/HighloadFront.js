﻿var model = new TasksViewModel();

ko.bindingHandlers.updateProgress = {
    init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        $(element).kendoProgressBar({
            value: viewModel.TaskProgress,
            type: "percent"
        });
    },
    update: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        $(element).kendoProgressBar({
            value: viewModel.TaskProgress,
            type: "percent"
        });
    }
};

$(document).ready(function () {
    moment.locale('ru');
    ko.applyBindings(model);
    updateTasks(true);
});

function TasksViewModel() {
    var self = this;
    self.tasks = ko.observableArray();

    self.index = function (task, event) {
        $(event.target).hide();
        $(event.target).parent().find('img').show();
        indexChanel(task.ChannelLanguage, task.ChannelState);
    };

    self.getState = function (task) {
        return getTaskStateDescription(task.TaskState);
    };

    self.getStateLogo = function (task) {
        return getTaskStateLogo(task.TaskState);
    };

    self.isButtonVisible = function (task) {
        return task.TaskState != 1 && task.TaskState != 2;
    };
}

function updateTasks(loop) {

    $.getJSON(Url.Content('~/HighloadFront/GetSettings?url=sync/settings'), function (json) {        
        model.tasks(json);
    })

    if (loop) {
        setTimeout(function () {
            updateTasks(loop);
        }, 5000);
    }
}

function indexChanel(language, state) {
    $.post(Url.Content('~/HighloadFront/IndexChanel?url=sync/' + language + '/' + state + '/reset')).done(function () {
        updateTasks(false);
    });
}

function getTimePassed(time1, time2) {
    var timePassed = time1 || time1;
    if (timePassed) {
        return moment(timePassed).fromNow()
    }

    return null;
}

function getTaskStateLogo(state) {
    if (state == null) {
        return 'Content/img/icons/0.gif';
    }
    else if (state == 1) {
        return 'Content/img/TaskStates/New16.png';
    }
    else if (state == 2) {
        return 'Content/img/TaskStates/Running16.png';
    }
    else if (state == 3) {
        return 'Content/img/TaskStates/Done16.png';
    }
    else if (state == 4) {
        return 'Content/img/TaskStates/Failed16.png';
    }
    else if (state == 5) {
        return 'Content/img/TaskStates/Cancelled16.png';
    }

    return 'Content/img/icons/0.gif';
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