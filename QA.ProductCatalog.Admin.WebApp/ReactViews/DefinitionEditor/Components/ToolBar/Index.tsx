import React from "react";
import { observer } from "mobx-react-lite";
import { Button, ButtonGroup, Intent, Slider, Switch } from "@blueprintjs/core";
import { IconNames } from "@blueprintjs/icons";
import { useStores } from "DefinitionEditor";
import { TreeErrorDialog, Loading } from "DefinitionEditor/Components";
import { OperationState } from "Shared/Enums";
import { l } from "DefinitionEditor/Localization";
import "./Style.scss";

const ToolBar = observer(() => {
  const { xmlEditorStore, treeStore, controlsStore } = useStores();

  return (
    <div className="editor-toolbar">
      <TreeErrorDialog />
      <div className="editor-toolbar__buttons">
        <ButtonGroup>
          <Button
            icon={IconNames.REFRESH}
            onClick={controlsStore.refresh}
            disabled={treeStore.operationState === OperationState.Pending}
          >
            {l("Refresh")}
          </Button>
          <Button
            icon={IconNames.CONFIRM}
            type="submit"
            onClick={event => {
              treeStore.submitFormSyntheticEvent && treeStore.submitFormSyntheticEvent(event);
              controlsStore.apply();
            }}
            disabled={treeStore.operationState === OperationState.Pending}
          >
            {l("Apply")}
          </Button>
          <Button
            icon={IconNames.FLOPPY_DISK}
            onClick={controlsStore.saveAndExit}
            disabled={treeStore.operationState === OperationState.Pending}
          >
            {l("SaveAndExit")}
          </Button>
          <div className="editor-toolbar__divider" />
          <Button icon={IconNames.CROSS} onClick={controlsStore.exit}>
            {l("Exit")}
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
            label={l("QueryOnClick")}
            checked={xmlEditorStore.queryOnClick}
            onChange={xmlEditorStore.toggleQueryOnClick}
          />
          <Switch
            label={l("WrapLines")}
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
        {controlsStore.selectedNodeId !== null && (
          <Button
            icon={xmlEditorStore.formMode ? IconNames.APPLICATION : IconNames.CODE_BLOCK}
            intent={Intent.PRIMARY}
            onClick={xmlEditorStore.toggleFormMode}
            className="editor-toolbar__form-btn"
          >
            {xmlEditorStore.formMode ? <span>Редактор кода</span> : <span>Форма</span>}
          </Button>
        )}
      </div>
    </div>
  );
});

export default ToolBar;
