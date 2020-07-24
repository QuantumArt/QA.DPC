import { Intent, ProgressBar } from "@blueprintjs/core";
import React from "react";
import "./style.scss";

interface IProgressBarProps {
  value: number;
  intent: Intent;
  animate: boolean;
  className: string;
}

export const ProgressBarCell = ({ value, stateId }: { value: number; stateId: number }) => {
  if (!value) return null;
  const progressBarProps: IProgressBarProps = {
    value: value / 100,
    intent: Intent.NONE,
    animate: false,
    className: ""
  };

  switch (true) {
    case value === 100 && stateId === 3:
      progressBarProps.intent = Intent.SUCCESS;
      progressBarProps.className = "bp3-no-stripes";
      break;
    //если пауза
    case stateId === 3:
      progressBarProps.intent = Intent.WARNING;
      progressBarProps.className = "bp3-no-stripes";
      break;
    //если в процессе
    case stateId === 2:
      progressBarProps.intent = Intent.PRIMARY;
      progressBarProps.animate = true;
      break;
  }

  return (
    <div className="progress-bar-cell">
      <ProgressBar {...progressBarProps} className="bp3-no-stripes" />
      <span className="progress-bar-cell__label">{value}%</span>
    </div>
  );
};
