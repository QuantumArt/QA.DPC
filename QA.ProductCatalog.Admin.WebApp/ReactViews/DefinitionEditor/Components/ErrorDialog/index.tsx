import React from "react";
import { observer, useLocalStore } from "mobx-react-lite";
import { OperationState } from "Shared/Enums";
import { Alert, Button, Collapse, Intent, Pre } from "@blueprintjs/core";
import { SavingMode } from "DefinitionEditor/Enums";
import { useStores } from "DefinitionEditor";
import { ErrorHandler } from "DefinitionEditor/Stores";
import { l } from "DefinitionEditor/Localization";
import "./Style.scss";

interface Props {
  store: ErrorHandler;
}

const ErrorDialog = observer<Props>(({ store }) => {
  const { controlsStore } = useStores();
  const state = useLocalStore(() => ({
    opened: false,
    toggle() {
      this.opened = !this.opened;
    }
  }));

  return (
    <Alert
      isOpen={store.operationState === OperationState.Error}
      intent={Intent.DANGER}
      icon="warning-sign"
      confirmButtonText={
        controlsStore.savingMode === SavingMode.Apply ? l("Close") : l("ExitAnyway")
      }
      onConfirm={
        controlsStore.savingMode === SavingMode.Apply ? store.resetErrorState : controlsStore.exit
      }
      cancelButtonText={controlsStore.savingMode === SavingMode.Apply ? null : l("BackToEditing")}
      onCancel={store.resetErrorState}
      className="error-dialog"
      onClose={() => {
        if (state.opened) {
          state.toggle();
        }
      }}
    >
      {store.errorText}
      {store.errorLog && (
        <div className="error-dialog__error-log">
          <Button onClick={state.toggle}>{state.opened ? l("HideLog") : l("ShowLog")}</Button>
          <Collapse isOpen={state.opened}>
            <Pre className="error-dialog__pre">{store.errorLog}</Pre>
          </Collapse>
        </div>
      )}
    </Alert>
  );
});

export default ErrorDialog;
