import React from "react";
import { ProgressBar as BlueprintProgressBar } from "@blueprintjs/core";

import "./style.css";

type Props = {
  barWidth: string;
  progress: number;
};

const getFormattedProgress = (progress: number): string => {
  return `${progress || 0}%`;
};

export default function ProgressBar({ progress, barWidth }: Props) {
  return (
    <div className="progress-bar">
      <div style={{ width: barWidth }}>
        <BlueprintProgressBar intent="none" animate={false} stripes={false} value={progress || 0} />
      </div>
      <span className="progress-bar__label">{getFormattedProgress(progress)}</span>
    </div>
  );
}
