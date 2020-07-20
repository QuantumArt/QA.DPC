import { ITagProps } from "@blueprintjs/core/src/components/tag/tag";
import { Intent, Tag } from "@blueprintjs/core";
import React from "react";
import "./style.scss";

interface StatusCell {
  label: string;
  props: ITagProps;
}

export const StatusCell = ({ value }) => {
  const statusValues = window.QP.Tasks.tableFields.statusValues;
  const statusObj = statusValues.find(status => {
    return status.value === value;
  });

  if (!statusObj) return null;

  const StatusCellObj: StatusCell = {
    label: statusObj.text,
    props: { round: true, icon: false, intent: Intent.NONE }
  };

  switch (statusObj.value) {
    case 3:
      Object.assign(StatusCellObj.props, { icon: "tick-circle", intent: Intent.SUCCESS });
      break;
    case 2:
      Object.assign(StatusCellObj.props, { icon: "pause", intent: Intent.WARNING });
      break;
    case 1:
      Object.assign(StatusCellObj.props, { icon: "automatic-updates", intent: Intent.PRIMARY });
      break;
  }

  return (
    <Tag {...StatusCellObj.props} className="status-cell-tag">
      {StatusCellObj.label}
    </Tag>
  );
};
