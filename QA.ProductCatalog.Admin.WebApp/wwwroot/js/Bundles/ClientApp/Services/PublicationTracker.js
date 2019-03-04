import * as tslib_1 from "tslib";
import qs from "qs";
import { inject } from "react-ioc";
import { computed, reaction } from "mobx";
import { rootUrl } from "Utils/Common";
import {
  EditorQueryParams,
  PublicationTrackerSettings
} from "Models/EditorSettingsModels";
import { DataContext } from "Services/DataContext";
import { PublicationContext } from "Services/PublicationContext";
import { isIsoDateString } from "Utils/TypeChecks";
var PublicationTracker = /** @class */ (function() {
  function PublicationTracker() {
    var _this = this;
    this._loadedProductIds = new Set();
    this.loadMaxPublicationTime = function() {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var response, _a, maxPublicationTime, _b;
        return tslib_1.__generator(this, function(_c) {
          switch (_c.label) {
            case 0:
              return [
                4 /*yield*/,
                fetch(
                  rootUrl +
                    "/ProductEditorQuery/GetMaxPublicationTime?" +
                    qs.stringify(this._queryParams),
                  {
                    credentials: "include"
                  }
                )
              ];
            case 1:
              response = _c.sent();
              if (!!response.ok) return [3 /*break*/, 3];
              _a = Error.bind;
              return [4 /*yield*/, response.text()];
            case 2:
              throw new (_a.apply(Error, [void 0, _c.sent()]))();
            case 3:
              _b = Date.bind;
              return [4 /*yield*/, response.json()];
            case 4:
              maxPublicationTime = new (_b.apply(Date, [void 0, _c.sent()]))();
              this._publicationContext.updateMaxPublicationTime(
                maxPublicationTime
              );
              return [2 /*return*/];
          }
        });
      });
    };
    this.loadPublicationTimestamps = function() {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var newProductIds, response, _a, timestampsById, _b, _c;
        var _this = this;
        return tslib_1.__generator(this, function(_d) {
          switch (_d.label) {
            case 0:
              newProductIds = this.allProductIds.filter(function(id) {
                return !_this._loadedProductIds.has(id);
              });
              if (newProductIds.length === 0) {
                return [2 /*return*/];
              }
              newProductIds.forEach(function(id) {
                return _this._loadedProductIds.add(id);
              });
              return [
                4 /*yield*/,
                fetch(
                  rootUrl +
                    "/ProductEditorQuery/GetPublicationTimestamps?" +
                    qs.stringify(this._queryParams),
                  {
                    method: "POST",
                    credentials: "include",
                    headers: {
                      "Content-Type": "application/json"
                    },
                    body: JSON.stringify(newProductIds)
                  }
                )
              ];
            case 1:
              response = _d.sent();
              if (!!response.ok) return [3 /*break*/, 3];
              _a = Error.bind;
              return [4 /*yield*/, response.text()];
            case 2:
              throw new (_a.apply(Error, [void 0, _d.sent()]))();
            case 3:
              _c = (_b = JSON).parse;
              return [4 /*yield*/, response.text()];
            case 4:
              timestampsById = _c.apply(_b, [
                _d.sent(),
                function(_key, value) {
                  return isIsoDateString(value) ? new Date(value) : value;
                }
              ]);
              this._publicationContext.updateTimestamps(timestampsById);
              return [2 /*return*/];
          }
        });
      });
    };
    this.updatePublicationTimestamps = function() {
      return tslib_1.__awaiter(_this, void 0, void 0, function() {
        var maxPublicationTime, response, _a, timestampsById, _b, _c;
        return tslib_1.__generator(this, function(_d) {
          switch (_d.label) {
            case 0:
              if (document.hidden) {
                return [2 /*return*/];
              }
              maxPublicationTime = this._publicationContext.maxPublicationTime;
              if (!maxPublicationTime) {
                return [2 /*return*/];
              }
              return [
                4 /*yield*/,
                fetch(
                  rootUrl +
                    "/ProductEditorQuery/GetPublicationTimestamps?" +
                    qs.stringify(
                      tslib_1.__assign({}, this._queryParams, {
                        updatedSince: maxPublicationTime
                      })
                    ),
                  {
                    credentials: "include"
                  }
                )
              ];
            case 1:
              response = _d.sent();
              if (!!response.ok) return [3 /*break*/, 3];
              _a = Error.bind;
              return [4 /*yield*/, response.text()];
            case 2:
              throw new (_a.apply(Error, [void 0, _d.sent()]))();
            case 3:
              _c = (_b = JSON).parse;
              return [4 /*yield*/, response.text()];
            case 4:
              timestampsById = _c.apply(_b, [
                _d.sent(),
                function(_key, value) {
                  return isIsoDateString(value) ? new Date(value) : value;
                }
              ]);
              this._publicationContext.updateTimestamps(timestampsById);
              return [2 /*return*/];
          }
        });
      });
    };
  }
  Object.defineProperty(PublicationTracker.prototype, "allProductIds", {
    get: function() {
      var _this = this;
      var ids = [];
      this._settings.contentNames.forEach(function(tableName) {
        var e_1, _a;
        try {
          for (
            var _b = tslib_1.__values(
                _this._dataContext.tables[tableName].values()
              ),
              _c = _b.next();
            !_c.done;
            _c = _b.next()
          ) {
            var entity = _c.value;
            if (entity._ServerId) {
              ids.push(Number(entity._ServerId));
            }
          }
        } catch (e_1_1) {
          e_1 = { error: e_1_1 };
        } finally {
          try {
            if (_c && !_c.done && (_a = _b.return)) _a.call(_b);
          } finally {
            if (e_1) throw e_1.error;
          }
        }
      });
      return ids;
    },
    enumerable: true,
    configurable: true
  });
  PublicationTracker.prototype.dispose = function() {
    if (this._reactionDisposer) {
      this._reactionDisposer();
    }
    if (this._updateTimer) {
      window.clearInterval(this._updateTimer);
    }
  };
  PublicationTracker.prototype.initStatusTracking = function() {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var _this = this;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            if (!(this._settings.contentNames.length > 0))
              return [3 /*break*/, 2];
            return [
              4 /*yield*/,
              Promise.all([
                this.loadMaxPublicationTime(),
                this.loadPublicationTimestamps()
              ])
            ];
          case 1:
            _a.sent();
            this._reactionDisposer = reaction(function() {
              return _this.allProductIds;
            }, this.loadPublicationTimestamps);
            this._updateTimer = window.setInterval(
              this.updatePublicationTimestamps,
              this._settings.updateInterval
            );
            _a.label = 2;
          case 2:
            return [2 /*return*/];
        }
      });
    });
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorQueryParams)],
    PublicationTracker.prototype,
    "_queryParams",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    PublicationTracker.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationContext)],
    PublicationTracker.prototype,
    "_publicationContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", PublicationTrackerSettings)],
    PublicationTracker.prototype,
    "_settings",
    void 0
  );
  tslib_1.__decorate(
    [
      computed,
      tslib_1.__metadata("design:type", Object),
      tslib_1.__metadata("design:paramtypes", [])
    ],
    PublicationTracker.prototype,
    "allProductIds",
    null
  );
  return PublicationTracker;
})();
export { PublicationTracker };
//# sourceMappingURL=PublicationTracker.js.map
