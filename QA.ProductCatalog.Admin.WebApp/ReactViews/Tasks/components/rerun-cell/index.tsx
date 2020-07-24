import React from "react";
import { Icon, Intent } from "@blueprintjs/core";
import "./style.scss";

export const RerunCell = ({ id, method }) => {
  return (
    <div className="rerun-container">
      <Icon
        className="rerun-container__icon"
        intent={Intent.PRIMARY}
        onClick={() => method(id)}
        icon="refresh"
      />
    </div>
  );
};
