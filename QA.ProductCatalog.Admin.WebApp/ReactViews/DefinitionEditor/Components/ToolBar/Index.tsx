import React from "react";
import { observer } from "mobx-react-lite";
import { Button, ButtonGroup, Intent, Slider, Switch } from "@blueprintjs/core";
import { IconNames } from "@blueprintjs/icons";
import { useStores } from "DefinitionEditor";
import { Loading } from "DefinitionEditor/Components";
import { OperationState } from "Shared/Enums";
import { EditorMode } from "DefinitionEditor/Enums";
import "./Style.scss";

const ToolBar = observer(() => {
  const { xmlEditorStore, treeStore } = useStores();
  return (
    <div className="editor-toolbar">
      <div className="editor-toolbar__buttons">
        <ButtonGroup>
          <Button icon={IconNames.REFRESH} intent={Intent.WARNING}>
            Refresh
          </Button>
          <Button icon={IconNames.FLOPPY_DISK} intent={Intent.DANGER}>
            Save
          </Button>
          <Loading
            className="editor-toolbar__loading"
            active={treeStore.operationState === OperationState.Pending}
          />
        </ButtonGroup>
        <div
          className="editor-toolbar__xml-controls"
          style={{ visibility: xmlEditorStore.mode === EditorMode.Xml ? "visible" : "hidden" }}
        >
          <Switch
            label="Search on click"
            checked={xmlEditorStore.searchOnClick}
            onChange={xmlEditorStore.toggleSearchOnClick}
            large
          />
          <Switch
            label="Wrap lines"
            checked={xmlEditorStore.wrapLines}
            onChange={xmlEditorStore.toggleWrapLines}
            large
          />
          <div className="editor-toolbar__slider">
            <Slider
              value={xmlEditorStore.fontSize}
              max={20}
              min={14}
              stepSize={2}
              labelStepSize={2}
              onChange={(val: number) => {
                xmlEditorStore.changeFontSize(val);
              }}
            />
          </div>
        </div>
        <ButtonGroup>
          <Button
            icon={IconNames.APPLICATION}
            intent={Intent.PRIMARY}
            disabled={treeStore.selectedNodeId === null}
            onClick={() =>
              treeStore.selectedNodeId !== null && xmlEditorStore.setMode(EditorMode.Form)
            }
          >
            Форма
          </Button>
          <Button
            icon={IconNames.CODE_BLOCK}
            intent={Intent.PRIMARY}
            onClick={() => xmlEditorStore.setMode(EditorMode.Xml)}
          >
            Редактор кода
          </Button>
        </ButtonGroup>
      </div>
    </div>
  );
});

export default ToolBar;
