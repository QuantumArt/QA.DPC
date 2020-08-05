import BaseApiService from "Shared/BaseApiService";
import { IDefinitionNode } from "DefinitionEditor/ApiService/ApiInterfaces";

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
}

const apiService = new ApiService(window.definitionEditor);
export default apiService;