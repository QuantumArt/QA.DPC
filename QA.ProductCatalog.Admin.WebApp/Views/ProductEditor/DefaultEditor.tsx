import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldList } from "Components/FieldEditors/FieldEditors";

const settings = window["ProductEditorSettings"];

const App = () => (
  <ProductEditor
    productDefinitionId={settings.ProductDefinitionId}
    articleId={settings.ArticleId}
    relationEditors={{
      Region: props => <RelationFieldList selectMultiple orderByField="Title" {...props} />
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
);

ReactDOM.render(<App />, document.getElementById("editor"));
