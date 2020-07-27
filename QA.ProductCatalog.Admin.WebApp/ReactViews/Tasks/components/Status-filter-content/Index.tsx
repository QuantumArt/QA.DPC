import { HTMLSelect } from "@blueprintjs/core";
import React from "react";
import { IOptionProps } from "@blueprintjs/core/src/common/props";
import "./Style.scss";

interface IProps {
  setValue?: (val: any) => void;
  options: Array<string | number | IOptionProps>;
  value?: number;
}

export const StatusFilterContent = ({ setValue, options, value }: IProps) => {
  return (
    <HTMLSelect
      className="filter-options-select"
      iconProps={{ icon: "caret-down", style: { top: "10px" } }}
      value={value}
      onChange={event => {
        setValue(event.currentTarget.value);
      }}
      options={options}
    />
  );
};
