import React from "react";
import { observer } from "mobx-react-lite";
import { Alert, Intent } from "@blueprintjs/core";
import { useStores } from "DefinitionEditor";
import { l } from "DefinitionEditor/Localization";
import "./Style.scss";

const UnsavedChangesDialog = observer(() => {
  const { controlsStore } = useStores();
  const stay = controlsStore.toggleUnsavedChangesDialog;
  const leave = () => {
    controlsStore.unsavedChangesDialogOnLeaveCb();
    controlsStore.toggleUnsavedChangesDialog();
  };
  return (
    <Alert
      isOpen={controlsStore.isUnsavedChangesDialog}
      intent={Intent.WARNING}
      icon="walk"
      cancelButtonText={l("BackToEditing")}
      confirmButtonText="Exit anyway"
      onCancel={stay}
      onConfirm={leave}
      className="error-dialog"
    >
      Your changes will not save. Are you sure that you want to leave?
    </Alert>
  );
});

export default UnsavedChangesDialog;
