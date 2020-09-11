import React, { Context } from "react";
import { observer } from "mobx-react";
import AceEditor from "react-ace";
import "ace-builds/src-noconflict/mode-xml";
import "ace-builds/src-noconflict/theme-textmate";
import "ace-builds/src-min-noconflict/ext-searchbox";
import { EditForm } from "DefinitionEditor/Components";
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

  render() {
    const { xmlEditorStore, controlsStore } = this.context;
    const { height, width } = this.props;
    return (
      <div className="xml-editor">
        {controlsStore.formMode && controlsStore.selectedNodeId && (
          <div className="xml-editor__form-view">
            <EditForm />
          </div>
        )}
        <AceEditor
          ref="aceEditor"
          mode="xml"
          theme="textmate"
          onChange={data => xmlEditorStore.setXml(data)}
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
