import { Button, Intent } from "@blueprintjs/core";
import React, { useState } from "react";
import { ScheduleDialog } from "./Subcomponents";

interface IProps {
  taskIdNumber: number;
  hasSchedule: boolean;
  isScheduleEnabled: boolean;
  scheduleCronExpression: string;
  intent: Intent;
  className?: string;
}

export const ScheduleGridCellCalendar = React.memo((props: IProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const { hasSchedule, intent, className, ...rest } = props;
  return (
    hasSchedule && (
      <div className={className}>
        <Button intent={intent} icon="calendar" onClick={() => setIsOpen(true)} minimal />
        <ScheduleDialog {...props} isOpen={isOpen} closeDialogCb={() => setIsOpen(false)} />
      </div>
    )
  );
});
