import { action, observable } from "mobx";
import XmlEditorStore from "./XmlEditorStore";
import TreeStore from "./TreeStore";
import FormStore from "DefinitionEditor/Stores/FormStore";
import { SavingMode } from "DefinitionEditor/Enums";
import { OperationState } from "Shared/Enums";
import { l } from "DefinitionEditor/Localization";

export default class ControlsStore {
  constructor(
    private xmlEditorStore: XmlEditorStore,
    private treeStore: TreeStore,
    private formStore: FormStore
  ) {
    treeStore.init(this.setSelectedNodeId);
  }

  @observable savingMode: SavingMode = SavingMode.Apply;
  @observable selectedNodeId: string = null;

  @action
  setSelectedNodeId = (id: string) => {
    this.selectedNodeId = id;
  };

  @action
  setSavingMode = (mode: SavingMode) => (this.savingMode = mode);

  @action
  refresh = async () => {
    this.xmlEditorStore.setXml(this.xmlEditorStore.origXml);
    this.treeStore.resetErrorState();
    this.treeStore.openedNodes = [];
    await this.treeStore.getDefinitionLevel();
    await this.treeStore.onNodeExpand(this.treeStore.tree[0]);
  };

  @action
  apply = async () => {
    this.setSavingMode(SavingMode.Apply);
    if (this.xmlEditorStore.xml === this.xmlEditorStore.origXml) {
      this.treeStore.setError(l("SameDefinition"));
      return;
    }
    await this.treeStore.getDefinitionLevel();
    for (const nodeId of this.treeStore.openedNodes) {
      await this.treeStore.onNodeExpand(this.treeStore.nodesMap.get(nodeId));
    }
  };

  @action
  saveAndExit = async () => {
    this.setSavingMode(SavingMode.Finish);
    if (this.xmlEditorStore.xml === this.xmlEditorStore.origXml) {
      this.treeStore.setError(l("SameDefinition"));
      return;
    }
    await this.treeStore.getDefinitionLevel();
    if (this.treeStore.operationState === OperationState.Success) {
      window.pmrpc.call({
        destination: parent,
        publicProcedureName: "SaveXmlToDefinitionField",
        params: [this.xmlEditorStore.xml]
      });
    }
  };

  exit = () => {
    window.pmrpc.call({
      destination: parent,
      publicProcedureName: "CloseEditor"
    });
  };
}
