import { useContext } from "react";
import { NotificationStoreContext } from "./NotificationStore";

export const useStore = () => {
  return useContext(NotificationStoreContext);
};
