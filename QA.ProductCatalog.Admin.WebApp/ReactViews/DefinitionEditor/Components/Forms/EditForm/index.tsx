import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { useStores } from "DefinitionEditor";
import cn from "classnames";
import { Form } from "react-final-form";
import FormField from "../FormField";
import FormFieldWrapper from "../FormFieldWrap";
import "./Style.scss";
import { OperationState } from "Shared/Enums";
import { keys } from "lodash";
import { CheckboxParsedModel } from "Shared/Utils";
import { Intent, Spinner } from "@blueprintjs/core";

interface Props {
  width: string;
}

const EditForm = observer(({ width }: Props) => {
  const { formStore, controlsStore } = useStores();

  useEffect(() => {
    if (formStore.UIEditModelPresent && formStore.UIEditModel["InDefinition"])
      formStore.hideUiFields(
        ["InDefinition"],
        false,
        !Boolean(formStore.UIEditModel["InDefinition"].value)
      );
  }, [formStore.UIEditModelPresent && formStore.UIEditModel["InDefinition"]]);

  useEffect(() => {
    if (formStore.UIEditModelPresent && formStore.UIEditModel["CacheEnabled"]) {
      const model = formStore.UIEditModel["CacheEnabled"] as CheckboxParsedModel;
      model.subModel.toggleIsHide(!model.value);
    }
  }, [formStore.UIEditModelPresent && formStore.UIEditModel["CacheEnabled"]]);

  if (!formStore.UIEditModelPresent) {
    return null;
  }

  return (
    <div
      className={cn("forms-wrapper", {
        "forms-wrapper--loading": formStore.operationState === OperationState.Pending
      })}
      style={{ width }}
    >
      {formStore.UIEditModel &&
        (formStore.operationState === OperationState.Success ||
          formStore.operationState === OperationState.Error) && (
          <Form
            onSubmit={formStore.setFormData}
            destroyOnUnregister
            render={({ handleSubmit }) => {
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
      {formStore.operationState === OperationState.Pending && (
        <Spinner intent={Intent.PRIMARY} size={Spinner.SIZE_LARGE} />
      )}
    </div>
  );
});

export default EditForm;
