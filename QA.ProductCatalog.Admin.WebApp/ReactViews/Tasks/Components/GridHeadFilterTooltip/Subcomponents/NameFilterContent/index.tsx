import React from "react";
import { observer } from "mobx-react-lite";
import { Button, FormGroup, InputGroup, Intent } from "@blueprintjs/core";
import { ValidationResult } from "Tasks/TaskStore";
import { IconNames } from "@blueprintjs/icons";
import "./Style.scss";

interface Props {
  setValue?: (val: string) => void;
  value?: string;
  validationResult?: ValidationResult;
}

export const NameFilterContent = observer(({ setValue, value, validationResult }: Props) => {
  return (
    <FormGroup
      className="filter-options-input"
      labelFor="name-filter"
      helperText={validationResult?.message}
    >
      <InputGroup
        id="name-filter"
        value={value === null ? "" : value}
        intent={validationResult?.hasError ? Intent.DANGER : Intent.NONE}
        onChange={event => {
          setValue(event.currentTarget.value);
        }}
        rightElement={<Button icon={IconNames.CROSS} minimal onClick={() => setValue("")} />}
      />
    </FormGroup>
  );
});
