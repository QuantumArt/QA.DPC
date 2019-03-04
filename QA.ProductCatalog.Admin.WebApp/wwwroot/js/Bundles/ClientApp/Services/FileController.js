import * as tslib_1 from "tslib";
import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import { FieldExactTypes } from "Models/EditorSchemaModels";
import { untracked, runInAction } from "mobx";
import { newUid } from "Utils/Common";
import { inject } from "react-ioc";
import { EditorQueryParams } from "ClientApp/Models/EditorSettingsModels";
/** Интеграция с SiteLibrary QP */
var FileController = /** @class */ (function() {
  function FileController() {
    var _this = this;
    this._callbackUid = newUid();
    this._observer = new QP8.BackendEventObserver(this._callbackUid, function(
      eventType,
      args
    ) {
      if (eventType === QP8.BackendEventTypes.FileSelected && args.filePath) {
        _this._resolvePromise(args.filePath);
      } else {
        _this._resolvePromise(CANCEL);
      }
    });
  }
  FileController.prototype.dispose = function() {
    this._observer.dispose();
  };
  /** Открыть окно SiteLibrary QP для выбора файла */
  FileController.prototype.selectFile = function(
    model,
    fieldSchema,
    customSubFolder
  ) {
    return tslib_1.__awaiter(this, void 0, void 0, function() {
      var subFolder, options, relativePath, filePath;
      var _this = this;
      return tslib_1.__generator(this, function(_a) {
        switch (_a.label) {
          case 0:
            subFolder = (
              "/" + [fieldSchema.SubFolder, customSubFolder].join("/")
            )
              .replace(/[\/\\]+/g, "\\")
              .replace(/\\$/, "");
            options = {
              subFolder: subFolder,
              isImage: fieldSchema.FieldType === FieldExactTypes.Image,
              useSiteLibrary: fieldSchema.UseSiteLibrary,
              libraryEntityId: fieldSchema.LibraryEntityId,
              libraryParentEntityId: fieldSchema.LibraryParentEntityId,
              callerCallback: this._callbackUid
            };
            QP8.openFileLibrary(
              options,
              this._queryParams.hostUID,
              window.parent
            );
            return [
              4 /*yield*/,
              new Promise(function(resolve) {
                _this._resolvePromise = resolve;
              })
            ];
          case 1:
            relativePath = _a.sent();
            if (relativePath === CANCEL) {
              return [2 /*return*/];
            }
            filePath = [customSubFolder, relativePath]
              .join("/")
              .replace(/[\/\\]+/g, "/")
              .replace(/^\//, "");
            runInAction("selectFile", function() {
              model[fieldSchema.FieldName] = filePath;
              model.setTouched(fieldSchema.FieldName);
            });
            return [2 /*return*/];
        }
      });
    });
  };
  /** Открыть окно QP для предпросмотра изображения */
  FileController.prototype.previewImage = function(model, fieldSchema) {
    var entityId = model._ServerId > 0 ? model._ServerId : 0;
    var fieldId = fieldSchema.FieldId;
    var fileName = untracked(function() {
      return model[fieldSchema.FieldName];
    });
    if (fileName) {
      QP8.previewImage(
        { entityId: entityId, fieldId: fieldId, fileName: fileName },
        this._queryParams.hostUID,
        window.parent
      );
    }
  };
  /** Скачать файл из SiteLibrary */
  FileController.prototype.downloadFile = function(model, fieldSchema) {
    var entityId = model._ServerId > 0 ? model._ServerId : 0;
    var fieldId = fieldSchema.FieldId;
    var fileName = untracked(function() {
      return model[fieldSchema.FieldName];
    });
    if (fileName) {
      QP8.downloadFile(
        { entityId: entityId, fieldId: fieldId, fileName: fileName },
        this._queryParams.hostUID,
        window.parent
      );
    }
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", EditorQueryParams)],
    FileController.prototype,
    "_queryParams",
    void 0
  );
  return FileController;
})();
export { FileController };
var CANCEL = Symbol();
//# sourceMappingURL=FileController.js.map
