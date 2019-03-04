import { isIsoDateString, isString } from "Utils/TypeChecks";
import { isEntityObject, ArticleObject } from "Models/EditorDataModels";
import {
  isExtensionField,
  UpdatingMode,
  isSingleRelationField,
  isMultiRelationField
} from "Models/EditorSchemaModels";
var DataSerializer = /** @class */ (function() {
  function DataSerializer() {
    this._idMappingDict = Object.create(null);
  }
  /** Обновление соответствия положительные серверные Id => отрицательные клиентские Id  */
  DataSerializer.prototype.extendIdMappings = function(idMappings) {
    var _this = this;
    idMappings.forEach(function(pair) {
      _this._idMappingDict[pair.ServerId] = pair.ClientId;
    });
  };
  DataSerializer.prototype.deserialize = function(argument) {
    var _this = this;
    var json = isString(argument) ? argument : JSON.stringify(argument);
    return JSON.parse(json, function(_key, value) {
      if (isEntityObject(value)) {
        // @ts-ignore
        value._ClientId = _this.getClientId(value._ServerId);
        return value;
      } else if (isIsoDateString(value)) {
        return Number(new Date(value));
      }
      return value;
    });
  };
  /**
   * Статья может быть создана на клиенте: `{ _ClientId: -1, _ServerId: -1 }`,
   * затем сохранена: `{ _ClientId: -1, _ServerId: 100500 }, _idMappingDict === { 100500: -1 }`,
   * затем удалена параллельно другим пользователем, и наконец, вновь сохранена  на клиенте
   * `{ _ClientId: -1, _ServerId: 100501 }, _idMappingDict === { 100500: -1, 100501: 100500 }`.
   * Таким образом, для получения исходного `_ClientId` мы должны пройти по цепочке отображения
   * _ServerId --> _ClientId.
   */
  DataSerializer.prototype.getClientId = function(serverId) {
    for (var i = 0; i < 99999; i++) {
      var clientId = this._idMappingDict[serverId];
      if (!clientId) {
        return serverId;
      }
      serverId = clientId;
    }
    throw new Error("There is a cycle in DataSerializer._idMappingDict");
  };
  DataSerializer.prototype.serialize = function(article, contentSchema) {
    var _this = this;
    var snapshot = {
      _ServerId: article._ServerId,
      _Modified: article._Modified
    };
    var _loop_1 = function(fieldName) {
      var _a;
      var fieldValue = article[fieldName];
      if (fieldValue == null) {
        return "continue";
      }
      var fieldSchema = contentSchema.Fields[fieldName];
      if (isSingleRelationField(fieldSchema)) {
        var relatedEntity = fieldValue;
        if (relatedEntity._IsVirtual) {
          snapshot[fieldName] = null;
        } else if (fieldSchema.UpdatingMode === UpdatingMode.Ignore) {
          snapshot[fieldName] = { _ServerId: relatedEntity._ServerId };
        } else {
          snapshot[fieldName] = this_1.serialize(
            relatedEntity,
            fieldSchema.RelatedContent
          );
        }
      } else if (isMultiRelationField(fieldSchema)) {
        var relatedCollection = fieldValue;
        if (fieldSchema.UpdatingMode === UpdatingMode.Ignore) {
          snapshot[fieldName] = relatedCollection
            .filter(function(entity) {
              return !entity._IsVirtual;
            })
            .map(function(entity) {
              return { _ServerId: entity._ServerId };
            });
        } else {
          snapshot[fieldName] = relatedCollection
            .filter(function(entity) {
              return !entity._IsVirtual;
            })
            .map(function(entity) {
              return _this.serialize(entity, fieldSchema.RelatedContent);
            });
        }
      } else if (isExtensionField(fieldSchema)) {
        var extensionFieldName = "" + fieldName + ArticleObject._Extension;
        var extensionArticle = article[extensionFieldName][fieldValue];
        var extensionContentSchema = fieldSchema.ExtensionContents[fieldValue];
        snapshot[fieldName] = fieldValue;
        snapshot[extensionFieldName] = ((_a = {}),
        (_a[fieldValue] = this_1.serialize(
          extensionArticle,
          extensionContentSchema
        )),
        _a);
      } else {
        snapshot[fieldName] = fieldValue;
      }
    };
    var this_1 = this;
    for (var fieldName in contentSchema.Fields) {
      _loop_1(fieldName);
    }
    return snapshot;
  };
  return DataSerializer;
})();
export { DataSerializer };
//# sourceMappingURL=DataSerializer.js.map
