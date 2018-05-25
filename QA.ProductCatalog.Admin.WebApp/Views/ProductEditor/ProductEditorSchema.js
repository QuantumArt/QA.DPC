import { download } from "Utils/Download";
import { isObject } from "Utils/TypeChecks";
import {
  isRelationField,
  isBackwardField,
  isExtensionField,
  isEnumField
} from "Models/EditorSchemaModels";

const dataInterfaces = compileEditorDataInterfaces(
  // @ts-ignore
  window.mergedSchema,
  // @ts-ignore
  window.editorSchema
);

// @ts-ignore
const schemaInterfaces = compileEditorSchemaInterfaces(window.editorSchema);

download("ProductEditorSchema.ts", dataInterfaces + "\n" + schemaInterfaces);

// @ts-ignore
document.querySelector("#typeScriptCode").innerText =
  dataInterfaces + "\n" + schemaInterfaces;

function getRefRegex() {
  return /\{\s*"?\$ref"?:\s*"#\/Definitions\/([A-Za-z0-9]+)"\s*}/gm;
}

function getRootName(editorSchema) {
  return (
    editorSchema.Content.ContentName ||
    getRefRegex().exec(JSON.stringify(editorSchema.Content))[1]
  );
}

/**
 * Преобразовать JSON схемы в интерфейсы TypeScript (для объектов данных)
 * @param {object} mergedSchemas
 * @param {object} editorSchema
 * @returns {string} Код TypeScript
 */
function compileEditorDataInterfaces(mergedSchemas, editorSchema) {
  const getFields = content => mergedSchemas[content.ContentId].Fields;
  const getName = content => mergedSchemas[content.ContentId].ContentName;
  const print = func => func();
  const forEach = (obj, filter, map) => {
    if (!map) {
      map = filter;
      filter = () => true;
    }
    return Object.values(obj)
      .filter(filter)
      .map(map)
      .join("");
  };

  // prettier-ignore
  return `/** Типизация хранилища данных */
export interface ${getRootName(editorSchema)}Entities {${
  forEach(mergedSchemas, content => !content.ForExtension, content => `
  ${getName(content)}: ${getName(content)};`)}
}
${forEach(mergedSchemas, content => `
export interface ${getName(content)} {${
content.ForExtension
  ? `
  ContentName: "${getName(content)}";`
  : `
  Id: number;
  ContentName: "${getName(content)}";
  Timestamp: Date;`
}${forEach(getFields(content), field => `
  ${field.FieldName}: ${print(() => {

    if (isExtensionField(field)) {
      return `${forEach(field.Contents, content => `
    | "${getName(content)}"`)};
  ${field.FieldName}_Contents: {${forEach(field.Contents, content => `
    ${getName(content)}: ${getName(content)};`)}
  }`;
    }

    if (isBackwardField(field)) {
      return `${getName(field.Content)}[]`;
    }

    if (isRelationField(field)) {
      switch (field.FieldType) {
        case "M2MRelation":
        case "M2ORelation":
          return `${getName(field.Content)}[]`;
        case "O2MRelation":
          return `${getName(field.Content)}`;
      }
    }

    if (isEnumField(field)) {
      return forEach(field.Items, item => `
    | "${item.Value}"`);
    }
    
    switch (field.FieldType) {
      case "String":
      case "Textbox":
      case "VisualEdit":
      case "Classifier":
        return `string`;
      case "Numeric":
      case "O2MRelation":
        return `number`;
      case "Boolean":
        return `boolean`;
      case "Date":
      case "Time":
      case "DateTime":
        return `Date`;
      case "File":
      case "Image":
        return `{
    Name: string;
    AbsoluteUrl: string;
  }`;
      case "DynamicImage":
      default:
        throw new Error(
          `Field "${field.FieldName}" has unsupported type FieldExactTypes.${field.FieldType}`
        );
    }
  })};`)}
}
`)}`;
}

/**
 * Преобразовать JSON схемы в интерфейсы TypeScript (для объекта схемы)
 * @param {object} editorSchema
 * @returns {string} Код TypeScript
 */
function compileEditorSchemaInterfaces(editorSchema) {
  const productSchema = JSON.parse(JSON.stringify(editorSchema));
  const content = productSchema.Content;
  const definitions = productSchema.Definitions;
  delete productSchema.Definitions;
  const rootName = getRootName(editorSchema) + "Schema";

  // prettier-ignore
  return `
/** Описание полей продукта */
export interface ${rootName} ${replaceRefs(content, rootName)}
${Object.keys(definitions).map(name => `
interface ${name}Schema ${replaceRefs(
  definitions[name],
  `${name}Schema`
)}`).join("")}
`;
}

/**
 * Заменить ссылки вида { $ref: "#/Definitions/Type" } на имена интерфейсов вида Type
 * @param {object} schema
 * @param {string} interfaceName
 * @returns {string} Код TypeScript
 */
function replaceRefs(schema, interfaceName) {
  const MARKER = "__gGDoQEM6rY3nczztvX68__";
  const identifierRegex = /^[-_$A-Za-z][-_$A-Za-z0-9]*$/;
  const typeRegex = new RegExp(`${MARKER}(.+)${MARKER}`);
  const typeStringRegex = new RegExp(`"${MARKER}(.+)${MARKER}"`, "g");

  function visitObject(object, path) {
    if (typeof object === "object" && object !== null) {
      if (Array.isArray(object)) {
        object.forEach((value, index) => {
          visitObject(value, path + `['${index}']`);
        });
      } else {
        Object.keys(object).forEach(key => {
          if (
            object.ContentId &&
            Object.keys(object.Fields).some(
              fieldName =>
                object.Fields[fieldName].Content ||
                object.Fields[fieldName].Contents
            )
          ) {
            object.include = `${MARKER}(selector: (fields: ${path}['Fields']) => Selection[]) => string[]${MARKER}`;
          } else if (object.FieldId) {
            if (
              object.Content &&
              object.Content.ContentId &&
              Object.keys(object.Content.Fields).some(
                fieldName =>
                  object.Content.Fields[fieldName].Content ||
                  object.Content.Fields[fieldName].Contents
              )
            ) {
              object.include = `${MARKER}(selector: (fields: ${path}['Content']['Fields']) => Selection[]) => string[]${MARKER}`;
            } else if (object.Contents) {
              object.include = `${MARKER}(selector: (contents: ${path}['Contents']) => string[][]) => string[]${MARKER}`;
            }
          }
          visitObject(object[key], path + `['${key}']`);
        });
      }
    }
  }

  visitObject(schema, interfaceName);

  const valueTypeReplacer = (key, value) => {
    if (key === "$ref") {
      return value;
    }
    if (
      key === "__aklB9Qpj5Gtf7X2wEoYV__ObjectShape__aklB9Qpj5Gtf7X2wEoYV__" &&
      value === null
    ) {
      return `${MARKER}any${MARKER}`;
    }
    if (typeof value === "number") {
      return `${MARKER}number${MARKER}`;
    }
    if (
      typeof value === "string" &&
      !identifierRegex.test(value) &&
      !typeRegex.test(value)
    ) {
      return `${MARKER}string${MARKER}`;
    }
    return value;
  };

  return prettyPrint(schema, valueTypeReplacer)
    .replace(typeStringRegex, "$1")
    .replace(getRefRegex(), "$1Schema");
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
  const keyStringRegex = new RegExp(
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
    .replace(keyStringRegex, "$1:");
}
