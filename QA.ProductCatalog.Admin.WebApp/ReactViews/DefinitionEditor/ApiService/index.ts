import BaseApiService from "Shared/BaseApiService";
import {
  EnumBackendModel,
  IDefinitionNode,
  IEditFormModel
} from "DefinitionEditor/ApiService/ApiInterfaces";
import { BackendEnumType } from "DefinitionEditor/Enums";
import { mapEditFormModel } from "DefinitionEditor/ApiService/Mappers";

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
    const res = await fetch(this.settings.getDefinitionLevelUrl, {
      method: "POST",
      body
    });
    return this.mapResponse(res, (x: IDefinitionNode[]) => x);
  };

  public getInitialNode = async (contentId: FormData): Promise<string> => {
    const res = await fetch(`${this.settings.getInitialNodeUrl}?contentId=${contentId}`, {
      method: "GET"
    });
    return this.mapResponse(res, (x: string) => x);
  };

  /**
   * POST
   *
   * @param body
   */
  public getSingleNode = async (body: FormData): Promise<IDefinitionNode> => {
    const res = await fetch(this.settings.getSingleNodeUrl, {
      method: "POST",
      body
    });
    return this.mapResponse(res, (x: IDefinitionNode) => x);
  };

  /**
   * GET
   *
   */
  public getSelectEnums = async (): Promise<{ [key in BackendEnumType]: EnumBackendModel[] }> => {
    const [updateEnum, publishEnum, preloadEnum, deleteEnum, cloneEnum] = await Promise.all(
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
      [BackendEnumType.Delete]: deleteEnum,
      [BackendEnumType.Clone]: cloneEnum
    };
  };

  /**
   * POST
   * @param body formData
   */
  public getEditForm = async (body: string): Promise<IEditFormModel> => {
    const res = await fetch(this.settings.editUrl, {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
      },
      method: "POST",
      body
    });
    return this.mapResponse(res, (x: IEditFormModel) => mapEditFormModel(x));
  };

  /**
   * POST
   * @param body formData
   */
  public saveField = async (body: string): Promise<IEditFormModel> => {
    const res = await fetch(this.settings.saveFieldUrl, {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
      },
      method: "POST",
      body
    });
    return this.mapResponse(res, (x: IEditFormModel) => mapEditFormModel(x));
  };
  /**
   * POST
   * @param body formData
   */
  public saveContent = async (body: string): Promise<IEditFormModel> => {
    const res = await fetch(this.settings.saveContentUrl, {
      headers: {
        "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8"
      },
      method: "POST",
      body
    });
    return this.mapResponse(res, (x: IEditFormModel) => mapEditFormModel(x));
  };
}

const apiService = new ApiService(window.definitionEditor);
export default apiService;
