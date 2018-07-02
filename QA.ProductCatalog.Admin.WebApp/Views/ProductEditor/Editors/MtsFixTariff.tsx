import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { toJS } from "mobx";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import {
  RelationFieldAccordion,
  RelationFieldList,
  RelationFieldTable
} from "Components/FieldEditors/FieldEditors";
import { maxCount } from "Utils/Validators";
import { Product } from "./MtsFixTariff/ProductEditorSchema";

const App = () => (
  <ProductEditor>
    {(product: Product, contentSchema) => (
      <ArticleEditor
        model={product}
        contentSchema={contentSchema}
        titleField={model => model.MarketingProduct && model.MarketingProduct.Title}
        save
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
        fields={{
          Regions1: props => (
            <RelationFieldList
              selectMultiple
              validate={maxCount(25)}
              orderByField="Title"
              onClick={(_e, a) => console.log(toJS(a))}
              {...props}
            />
          ),
          Regions2: props => (
            <RelationFieldTable validate={maxCount(25)} orderByField="Title" {...props} />
          ),
          Regions: props => (
            <RelationFieldAccordion save validate={maxCount(25)} orderByField="Title" {...props} />
          ),
          MarketingProduct: RelationFieldAccordion,
          Parameters: RelationFieldAccordion
        }}
      >
        {(header, fields) => (
          <>
            {header}
            <hr />
            {fields}
            <hr />
          </>
        )}
      </ArticleEditor>
    )}
  </ProductEditor>
);

ReactDOM.render(<App />, document.getElementById("editor"));
