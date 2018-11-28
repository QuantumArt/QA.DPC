import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { MultiRelationFieldTags } from "Components/FieldEditors/FieldEditors";
import { EditorTabs } from "./Components/EditorTabs";
import { Product } from "./TypeScriptSchema";
import { AdvantagesTable } from "./Components/AdvantagesTable";
import { ParameterFields } from "./Components/ParameterFields";
import "./FixConnectTariff.scss";

const App = () => (
  <ProductEditor
    editorSettings={window["ProductEditorSettings"]}
    relationEditors={{
      Region: props => <MultiRelationFieldTags {...props} sortItemsBy="Title" />,
      ProductParameter: ParameterFields,
      LinkParameter: ParameterFields,
      Advantage: AdvantagesTable
    }}
  >
    {(model: Product, contentSchema) => <EditorTabs model={model} contentSchema={contentSchema} />}
  </ProductEditor>
);

ReactDOM.render(<App />, document.getElementById("editor"));
