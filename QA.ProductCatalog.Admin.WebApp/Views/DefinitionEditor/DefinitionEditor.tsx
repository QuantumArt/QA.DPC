import React from "react";
import { render } from "react-dom";
import { Root, DefinitionEditorStore } from "DefinitionEditor";

const store = new DefinitionEditorStore(window.definitionEditor);
render(<Root store={store} />, document.getElementById("definitionEditor"));
