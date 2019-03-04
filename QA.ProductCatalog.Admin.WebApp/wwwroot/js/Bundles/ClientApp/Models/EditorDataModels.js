import { isObject } from "Utils/TypeChecks";
var ArticleObject = /** @class */ (function() {
  function ArticleObject() {}
  ArticleObject._ClientId = "_ClientId";
  ArticleObject._ServerId = "_ServerId";
  ArticleObject._Modified = "_Modified";
  ArticleObject._IsExtension = "_IsExtension";
  ArticleObject._Extension = "_Extension";
  ArticleObject._IsVirtual = "_IsVirtual";
  return ArticleObject;
})();
export { ArticleObject };
export function isArticleObject(object) {
  return isObject(object) && ArticleObject._ServerId in object;
}
export function isEntityObject(object) {
  return isArticleObject(object) && !object._IsExtension;
}
export function isExtensionObject(object) {
  return isArticleObject(object) && object._IsExtension;
}
export function isExtensionDictionary(object) {
  return isObject(object) && Object.values(object).every(isExtensionObject);
}
//# sourceMappingURL=EditorDataModels.js.map
