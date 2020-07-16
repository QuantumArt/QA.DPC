var model = new TasksViewModel();

ko.bindingHandlers.updateProgress = {
  init: function(
    element,
    valueAccessor,
    allBindings,
    viewModel,
    bindingContext
  ) {
    $(element).attr("style", "width:" + (viewModel.TaskState || 0) + "%");
  },
  update: function(
    element,
    valueAccessor,
    allBindings,
    viewModel,
    bindingContext
  ) {
    $(element).attr("style", "width:" + (viewModel.TaskState || 0) + "%");
  }
};

$(document).ready(function() {
  ko.applyBindings(model);
  updateTasks(true);
});

function TasksViewModel() {
  var self = this;

  self.tasks = ko.observableArray();

  self.index = function(task) {
    indexChanel(task.ChannelLanguage, task.ChannelState);
  };

  self.getState = function(task) {
    return getTaskStateDescription(task.TaskState);
  };

  self.getProgress = function(task) {
    return (task.TaskState || 0) + "%";
  };

  self.isButtonVisible = function(task) {
    return task.TaskState != 1 && task.TaskState != 2;
  };
}

function updateTasks(loop) {
  $.getJSON(
    Url.Content(
      "~/HighloadFront/GetSettings?customerCode=" +
        getCustomerCode() +
        "&url=api/sync/settings"
    ),
    function(json) {
      model.tasks(json);
    }
  );

  if (loop) {
    setTimeout(function() {
      updateTasks(loop);
    }, 5000);
  }
}

function getCustomerCode() {
  return $("#tasks").data("customerCode");
}

function indexChanel(language, state) {
  $.post(
    Url.Content(
      "~/HighloadFront/IndexChanel?customerCode=" +
        getCustomerCode() +
        "&url=api/sync/" +
        language +
        "/" +
        state +
        "/reset"
    )
  ).done(function() {
    updateTasks(false);
  });
}

function getTimePassed(time1, time2) {
  var timePassed = time1 || time2;
  if (timePassed) {
    return moment(timePassed).fromNow();
  }

  return null;
}

function getDate(date) {
  if (date) {
    return moment(date).calendar();
  }

  return null;
}

function getTaskStateDescription(state) {
  if (state == null) {
    return null;
  } else if (state == 1) {
    return "New";
  } else if (state == 2) {
    return "Running";
  } else if (state == 3) {
    return "Done";
  } else if (state == 4) {
    return "Failed";
  } else if (state == 5) {
    return "Cancelled";
  }

  return null;
}
