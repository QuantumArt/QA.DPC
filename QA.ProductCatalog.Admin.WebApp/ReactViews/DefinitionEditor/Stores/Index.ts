import React from "react";
import XmlEditorStore from "./XmlEditorStore";
import TreeStore from "./TreeStore";
import FormStore from "./FormStore";

export interface StoresCtx {
  xmlEditorStore: XmlEditorStore;
  treeStore: TreeStore;
  formStore: FormStore;
}

const xmlEditorStore = new XmlEditorStore(window.definitionEditor);
const treeStore = new TreeStore(xmlEditorStore);
const formStore = new FormStore(window.definitionEditor, xmlEditorStore);

export const storesCtx = React.createContext<StoresCtx>({
  xmlEditorStore,
  treeStore,
  formStore
});

export const useStores = () => React.useContext(storesCtx);
