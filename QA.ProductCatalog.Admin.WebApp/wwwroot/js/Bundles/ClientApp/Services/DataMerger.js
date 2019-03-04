import * as tslib_1 from "tslib";
import { inject } from "react-ioc";
import { action, comparer } from "mobx";
import { getSnapshot, isStateTreeNode } from "mobx-state-tree";
import { isArray } from "Utils/TypeChecks";
import {
  ArticleObject,
  isExtensionDictionary,
  isEntityObject
} from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
export var MergeStrategy;
(function(MergeStrategy) {
  /** Обновить только неизмененные поля. Не менять поле `_Modified`. */
  MergeStrategy[(MergeStrategy["Refresh"] = 1)] = "Refresh";
  /** Обновить только неизмененные поля. Обновить поле `_Modified`. */
  MergeStrategy[(MergeStrategy["ClientWins"] = 2)] = "ClientWins";
  /** Перезаписать поля статей, которые были изменены на сервере. Обновить поле `_Modified`. */
  MergeStrategy[(MergeStrategy["ServerWins"] = 3)] = "ServerWins";
  /** Перезаписать с сервера все поля всех статей. Обновить поле `_Modified`. */
  MergeStrategy[(MergeStrategy["Overwrite"] = 4)] = "Overwrite";
})(MergeStrategy || (MergeStrategy = {}));
var DataMerger = /** @class */ (function() {
  function DataMerger() {}
  DataMerger.prototype.tablesHasConflicts = function(tablesSnapshot) {
    var e_1, _a, e_2, _b;
    try {
      for (
        var _c = tslib_1.__values(Object.entries(tablesSnapshot)),
          _d = _c.next();
        !_d.done;
        _d = _c.next()
      ) {
        var _e = tslib_1.__read(_d.value, 2),
          contentName = _e[0],
          articlesById = _e[1];
        var collection = this._dataContext.tables[contentName];
        try {
          for (
            var _f = tslib_1.__values(Object.entries(articlesById)),
              _g = _f.next();
            !_g.done;
            _g = _f.next()
          ) {
            var _h = tslib_1.__read(_g.value, 2),
              id = _h[0],
              articleSnapshot = _h[1];
            var article = collection.get(id);
            if (article && this.articleHasConfilcts(article, articleSnapshot)) {
              return true;
            }
          }
        } catch (e_2_1) {
          e_2 = { error: e_2_1 };
        } finally {
          try {
            if (_g && !_g.done && (_b = _f.return)) _b.call(_f);
          } finally {
            if (e_2) throw e_2.error;
          }
        }
      }
    } catch (e_1_1) {
      e_1 = { error: e_1_1 };
    } finally {
      try {
        if (_d && !_d.done && (_a = _c.return)) _a.call(_c);
      } finally {
        if (e_1) throw e_1.error;
      }
    }
    return false;
  };
  DataMerger.prototype.articleHasConfilcts = function(article, snapshot) {
    var e_3, _a, e_4, _b;
    var oldSnapshot = getSnapshot(article);
    try {
      for (
        var _c = tslib_1.__values(Object.entries(snapshot)), _d = _c.next();
        !_d.done;
        _d = _c.next()
      ) {
        var _e = tslib_1.__read(_d.value, 2),
          name_1 = _e[0],
          fieldSnapshot = _e[1];
        var fieldValue = article[name_1];
        if (
          name_1.endsWith(ArticleObject._Extension) &&
          isExtensionDictionary(fieldValue)
        ) {
          try {
            for (
              var _f = tslib_1.__values(Object.entries(fieldSnapshot)),
                _g = _f.next();
              !_g.done;
              _g = _f.next()
            ) {
              var _h = tslib_1.__read(_g.value, 2),
                contentName = _h[0],
                extensionSnapshot = _h[1];
              if (
                this.articleHasConfilcts(
                  fieldValue[contentName],
                  extensionSnapshot
                )
              ) {
                return true;
              }
            }
          } catch (e_4_1) {
            e_4 = { error: e_4_1 };
          } finally {
            try {
              if (_g && !_g.done && (_b = _f.return)) _b.call(_f);
            } finally {
              if (e_4) throw e_4.error;
            }
          }
        } else if (
          oldSnapshot._Modified < snapshot._Modified &&
          article.isEdited(name_1) &&
          !comparer.structural(oldSnapshot[name_1], fieldSnapshot)
        ) {
          return true;
        }
      }
    } catch (e_3_1) {
      e_3 = { error: e_3_1 };
    } finally {
      try {
        if (_d && !_d.done && (_a = _c.return)) _a.call(_c);
      } finally {
        if (e_3) throw e_3.error;
      }
    }
    return false;
  };
  DataMerger.prototype.mergeTables = function(snapshot, strategy) {
    var _this = this;
    Object.entries(snapshot).forEach(function(_a) {
      var _b = tslib_1.__read(_a, 2),
        contentName = _b[0],
        articlesById = _b[1];
      var collection = _this._dataContext.tables[contentName];
      if (collection) {
        Object.entries(articlesById).forEach(function(_a) {
          var _b = tslib_1.__read(_a, 2),
            id = _b[0],
            articleSnapshot = _b[1];
          var article = collection.get(id);
          if (article) {
            _this.mergeArticle(article, articleSnapshot, strategy);
          } else {
            collection.put(articleSnapshot);
          }
        });
      }
    });
  };
  DataMerger.prototype.mergeArticle = function(article, snapshot, strategy) {
    var _this = this;
    article.clearErrors();
    var articleSnapshot = getSnapshot(article);
    Object.entries(snapshot).forEach(function(_a) {
      var _b = tslib_1.__read(_a, 2),
        name = _b[0],
        fieldSnapshot = _b[1];
      var fieldValue = article[name];
      if (name === ArticleObject._Modified) {
        if (
          snapshot._Modified &&
          articleSnapshot._Modified !== snapshot._Modified
        ) {
          switch (strategy) {
            case MergeStrategy.Refresh:
              break;
            case MergeStrategy.ClientWins:
            case MergeStrategy.ServerWins:
            case MergeStrategy.Overwrite:
              _this.setProperty(article, name, fieldSnapshot);
              break;
            default:
              throw new Error("Uncovered MergeStrategy");
          }
        }
      } else if (
        name.endsWith(ArticleObject._Extension) &&
        isExtensionDictionary(fieldValue)
      ) {
        Object.entries(fieldSnapshot).forEach(function(_a) {
          var _b = tslib_1.__read(_a, 2),
            contentName = _b[0],
            extensionSnapshot = _b[1];
          _this.mergeArticle(
            fieldValue[contentName],
            extensionSnapshot,
            strategy
          );
        });
      } else if (comparer.structural(articleSnapshot[name], fieldSnapshot)) {
        article.setChanged(name, false);
      } else if (!article.isEdited(name)) {
        _this.setProperty(article, name, fieldSnapshot);
      } else {
        switch (strategy) {
          case MergeStrategy.Refresh:
          case MergeStrategy.ClientWins:
            break;
          case MergeStrategy.ServerWins:
            if (snapshot._Modified || !articleSnapshot._Modified) {
              var baseValueSnapshot = _this.getBaseValueSnapshot(article, name);
              if (!comparer.structural(baseValueSnapshot, fieldSnapshot)) {
                _this.setProperty(article, name, fieldSnapshot);
              }
            }
            break;
          case MergeStrategy.Overwrite:
            if (snapshot._Modified || !articleSnapshot._Modified) {
              _this.setProperty(article, name, fieldSnapshot);
            }
            break;
          default:
            throw new Error("Uncovered MergeStrategy");
        }
      }
    });
  };
  DataMerger.prototype.getBaseValueSnapshot = function(article, name) {
    var baseValue = article.getBaseValue(name);
    if (isStateTreeNode(baseValue)) {
      return getSnapshot(baseValue);
    }
    if (isArray(baseValue) && isEntityObject(baseValue[0])) {
      return baseValue.map(function(entity) {
        return entity._ClientId;
      });
    }
    if (baseValue instanceof Date) {
      return Number(baseValue);
    }
    return baseValue;
  };
  DataMerger.prototype.setProperty = function(article, name, value) {
    article[name] = value;
    article.setChanged(name, false);
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    DataMerger.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [Object, Number]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    DataMerger.prototype,
    "mergeTables",
    null
  );
  tslib_1.__decorate(
    [
      action,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [ArticleObject, Object, Number]),
      tslib_1.__metadata("design:returntype", void 0)
    ],
    DataMerger.prototype,
    "mergeArticle",
    null
  );
  return DataMerger;
})();
export { DataMerger };
//# sourceMappingURL=DataMerger.js.map
