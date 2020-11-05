import BaseApiService from "Shared/BaseApiService";
import { IChannelsResponse } from "Notification/ApiServices/ApiInterfaces";

class ApiService extends BaseApiService {
  /**
   * GET /Notification/IndexBeta
   */
  public async getModel(): Promise<IChannelsResponse> {
    const response = await fetch(`${this.rootUrl}/Notification/_Index`);
    return await this.mapResponse<IChannelsResponse, IChannelsResponse>(response, x => x);
  }
  public async updateConfiguration(): Promise<IChannelsResponse> {
    const response = await fetch(`${this.rootUrl}/Notification/UpdateConfiguration`);
    return await this.mapResponse<IChannelsResponse, IChannelsResponse>(response, x => x);
  }
}

export const apiService = new ApiService();
