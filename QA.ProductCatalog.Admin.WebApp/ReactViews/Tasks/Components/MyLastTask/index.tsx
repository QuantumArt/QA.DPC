import React, { memo } from "react";
import { Callout, Icon, IProgressBarProps, Tooltip } from "@blueprintjs/core";
import { Task } from "Tasks/ApiServices/DataContracts";
import { StatusTag } from "Tasks/Components";
import { getTaskIntentDependsOnStatus } from "Shared/Utils";
import ProgressBar from "Shared/Components/ProgressBar";
import { TaskStatuses } from "Shared/Enums";
import { getClassnameByIntent } from "Shared/Utils";
import cn from "classnames";
import { format, isValid } from "date-fns";
import "./Style.scss";
import { l } from "Tasks/Localization";

interface IProps {
  task: Task;
  width?: number;
}

export const MyLastTask = memo(({ task, width }: IProps) => {
  if (!task) return null;
  const createdTime = new Date(task.CreatedTime);
  const lastStatusChangeTime = new Date(task.LastStatusChangeTime);
  const intent = getTaskIntentDependsOnStatus(task.StateId);
  const TooltipContent = React.useMemo(() => {
    const progressBarProps: IProgressBarProps = {
      value: task.Progress,
      intent: getTaskIntentDependsOnStatus(task.StateId),
      animate: task.StateId === TaskStatuses.Progress,
      stripes: task.StateId === TaskStatuses.Progress
    };

    return (
      <div className="last-task-tooltip__wrap">
        <div className="last-task-tooltip__row last-task-tooltip__row--margin-top">
          {l("customer")}: {task.UserName}
        </div>
        <div className="last-task-tooltip__row">
          {l("created")}:{" "}
          {task.CreatedTime && isValid(createdTime) && format(createdTime, "DD.MM.YYYY HH:mm:ss")}
        </div>
        <div className="last-task-tooltip__row last-task-tooltip__row--margin-bottom">
          {l("changed")}:{" "}
          {task.LastStatusChangeTime &&
            isValid(lastStatusChangeTime) &&
            format(lastStatusChangeTime, "DD.MM.YYYY HH:mm:ss")}
        </div>
        <div className="last-task-tooltip__bar-wrapper">
          <ProgressBar defaultBarProps={progressBarProps} withLabel={false} />
        </div>
      </div>
    );
  }, [task.Progress, task.StateId, task.DisplayName, task.CreatedTime, task.LastStatusChangeTime]);

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
          <StatusTag
            className="my-last-task__header-item"
            state={task.State}
            stateId={task.StateId}
          />
          <Tooltip
            className="last-task-tooltip my-last-task__header-item"
            popoverClassName="last-task-tooltip__wrap"
            position={"bottom"}
            content={TooltipContent}
          >
            <Icon icon="info-sign" intent={intent} iconSize={23} className="my-last-task__icon" />
          </Tooltip>
        </div>
        <div className="my-last-task__message">{task.Message}</div>
      </Callout>
    </div>
  );
});
