import React from "react";
import { ISystemSettings } from "Notification/ApiServices/ApiInterfaces";
import "./Style.scss";
import { RowLabel, StaticTextField } from "Notification/Components";
import { format, isValid } from "date-fns";
import { l } from "Notification/Localization";

interface IProps {
  settings: ISystemSettings;
}

export const ChannelsSystemSettings = React.memo(({ settings }: IProps) => {
  const date = new Date(settings?.Started);
  const dd = isValid(date) ? format(date, "DD.MM.YYYY") : "";
  const mm = isValid(date) ? format(date, "HH:mm:ss") : "";

  return (
    <>
      <RowLabel label={l("channelProvider")}>
        <StaticTextField text={settings?.NotificationProvider || ""} />
      </RowLabel>
      <RowLabel label={l("startTime")}>
        <StaticTextField text={dd} subtext={mm} />
      </RowLabel>
    </>
  );
});
