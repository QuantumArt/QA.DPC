import React from "react";
import AceEditor from "react-ace";
import "ace-builds/src-noconflict/mode-xml";
import "ace-builds/src-noconflict/theme-github";

const Editor = () => {
  const onChange = newValue => console.log("change", newValue);
  console.log((window as any).definitionEditor);
  return (
    <AceEditor
      mode="java"
      theme="github"
      onChange={onChange}
      name="UNIQUE_ID_OF_DIV"
      editorProps={{ $blockScrolling: true }}
      value={(window as any).definitionEditor.xml}
    />
  );
};

export default Editor;
