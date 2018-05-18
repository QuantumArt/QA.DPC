import { download } from "Utils/Downloading";
import { isObject, isInteger, isBoolean } from "Utils/TypeChecking";

// @ts-ignore
const editorSchemaTypes = compileEditorSchemaTypes(window.editorSchema);
// @ts-ignore
const editorSchemaObject = compileEditorSchemaObject(window.editorSchema);
// @ts-ignore
const editorMobxModel = compileMobxModel(window.mobxSchema);

// prettier-ignore
download("ProductEditorSchema.ts", editorSchemaTypes + "\n" + editorSchemaObject);
download("ProductEditorModel.ts", editorMobxModel);

// @ts-ignore
document.querySelector("#typeScriptCode").innerText =
  editorMobxModel + "\n" + editorSchemaTypes + "\n" + editorSchemaObject;

/**
 * Преобразовать JSON схемы в интерфейсы TypeScript (для объекта схемы)
 * @param {object} editorSchema
 * @returns {string} Код TypeScript
 */
function compileEditorSchemaTypes(editorSchema) {
  const productSchema = JSON.parse(JSON.stringify(editorSchema));
  const content = productSchema.Content;
  const definitions = productSchema.Definitions;
  delete productSchema.Definitions;
  const rootName = getRootName(editorSchema);

  const procuctInterface = `
/** Описание полей продукта */
export interface ${rootName} ${replaceRefs(content, rootName)}`;

  const result = [procuctInterface];

  Object.keys(definitions).forEach(name => {
    result.push(
      `interface ${name}Schema ${replaceRefs(
        definitions[name],
        `${name}Schema`
      )}`
    );
  });

  return result.join("\n");
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

/**
 * Преобразовать JSON схемы в код объекта схемы
 * @param {object} editorSchema
 * @returns {string} Код TypeScript
 */
function compileEditorSchemaObject(editorSchema) {
  const rootName = getRootName(editorSchema);
  return `
/** Описание полей продукта */
export default linkJsonRefs<${rootName}>(${prettyPrint(editorSchema)});

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
      if (object.ContentId) {
        object.include = includeContent;
      } else if (object.FieldId) {
        if (
          object.Content && (
            object.FieldType === "M2ORelation" ||
            object.FieldType === "O2MRelation" ||
            object.FieldType === "M2MRelation"
          )
        ) {
          object.include = includeRelation;
        } else if (object.Contents && object.FieldType === "Classifier") {
          object.include = includeExtension;
        }
      }
    }
  }
}

function resolveRef(object: any, definitions: object): any {
  return typeof object === "object" && object !== null && "$ref" in object
    ? definitions[object.$ref.slice(14)]
    : object;
}

type Selection = { Content: object } | { Contents: object } | string[];

function includeContent(selector: (fields: object) => Selection[]): string[] {
  const paths = [this.ContentPath];
  selector(this.Fields).forEach((field: any, i) => {
    if (field.Content) {
      paths.push(field.Content.ContentPath);
    } else if (field.Contents) {
      const contentsPaths = Object.keys(field.Contents).map(key => field.Contents[key].ContentPath);
      paths.push(...contentsPaths);
    } else if (Array.isArray(field) && typeof (field[0] === "string")) {
      paths.push(...field);
    } else {
      throw new TypeError("Invalid field selection [" + i + "]: " + field);
    }
  });
  return paths;
}

function includeRelation(selector: (fields: object) => Selection[]): string[] {
  return includeContent.call(this.Content, selector);
}

function includeExtension(selector: (contents: object) => string[][]): string[] {
  const paths = Object.keys(this.Contents).map(key => this.Contents[key].ContentPath);
  selector(this.Contents).forEach((content: any, i) => {
    if (Array.isArray(content) && typeof (content[0] === "string")) {
      paths.push(...content);
    } else {
      throw new TypeError("Invalid content selection [" + i + "]: " + content);
    }
  });
  return paths;
}`;
}

/*=================================== MobX ==================================*/

function compileMobxModel(mobxSchema) {
  const isContentRef = arg =>
    isObject(arg) && Object.keys(arg).length === 1 && isInteger(arg.ContentId);

  const isRelation = field =>
    isObject(field) &&
    isBoolean(field.IsBackward) &&
    isContentRef(field.Content) &&
    field.FieldType.endsWith("Relation") &&
    !field.IsBackward;

  const isBackward = field =>
    isObject(field) &&
    isBoolean(field.IsBackward) &&
    isContentRef(field.Content) &&
    field.FieldType.endsWith("Relation") &&
    field.IsBackward;

  const isExtension = field =>
    isObject(field) &&
    isObject(field.Contents) &&
    field.FieldType === "Classifier" &&
    Object.values(field.Contents).every(isContentRef);

  const modelName = ({ ContentId }) => mobxSchema[ContentId].ContentName;
  const modelTitle = ({ ContentId }) => mobxSchema[ContentId].ContentTitle;

  // prettier-ignore
  const fieldType = field => {
    if (isExtension(field)) {
      return `t.model({
    /** Значение поля-классификатора */
    Value: t.string,
    /** Контенты поля-классификатора */
    Contents: t.model({${
      Object.values(field.Contents).map(content => `
      /** ${modelTitle(content)} */
      ${modelName(content)}: t.late(() => ${modelName(content)}),`).join("")}
    }),
  })`;
    } else if (isBackward(field)) {
      return `t.array(t.late(() => ${modelName(field.Content)}))`;
    } else if (isRelation(field)) {
      switch (field.FieldType) {
        case "M2MRelation":
        case "M2ORelation":
          return `t.array(t.late(() => ${modelName(field.Content)}))`;
        case "O2MRelation":
          return `t.reference(t.late(() => ${modelName(field.Content)}))`;
      }
    } else {
      switch (field.FieldType) {
        case "String":
        case "Image":
        case "Textbox":
        case "VisualEdit":
        case "DynamicImage":
          return `t.string`;
        case "Numeric":
          return `t.number`;
        case "Boolean":
          return `t.boolean`;
        case "Date":
        case "Time":
        case "DateTime":
          return `t.Date`;
        case "File":
          return `t.maybe(FileModel)`;
        case "Classifier":
          return `t.string`; // TODO: `t.enumeration("${field.FieldName}", [])`;
        case "StringEnum":
          return `t.enumeration("${field.FieldName}", [${field.Items.map(item => `
    "${item.Value}",`).join("")}
  ])`;
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
${Object.values(mobxSchema).filter(content => !content.IsExtension).map(content => `
type _I${modelName(content)} = typeof ${modelName(content)}.Type;
/** ${content.ContentTitle} */
export interface I${modelName(content)} extends _I${modelName(content)} {}
/** ${content.ContentTitle} */
export const ${modelName(content)} = t.model("${content.ContentName}(${content.ContentId})", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.Date,${
Object.values(content.Fields).map(field => `
  /** ${field.FieldTitle} */
  ${field.FieldName}: ${fieldType(field)},`).join("")}
});
`).join("")}
${Object.values(mobxSchema).filter(content => content.IsExtension).map(content => `
type _I${modelName(content)} = typeof ${modelName(content)}.Type;
/** ${content.ContentTitle} (Extension) */
export interface I${modelName(content)} extends _I${modelName(content)} {}
/** ${content.ContentTitle} (Extension) */
export const ${modelName(content)} = t.model("${content.ContentName}(${content.ContentId})", {${
Object.values(content.Fields).map(field => `
  /** ${field.FieldTitle} */
  ${field.FieldName}: ${fieldType(field)},`).join("")}
});
`).join("")}
export default {${Object.values(mobxSchema).map(content => `
  ${content.ContentId}: ${modelName(content)},`).join("")}
};
`;
}
