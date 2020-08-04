import { useContext } from "react";
import { TaskStoreContext } from "./TaskStore";

export const useStore = () => {
  return useContext(TaskStoreContext);
};
