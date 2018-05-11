﻿const jsonToTypeScript = require("json-schema-to-typescript/dist/bundle");

// @ts-ignore
const jsonSchema = window.jsonSchema;
// @ts-ignore
jsonToTypeScript
  .compile(jsonSchema, "ProductDefinition")
  .then(fixJsonDeclarations)
  .then(jsonDeclarations => {
    // @ts-ignore
    document.querySelector("#typeScriptCode").innerText = jsonDeclarations;

    download("ProductDefinition.ts", jsonDeclarations);
  });

/**
 * Убрать лишние экспорты интерфейсов из TypeScript кода описания продукта
 * @param {string} jsonDeclarations
 * @returns {string} Код TypeScript
 */
function fixJsonDeclarations(jsonDeclarations) {
  return jsonDeclarations
    .replace(/^export interface /gm, "interface ")
    .replace(
      /^interface ProductDefinition {$/m,
      "export interface ProductDefinition {"
    );
}

/**
 * Скачать текст как UTF-8 файл
 * @param {string} filename
 * @param {string} text
 */
function download(filename, text) {
  const element = document.createElement("a");
  element.setAttribute(
    "href",
    "data:text/plain;charset=utf-8," + encodeURIComponent(text)
  );
  element.setAttribute("download", filename);

  element.style.display = "none";
  document.body.appendChild(element);

  element.click();

  document.body.removeChild(element);
}
