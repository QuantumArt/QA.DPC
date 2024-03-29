import React from "react";
import { Classes, Icon, IconName, Intent, Tooltip } from "@blueprintjs/core";
import { action, computed, observable, when } from "mobx";
import XmlEditorStore from "./XmlEditorStore";
import { OperationState } from "Shared/Enums";
import { ITreeNode } from "@blueprintjs/core";
import { IDefinitionNode } from "DefinitionEditor/ApiService/ApiInterfaces";
import ApiService from "DefinitionEditor/ApiService";
import { l } from "DefinitionEditor/Localization";
import ErrorHandler from "./ErrorHandler";

export default class TreeStore extends ErrorHandler {
  constructor(private xmlEditorStore: XmlEditorStore) {
    super();
  }

  init = (setSelectedNodeId: (id: string) => void) => {
    this.setSelectedNodeId = setSelectedNodeId;
    when(
      () => this.xmlEditorStore.rootId != null,
      async () => {
        await this.withLogError(async () => await this.getDefinitionLevel());
        await this.onNodeExpand(this.tree[0]);
      }
    );
  };
  @observable operationState: OperationState = OperationState.None;

  @observable errorText: string = null;
  @observable errorLog: string = null;
  @observable nodesMap: Map<ITreeNode["id"], ITreeNode<Partial<IDefinitionNode>>> = new Map();
  @observable openedNodes: ITreeNode["id"][] = [];
  @observable private selectedNodeId: string = null;

  setSelectedNodeId: (id: string) => void = null;

  @action
  setSelectedNodeIdInUI = () => {
    if (this.selectedNodeId) this.nodesMap.get(this.selectedNodeId).isSelected = true;
  };

  @computed get tree() {
    if (this.xmlEditorStore.rootId && this.nodesMap.has(`/${this.xmlEditorStore.rootId}`)) {
      return [this.nodesMap.get(`/${this.xmlEditorStore.rootId}`)];
    } else {
      return [];
    }
  }

  @action
  onNodeExpand = async (node: ITreeNode<Partial<IDefinitionNode>>) => {
    if (node.nodeData.hasChildren && node.childNodes.length === 0) {
      node.childNodes =
        (await this.withLogError(async () => await this.getDefinitionLevel(node.id.toString()))) ||
        [];
    }
    node.isExpanded = node.nodeData.InDefinition;
    if (node.isExpanded) {
      this.setOpenedNodes(node.id);
      for (const el of node.childNodes) {
        if (el.nodeData.expanded && el.nodeData.hasChildren) {
          await this.onNodeExpand(el);
        }
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
    this.setSelectedNodeId(this.selectedNodeId);
    if (this.xmlEditorStore.queryOnClick && this.selectedNodeId !== null) {
      this.gatherSearchString();
    }
  };

  @action
  getDefinitionLevel = async (path?: string): Promise<ITreeNode<Partial<IDefinitionNode>>[]> => {
    const formData = new FormData();
    if (path) {
      formData.append("path", path.charAt(0) === "/" ? path : `/${path}`);
    }
    const validation = this.xmlEditorStore.validateXml();
    if (validation !== true) {
      throw validation;
    }
    formData.append("xml", this.xmlEditorStore.xml);
    this.operationState = OperationState.Pending;
    const res = await ApiService.getDefinitionLevel(formData);
    this.xmlEditorStore.setLastLocalSavedXml(this.xmlEditorStore.xml);
    this.operationState = OperationState.Success;
    return this.mapTree(res, path);
  };

  @action
  withLogError = async <T extends {} | void>(cb: () => Promise<T>): Promise<T> => {
    try {
      return await cb();
    } catch (e) {
      console.error(e);
      await this.logError(e, l("InvalidXml"));
      return null;
    }
  };

  @action
  logError = async (e, fallbackText: string) => {
    let log: string;
    try {
      if (e.err) {
        log = `${e.err.code}\n${e.err.msg}\nOn line ${e.err.line}`;
      } else {
        log = (await e.text()) ?? null;
      }
    } catch {
    } finally {
      const text = e.message || e.statusMessage || e.statusText || (e.err && fallbackText);
      this.setError(text, log);
    }
  };

  @action
  getSingleNode = async (path: string): Promise<ITreeNode<Partial<IDefinitionNode>>> => {
    const formData = new FormData();
    formData.append("path", path.charAt(0) === "/" ? path : `/${path}`);
    const validation = this.xmlEditorStore.validateXml();
    if (validation !== true) {
      throw validation;
    }
    formData.append("xml", this.xmlEditorStore.xml);
    this.operationState = OperationState.Pending;
    const singleNode = await ApiService.getSingleNode(formData);
    this.xmlEditorStore.setLastLocalSavedXml(this.xmlEditorStore.xml);
    this.operationState = OperationState.Success;
    return this.updateSingleNode(singleNode);
  };

  @action
  setError = (errText?: string, log?: string) => {
    this.operationState = OperationState.Error;
    this.errorText = errText ?? l("GenericError");
    if (log) {
      this.errorLog = log;
    }
  };

  @action
  resetErrorState = () => {
    this.operationState = OperationState.None;
    this.errorText = null;
    this.errorLog = null;
  };

  private getNodeStatus = (node: IDefinitionNode): Partial<ITreeNode> => {
    if (node.IsDictionaries) {
      return {
        label: l("DictionaryCachingSettings"),
        icon: <Icon icon="cog" intent={Intent.PRIMARY} className={Classes.TREE_NODE_ICON} />
      };
    }
    if (node.MissingInQp) {
      return {
        label: node.text,
        icon: node.IconName as IconName,
        secondaryLabel: (
          <Tooltip content={l("MissingInQP")}>
            <Icon icon="warning-sign" intent={Intent.WARNING} />
          </Tooltip>
        )
      };
    }
    if (!node.InDefinition) {
      return {
        label: node.text,
        icon: (
          <Tooltip content={l("NotInDefinition")}>
            <Icon icon="exclude-row" className={Classes.TREE_NODE_ICON} />
          </Tooltip>
        ),
        className: "xml-tree-node-gray"
      };
    }

    return {
      label: node.text,
      icon: node.IconName as IconName,
      className: ""
    };
  };

  private mapTree = (
    rawTree: IDefinitionNode[],
    parentId: string
  ): ITreeNode<Partial<IDefinitionNode>>[] => {
    return rawTree.map(node => {
      return this.setSingleNode(node, parentId);
    });
  };

  @action
  setSingleNode = (
    node: IDefinitionNode,
    parentId: string
  ): ITreeNode<Partial<IDefinitionNode>> => {
    this.nodesMap.set(node.Id, {
      id: node.Id,
      label: "",
      isExpanded: false,
      hasCaret: node.hasChildren,
      childNodes: [],
      isSelected: false,
      ...this.getNodeStatus(node),
      nodeData: {
        parentId: parentId,
        expanded: node.expanded,
        hasChildren: node.hasChildren,
        missingInQp: node.MissingInQp,
        InDefinition: node.InDefinition
      }
    });
    return this.nodesMap.get(node.Id);
  };

  @action
  updateSingleNode = (node: IDefinitionNode): ITreeNode<Partial<IDefinitionNode>> => {
    const nodeObj = this.nodesMap.get(node.Id);
    Object.assign(nodeObj, this.getNodeStatus(node));
    nodeObj.isSelected = false;
    nodeObj.hasCaret = node.hasChildren;
    nodeObj.isExpanded = node.InDefinition && node.hasChildren;
    nodeObj.nodeData.expanded = node.InDefinition && node.hasChildren;
    nodeObj.nodeData.hasChildren = node.InDefinition && node.hasChildren;
    nodeObj.nodeData.InDefinition = node.InDefinition;
    return nodeObj;
  };

  private gatherSearchString = () => {
    const arr = this.selectedNodeId.split("/");
    let lastPart = arr.pop();
    if (lastPart === "0") {
      lastPart = arr.pop();
    }
    const nodeLabel = this.nodesMap
      .get(this.selectedNodeId)
      .label.toString()
      .split(" ")[0];
    const dummy = document.createElement("input");
    document.body.appendChild(dummy);
    dummy.value = `(.*${lastPart})(.*${nodeLabel}).*`;
    dummy.select();
    document.execCommand("copy");
    document.body.removeChild(dummy);
  };
}
