import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { FileFieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
import { command } from "Utils/Command";
import { untracked } from "mobx";

export class FileController {
  private _hostUid = qs.parse(document.location.search).hostUID as string;
  private _resolvePromise: () => void;
  private _callbackUid = Math.random()
    .toString(36)
    .slice(2);

  private _observer = new QP8.BackendEventObserver(this._callbackUid, (_eventType, _args) => {
    this._resolvePromise();
  });

  public dispose() {
    this._observer.dispose();
  }

  @command
  public previewImage(model: ArticleObject, fieldSchema: FileFieldSchema) {
    const entityId = model._ServerId > 0 ? model._ServerId : 0;
    const fieldId = fieldSchema.FieldId;
    const fileName = untracked(() => model[fieldSchema.FieldName]);
    if (fileName) {
      QP8.previewImage({ entityId, fieldId, fileName }, this._hostUid, window.parent);
    }
  }

  @command
  public downloadFile(model: ArticleObject, fieldSchema: FileFieldSchema) {
    const entityId = model._ServerId > 0 ? model._ServerId : 0;
    const fieldId = fieldSchema.FieldId;
    const fileName = untracked(() => model[fieldSchema.FieldName]);
    if (fileName) {
      QP8.downloadFile({ entityId, fieldId, fileName }, this._hostUid, window.parent);
    }
  }
}
