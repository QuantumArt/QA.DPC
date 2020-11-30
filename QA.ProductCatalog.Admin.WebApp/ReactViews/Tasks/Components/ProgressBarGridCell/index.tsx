import { Intent } from "@blueprintjs/core";
import React from "react";
import ProgressBar from "Shared/Components/ProgressBar";
import { TaskStatuses } from "Shared/Enums";
import { getTaskIntentDependsOnStatus } from "Shared/Utils";
import "./Style.scss";

interface IProgressBarProps {
  value: number;
  animate: boolean;
  stripes: boolean;
  intent: Intent;
}
interface IProps {
  value: number;
  stateId: number;
}

export const ProgressBarGridCell = React.memo(({ value, stateId }: IProps) => {
  if (value !== 0 && !value) return null;
  const progressBarProps: IProgressBarProps = {
    value: value,
    intent: getTaskIntentDependsOnStatus(stateId),
    animate: false,
    stripes: false
  };
  if (stateId === TaskStatuses.Cancelled || stateId === TaskStatuses.Error) return null;
  if (stateId === TaskStatuses.Progress) {
    progressBarProps.animate = true;
    progressBarProps.stripes = true;
  }

  return (
    <ProgressBar
      defaultBarProps={progressBarProps}
      barWidth="110px"
      labelWidth="30px"
    />
  );
});
