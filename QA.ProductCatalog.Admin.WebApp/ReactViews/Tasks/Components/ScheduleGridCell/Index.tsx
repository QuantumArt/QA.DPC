import {
  AnchorButton,
  Button,
  Classes,
  Dialog,
  HTMLSelect,
  Icon,
  InputGroup,
  Intent,
  Switch
} from "@blueprintjs/core";
import { DateInput, TimePicker } from "@blueprintjs/datetime";
import React, { useEffect, useMemo, useState } from "react";
import cn from "classnames";
import { Position } from "@blueprintjs/core/lib/esm/common/position";
import { IMultiSelectForm, MultiSelectFormGroup, SelectRow } from "./Subcomponents";
import { ICronsTagModel, partToString, getValuesFromCronString, UNITS } from "Shared/Utils";
import { useStore } from "Tasks/UseStore";
import "@blueprintjs/datetime/lib/css/blueprint-datetime.css";
import "./Style.scss";
import { CronPeriodType, CronUnitType } from "Shared/Enums";

interface IProps {
  taskId: number;
  hasSchedule: boolean;
  scheduleCronExpression: string;
}

export const ScheduleGridCell = ({ taskId, hasSchedule, scheduleCronExpression }: IProps) => {
  const store = useStore();
  const [isOpen, setIsOpen] = useState(false);
  const [taskSetType, setTaskSetType] = useState("repeat");
  const [isEnable, setIsEnable] = useState(hasSchedule);
  const [period, setPeriod] = useState(CronPeriodType.Minute);
  const [isShouldClear, setIsShouldClear] = useState(false);

  const [monthDays, setMonthDays] = useState<ICronsTagModel[] | undefined>();
  const [months, setMonths] = useState<ICronsTagModel[] | undefined>();
  const [weekDays, setWeekDays] = useState<ICronsTagModel[] | undefined>();
  const [hours, setHours] = useState<ICronsTagModel[] | undefined>();
  const [minutes, setMinutes] = useState<ICronsTagModel[] | undefined>();

  const [singleDate, setSingleDate] = useState<Date | undefined>(new Date());
  const [singleTime, setSingleTime] = useState<Date | undefined>(new Date());

  const parsedCronsMultiSelectsModel = () => {
    const models = [monthDays, months, weekDays, hours, minutes];
    const ModelParser = (model: ICronsTagModel[]): string => {
      if (!model || !model.length) return "*";
      return model
        .map(val => (val.label === "Все" ? "*" : val.label))
        .join(",")
        .trim();
    };
    return models.map(ModelParser).join("+");
  };

  const parseCronsSingleModel = () => {
    return `${singleTime.getMinutes()}+${singleTime.getHours()}+${singleDate.getDate()}+${singleDate.getMonth() +
      1}+?+${singleDate.getFullYear()}`;
  };

  const acceptSchedule = (): void => {
    if (taskSetType === "repeat") {
      store.setSchedule(taskId, isEnable, parsedCronsMultiSelectsModel(), "on");
    } else {
      store.setSchedule(taskId, isEnable, parseCronsSingleModel(), "on");
    }
  };

  const multiSelectPropsByUnit = useMemo(
    () =>
      new Map<CronUnitType, IMultiSelectForm>([
        [
          CronUnitType.MonthDays,
          {
            label: "День месяца",
            selectProps: {
              isShouldClear,
              values: monthDays,
              type: CronUnitType.MonthDays,
              setValue: v => setMonthDays(v)
            }
          }
        ],
        [
          CronUnitType.Months,
          {
            label: "Месяц",
            selectProps: {
              isShouldClear,
              values: months,
              type: CronUnitType.Months,
              setValue: v => setMonths(v)
            }
          }
        ],
        [
          CronUnitType.Hours,
          {
            label: "Часы",
            selectProps: {
              isShouldClear,
              values: hours,
              type: CronUnitType.Hours,
              setValue: v => setHours(v)
            }
          }
        ],
        [
          CronUnitType.Minutes,
          {
            label: "Минуты",
            selectProps: {
              isShouldClear,
              values: minutes,
              type: CronUnitType.Minutes,
              setValue: v => setMinutes(v)
            }
          }
        ],
        [
          CronUnitType.WeekDays,
          {
            label: "День недели",
            selectProps: {
              isShouldClear,
              values: weekDays,
              type: CronUnitType.WeekDays,
              setValue: v => setWeekDays(v)
            }
          }
        ]
      ]),
    [isShouldClear, weekDays, setHours, hours, months, minutes]
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
    [period]
  );

  useEffect(
    () => {
      const initSchedule = () => {
        if (hasSchedule && scheduleCronExpression) {
          const cronParts = getValuesFromCronString(scheduleCronExpression);
          setPeriod(cronParts.period);
          setMinutes(partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Minutes), true));
          setHours(partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Hours), true));
          setMonthDays(
            partToString(cronParts.cronParts[2], UNITS.get(CronUnitType.MonthDays), true)
          );
          setMonths(partToString(cronParts.cronParts[3], UNITS.get(CronUnitType.Months), true));
          setWeekDays(partToString(cronParts.cronParts[4], UNITS.get(CronUnitType.WeekDays), true));
        }
      };
      initSchedule();
    },
    [hasSchedule, scheduleCronExpression]
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
    [isShouldClear, setIsShouldClear]
  );

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
      {hasSchedule && scheduleCronExpression}
      <Icon icon="calendar" intent={Intent.PRIMARY} onClick={() => setIsOpen(true)} />
      <Dialog
        className="schedule-popup"
        icon="calendar"
        onClose={() => setIsOpen(false)}
        title="Расписание повторения задачи"
        isOpen={isOpen}
      >
        <div className={cn(Classes.DIALOG_BODY, "schedule-popup__body")}>
          <SelectRow label="ID задачи">
            <InputGroup disabled={true} value={"123"} />
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
                setTaskSetType(event.target.value);
                clearValues();
              }}
              options={[
                { label: "Повторять", value: "repeat" },
                { label: "Одноразовое", value: "onetime" }
              ]}
            />
          </SelectRow>

          {taskSetType === "repeat" && (
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
                    setPeriod((event.target.value as unknown) as CronPeriodType);
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
                  {!isShouldClear && <MultiSelectFormGroup selectProps={getMultiSelectProps} />}
                </div>
              </>
            </SelectRow>
          )}

          {taskSetType === "onetime" && (
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
            <Button onClick={() => setIsOpen(false)}>Закрыть</Button>
            <AnchorButton intent={Intent.PRIMARY} onClick={acceptSchedule}>
              Принять
            </AnchorButton>
          </div>
        </div>
      </Dialog>
    </div>
  );
};
