import { download } from "Utils/Download";
import {
  FieldExactTypes,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  isPlainField,
  isEnumField
} from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
var dataInterfaces = compileEditorDataInterfaces(
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
  var getFields = function(content) {
    return mergedSchemas[content.ContentId].Fields;
  };
  var getName = function(content) {
    return mergedSchemas[content.ContentId].ContentName;
  };
  var forEach = function(obj, filter, map) {
    if (!map) {
      map = filter;
      filter = function() {
        return true;
      };
    }
    return Object.values(obj)
      .filter(filter)
      .map(map)
      .join("");
  };
  var print = function(func) {
    return func();
  };
  // prettier-ignore
  return "/**\n * DO NOT MODIFY THIS FILE BECAUSE IT WAS GENERATED AUTOMATICALLY\n * @see ProductEditorController.TypeScriptSchema()\n */\n// @ts-ignore\nimport { EntityObject, ExtensionObject, TablesObject, IArray, IMap } from \"Models/EditorDataModels\";\n\n/** \u0422\u0438\u043F\u0438\u0437\u0430\u0446\u0438\u044F \u0445\u0440\u0430\u043D\u0438\u043B\u0438\u0449\u0430 \u0434\u0430\u043D\u043D\u044B\u0445 */\nexport interface Tables extends TablesObject {" + forEach(mergedSchemas, function (content) { return !content.ForExtension; }, function (content) { return "\n  " + getName(content) + ": IMap<" + getName(content) + ">;"; }) + "\n}\n" + forEach(mergedSchemas, function (content) { return "\nexport interface " + getName(content) + " extends " + (content.ForExtension ? "ExtensionObject" : "EntityObject") + " {" + forEach(getFields(content), function (field) { return (field.FieldTitle || field.FieldDescription ? "\n  /** " + (field.FieldTitle || field.FieldDescription) + " */" : "") + "\n  " + (content.IsReadOnly || field.IsReadOnly ? "readonly " : "") + field.FieldName + ": " + print(function () {
        if (isExtensionField(field)) {
            return forEach(field.ExtensionContents, function (content) { return "\n    | \"" + getName(content) + "\""; }) + ";\n  " + field.FieldName + ArticleObject._Extension + ": {" + forEach(field.ExtensionContents, function (content) { return "\n    " + getName(content) + ": " + getName(content) + ";"; }) + "\n  }";
        }
        if (isSingleRelationField(field)) {
            return "" + getName(field.RelatedContent);
        }
        if (isMultiRelationField(field)) {
            return "IArray<" + getName(field.RelatedContent) + ">";
        }
        if (isEnumField(field)) {
            return forEach(field.Items, function (item) { return "\n    | \"" + item.Value + "\""; });
        }
        if (isPlainField(field)) {
            switch (field.FieldType) {
                case FieldExactTypes.String:
                case FieldExactTypes.Textbox:
                case FieldExactTypes.VisualEdit:
                case FieldExactTypes.File:
                case FieldExactTypes.Image:
                case FieldExactTypes.Classifier:
                    return "string";
                case FieldExactTypes.Numeric:
                    return "number";
                case FieldExactTypes.Boolean:
                    return "boolean";
                case FieldExactTypes.Date:
                case FieldExactTypes.Time:
                case FieldExactTypes.DateTime:
                    return "Date";
            }
        }
        throw new Error("Field \"" + field.FieldName + "\" has unsupported ClassName " + field.ClassNames.slice(-1)[0] + " or FieldType FieldExactTypes." + field.FieldType);
    }) + ";"; }) + "\n}\n"; });
}
//# sourceMappingURL=TypeScriptSchema.js.map
