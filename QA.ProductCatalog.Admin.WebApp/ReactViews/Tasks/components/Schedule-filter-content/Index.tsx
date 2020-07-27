import { RadioGroup } from "@blueprintjs/core";
import React from "react";
import { IOptionProps } from "@blueprintjs/core/src/common/props";
import "./Style.scss";

interface IProps {
  setValue?: (val: any) => void;
  options: IOptionProps[];
  value?: number;
}

export const ScheduleFilterContent = ({ setValue, options, value }: IProps) => {
  return (
    <RadioGroup
      className="filter-options-radio"
      options={options}
      value={value}
      inline
      onChange={event => {
        setValue(event.currentTarget.value);
      }}
      selectedValue={value}
    />
  );
};
