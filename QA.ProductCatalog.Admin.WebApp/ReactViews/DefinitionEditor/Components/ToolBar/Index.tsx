import React from "react";
import { observer } from "mobx-react-lite";
import { Button, ButtonGroup, Intent, Slider, Switch } from "@blueprintjs/core";
import { IconNames } from "@blueprintjs/icons";
import { useStores } from "DefinitionEditor";
import { ErrorDialog, Loading } from "DefinitionEditor/Components";
import { OperationState } from "Shared/Enums";
import "./Style.scss";

const ToolBar = observer(() => {
  const { xmlEditorStore, treeStore } = useStores();

  return (
    <div className="editor-toolbar">
      <ErrorDialog />
      <div className="editor-toolbar__buttons">
        <ButtonGroup>
          <Button icon={IconNames.REFRESH} onClick={treeStore.refresh}>
            Refresh
          </Button>
          <Button icon={IconNames.CONFIRM} onClick={treeStore.apply}>
            Apply
          </Button>
          <Button icon={IconNames.FLOPPY_DISK} onClick={treeStore.saveAndExit}>
            Save and Exit
          </Button>
          <div className="editor-toolbar__divider" />
          <Button icon={IconNames.CROSS} onClick={treeStore.exit}>
            Exit
          </Button>
          <Loading
            className="editor-toolbar__loading"
            active={treeStore.operationState === OperationState.Pending}
          />
        </ButtonGroup>
        <div
          className="editor-toolbar__xml-controls"
          style={{ visibility: xmlEditorStore.formMode ? "hidden" : "visible" }}
        >
          <Switch
            label="Query on click"
            checked={xmlEditorStore.queryOnClick}
            onChange={xmlEditorStore.toggleQueryOnClick}
          />
          <Switch
            label="Wrap lines"
            checked={xmlEditorStore.wrapLines}
            onChange={xmlEditorStore.toggleWrapLines}
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
        {treeStore.selectedNodeId !== null &&
          <Button
            icon={xmlEditorStore.formMode ? IconNames.APPLICATION : IconNames.CODE_BLOCK}
            intent={Intent.PRIMARY}
            onClick={xmlEditorStore.toggleFormMode}
            className="editor-toolbar__form-btn"
          >
            {xmlEditorStore.formMode ? <span>Редактор кода</span> : <span>Форма</span>}
          </Button>
        }
      </div>
    </div>
  );
});

export default ToolBar;
