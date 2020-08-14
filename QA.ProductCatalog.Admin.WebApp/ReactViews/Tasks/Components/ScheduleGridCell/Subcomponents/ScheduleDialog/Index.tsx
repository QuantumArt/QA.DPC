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
  taskId: number;
  hasSchedule: boolean;
  scheduleCronExpression: string;
  isOpen: boolean;
  closeDialogCb: () => void;
  scheduleEnabled: boolean;
}

export const ScheduleDialog = ({
  taskId,
  scheduleCronExpression,
  isOpen,
  closeDialogCb,
  scheduleEnabled
}: IProps) => {
  const store = useStore();
  const [taskSetType, setTaskSetType] = useState(ScheduleType.Repeat);
  const [isEnable, setIsEnable] = useState(scheduleEnabled);
  const [period, setPeriod] = useState(CronPeriodType.Minute);
  const [isShouldClear, setIsShouldClear] = useState(false);

  const [monthDays, setMonthDays] = useState<ICronsTagModel[] | undefined>();
  const [months, setMonths] = useState<ICronsTagModel[] | undefined>();
  const [weekDays, setWeekDays] = useState<ICronsTagModel[] | undefined>();
  const [hours, setHours] = useState<ICronsTagModel[] | undefined>();
  const [minutes, setMinutes] = useState<ICronsTagModel[] | undefined>();

  const [singleDate, setSingleDate] = useState<Date | undefined>(new Date());
  const [singleTime, setSingleTime] = useState<Date | undefined>(new Date());

  useEffect(
    () => {
      if (scheduleCronExpression && isOpen) {
        const cronParts = getValuesFromCronString(scheduleCronExpression);
        if (cronParts.cronParts.length === 5) {
          setPeriod(cronParts.period);
          setMinutes(partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Minutes), true));
          setHours(partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Hours), true));
          setMonthDays(
            partToString(cronParts.cronParts[2], UNITS.get(CronUnitType.MonthDays), true)
          );
          setMonths(partToString(cronParts.cronParts[3], UNITS.get(CronUnitType.Months), true));
          setWeekDays(partToString(cronParts.cronParts[4], UNITS.get(CronUnitType.WeekDays), true));
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
      if (isOpen) setIsEnable(scheduleEnabled);
    },
    [scheduleEnabled, isOpen]
  );

  const multiSelectPropsByUnit = useMemo(
    () =>
      new Map<CronUnitType, IMultiSelectForm>([
        [
          CronUnitType.MonthDays,
          {
            label: "День месяца",
            selectProps: {
              isShouldClear,
              parsedCronsModel: monthDays,
              type: CronUnitType.MonthDays,
              setParsedCronsModel: v => setMonthDays(v)
            }
          }
        ],
        [
          CronUnitType.Months,
          {
            label: "Месяц",
            selectProps: {
              isShouldClear,
              parsedCronsModel: months,
              type: CronUnitType.Months,
              setParsedCronsModel: v => setMonths(v)
            }
          }
        ],
        [
          CronUnitType.Hours,
          {
            label: "Часы",
            selectProps: {
              isShouldClear,
              parsedCronsModel: hours,
              type: CronUnitType.Hours,
              setParsedCronsModel: v => setHours(v)
            }
          }
        ],
        [
          CronUnitType.Minutes,
          {
            label: "Минуты",
            selectProps: {
              isShouldClear,
              parsedCronsModel: minutes,
              type: CronUnitType.Minutes,
              setParsedCronsModel: v => setMinutes(v)
            }
          }
        ],
        [
          CronUnitType.WeekDays,
          {
            label: "День недели",
            selectProps: {
              isShouldClear,
              parsedCronsModel: weekDays,
              type: CronUnitType.WeekDays,
              setParsedCronsModel: v => setWeekDays(v)
            }
          }
        ]
      ]),
    [isShouldClear, weekDays, setHours, hours, months, minutes, monthDays]
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
      minutes,
      hours,
      monthDays,
      months,
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
      store.setSchedule(taskId, isEnable, parsedCronsMultiSelectsModel(), "on");
    } else {
      store.setSchedule(taskId, isEnable, parseCronsSingleModel(), "on");
    }
    closeDialogCb();
  };

  const clearValues = (): void => {
    setMonthDays(undefined);
    setMonths(undefined);
    setWeekDays(undefined);
    setHours(undefined);
    setMinutes(undefined);
  };

  const periodItems = [
    { label: "Минуту", value: CronPeriodType.Minute },
    { label: "Час", value: CronPeriodType.Hour },
    { label: "День", value: CronPeriodType.Day },
    { label: "Неделю", value: CronPeriodType.Week },
    { label: "Месяц", value: CronPeriodType.Month },
    { label: "Год", value: CronPeriodType.Year }
  ];

  return (
    <div>
      <Dialog
        className="schedule-popup"
        icon="calendar"
        onClose={closeDialogCb}
        title="Расписание повторения задачи"
        isOpen={isOpen}
      >
        <div className={cn(Classes.DIALOG_BODY, "schedule-popup__body")}>
          <SelectRow label="ID задачи">
            <InputGroup disabled={true} value={String(taskId)} />
          </SelectRow>

          <SelectRow>
            <Switch
              alignIndicator={"left"}
              labelElement={"Расписание активно"}
              checked={isEnable}
              className="schedule-popup__switch"
              onChange={(): void => {
                setIsEnable(prevState => !prevState);
              }}
            />
          </SelectRow>

          <SelectRow label="Режим повторения">
            <HTMLSelect
              className="schedule-popup__select"
              iconProps={{ icon: "caret-down" }}
              value={taskSetType}
              onChange={event => {
                setTaskSetType(event.target.value as ScheduleType);
                clearValues();
              }}
              options={[
                { label: "Повторять", value: ScheduleType.Repeat },
                { label: "Одноразовое", value: ScheduleType.Single }
              ]}
            />
          </SelectRow>

          {taskSetType === ScheduleType.Repeat && (
            <SelectRow label="Период повторения">
              <>
                <span className="just-label">Каждый(ую)</span>
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
                  outlined="true"
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
        </div>

        <div className={Classes.DIALOG_FOOTER}>
          <div className={Classes.DIALOG_FOOTER_ACTIONS}>
            <Button onClick={closeDialogCb}>Закрыть</Button>
            <AnchorButton intent={Intent.PRIMARY} onClick={acceptSchedule}>
              Принять
            </AnchorButton>
          </div>
        </div>
      </Dialog>
    </div>
  );
};
