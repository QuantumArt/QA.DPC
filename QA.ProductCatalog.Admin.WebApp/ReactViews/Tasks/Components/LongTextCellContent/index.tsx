import React, { useState } from "react";
import cn from "classnames";
import { observer } from "mobx-react-lite";
import { Button, Classes, Dialog, Intent } from "@blueprintjs/core";
import { l } from "Tasks/Localization";

interface Props {
  value: string;
  isLoading: boolean;
}

export const LongTextCellContent = observer<Props>(({ value, isLoading }) => {
  const [opened, toggleOpened] = useState(false);
  return (
    <div className={cn({ "bp3-skeleton": isLoading })}>
      <Dialog isOpen={opened} onClose={() => toggleOpened(false)} title={l("message")}>
        <div className={Classes.DIALOG_BODY}>
          <p>{value}</p>
        </div>
      </Dialog>
      <Button intent={Intent.PRIMARY} minimal onClick={() => toggleOpened(true)}>
        {`${value.substr(0, 10)}...`}
      </Button>
    </div>
  );
});
