import React from "react";
import { observer, useLocalStore } from "mobx-react-lite";
import { OperationState } from "Shared/Enums";
import { Alert, Button, Collapse, Intent, Pre } from "@blueprintjs/core";
import { SavingMode } from "DefinitionEditor/Enums";
import { useStores } from "DefinitionEditor";
import { l } from "DefinitionEditor/Localization";
import "./Style.scss";

interface Props {

}

const TreeErrorDialog = observer<Props>(() => {
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
      confirmButtonText={controlsStore.savingMode === SavingMode.Apply ? l("Close") : l("ExitAnyway")}
      onConfirm={
        controlsStore.savingMode === SavingMode.Apply ? treeStore.resetErrorState : controlsStore.exit
      }
      cancelButtonText={controlsStore.savingMode === SavingMode.Apply ? null : l("BackToEditing")}
      onCancel={treeStore.resetErrorState}
      className="error-dialog"
    >
      {treeStore.errorText}
      {treeStore.errorLog && (
        <div className="error-dialog__error-log">
          <Button onClick={state.toggle}>{state.opened ? l("HideLog") : l("ShowLog")}</Button>
          <Collapse isOpen={state.opened}>
            <Pre className="error-dialog__pre">{treeStore.errorLog}</Pre>
          </Collapse>
        </div>
      )}
    </Alert>
  );
});

export default TreeErrorDialog;
