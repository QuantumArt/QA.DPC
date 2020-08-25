import React from "react";
import XmlEditorStore from "./XmlEditorStore";
import TreeStore from "./TreeStore";

export interface StoresCtx {
  xmlEditorStore: XmlEditorStore;
  treeStore: TreeStore;
}

const xmlEditorStore = new XmlEditorStore(window.definitionEditor);
const treeStore = new TreeStore(xmlEditorStore);

export const storesCtx = React.createContext<StoresCtx>({
  xmlEditorStore,
  treeStore
});

export const useStores = () => React.useContext(storesCtx);
