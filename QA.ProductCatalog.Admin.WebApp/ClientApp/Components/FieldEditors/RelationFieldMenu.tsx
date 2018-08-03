import React from "react";
import { ButtonGroup, Button, Intent } from "@blueprintjs/core";

interface RelationFieldMenuProps {
  onCreate?: (e: any) => void;
  onSelect?: (e: any) => void;
  onClear?: (e: any) => void;
  onRefresh?: (e: any) => void;
}

export const RelationFieldMenu = ({
  onCreate,
  onSelect,
  onClear,
  onRefresh
}: RelationFieldMenuProps) => (
  <ButtonGroup>
    {onCreate && (
      <Button minimal small rightIcon="add" intent={Intent.SUCCESS} onClick={onCreate}>
        Создать
      </Button>
    )}
    {onSelect && (
      <Button minimal small rightIcon="th-derived" intent={Intent.PRIMARY} onClick={onSelect}>
        Выбрать
      </Button>
    )}
    {onClear && (
      <Button minimal small rightIcon="eraser" intent={Intent.DANGER} onClick={onClear}>
        Очистить
      </Button>
    )}
    {onRefresh && (
      <Button
        minimal
        small
        rightIcon="automatic-updates"
        intent={Intent.WARNING}
        onClick={onRefresh}
      >
        Обновить
      </Button>
    )}
  </ButtonGroup>
);
