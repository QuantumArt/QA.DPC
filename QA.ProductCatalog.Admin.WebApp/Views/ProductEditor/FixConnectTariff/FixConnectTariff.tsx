import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "Packages/react-lazy-i18n";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { MultiRelationFieldTags, RelationFieldTabs } from "Components/FieldEditors/FieldEditors";
import { EditorTabs } from "./Components/EditorTabs";
import { Product } from "./ProductEditorSchema";
import { AdvantagesTable } from "./Components/AdvantagesTable";
import "./FixConnectTariff.scss";

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
      relationEditors={{
        Region: props => <MultiRelationFieldTags {...props} orderByField="Title" />,
        Advantage: AdvantagesTable,
        ProductParameter: ProductParameterTabs
      }}
    >
      {(model: Product, contentSchema) => (
        <EditorTabs model={model} contentSchema={contentSchema} />
      )}
    </ProductEditor>
  </LocaleContext.Provider>
);

const ProductParameterTabs = props => (
  <RelationFieldTabs {...props} displayField="Title" orderByField="Title">
    {(_headerNode, fieldsNode) => fieldsNode}
  </RelationFieldTabs>
);

ReactDOM.render(<App />, document.getElementById("editor"));
