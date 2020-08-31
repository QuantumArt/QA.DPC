import { action, observable } from "mobx";
import { parse, validate } from "fast-xml-parser";

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
  @observable fontSize: number = 14;
  @observable wrapLines: boolean = true;
  @observable searchOnClick: boolean = true;
  @observable formMode: boolean = false;

  @action
  toggleFormMode = () => this.formMode = !this.formMode;

  @action
  changeFontSize = (size: number) => {
    this.fontSize = size;
  };

  @action
  toggleWrapLines = () => {
    this.wrapLines = !this.wrapLines;
  };

  @action
  toggleSearchOnClick = () => {
    this.searchOnClick = !this.searchOnClick;
  };

  @action
  setXml = (xml: string, firstTime: boolean = false) => {
    if (firstTime) {
      this.origXml = xml;
    }
    this.xml = xml;
    console.log(this.origXml);
  };

  validateXml = () => validate(this.xml) === true;

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
