import { Popover, Icon, Position } from "@blueprintjs/core";
import React from "react";
import "./Style.scss";

interface Props {
  label: string | number;
  children: React.ReactElement | string;
}

export const GridHeadFilterTooltip = ({ label, children }: Props) => {
  return (
    <Popover position={Position.BOTTOM} usePortal={true} portalClassName="grid-body">
      <span className="filter-cell">
        {label} <Icon className="filter-cell__icon" iconSize={14} icon="filter" intent="primary" />
      </span>
      {React.Children.map(children, child => React.cloneElement(child as React.ReactElement))}
    </Popover>
  );
};
