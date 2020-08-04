import React from "react";
import { Icon } from "@blueprintjs/core";
import "./Style.scss";

export const RerunGridCell = ({ id, method }) => {
  return (
    <div className="rerun-container">
      <Icon
        className="rerun-container__icon"
        intent="primary"
        onClick={() => method(id)}
        icon="refresh"
      />
    </div>
  );
};
