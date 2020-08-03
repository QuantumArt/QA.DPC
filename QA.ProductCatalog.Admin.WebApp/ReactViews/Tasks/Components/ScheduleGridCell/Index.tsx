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
import React, { useEffect, useState } from "react";
import cn from "classnames";
import { Position } from "@blueprintjs/core/lib/esm/common/position";
import { CronsMultiselect } from "Shared/Components";
import { CronSelectRow, SelectRow } from "./Subcomponents";
import {
  ICronsTagModel,
  partToString,
  PeriodType,
  getValuesFromCronString,
  UNITS
} from "Shared/Utils";
import { useStore } from "Tasks/UseStore";
import "@blueprintjs/datetime/lib/css/blueprint-datetime.css";
import "./Style.scss";
import { CronUnitType } from "Shared/Enums";

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
  const [period, setPeriod] = useState("minute" as PeriodType);

  const [monthDays, setMonthDays] = useState<ICronsTagModel[] | undefined>();
  const [months, setMonths] = useState<ICronsTagModel[] | undefined>();
  const [weekDays, setWeekDays] = useState<ICronsTagModel[] | undefined>();
  const [hours, setHours] = useState<ICronsTagModel[] | undefined>();
  const [minutes, setMinutes] = useState<ICronsTagModel[] | undefined>();

  const parsedCronsModel = () => {
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

  const acceptSchedule = (): void => {
    store.setSchedule(taskId, isEnable, parsedCronsModel(), "on");
  };
  //
  // const getMultiSelectProps = (): {
  //
  // } => {
  //
  // }

  useEffect(
    () => {
      const initSchedule = () => {
        if (hasSchedule && scheduleCronExpression) {
          const cronParts = getValuesFromCronString(scheduleCronExpression);
          setPeriod(cronParts.period);
          setHours(partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Hours), true));
          setMinutes(partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Minutes), true));
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

  const clearValues = (): void => {
    setMonthDays(undefined);
    setMonths(undefined);
    setWeekDays(undefined);
    setHours(undefined);
    setMinutes(undefined);
  };

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
              defaultChecked={false}
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

          <SelectRow label="Период повторения">
            {taskSetType === "repeat" && (
              <>
                <span className="just-label">Каждый(ую)</span>
                {"   "}
                <HTMLSelect
                  className="schedule-popup__select schedule-popup__select--inline"
                  iconProps={{ icon: "caret-down" }}
                  value={period}
                  onChange={event => {
                    setPeriod(event.target.value as PeriodType);
                    clearValues();
                  }}
                  options={[
                    { label: "Минуту", value: "minute" },
                    { label: "Час", value: "hour" },
                    { label: "День", value: "day" },
                    { label: "Неделю", value: "week" },
                    { label: "Месяц", value: "month" },
                    { label: "Год", value: "year" }
                  ]}
                />
                <div className="schedule-popup__selects">
                  {period === "hour" && (
                    <CronsMultiselect
                      values={minutes}
                      setValue={v => setMinutes(v)}
                      type={CronUnitType.Minutes}
                    />
                  )}
                  {period === "year" && (
                    <>
                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">День месяца</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={monthDays}
                            type={CronUnitType.MonthDays}
                            setValue={v => setMonthDays(v)}
                          />
                        </div>
                      </div>

                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Месяц</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={months}
                            type={CronUnitType.Months}
                            setValue={v => setMonths(v)}
                          />
                        </div>
                      </div>
                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Часы</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={hours}
                            type={CronUnitType.Hours}
                            setValue={v => setHours(v)}
                          />
                        </div>
                      </div>
                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Минуты</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={minutes}
                            type={CronUnitType.Minutes}
                            setValue={v => setMinutes(v)}
                          />
                        </div>
                      </div>
                    </>
                  )}
                  {period === "month" && (
                    <>
                      <CronSelectRow label="День месяца">
                        <CronsMultiselect
                          values={monthDays}
                          type={CronUnitType.MonthDays}
                          setValue={v => setMonthDays(v)}
                        />
                      </CronSelectRow>

                      <CronSelectRow label="Часы">
                        <CronsMultiselect
                          values={hours}
                          type={CronUnitType.Hours}
                          setValue={v => setHours(v)}
                        />
                      </CronSelectRow>

                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Минуты</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={minutes}
                            type={CronUnitType.Minutes}
                            setValue={v => setMinutes(v)}
                          />
                        </div>
                      </div>
                    </>
                  )}
                  {period === "day" && (
                    <>
                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Часы</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={hours}
                            type={CronUnitType.Hours}
                            setValue={v => setHours(v)}
                          />
                        </div>
                      </div>
                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Минуты</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={minutes}
                            type={CronUnitType.Minutes}
                            setValue={v => setMinutes(v)}
                          />
                        </div>
                      </div>
                    </>
                  )}
                  {period === "week" && (
                    <>
                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">День недели</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={weekDays}
                            type={CronUnitType.WeekDays}
                            setValue={v => setWeekDays(v)}
                          />
                        </div>
                      </div>

                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Часы</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={hours}
                            type={CronUnitType.Hours}
                            setValue={v => setHours(v)}
                          />
                        </div>
                      </div>

                      <div className="schedule-popup__select-wrap">
                        <div className="schedule-popup__label-row">Минута</div>
                        <div className="schedule-popup__select-row">
                          <CronsMultiselect
                            values={minutes}
                            type={CronUnitType.Minutes}
                            setValue={v => setMinutes(v)}
                          />
                        </div>
                      </div>
                    </>
                  )}
                </div>
              </>
            )}

            {taskSetType === "onetime" && (
              <>
                <div className="select-label">Дата и время</div>
                <DateInput
                  className="schedule-popup__select--inline"
                  defaultValue={new Date()} // onChange={this.handleDateChange}
                  formatDate={date => (date == null ? "" : date.toLocaleDateString())}
                  parseDate={str => new Date(Date.parse(str))}
                  popoverProps={{ position: Position.BOTTOM }}
                />
                <TimePicker className="schedule-popup__select--inline" />
              </>
            )}
          </SelectRow>
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
