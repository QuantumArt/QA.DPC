import React from "react";
import { IProgressBarProps, ProgressBar as BlueprintProgressBar } from "@blueprintjs/core";
import cn from "classnames";
import "./Style.scss";

interface Props {
  barWidth?: string;
  defaultBarProps: IProgressBarProps;
  labelWidth?: string;
  barClassName?: string;
  labelClassName?: string;
  withLabel?: boolean;
}

const getFormattedProgress = (progress: number): string => {
  return `${progress || 0}%`;
};

export default function ProgressBar({
  defaultBarProps,
  barWidth,
  labelWidth,
  barClassName,
  labelClassName,
  withLabel = true
}: Props) {
  const value = (defaultBarProps.value || 0) / 100;
  return (
    <div className={cn("progress-bar", barClassName)} style={{ width: barWidth }}>
      <BlueprintProgressBar {...defaultBarProps} value={value} />
      {withLabel && (
        <span
          className={cn("progress-bar__label", labelClassName)}
          style={labelWidth && { width: labelWidth }}
        >
          {getFormattedProgress(defaultBarProps.value)}
        </span>
      )}
    </div>
  );
}
