import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "Packages/react-lazy-i18n";
import { EntityEditor } from "Components/ArticleEditor/EntityEditor";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldTags } from "Components/FieldEditors/FieldEditors";

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
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
  </LocaleContext.Provider>
);

const RelationFieldTagsDefault = props => (
  <RelationFieldTags displayField="Title" orderByField="Title" {...props} />
);

ReactDOM.render(<App />, document.getElementById("editor"));
