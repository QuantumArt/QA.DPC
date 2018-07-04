import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { toJS } from "mobx";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import {
  RelationFieldList,
  RelationFieldTable,
  RelationFieldTabs
} from "Components/FieldEditors/FieldEditors";
import { maxCount } from "Utils/Validators";
import { Product, DeviceOnTariffs } from "./MtsFixTariff/ProductEditorSchema";

const App = () => (
  <ProductEditor
    relationEditors={{
      Region: props => (
        <RelationFieldList
          selectMultiple
          validate={maxCount(25)}
          orderByField="Title"
          onClick={(_e, a) => console.log(toJS(a))}
          {...props}
        />
      ),
      DeviceOnTariffs: props => (
        <RelationFieldTabs
          displayField={(d: DeviceOnTariffs) => d.Parent && d.Parent.Title}
          collapsed
          vertical
          {...props}
        />
      ),
      BaseParameterModifier: RelationFieldTable,
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
      >
        {(header, fields) => (
          <>
            {header}
            {fields}
            <hr />
          </>
        )}
      </ArticleEditor>
    )}
  </ProductEditor>
);

ReactDOM.render(<App />, document.getElementById("editor"));
