import "../../Scripts/pmrpc";
import QP8 from "../../Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { FileFieldSchema, FieldExactTypes } from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
import { untracked, runInAction } from "mobx";

export class FileController {
  private _hostUid = qs.parse(document.location.search).hostUID as string;
  private _resolvePromise: (filePath: string | typeof CANCEL) => void;
  private _callbackUid = Math.random()
    .toString(36)
    .slice(2);

  private _observer = new QP8.BackendEventObserver(this._callbackUid, (eventType, args) => {
    if (eventType === QP8.BackendEventTypes.FileSelected && args.filePath) {
      this._resolvePromise(args.filePath);
    } else {
      this._resolvePromise(CANCEL);
    }
  });

  public dispose() {
    this._observer.dispose();
  }

  public async selectFile(model: ArticleObject, fieldSchema: FileFieldSchema) {
    const options: QP8.OpenFileLibraryOptions = {
      isImage: fieldSchema.FieldType === FieldExactTypes.Image,
      useSiteLibrary: fieldSchema.UseSiteLibrary,
      subFolder: fieldSchema.SubFolder,
      libraryEntityId: fieldSchema.LibraryEntityId,
      libraryParentEntityId: fieldSchema.LibraryParentEntityId,
      callerCallback: this._callbackUid
    };
    QP8.openFileLibrary(options, this._hostUid, window.parent);

    const filePath = await new Promise<string | typeof CANCEL>(resolve => {
      this._resolvePromise = resolve;
    });
    if (filePath === CANCEL) {
      return;
    }

    runInAction("selectFile", () => {
      model[fieldSchema.FieldName] = filePath;
    });
  }

  public previewImage(model: ArticleObject, fieldSchema: FileFieldSchema) {
    const entityId = model._ServerId > 0 ? model._ServerId : 0;
    const fieldId = fieldSchema.FieldId;
    const fileName = untracked(() => model[fieldSchema.FieldName]);
    if (fileName) {
      QP8.previewImage({ entityId, fieldId, fileName }, this._hostUid, window.parent);
    }
  }

  public downloadFile(model: ArticleObject, fieldSchema: FileFieldSchema) {
    const entityId = model._ServerId > 0 ? model._ServerId : 0;
    const fieldId = fieldSchema.FieldId;
    const fileName = untracked(() => model[fieldSchema.FieldName]);
    if (fileName) {
      QP8.downloadFile({ entityId, fieldId, fileName }, this._hostUid, window.parent);
    }
  }
}

const CANCEL = Symbol();
