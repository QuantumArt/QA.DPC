import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { Grid } from "react-flexbox-grid";
import { inject } from "react-ioc";
import { EditorController } from "Services/EditorController";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { Product } from "Editors/MtsFixTariff/ProductEditorSchema";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { onPatch } from "mobx-state-tree";
// @ts-ignore
import { RelationFieldList, RelationFieldTable } from "Components/FieldEditors/FieldEditors";
import { toJS } from "mobx";
import { maxCount } from "Utils/Validators";

class Example {
  @inject editorController: EditorController;
  @inject dataContext: DataContext;
  @inject schemaContext: SchemaContext;
}

(async () => {
  const element = document.getElementById("editor");
  const example = new Example();
  try {
    await example.editorController.initialize();

    const product = example.dataContext.store["Product"].get(String(2460423)) as Product;
    // @ts-ignore
    onPatch(product, patch => {
      console.log(patch);
    });

    ReactDOM.render(
      <Grid fluid>
        <ArticleEditor
          model={product}
          contentSchema={example.schemaContext.contentSchema}
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
            _Regions: props => (
              <RelationFieldList
                selectMultiple
                validate={maxCount(25)}
                orderByField="Title"
                onClick={(_e, a) => console.log(toJS(a))}
                {...props}
              />
            ),
            Regions: props => (
              <RelationFieldTable validate={maxCount(25)} orderByField="Title" {...props} />
            ),
            MarketingProduct: RelationFieldTable,
            Parameters: RelationFieldTable
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
      </Grid>,
      element
    );
  } catch (error) {
    element.innerHTML = error.message;
  }
})();
