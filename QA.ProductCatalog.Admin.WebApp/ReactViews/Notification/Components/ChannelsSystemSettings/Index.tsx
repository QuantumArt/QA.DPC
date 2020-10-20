import React from "react";
import { ISystemSettings } from "Notification/ApiServices/ApiInterfaces";
import "./Style.scss";
import { RowLabel } from "Notification/Components";
import { InputGroup } from "@blueprintjs/core";
import { format, isValid } from "date-fns";

interface IProps {
  settings: ISystemSettings;
}

export const ChannelsSystemSettings = React.memo(({ settings }: IProps) => {
  const date = new Date(settings?.Started);

  return (
    <>
      <RowLabel label={"Channel provider"}>
        <InputGroup disabled={true} value={settings?.NotificationProvider || ""} />
      </RowLabel>
      <RowLabel label={"Service start time"}>
        <InputGroup
          disabled={true}
          value={(isValid(date) && format(date, "DD.MM.YYYY HH:mm:ss")) || ""}
        />
      </RowLabel>
    </>
  );
});
