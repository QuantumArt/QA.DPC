import React, { Context } from "react";
import { observer } from "mobx-react";
import AceEditor from "react-ace";
import "ace-builds/src-noconflict/mode-xml";
import "ace-builds/src-noconflict/theme-textmate";
import "ace-builds/src-min-noconflict/ext-searchbox";
import { EditorMode } from "DefinitionEditor/Enums";
import { StubForm } from "DefinitionEditor/Components";
import { StoresCtx, storesCtx } from "DefinitionEditor/Stores";
import "./Style.scss";

interface Props {
  height: string;
  width: string;
}

@observer
export default class XmlEditor extends React.Component<Props> {
  context: React.ContextType<typeof storesCtx>;
  static contextType: Context<StoresCtx> = storesCtx;

  // componentDidMount() {
  //   const reactAceComponent = this.refs.aceEditor as AceEditor;
  //   const editor = reactAceComponent.editor;
  // }

  render() {
    const { xmlEditorStore, treeStore } = this.context;
    const { height, width } = this.props;
    return (
      <div className="xml-editor">
        {xmlEditorStore.mode === EditorMode.Form && treeStore.selectedNodeId && (
          <div className="xml-editor__form-view">
            <StubForm nodeId={treeStore.selectedNodeId} />
          </div>
        )}
        <AceEditor
          ref="aceEditor"
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
  }
}
