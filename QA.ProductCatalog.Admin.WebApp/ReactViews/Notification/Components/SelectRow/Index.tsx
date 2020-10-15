import React, { ReactNode } from "react";
import cn from "classnames";
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
    <div className={cn("field", className)}>
      {label && <label className="field__label">{label}</label>}
      <div className="field__element">{children}</div>
    </div>
  );
};
