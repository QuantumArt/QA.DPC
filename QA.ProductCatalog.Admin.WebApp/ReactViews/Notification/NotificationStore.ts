import { createContext } from "react";
import { action, computed, observable } from "mobx";
import { apiService } from "Notification/ApiServices";

export class NotificationStore {
  @observable systemSettings = 1;
  @observable channels;
  @observable generalSettings;

  init = () => {
    apiService.getModel();
  };
}
export const NotificationStoreContext = createContext(new NotificationStore());
