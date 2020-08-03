import { Popover, Icon, Position } from "@blueprintjs/core";
import React from "react";
import "./Style.scss";
import { Filter } from "../../TaskStore";

interface Props {
  filter: Filter;
  label: string | number;
  children: React.ReactElement<any>;
}

export const GridHeadFilterTooltip = ({ label, children, filter }: Props) => {
  return (
    <Popover
      position={Position.BOTTOM}
      usePortal={true}
      // forward ref
      portalClassName="grid-body"
    >
      <span className="filter-cell">
        {label} <Icon className="filter-cell__icon" iconSize={14} icon="filter" intent="primary" />
      </span>
      {React.Children.map(children, child => {
        return React.cloneElement(child, { filter });
      })}
      {children}
    </Popover>
  );
};
