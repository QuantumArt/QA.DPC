import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { Grid } from "react-flexbox-grid";
import { inject } from "react-ioc";
import { toJS } from "mobx";
import { EditorController } from "Services/EditorController";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { Product, Region } from "Editors/MtsFixTariff/ProductEditorSchema";
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
    const start = new Date();
    await example.editorController.initialize();

    console.dir(example.schemaContext.contentSchema);

    const product = example.dataContext.store["Product"].get(String(2460423)) as Product;
    const newRegion = example.dataContext.createArticle<Region>("Region");
    newRegion.Parent = example.dataContext.createArticle<Region>("Region");
    product.Regions[0] = newRegion;

    // @ts-ignore
    onPatch(product, patch => {
      console.log(patch);
    });

    console.dir(toJS(example.dataContext.store["Region"]));
    // @ts-ignore
    window.store = example.dataContext.store;

    element.innerHTML = `Loaded in ${Number(new Date()) - Number(start)} msec!`;

    ReactDOM.render(
      <Grid fluid>
        <ArticleEditor
          model={product}
          contentSchema={example.schemaContext.contentSchema}
          includeOnSave={{
            MarketingProduct: {
              ActionsOnMarketingDevice: true
            },
            Type: {
              FixConnectAction: {
                MarketingOffers: true
              }
            }
          }}
          save
        />
      </Grid>,
      element
    );
  } catch (error) {
    element.innerHTML = error.message;
  }
})();
