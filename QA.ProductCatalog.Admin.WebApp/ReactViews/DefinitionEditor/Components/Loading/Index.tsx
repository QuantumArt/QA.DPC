import React, { CSSProperties } from "react";
import cn from "classnames";
import "./Style.scss";

interface Props {
  style?: CSSProperties;
  className?: string;
  active: boolean;
}

const Loading = ({ style, className, active }: Props) => {
  return (
    <div
      className={cn("loader", className)}
      style={{ visibility: active ? "visible" : "hidden", ...style }}
    >
      <div className="rect rect1"/>
      <div className="rect rect2"/>
      <div className="rect rect3"/>
      <div className="rect rect4"/>
      <div className="rect rect5"/>
    </div>
  );
};

export default Loading;
