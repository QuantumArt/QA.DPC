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
  const { treeStore } = useStores();
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
      confirmButtonText={treeStore.savingMode === SavingMode.Apply ? "Close" : "Exit anyway"}
      onConfirm={
        treeStore.savingMode === SavingMode.Apply ? treeStore.resetState : treeStore.exit
      }
      cancelButtonText={treeStore.savingMode === SavingMode.Apply ? null : "Back to editing"}
      onCancel={treeStore.resetState}
      className="error-dialog"
    >
      {treeStore.errorText}
      {treeStore.errorLog && (
        <div className="error-dialog__error-log">
          <Button onClick={state.toggle}>Show log</Button>
          <Collapse isOpen={state.opened}>
            <Pre className="error-dialog__pre">{treeStore.errorLog}</Pre>
          </Collapse>
        </div>
      )}
    </Alert>
  );
});

export default ErrorDialog;
