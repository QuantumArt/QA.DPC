import { Intent } from "@blueprintjs/core";
import { IconName } from "@blueprintjs/icons";

import { getTaskIntentDependsOnState, getTaskIconDependsOnState } from "Shared/Utils";

import { TaskState } from "Shared/Enums";

const getStateInfoByStateId = (stateId: TaskState): [Intent, IconName] => {
  const intent = getTaskIntentDependsOnState(stateId);
  const iconName = getTaskIconDependsOnState(stateId);
  return [intent, iconName];
};

export { getStateInfoByStateId };
