import React from "react";
import { observer } from "mobx-react-lite";
import { Icon, ButtonGroup, Button, Intent } from "@blueprintjs/core";
import { IconNames } from "@blueprintjs/icons";
import "./Style.scss";

const Header = observer(() => {
  return (
    <div className="header">
      <div className="header__buttons">
        <ButtonGroup>
          <Button icon={IconNames.REFRESH} intent={Intent.WARNING}>
            Refresh
          </Button>
          <Button icon={IconNames.FLOPPY_DISK} intent={Intent.DANGER}>
            Save
          </Button>
        </ButtonGroup>
        <ButtonGroup>
          <Button icon={IconNames.APPLICATION} intent={Intent.PRIMARY}>
            Форма
          </Button>
          <Button icon={IconNames.CODE_BLOCK} intent={Intent.PRIMARY}>
            Редактор кода
          </Button>
        </ButtonGroup>
      </div>
    </div>
  );
});

export default Header;
