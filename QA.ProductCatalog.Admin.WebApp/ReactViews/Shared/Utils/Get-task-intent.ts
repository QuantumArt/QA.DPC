import { TaskStatuses } from "Shared/Enums";
import { Intent } from "@blueprintjs/core";

export const getTaskIntentDependsOnStatus = (status: TaskStatuses) => {
  switch (status) {
    case TaskStatuses.Cancelled:
      return Intent.NONE;
    case TaskStatuses.Error:
      return Intent.DANGER;
    case TaskStatuses.New:
      return Intent.WARNING;
    case TaskStatuses.Progress:
      return Intent.PRIMARY;
    case TaskStatuses.Success:
      return Intent.SUCCESS;
  }
};
