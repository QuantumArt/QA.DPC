import { download } from "Utils/Download";
import { isObject, isInteger, isBoolean } from "Utils/TypeChecks";

// @ts-ignore
const editorSchema = compileEditorSchema(window.editorSchema);
// @ts-ignore
const editorMobxModel = compileMobxModel(window.mergedSchema);
// @ts-ignore
const editorNormalizrSchema = compileNormalizrSchema(window.mergedSchema);

// prettier-ignore
download("ProductEditorSchema.ts", editorSchema);
download("ProductEditorMobxModel.ts", editorMobxModel);
download("ProductEditorNormalizrSchema.ts", editorNormalizrSchema);

// @ts-ignore
document.querySelector("#typeScriptCode").innerText = `
${editorNormalizrSchema}
${editorMobxModel}
${editorSchema}`;

/**
 * Преобразовать JSON схемы в интерфейсы TypeScript (для объекта схемы)
 * @param {object} editorSchema
 * @returns {string} Код TypeScript
 */
function compileEditorSchema(editorSchema) {
  const productSchema = JSON.parse(JSON.stringify(editorSchema));
  const content = productSchema.Content;
  const definitions = productSchema.Definitions;
  delete productSchema.Definitions;
  const rootName = getRootName(editorSchema);

  // prettier-ignore
  return `import { linkJsonRefs } from "Utils/SchemaGeneration";

/** Описание полей продукта */
export interface ${rootName} ${replaceRefs(content, rootName)}
${Object.keys(definitions).map(name => `
interface ${name}Schema ${replaceRefs(
  definitions[name],
  `${name}Schema`
)}`).join("")}

/** Описание полей продукта */
export default linkJsonRefs<${rootName}>(${prettyPrint(editorSchema)});
`;
}

function getRefRegex() {
  return /\{\s*"?\$ref"?:\s*"#\/Definitions\/([A-Za-z0-9]+)"\s*}/gm;
}

function getRootName(editorSchema) {
  return `${editorSchema.Content.ContentName ||
    getRefRegex().exec(JSON.stringify(editorSchema.Content))[1]}Schema`;
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

/*=================================== MobX ==================================*/

function isContentRef(arg) {
  return (
    isObject(arg) && Object.keys(arg).length === 1 && isInteger(arg.ContentId)
  );
}

function isRelation(field) {
  return (
    isObject(field) &&
    isBoolean(field.IsBackward) &&
    isContentRef(field.Content) &&
    field.FieldType.endsWith("Relation") &&
    !field.IsBackward
  );
}

function isBackward(field) {
  return (
    isObject(field) &&
    isBoolean(field.IsBackward) &&
    isContentRef(field.Content) &&
    field.FieldType.endsWith("Relation") &&
    field.IsBackward
  );
}

function isExtension(field) {
  return (
    isObject(field) &&
    isObject(field.Contents) &&
    field.FieldType === "Classifier" &&
    Object.values(field.Contents).every(isContentRef)
  );
}

function isReferenceField(field) {
  return isExtension(field) || isBackward(field) || isRelation(field);
}

function compileMobxModel(mergedSchemas) {
  const contentName = ({ ContentId }) => mergedSchemas[ContentId].ContentName;
  const contentTitle = ({ ContentId }) => mergedSchemas[ContentId].ContentTitle;
  const contentFields = ({ ContentId }) => mergedSchemas[ContentId].Fields;

  // prettier-ignore
  const fieldType = field => {
    if (isExtension(field)) {
      return `t.maybe(t.enumeration("${field.FieldName}", [${
        Object.keys(field.Contents)
          .map(name => `
    "${name}",`)
          .join("")}
  ])),
  /** Контенты поля-классификатора */
  ${field.FieldName}_Contents: t.optional(t.model({${
        Object.values(field.Contents)
          .map(content => Object.keys(contentFields(content)).length > 0 ? `
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // ${contentName(content)}: t.optional(t.late(() => ${contentName(content)}), {})
    /** ${contentTitle(content)} */
    ${contentName(content)}: t.optional(t.model({${
          Object.values(contentFields(content))
            .map(extField => `
      ${extField.FieldName}: ${fieldType(extField)},`)
            .join("")}
    }), {}),` : `
    ${contentName(content)}: t.optional(t.frozen, {}),`)
            .join("")}
  }), {})`;
    } else if (isBackward(field)) {
      return `t.optional(t.array(t.reference(t.late(() => ${contentName(field.Content)}))), [])`;
    } else if (isRelation(field)) {
      switch (field.FieldType) {
        case "M2MRelation":
        case "M2ORelation":
          return `t.optional(t.array(t.reference(t.late(() => ${contentName(field.Content)}))), [])`;
        case "O2MRelation":
          return `t.maybe(t.reference(t.late(() => ${contentName(field.Content)})))`;
      }
    } else {
      switch (field.FieldType) {
        case "String":
        case "Image":
        case "Textbox":
        case "VisualEdit":
        case "DynamicImage":
          return `t.maybe(t.string)`;
        case "Numeric":
          return `t.maybe(t.number)`;
        case "Boolean":
          return `t.maybe(t.boolean)`;
        case "Date":
        case "Time":
        case "DateTime":
          return `t.maybe(t.Date)`;
        case "File":
          return `t.maybe(FileModel)`;
        case "Classifier":
          return `t.maybe(t.string)`;
        case "StringEnum":
          return `t.maybe(t.enumeration("${field.FieldName}", [${
            field.Items
              .map(item => `
    "${item.Value}",`)
              .join("")}
  ]))`;
      }
    }
    throw new Error(`Can not parse field ${JSON.stringify(field, null, 2)}`);
  };

  // prettier-ignore
  return `import { types as t } from "mobx-state-tree";

/** Файл, загружаемый в QPublishing */
export const FileModel = t.model("FileModel", {
  /** Имя файла */
  Name: t.string,
  /** URL файла */
  AbsoluteUrl: t.string,
});
${Object.values(mergedSchemas)
  .filter(content => !content.ForExtension)
  .map(content => `
type _I${contentName(content)} = typeof ${contentName(content)}.Type;
/** ${contentTitle(content)} */
export interface I${contentName(content)} extends _I${contentName(content)} {}
/** ${contentTitle(content)} */
export const ${contentName(content)} = t.model("${contentName(content)}", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),${
    Object.values(content.Fields)
      .map(field => `
  /** ${field.FieldTitle} */
  ${field.FieldName}: ${fieldType(field)},`)
      .join("")}
});
`).join("")}
${Object.values(mergedSchemas)
  .filter(content => content.ForExtension)
  .map(content => `
type _I${contentName(content)} = typeof ${contentName(content)}.Type;
/** ${contentTitle(content)} (Extension) */
export interface I${contentName(content)} extends _I${contentName(content)} {}
/** ${contentTitle(content)} (Extension) */
export const ${contentName(content)} = t.model("${contentName(content)}", {${
  Object.values(content.Fields)
    .map(field => `
  /** ${field.FieldTitle} */
  ${field.FieldName}: ${fieldType(field)},`)
    .join("") || `
  // no fields`}
});
`).join("")}

export default t.model({${
Object.values(mergedSchemas)
  .filter(content => !content.ForExtension)
  .map(content => `
  ${contentName(content)}: t.optional(t.map(${contentName(content)}), {}),`)
  .join("")}
});
`;
}

/*================================ Normalizr ================================*/

function compileNormalizrSchema(mergedSchemas) {
  const contentName = ({ ContentId }) => mergedSchemas[ContentId].ContentName;
  const contentTitle = ({ ContentId }) => mergedSchemas[ContentId].ContentTitle;
  const variableName = ({ ContentId }) =>
    mergedSchemas[ContentId].ContentName + "Shape";

  // prettier-ignore
  const fieldDefinition = field => {
    if (isExtension(field)) {
      return `${field.FieldName}_Contents: {${
        Object.values(field.Contents)
          .map(content => `
    ${contentName(content)}: ${variableName(content)},`)
          .join("")}
  }`;
    } else if (isBackward(field)) {
      return `${field.FieldName}: [${variableName(field.Content)}]`;
    } else if (isRelation(field)) {
      switch (field.FieldType) {
        case "M2MRelation":
        case "M2ORelation":
          return `${field.FieldName}: [${variableName(field.Content)}]`;
        case "O2MRelation":
          return `${field.FieldName}: ${variableName(field.Content)}`;
      }
    }
    throw new Error(`Can not parse field ${JSON.stringify(field, null, 2)}`);
  };

  // prettier-ignore
  return `import { schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";

const options = { idAttribute: "Id", mergeStrategy: deepMerge };
${Object.values(mergedSchemas)
  .filter(content => !content.ForExtension)
  .map(content => `
/** ${contentTitle(content)} */
export const ${variableName(content)} = new schema.Entity("${contentName(content)}", {}, options);`)
  .join("")}

// Extensions
${Object.values(mergedSchemas)
  .filter(content => content.ForExtension)
  .map(content => `
const ${variableName(content)} = new schema.Object({}); // ${contentTitle(content)}`)
  .join("")}

${Object.values(mergedSchemas)
  .filter(content => Object.values(content.Fields).filter(isReferenceField).length > 0)
  .map(content => `
// ${contentTitle(content)}
${variableName(content)}.define({${
  Object.values(content.Fields)
    .filter(isReferenceField)
    .map(field => `
  ${fieldDefinition(field)},`)
    .join("")}
});
`).join("")}

/** Shapes by ContentName */
export default {${
Object.values(mergedSchemas)
  .filter(content => !content.ForExtension)
  .map(content => `
  ${contentName(content)}: ${variableName(content)},`)
  .join("")}
};
`;
}
