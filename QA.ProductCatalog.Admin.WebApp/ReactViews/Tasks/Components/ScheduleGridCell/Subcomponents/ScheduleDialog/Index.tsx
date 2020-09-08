import {
  AnchorButton,
  Button,
  Classes,
  Dialog,
  HTMLSelect,
  InputGroup,
  Intent,
  Switch
} from "@blueprintjs/core";
import { DateInput, TimePicker } from "@blueprintjs/datetime";
import React, { useEffect, useMemo, useState } from "react";
import cn from "classnames";
import { Position } from "@blueprintjs/core/lib/esm/common/position";
import { IMultiSelectForm, MultiSelectFormGroup, SelectRow } from "../";
import {
  ICronsTagModel,
  partToString,
  getValuesFromCronString,
  UNITS,
  getUnitByIndex
} from "Shared/Utils";
import { useStore } from "Tasks/UseStore";
import { CronPeriodType, CronUnitType, ScheduleType } from "Shared/Enums";
import "@blueprintjs/datetime/lib/css/blueprint-datetime.css";
import _ from "lodash";
import "./Style.scss";

interface IProps {
  taskIdNumber: number;
  hasSchedule: boolean;
  scheduleCronExpression: string;
  isOpen: boolean;
  closeDialogCb: () => void;
  isScheduleEnabled: boolean;
}

export const ScheduleDialog = ({
  taskIdNumber,
  scheduleCronExpression,
  isOpen,
  closeDialogCb,
  isScheduleEnabled
}: IProps) => {
  const store = useStore();
  const {
    taskRecurrenceSchedule,
    weekDays,
    monthDays,
    hours,
    minutes,
    minute,
    hour,
    day,
    year,
    recurrenceMode,
    modeRepeat,
    modeOneTime,
    recurrencePeriod,
    every,
    taskId,
    months,
    month,
    scheduleEnabled,
    close,
    apply
  } = window.task.schedule;
  const [taskSetType, setTaskSetType] = useState(ScheduleType.Repeat);
  const [isEnable, setIsEnable] = useState(isScheduleEnabled);
  const [period, setPeriod] = useState(CronPeriodType.Minute);
  const [isShouldClear, setIsShouldClear] = useState(false);
  const [isCronsParseError, setIsCronsParseError] = useState(false);

  const [monthDaysValues, setMonthDaysValues] = useState<ICronsTagModel[] | undefined>();
  const [monthValues, setMonthValues] = useState<ICronsTagModel[] | undefined>();
  const [weekDaysValues, setWeekDaysValues] = useState<ICronsTagModel[] | undefined>();
  const [hourValues, setHourValues] = useState<ICronsTagModel[] | undefined>();
  const [minuteValues, setMinuteValues] = useState<ICronsTagModel[] | undefined>();

  const [singleDate, setSingleDate] = useState<Date | undefined>(new Date());
  const [singleTime, setSingleTime] = useState<Date | undefined>(new Date());

  useEffect(
    () => {
      if (scheduleCronExpression && isOpen) {
        try {
          setIsCronsParseError(false);
          const cronParts = getValuesFromCronString(scheduleCronExpression);
          if (cronParts.cronParts.length === 5) {
            setPeriod(cronParts.period);
            setMinuteValues(
              partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Minutes), true)
            );
            setHourValues(
              partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Hours), true)
            );
            setMonthDaysValues(
              partToString(cronParts.cronParts[2], UNITS.get(CronUnitType.MonthDays), true)
            );
            setMonthValues(
              partToString(cronParts.cronParts[3], UNITS.get(CronUnitType.Months), true)
            );
            setWeekDaysValues(
              partToString(cronParts.cronParts[4], UNITS.get(CronUnitType.WeekDays), true)
            );
          }
          if (cronParts.cronParts.length === 6) {
            setTaskSetType(ScheduleType.Single);
            const date = cronParts.cronParts[2][0];
            const month = cronParts.cronParts[3][0];
            const year = cronParts.cronParts[5][0];
            const hours = cronParts.cronParts[1][0];
            const mins = cronParts.cronParts[0][0];
            const dd = new Date();
            dd.setDate(date);
            dd.setMonth(month - 1);
            dd.setFullYear(year);
            dd.setHours(hours);
            dd.setMinutes(mins);
            setSingleDate(dd);
            setSingleTime(dd);
          }
        } catch (e) {
          setIsCronsParseError(true);
        }
      }
    },
    [scheduleCronExpression, isOpen]
  );

  useEffect(
    () => {
      const clear = () => {
        if (isShouldClear) {
          setIsShouldClear(false);
        }
      };
      clear();
    },
    [isShouldClear]
  );

  useEffect(
    () => {
      if (isOpen) setIsEnable(isScheduleEnabled);
    },
    [isScheduleEnabled, isOpen]
  );

  const multiSelectPropsByUnit = useMemo(
    () =>
      new Map<CronUnitType, IMultiSelectForm>([
        [
          CronUnitType.MonthDays,
          {
            label: monthDays,
            selectProps: {
              isShouldClear,
              parsedCronsModel: monthDaysValues,
              type: CronUnitType.MonthDays,
              setParsedCronsModel: v => setMonthDaysValues(v)
            }
          }
        ],
        [
          CronUnitType.Months,
          {
            label: months,
            selectProps: {
              isShouldClear,
              parsedCronsModel: monthValues,
              type: CronUnitType.Months,
              setParsedCronsModel: v => setMonthValues(v)
            }
          }
        ],
        [
          CronUnitType.Hours,
          {
            label: hours,
            selectProps: {
              isShouldClear,
              parsedCronsModel: hourValues,
              type: CronUnitType.Hours,
              setParsedCronsModel: v => setHourValues(v)
            }
          }
        ],
        [
          CronUnitType.Minutes,
          {
            label: minutes,
            selectProps: {
              isShouldClear,
              parsedCronsModel: minuteValues,
              type: CronUnitType.Minutes,
              setParsedCronsModel: v => setMinuteValues(v)
            }
          }
        ],
        [
          CronUnitType.WeekDays,
          {
            label: weekDays,
            selectProps: {
              isShouldClear,
              parsedCronsModel: weekDaysValues,
              type: CronUnitType.WeekDays,
              setParsedCronsModel: v => setWeekDaysValues(v)
            }
          }
        ]
      ]),
    [
      isShouldClear,
      weekDaysValues,
      setHourValues,
      hourValues,
      monthValues,
      minuteValues,
      monthDaysValues
    ]
  );

  const getMultiSelectProps = useMemo(
    (): IMultiSelectForm[] => {
      switch (period) {
        case CronPeriodType.Week:
          return [
            multiSelectPropsByUnit.get(CronUnitType.WeekDays),
            multiSelectPropsByUnit.get(CronUnitType.Hours),
            multiSelectPropsByUnit.get(CronUnitType.Minutes)
          ];
        case CronPeriodType.Year:
          return [
            multiSelectPropsByUnit.get(CronUnitType.MonthDays),
            multiSelectPropsByUnit.get(CronUnitType.Months),
            multiSelectPropsByUnit.get(CronUnitType.Hours),
            multiSelectPropsByUnit.get(CronUnitType.Minutes)
          ];
        case CronPeriodType.Day:
          return [
            multiSelectPropsByUnit.get(CronUnitType.Hours),
            multiSelectPropsByUnit.get(CronUnitType.Minutes)
          ];
        case CronPeriodType.Hour:
          return [multiSelectPropsByUnit.get(CronUnitType.Minutes)];
        case CronPeriodType.Month:
          return [
            multiSelectPropsByUnit.get(CronUnitType.MonthDays),
            multiSelectPropsByUnit.get(CronUnitType.Hours),
            multiSelectPropsByUnit.get(CronUnitType.Minutes)
          ];
        default:
          return [];
      }
    },
    [period, multiSelectPropsByUnit]
  );

  const parsedCronsMultiSelectsModel = () => {
    /** don't change the order of array*/
    const apiRequestModel: ReadonlyArray<ICronsTagModel[]> = [
      minuteValues,
      hourValues,
      monthDaysValues,
      monthValues,
      []
    ];

    const ModelParser = (model: ICronsTagModel[]): string => {
      if (!model || !model.length) return "*";
      return model
        .map((val, index) =>
          val.label === "Все"
            ? "*"
            : partToString(
                _.flatten(model.map(x => x.values)).sort(),
                UNITS.get(getUnitByIndex(index)),
                false
              ).map(x => x.label)
        )
        .join(",")
        .trim();
    };
    return apiRequestModel.map(ModelParser).join(" ");
  };

  const parseCronsSingleModel = () => {
    return `${singleTime.getMinutes()} ${singleTime.getHours()} ${singleDate.getDate()} ${singleDate.getMonth() +
      1} ? ${singleDate.getFullYear()}`;
  };

  const acceptSchedule = (): void => {
    if (taskSetType === "repeat") {
      store.setSchedule(taskIdNumber, isEnable, parsedCronsMultiSelectsModel(), "on");
    } else {
      store.setSchedule(taskIdNumber, isEnable, parseCronsSingleModel(), "on");
    }
    closeDialogCb();
  };

  const clearValues = (): void => {
    setMonthDaysValues(undefined);
    setMonthValues(undefined);
    setWeekDaysValues(undefined);
    setHourValues(undefined);
    setMinuteValues(undefined);
  };

  const periodItems = [
    { label: minute, value: CronPeriodType.Minute },
    { label: hour, value: CronPeriodType.Hour },
    { label: day, value: CronPeriodType.Day },
    { label: "Неделю", value: CronPeriodType.Week },
    { label: month, value: CronPeriodType.Month },
    { label: year, value: CronPeriodType.Year }
  ];

  const renderSelectsUiPart = () => {
    if (isCronsParseError) return "Ошибка обработки cronExpression";
    return (
      <>
        <SelectRow>
          <Switch
            alignIndicator={"left"}
            labelElement={scheduleEnabled}
            checked={isEnable}
            className="schedule-popup__switch"
            onChange={(): void => {
              setIsEnable(prevState => !prevState);
            }}
          />
        </SelectRow>

        <SelectRow label={recurrenceMode}>
          <HTMLSelect
            className="schedule-popup__select"
            iconProps={{ icon: "caret-down" }}
            value={taskSetType}
            onChange={event => {
              setTaskSetType(event.target.value as ScheduleType);
              clearValues();
            }}
            options={[
              { label: modeRepeat, value: ScheduleType.Repeat },
              { label: modeOneTime, value: ScheduleType.Single }
            ]}
          />
        </SelectRow>

        {taskSetType === ScheduleType.Repeat && (
          <SelectRow label={recurrencePeriod}>
            <>
              <span className="just-label">{every}</span>
              {"   "}
              <HTMLSelect
                className="schedule-popup__select schedule-popup__select--inline"
                iconProps={{ icon: "caret-down" }}
                value={period}
                onChange={event => {
                  clearValues();
                  setPeriod(event.target.value as CronPeriodType);
                }}
                options={periodItems}
              />
              <Button
                icon="cross"
                outlined
                onClick={() => {
                  setIsShouldClear(true);
                  clearValues();
                }}
              />
              <div className="schedule-popup__selects">
                <MultiSelectFormGroup selectProps={getMultiSelectProps} />
              </div>
            </>
          </SelectRow>
        )}

        {taskSetType === ScheduleType.Single && (
          <SelectRow label="Дата и время">
            <>
              <DateInput
                className="schedule-popup__select--inline"
                formatDate={date => (date == null ? "" : date.toLocaleDateString())}
                parseDate={str => new Date(Date.parse(str))}
                popoverProps={{ position: Position.BOTTOM }}
                value={singleDate}
                onChange={(selectedDate: Date) => {
                  setSingleDate(selectedDate);
                }}
              />
              <TimePicker
                value={singleTime}
                onChange={(newTime: Date) => {
                  setSingleTime(newTime);
                }}
                className="schedule-popup__select--inline"
              />
            </>
          </SelectRow>
        )}
      </>
    );
  };

  const renderDialogButtons = () => {
    return (
      <div className={Classes.DIALOG_FOOTER_ACTIONS}>
        <Button onClick={closeDialogCb}>{close}</Button>
        {!isCronsParseError && (
          <AnchorButton intent={Intent.PRIMARY} onClick={acceptSchedule}>
            {apply}
          </AnchorButton>
        )}
      </div>
    );
  };

  return (
    <div>
      <Dialog
        className="schedule-popup"
        icon="calendar"
        onClose={closeDialogCb}
        title={taskRecurrenceSchedule}
        isOpen={isOpen}
      >
        <div className={cn(Classes.DIALOG_BODY, "schedule-popup__body")}>
          <SelectRow label={taskId}>
            <InputGroup disabled={true} value={String(taskIdNumber)} />
          </SelectRow>

          {renderSelectsUiPart()}
        </div>

        <div className={Classes.DIALOG_FOOTER}>{renderDialogButtons()}</div>
      </Dialog>
    </div>
  );
};
