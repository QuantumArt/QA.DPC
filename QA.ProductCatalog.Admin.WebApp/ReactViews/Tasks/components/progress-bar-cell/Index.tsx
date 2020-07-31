import { Intent } from "@blueprintjs/core";
import React from "react";
import "./Style.scss";
import ProgressBar from "Shared/Components/ProgressBar";
import { TaskStatuses } from "Shared/Enums";
import { getTaskIntentDependsOnStatus } from "Shared/Utils";

interface IProgressBarProps {
  value: number;
  animate: boolean;
  stripes: boolean;
  intent: Intent;
}

export const ProgressBarCell = ({ value, stateId }: { value: number; stateId: number }) => {
  if (value !== 0 && !value) return null;
  const progressBarProps: IProgressBarProps = {
    value: value,
    intent: getTaskIntentDependsOnStatus(stateId),
    animate: false,
    stripes: false
  };
  if (stateId === TaskStatuses.Cancelled || stateId === TaskStatuses.Error) return null;

  switch (true) {
    case stateId === TaskStatuses.Progress:
      progressBarProps.animate = true;
      progressBarProps.stripes = true;
      break;
  }

  return (
    <div className="progress-bar-cell">
      <ProgressBar
        defaultBarProps={progressBarProps}
        barWidth="140px"
        labelWidth="40px"
        labelClassName="progress-bar-cell__label"
      />
    </div>
  );
};
