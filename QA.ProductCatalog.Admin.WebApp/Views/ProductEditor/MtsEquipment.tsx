import "normalize.css/normalize.css";
import React from "react";
import ReactDOM from "react-dom";
import { observable } from "mobx";
import { ArticleEditor } from "../../ClientApp/Components/ArticleEditor/ArticleEditor";
import { productDefinitionSchema } from "../../ClientApp/Editors/MtsEquipment/ProductDefinition";

(async () => {
  const element = document.getElementById("editor");
  const articleId = Number(document.location.pathname.split("/").slice(-1)[0]);
  const rootUrl = document.location.href.slice(0, document.location.href.indexOf("/ProductEditor"));
  const query = document.location.search;

  const response = await fetch(
    `${rootUrl}/ProductEditor/GetProduct${query}&productTypeId=503&id=${articleId}`
  );
  if (response.ok) {
    const article = observable(await response.json());

    ReactDOM.render(
      <ArticleEditor article={article} contentSchema={productDefinitionSchema} />,
      element
    );
  } else {
    element.innerHTML = await response.text();
  }
})();
