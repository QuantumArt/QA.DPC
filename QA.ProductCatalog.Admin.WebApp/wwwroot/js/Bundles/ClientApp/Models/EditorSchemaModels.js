import { isObject, isInteger } from "Utils/TypeChecks";
export function isContent(content) {
  return isObject(content) && isInteger(content.ContentId);
}
export function isField(field) {
  return isObject(field) && isInteger(field.FieldId);
}
export var FieldExactTypes;
(function(FieldExactTypes) {
  FieldExactTypes["Undefined"] = "Undefined";
  FieldExactTypes["String"] = "String";
  FieldExactTypes["Numeric"] = "Numeric";
  FieldExactTypes["Boolean"] = "Boolean";
  FieldExactTypes["Date"] = "Date";
  FieldExactTypes["Time"] = "Time";
  FieldExactTypes["DateTime"] = "DateTime";
  FieldExactTypes["File"] = "File";
  FieldExactTypes["Image"] = "Image";
  FieldExactTypes["Textbox"] = "Textbox";
  FieldExactTypes["VisualEdit"] = "VisualEdit";
  FieldExactTypes["DynamicImage"] = "DynamicImage";
  FieldExactTypes["M2ORelation"] = "M2ORelation";
  FieldExactTypes["O2MRelation"] = "O2MRelation";
  FieldExactTypes["M2MRelation"] = "M2MRelation";
  FieldExactTypes["Classifier"] = "Classifier";
  FieldExactTypes["StringEnum"] = "StringEnum";
})(FieldExactTypes || (FieldExactTypes = {}));
export function isPlainField(field) {
  return isField(field) && field.ClassNames.includes("PlainFieldSchema");
}
export function isStringField(field) {
  return isPlainField(field) && field.ClassNames.includes("StringFieldSchema");
}
export function isNumericField(field) {
  return isPlainField(field) && field.ClassNames.includes("NumericFieldSchema");
}
export function isClassifierField(field) {
  return (
    isPlainField(field) && field.ClassNames.includes("ClassifierFieldSchema")
  );
}
export function isFileField(field) {
  return isPlainField(field) && field.ClassNames.includes("FileFieldSchema");
}
export function isEnumField(field) {
  return isPlainField(field) && field.ClassNames.includes("EnumFieldSchema");
}
export function isRelationField(field) {
  return isField(field) && field.ClassNames.includes("RelationFieldSchema");
}
export var UpdatingMode;
(function(UpdatingMode) {
  UpdatingMode["Ignore"] = "Ignore";
  UpdatingMode["Update"] = "Update";
})(UpdatingMode || (UpdatingMode = {}));
export var PreloadingMode;
(function(PreloadingMode) {
  PreloadingMode["None"] = "None";
  PreloadingMode["Eager"] = "Eager";
  PreloadingMode["Lazy"] = "Lazy";
})(PreloadingMode || (PreloadingMode = {}));
export var PreloadingState;
(function(PreloadingState) {
  PreloadingState["NotStarted"] = "NotStarted";
  PreloadingState["Loading"] = "Loading";
  PreloadingState["Done"] = "Done";
})(PreloadingState || (PreloadingState = {}));
export function isSingleRelationField(field) {
  return (
    isRelationField(field) &&
    field.ClassNames.includes("SingleRelationFieldSchema")
  );
}
export function isMultiRelationField(field) {
  return (
    isRelationField(field) &&
    field.ClassNames.includes("MultiRelationFieldSchema")
  );
}
export function isExtensionField(field) {
  return isField(field) && field.ClassNames.includes("ExtensionFieldSchema");
}
//# sourceMappingURL=EditorSchemaModels.js.map
