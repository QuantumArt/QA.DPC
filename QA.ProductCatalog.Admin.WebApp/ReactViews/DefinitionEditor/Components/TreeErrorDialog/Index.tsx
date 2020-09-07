import React from "react";
import { observer, useLocalStore } from "mobx-react-lite";
import { OperationState } from "Shared/Enums";
import { Alert, Button, Collapse, Intent, Pre } from "@blueprintjs/core";
import { SavingMode } from "DefinitionEditor/Enums";
import { useStores } from "DefinitionEditor";
import "./Style.scss";

interface Props {

}

const ErrorDialog = observer<Props>(() => {
  const { treeStore, controlsStore } = useStores();
  const state = useLocalStore(() => ({
    opened: false,
    toggle() {
      this.opened = !this.opened;
    }
  }));
  return (
    <Alert
      isOpen={treeStore.operationState === OperationState.Error}
      intent={Intent.DANGER}
      icon="warning-sign"
      confirmButtonText={controlsStore.savingMode === SavingMode.Apply ? "Close" : "Exit anyway"}
      onConfirm={
        controlsStore.savingMode === SavingMode.Apply ? treeStore.resetErrorState : controlsStore.exit
      }
      cancelButtonText={controlsStore.savingMode === SavingMode.Apply ? null : "Back to editing"}
      onCancel={treeStore.resetErrorState}
      className="error-dialog"
    >
      {treeStore.errorText}
      {treeStore.errorLog && (
        <div className="error-dialog__error-log">
          <Button onClick={state.toggle}>{state.opened ? "Hide log" : "Show log"}</Button>
          <Collapse isOpen={state.opened}>
            <Pre className="error-dialog__pre">{treeStore.errorLog}</Pre>
          </Collapse>
        </div>
      )}
    </Alert>
  );
});

export default ErrorDialog;
