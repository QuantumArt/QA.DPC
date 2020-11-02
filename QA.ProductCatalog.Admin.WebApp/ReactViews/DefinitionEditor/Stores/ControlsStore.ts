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
    formStore.init(onReactionFireCb =>
      reaction(
        () => this.selectedNodeId,
        (nodeId: string) => {
          if (nodeId && onReactionFireCb) onReactionFireCb(nodeId);
        }
      )
    );
  }
  @observable formMode: boolean = false;
  @observable savingMode: SavingMode = SavingMode.Apply;
  @observable selectedNodeId: string = null;
  submitFormSyntheticEvent;

  @observable isUnsavedChangesDialog: boolean = false;
  @observable unsavedChangesDialogOnLeaveCb: () => void;

  @action
  toggleUnsavedChangesDialog = () => (this.isUnsavedChangesDialog = !this.isUnsavedChangesDialog);

  @action
  onChangeFormMode = () => {
    if (this.formMode && !this.formStore.isEqualFormDataWithOriginalModel()) {
      this.toggleUnsavedChangesDialog();
      this.unsavedChangesDialogOnLeaveCb = () => this.toggleFormMode();
      return;
    } else {
      if (!this.isSameDefinition(false)) {
        this.toggleUnsavedChangesDialog();
        this.unsavedChangesDialogOnLeaveCb = () => {
          this.xmlEditorStore.setXml(this.xmlEditorStore.lastLocalSavedXml);
          this.toggleFormMode();
        };
        return;
      }
    }
    this.toggleFormMode();
  };

  @action
  toggleFormMode = () => (this.formMode = !this.formMode);

  @action
  setSelectedNodeId = (id: string) => {
    /**
     * При клике на ноду проверяется были ли изменения в модели формы.
     * */
    if (this.formMode && !this.formStore.isFormTheSame()) {
      this.toggleUnsavedChangesDialog();
      this.unsavedChangesDialogOnLeaveCb = () => this.setNodeId(id);
      return;
    }
    this.setNodeId(id);
  };

  @action
  setNodeId = (id: string) => {
    this.selectedNodeId = id;
  };

  @action
  setSavingMode = (mode: SavingMode) => (this.savingMode = mode);

  @action
  refresh = async () => {
    this.xmlEditorStore.setXml(this.xmlEditorStore.origXml);
    this.treeStore.resetErrorState();
    this.formStore.UIEditModel = undefined;
    this.selectedNodeId = null;
    this.treeStore.openedNodes = [];
    await this.treeStore.withLogError(async () => await this.treeStore.getDefinitionLevel());
    await this.treeStore.onNodeExpand(this.treeStore.tree[0]);
  };

  isSameDefinition = (originalMode: boolean = true): boolean => {
    return (
      (this.xmlEditorStore.isSameDefinition() && originalMode) ||
      (this.xmlEditorStore.isSameDefinitionWithLastSaved() && !originalMode)
    );
  };

  applyOnOpenedForm = async (): Promise<boolean> => {
    if (this.formStore.isFormTheSame()) {
      return false;
    }
    await this.formStore.saveForm(this.selectedNodeId);
    await this.treeStore.withLogError(async () => await this.treeStore.getDefinitionLevel());
    return true;
  };

  applyOnOpenedXmlEditor = async (): Promise<boolean> => {
    if (this.isSameDefinition(this.savingMode === SavingMode.Finish)) {
      this.treeStore.setError(l("SameDefinition"));
      return false;
    }
    await this.treeStore.withLogError(async () => await this.treeStore.getDefinitionLevel());
    return true;
  };

  apply = async () => {
    this.setSavingMode(SavingMode.Apply);
    const isSaveSuccess = await this.doLocalSave();
    if (!isSaveSuccess) return;
    for (const nodeId of this.treeStore.openedNodes) {
      await this.treeStore.onNodeExpand(this.treeStore.nodesMap.get(nodeId));
    }
    this.treeStore.setSelectedNodeIdInUI();
  };

  updateFormWithNewData = async () => {
    if (this.selectedNodeId) await this.formStore.fetchFormFields(this.selectedNodeId);
  };

  doLocalSave = async (): Promise<boolean> => {
    let isLocalSaveWasCorrect: boolean;
    if (this.formMode) {
      isLocalSaveWasCorrect = await this.applyOnOpenedForm();
    } else {
      isLocalSaveWasCorrect = await this.applyOnOpenedXmlEditor();
      await this.updateFormWithNewData();
    }
    return isLocalSaveWasCorrect;
  };

  saveAndExit = async () => {
    this.setSavingMode(SavingMode.Finish);
    const isSaveSuccess = await this.doLocalSave();
    if (!isSaveSuccess) return;
    if (
      (this.treeStore.operationState === OperationState.Success && !this.formMode) ||
      (this.formMode && this.formStore.operationState === OperationState.Success)
    ) {
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
