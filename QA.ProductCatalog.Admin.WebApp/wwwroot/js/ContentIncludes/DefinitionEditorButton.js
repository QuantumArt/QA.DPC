function addEditDefinitionButton(contentFieldId, xmlFieldId, dpcUrl) {
  QP_CURRENT_CONTEXT.addCustomLinkButton({
    name: "field_" + xmlFieldId,
    title: "Редактировать описание",
    class: "customLinkButton",
    url: "/Backend/Content/QP8/icons/16x16/action.gif",
    onClick: function(evt) {
      var contentId = parseInt(
        evt.data.$form.find("[name=field_" + contentFieldId + "]").val()
      );

      var xmlField = evt.data.$form.find("[name=field_" + xmlFieldId + "]");

      var xml = xmlField.val();

      if (isNaN(contentId) && !xml) {
        alert("Необходимо сначало выбрать контент");

        return;
      }

      var urlQs = isNaN(contentId) ? "" : "?contentId=" + contentId;

      var editorWindow = $.telerik.window
        .create({
          title: "Редактор описаний",
          contentUrl: dpcUrl + "/DefinitionEditor" + urlQs,
          actions: ["Refresh", "Maximize", "Close"],
          modal: true,
          resizable: true,
          draggable: true,
          scrollable: false,
          onClose: function() {
            $(this)
              .data("tWindow")
              .content(" ");
          }
        })
        .data("tWindow");

      editorWindow.center();

      editorWindow.maximize();

      var editorIframe = $("iframe", editorWindow.element)[0];

      pmrpc.register({
        publicProcedureName: "DefinitionEditorLoaded",
        procedure: function(setXmlProcName) {
          if (window.console) {
            console.log("DefinitionEditorLoaded called with ", setXmlProcName);

            console.log("Calling " + setXmlProcName);
          }

          pmrpc.call({
            destination: editorIframe.contentWindow,
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
          xmlField.val(xmlToSave);

          editorWindow.close();
        }
      });
    }
  });
}
