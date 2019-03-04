import * as tslib_1 from "tslib";
import "Scripts/pmrpc";
import QP8 from "Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { rootUrl } from "Utils/Common";
import { handleError, modal } from "Utils/Decorators";
import { EditorQueryParams } from "Models/EditorSettingsModels";
var actionInfosByAlias = {};
/** Выполнение произвольных CustomAction */
var ActionController = /** @class */ (function() {
  function ActionController() {}
  /** Найти CustomAction по Alias и выполнить его для заданной статьи */
  ActionController.prototype.executeCustomAction = function(
    actionAlias,
    entity,
    contentSchema,
    options
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var actionInfo, executeOptions;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            return [4 /*yield*/, this.getCustomActionInfo(actionAlias)];
          case 1:
            actionInfo = _a.sent();
            executeOptions = tslib_1.__assign(
              {
                actionCode: actionInfo.ActionCode,
                entityTypeCode: actionInfo.EntityTypeCode,
                parentEntityId: contentSchema.ContentId,
                entityId: entity._ServerId > 0 ? entity._ServerId : 0
              },
              options
            );
            QP8.executeBackendAction(
              executeOptions,
              this._queryParams.hostUID,
              window.parent
            );
            return [2 /*return*/];
        }
      });
    });
  };
  ActionController.prototype.getCustomActionInfo = function(alias) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var actionInfo, response, _a;
      return tslib_1.__generator(this, function(_b) {
        switch (_b.label) {
          case 0:
            actionInfo = actionInfosByAlias[alias];
            if (actionInfo) {
              return [2 /*return*/, actionInfo];
            }
            return [
              4 /*yield*/,
              fetch(
                rootUrl +
                  "/ProductEditorQuery/GetCustomActionByAlias?" +
                  qs.stringify(
                    tslib_1.__assign({}, this._queryParams, { alias: alias })
                  ),
                {
                  credentials: "include"
                }
              )
            ];
          case 1:
            response = _b.sent();
            if (!!response.ok) return [3 /*break*/, 3];
            _a = Error.bind;
            return [4 /*yield*/, response.text()];
          case 2:
            throw new (_a.apply(Error, [void 0, _b.sent()]))();
          case 3:
            return [4 /*yield*/, response.json()];
          case 4:
            actionInfo = _b.sent();
            actionInfosByAlias[alias] = actionInfo;
            return [2 /*return*/, actionInfo];
        }
      });
    });
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorQueryParams)],
    ActionController.prototype,
    "_queryParams",
    void 0
  );
  tslib_1.__decorate(
    [
      modal,
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [String, Object, Object, Object]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    ActionController.prototype,
    "executeCustomAction",
    null
  );
  tslib_1.__decorate(
    [
      handleError,
      tslib_1.__metadata("design:type", Function),
      tslib_1.__metadata("design:paramtypes", [String]),
      tslib_1.__metadata("design:returntype", Promise)
    ],
    ActionController.prototype,
    "getCustomActionInfo",
    null
  );
  return ActionController;
})();
export { ActionController };
//# sourceMappingURL=ActionController.js.map
