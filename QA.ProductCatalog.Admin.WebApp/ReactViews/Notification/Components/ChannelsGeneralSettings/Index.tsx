import React from "react";
import { IGeneralSettings } from "Notification/ApiServices/ApiInterfaces";
import "./Style.scss";
import { RowLabel, StaticTextField } from "Notification/Components";
import { Checkbox } from "@blueprintjs/core";
import { l } from "Notification/Localization";

interface IProps {
  settings: IGeneralSettings;
}

export const ChannelsGeneralSettings = React.memo(({ settings }: IProps) => {
  return (
    <>
      <RowLabel label={l("autoPublication")}>
        <Checkbox disabled checked={settings?.Autopublish || false}>
          <strong>Автоматическая</strong>
        </Checkbox>
      </RowLabel>

      <RowLabel label={l("sendInterval")}>
        <StaticTextField text={String(settings?.CheckInterval) || ""} subtext={l("seconds")} />
      </RowLabel>

      <RowLabel label={l("errorSendInterval")}>
        <StaticTextField
          text={String(settings?.WaitIntervalAfterErrors) || ""}
          subtext={l("seconds")}
        />
      </RowLabel>

      <RowLabel label={l("errNumBeforeWait")}>
        <StaticTextField text={String(settings?.ErrorCountBeforeWait) || ""} />
      </RowLabel>

      <RowLabel label={l("packageSize")}>
        <StaticTextField text={String(settings?.PackageSize) || ""} />
      </RowLabel>

      <RowLabel label={l("notifyTimeout")}>
        <StaticTextField text={String(settings?.TimeOut) || ""} subtext={l("seconds")} />
      </RowLabel>
    </>
  );
});
