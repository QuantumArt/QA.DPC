import "normalize.css/normalize.css";
import React from "react";
import ReactDOM from "react-dom";
import { ArticleEditor } from "Components/ArticleEditor/ArticleEditor";
import { ArticleService } from "Services/ArticleService";
import { SerializationService } from "Services/SerializationService";
import schema from "../../ClientApp/Editors/MtsFixTariff/ProductEditorSchema";

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
    const articleService = new ArticleService(productSnapshot);

    console.log(serializationService.serialize(articleService.rootArticle));

    ReactDOM.render(
      <ArticleEditor
        article={articleService.rootArticle}
        contentSchema={schema}
        contentPaths={schema.include(p => [
          p.MarketingProduct.include(m => [m.Modifiers]),
          p.Modifiers
        ])}
      />,
      element
    );
  } else {
    element.innerHTML = await response.text();
  }
})();
