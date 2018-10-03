import React from "react";
import { ButtonGroup, Button, Intent } from "@blueprintjs/core";

interface RelationFieldMenuProps {
  onCreate?: (e: any) => void;
  onClonePrototype?: (e: any) => void;
  onSelect?: (e: any) => void;
  onClear?: (e: any) => void;
  onReload?: (e: any) => void;
}

export const RelationFieldMenu = ({
  onCreate,
  onClonePrototype,
  onSelect,
  onClear,
  onReload
}: RelationFieldMenuProps) => (
  <ButtonGroup>
    {onCreate && (
      <Button
        minimal
        small
        rightIcon="add"
        intent={Intent.SUCCESS}
        onClick={onCreate}
        title="Создать пустую связанную статью с нуля"
      >
        Создать
      </Button>
    )}
    {onClonePrototype && (
      <Button
        minimal
        small
        rightIcon="add"
        intent={Intent.SUCCESS}
        onClick={onClonePrototype}
        title="Создать связанную статью по образцу"
      >
        Создать по образцу
      </Button>
    )}
    {onSelect && (
      <Button
        minimal
        small
        rightIcon="th-derived"
        intent={Intent.PRIMARY}
        onClick={onSelect}
        title="Выбрать одну или несколько статей и связать с продуктом"
      >
        Выбрать
      </Button>
    )}
    {onClear && (
      <Button
        minimal
        small
        rightIcon="eraser"
        intent={Intent.DANGER}
        onClick={onClear}
        title="Очистить связь статьи (несохраненные изменения полей потеряются)"
      >
        Очистить
      </Button>
    )}
    {onReload && (
      <Button
        minimal
        small
        rightIcon="automatic-updates"
        intent={Intent.WARNING}
        onClick={onReload}
        title="Перезагрузить связь статьи с сервера (несохраненные изменения полей потеряются)"
      >
        Перезагрузить
      </Button>
    )}
  </ButtonGroup>
);
