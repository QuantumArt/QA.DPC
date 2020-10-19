import React from "react";
import { observer, useLocalStore } from "mobx-react-lite";
import { Button, ButtonGroup, Card, Intent, Popover, Slider, Switch } from "@blueprintjs/core";
import { IconNames } from "@blueprintjs/icons";
import { useStores } from "DefinitionEditor";
import { TreeErrorDialog, Loading } from "DefinitionEditor/Components";
import { OperationState } from "Shared/Enums";
import { l } from "DefinitionEditor/Localization";
import "./Style.scss";
import { Position } from "@blueprintjs/core/lib/esm/common/position";

const ToolBar = observer(() => {
  const { xmlEditorStore, treeStore, controlsStore } = useStores();
  const infoState = useLocalStore(() => ({
    opened: false,
    toggle() {
      this.opened = !this.opened;
    }
  }));
  const infoContent = () => (
    <Card>
      <h4 className="bp3-heading">"Query on click" Guide:</h4>
      <ol className="bp3-list">
        <li>Turn on "Query on click" setting</li>
        <li>Click on node in the tree</li>
        <li>
          Open Search in the XML editor <b>(Ctrl+F)</b>
        </li>
        <li>
          Turn on <i>Regexp mode</i> <b>(Alt+R)</b>
        </li>
        <li>
          Insert generated query <b>(Ctrl+V)</b>
        </li>
      </ol>
    </Card>
  );

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
              controlsStore.submitFormSyntheticEvent &&
                controlsStore.submitFormSyntheticEvent(event);
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
          style={{ visibility: controlsStore.formMode ? "hidden" : "visible" }}
        >
          <Popover
            position={Position.BOTTOM}
            content={infoContent()}
            isOpen={infoState.opened}
            modifiers={{ arrow: { enabled: false } }}
          >
            <Button
              minimal
              icon={IconNames.INFO_SIGN}
              className="editor-toolbar__info-btn"
              onClick={infoState.toggle}
            />
          </Popover>
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
              onChange={xmlEditorStore.changeFontSize}
            />
          </div>
        </div>
        {controlsStore.selectedNodeId !== null && (
          <Button
            icon={controlsStore.formMode ? IconNames.APPLICATION : IconNames.CODE_BLOCK}
            intent={Intent.PRIMARY}
            onClick={controlsStore.onChangeFormMode}
            className="editor-toolbar__form-btn"
          >
            {controlsStore.formMode ? <span>Редактор кода</span> : <span>Форма</span>}
          </Button>
        )}
      </div>
    </div>
  );
});

export default ToolBar;
