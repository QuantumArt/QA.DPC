import { Intent } from "@blueprintjs/core";
import React from "react";
import "./Style.scss";
import ProgressBar from "Shared/Components/ProgressBar";

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
    intent: Intent.NONE,
    animate: false,
    stripes: false
  };

  switch (true) {
    case value === 100 && stateId === 3:
      progressBarProps.intent = Intent.SUCCESS;
      break;
    //если пауза
    case stateId === 3:
      progressBarProps.intent = Intent.WARNING;
      break;
    //если в процессе
    case stateId === 2:
      progressBarProps.intent = Intent.PRIMARY;
      progressBarProps.animate = true;
      progressBarProps.stripes = true;
      progressBarProps.value = 30;
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
