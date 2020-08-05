import { action, computed, observable, when } from "mobx";
import { parse } from "fast-xml-parser";
import { ITreeNode } from "@blueprintjs/core";
import ApiService from "../ApiService";
import { IDefinitionNode } from "../ApiService/ApiInterfaces";
import { OperationState } from "Shared/Enums";

export default class DefinitionEditorStore {
  @observable xml: string;
  @observable rootId: string;
  @observable operationState: OperationState = OperationState.None;

  constructor(private settings: DefinitionEditorSettings) {
    window.pmrpc.register({
      publicProcedureName: "DefinitionEditor.SetXml",
      procedure: xml => {
        const xmlEmpty = xml.match(/ contentid="\d+"/i) == null;
        if (!xmlEmpty) {
          this.setXml(xml);
          this.setRootId(xml);
        } else {
          this.setInitialXml();
        }
      }
    });
    when(() => this.rootId != null, () => this.getDefinitionLevel(this.rootId));
    window.pmrpc.call({
      destination: parent,
      publicProcedureName: "DefinitionEditorLoaded",
      params: ["DefinitionEditor.SetXml"],
      onError: function(statusObj) {
        console.log("Error calling DefinitionEditorLoaded", statusObj);
      }
    });
  }

  @observable nodesMap: Map<string, ITreeNode> = new Map();

  @computed get tree() {
    return Array.from(this.nodesMap.values());
  }

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
  private setRootId = (xml: string) => {
    this.rootId = this.parseXml(xml).Content[`${this.attributeNamePrefix}ContentId`];
  };

  private getDefinitionLevel = async (
    path: string
  ): Promise<ITreeNode<Partial<IDefinitionNode>>[]> => {
    const formData = new FormData();
    formData.append("path", path.charAt(0) === "/" ? path : `/${path}`);
    formData.append("xml", this.xml);
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
      const mappedNode: ITreeNode<Partial<IDefinitionNode>> = {
        id: node.Id,
        label: node.text,
        isExpanded: false,
        hasCaret: node.hasChildren,
        childNodes: [],
        nodeData: {
          expanded: node.expanded,
          hasChildren: node.hasChildren
        }
      };
      this.nodesMap.set(node.Id, mappedNode);
      return mappedNode;
    });
  };

  private setInitialXml = () => {
    if (this.settings.xml) {
      const xml = this.settings.xml
        .replace(/&amp;/g, "&")
        .replace(/&lt;/g, "<")
        .replace(/&gt;/g, ">")
        .replace(/&apos;/g, '"')
        .replace(/&quot;/g, "'");
      this.setXml(xml);
      this.setRootId(xml);
    } else {
      this.setXml("");
    }
  };

  @action
  private setXml = (xml: string) => {
    this.xml = xml;
  };

  private parseXml = (xml: string, mode: boolean | "strict" = false) => {
    try {
      return parse(
        xml,
        { ignoreAttributes: false, arrayMode: mode, attributeNamePrefix: this.attributeNamePrefix },
        true
      );
    } catch (error) {
      console.log(error.message);
      return null;
    }
  };

  private attributeNamePrefix: string = "attr_";
}
