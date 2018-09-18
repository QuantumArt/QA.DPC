import React from "react";
import { Button, Menu, MenuItem, Intent, Popover, Position, Icon } from "@blueprintjs/core";
import "./ArticleEditor.scss";

interface ArticleMenuProps {
  small?: boolean;
  onSave?: (e: any) => void;
  onRemove?: (e: any) => void;
  onRefresh?: (e: any) => void;
  onReload?: (e: any) => void;
  onClone?: (e: any) => void;
  onPublish?: (e: any) => void;
}

export const ArticleMenu = ({
  small,
  onSave,
  onRemove,
  onRefresh,
  onReload,
  onClone,
  onPublish
}: ArticleMenuProps) => (
  <Popover position={Position.BOTTOM_RIGHT} usePortal={false}>
    <Button minimal small={small} icon="caret-down" intent={Intent.PRIMARY}>
      Действия
    </Button>
    <Menu>
      {onSave && (
        <MenuItem
          labelElement={<Icon icon="floppy-disk" />}
          intent={Intent.PRIMARY}
          onClick={onSave}
          text="Сохранить"
          title="Сохранить текущую статью"
        />
      )}
      {onClone && (
        <MenuItem
          labelElement={<Icon icon="duplicate" />}
          intent={Intent.SUCCESS}
          onClick={onPublish}
          text="Клонировать"
          title="Клонировать текущую статью"
        />
      )}
      {onRefresh && (
        <MenuItem
          labelElement={<Icon icon="refresh" />}
          intent={Intent.WARNING}
          onClick={onRefresh}
          text="Обновить"
          title="Загрузить изменения продукта с сервера (несохраненные изменения полей останутся)"
        />
      )}
      {onReload && (
        <MenuItem
          labelElement={<Icon icon="automatic-updates" />}
          intent={Intent.WARNING}
          onClick={onReload}
          text="Перезагрузить"
          title="Перезагрузить часть продукта с сервера (несохраненные изменения полей потеряются)"
        />
      )}
      {onRemove && (
        <MenuItem
          labelElement={<Icon icon="remove" />}
          intent={Intent.DANGER}
          onClick={onRemove}
          text="Удалить"
          title="Удалить (архивировать) текущую статью"
        />
      )}
      {onPublish && (
        <MenuItem
          labelElement={<Icon icon="share" />}
          intent={Intent.NONE}
          onClick={onPublish}
          text="Опубликовать"
          title="Опубликовать часть продукта"
        />
      )}
    </Menu>
  </Popover>
);
