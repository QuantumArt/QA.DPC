import React from "react";
import { DefinitionEditorStore } from "./DefinitionEditorStore";
import { TreeStore } from "./TreeStore";

const storesCtx = React.createContext({
  editorStore: new DefinitionEditorStore(window.definitionEditor),
  treeStore: new TreeStore()
});

export const useStores = () => React.useContext(storesCtx);
