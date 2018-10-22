import "Environment";
import React from "react";
import ReactDOM from "react-dom";
import { LocaleContext } from "react-lazy-i18n";
import { ProductEditor } from "Components/ProductEditor/ProductEditor";
import { MultiRelationFieldTags } from "Components/FieldEditors/FieldEditors";
import { EditorTabs } from "./Components/EditorTabs";
import { Product } from "./TypeScriptSchema";
import { AdvantagesTable } from "./Components/AdvantagesTable";
import { ParameterFields } from "./Components/ParameterFields";
import "./FixConnectTariff.scss";

const App = () => (
  <LocaleContext.Provider value="ru">
    <ProductEditor
      settings={window["ProductEditorSettings"]}
      relationEditors={{
        Region: props => <MultiRelationFieldTags {...props} sortItemsBy="Title" />,
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
