import { Icon, Intent } from "@blueprintjs/core";
import React, { useState } from "react";
import { ScheduleDialog } from "./Subcomponents";

import "./Style.scss";

interface IProps {
  taskId: number;
  hasSchedule: boolean;
  scheduleCronExpression: string;
}

export const ScheduleGridCell = (props: IProps) => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div>
      {props.hasSchedule && props.scheduleCronExpression}
      <Icon icon="calendar" intent={Intent.PRIMARY} onClick={() => setIsOpen(true)} />
      <ScheduleDialog {...props} isOpen={isOpen} closeDialogCb={() => setIsOpen(false)} />
    </div>
  );
};
