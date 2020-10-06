import React from "react";
import { observer } from "mobx-react-lite";
import { OperationState } from "Shared/Enums";
import { Alert, Intent } from "@blueprintjs/core";
import { SavingMode } from "DefinitionEditor/Enums";
import { useStores } from "DefinitionEditor";
import { l } from "DefinitionEditor/Localization";
import "./Style.scss";

const FormErrorDialog = observer(() => {
  const { formStore, controlsStore } = useStores();

  return (
    <Alert
      isOpen={formStore.operationState === OperationState.Error}
      intent={Intent.DANGER}
      icon="warning-sign"
      confirmButtonText={
        controlsStore.savingMode === SavingMode.Apply ? l("Close") : l("ExitAnyway")
      }
      onConfirm={
        controlsStore.savingMode === SavingMode.Apply
          ? formStore.resetErrorState
          : controlsStore.exit
      }
      cancelButtonText={controlsStore.savingMode === SavingMode.Apply ? null : l("BackToEditing")}
      onCancel={formStore.resetErrorState}
      className="error-dialog"
    >
      {formStore.errorText}
    </Alert>
  );
});

export default FormErrorDialog;
