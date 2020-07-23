import React from "react";
import AceEditor from "react-ace";
import "ace-builds/src-noconflict/mode-xml";
import "ace-builds/src-noconflict/theme-monokai";
import parser from "fast-xml-parser";
//import { parseString } from "xml2js";

const Editor = () => {
  const onChange = newValue => console.log("change", newValue);
  const rawXml = window.definitionEditor.xml;
  const xml = rawXml
    .replace(/&amp;/g, "&")
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&apos;/g, '"')
    .replace(/&quot;/g, "'");

  const isValid = parser.validate(xml);
  console.log(xml, isValid);
  //parseString(xml, (_err, res) => {
  //  console.dir(res);
  //});

  return (
    <AceEditor
      mode="xml"
      theme="monokai"
      onChange={onChange}
      name="UNIQUE_ID_OF_DIV"
      editorProps={{ $blockScrolling: true }}
      value={xml}
    />
  );
};

export default Editor;
