const jsonToTypeScript = require("json-schema-to-typescript/dist/bundle");

// @ts-ignore
const jsonSchema = window.productEditorSchema.jsonSchema;
// @ts-ignore
const editorSchema = window.productEditorSchema.editorSchema;
// @ts-ignore
jsonToTypeScript
  .compile(jsonSchema, "ProductDefinition")
  .then(fixJsonDeclarations)
  .then(jsonDeclarations => {
    const editorDeclarations = compileEritorSchema(editorSchema);

    const code = jsonDeclarations + "\n" + editorDeclarations;

    document.querySelector("#typeScriptCode").innerHTML = code;

    download("ProductDefinition.ts", code);
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
 * Преобразовать JSON схемы в интерфейсы TypeScript (для объекта схемы)
 * @param {object} editorSchema
 * @returns {string} Код TypeScript
 */
function compileEritorSchema(editorSchema) {
  const productSchema = JSON.parse(JSON.stringify(editorSchema));
  const definitions = productSchema.Definitions;
  delete productSchema.Definitions;

  const result = [
    `export interface ProductDefinitionSchema ${replaceRefs(productSchema)}`
  ];

  Object.keys(definitions).forEach(name => {
    result.push(`interface ${name}Schema ${replaceRefs(definitions[name])}`);
  });

  return result.join("\n");
}

/**
 * Заменить ссылки вида { $ref: "#/Definitions/Type" } на имена интерфейсов вида Type
 * @param {object} schema
 * @returns {string} Код TypeScript
 */
function replaceRefs(schema) {
  const MARKER = "__gGDoQEM6rY3nczztvX68__";
  const typeMarkerRegex = new RegExp(`"${MARKER}([a-z]+)${MARKER}"`, "g");
  const refRegex = /\{\s*"?\$ref"?:\s*"#\/Definitions\/([A-Za-z0-9]+)"\s*}/gm;
  const valueTypeReplacer = (key, value) => {
    if (key === "$ref") {
      return value;
    }
    if (typeof value === "number") {
      return `${MARKER}number${MARKER}`;
    }
    if (
      typeof value === "string" &&
      !/^[-_$A-Za-z][-_$A-Za-z0-9]*$/.test(value)
    ) {
      return `${MARKER}string${MARKER}`;
    }
    return value;
  };
  return prettyPrint(schema, valueTypeReplacer)
    .replace(typeMarkerRegex, "$1")
    .replace(refRegex, "$1Schema");
}

/**
 * Преобразовать объект в его код на JavaScript (без циклических ссылок)
 * @param {object} object
 * @param {function(string, any): any} [replacer]
 * @returns {string} Код JavaScript
 */
function prettyPrint(object, replacer = (_k, v) => v) {
  const MARKER = "__aklB9Qpj5Gtf7X2wEoYV__";
  const keyRegex = /^[_$A-Za-z][_$A-Za-z0-9]*$/;
  const keyMarkerRegex = new RegExp(
    `"${MARKER}(${keyRegex.source.slice(1, -1)})${MARKER}":`,
    "g"
  );
  const objectKeyReplacer = value => {
    if (isObject(value) && !("$ref" in value)) {
      const res = {};
      Object.keys(value).forEach(key => {
        if (keyRegex.test(key)) {
          res[`${MARKER}${key}${MARKER}`] = value[key];
        } else {
          res[key] = value[key];
        }
      });
      return res;
    }
    return value;
  };

  const combinedReplacer = (key, value) =>
    objectKeyReplacer(replacer(key, value));

  // prettier-ignore
  return JSON.stringify(object, combinedReplacer, 2)
    .replace(keyMarkerRegex, "$1:");
}

function isObject(value) {
  return typeof value === "object" && value !== null && !Array.isArray(value);
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
