import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "Packages/react-lazy-i18n";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldTags, RelationFieldTabs } from "Components/FieldEditors/FieldEditors";
import { FixConnectTariffEditor } from "./FixConnectTariffEditor";

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
      relationEditors={{
        Region: props => <RelationFieldTags {...props} selectMultiple orderByField="Title" />,
        ProductModifer: RelationFieldTagsEditor,
        BaseParameter: RelationFieldTagsEditor,
        Unit: RelationFieldTagsEditor,
        Segment: RelationFieldTagsEditor,
        TariffCategory: RelationFieldTagsEditor,
        ProductParameter: ProductParameterEditor
      }}
    >
      {(model, contentSchema) => (
        <FixConnectTariffEditor model={model} contentSchema={contentSchema} />
      )}
    </ProductEditor>
  </LocaleContext.Provider>
);

const RelationFieldTagsEditor = props => (
  <RelationFieldTags {...props} displayField="Title" orderByField="Title" />
);

const ProductParameterEditor = props => (
  <RelationFieldTabs {...props} displayField="Title" orderByField="Title">
    {(_headerNode, fieldsNode) => fieldsNode}
  </RelationFieldTabs>
);

ReactDOM.render(<App />, document.getElementById("editor"));
