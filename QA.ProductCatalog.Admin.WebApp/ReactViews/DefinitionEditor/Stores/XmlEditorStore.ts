import { action, observable } from "mobx";
import { parse, validate, ValidationError } from "fast-xml-parser";
import ControlsStore from "./ControlsStore";

export default class XmlEditorStore {
  constructor(private settings: DefinitionEditorSettings) {
    window.pmrpc.register({
      publicProcedureName: "DefinitionEditor.SetXml",
      procedure: xml => {
        const xmlEmpty = xml.match(/ contentid="\d+"/i) == null;
        if (!xmlEmpty) {
          this.setXml(xml, true);
          this.setRootId(xml);
        } else {
          this.setDefaultXml();
        }
      }
    });
    window.pmrpc.call({
      destination: parent,
      publicProcedureName: "DefinitionEditorLoaded",
      params: ["DefinitionEditor.SetXml"],
      onError: function(statusObj) {
        console.log("Error calling DefinitionEditorLoaded", statusObj);
      }
    });
  }

  @observable xml: string;
  origXml: string;
  @observable rootId: string;
  @observable fontSize: number = localStorage.getItem("fontSize")
    ? parseInt(localStorage.getItem("fontSize"))
    : 14;
  @observable wrapLines: boolean = localStorage.getItem("wrapLines") === "true";
  @observable queryOnClick: boolean = localStorage.getItem("queryOnClick") === "true";
  @observable formMode: boolean = false;

  @action
  toggleFormMode = () => (this.formMode = !this.formMode);

  @action
  changeFontSize = (size: number) => {
    this.fontSize = size;
    localStorage.setItem("fontSize", `${size}`);
  };

  @action
  toggleWrapLines = () => {
    this.wrapLines = !this.wrapLines;
    localStorage.setItem("wrapLines", `${this.wrapLines}`);
  };

  @action
  toggleQueryOnClick = () => {
    this.queryOnClick = !this.queryOnClick;
    localStorage.setItem("queryOnClick", `${this.queryOnClick}`);
  };

  @action
  setXml = (xml: string, firstTime: boolean = false) => {
    if (firstTime) {
      this.origXml = xml;
    }
    this.xml = xml;
  };

  validateXml = (): true | ValidationError => validate(this.xml);

  @action
  private setRootId = (xml: string) => {
    this.rootId = this.parseXml(xml).Content[`${this.attributeNamePrefix}ContentId`];
  };

  private setDefaultXml = () => {
    if (this.settings.xml) {
      const xml = this.settings.xml
        .replace(/&amp;/g, "&")
        .replace(/&lt;/g, "<")
        .replace(/&gt;/g, ">")
        .replace(/&apos;/g, '"')
        .replace(/&quot;/g, "'");
      this.setXml(xml, true);
      this.setRootId(xml);
    } else {
      this.setXml("", true);
    }
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
