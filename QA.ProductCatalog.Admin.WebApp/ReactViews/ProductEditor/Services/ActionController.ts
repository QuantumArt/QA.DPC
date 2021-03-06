import "../../../wwwroot/js/pmrpc";
import QP8 from "../../../wwwroot/js/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { rootUrl } from "ProductEditor/Utils/Common";
import { handleError, modal } from "ProductEditor/Utils/Decorators";
import { CustomActionInfo } from "ProductEditor/Models/CustomActionModels";
import { ContentSchema } from "ProductEditor/Models/EditorSchemaModels";
import { EditorQueryParams } from "ProductEditor/Models/EditorSettingsModels";
import { EntityObject } from "ProductEditor/Models/EditorDataModels";

const actionInfosByAlias: {
  [alias: string]: CustomActionInfo;
} = {};

/** Выполнение произвольных CustomAction */
export class ActionController {
  @inject private _queryParams: EditorQueryParams;

  /** Найти CustomAction по Alias и выполнить его для заданной статьи */
  @modal
  @handleError
  public async executeCustomAction(
    actionAlias: string,
    entity: EntityObject,
    contentSchema: ContentSchema,
    options?: Partial<QP8.ExecuteActionOptions>
  ) {
    const actionInfo = await this.getCustomActionInfo(actionAlias);

    const executeOptions: QP8.ExecuteActionOptions = {
      actionCode: actionInfo.ActionCode,
      entityTypeCode: actionInfo.EntityTypeCode,
      parentEntityId: contentSchema.ContentId,
      entityId: entity._ServerId > 0 ? entity._ServerId : 0,
      ...options
    };

    QP8.executeBackendAction(executeOptions, this._queryParams.hostUID, window.parent);
  }

  @handleError
  private async getCustomActionInfo(alias: string) {
    let actionInfo = actionInfosByAlias[alias];
    if (actionInfo) {
      return actionInfo;
    }
    const response = await fetch(
      `${rootUrl}/ProductEditorQuery/GetCustomActionByAlias?${qs.stringify({
        ...this._queryParams,
        alias
      })}`,
      {
        credentials: "include"
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    actionInfo = await response.json();
    actionInfosByAlias[alias] = actionInfo;
    return actionInfo;
  }
}
