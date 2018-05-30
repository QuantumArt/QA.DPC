import { download } from "Utils/Download";
import {
  ContentSchema,
  FieldExactTypes,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  isPlainField,
  isEnumField
} from "Models/EditorSchemaModels";

const dataInterfaces = compileEditorDataInterfaces(
  // @ts-ignore
  window.mergedSchema,
  // @ts-ignore
  window.editorSchema
);

download("ProductEditorSchema.ts", dataInterfaces);

// @ts-ignore
document.querySelector("#typeScriptCode").innerText = dataInterfaces;

/**
 * Преобразовать JSON схемы в интерфейсы TypeScript (для объектов данных)
 * @param {{ [name: string]: ContentSchema }} mergedSchemas
 * @param {{ Content: any; Definitions: { [name: string]: any } }} editorSchema
 * @returns {string} Код TypeScript
 */
function compileEditorDataInterfaces(mergedSchemas, editorSchema) {
  const getFields = content => mergedSchemas[content.ContentId].Fields;
  const getName = content => mergedSchemas[content.ContentId].ContentName;

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

  const print = func => func();

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

    if (isSingleRelationField(field)) {
      return `${getName(field.Content)}`;
    }

    if (isMultiRelationField(field)) {
      return `${getName(field.Content)}[]`;
    }

    if (isEnumField(field)) {
      return forEach(field.Items, item => `
    | "${item.Value}"`);
    }

    if (isPlainField(field)) {
      switch (field.FieldType) {
        case FieldExactTypes.String:
        case FieldExactTypes.Textbox:
        case FieldExactTypes.VisualEdit:
        case FieldExactTypes.Classifier:
          return `string`;
        case FieldExactTypes.Numeric:
          return `number`;
        case FieldExactTypes.Boolean:
          return `boolean`;
        case FieldExactTypes.Date:
        case FieldExactTypes.Time:
        case FieldExactTypes.DateTime:
          return `Date`;
        case FieldExactTypes.File:
        case FieldExactTypes.Image:
          return `{
    Name: string;
    AbsoluteUrl: string;
  }`;
      }
    }
    
    throw new Error(
      `Field "${field.FieldName}" has unsupported ClassName ${
        field.ClassNames.slice(-1)[0]
      } or FieldType FieldExactTypes.${field.FieldType}`
    );
  })};`)}
}
`)}`;
}

function getRootName(editorSchema) {
  const refRegex = /\{\s*"?\$ref"?:\s*"#\/Definitions\/([A-Za-z0-9]+)"\s*}/gm;
  return (
    editorSchema.Content.ContentName ||
    refRegex.exec(JSON.stringify(editorSchema.Content))[1]
  );
}
