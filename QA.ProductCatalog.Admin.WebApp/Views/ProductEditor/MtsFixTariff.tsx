// import "normalize.css/normalize.css";
// import React from "react";
// import ReactDOM from "react-dom";
import { toJS } from "mobx";
import editorController from "Services/EditorController";
import dataContext from "Services/DataContext";

(async () => {
  const element = document.getElementById("editor");
  try {
    await editorController.initialize();

    console.dir(toJS(dataContext.store["Product"]));
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
  } catch (error) {
    element.innerHTML = error.message;
  }
})();
