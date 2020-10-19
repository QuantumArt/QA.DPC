import React, { ReactNode } from "react";
import cn from "classnames";

import "@blueprintjs/datetime/lib/css/blueprint-datetime.css";
import "./Style.scss";

export const SelectRow = ({
  className,
  label,
  children
}: {
  className?: string;
  label?: string;
  children: ReactNode;
}) => {
  return (
    <div className={cn("select-row", className)}>
      {label && <div className="select-row__label">{label}</div>}
      {children}
    </div>
  );
};
