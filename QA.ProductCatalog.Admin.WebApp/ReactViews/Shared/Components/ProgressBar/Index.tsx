import React from "react";
import { IProgressBarProps, ProgressBar as BlueprintProgressBar } from "@blueprintjs/core";
import cn from "classnames";

import "./Style.css";

interface Props {
  barWidth: string;
  defaultBarProps: IProgressBarProps;
  labelWidth?: string;
  barClassName?: string;
  labelClassName?: string;
}

const getFormattedProgress = (progress: number): string => {
  return `${progress || 0}%`;
};

export default function ProgressBar(props: Props) {
  const { defaultBarProps, barWidth, labelWidth, barClassName, labelClassName } = props;
  const value = (defaultBarProps.value || 0) / 100;
  return (
    <div className={cn("progress-bar", barClassName)} style={{ width: barWidth }}>
      <BlueprintProgressBar {...defaultBarProps} value={value} />
      <span
        className={cn("progress-bar__label", labelClassName)}
        style={labelWidth && { width: labelWidth }}
      >
        {getFormattedProgress(defaultBarProps.value)}
      </span>
    </div>
  );
}
