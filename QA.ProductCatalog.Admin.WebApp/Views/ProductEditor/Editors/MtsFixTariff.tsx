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
        />
      </Grid>,
      element
    );
  } catch (error) {
    element.innerHTML = error.message;
  }
})();
