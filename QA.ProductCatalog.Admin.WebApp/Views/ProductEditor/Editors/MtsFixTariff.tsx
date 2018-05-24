// import "normalize.css/normalize.css";
import "reflect-metadata";
// import React from "react";
// import ReactDOM from "react-dom";
import { inject } from "react-ioc";
import { toJS } from "mobx";
import { EditorController } from "Services/EditorController";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";

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

    console.dir(example.schemaContext.rootSchema);

    const product = example.dataContext.store["Product"].get(String(2254329));
    product.Regions[0] = example.dataContext.createArticle("Region");
    console.dir(toJS(example.dataContext.store["Region"]));

    element.innerHTML = `Loaded in ${Number(new Date()) - Number(start)} msec!`;
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
