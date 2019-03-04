import * as tslib_1 from "tslib";
import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldTags } from "Components/FieldEditors/FieldEditors";
var App = function() {
  return React.createElement(
    ProductEditor,
    {
      editorSettings: window["ProductEditorSettings"],
      relationEditors: {
        Region: RelationFieldTagsDefault,
        Group: RelationFieldTagsDefault,
        ProductModifer: RelationFieldTagsDefault,
        TariffZone: RelationFieldTagsDefault,
        Direction: RelationFieldTagsDefault,
        ParameterModifier: RelationFieldTagsDefault,
        LinkModifier: RelationFieldTagsDefault,
        CommunicationType: RelationFieldTagsDefault,
        Segment: RelationFieldTagsDefault,
        FixedType: RelationFieldTagsDefault
      }
    },
    function(model, contentSchema) {
      return React.createElement(EntityEditor, {
        withHeader: true,
        model: model,
        contentSchema: contentSchema,
        titleField: function(p) {
          return p.MarketingProduct && p.MarketingProduct.Title;
        }
      });
    }
  );
};
var RelationFieldTagsDefault = function(props) {
  return React.createElement(
    RelationFieldTags,
    tslib_1.__assign({ displayField: "Title", sortItemsBy: "Title" }, props)
  );
};
ReactDOM.render(
  React.createElement(App, null),
  document.getElementById("editor")
);
//# sourceMappingURL=DefaultEditor.js.map
