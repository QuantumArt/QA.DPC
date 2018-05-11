import "normalize.css/normalize.css";
import React from "react";
import ReactDOM from "react-dom";
import { observable } from "mobx";
import { ArticleEditor } from "../../ClientApp/Components/ArticleEditor/ArticleEditor";
import { productEditorSchema as schema } from "../../ClientApp/Editors/MtsFixTariff/ProductEditorSchema";

(async () => {
  const element = document.getElementById("editor");
  const articleId = Number(document.location.pathname.split("/").slice(-1)[0]);
  const rootUrl = document.location.href.slice(0, document.location.href.indexOf("/ProductEditor"));
  const query = document.location.search;

  const response = await fetch(
    `${rootUrl}/ProductEditor/GetProductTest${query}&articleId=${articleId}`
  );
  if (response.ok) {
    const article = await response.json();

    console.log(article);

    ReactDOM.render(
      <ArticleEditor
        article={observable(article)}
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
