import React from "react";
import { observer } from "mobx-react-lite";
import { Button, FormGroup, InputGroup, Intent, Switch, TextArea } from "@blueprintjs/core";
import { useStores } from "DefinitionEditor";
import { Form, Field } from "react-final-form";
import "./Style.scss";
import { FormFieldType } from "DefinitionEditor/Enums";

interface Props {
  nodeId: string;
}

const StubFrom = observer<Props>(({ nodeId }) => {
  const { formStore } = useStores();
  console.log(window.definitionEditor);
  formStore.setNodeId(nodeId);

  const renderFieldDependsOnType = model => {
    console.log(model);
    switch (model.type) {
      case FormFieldType.Text:
        return (
          <div className="field">
            <label className="label">{model.label}</label>
            <InputGroup disabled={true} value={model.value} className="input" />
          </div>
        );
      case FormFieldType.Input:
        return (
          <div className="field">
            <label className="label">{model.label}</label>
            <Field name={model.label} defaultValue={model.value}>
              {({ input }) => {
                console.log(input);
                return (
                  <>
                    <InputGroup
                      {...input}
                      className="input"
                      placeholder={model.placeholder || ""}
                    />
                  </>
                );
              }}
            </Field>
          </div>
        );
      case FormFieldType.Textarea:
        return (
          <div className="field">
            <label className="label">{model.label}</label>
            <Field name={model.label} initialValue={model.value}>
              {({ input }) => {
                return <TextArea {...input} {...model.extraOptions} className="input" />;
              }}
            </Field>
          </div>
        );
      case FormFieldType.Checkbox:
        return (
          <div className="field">
            <label className="label">{model.label}</label>
            <Field name={model.label} initialValue={model.value}>
              {({ input }) => {
                return (
                  <div className="input">
                    <Switch label={model.subString || ""} checked={input.value} inline={true} />
                  </div>
                );
              }}
            </Field>
          </div>
        );

      default:
        return "";
    }
  };

  return (
    <form>
      <FormGroup inline label="Some field">
        <InputGroup placeholder={nodeId} fill />
      </FormGroup>

      <Form
        onSubmit={formObj => console.log(formObj)}
        render={({ handleSubmit }) => (
          <form onSubmit={handleSubmit}>
            {formStore.UIEditModel &&
              formStore.UIEditModel.map(x => {
                return x && renderFieldDependsOnType(x);
              })}
            <Button intent={Intent.PRIMARY} type="submit">
              Apply
            </Button>
          </form>
        )}
      />
    </form>
  );
});

export default StubFrom;
