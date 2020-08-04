import React from "react";
import { CronSelectRow } from "Tasks/Components/ScheduleGridCell/Subcomponents";
import { CronsMultiselect } from "Shared/Components";
import "./Style.scss";
import { CronUnitType } from "Shared/Enums";
import { ICronsTagModel } from "Shared/Utils";

export interface IMultiSelectForm {
  label: string;
  selectProps: {
    type: CronUnitType;
    setValue: (val: ICronsTagModel[]) => void;
    values: ICronsTagModel[];
    isShouldClear: boolean;
  };
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
