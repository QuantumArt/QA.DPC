import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "Packages/react-lazy-i18n";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldTags } from "Components/FieldEditors/FieldEditors";
import { MtsFixTariffEditor } from "./MtsFixTariffEditor";

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
      relationEditors={{
        Region: props => <RelationFieldTags selectMultiple orderByField="Title" {...props} />,
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
      {(model, contentSchema) => <MtsFixTariffEditor model={model} contentSchema={contentSchema} />}
    </ProductEditor>
  </LocaleContext.Provider>
);

const RelationFieldTagsDefault = props => (
  <RelationFieldTags displayField="Title" orderByField="Title" {...props} />
);

ReactDOM.render(<App />, document.getElementById("editor"));
