import { Spinner, Intent } from "@blueprintjs/core";
import React from "react";
import "./Style.scss";

export const ChannelsLoader = ({ isActive }: { isActive: boolean }) => {
  return (
    isActive && (
      <div className="channels-loader">
        <Spinner intent={Intent.PRIMARY} size={Spinner.SIZE_STANDARD} />
      </div>
    )
  );
};
