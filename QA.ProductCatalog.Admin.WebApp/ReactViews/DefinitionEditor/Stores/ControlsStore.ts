import { action, observable, reaction } from "mobx";
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
    formStore.init(action =>
      reaction(
        () => this.selectedNodeId,
        (nodeId: string) => {
          if (nodeId && action) action(nodeId);
        }
      )
    );
  }
  @observable formMode: boolean = false;
  @observable savingMode: SavingMode = SavingMode.Apply;
  @observable selectedNodeId: string = null;
  submitFormSyntheticEvent;

  @action
  toggleFormMode = () => (this.formMode = !this.formMode);

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

  isSameDefinition = (): boolean => {
    if (this.xmlEditorStore.xml === this.xmlEditorStore.origXml) {
      this.treeStore.setError(l("SameDefinition"));
      return true;
    }
    return false;
  };

  applyOnOpenedForm = async (): Promise<void> => {
    //TODO добавить проверку на неизмененные данные формы
    console.log(this.formStore.isEqualFormDataWithOriginalModel());

    await this.formStore.saveForm(this.selectedNodeId);
    if (this.isSameDefinition()) {
      return;
    }
    const singleNode = await this.treeStore.getSingleNode(this.selectedNodeId);
    await this.treeStore.setSingleNode(singleNode);
  };

  applyOnOpenedXmlEditor = async (): Promise<void> => {
    if (this.isSameDefinition()) {
      return;
    }
    await this.treeStore.getDefinitionLevel();
  };

  @action
  apply = async () => {
    this.setSavingMode(SavingMode.Apply);

    if (this.formMode) {
      await this.applyOnOpenedForm();
    } else {
      await this.applyOnOpenedXmlEditor();
    }

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
