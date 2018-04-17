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
    const editorDeclarations = compileEritorSchemaDeclarations(editorSchema);
    const editorObject = compileEritorSchemaObject(editorSchema);

    const code =
      jsonDeclarations + editorDeclarations + "\n" + editorObject + "\n";

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
function compileEritorSchemaDeclarations(editorSchema) {
  const productSchema = JSON.parse(JSON.stringify(editorSchema));
  const content = productSchema.Content;
  const definitions = productSchema.Definitions;
  delete productSchema.Definitions;

  const procuctInterface = `
/** Описание полей продукта */
export interface ProductDefinitionSchema ${replaceRefs(content)}`;

  const result = [procuctInterface];

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
 * Преобразовать JSON схемы в код объекта схемы
 * @param {object} editorSchema
 * @returns {string} Код TypeScript
 */
const compileEritorSchemaObject = editorSchema => `
/** Описание полей продукта */
export const productDefinitionSchema = linkJsonRefs<ProductDefinitionSchema>(${prettyPrint(
  editorSchema
)});

function linkJsonRefs<T>(schema: any): T {
  const definitions = schema.Definitions;
  delete schema.Definitions;
  visitObject(schema, definitions, new Set());
  return schema.Content;
}

function visitObject(object: any, definitions: object, visited: Set<object>): void {
  if (typeof object === "object" && object !== null) {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveRef(value, definitions);
        visitObject(object[index], definitions, visited);
      });
    } else {
      Object.keys(object).forEach(key => {
        object[key] = resolveRef(object[key], definitions);
        visitObject(object[key], definitions, visited);
      });
    }
  }
}

function resolveRef(object: any, definitions: object): any {
  return typeof object === "object" && object !== null && "$ref" in object
    ? definitions[object.$ref.slice(14)]
    : object;
}`;

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
