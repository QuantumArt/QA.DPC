import { observable, action } from "mobx";
import parser from "fast-xml-parser";

type ValidationError = {
  err: { code: string; msg: string; line: number };
};

export default class DefinitionEditorStore {
  @observable xml: string;
  xmlIsValid: true | ValidationError;

  constructor(private settings: DefinitionEditorSettings) {
    window.pmrpc.register({
      publicProcedureName: "DefinitionEditor.SetXml",
      procedure: xml => {
        const xmlEmpty = xml.match(/ contentid="\d+"/i) == null;
        if (!xmlEmpty) {
          this.xml = xml;
        } else {
          this.setInitialXml();
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

  private setInitialXml = () => {
    if (this.settings.xml) {
      const xml = this.settings.xml
        .replace(/&amp;/g, "&")
        .replace(/&lt;/g, "<")
        .replace(/&gt;/g, ">")
        .replace(/&apos;/g, '"')
        .replace(/&quot;/g, "'");
      this.xmlIsValid = parser.validate(xml);
      this.xml = xml;
    } else {
      this.xml = "";
    }
  };

  private initTree = () => {};

  private setXml = () => {};
}
