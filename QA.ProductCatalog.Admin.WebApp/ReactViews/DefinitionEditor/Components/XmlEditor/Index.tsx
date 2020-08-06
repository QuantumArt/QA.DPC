import React from "react";
import { observer } from "mobx-react-lite";
import AceEditor from "react-ace";
import "ace-builds/src-noconflict/mode-xml";
import "ace-builds/src-noconflict/theme-textmate";
import "ace-builds/src-min-noconflict/ext-searchbox";
import { useStores } from "DefinitionEditor";
import { EditorMode } from "DefinitionEditor/Enums";
import { StubForm } from "DefinitionEditor/Components";
import "./Style.scss";

interface Props {
  height: string;
  width: string;
}

const XmlEditor = observer<Props>(({ height, width }) => {
  const { xmlEditorStore, treeStore } = useStores();
  return (
    <div className="xml-editor">
      {xmlEditorStore.mode === EditorMode.Form && treeStore.selectedNodeId && (
        <div className="xml-editor__form-view">
          <StubForm nodeId={treeStore.selectedNodeId} />
        </div>
      )}
      <AceEditor
        mode="xml"
        theme="textmate"
        onChange={newValue => console.log("change", newValue)}
        name="definitionXml"
        editorProps={{ $blockScrolling: true }}
        value={xmlEditorStore.xml}
        fontSize={xmlEditorStore.fontSize}
        height={height}
        width={width}
        wrapEnabled={xmlEditorStore.wrapLines}
      />
    </div>
  );
});

export default XmlEditor;
