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
  let every = "Каждую ";
  switch (preiodType) {
    case CronPeriodType.Week:
      every += `неделю${weekDays}${hours}:${minutes}`;
      break;
    case CronPeriodType.Month:
      every = `Каждый месяц${monthDays}${hours}:${minutes}`;
      break;
    case CronPeriodType.Minute:
      every += "минуту";
      break;
    case CronPeriodType.Hour:
      every = `Каждый час в ${minutes} минут`;
      break;
    case CronPeriodType.Day:
      every = `Каждый день${hours}:${minutes}`;
      break;
    case CronPeriodType.Year:
      every = `Каждый год${monthDays}${months}${hours}:${minutes}`;
      break;
  }
  return every;
};

const parseTagModelToString = (tagModel: ICronsTagModel[]): string => {
  return tagModel.map(tag => tag.label).join(", ");
};

export const ScheduleGridCellDescription = ({ cronExpression }: IProps) => {
  const parseExpressionToString = React.useMemo(
    (): string => {
      if (cronExpression) {
        const cronParts = getValuesFromCronString(cronExpression);
        if (!cronParts) return "";

        if (cronParts.cronParts.length === 5) {
          const monthDays = cronParts.cronParts[2].length
            ? ` ${parseTagModelToString(
                partToString(cronParts.cronParts[2], UNITS.get(CronUnitType.MonthDays), true)
              )} числа,`
            : " каждый день месяца";
          const months = cronParts.cronParts[3].length
            ? ` ${parseTagModelToString(
                partToString(cronParts.cronParts[3], UNITS.get(CronUnitType.Months), true)
              )}`
            : " каждый месяц";
          const weekDays = cronParts.cronParts[4].length
            ? ` ${parseTagModelToString(
                partToString(cronParts.cronParts[4], UNITS.get(CronUnitType.WeekDays), true)
              )}`
            : " каждый день недели";
          const hours = cronParts.cronParts[1].length
            ? ` в ${parseTagModelToString(
                partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Hours), true)
              )}`
            : " каждый час";
          const minutes = cronParts.cronParts[0].length
            ? `${parseTagModelToString(
                partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Minutes), true)
              )}`
            : "каждую минуту";

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
