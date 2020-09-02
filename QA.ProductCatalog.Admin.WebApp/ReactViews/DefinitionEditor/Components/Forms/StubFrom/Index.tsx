import React from "react";
import { observer } from "mobx-react-lite";
import { useStores } from "DefinitionEditor";
import { Form } from "react-final-form";
import FormField from "../FormField";
import "./Style.scss";

interface Props {
  nodeId: string;
}

const StubFrom = observer<Props>(({ nodeId }) => {
  const { formStore, treeStore } = useStores();
  formStore.setNodeId(nodeId);

  return (
    <div className="forms-wrapper">
      {formStore.UIEditModel && (
        <Form
          onSubmit={formObj => console.log(formObj)}
          render={({ handleSubmit }) => {
            treeStore.submitFormSyntheticEvent = handleSubmit;
            return (
              <form onSubmit={handleSubmit}>
                {formStore.UIEditModel.map(fieldModel => (
                  <FormField model={fieldModel} />
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
