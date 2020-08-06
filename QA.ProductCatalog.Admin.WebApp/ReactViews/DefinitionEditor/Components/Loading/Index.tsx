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
      className={cn("sk-circle", className)}
      style={{ visibility: active ? "visible" : "hidden", ...style }}
    >
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
      <div className="sk-circle-dot" />
    </div>
  );
};

export default Loading;
