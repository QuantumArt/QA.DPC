AutoUpdateStrategy = {
  timeoutId: null,

  updateRequestFunction: null,

  updateInterval: 2000,

  updateOnErrorInterval: 10000,

  notifyUpdateRequestEnded: function() {
    AutoUpdateStrategy.timeoutId = window.setTimeout(
      AutoUpdateStrategy.updateRequestFunction,
      AutoUpdateStrategy.updateInterval
    );
  },

  notifyUpdateRequestFailed: function(statusId) {
    if (statusId == 401) {
      document.body.innerHTML =
        "<h1>Сессия устарела. Переоткройте или обновите вкладку.</h1>";
    } else
      AutoUpdateStrategy.timeoutId = window.setTimeout(
        AutoUpdateStrategy.updateRequestFunction,
        AutoUpdateStrategy.updateOnErrorInterval
      );
  }
};
