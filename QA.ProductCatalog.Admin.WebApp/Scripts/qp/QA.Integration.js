/// <reference path="~/scripts/_references.js" />

var QA = QA || {};

QA.Integration = QA.Integration || (function () {
    var getHostUID = function () { return window.name; };

    showQPForm = function (id, contentId, callback, isInTab, isCreate, fieldsToSet, fieldsToBlock, action) {
        var uid = QA.Utils.newUID();

        var actionCode = action || "edit_article";
        var entityTypeCode = "article";

        if (actionCode == "view_archive_article")
            entityTypeCode = "archive_article";

        var observer = new Quantumart.QP8.Interaction.BackendEventObserver("observer" + uid, function (eventType, args) {
            console.log(eventType);
            console.log(args);
            if (args.actionUID != uid) {
                console.log('not matched: ' + uid);
                return;
            }

            callback(eventType, args);
        });

        if (isCreate) { actionCode = "new_article";}

        var executeOptions = new Quantumart.QP8.Interaction.ExecuteActionOtions();
        executeOptions.actionCode = actionCode;
        executeOptions.entityTypeCode = entityTypeCode;
        executeOptions.entityId = id;

        if (isCreate) {
            executeOptions.entityId = 0;
        }

        executeOptions.parentEntityId = contentId;
        executeOptions.actionUID = uid;
        executeOptions.callerCallback = observer.callbackProcName;
        executeOptions.options = new Quantumart.QP8.Interaction.ArticleFormState();

        if (fieldsToSet) {
            executeOptions.options.initFieldValues = fieldsToSet;
        }

        if (fieldsToBlock) {
            executeOptions.options.disabledFields = fieldsToBlock;
        }
        // убираем save
        executeOptions.options.disabledActionCodes = ["update_article"];

        var hostId = QA.Utils.hostId();

        console.log('hostId: ' + hostId);

        if (!isInTab) {
            executeOptions.isWindow = true;
            executeOptions.changeCurrentTab = false;
        } else {
            executeOptions.isWindow = false;
            executeOptions.changeCurrentTab = false;
        }

        var backendWnd = QA.Utils.getParent();

        console.log(executeOptions);
        Quantumart.QP8.Interaction.executeBackendAction(executeOptions, hostId, backendWnd);
    };


    return {
        showQPForm: showQPForm,
        getHostUID: getHostUID
    };
})();