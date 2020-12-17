import React, { useEffect, useMemo, useState, memo } from "react";
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
import { Position } from "@blueprintjs/core/lib/esm/common/position";
import { IconNames } from "@blueprintjs/icons";
import _ from "lodash";
import moment from "moment";
import MomentLocaleUtils from "react-day-picker/moment";
import cn from "classnames";
import { IMultiSelectForm, MultiSelectFormGroup, SelectRow } from "../";
import {
  ICronsTagModel,
  partToString,
  getValuesFromCronString,
  UNITS,
  getUnitByIndex
} from "Tasks/Utils";
import { useStore } from "Tasks/UseStore";
import { CronPeriodType, CronUnitType, ScheduleType } from "Shared/Enums";
import { l } from "Tasks/Localization";
import "./Style.scss";
import { ConfirmationDialog } from "./ConfirmationDialog";

interface IProps {
  taskIdNumber: number;
  scheduleCronExpression: string;
  isOpen: boolean;
  closeDialogCb: () => void;
  isScheduleEnabled: boolean;
}

const recurrenceOptions = [
  { label: l("modeRepeat"), value: ScheduleType.Repeat },
  { label: l("modeOneTime"), value: ScheduleType.Single }
];

const periodItems = [
  { label: l("minute"), value: CronPeriodType.Minute },
  { label: l("hour"), value: CronPeriodType.Hour },
  { label: l("day"), value: CronPeriodType.Day },
  { label: l("week"), value: CronPeriodType.Week },
  { label: l("month"), value: CronPeriodType.Month },
  { label: l("year"), value: CronPeriodType.Year }
];

export const ScheduleDialog = ({
  taskIdNumber,
  scheduleCronExpression,
  isOpen,
  closeDialogCb,
  isScheduleEnabled
}: IProps) => {
  const store = useStore();

  const [taskSetType, setTaskSetType] = useState<ScheduleType>(ScheduleType.Repeat);
  const [isEnable, setIsEnable] = useState<boolean>(isScheduleEnabled);
  const [period, setPeriod] = useState<CronPeriodType>(CronPeriodType.Minute);
  const [isShouldClear, setIsShouldClear] = useState<boolean>(false);
  const [isCronsParseError, setIsCronsParseError] = useState<boolean>(false);

  const [monthDaysValues, setMonthDaysValues] = useState<ICronsTagModel[] | undefined>();
  const [monthValues, setMonthValues] = useState<ICronsTagModel[] | undefined>();
  const [weekDaysValues, setWeekDaysValues] = useState<ICronsTagModel[] | undefined>();
  const [hourValues, setHourValues] = useState<ICronsTagModel[] | undefined>();
  const [minuteValues, setMinuteValues] = useState<ICronsTagModel[] | undefined>();

  const [singleDate, setSingleDate] = useState<Date | undefined>(new Date());
  const [singleTime, setSingleTime] = useState<Date | undefined>(new Date());

  const [confirmationDialog, setConfirmationDialog] = useState<boolean>(false);

  useEffect(() => {
    if (scheduleCronExpression && isOpen) {
      try {
        setIsCronsParseError(false);
        setIsEnable(isScheduleEnabled);
        const cronParts = getValuesFromCronString(scheduleCronExpression);
        if (cronParts.cronParts.length === 5) {
          setPeriod(cronParts.period);
          setMinuteValues(
            partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Minutes), true)
          );
          setHourValues(partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Hours), true));
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
  }, [scheduleCronExpression, isOpen]);

  useEffect(() => {
    setIsShouldClear(isShouldClear => {
      return isShouldClear ? !isShouldClear : isShouldClear;
    });
  }, []);

  const multiSelectPropsByUnit = useMemo(
    () =>
      new Map<CronUnitType, IMultiSelectForm>([
        [
          CronUnitType.MonthDays,
          {
            label: l("monthDays"),
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
            label: l("months"),
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
            label: l("hours"),
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
            label: l("minutes"),
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
            label: l("weekDays"),
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

  const getMultiSelectProps = useMemo((): IMultiSelectForm[] => {
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
  }, [period, multiSelectPropsByUnit]);

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
          val.label === l("all")
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

  const acceptSchedule = async () => {
    store.toggleLoading(true);
    if (taskSetType === ScheduleType.Repeat) {
      await store.setSchedule(taskIdNumber, isEnable, parsedCronsMultiSelectsModel(), "on");
    } else {
      await store.setSchedule(taskIdNumber, isEnable, parseCronsSingleModel(), "on");
    }
    closeDialogCb();
    store.toggleLoading(false);
  };

  const deleteSchedule = async () => {
    store.toggleLoading(true);
    await store.setSchedule(taskIdNumber, false, "", "off");
    setConfirmationDialog(false);
    closeDialogCb();
    store.toggleLoading(false);
  };

  const clearValues = (): void => {
    setMonthDaysValues(undefined);
    setMonthValues(undefined);
    setWeekDaysValues(undefined);
    setHourValues(undefined);
    setMinuteValues(undefined);
  };

  const renderSelectsUiPart = () => {
    if (isCronsParseError) return l("cronParseError");
    return (
      <>
        <SelectRow>
          <Switch
            alignIndicator={"left"}
            labelElement={l("scheduleEnabled")}
            checked={isEnable}
            className="schedule-popup__switch"
            onChange={(): void => {
              setIsEnable(prevState => !prevState);
            }}
          />
        </SelectRow>

        <SelectRow label={l("recurrenceMode")}>
          <HTMLSelect
            className="schedule-popup__select"
            iconProps={{ icon: "caret-down" }}
            value={taskSetType}
            onChange={event => {
              setTaskSetType(event.target.value as ScheduleType);
              clearValues();
            }}
            options={recurrenceOptions}
          />
        </SelectRow>

        {taskSetType === ScheduleType.Repeat && (
          <SelectRow label={l("recurrencePeriod")}>
            <>
              <span className="just-label">{l("every")}</span>
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
          <SelectRow>
            <DateInput
              className="schedule-popup__select--inline"
              formatDate={date =>
                moment(date)
                  .locale(window.task.locale)
                  .format("DD.MM.YYYY")
              }
              parseDate={str =>
                moment(str, "DD.MM.YYYY")
                  .locale(window.task.locale)
                  .toDate()
              }
              popoverProps={{ position: Position.BOTTOM }}
              value={singleDate}
              highlightCurrentDay
              onChange={(selectedDate: Date) => {
                setSingleDate(selectedDate);
              }}
              maxDate={moment()
                .add(6, "month")
                .toDate()}
              locale={window.task.locale}
              localeUtils={MomentLocaleUtils}
            />
            <TimePicker
              value={singleTime}
              onChange={(newTime: Date) => {
                setSingleTime(newTime);
              }}
              className="schedule-popup__select--inline"
            />
          </SelectRow>
        )}
      </>
    );
  };

  return (
    <Dialog
      className="schedule-popup"
      icon={IconNames.CALENDAR}
      onClose={closeDialogCb}
      title={l("taskRecurrenceSchedule")}
      isOpen={isOpen}
    >
      <div className={cn(Classes.DIALOG_BODY, "schedule-popup__body")}>
        <SelectRow label={l("taskId")}>
          <InputGroup disabled={true} value={String(taskIdNumber)} />
        </SelectRow>
        {renderSelectsUiPart()}
      </div>
      <div className={Classes.DIALOG_FOOTER}>
        <div className={Classes.DIALOG_FOOTER_ACTIONS}>
          {scheduleCronExpression !== null && (
            <Button
              style={{ marginLeft: 0, marginRight: "auto" }}
              intent={Intent.DANGER}
              icon={IconNames.REMOVE}
              onClick={() => setConfirmationDialog(true)}
              loading={store.isLoading}
            >
              {l("deleteSchedule")}
            </Button>
          )}
          {!isCronsParseError && (
            <Button
              intent={Intent.PRIMARY}
              onClick={acceptSchedule}
              icon={IconNames.CONFIRM}
              loading={store.isLoading}
            >
              {l("apply")}
            </Button>
          )}
          <ConfirmationDialog
            isOpen={confirmationDialog}
            isLoading={store.isLoading}
            confirmAction={deleteSchedule}
            declineAction={() => setConfirmationDialog(false)}
          />
        </div>
      </div>
    </Dialog>
  );
};
