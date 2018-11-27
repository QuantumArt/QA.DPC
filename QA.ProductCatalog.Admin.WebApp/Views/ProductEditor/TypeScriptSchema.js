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
import { ArticleObject } from "Models/EditorDataModels";

const dataInterfaces = compileEditorDataInterfaces(
  // @ts-ignore
  window.mergedSchema
);

download("TypeScriptSchema.ts", dataInterfaces);

// @ts-ignore
document.querySelector("#typeScriptCode").innerText = dataInterfaces;

/**
 * Преобразовать JSON схемы в интерфейсы TypeScript (для объектов данных)
 * @param {{ [name: string]: ContentSchema }} mergedSchemas
 * @returns {string} Код TypeScript
 */
function compileEditorDataInterfaces(mergedSchemas) {
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
  return `/**
 * DO NOT MODIFY THIS FILE BECAUSE IT WAS GENERATED AUTOMATICALLY
 * @see ProductEditorController.TypeScriptSchema()
 */
import { IMSTArray, IMSTMap } from "mobx-state-tree";
// @ts-ignore
import { EntityObject, ExtensionObject, TablesObject } from "Models/EditorDataModels";

type IArray<T> = IMSTArray<any, any, T>;
type IMap<T> = IMSTMap<any, any, T>;

/** Типизация хранилища данных */
export interface Tables extends TablesObject {${
  forEach(mergedSchemas, content => !content.ForExtension, content => `
  ${getName(content)}: IMap<${getName(content)}>;`)}
}
${forEach(mergedSchemas, content => `
export interface ${getName(content)} extends ${
  content.ForExtension ? "ExtensionObject" : "EntityObject"
} {${forEach(getFields(content), field => `${field.FieldTitle || field.FieldDescription ? `
  /** ${field.FieldTitle || field.FieldDescription} */`: ""}
  ${content.IsReadOnly || field.IsReadOnly? "readonly " : ""
  }${field.FieldName}: ${print(() => {

    if (isExtensionField(field)) {
      return `${forEach(field.ExtensionContents, content => `
    | "${getName(content)}"`)};
  ${field.FieldName}${ArticleObject._Extension}: {${forEach(field.ExtensionContents, content => `
    ${getName(content)}: ${getName(content)};`)}
  }`;
    }

    if (isSingleRelationField(field)) {
      return `${getName(field.RelatedContent)}`;
    }

    if (isMultiRelationField(field)) {
      return `IArray<${getName(field.RelatedContent)}>`;
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
        case FieldExactTypes.File:
        case FieldExactTypes.Image:
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
