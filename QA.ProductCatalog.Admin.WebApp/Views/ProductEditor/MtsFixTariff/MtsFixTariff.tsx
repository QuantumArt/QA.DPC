import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "react-lazy-i18n";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldList } from "Components/FieldEditors/FieldEditors";
import { MtsFixTariffEditor } from "./MtsFixTariffEditor";

const settings = window["ProductEditorSettings"];

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      productDefinitionId={settings.ProductDefinitionId}
      articleId={settings.ArticleId}
      relationEditors={{
        Region: props => <RelationFieldList selectMultiple orderByField="Title" {...props} />,
        Group: RelationFieldListDefault,
        ProductModifer: RelationFieldListDefault,
        TariffZone: RelationFieldListDefault,
        Direction: RelationFieldListDefault,
        ParameterModifier: RelationFieldListDefault,
        LinkModifier: RelationFieldListDefault,
        CommunicationType: RelationFieldListDefault,
        Segment: RelationFieldListDefault,
        FixedType: RelationFieldListDefault
      }}
    >
      {(model, contentSchema) => <MtsFixTariffEditor model={model} contentSchema={contentSchema} />}
    </ProductEditor>
  </LocaleContext.Provider>
);

const RelationFieldListDefault = props => (
  <RelationFieldList displayField="Title" orderByField="Title" {...props} />
);

ReactDOM.render(<App />, document.getElementById("editor"));
