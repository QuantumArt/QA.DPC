import React from "react";
import { observer } from "mobx-react-lite";
import { Alert, Intent } from "@blueprintjs/core";
import { useStores } from "DefinitionEditor";
import { l } from "DefinitionEditor/Localization";
import "./Style.scss";

const FormWarningDialog = observer(() => {
  const { formStore } = useStores();

  return (
    <Alert
      isOpen={formStore.isLeaveWithoutSaveDialog}
      intent={Intent.WARNING}
      icon="walk"
      cancelButtonText={l("BackToEditing")}
      confirmButtonText="Exit anyway"
      onCancel={formStore.toggleLeaveWithoutSaveDialog}
      onConfirm={() => {
        formStore.warningPopupOnExitCb();
        formStore.toggleLeaveWithoutSaveDialog();
      }}
      className="error-dialog"
    >
      Your changes will not save. Are you sure that you want to leave to another form?
    </Alert>
  );
});

export default FormWarningDialog;
