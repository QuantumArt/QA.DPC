import React from "react";
import XmlEditorStore from "./XmlEditorStore";
import { action, computed, observable, toJS, when } from "mobx";
import { OperationState } from "Shared/Enums";
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
  @observable nodesMap: Map<string, ITreeNode> = new Map();
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
    for (const el of node.childNodes) {
      if (el.nodeData.expanded && el.nodeData.hasChildren) {
        await this.onNodeExpand(el);
      }
    }
  };

  @action
  onNodeCollapse = (node: ITreeNode) => {
    node.isExpanded = false;
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
    const formData = new FormData();
    if (path) {
      formData.append("path", path.charAt(0) === "/" ? path : `/${path}`);
    }
    formData.append("xml", this.xmlEditorStore.xml);
    try {
      this.operationState = OperationState.Pending;
      const res = await ApiService.getDefinitionLevel(formData);
      this.operationState = OperationState.Success;
      return this.mapTree(res);
    } catch (e) {
      this.operationState = OperationState.Error;
      console.log("Error fetching Definition Level", e);
      return [];
    }
  };

  private mapTree = (rawTree: IDefinitionNode[]): ITreeNode<Partial<IDefinitionNode>>[] => {
    return rawTree.map(node => {
      this.nodesMap.set(node.Id, {
        id: node.Id,
        label: node.text ?? 'Dictionary caching settings',
        isExpanded: false,
        hasCaret: node.hasChildren,
        childNodes: [],
        isSelected: false,
        nodeData: {
          expanded: node.expanded,
          hasChildren: node.hasChildren
        }
      });
      return this.nodesMap.get(node.Id);
    });
  };
}
