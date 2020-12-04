import { HTMLSelect } from "@blueprintjs/core";
import React from "react";
import { IOptionProps } from "@blueprintjs/core/src/common/props";
import "./Style.scss";

interface IProps {
  setValue?: (val: string) => void;
  options: Array<string | number | IOptionProps>;
  value?: number;
}

export const StatusFilterContent = ({ setValue, options, value }: IProps) => {
  return (
    <HTMLSelect
      className="filter-options-select"
      iconProps={{ icon: "caret-down" }}
      value={value}
      onChange={event => {
        setValue(event.currentTarget.value);
      }}
      options={options}
    />
  );
};
