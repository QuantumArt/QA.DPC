import * as tslib_1 from "tslib";
import { action } from "mobx";
import {
  types as t,
  unprotect,
  onPatch,
  getType,
  getSnapshot
} from "mobx-state-tree";
import { validationMixin } from "mst-validation-mixin";
import {
  isNumber,
  isString,
  isIsoDateString,
  isBoolean
} from "Utils/TypeChecks";
import { ArticleObject } from "Models/EditorDataModels";
import {
  FieldExactTypes,
  isExtensionField,
  isMultiRelationField,
  isSingleRelationField,
  isPlainField,
  isEnumField
} from "Models/EditorSchemaModels";
var DataContext = /** @class */ (function() {
  function DataContext() {
    this._nextId = -1;
    this._defaultSnapshots = null;
    this._tablesType = null;
    /** Набор таблиц статей-сущностей. Каждая таблица — ES6 Map */
    this.tables = null;
  }
  DataContext.prototype.initSchema = function(mergedSchemas) {
    var _this = this;
    this._tablesType = compileTablesType(mergedSchemas, function() {
      return _this._nextId--;
    });
    this._defaultSnapshots = compileDefaultSnapshots(mergedSchemas);
  };
  DataContext.prototype.initTables = function(tablesSnapshot) {
    this.tables = this._tablesType.create(tablesSnapshot);
    // разрешаем изменения моделей из других сервисов и компонентов
    unprotect(this.tables);
    if (DEBUG) {
      // отладочный вывод в консоль
      var tables_1 = this.tables;
      var SnapshotView_1 = /** @class */ (function() {
        function SnapshotView() {}
        Object.defineProperty(SnapshotView.prototype, "snapshot", {
          get: function() {
            return getSnapshot(tables_1);
          },
          enumerable: true,
          configurable: true
        });
        return SnapshotView;
      })();
      var isChrome =
        /Chrome/.test(navigator.userAgent) &&
        /Google Inc/.test(navigator.vendor);
      if (isChrome) {
        onPatch(this.tables, function(patch) {
          return console.log(patch, new SnapshotView_1());
        });
      } else {
        onPatch(this.tables, function(patch) {
          return console.log(patch);
        });
      }
    }
  };
  /**
   * Создать статью в указанном конетнте.
   * @param contentName Имя контента
   * @param properties Значения полей статьи.
   */
  DataContext.prototype.createEntity = function(contentName, properties) {
    var entity = this.getContentType(contentName).create(
      tslib_1.__assign(
        { _ClientId: this._nextId },
        this._defaultSnapshots[contentName],
        properties
      )
    );
    this.tables[contentName].put(entity);
    return entity;
  };
  /**
   * Удалить статью. Статья удаляется только из памяти и НЕ УДАЛЯЕТСЯ НА СЕРВЕРЕ.
   * @param entity Статья для удаления
   */
  DataContext.prototype.deleteEntity = function(entity) {
    var contentName = getType(entity).name;
    this.tables[contentName].delete(String(entity._ClientId));
  };
  DataContext.prototype.getContentType = function(contentName) {
    var optionalType = this._tablesType.properties[contentName];
    if (!optionalType) {
      throw new TypeError(
        'Content "' + contentName + '" is not defined in this Tables schema'
      );
    }
    // @ts-ignore
    return optionalType.type.subType;
  };
  var _a;
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [String, Object]),
      tslib_1.__metadata(
        "design:returntype",
        typeof (_a = typeof T !== "undefined" && T) === "function" ? _a : Object
      )
    ],
    DataContext.prototype,
    "createEntity",
    null
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    DataContext.prototype,
    "deleteEntity",
    null
  );
  return DataContext;
})();
export { DataContext };
/**
 * Компилирует MobX-модели из нормализованных схем контентов
 * @example
 *
 * interface Tables {
 *   Product: Map<string, Product>;
 *   Region: Map<string, Region>;
 * }
 * interface Product {
 *   _ClientId: number;
 *   Description: string;
 *   Regions: Region[];
 * }
 * interface Region {
 *   _ClientId: number;
 *   Title: string;
 *   Parent: Region;
 * }
 */
function compileTablesType(mergedSchemas, getNextId) {
  // создаем словарь с моделями основных контентов
  var contentModels = {};
  var visitField = function(field, fieldModels) {
    if (isExtensionField(field)) {
      // создаем nullable поле-классификатор в виде enum
      fieldModels[field.FieldName] = t.maybeNull(
        t.enumeration(field.FieldName, Object.keys(field.ExtensionContents))
      );
      // создаем словарь с моделями контентов-расширений
      var extContentModels_1 = {};
      // заполняем его, обходя field.Contents
      Object.entries(field.ExtensionContents).forEach(function(_a) {
        var _b = tslib_1.__read(_a, 2),
          extName = _b[0],
          extContent = _b[1];
        // для каждого контента-расширения создаем словарь его полей
        var extFieldModels = {
          _ServerId: t.optional(t.number, getNextId),
          _Modified: t.maybeNull(t.Date),
          _IsExtension: t.optional(t.literal(true), true)
        };
        // заполняем словарь полей обходя все поля контента-расширения
        Object.values(extContent.Fields).forEach(function(field) {
          return visitField(field, extFieldModels);
        });
        // создаем анонимную модель контента-расширения
        // prettier-ignore
        extContentModels_1[extName] = t.optional(t.model(extFieldModels).extend(validationMixin), {});
      });
      // создаем анонимную модель словаря контентов-расширений
      // prettier-ignore
      fieldModels["" + field.FieldName + ArticleObject._Extension] = t.optional(t.model(extContentModels_1), {});
    } else if (isSingleRelationField(field)) {
      // для O2MRelation создаем nullable ссылку на сущность
      fieldModels[field.FieldName] = t.maybeNull(
        t.reference(
          t.late(function() {
            return contentModels[field.RelatedContent.ContentName];
          })
        )
      );
    } else if (isMultiRelationField(field)) {
      // для M2MRelation и M2ORelation создаем массив ссылок на сущности
      // prettier-ignore
      fieldModels[field.FieldName] = t.optional(t.array(t.reference(t.late(function () { return contentModels[field.RelatedContent.ContentName]; }))), []);
    } else if (isEnumField(field)) {
      // создаем nullable строковое поле в виде enum
      fieldModels[field.FieldName] = t.maybeNull(
        t.enumeration(
          field.FieldName,
          field.Items.map(function(item) {
            return item.Value;
          })
        )
      );
    } else if (isPlainField(field)) {
      switch (field.FieldType) {
        case FieldExactTypes.String:
        case FieldExactTypes.Textbox:
        case FieldExactTypes.VisualEdit:
        case FieldExactTypes.File:
        case FieldExactTypes.Image:
        case FieldExactTypes.Classifier:
          fieldModels[field.FieldName] = t.maybeNull(t.string);
          break;
        case FieldExactTypes.Numeric:
          fieldModels[field.FieldName] = t.maybeNull(t.number);
          break;
        case FieldExactTypes.Boolean:
          fieldModels[field.FieldName] = t.maybeNull(t.boolean);
          break;
        case FieldExactTypes.Date:
        case FieldExactTypes.Time:
        case FieldExactTypes.DateTime:
          fieldModels[field.FieldName] = t.maybeNull(t.Date);
          break;
      }
    }
    if (!fieldModels[field.FieldName]) {
      throw new Error(
        'Field "' +
          field.FieldName +
          '" has unsupported ClassName ' +
          field.ClassNames.slice(-1)[0] +
          " or FieldType FieldExactTypes." +
          field.FieldType
      );
    }
  };
  Object.values(mergedSchemas)
    .filter(function(content) {
      return !content.ForExtension;
    })
    .forEach(function(content) {
      var fieldModels = {
        _ClientId: t.identifierNumber,
        _ServerId: t.optional(t.number, getNextId),
        _Modified: t.maybeNull(t.Date),
        _IsExtension: t.optional(t.literal(false), false),
        _IsVirtual: t.optional(t.boolean, false)
      };
      // заполняем поля модели объекта
      Object.values(content.Fields).forEach(function(field) {
        return visitField(field, fieldModels);
      });
      // создаем именованную модель объекта на основе полей
      contentModels[content.ContentName] = t
        .model(content.ContentName, fieldModels)
        .extend(validationMixin);
    });
  var collectionModels = {};
  // заполняем словарь с моделями коллекций
  Object.entries(contentModels).forEach(function(_a) {
    var _b = tslib_1.__read(_a, 2),
      name = _b[0],
      model = _b[1];
    collectionModels[name] = t.optional(t.map(model), {});
  });
  // создаем модель хранилища
  return t.model(collectionModels);
}
/**
 * Создает словарь со снапшотами-по умолчанию для создания новых статей.
 * Заполняет его на основе `FieldSchema.DefaultValue`.
 * @example
 *
 * {
 *   Product: {
 *     Type: "InternetTariff",
 *     Type_Extension: {
 *       InternetTariff: {
 *         Description: "Тариф интернет"
 *       }
 *     }
 *   },
 *   Region: {
 *     Title: "Москва"
 *   }
 * }
 */
function compileDefaultSnapshots(mergedSchemas) {
  var getFields = function(content) {
    return mergedSchemas[content.ContentId].Fields;
  };
  var visitField = function(field, fieldValues) {
    if (isExtensionField(field)) {
      // заполняем значение по-умолчанию для поля-классификатора
      if (isString(field.DefaultValue)) {
        fieldValues[field.FieldName] = field.DefaultValue;
      }
      // создаем словарь со снапшотами по-умолчанию контентов-расширений
      var extContentSnapshots_1 = {};
      // заполняем его, обходя field.Contents
      Object.entries(field.ExtensionContents).forEach(function(_a) {
        var _b = tslib_1.__read(_a, 2),
          extName = _b[0],
          extContent = _b[1];
        // для каждого контента-расширения создаем словарь значений его полей
        var extFieldValues = {};
        // заполняем словарь полей обходя все поля контента-расширения
        Object.values(getFields(extContent)).forEach(function(field) {
          return visitField(field, extFieldValues);
        });
        // запоминаем объект с полями контента, если они определены
        if (Object.keys(extFieldValues).length > 0) {
          extContentSnapshots_1[extName] = extFieldValues;
        }
      });
      // запоминаем объект со со снапшотами контентов-расширений, если они определены
      if (Object.keys(extContentSnapshots_1).length > 0) {
        fieldValues[
          "" + field.FieldName + ArticleObject._Extension
        ] = extContentSnapshots_1;
      }
    } else if (isPlainField(field)) {
      switch (field.FieldType) {
        case FieldExactTypes.String:
        case FieldExactTypes.Textbox:
        case FieldExactTypes.VisualEdit:
        case FieldExactTypes.Classifier:
        case FieldExactTypes.File:
        case FieldExactTypes.Image:
        case FieldExactTypes.StringEnum:
          if (isString(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case FieldExactTypes.Numeric:
          if (isNumber(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case FieldExactTypes.Boolean:
          if (isBoolean(field.DefaultValue)) {
            fieldValues[field.FieldName] = field.DefaultValue;
          }
          break;
        case FieldExactTypes.Date:
        case FieldExactTypes.Time:
        case FieldExactTypes.DateTime:
          if (isIsoDateString(field.DefaultValue)) {
            fieldValues[field.FieldName] = new Date(field.DefaultValue);
          }
          break;
      }
    }
  };
  var contentSnapshots = {};
  // заполняем словарь со снапшотами по-умолчанию для основных контентов
  Object.values(mergedSchemas)
    .filter(function(content) {
      return !content.ForExtension;
    })
    .forEach(function(content) {
      var fieldValues = {};
      Object.values(content.Fields).forEach(function(field) {
        return visitField(field, fieldValues);
      });
      contentSnapshots[content.ContentName] = fieldValues;
    });
  return contentSnapshots;
}
//# sourceMappingURL=DataContext.js.map
