import "normalize.css/normalize.css";
import React from "react";
import ReactDOM from "react-dom";
import { ArticleEditor } from "../../ClientApp/Components/ArticleEditor/ArticleEditor";
import { productEditorSchema as schema } from "../../ClientApp/Editors/MtsFixTariff/ProductEditorSchema";
import { ArticleService } from "../../ClientApp/Services/ArticleService";

(async () => {
  const element = document.getElementById("editor");
  const articleId = Number(document.location.pathname.split("/").slice(-1)[0]);
  const rootUrl = document.location.href.slice(0, document.location.href.indexOf("/ProductEditor"));
  const query = document.location.search;

  const response = await fetch(
    `${rootUrl}/ProductEditor/GetProductTest${query}&articleId=${articleId}`
  );
  if (response.ok) {
    const articleService = new ArticleService(await response.json());

    console.log(articleService.serializeArticle(articleService.rootArticle));

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
