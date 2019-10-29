DefinitionEditor = {
  init: function(
    saveToDb,
    saveDefinitionUrl,
    getSingleNodeUrl,
    getDefinitionLevelUrl,
    editUrl
  ) {
    DefinitionEditor._saveToDb = saveToDb;

    DefinitionEditor._saveDefinitionUrl = saveDefinitionUrl;

    DefinitionEditor._getSingleNodeUrl = getSingleNodeUrl;

    DefinitionEditor._getDefinitionLevelUrl = getDefinitionLevelUrl;

    DefinitionEditor._editUrl = editUrl;

    $("#verticalRootSplitter").kendoSplitter({
      orientation: "vertical",
      panes: [{ collapsible: true, size: "60%" }, { collapsible: false }]
    });

    $("#horizontalTreeAndPropsSplitter").kendoSplitter({
      panes: [{ collapsible: false, size: "30%" }, { collapsible: false }]
    });

    $("#dvToolbar").kendoToolBar({
      items: [
        {
          type: "button",
          text: "Сохранить",
          enable: false,
          id: "SaveButton",
          click: DefinitionEditor.SaveXml
        },
        {
          type: "button",
          text: "Править XML",
          id: "EditXml",
          click: DefinitionEditor.StartEditXml
        },
        {
          type: "button",
          text: "Завершить правку XML",
          id: "EndEditXmlBtn",
          click: DefinitionEditor.EndEditXml,
          hidden: true
        }
      ]
    });

    if (DefinitionEditor._saveToDb) DefinitionEditor.InitTree();
    else {
      pmrpc.register({
        publicProcedureName: "DefinitionEditor.SetXml",
        procedure: function(xml) {
          console.log("DefinitionEditor.SetXml called with", xml);

          var xmlEmpty = xml.match(/ contentid="\d+"/i) == null;

          if (!xmlEmpty) DefinitionEditor.SetXml(xml);

          DefinitionEditor.InitTree();
        }
      });

      pmrpc.call({
        destination: parent,
        publicProcedureName: "DefinitionEditorLoaded",
        params: ["DefinitionEditor.SetXml"],
        onError: function(statusObj) {
          console.log("Error calling DefinitionEditorLoaded", statusObj);
        }
      });
    }
  },

  UpdateFormAfterSave: function(formHtml) {
    $("#propsPane").html(formHtml);

    DefinitionEditor.SetXml($("#propsPane #Xml").val());

    DefinitionEditor.getToolbar().enable("#SaveButton");

    DefinitionEditor.UpdateNode($("#propsPane #Path").val());
  },

  SaveXml: function() {
    var xml = $("#xmlViewerDiv")
      .data("editor")
      .getValue();

    if (DefinitionEditor._saveToDb) {
      DefinitionEditor.getToolbar().enable("#SaveButton", false);

      kendo.ui.progress($(document.body), true);

      $.ajax({
        url: DefinitionEditor._saveDefinitionUrl,
        dataType: "json",
        cache: false,
        type: "POST",
        data: { xml: xml },
        success: function() {
          DefinitionEditor.getToolbar().enable("#SaveButton");
        },
        error: function(result) {
          $("#propsPane").html(result.responseText);
        },
        complete: function() {
          kendo.ui.progress($(document.body), false);
        }
      });
    } else {
      pmrpc.call({
        destination: parent,
        publicProcedureName: "SaveXmlToDefinitionField",
        params: [xml]
      });
    }
  },
  OnTreeNodeSaveError: function(xhr, status, error) {
    console.log(status);

    console.log(xhr);

    alert("Error: " + error);
  },
  UpdateNode: function(path) {
    kendo.ui.progress($("#treeViewContainer"), true);

    $.ajax({
      url: DefinitionEditor._getSingleNodeUrl,
      dataType: "json",
      cache: false,
      type: "POST",
      data: {
        path: path,
        xml: $("#xmlViewerDiv")
          .data("editor")
          .getValue()
      },
      success: DefinitionEditor.BindNode,
      complete: function() {
        kendo.ui.progress($("#treeViewContainer"), false);
      }
    });
  },

  SetXml: function(xml) {
    $("#xmlViewerDiv")
      .data("editor")
      .setValue(xml);
  },
  getToolbar: function() {
    return $("#dvToolbar").data("kendoToolBar");
  },

  StartEditXml: function() {
    $("#xmlViewerDiv")
      .data("editor")
      .setOption("readOnly", false);

    var toolbar = DefinitionEditor.getToolbar();

    toolbar.hide($("#SaveButton"));

    toolbar.hide($("#EditXml"));

    toolbar.show($("#EndEditXmlBtn"));

    $("#verticalRootSplitter")
      .data("kendoSplitter")
      .collapse(".k-pane:first");
  },

  EndEditXml: function() {
    DefinitionEditor.getToolbar().hide($("#EndEditXmlBtn"));

    $("#treeView")
      .data("kendoTreeView")
      .destroy();

    $("#treeView").empty();

    DefinitionEditor.InitTree(
      function() {
        var toolbar = DefinitionEditor.getToolbar();

        $("#xmlViewerDiv")
          .data("editor")
          .setOption("readOnly", true);

        toolbar.show($("#SaveButton"));

        toolbar.show($("#EditXml"));

        toolbar.hide($("#EndEditXmlBtn"));

        toolbar.enable("#SaveButton");

        $("#verticalRootSplitter")
          .data("kendoSplitter")
          .expand(".k-pane:first");
      },
      function() {
        DefinitionEditor.getToolbar().show($("#EndEditXmlBtn"));

        alert("Некорректный XAML!");
      }
    );
  },

  BindNode: function(nodeData) {
    var treeView = $("#treeView").data("kendoTreeView");

    var nodeId = nodeData.Id;

    if (!nodeId && nodeData.MissingFieldToDeleteId)
      nodeId = nodeData.MissingFieldToDeleteId;

    var nodeDataItem = treeView.dataSource.get(nodeId);

    var treeNode = treeView.findByUid(nodeDataItem.uid);

    if (!nodeData.MissingFieldToDeleteId)
      treeView.insertAfter(nodeData, treeNode);

    treeView.remove(treeNode);
  },

  InitTree: function(successParse, errorParse) {
    $("#treeView").kendoTreeView({
      dataSource: {
        transport: {
          read: function(options) {
            kendo.ui.progress($("#treeViewContainer"), true);

            $.ajax({
              url: DefinitionEditor._getDefinitionLevelUrl,
              dataType: "json",
              cache: false,
              type: "POST",
              data: {
                path: options.data.Id,
                xml: $("#xmlViewerDiv")
                  .data("editor")
                  .getValue()
              },
              success: function(result) {
                kendo.ui.progress($("#treeViewContainer"), false);
                options.success(result);
                if (successParse) successParse();
              },
              error: function(result) {
                kendo.ui.progress($("#treeViewContainer"), false);
                options.error(result);
                if (errorParse) errorParse();
              }
            });
          }
        },
        schema: {
          model: {
            id: "Id"
          }
        }
      },
      select: function(e) {
        var dataItem = this.dataItem(e.node);

        kendo.ui.progress($("#propsPane"), true);

        $.ajax({
          url: DefinitionEditor._editUrl,
          cache: false,
          type: "POST",
          data: {
            path: dataItem.Id,
            xml: $("#xmlViewerDiv")
              .data("editor")
              .getValue()
          },
          success: function(result) {
            $("#propsPane").html(result);
          },
          error: function(result) {
            $("#propsPane").html(result.responseText);
          },
          complete: function() {
            kendo.ui.progress($("#propsPane"), false);
          }
        });
      },
      template: $("#treeNodeTemplate").html()
    });
  },

  ShowOrHideFieldProps: function() {
    var dvFieldProps = $("#FieldProps");

    var inDefCheckbox = $("#inDefinitionCheckbox");

    if (inDefCheckbox.length == 0) return;

    if (inDefCheckbox[0].checked) dvFieldProps.show();
    else dvFieldProps.hide();
  },
  onTreeNodeSaveBegin: function() {
    kendo.ui.progress($("#propsPane"), true);
  },
  onTreeNodeSaveComplete: function() {
    kendo.ui.progress($("#propsPane"), false);
  }
};
