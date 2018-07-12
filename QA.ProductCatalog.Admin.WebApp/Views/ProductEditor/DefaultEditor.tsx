import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "Packages/react-lazy-i18n";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldList } from "Components/FieldEditors/FieldEditors";

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
      relationEditors={{
        Region: RelationFieldListDefault,
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
      {(model, contentSchema) => (
        <ArticleEditor
          model={model}
          contentSchema={contentSchema}
          titleField={p => p.MarketingProduct && p.MarketingProduct.Title}
          header
          buttons
        />
      )}
    </ProductEditor>
  </LocaleContext.Provider>
);

const RelationFieldListDefault = props => (
  <RelationFieldList displayField="Title" orderByField="Title" {...props} />
);

ReactDOM.render(<App />, document.getElementById("editor"));
