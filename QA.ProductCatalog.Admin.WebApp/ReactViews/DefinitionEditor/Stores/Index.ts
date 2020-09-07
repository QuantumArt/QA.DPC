import React from "react";
import XmlEditorStore from "./XmlEditorStore";
import TreeStore from "./TreeStore";
import FormStore from "./FormStore";
import ControlsStore from "./ControlsStore";

export interface StoresCtx {
  controlsStore: ControlsStore;
  xmlEditorStore: XmlEditorStore;
  treeStore: TreeStore;
  formStore: FormStore;
}

const controlsStore = new ControlsStore();
const xmlEditorStore = new XmlEditorStore(controlsStore, window.definitionEditor);
const treeStore = new TreeStore(controlsStore, xmlEditorStore);
const formStore = new FormStore(window.definitionEditor, xmlEditorStore);

export const storesCtx = React.createContext<StoresCtx>({
  controlsStore,
  xmlEditorStore,
  treeStore,
  formStore
});

export const useStores = () => React.useContext(storesCtx);
