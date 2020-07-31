import { Intent } from "@blueprintjs/core";

import { TaskState } from "../Shared/Enums";

const getStateInfoByStateId = (stateId: TaskState): [Intent, string] => {
  switch (stateId) {
    case TaskState.New:
      return [Intent.PRIMARY, "circle-arrow-up"];
    case TaskState.Running:
      return [Intent.PRIMARY, "refresh"];
    case TaskState.Done:
      return [Intent.SUCCESS, "tick-circle"];
    case TaskState.Failed:
      return [Intent.DANGER, "delete"];
    case TaskState.Cancelled:
      return [Intent.DANGER, "disable"];
  }
};

export { getStateInfoByStateId };
