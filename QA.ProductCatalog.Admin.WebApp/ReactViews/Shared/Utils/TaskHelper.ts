import { TaskState, TaskStatuses } from "Shared/Enums";
import { Intent } from "@blueprintjs/core";
import { IconNames, IconName } from "@blueprintjs/icons";
import { SESSION_EXPIRED } from "Tasks/Constants";

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

export const getTaskIntentDependsOnState = (stateId: TaskState): Intent => {
  switch (stateId) {
    case TaskState.New:
      return Intent.PRIMARY;
    case TaskState.Running:
      return Intent.PRIMARY;
    case TaskState.Done:
      return Intent.SUCCESS;
    case TaskState.Failed:
      return Intent.DANGER;
    case TaskState.Cancelled:
      return Intent.DANGER;
  }
};

export const getTaskIconDependsOnState = (stateId: TaskState): IconName => {
  switch (stateId) {
    case TaskState.New:
      return IconNames.CIRCLE_ARROW_UP;
    case TaskState.Running:
      return IconNames.REFRESH;
    case TaskState.Done:
      return IconNames.TICK_CIRCLE;
    case TaskState.Failed:
      return IconNames.DELETE;
    case TaskState.Cancelled:
      return IconNames.DISABLE;
  }
};

export const getDateValueWithZeroAhead = (num: number): number | string => {
  if (num < 10) {
    return `0${num}`;
  }
  return num;
};

export const throwOnExpiredSession = (status: number): void => {
  if (status === 401) {
    document.body.innerHTML = "<h1>Сессия устарела. Переоткройте или обновите вкладку.</h1>";
    throw SESSION_EXPIRED;
  }
};

export const throwOnSameHashCode = (newHashCode: number, oldHashCode: number): void => {
  if (oldHashCode && newHashCode && newHashCode === oldHashCode) throw "hash code was repeated";
};
