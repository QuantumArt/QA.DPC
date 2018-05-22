// import "normalize.css/normalize.css";
// import React from "react";
// import ReactDOM from "react-dom";
import { toJS } from "mobx";
import { getSnapshot, getType, unprotect, onPatch, resolvePath } from "mobx-state-tree";
// import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
// import { ArticleService } from "Services/ArticleService";
import { SerializationService } from "Services/SerializationService";
import { NormalizationService } from "Services/NormalizationService";
import Store, { Region } from "Editors/MtsFixTariff/ProductEditorMobxModel";
// import schema from "Editors/MtsFixTariff/ProductEditorSchema";

// // @ts-ignore
// window.types = window.t = types;
// // @ts-ignore
// window.toJS = toJS;

(async () => {
  const element = document.getElementById("editor");
  const productDefinitionId = Number(document.location.pathname.split("/").slice(-3)[0]);
  const articleId = Number(document.location.pathname.split("/").slice(-1)[0]);
  const rootUrl = document.location.href.slice(0, document.location.href.indexOf("/ProductEditor"));
  const query = document.location.search;

  const serializationService = new SerializationService();
  const normalizationService = new NormalizationService();

  const schemaInitializationPromise = fetch(
    `${rootUrl}/ProductEditor/GetProductSchema_Test${query}&productDefinitionId=${productDefinitionId}`
  ).then(async response => {
    if (response.ok) {
      const schema = await response.json();
      normalizationService.initialize(schema.MergedSchemas);
    } else {
      element.innerHTML = await response.text();
    }
  });

  const response = await fetch(
    `${rootUrl}/ProductEditor/GetProduct_Test${query}&articleId=${articleId}`
  );
  if (response.ok) {
    const productObject = serializationService.deserialize(await response.text());
    await schemaInitializationPromise;

    const storeSnapshot = normalizationService.normalize(productObject, "Product");
    console.dir(storeSnapshot);

    if (0) {
      const store = Store.create(storeSnapshot);
      unprotect(store);
      onPatch(store, ({ op, path }) => {
        console.log({ op, path });
        if (op === "add") {
          const object = resolvePath(store, path);
          const type: any = getType(object);
          if (type.properties && type.properties.Id) {
            store[type.name].put(object);
          }
        }
      });

      const product: any = store.Product.get(String(2254329));
      const region = Region.create({ Id: -12345 });
      product.Regions.push(region);
      console.dir(toJS(product));
      console.dir(getSnapshot(store.Region));
    }

    //     const articleService = new ArticleService(productSnapshot);
    //     const rootArticle = articleService.rootArticle;

    //     console.log(serializationService.serialize(rootArticle));

    //     const productTree = getSnapshot(Product.create(productSnapshot));
    //     console.log(productTree);

    //     ReactDOM.render(
    //       <ArticleEditor
    //         article={rootArticle}
    //         contentSchema={schema}
    //         contentPaths={schema.include(p => [
    //           p.MarketingProduct.include(m => [m.Modifiers]),
    //           p.Modifiers
    //         ])}
    //       />,
    //       element
    //     );
  } else {
    element.innerHTML = await response.text();
  }
})();
