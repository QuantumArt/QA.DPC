import React from "react";
import { IconNames } from "@blueprintjs/icons";
import { Button, Classes, Dialog, Intent } from "@blueprintjs/core";
import { l } from "Tasks/Localization";

interface Props {
  isOpen: boolean;
  declineAction: () => void;
  confirmAction: () => void;
}

export const ConfirmationDialog = React.memo(({ isOpen, declineAction, confirmAction }: Props) => {
  return (
    <Dialog
      icon={IconNames.WARNING_SIGN}
      isOpen={isOpen}
      canOutsideClickClose={false}
      canEscapeKeyClose={false}
    >
      <div className={Classes.DIALOG_BODY}>
        <p>{l("prompt")}</p>
      </div>
      <div className={Classes.DIALOG_FOOTER}>
        <div className={Classes.DIALOG_FOOTER_ACTIONS}>
          <Button intent={Intent.DANGER} icon={IconNames.REMOVE} onClick={confirmAction}>
            {l("yes")}
          </Button>
          <Button intent={Intent.PRIMARY} icon={IconNames.REMOVE} onClick={declineAction}>
            {l("no")}
          </Button>
        </div>
      </div>
    </Dialog>
  );
});
