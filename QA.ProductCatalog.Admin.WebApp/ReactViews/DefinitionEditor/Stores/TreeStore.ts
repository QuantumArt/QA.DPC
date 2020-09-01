import React from "react";
import { IconName } from "@blueprintjs/core/lib/esm/components/icon/icon";
import { action, computed, observable, toJS, when } from "mobx";
import XmlEditorStore from "./XmlEditorStore";
import { OperationState } from "Shared/Enums";
import { SavingMode } from "DefinitionEditor/Enums";
import { ITreeNode } from "@blueprintjs/core";
import { IDefinitionNode } from "DefinitionEditor/ApiService/ApiInterfaces";
import ApiService from "DefinitionEditor/ApiService";

export default class TreeStore {
  constructor(private xmlEditorStore: XmlEditorStore) {
    when(
      () => this.xmlEditorStore.rootId != null,
      async () => {
        await this.getDefinitionLevel();
        await this.onNodeExpand(this.tree[0]);
      }
    );
  };

  @observable operationState: OperationState = OperationState.None;
  @observable savingMode: SavingMode = SavingMode.Apply;
  @observable errorText: string = null;
  @observable errorLog: string = null;
  @observable nodesMap: Map<ITreeNode["id"], ITreeNode<Partial<IDefinitionNode>>> = new Map();
  @observable openedNodes: ITreeNode["id"][] = [];
  @observable selectedNodeId: string = null;

  @computed get tree() {
    if (this.xmlEditorStore.rootId && this.nodesMap.has(`/${this.xmlEditorStore.rootId}`)) {
      return [this.nodesMap.get(`/${this.xmlEditorStore.rootId}`)]
    } else {
      return [];
    }
  };

  @action
  onNodeExpand = async (node: ITreeNode<Partial<IDefinitionNode>>) => {
    if (node.nodeData.hasChildren && node.childNodes.length === 0) {
      node.childNodes = await this.getDefinitionLevel(node.id.toString());
    }
    node.isExpanded = true;
    this.setOpenedNodes(node.id);
    for (const el of node.childNodes) {
      if (el.nodeData.expanded && el.nodeData.hasChildren) {
        await this.onNodeExpand(el);
      }
    }
  };

  @action
  onNodeCollapse = (node: ITreeNode) => {
    node.isExpanded = false;
    this.setOpenedNodes(node.id, true);
  };

  @action
  setOpenedNodes = (nodeId: ITreeNode["id"], remove: boolean = false) => {
    const node = this.nodesMap.get(nodeId);
    if (node.isExpanded && this.openedNodes.indexOf(nodeId) === -1) {
      this.openedNodes.push(node.id);
    } else if (remove) {
      this.openedNodes.splice(this.openedNodes.indexOf(nodeId), 1);
    }
  };

  @action
  onNodeClick = (node: ITreeNode, _nodePath: number[], e: React.MouseEvent<HTMLElement>) => {
    const originallySelected = node.isSelected;
    if (!e.shiftKey) {
      this.nodesMap.forEach(val => {
        val.isSelected = false;
      });
    }
    node.isSelected = originallySelected == null ? true : !originallySelected;
    this.selectedNodeId = node.isSelected ? node.id.toString() : null;
  };

  @action
  getDefinitionLevel = async (
    path?: string
  ): Promise<ITreeNode<Partial<IDefinitionNode>>[]> => {
    try {
      const formData = new FormData();
      if (path) {
        formData.append("path", path.charAt(0) === "/" ? path : `/${path}`);
      }
      if (!this.xmlEditorStore.validateXml()) {
        throw new Error('XML is not valid');
      }
      formData.append("xml", this.xmlEditorStore.xml);
      this.operationState = OperationState.Pending;
      const res = await ApiService.getDefinitionLevel(formData);
      this.operationState = OperationState.Success;
      return this.mapTree(res);
    } catch (e) {
      console.log(e);
      this.operationState = OperationState.Error;
      this.errorLog = await e.text() ?? null;
      this.errorText = e.message ?? e.statusMessage ?? e.statusText;
      return [];
    }
  };

  @action
  setSavingMode = (mode: SavingMode) => this.savingMode = mode;

  @action
  apply = async () => {
    this.setSavingMode(SavingMode.Apply);
    await this.getDefinitionLevel();
    for (const nodeId of this.openedNodes) {
      await this.onNodeExpand(this.nodesMap.get(nodeId));
    }
  };

  @action
  saveAndExit = async () => {
    this.setSavingMode(SavingMode.Finish);
    await this.getDefinitionLevel();
    if (this.operationState === OperationState.Success) {
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
      publicProcedureName: "CloseEditor",
    });
  };

  @action
  resetState = () => {
    this.operationState = OperationState.None;
    this.errorText = null;
  }

  private getNodeStatus = (node: IDefinitionNode): { icon: IconName, className: string, label: string } => {
    const label = node.text ?? 'Dictionary caching settings';
    if (node.MissingInQp) {
      return {
        label: `${label} (Missing in QP)`,
        icon: "warning-sign",
        className: "",
      };
    }
    if (node.NotInDefinition) {
      return {
        label,
        icon: "exclude-row",
        className: "xml-tree-node-gray",
      };
    }

    return {
      label,
      icon: "document",
      className: "",
    };
  }

  private mapTree = (rawTree: IDefinitionNode[]): ITreeNode<Partial<IDefinitionNode>>[] => {
    return rawTree.map(node => {
      const nodeStatus = this.getNodeStatus(node);
      this.nodesMap.set(node.Id, {
        id: node.Id,
        label: nodeStatus.label,
        isExpanded: false,
        hasCaret: node.hasChildren,
        childNodes: [],
        isSelected: false,
        className: nodeStatus.className,
        icon: nodeStatus.icon,
        nodeData: {
          expanded: node.expanded,
          hasChildren: node.hasChildren,
          missingInQp: node.MissingInQp,
        }
      });
      return this.nodesMap.get(node.Id);
    });
  };
}
