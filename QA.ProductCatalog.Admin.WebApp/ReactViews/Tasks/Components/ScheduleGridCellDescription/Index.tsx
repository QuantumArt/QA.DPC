import React from "react";
import {
  getDateValueWithZeroAhead,
  getValuesFromCronString,
  ICronsTagModel,
  partToString,
  UNITS
} from "Shared/Utils";
import { CronPeriodType, CronUnitType } from "Shared/Enums";
import "./style.scss";

interface IProps {
  cronExpression: string;
}

const getStringByPeriod = (
  preiodType: CronPeriodType,
  monthDays: string,
  months: string,
  weekDays: string,
  hours: string,
  minutes: string
): string => {
  const { every, year, day, minute, hour, at, month, on, minutesPastHour } = window.task.schedule;

  let label = "";
  switch (preiodType) {
    case CronPeriodType.Week:
      label += `${every} неделю${weekDays}${hours}:${minutes}`;
      break;
    case CronPeriodType.Month:
      label = `${every} ${month}${monthDays}${hours}:${minutes}`;
      break;
    case CronPeriodType.Minute:
      label += `${every} ${minute}`;
      break;
    case CronPeriodType.Hour:
      label = `${every} ${hour} ${at} ${minutes} ${minutesPastHour}`;
      break;
    case CronPeriodType.Day:
      label = `${every} ${day}${hours}:${minutes}`;
      break;
    case CronPeriodType.Year:
      label = `${every} ${year} ${on} ${monthDays}${months}${hours}:${minutes}`;
      break;
  }
  return label[0].toUpperCase() + label.slice(1);
};

const parseTagModelToString = (tagModel: ICronsTagModel[]): string => {
  return tagModel.map(tag => tag.label).join(", ");
};

export const ScheduleGridCellDescription = ({ cronExpression }: IProps) => {
  const parseExpressionToString = React.useMemo(
    (): string => {
      const {
        dayOfMonth,
        everyDayOfMonth,
        every,
        month,
        everyDayOfWeek,
        at,
        hour,
        minute
      } = window.task.schedule;

      if (cronExpression) {
        const cronParts = getValuesFromCronString(cronExpression);
        if (!cronParts) return "cronExpression error";

        if (cronParts.cronParts.length === 5) {
          const monthDays = cronParts.cronParts[2].length
            ? ` ${parseTagModelToString(
                partToString(cronParts.cronParts[2], UNITS.get(CronUnitType.MonthDays), true)
              )} ${dayOfMonth},`
            : `${everyDayOfMonth}`;
          const months = cronParts.cronParts[3].length
            ? ` ${parseTagModelToString(
                partToString(cronParts.cronParts[3], UNITS.get(CronUnitType.Months), true)
              )}`
            : ` ${every} ${month}`;
          const weekDays = cronParts.cronParts[4].length
            ? ` ${parseTagModelToString(
                partToString(cronParts.cronParts[4], UNITS.get(CronUnitType.WeekDays), true)
              )}`
            : ` ${everyDayOfWeek}`;
          const hours = cronParts.cronParts[1].length
            ? ` ${at} ${parseTagModelToString(
                partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Hours), true)
              )}`
            : ` ${every} ${hour}`;
          const minutes = cronParts.cronParts[0].length
            ? `${parseTagModelToString(
                partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Minutes), true)
              )}`
            : `${every} ${minute}`;

          return getStringByPeriod(cronParts.period, monthDays, months, weekDays, hours, minutes);
        }

        if (cronParts.cronParts.length === 6) {
          const date = getDateValueWithZeroAhead(cronParts.cronParts[2][0]);
          const month = getDateValueWithZeroAhead(cronParts.cronParts[3][0]);
          const year = cronParts.cronParts[5][0];
          const hours = getDateValueWithZeroAhead(cronParts.cronParts[1][0]);
          const mins = getDateValueWithZeroAhead(cronParts.cronParts[0][0]);
          return `${date}.${month}.${year} ${hours}:${mins}`;
        }
      }
      return "";
    },
    [cronExpression]
  );

  return <>{parseExpressionToString}</>;
};
