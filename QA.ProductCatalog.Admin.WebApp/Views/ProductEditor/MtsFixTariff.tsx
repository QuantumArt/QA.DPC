// import "normalize.css/normalize.css";
// import React from "react";
// import ReactDOM from "react-dom";
import { toJS } from "mobx";
// import { getSnapshot } from "mobx-state-tree";
// import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
// import { ArticleService } from "Services/ArticleService";
import { SerializationService } from "Services/SerializationService";
import { NormalizationService } from "Services/NormalizationService";
import { DataContextService } from "Services/DataContextService";

(async () => {
  const element = document.getElementById("editor");
  const productDefinitionId = Number(document.location.pathname.split("/").slice(-3)[0]);
  const articleId = Number(document.location.pathname.split("/").slice(-1)[0]);
  const rootUrl = document.location.href.slice(0, document.location.href.indexOf("/ProductEditor"));
  const query = document.location.search;

  const serializationService = new SerializationService();
  const normalizationService = new NormalizationService();
  const dataContextService = new DataContextService();

  const schemaInitializationPromise = fetch(
    `${rootUrl}/ProductEditor/GetProductSchema_Test${query}&productDefinitionId=${productDefinitionId}`
  ).then(async response => {
    if (response.ok) {
      const schema = await response.json();
      normalizationService.initSchema(schema.MergedSchemas);
      dataContextService.initSchema(schema.MergedSchemas);
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

    dataContextService.initStore(storeSnapshot);

    console.dir(storeSnapshot);

    const product = dataContextService.store.Product.get(String(2254329));

    console.dir(toJS(product));

    if (1) {
      // const region =  dataContextService.store.Region.create({ Id: -12345 });
      // product.Regions.push(region);
      // console.dir(getSnapshot(dataContextService.store.Region));
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
