import { RadioGroup } from "@blueprintjs/core";
import React from "react";
import { IOptionProps } from "@blueprintjs/core/src/common/props";
import "./Style.scss";

interface IProps {
  setValue?: (val: string) => void;
  options: IOptionProps[];
  value?: string;
}

export const ScheduleFilterContent = ({ setValue, options, value }: IProps) => {
  return (
    <RadioGroup
      className="filter-options-radio"
      options={options}
      inline
      onChange={event => {
        setValue(event.currentTarget.value);
      }}
      selectedValue={value || ""}
    />
  );
};
