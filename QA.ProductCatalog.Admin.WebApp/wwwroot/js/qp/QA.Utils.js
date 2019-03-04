var QA = QA || {};
var HOSTID_SESSION_STORAGE_KEY = "7C46280A-A276-4F1A-8FA3-053B38959E09";
if (jQuery.type(QP) == "undefined") {
  var QP = {};
}
var QpEntityCodes =
  QpEntityCodes ||
  (function() {
    return {
      article: "article"
    };
  })();

var QpActionCodes =
  QpActionCodes ||
  (function() {
    return {
      multiple_select: "multiple_select_article",
      select_article: "select_article",
      remove_article: "remove_article",
      multiple_remove_article: "multiple_remove_article",
      archive_article: "move_to_archive_article",
      multiple_archive_article: "multiple_move_to_archive_article"
    };
  })();

if (jQuery.type(QA.Utils) == "undefined") {
  QA.Utils = (function() {
    function getParent() {
      return window.top;
    }

    function isGuid(guid) {
      if (guid) {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(
          guid
        );
      }
      return false;
    }

    return {
      initNew: function(name, callback) {
        return new Quantumart.QP8.Interaction.BackendEventObserver(
          name,
          callback
        );
      },

      initOptions: function(
        action,
        type,
        itemId,
        entityId,
        actionId,
        observer
      ) {
        var executeOptions = new Quantumart.QP8.Interaction.ExecuteActionOtions();
        executeOptions.actionCode = action;
        executeOptions.entityTypeCode = type;
        executeOptions.entityId = itemId;
        executeOptions.parentEntityId = entityId;
        executeOptions.actionUID = actionId;
        executeOptions.callerCallback = observer.callbackProcName;

        return executeOptions;
      },

      initSelectOptions: function(
        isMultiple,
        entityId,
        actionId,
        selectedEntityIDs,
        observer
      ) {
        var executeOptions = {};
        executeOptions.selectActionCode = isMultiple
          ? QpActionCodes.multiple_select_article
          : QpActionCodes.select_article;
        executeOptions.entityTypeCode = QpEntityCodes.article;
        (executeOptions.isMultiple = isMultiple == true ? true : false),
          (executeOptions.parentEntityId = entityId);
        executeOptions.selectWindowUID = actionId;
        executeOptions.callerCallback = observer.callbackProcName;
        executeOptions.selectedEntityIDs = selectedEntityIDs;

        return executeOptions;
      },

      setFieldValues: function(executeOptions, values) {
        if (executeOptions.options == null) {
          executeOptions.options = new Quantumart.QP8.Interaction.ArticleFormState();
        }

        executeOptions.options.initFieldValues = values;
      },

      setDisabledFields: function(executeOptions, values) {
        if (executeOptions.options == null) {
          executeOptions.options = new Quantumart.QP8.Interaction.ArticleFormState();
        }

        executeOptions.options.disabledFields = values;
      },

      setDisabledButtons: function(executeOptions, values) {
        if (executeOptions.options == null) {
          executeOptions.options = new Quantumart.QP8.Interaction.ArticleFormState();
        }

        executeOptions.options.disabledActionCodes = values;
      },

      setHideFields: function(executeOptions, values) {
        if (executeOptions.options == null) {
          executeOptions.options = new Quantumart.QP8.Interaction.ArticleFormState();
        }

        executeOptions.options.hideFields = values;
      },

      setAdditionalParams: function(executeOptions, values) {
        if (executeOptions.options == null) {
          executeOptions.options = new Quantumart.QP8.Interaction.ArticleFormState();
        }

        executeOptions.options.additionalParams = values;
      },

      setFilter: function(executeOptions, value) {
        if (executeOptions.options == null) {
          executeOptions.options = new Quantumart.QP8.Interaction.ArticleFormState();
        }

        executeOptions.options.filter = value;
      },

      executeWindow: function(options, hostId) {
        var backendWnd = getParent();

        options.isWindow = true;
        options.changeCurrentTab = false;

        Quantumart.QP8.Interaction.executeBackendAction(
          options,
          hostId,
          backendWnd
        );
      },

      executeTab: function(options, id) {
        var backendWnd = getParent();

        options.isWindow = false;
        options.changeCurrentTab = false;

        Quantumart.QP8.Interaction.executeBackendAction(
          options,
          id,
          backendWnd
        );
      },

      executeAction: function(options, id) {
        var backendWnd = getParent();

        Quantumart.QP8.Interaction.executeBackendAction(
          options,
          id,
          backendWnd
        );
      },

      executeSelectWindow: function(options, id) {
        var backendWnd = getParent();

        Quantumart.QP8.Interaction.openSelectWindow(options, id, backendWnd);
      },

      close: function(action, id) {
        var backendWnd = getParent();
        Quantumart.QP8.Interaction.closeBackendHost(action, id, backendWnd);
      },

      hostId: function() {
        getParent();
        if (isGuid(window.name) == false) {
          console.log(
            "window.name is not a guid so it will restored from session storage, window.name: " +
              window.name
          );
          return sessionStorage.getItem(HOSTID_SESSION_STORAGE_KEY);
        } else {
          return window.name;
        }
      },
      newUID: function() {
        return (
          "uid" + new Date().getTime() + "_" + Math.floor(Math.random() * 10000)
        );
      },
      refresh: function() {
        $(".loadingLayer").removeClass("hidden");
        window.location.reload();
      },
      getParent: getParent
    };
  })();
}

if (jQuery.type(QA.Enums) == "undefined") {
  QA.Enums = (function() {
    return {
      EventType: {
        CloseHost: { Id: 1, Name: "CloseHost" },
        ActionHost: { Id: 2, Name: "ActionHost" },
        Other: { Id: 3, Name: "Other" }
      }
    };
  })();
}
