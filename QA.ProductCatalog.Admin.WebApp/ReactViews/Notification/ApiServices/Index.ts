import qs from "qs";
import BaseApiService from "Shared/BaseApiService";

class ApiService extends BaseApiService {
  /**
   * GET /Notification/IndexBeta
   *

   */
  public async getModel(): Promise<any> {
    const response = await fetch(`${this.rootUrl}/Notification/IndexBeta`);
    const mappedResponse = await this.mapResponse<any, any>(response, x => x);
    return mappedResponse;
  }
}

export const apiService = new ApiService();
