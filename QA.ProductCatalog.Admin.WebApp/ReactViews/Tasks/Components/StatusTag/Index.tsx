import { Tag, ITagProps } from "@blueprintjs/core";
import React from "react";
import { getTaskIntentDependsOnStatus } from "Shared/Utils";
import { TaskStatuses } from "Shared/Enums";
import cn from "classnames";
import "./Style.scss";

interface IProps {
  stateId: number;
  state: string;
  className?: string;
}

export const StatusTag = React.memo(({ stateId, className, state }: IProps) => {
  const tagProps = React.useMemo(() => {
    const tagProps: ITagProps = {
      round: true,
      icon: false,
      intent: getTaskIntentDependsOnStatus(stateId)
    };

    switch (stateId) {
      case TaskStatuses.Success:
        Object.assign(tagProps, { icon: "tick-circle" });
        break;
      case TaskStatuses.New:
        Object.assign(tagProps, { icon: "pause" });
        break;
      case TaskStatuses.Progress:
        Object.assign(tagProps, { icon: "refresh" });
        break;
    }
    return tagProps;
  }, [stateId]);

  return (
    <Tag {...tagProps} className={cn("status-cell-tag", className)}>
      {state}
    </Tag>
  );
});
