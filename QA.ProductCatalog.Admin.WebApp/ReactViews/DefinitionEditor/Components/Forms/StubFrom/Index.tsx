import React from "react";
import { observer } from "mobx-react-lite";
import { useStores } from "DefinitionEditor";
import { Form } from "react-final-form";
import FormField from "../FormField";
import FormFieldWrapper from "../FormFieldWrap";
import "./Style.scss";

const StubFrom = observer(() => {
  const { formStore, controlsStore } = useStores();

  return (
    <div className="forms-wrapper">
      {formStore.UIEditModel && (
        <Form
          onSubmit={formObj => console.log(formObj)}
          render={({ handleSubmit }) => {
            controlsStore.submitFormSyntheticEvent = handleSubmit;
            return (
              <form onSubmit={handleSubmit}>
                {formStore.UIEditModel.map(fieldModel => (
                  <FormFieldWrapper model={fieldModel} key={fieldModel.label}>
                    <FormField />
                  </FormFieldWrapper>
                ))}
              </form>
            );
          }}
        />
      )}
    </div>
  );
});

export default StubFrom;
