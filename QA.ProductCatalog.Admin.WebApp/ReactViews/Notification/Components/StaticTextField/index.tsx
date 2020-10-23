import React from "react";
import cn from "classnames";
import "./Style.scss";

interface IProps {
  text: string;
  subtext?: string;
  className?: string;
}

export const StaticTextField = ({ text, subtext, className }: IProps) => {
  return (
    <div className={cn("static-text-field", className)}>
      <div className="static-text-label">
        {text} {subtext && <span className="static-text-sublabel">{subtext}</span>}
      </div>
    </div>
  );
};
