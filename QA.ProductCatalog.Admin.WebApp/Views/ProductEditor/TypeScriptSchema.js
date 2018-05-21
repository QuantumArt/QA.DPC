import { download } from "Utils/Download";
const jsonToTypeScript = require("json-schema-to-typescript/dist/bundle");

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
