import React from "react";
import { CronSelectRow } from "Tasks/Components/ScheduleGridCell/Subcomponents";
import { CronsMultiselect, ICronsMultiSelectProps } from "Shared/Components";
import "./Style.scss";

export interface IMultiSelectForm {
  label: string;
  selectProps: ICronsMultiSelectProps;
}

export const MultiSelectFormGroup = ({ selectProps }: { selectProps: IMultiSelectForm[] }) => {
  return (
    <>
      {selectProps.map(multiSelectProps => {
        return (
          <CronSelectRow key={multiSelectProps.label} label={multiSelectProps.label}>
            <CronsMultiselect {...multiSelectProps.selectProps} />
          </CronSelectRow>
        );
      })}
    </>
  );
};
