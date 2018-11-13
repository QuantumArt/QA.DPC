import "Scripts/pmrpc";
import QP8 from "Scripts/qp/QP8BackendApi.Interaction";
import qs from "qs";
import { inject } from "react-ioc";
import { rootUrl } from "Utils/Common";
import { handleError, modal } from "Utils/Decorators";
import { CustomActionInfo } from "Models/CustomActionModels";
import { ContentSchema } from "Models/EditorSchemaModels";
import { EditorQueryParams } from "Models/EditorSettingsModels";
import { EntityObject } from "Models/EditorDataModels";

const actionInfosByName: {
  [name: string]: CustomActionInfo;
} = {};

export class ActionController {
  @inject private _queryParams: EditorQueryParams;

  @modal
  @handleError
  public async executeCustomAction(
    actionName: string,
    entity: EntityObject,
    contentSchema: ContentSchema,
    options?: Partial<QP8.ExecuteActionOptions>
  ) {
    const actionInfo = await this.getCustomActionInfo(actionName);

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
  private async getCustomActionInfo(actionName: string) {
    let actionInfo = actionInfosByName[actionName];
    if (actionInfo) {
      return actionInfo;
    }
    const response = await fetch(
      `${rootUrl}/ProductEditorQuery/GetCustomActionByName?${qs.stringify({
        ...this._queryParams,
        actionName
      })}`,
      {
        credentials: "include"
      }
    );
    if (!response.ok) {
      throw new Error(await response.text());
    }
    actionInfo = await response.json();
    actionInfosByName[actionName] = actionInfo;
    return actionInfo;
  }
}
