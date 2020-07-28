import React from "react";
import AceEditor from "react-ace";
import { observer } from "mobx-react-lite";
import SplitPane from "react-split-pane";
import "ace-builds/src-noconflict/mode-xml";
import "ace-builds/src-noconflict/theme-monokai";
import DefinitionEditorStore from "./DefinitionEditorStore";
import Header from "./Components/Header";
import XmlTree from "./Components/XmlTree";
import "./Root.scss";

interface Props {
  store: DefinitionEditorStore;
}

const Root = observer<Props>(({ store }) => {
  const onChange = newValue => console.log("change", newValue);
  const height = `${document.body.clientHeight - 80}px`;
  return (
    <>
      <Header />
      <SplitPane split="vertical" minSize={250} maxSize={700} defaultSize={400} style={{ height }}>
        <XmlTree />
        <AceEditor
          mode="xml"
          theme="monokai"
          onChange={onChange}
          name="UNIQUE_ID_OF_DIV"
          editorProps={{ $blockScrolling: true }}
          value={store.xml}
          height={height}
          width="100%"
        />
      </SplitPane>
    </>
  );
});

export default Root;
