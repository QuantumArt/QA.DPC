﻿import "ProductEditor/Environment";
import React from "react";
import ReactDOM from "react-dom";
import { EntityEditor } from 'ProductEditor/Components/ArticleEditor/EntityEditor';
import { ProductEditor } from 'ProductEditor/Components/ProductEditor/ProductEditor';
import { RelationFieldTags } from 'ProductEditor/Components/FieldEditors/FieldEditors';

const App = () => (
  <ProductEditor
    editorSettings={window["ProductEditorSettings"]}
    relationEditors={{
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
    }}
  >
    {(model, contentSchema) => (
      <EntityEditor
        withHeader
        model={model}
        contentSchema={contentSchema}
        titleField={p => p.MarketingProduct && p.MarketingProduct.Title}
      />
    )}
  </ProductEditor>
);

const RelationFieldTagsDefault = props => (
  <RelationFieldTags displayField="Title" sortItemsBy="Title" {...props} />
);

ReactDOM.render(<App />, document.getElementById("editor"));
