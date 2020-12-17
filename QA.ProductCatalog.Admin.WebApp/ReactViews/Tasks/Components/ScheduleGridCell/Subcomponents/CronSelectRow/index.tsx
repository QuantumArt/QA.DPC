import React, { ReactNode } from "react";
import "./Style.scss";

export const CronSelectRow = ({ label, children }: { label?: string; children: ReactNode }) => {
  return (
    <div className="schedule-popup__select-wrap">
      <div className="schedule-popup__label-row">{label && label}</div>
      <div className="schedule-popup__select-row">{children}</div>
    </div>
  );
};
