import * as tslib_1 from "tslib";
import { normalize, schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";
import { ArticleObject } from "Models/EditorDataModels";
import {
  isSingleRelationField,
  isMultiRelationField,
  isExtensionField
} from "Models/EditorSchemaModels";
var options = {
  idAttribute: ArticleObject._ClientId,
  mergeStrategy: deepMerge
};
var DataNormalizer = /** @class */ (function() {
  function DataNormalizer() {
    this._entitySchemas = {};
    this._objectSchemas = {};
  }
  DataNormalizer.prototype.initSchema = function(mergedSchemas) {
    var _this = this;
    Object.values(mergedSchemas).forEach(function(content) {
      _this._entitySchemas[content.ContentName] = new schema.Entity(
        content.ContentName,
        {},
        options
      );
      _this._objectSchemas[content.ContentName] = new ObjectSchema({});
    });
    this._tablesSchema = new ObjectSchema({});
    Object.values(mergedSchemas).forEach(function(content) {
      var _a;
      var references = {};
      Object.values(content.Fields).forEach(function(field) {
        if (isExtensionField(field)) {
          var extReferences_1 = {};
          Object.values(field.ExtensionContents).forEach(function(extContent) {
            extReferences_1[extContent.ContentName] =
              _this._objectSchemas[extContent.ContentName];
          });
          references[
            "" + field.FieldName + ArticleObject._Extension
          ] = extReferences_1;
        } else if (isSingleRelationField(field)) {
          references[field.FieldName] =
            _this._entitySchemas[field.RelatedContent.ContentName];
        } else if (isMultiRelationField(field)) {
          references[field.FieldName] = [
            _this._entitySchemas[field.RelatedContent.ContentName]
          ];
        }
      });
      _this._entitySchemas[content.ContentName].define(references);
      _this._objectSchemas[content.ContentName].define(references);
      _this._tablesSchema.define(
        ((_a = {}),
        (_a[content.ContentName] = [_this._entitySchemas[content.ContentName]]),
        _a)
      );
    });
  };
  DataNormalizer.prototype.normalize = function(articleObject, contentName) {
    return normalize(articleObject, this._entitySchemas[contentName]).entities;
  };
  DataNormalizer.prototype.normalizeAll = function(
    articleObjects,
    contentName
  ) {
    return normalize(articleObjects, [this._entitySchemas[contentName]])
      .entities;
  };
  DataNormalizer.prototype.normalizeTables = function(articleObjectsByContent) {
    return normalize(articleObjectsByContent, this._tablesSchema).entities;
  };
  return DataNormalizer;
})();
export { DataNormalizer };
/**
 * `schema.Object` that preserves `null` in property values
 * https://github.com/paularmstrong/normalizr/issues/332
 */
var ObjectSchema = /** @class */ (function(_super) {
  tslib_1.__extends(ObjectSchema, _super);
  function ObjectSchema() {
    return (_super !== null && _super.apply(this, arguments)) || this;
  }
  ObjectSchema.prototype.normalize = function(
    input,
    _parent,
    _key,
    visit,
    addEntity
  ) {
    var _this = this;
    var object = tslib_1.__assign({}, input);
    Object.keys(this.schema).forEach(function(key) {
      var localSchema = _this.schema[key];
      var value = visit(input[key], input, key, localSchema, addEntity);
      if (value === undefined) {
        delete object[key];
      } else {
        object[key] = value;
      }
    });
    return object;
  };
  return ObjectSchema;
})(schema.Object);
//# sourceMappingURL=DataNormalizer.js.map
