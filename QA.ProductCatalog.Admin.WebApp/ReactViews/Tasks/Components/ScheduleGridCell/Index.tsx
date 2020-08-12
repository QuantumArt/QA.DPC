import { Icon, Intent } from "@blueprintjs/core";
import React, { useState } from "react";
import { ScheduleDialog } from "./Subcomponents";
import "./Style.scss";

interface IProps {
  taskId: number;
  hasSchedule: boolean;
  scheduleEnabled: boolean;
  scheduleCronExpression: string;
  className?: string;
}

export const ScheduleGridCellCalendar = React.memo((props: IProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const { hasSchedule, className } = props;
  return (
    hasSchedule && (
      <span className={className}>
        <Icon
          icon="calendar"
          intent={Intent.PRIMARY}
          onClick={() => setIsOpen(true)}
          className="schedule-calendar-icon"
        />
        <ScheduleDialog {...props} isOpen={isOpen} closeDialogCb={() => setIsOpen(false)} />
      </span>
    )
  );
});
