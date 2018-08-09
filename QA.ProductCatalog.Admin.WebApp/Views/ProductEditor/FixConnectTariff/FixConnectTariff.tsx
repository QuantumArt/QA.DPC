import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "Packages/react-lazy-i18n";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldTags } from "Components/FieldEditors/FieldEditors";
import { FixConnectTariffEditor } from "./FixConnectTariffEditor";

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
      relationEditors={{
        Region: props => <RelationFieldTags selectMultiple orderByField="Title" {...props} />,
        ProductModifer: RelationFieldTagsDefault,
        BaseParameter: RelationFieldTagsDefault,
        Unit: RelationFieldTagsDefault,
        Segment: RelationFieldTagsDefault,
        TariffCategory: RelationFieldTagsDefault
      }}
    >
      {(model, contentSchema) => (
        <FixConnectTariffEditor model={model} contentSchema={contentSchema} />
      )}
    </ProductEditor>
  </LocaleContext.Provider>
);

const RelationFieldTagsDefault = props => (
  <RelationFieldTags displayField="Title" orderByField="Title" {...props} />
);

ReactDOM.render(<App />, document.getElementById("editor"));
