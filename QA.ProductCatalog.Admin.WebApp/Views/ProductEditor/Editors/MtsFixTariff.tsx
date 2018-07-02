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
      BaseParameterModifier: RelationFieldTable,
      Unit: RelationFieldList
    }}
  >
    {(model: Product, contentSchema) => (
      <ArticleEditor
        model={model}
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
        fieldEdiors={{
          MarketingProduct: props => <RelationFieldAccordion save {...props} />
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
