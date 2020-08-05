import React from "react";
import AceEditor from "react-ace";
import { observer } from "mobx-react-lite";
import SplitPane from "react-split-pane";
import "ace-builds/src-noconflict/mode-xml";
import "ace-builds/src-noconflict/theme-monokai";
import { useStores } from "./Stores";
import "./Root.scss";
import { XmlTree, Header } from "DefinitionEditor/Components";

const Root = observer(() => {
  const { editorStore } = useStores();
  const height = `${document.body.clientHeight - 80}px`;
  return (
    <>
      <Header />
      <SplitPane split="vertical" minSize={250} maxSize={700} defaultSize={400} style={{ height }}>
        <XmlTree />
        <AceEditor
          mode="xml"
          theme="monokai"
          onChange={newValue => console.log("change", newValue)}
          name="UNIQUE_ID_OF_DIV"
          editorProps={{ $blockScrolling: true }}
          value={editorStore.xml}
          height={height}
          width="100%"
        />
      </SplitPane>
    </>
  );
});

export default Root;
