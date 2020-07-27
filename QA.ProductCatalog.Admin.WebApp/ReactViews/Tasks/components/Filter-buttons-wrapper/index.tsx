import { Popover, Icon } from "@blueprintjs/core";
import React from "react";
import "./style.scss";
import { Position } from "@blueprintjs/core/lib/esm/common/position";

interface Props {
  label: string | number;
  children: string | JSX.Element;
}

export const FilterTooltip = ({ label, children }: Props) => {
  return (
    <Popover
      content={children}
      position={Position.BOTTOM}
      usePortal={true}
      // forward ref
      portalClassName="grid-body"
    >
      <span className="filter-cell">
        {label} <Icon className="filter-cell__icon" iconSize={14} icon="filter" intent="primary" />
      </span>
    </Popover>
  );
};
