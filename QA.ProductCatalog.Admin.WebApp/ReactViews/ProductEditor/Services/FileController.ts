import "../../../wwwroot/js/pmrpc";
import QP8 from "../../../wwwroot/js/qp/QP8BackendApi.Interaction";
import { FileFieldSchema, FieldExactTypes } from "ProductEditor/Models/EditorSchemaModels";
import { ArticleObject } from "ProductEditor/Models/EditorDataModels";
import { untracked, runInAction } from "mobx";
import { newUid } from "ProductEditor/Utils/Common";
import { inject } from "react-ioc";
import { EditorQueryParams } from "ProductEditor/Models/EditorSettingsModels";

/** Интеграция с SiteLibrary QP */
export class FileController {
  @inject private _queryParams: EditorQueryParams;

  private _resolvePromise: (filePath: string | typeof CANCEL) => void;
  private _callbackUid = newUid();

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

  /** Открыть окно SiteLibrary QP для выбора файла */
  public async selectFile(
    model: ArticleObject,
    fieldSchema: FileFieldSchema,
    customSubFolder?: string
  ) {
    const subFolder = ("/" + [fieldSchema.SubFolder, customSubFolder].join("/"))
      .replace(/[\/\\]+/g, "\\")
      .replace(/\\$/, "");

    const options: QP8.OpenFileLibraryOptions = {
      subFolder,
      isImage: fieldSchema.FieldType === FieldExactTypes.Image,
      useSiteLibrary: fieldSchema.UseSiteLibrary,
      libraryEntityId: fieldSchema.LibraryEntityId,
      libraryParentEntityId: fieldSchema.LibraryParentEntityId,
      callerCallback: this._callbackUid
    };
    QP8.openFileLibrary(options, this._queryParams.hostUID, window.parent);

    const relativePath = await new Promise<string | typeof CANCEL>(resolve => {
      this._resolvePromise = resolve;
    });
    if (relativePath === CANCEL) {
      return;
    }

    const filePath = [customSubFolder, relativePath]
      .join("/")
      .replace(/[\/\\]+/g, "/")
      .replace(/^\//, "");

    runInAction("selectFile", () => {
      model[fieldSchema.FieldName] = filePath;
      model.setTouched(fieldSchema.FieldName);
    });
  }

  /** Открыть окно QP для предпросмотра изображения */
  public previewImage(model: ArticleObject, fieldSchema: FileFieldSchema) {
    const entityId = model._ServerId > 0 ? model._ServerId : 0;
    const fieldId = fieldSchema.FieldId;
    const fileName = untracked(() => model[fieldSchema.FieldName]);
    if (fileName) {
      QP8.previewImage({ entityId, fieldId, fileName }, this._queryParams.hostUID, window.parent);
    }
  }

  /** Скачать файл из SiteLibrary */
  public downloadFile(model: ArticleObject, fieldSchema: FileFieldSchema) {
    const entityId = model._ServerId > 0 ? model._ServerId : 0;
    const fieldId = fieldSchema.FieldId;
    const fileName = untracked(() => model[fieldSchema.FieldName]);
    if (fileName) {
      QP8.downloadFile({ entityId, fieldId, fileName }, this._queryParams.hostUID, window.parent);
    }
  }
}

const CANCEL = Symbol();
