import React from "react";
import { Callout, Icon, IProgressBarProps, Tooltip } from "@blueprintjs/core";
import { Task } from "Tasks/Api-services/DataContracts";
import { DateCell, StatusCell } from "Tasks/Components";
import ProgressBar from "Shared/Components/ProgressBar";
import { TaskStatuses } from "Shared/Enums";
import { getClassNameByIntent, getTaskIntentDependsOnStatus } from "Shared/Utils";
import cn from "classnames";
import "./Style.scss";

export const MyLastTask = ({ task }: { task: Task }) => {
  if (!task) return null;
  const intent = getTaskIntentDependsOnStatus(task.StateId);

  const TooltipContent = () => {
    const progressBarProps: IProgressBarProps = {
      value: task.Progress,
      intent: getTaskIntentDependsOnStatus(task.StateId),
      animate: false,
      stripes: false
    };

    if (task.StateId === TaskStatuses.Progress) {
      progressBarProps.animate = true;
      progressBarProps.stripes = true;
    }

    return (
      <div className="last-task-tooltip__wrap">
        <div className="last-task-tooltip__row last-task-tooltip__row--margin-top">
          Заказчик: {task.DisplayName}
        </div>
        <div className="last-task-tooltip__row">
          Создано: {DateCell({ value: task.CreatedTime })}
        </div>
        <div className="last-task-tooltip__row last-task-tooltip__row--margin-bottom">
          Изменено: {DateCell({ value: task.LastStatusChangeTime })}
        </div>
        <div className="last-task-tooltip__bar-wrapper">
          <ProgressBar defaultBarProps={progressBarProps} withLabel={false} />
        </div>
      </div>
    );
  };

  return (
    <div className="my-last-task">
      <Callout
        icon={
          <Icon icon="time" intent={intent} iconSize={20} className="my-last-task__callout-icon" />
        }
        intent={intent}
        className="my-last-task__callout"
      >
        <div className="my-last-task__header">
          <span className="my-last-task__header-item my-last-task__id">{task.Id}</span>
          <span
            className={cn(
              "my-last-task__header-item my-last-task__name",
              getClassNameByIntent("my-last-task__name-", intent)
            )}
          >
            {task.Name}
          </span>
          <StatusCell className="my-last-task__header-item" value={task.StateId} />
          <Tooltip
            className="last-task-tooltip my-last-task__header-item"
            popoverClassName="last-task-tooltip__wrap"
            position={"bottom"}
            content={<TooltipContent />}
          >
            <Icon icon="info-sign" intent={intent} iconSize={24} className="my-last-task__icon" />
          </Tooltip>
        </div>
        <div className="my-last-task__message">{task.Message}</div>
      </Callout>
    </div>
  );
};
