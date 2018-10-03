import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { command } from "Utils/Command";
import { LocaleContext } from "Packages/react-lazy-i18n";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { MultiRelationFieldTags } from "Components/FieldEditors/FieldEditors";
import { EditorTabs } from "./Components/EditorTabs";
import { Product } from "./ProductEditorSchema";
import { AdvantagesTable } from "./Components/AdvantagesTable";
import { ParameterFields } from "./Components/ParameterFields";
import "./FixConnectTariff.scss";

command.alertErrors = true;

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
      relationEditors={{
        Region: props => <MultiRelationFieldTags {...props} orderByField="Title" />,
        ProductParameter: props => <ParameterFields {...props} />,
        LinkParameter: props => <ParameterFields {...props} />,
        Advantage: AdvantagesTable
      }}
    >
      {(model: Product, contentSchema) => (
        <EditorTabs model={model} contentSchema={contentSchema} />
      )}
    </ProductEditor>
  </LocaleContext.Provider>
);

ReactDOM.render(<App />, document.getElementById("editor"));
