import { inject } from "react-ioc";
import { command } from "Utils/Command";
import { rootUrl } from "Utils/Common";
import { DataValidator } from "Services/DataValidator";
import { EntityObject } from "Models/EditorDataModels";
import { ContentSchema } from "Models/EditorSchemaModels";

export class PublicationController {
  @inject private _dataValidator: DataValidator;

  private _query = document.location.search;

  @command
  public async publishEntity(entity: EntityObject, contentSchema: ContentSchema) {
    const errors = this._dataValidator.collectErrors(entity, contentSchema, false);
    if (errors.length > 0) {
      // TODO: React alert dialog
      window.alert(this._dataValidator.getErrorMessage(errors));
      return;
    }

    const response = await fetch(
      `${rootUrl}/ProductEditor/PublishProduct${this._query}&articleId=${entity._ServerId}`,
      {
        method: "POST",
        credentials: "include"
      }
    );
    if (
      response.status === 500 &&
      response.headers.get("Content-Type").startsWith("application/json")
    ) {
      const errorData = await response.json();
      // TODO: React alert dialog
      window.alert(errorData.Message);
    } else if (!response.ok) {
      throw new Error(await response.text());
    }
  }
}
