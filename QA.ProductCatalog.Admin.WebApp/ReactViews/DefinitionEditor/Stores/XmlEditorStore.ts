import { action, computed, observable } from "mobx";
import { parse, validate, ValidationError } from "fast-xml-parser";
import apiService from "DefinitionEditor/ApiService";

export default class XmlEditorStore {
  constructor(private settings: DefinitionEditorSettings) {
    window.pmrpc.register({
      publicProcedureName: "DefinitionEditor.SetXml",
      procedure: async (xml, contentId) => {
        const xamlEmpty = xml.match(/ contentid="\d+"/i) == null;
        if (!xamlEmpty) {
          this.setXml(xml, true);
          this.setRootId(xml);
        } else {
          this.origXmlWasEmpty = true;

          if (contentId) {
            let node = await apiService.getInitialNode(contentId);
            if (node) {
              this.setXml(node, true);
              this.setRootId(node);
              return;
            }
          }

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
  lastLocalSavedXml: string;
  @computed get xmlIsEmpty() {
    return this.xml?.replace(/\s/g, "") === "";
  }
  @observable origXmlWasEmpty: boolean = false;
  @observable rootId: string;
  @observable fontSize: number = localStorage.getItem("fontSize")
    ? parseInt(localStorage.getItem("fontSize"))
    : 14;
  @observable wrapLines: boolean = localStorage.getItem("wrapLines") === "true";
  @observable queryOnClick: boolean = localStorage.getItem("queryOnClick") === "true";

  @action
  setLastLocalSavedXml = (xml: string) => {
    this.lastLocalSavedXml = xml;
  };

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

  isSameDefinition = (): boolean => {
    if (this.origXmlWasEmpty) {
      return false;
    }
    return this.xml === this.origXml;
  };
  isSameDefinitionWithLastSaved = (): boolean => this.xml === this.lastLocalSavedXml;

  @action
  toggleQueryOnClick = () => {
    this.queryOnClick = !this.queryOnClick;
    localStorage.setItem("queryOnClick", `${this.queryOnClick}`);
  };

  @action
  setXml = (xml: string, firstTime: boolean = false) => {
    if (firstTime) {
      this.origXml = xml;
      this.setLastLocalSavedXml(xml);
    }
    this.xml = xml;
  };

  validateXml = (): true | ValidationError => validate(this.xml);

  @action
  private setRootId = (xml: string) => {
    this.rootId = this.parseXml(xml).Content[`${this.attributeNamePrefix}ContentId`];
  };

  @action
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
