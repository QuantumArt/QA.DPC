import React from "react";
import { Button, ButtonGroup, Intent, Popover, Position, Alignment } from "@blueprintjs/core";

interface SaveButtonProps {
  small?: boolean;
  onSave?: (e: any) => void;
  onSaveAll?: (e: any) => void;
}

export const SaveButton = ({ onSave, onSaveAll, small }: SaveButtonProps) => (
  <Popover position={Position.BOTTOM_LEFT} usePortal={false}>
    <Button minimal small={small} icon="caret-down" rightIcon="floppy-disk" intent={Intent.PRIMARY}>
      Сохранить
    </Button>
    <ButtonGroup vertical minimal alignText={Alignment.LEFT} className="editor-save-button">
      <Button rightIcon="floppy-disk" intent={Intent.PRIMARY} onClick={onSave}>
        Сохранить
      </Button>
      <Button rightIcon="floppy-disk" intent={Intent.PRIMARY} onClick={onSaveAll}>
        Сохранить все
      </Button>
    </ButtonGroup>
  </Popover>
);
