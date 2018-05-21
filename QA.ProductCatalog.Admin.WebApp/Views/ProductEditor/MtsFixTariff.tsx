// import "normalize.css/normalize.css";
// import React from "react";
// import ReactDOM from "react-dom";
// import { toJS } from "mobx";
// import { types, getSnapshot } from "mobx-state-tree";
import { normalize } from "normalizr";
// import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
// import { ArticleService } from "Services/ArticleService";
import { SerializationService } from "Services/SerializationService";
import { ProductShape } from "Editors/MtsFixTariff/ProductEditorNormalizrSchema";
import Store from "Editors/MtsFixTariff/ProductEditorMobxModel";
// import schema from "Editors/MtsFixTariff/ProductEditorSchema";

// // @ts-ignore
// window.types = window.t = types;
// // @ts-ignore
// window.toJS = toJS;

(async () => {
  const element = document.getElementById("editor");
  const articleId = Number(document.location.pathname.split("/").slice(-1)[0]);
  const rootUrl = document.location.href.slice(0, document.location.href.indexOf("/ProductEditor"));
  const query = document.location.search;

  const response = await fetch(
    `${rootUrl}/ProductEditor/GetProduct_Test${query}&articleId=${articleId}`
  );
  if (response.ok) {
    const serializationService = new SerializationService();
    const productObject = serializationService.deserialize(await response.text());
    const storeSnapshot = normalize(productObject, ProductShape).entities;

    console.dir(storeSnapshot.MarketingProduct);

    if (0) {
      const store = Store.create(storeSnapshot);
      console.dir(store);
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
