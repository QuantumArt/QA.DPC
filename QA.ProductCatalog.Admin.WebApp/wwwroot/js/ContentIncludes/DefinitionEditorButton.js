/* Reference only code */
function addEditDefinitionButton(
  contentFieldId,
  xmlFieldId,
  dpcHttpsUrl,
  customerCode
) {
  QP_CURRENT_CONTEXT.addCustomLinkButton({
    name: "field_" + xmlFieldId,
    title: "Редактировать описание",
    class: "customLinkButtonBeta",
    suffix: "1",
    url: "/Backend/Content/QP8/icons/16x16/action.gif",
    actionAlias: "DefinitionEditor",
    entityId: QP_CURRENT_CONTEXT._entityId,
    parentEntityId: QP_CURRENT_CONTEXT._parentEntityId,
    onClick: function(evt) {
      var contentId = parseInt(
        evt.data.$form.find("[name=field_" + contentFieldId + "]").val()
      );

      var xmlField = evt.data.$form.find("[name=field_" + xmlFieldId + "]");

      var xml = xmlField.val();

      if (isNaN(contentId) && !xml) {
        alert("Необходимо сначала выбрать контент");

        return;
      }

      pmrpc.register({
        publicProcedureName: "DefinitionEditorLoaded",
        procedure: function(setXmlProcName) {
          if (window.console) {
            console.log("DefinitionEditorLoaded called with ", setXmlProcName);
            console.log("Calling " + setXmlProcName);
          }
          $("iframe")
            .parents(".popupWindow")
            .find(".t-maximize")
            .click();

          pmrpc.call({
            destination: $("iframe", window.element)[0].contentWindow,
            publicProcedureName: setXmlProcName,
            params: [xml],
            onError: function(statusObj) {
              console.log("Error calling " + setXmlProcName, statusObj);
            }
          });
        }
      });

      pmrpc.register({
        publicProcedureName: "SaveXmlToDefinitionField",
        procedure: function(xmlToSave) {
          xmlField.val(xmlToSave).addClass("changed");

          $("iframe")
            .parents(".popupWindow")
            .find(".t-close")
            .click();
        }
      });

      pmrpc.register({
        publicProcedureName: "CloseEditor",
        procedure: function() {
          $("iframe")
            .parents(".popupWindow")
            .find(".t-close")
            .click();
        }
      });
    }
  });
}
addEditDefinitionButton(1515, 1490);
