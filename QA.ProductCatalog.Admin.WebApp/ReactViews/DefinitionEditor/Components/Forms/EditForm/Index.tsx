import React from "react";
import { observer } from "mobx-react-lite";
import { useStores } from "DefinitionEditor";
import { Form } from "react-final-form";
import FormField from "../FormField";
import FormFieldWrapper from "../FormFieldWrap";
import "./Style.scss";
import FormErrorDialog from "DefinitionEditor/Components/Forms/FormErrorDialog";
import FormWarningDialog from "DefinitionEditor/Components/Forms/FormWarningDialog";

const EditForm = observer(() => {
  const { formStore, controlsStore } = useStores();

  return (
    <div className="forms-wrapper">
      {formStore.UIEditModel && (
        <Form
          onSubmit={formStore.setFormData}
          destroyOnUnregister
          render={({ handleSubmit, form }) => {
            formStore.resetForm = form.reset;
            controlsStore.submitFormSyntheticEvent = handleSubmit;
            return (
              <form onSubmit={handleSubmit}>
                {formStore.UIEditModel.map(fieldModel => (
                  <FormFieldWrapper model={fieldModel} key={fieldModel.name}>
                    <FormField />
                  </FormFieldWrapper>
                ))}
              </form>
            );
          }}
        />
      )}
      <FormErrorDialog />
      <FormWarningDialog />
    </div>
  );
});

export default EditForm;
