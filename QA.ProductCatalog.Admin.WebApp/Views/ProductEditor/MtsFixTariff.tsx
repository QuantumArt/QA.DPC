import "normalize.css/normalize.css";
// import React from "react";
// import ReactDOM from "react-dom";
import { types, getSnapshot } from "mobx-state-tree";
// import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
// import { ArticleService } from "Services/ArticleService";
import { SerializationService } from "Services/SerializationService";
import { Product } from "Editors/MtsFixTariff/ProductEditorModel";
// import schema from "Editors/MtsFixTariff/ProductEditorSchema";

// @ts-ignore
window.types = window.t = types;

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
    const productSnapshot = serializationService.deserialize(await response.text());

    // const articleService = new ArticleService(productSnapshot);
    // const rootArticle = articleService.rootArticle;

    // console.log(serializationService.serialize(rootArticle));

    const productTree = getSnapshot(Product.create(productSnapshot));
    console.log(productTree);

    // ReactDOM.render(
    //   <ArticleEditor
    //     article={rootArticle}
    //     contentSchema={schema}
    //     contentPaths={schema.include(p => [
    //       p.MarketingProduct.include(m => [m.Modifiers]),
    //       p.Modifiers
    //     ])}
    //   />,
    //   element
    // );
  } else {
    element.innerHTML = await response.text();
  }
})();
