import React from "react";
import { IGeneralSettings } from "Notification/ApiServices/ApiInterfaces";
import "./Style.scss";
import { RowLabel } from "Notification/Components";
import { Checkbox, InputGroup } from "@blueprintjs/core";

interface IProps {
  settings: IGeneralSettings;
}

export const ChannelsGeneralSettings = React.memo(({ settings }: IProps) => {
  return (
    <>
      <RowLabel label={"Autopublish"}>
        <Checkbox checked={settings?.Autopublish || false}>
          <strong>Автоматическая</strong>
        </Checkbox>
      </RowLabel>

      <RowLabel label={"Send interval"}>
        <InputGroup disabled={true} value={String(settings?.CheckInterval) || ""} />
      </RowLabel>

      <RowLabel label={"Send interval while errors are occuring"}>
        <InputGroup disabled={true} value={String(settings?.WaitIntervalAfterErrors) || ""} />
      </RowLabel>

      <RowLabel label={"Number of errors"}>
        <InputGroup disabled={true} value={String(settings?.ErrorCountBeforeWait) || ""} />
      </RowLabel>

      <RowLabel label={"Package size"}>
        <InputGroup disabled={true} value={String(settings?.PackageSize) || ""} />
      </RowLabel>

      <RowLabel label={"Notification timeout"}>
        <InputGroup disabled={true} value={String(settings?.TimeOut) || ""} />
      </RowLabel>
    </>
  );
});
