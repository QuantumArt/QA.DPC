import React from "react";
import { getValuesFromCronString, ICronsTagModel, partToString, UNITS } from "Tasks/Utils";
import { getDateValueWithZeroAhead } from "Shared/Utils";
import { CronPeriodType, CronUnitType } from "Shared/Enums";
import "./style.scss";
import { l } from "Tasks/Localization";

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
  let label = "";
  switch (preiodType) {
    case CronPeriodType.Week:
      label += `${l("every")} ${l("week")}${weekDays}${hours}:${minutes}`;
      break;
    case CronPeriodType.Month:
      label = `${l("every")} ${l("month")}${monthDays}${hours}:${minutes}`;
      break;
    case CronPeriodType.Minute:
      label += `${l("every")} ${l("minute")}`;
      break;
    case CronPeriodType.Hour:
      label = `${l("every")} ${l("hour")} ${l("at")} ${minutes} ${l("minutesPastHour")}`;
      break;
    case CronPeriodType.Day:
      label = `${l("every")} ${l("day")}${hours}:${minutes}`;
      break;
    case CronPeriodType.Year:
      label = `${l("every")} ${l("year")} ${l("on")} ${monthDays}${months}${hours}:${minutes}`;
      break;
  }
  return label[0].toUpperCase() + label.slice(1);
};

const parseTagModelToString = (tagModel: ICronsTagModel[]): string => {
  return tagModel.map(tag => tag.label).join(", ");
};

export const ScheduleGridCellDescription = React.memo(({ cronExpression }: IProps) => {
  const parseExpressionToString = (): string => {
    if (cronExpression) {
      const cronParts = getValuesFromCronString(cronExpression);
      if (!cronParts) return l("cronParseError");
      if (cronParts.cronParts.length === 5) {
        const monthDays = cronParts.cronParts[2].length
          ? ` ${parseTagModelToString(
              partToString(cronParts.cronParts[2], UNITS.get(CronUnitType.MonthDays), true)
            )} ${l("dayOfMonth")},`
          : `${l("everyDayOfMonth")}`;
        const months = cronParts.cronParts[3].length
          ? ` ${parseTagModelToString(
              partToString(cronParts.cronParts[3], UNITS.get(CronUnitType.Months), true)
            )}`
          : ` ${l("every")} ${l("month")}`;
        const weekDays = cronParts.cronParts[4].length
          ? ` ${parseTagModelToString(
              partToString(cronParts.cronParts[4], UNITS.get(CronUnitType.WeekDays), true)
            )}`
          : ` ${l("everyDayOfWeek")}`;
        const hours = cronParts.cronParts[1].length
          ? ` ${l("at")} ${parseTagModelToString(
              partToString(cronParts.cronParts[1], UNITS.get(CronUnitType.Hours), true)
            )}`
          : ` ${l("every")} ${l("hour")}`;
        const minutes = cronParts.cronParts[0].length
          ? `${parseTagModelToString(
              partToString(cronParts.cronParts[0], UNITS.get(CronUnitType.Minutes), true)
            )}`
          : `${l("every")} ${l("minute")}`;

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
  };

  return <>{parseExpressionToString()}</>;
});
