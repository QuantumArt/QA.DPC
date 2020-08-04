import { action, observable, when } from "mobx";
import { parse } from "fast-xml-parser";
import { ITreeNode } from "@blueprintjs/core";

type ValidationError = {
  err: { code: string; msg: string; line: number };
};

export default class DefinitionEditorStore {
  @observable xml: string;
  @observable rootId: string;
  @observable.ref tree: ITreeNode[];

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
      () => {
        this.getDefinitionLevel(this.rootId);
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

  private getDefinitionLevel = async (path: string) => {
    const formData = new FormData();
    formData.append("path", `/${path}`);
    formData.append("xml", this.xml);
    const res = await fetch(this.settings.getDefinitionLevelUrl, {
      method: "POST",
      body: formData
    });
    const tree = await res.json();
    console.log(tree);
    return res;
  };

  @action
  private mapTree = () => {};

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
