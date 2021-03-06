import React from "react";
import cn from "classnames";
import { Popover, Icon, Position, Intent } from "@blueprintjs/core";
import { observer } from "mobx-react-lite";
import { Filter, Pagination } from "Tasks/TaskStore";
import { getClassnameByIntent } from "Shared/Utils";
import "./Style.scss";

interface Props {
  label: string | number;
  filter: Filter;
  pagination: Pagination;
  children: React.ReactElement;
}

export const GridHeadFilterTooltip = observer(({ label, filter, pagination, children }: Props) => {
  const intent = filter.isActive ? Intent.SUCCESS : Intent.PRIMARY;
  return (
    <Popover position={Position.BOTTOM} minimal>
      <span className={cn("filter-cell", getClassnameByIntent("color", intent))}>
        {label} <Icon className="filter-cell__icon" iconSize={14} icon="filter" intent={intent} />
      </span>
      {React.Children.map(children, child => React.cloneElement(child, { filter, pagination }))}
    </Popover>
  );
});
