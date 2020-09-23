import React from "react";
import { observer } from "mobx-react-lite";
import { useStores } from "DefinitionEditor";
import { Form } from "react-final-form";
import FormField from "../FormField";
import FormFieldWrapper from "../FormFieldWrap";
import "./Style.scss";
import FormErrorDialog from "DefinitionEditor/Components/Forms/FormErrorDialog";
import FormWarningDialog from "DefinitionEditor/Components/Forms/FormWarningDialog";
import { OperationState } from "Shared/Enums";
import { keys } from "lodash";

const EditForm = observer(() => {
  const { formStore, controlsStore } = useStores();

  return (
    <div className="forms-wrapper">
      {formStore.UIEditModel && formStore.operationState === OperationState.Success && (
        <Form
          onSubmit={formStore.setFormData}
          destroyOnUnregister
          render={({ handleSubmit, form }) => {
            controlsStore.submitFormSyntheticEvent = handleSubmit;
            return (
              <form onSubmit={handleSubmit}>
                {keys(formStore.UIEditModel).map(field => {
                  const model = formStore.UIEditModel[field];
                  return (
                    <FormFieldWrapper model={model} key={model?.name}>
                      <FormField />
                    </FormFieldWrapper>
                  );
                })}
              </form>
            );
          }}
        />
      )}
      {formStore.operationState === OperationState.Pending && "loading"}
      <FormErrorDialog />
      <FormWarningDialog />
    </div>
  );
});

export default EditForm;
