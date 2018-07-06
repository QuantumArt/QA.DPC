import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "react-lazy-i18n";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { RelationFieldList, RelationFieldTabs } from "Components/FieldEditors/FieldEditors";
import { Product, DeviceOnTariffs } from "./ProductEditorSchema";

const settings = window["ProductEditorSettings"];

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      productDefinitionId={settings.ProductDefinitionId}
      articleId={settings.ArticleId}
      relationEditors={{
        Region: props => <RelationFieldList selectMultiple orderByField="Title" {...props} />,
        DeviceOnTariffs: props => (
          <RelationFieldTabs
            displayField={(d: DeviceOnTariffs) => d.Parent && d.Parent.Title}
            collapsed
            vertical
            {...props}
          />
        ),
        Unit: RelationFieldList
      }}
    >
      {(model: Product, contentSchema) => (
        <ArticleEditor
          model={model}
          contentSchema={contentSchema}
          titleField={(p: Product) => p.MarketingProduct && p.MarketingProduct.Title}
          saveRelations={{
            MarketingProduct: {
              ActionsOnMarketingDevice: true
            },
            Type: {
              FixConnectAction: {
                MarketingOffers: true
              }
            },
            Regions: {
              Parent: true
            }
          }}
          header
          buttons
        />
      )}
    </ProductEditor>
  </LocaleContext.Provider>
);

ReactDOM.render(<App />, document.getElementById("editor"));
