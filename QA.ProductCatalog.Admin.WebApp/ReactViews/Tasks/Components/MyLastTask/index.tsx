import React from "react";
import { Callout, Icon, IProgressBarProps, Tooltip } from "@blueprintjs/core";
import { Task } from "Tasks/ApiServices/DataContracts";
import { DateGridCell, StatusCell } from "Tasks/Components";
import { getTaskIntentDependsOnStatus } from "Shared/Utils";
import ProgressBar from "Shared/Components/ProgressBar";
import { TaskStatuses } from "Shared/Enums";
import { getClassnameByIntent } from "Shared/Utils";
import cn from "classnames";
import "./Style.scss";

interface IProps {
  task: Task;
  width?: number;
}

export const MyLastTask = ({ task, width }: IProps) => {
  if (!task) return null;
  const intent = getTaskIntentDependsOnStatus(task.StateId);

  const TooltipContent = React.useMemo(
    () => {
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
            Создано: {DateGridCell({ value: task.CreatedTime })}
          </div>
          <div className="last-task-tooltip__row last-task-tooltip__row--margin-bottom">
            Изменено: {DateGridCell({ value: task.LastStatusChangeTime })}
          </div>
          <div className="last-task-tooltip__bar-wrapper">
            <ProgressBar defaultBarProps={progressBarProps} withLabel={false} />
          </div>
        </div>
      );
    },
    [task.Progress, task.StateId, task.DisplayName, task.CreatedTime, task.LastStatusChangeTime]
  );

  return (
    <div className="my-last-task" style={width && { width: width }}>
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
              getClassnameByIntent("my-last-task__name-", intent)
            )}
          >
            {task.Name}
          </span>
          <StatusCell className="my-last-task__header-item" value={task.StateId} />
          <Tooltip
            className="last-task-tooltip my-last-task__header-item"
            popoverClassName="last-task-tooltip__wrap"
            position={"bottom"}
            content={TooltipContent}
          >
            <Icon icon="info-sign" intent={intent} iconSize={24} className="my-last-task__icon" />
          </Tooltip>
        </div>
        <div className="my-last-task__message">{task.Message}</div>
      </Callout>
    </div>
  );
};
