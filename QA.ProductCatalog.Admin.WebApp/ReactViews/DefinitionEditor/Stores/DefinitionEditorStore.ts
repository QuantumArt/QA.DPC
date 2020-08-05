import { action, observable, when } from "mobx";
import { parse } from "fast-xml-parser";
import { ITreeNode } from "@blueprintjs/core";
import ApiService from "../ApiService";
import { IDefinitionNode } from "../ApiService/ApiInterfaces";

export class DefinitionEditorStore {
  @observable xml: string;
  @observable rootId: string;
  @observable tree: ITreeNode[];

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
    when(
      () => this.rootId != null,
      async () => {
        const initialTree = await this.getDefinitionLevel(this.rootId);
        this.setTree(initialTree);
      }
    );

    window.pmrpc.call({
      destination: parent,
      publicProcedureName: "DefinitionEditorLoaded",
      params: ["DefinitionEditor.SetXml"],
      onError: function(statusObj) {
        console.log("Error calling DefinitionEditorLoaded", statusObj);
      }
    });
  }

  @action
  setTree = (newTree: ITreeNode[]) => {
    this.tree = newTree;
  };

  @action
  onNodeExpand = async (node: ITreeNode<Partial<IDefinitionNode>>) => {
    const treePart = await this.getDefinitionLevel(node.id.toString());
    node.childNodes = treePart;
    node.isExpanded = true;
    for (const el of treePart) {
      if (el.isExpanded) {
        await this.onNodeExpand(el);
      }
    }
    // this.forEachNode(treePart, (node: ITreeNode<Partial<IDefinitionNode>>) => {
    //   if (node.isExpanded && node.nodeData.hasChildren) {
    //     this.onNodeExpand(node);
    //   }
    // });
  };

  @action
  onNodeCollapse = (node: ITreeNode) => {
    console.log("close");
    node.isExpanded = false;
  };

  private forEachNode(
    nodes: ITreeNode<Partial<IDefinitionNode>>[],
    callback: (node: ITreeNode) => void
  ) {
    if (nodes == null) {
      return;
    }

    for (const node of nodes) {
      callback(node);
      this.forEachNode(node.childNodes, callback);
    }
  }

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
  private setRootId = (xml: string) => {
    this.rootId = this.parseXml(xml).Content[`${this.attributeNamePrefix}ContentId`];
  };

  private getDefinitionLevel = async (
    path: string
  ): Promise<ITreeNode<Partial<IDefinitionNode>>[]> => {
    const formData = new FormData();
    formData.append("path", path.charAt(0) === "/" ? path : `/${path}`);
    formData.append("xml", this.xml);
    const res = await ApiService.getDefinitionLevel(formData);
    return this.mapTree(res);
  };

  @action
  private mapTree = (rawTree: IDefinitionNode[]): ITreeNode<Partial<IDefinitionNode>>[] => {
    return rawTree.map(node => ({
      id: node.Id,
      label: node.text,
      isExpanded: node.expanded,
      hasCaret: node.hasChildren,
      nodeData: {
        hasChildren: node.hasChildren
      }
    }));
  };

  @action
  private setXml = (xml: string) => {
    this.xml = xml;
  };

  private parseXml = (xml: string, mode: boolean | "strict" = false) => {
    try {
      return parse(
        xml,
        {
          ignoreAttributes: false,
          arrayMode: mode,
          attributeNamePrefix: this.attributeNamePrefix
        },
        true
      );
    } catch (error) {
      console.log(error.message);
      return null;
    }
  };

  private attributeNamePrefix: string = "attr_";
}
