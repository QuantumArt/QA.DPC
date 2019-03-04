import * as tslib_1 from "tslib";
import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { MultiRelationFieldTags } from "Components/FieldEditors/FieldEditors";
import { EditorTabs } from "./Components/EditorTabs";
import { AdvantagesTable } from "./Components/AdvantagesTable";
import { ParameterFields } from "./Components/ParameterFields";
import "./FixConnectTariff.scss";
var App = function() {
  return React.createElement(
    ProductEditor,
    {
      editorSettings: window["ProductEditorSettings"],
      relationEditors: {
        Region: function(props) {
          return React.createElement(
            MultiRelationFieldTags,
            tslib_1.__assign({}, props, { sortItemsBy: "Title" })
          );
        },
        ProductParameter: ParameterFields,
        LinkParameter: ParameterFields,
        Advantage: AdvantagesTable
      }
    },
    function(model, contentSchema) {
      return React.createElement(EditorTabs, {
        model: model,
        contentSchema: contentSchema
      });
    }
  );
};
ReactDOM.render(
  React.createElement(App, null),
  document.getElementById("editor")
);
//# sourceMappingURL=FixConnectTariff.js.map
