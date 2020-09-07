import { observable } from "mobx";
import XmlEditorStore from "./XmlEditorStore";
import TreeStore from "./TreeStore";


export default class ControlsStore {
  // constructor(private xmlEditorStore: XmlEditorStore, private treeStore: TreeStore) {
  //
  // }

  @observable selectedNodeId: string = null;
}
