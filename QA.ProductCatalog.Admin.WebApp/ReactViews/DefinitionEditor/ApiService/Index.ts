import BaseApiService from "Shared/BaseApiService";
import {
  EnumBackendModel,
  IDefinitionNode,
  IEditFormModel
} from "DefinitionEditor/ApiService/ApiInterfaces";
import { BackendEnumType } from "DefinitionEditor/Enums";

class ApiService extends BaseApiService {
  constructor(private settings: DefinitionEditorSettings) {
    super();
  }

  /**
   * POST
   *
   * @param body
   */
  public getDefinitionLevel = async (body: FormData): Promise<IDefinitionNode[]> => {
    console.log(this.settings.getDefinitionLevelUrl);
    const res = await fetch(this.settings.getDefinitionLevelUrl, {
      method: "POST",
      body
    });
    return this.mapResponse(res, (x: IDefinitionNode[]) => x);
  };
  /**
   * GET
   *
   */
  public getSelectEnums = async (): Promise<{ [key in BackendEnumType]: EnumBackendModel[] }> => {
    const [updateEnum, publishEnum, preloadEnum, deleteEnum] = await Promise.all(
      Object.keys(this.settings.backendEnums).map(async enumMethod => {
        const result = await fetch(this.settings.backendEnums[enumMethod], {
          method: "GET"
        });
        return await this.tryGetResponse<EnumBackendModel[]>(result);
      })
    );
    return {
      [BackendEnumType.Update]: updateEnum,
      [BackendEnumType.Publish]: publishEnum,
      [BackendEnumType.Preload]: preloadEnum,
      [BackendEnumType.Delete]: deleteEnum
    };
  };

  /**
   * POST
   *
   * @param body
   */
  public getEditForm = async (body: FormData): Promise<IEditFormModel> => {
    const res = await fetch(this.settings.editBetaUrl, {
      method: "POST",
      body
    });
    return this.mapResponse(res, (x: IEditFormModel) => x);
  };
}

const apiService = new ApiService(window.definitionEditor);
export default apiService;
