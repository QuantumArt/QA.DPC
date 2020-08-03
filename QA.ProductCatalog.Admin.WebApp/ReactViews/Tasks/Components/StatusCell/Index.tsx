import { Tag, ITagProps } from "@blueprintjs/core";
import React from "react";
import { getTaskIntentDependsOnStatus } from "Shared/Utils";
import { TaskStatuses } from "Shared/Enums";
import cn from "classnames";
import "./Style.scss";

interface StatusCell {
  label: string;
  props: ITagProps;
}

export const StatusCell = ({ value, className }) => {
  const statusValues = window.QP.Tasks.tableFields.statusValues;
  const statusObj = statusValues.find(status => {
    return status.value === value;
  });

  if (!statusObj) return null;

  const StatusCellObj: StatusCell = {
    label: statusObj.label,
    props: { round: true, icon: false, intent: getTaskIntentDependsOnStatus(value) }
  };

  switch (statusObj.value) {
    case TaskStatuses.Success:
      Object.assign(StatusCellObj.props, { icon: "tick-circle" });
      break;
    case TaskStatuses.New:
      Object.assign(StatusCellObj.props, { icon: "pause" });
      break;
    case TaskStatuses.Progress:
      Object.assign(StatusCellObj.props, { icon: "refresh" });
      break;
  }

  return (
    <Tag {...StatusCellObj.props} className={cn("status-cell-tag", className)}>
      {StatusCellObj.label}
    </Tag>
  );
};
