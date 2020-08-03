﻿import _ from "lodash";
import { CronUnitType } from "Shared/Enums";
export type PeriodType = "year" | "month" | "week" | "day" | "hour" | "minute" | "reboot";

export type Unit = {
  type: CronUnitType;
  min: number;
  max: number;
  total: number;
  alt?: string[];
};

export interface ICronsTagModel {
  label: string;
  values: number[];
}

export const WEEK_UNITS = ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"];
export const MONTH_UNITS = [
  "JAN",
  "FEB",
  "MAR",
  "APR",
  "MAY",
  "JUN",
  "JUL",
  "AUG",
  "SEP",
  "OCT",
  "NOV",
  "DEC"
];

export const UNITS: Map<CronUnitType, Unit> = new Map([
  [
    CronUnitType.Minutes,
    {
      type: CronUnitType.Minutes,
      min: 0,
      max: 59,
      total: 60
    }
  ],
  [
    CronUnitType.Hours,
    {
      type: CronUnitType.Hours,
      min: 0,
      max: 23,
      total: 24
    }
  ],
  [
    CronUnitType.MonthDays,
    {
      type: CronUnitType.MonthDays,
      min: 1,
      max: 31,
      total: 31
    }
  ],
  [
    CronUnitType.Months,
    {
      type: CronUnitType.Months,
      min: 0,
      max: 11,
      total: 12,
      alt: MONTH_UNITS
    }
  ],
  [
    CronUnitType.WeekDays,
    {
      type: CronUnitType.WeekDays,
      min: 0,
      max: 6,
      total: 7,
      alt: WEEK_UNITS
    }
  ]
]);

//es7 experimental pollifil https://developer.mozilla.org/ru/docs/Web/JavaScript/Reference/Global_Objects/String/padStart#Browser_compatibility
//there is just function
function padStart(str: string, targetLength: number, padString: string) {
  targetLength = targetLength >> 0; //floor if number or convert non-number to 0;
  padString = String(padString || " ");
  if (this.length > targetLength) {
    return String(str);
  } else {
    targetLength = targetLength - str.length;
    if (targetLength > padString.length) {
      padString += padString.repeat(targetLength / padString.length); //append to original to ensure we are longer than needed
    }
    return padString.slice(0, targetLength) + String(str);
  }
}

function formatValue(
  value: number,
  unit: Unit,
  humanize?: boolean,
  leadingZero?: boolean | "month-days" | "hours" | "minutes",
  clockFormat?: "24-hour-clock" | "12-hour-clock"
) {
  let cronPartString = value.toString();
  const { type, alt, min } = unit;
  const needLeadingZero =
    leadingZero && (leadingZero === true || leadingZero.includes(type as any));
  const need24HourClock =
    clockFormat === "24-hour-clock" &&
    (type === CronUnitType.Hours || type === CronUnitType.Minutes);

  if ((humanize && type === CronUnitType.WeekDays) || (humanize && type === CronUnitType.Months)) {
    cronPartString = alt![value - min];
  } else if (value < 10 && (needLeadingZero || need24HourClock)) {
    cronPartString = padStart(cronPartString, 2, "0");
  }

  if (type === CronUnitType.Hours && clockFormat === "12-hour-clock") {
    const suffix = value >= 12 ? "PM" : "AM";
    let hour: number | string = value % 12 || 12;

    if (hour < 10 && needLeadingZero) {
      hour = padStart(hour.toString(), 2, "0");
    }

    cronPartString = `${hour}${suffix}`;
  }

  return cronPartString;
}

/**
 * Returns true if range has all the values of the unit
 */
function isFull(values: number[], unit: Unit) {
  return values.length === unit.max - unit.min + 1;
}

// /**
//  * Returns the difference between first and second elements in the range
//  */
// function getStep(values: number[]): number {
//   if (values.length > 2) {
//     const step = values[1] - values[0];
//
//     if (step > 1) {
//       return step;
//     }
//   }
//   return null;
// }
//
// /**
//  * Returns true if the range can be represented as an interval
//  */
// function isInterval(values: number[], step: number) {
//   for (let i = 1; i < values.length; i++) {
//     const prev = values[i - 1];
//     const value = values[i];
//
//     if (value - prev !== step) {
//       return false;
//     }
//   }
//
//   return true;
// }
// /**
//  * Returns the smallest value in the range
//  */
// function getMin(values: number[]) {
//   return values[0];
// }
//
// /**
//  * Returns the largest value in the range
//  */
// function getMax(values: number[]) {
//   return values[values.length - 1];
// }
// /**
//  * Returns true if the range contains all the interval values
//  */
// function isFullInterval(values: number[], unit: Unit, step: number) {
//   const min = getMin(values);
//   const max = getMax(values);
//   const haveAllValues = values.length === (max - min) / step + 1;
//
//   return min === unit.min && max + step > unit.max && haveAllValues;
// }

/**
 * Returns the range as an array of ranges
 * defined as arrays of positive integers
 */
function toRanges(values: number[]) {
  const retval: (number[] | number)[] = [];
  let startPart: number | null = null;

  values.forEach((value, index, self) => {
    if (value !== self[index + 1] - 1) {
      if (startPart !== null) {
        retval.push([startPart, value]);
        startPart = null;
      } else {
        retval.push(value);
      }
    } else if (startPart === null) {
      startPart = value;
    }
  });

  return retval;
}

function fillNumbersBetweenGaps(min: number, max: number) {
  return max - min > 0 ? [..._.range(min, max), max] : [min, max];
}

/**
 * Returns the cron part array as a string.
 */
export function partToString(
  cronPart: number[],
  unit: Unit,
  humanize?: boolean,
  leadingZero?: boolean | "month-days" | "hours" | "minutes",
  clockFormat?: "24-hour-clock" | "12-hour-clock"
): ICronsTagModel[] {
  let retval = [] as (ICronsTagModel)[];

  if (isFull(cronPart, unit)) {
    retval = [
      {
        label: "Все",
        values: cronPart
      }
    ];
  } else {
    // const step = getStep(cronPart);
    //
    // if (step && isInterval(cronPart, step)) {
    //   // if (isFullInterval(cronPart, unit, step)) {
    //   //   retval.push(`*/${step}`);
    //   // } else {
    //
    //   retval.push({
    //     label: `${formatValue(
    //       getMin(cronPart),
    //       unit,
    //       humanize,
    //       leadingZero,
    //       clockFormat
    //     )}-${formatValue(getMax(cronPart), unit, humanize, leadingZero, clockFormat)}/${step}`,
    //     values: fillNumbersBetweenGaps(getMin(cronPart), getMax(cronPart))
    //   });
    //   // }
    // } else {
    retval.push(
      ...toRanges(cronPart).map((range: number | number[]) => {
        if (Array.isArray(range)) {
          return {
            label: `${formatValue(
              range[0],
              unit,
              humanize,
              leadingZero,
              clockFormat
            )}-${formatValue(range[1], unit, humanize, leadingZero, clockFormat)}`,
            values: fillNumbersBetweenGaps(range[0], range[1])
          };
        }

        return {
          label: formatValue(range, unit, humanize, leadingZero, clockFormat),
          values: [range]
        };
      })
    );
  }
  // }
  return retval;
}

export type ShortcutsType =
  | "@yearly"
  | "@annually"
  | "@monthly"
  | "@weekly"
  | "@daily"
  | "@midnight"
  | "@hourly"
  | "@reboot";
export type Shortcuts = boolean | ShortcutsType[];
export interface ShortcutsValues {
  name: ShortcutsType;
  value: string;
}
// const SUPPORTED_SHORTCUTS: ShortcutsValues[] = [
//   {
//     name: '@yearly',
//     value: '0 0 1 1 *',
//   },
//   {
//     name: '@annually',
//     value: '0 0 1 1 *',
//   },
//   {
//     name: '@monthly',
//     value: '0 0 1 * *',
//   },
//   {
//     name: '@weekly',
//     value: '0 0 * * 0',
//   },
//   {
//     name: '@daily',
//     value: '0 0 * * *',
//   },
//   {
//     name: '@midnight',
//     value: '0 0 * * *',
//   },
//   {
//     name: '@hourly',
//     value: '0 * * * *',
//   },
// ]
/**
 * Set values from cron string
 */
export function getValuesFromCronString(
  cronString: string
  // setMinutes: SetValueNumbersOrUndefined,
  // setHours: SetValueNumbersOrUndefined,
  // setMonthDays: SetValueNumbersOrUndefined,
  // setMonths: SetValueNumbersOrUndefined,
  // setWeekDays: SetValueNumbersOrUndefined,
  // setPeriod: SetValuePeriod,
  // shortcuts: Shortcuts = [
  //   '@yearly',
  //   '@annually',
  //   '@monthly',
  //   '@weekly',
  //   '@daily',
  //   '@midnight',
  //   '@hourly',
  // ],
): { cronParts: number[][]; period: PeriodType } | null {
  //
  // // Shortcuts management
  // if (
  //   shortcuts &&
  //   (shortcuts === true || shortcuts.includes(cronString as any))
  // ) {
  //   if (cronString === '@reboot') {
  //     // setPeriod('reboot')
  //
  //     return null
  //   }
  //
  //   // Convert a shortcut to a valid cron string
  //   const shortcutObject = SUPPORTED_SHORTCUTS.find(
  //     (supportedShortcut) => supportedShortcut.name === cronString
  //   )
  //
  //   if (shortcutObject) {
  //     cronString = shortcutObject.value
  //   }
  // }

  try {
    const cronParts = parseCronString(cronString);
    const period = getPeriodFromCronparts(cronParts);
    return { cronParts, period };
  } catch (err) {
    return null;
    // Specific errors are not handle (yet)
  }
}

/**
 * Find the period from cron parts
 */
function getPeriodFromCronparts(cronParts: number[][]): PeriodType {
  if (cronParts[3].length > 0) {
    return "year";
  } else if (cronParts[2].length > 0) {
    return "month";
  } else if (cronParts[4].length > 0) {
    return "week";
  } else if (cronParts[1].length > 0) {
    return "day";
  } else if (cronParts[0].length > 0) {
    return "hour";
  }
  return "minute";
}

/**
 * Parses a cron string to an array of parts
 */
function parseCronString(str: string) {
  if (typeof str !== "string") {
    throw new Error("Invalid cron string");
  }

  const parts = str
    .replace(/\s+/g, " ")
    .trim()
    .split(" ");

  if (parts.length === 5) {
    return parts.map((partStr, idx) => {
      return parsePartString(partStr, UNITS.get(getidx(idx)));
    });
  }

  throw new Error("Invalid cron string format");
}
const getidx = (id: number) => {
  if (id === 0) return CronUnitType.Minutes;
  if (id === 1) return CronUnitType.Hours;
  if (id === 2) return CronUnitType.MonthDays;
  if (id === 3) return CronUnitType.Months;
  if (id === 4) return CronUnitType.WeekDays;
  return CronUnitType.Minutes;
};

/**
 * Parses a string as a range of positive integers
 */
function parsePartString(str: string, unit: Unit) {
  if (str === "*" || str === "*/1") {
    return [];
  }

  const stringParts = str.split("/");

  if (stringParts.length > 2) {
    throw new Error(`Invalid value "${unit.type}"`);
  }

  const rangeString = replaceAlternatives(stringParts[0], unit.min, unit.alt);
  let parsedValues: number[];

  if (rangeString === "*") {
    parsedValues = range(unit.min, unit.max);
  } else {
    parsedValues = sort(
      dedup(
        fixSunday(
          _.flatten(
            rangeString.split(",").map(range => {
              return parseRange(range, str, unit);
            })
          ),
          unit
        )
      )
    );

    const value = outOfRange(parsedValues, unit);

    if (typeof value !== "undefined") {
      throw new Error(`Value "${value}" out of range for ${unit.type}`);
    }
  }

  const step = parseStep(stringParts[1], unit);
  const intervalValues = applyInterval(parsedValues, step);

  if (intervalValues.length === unit.total) {
    return [];
  } else if (intervalValues.length === 0) {
    throw new Error(`Empty interval value "${str}" for ${unit.type}`);
  }

  return intervalValues;
}

/**
 * Replaces the alternative representations of numbers in a string
 */
function replaceAlternatives(str: string, min: number, alt?: string[]) {
  if (alt) {
    str = str.toUpperCase();

    for (let i = 0; i < alt.length; i++) {
      str = str.replace(alt[i], `${i + min}`);
    }
  }
  return str;
}
/**
 * Creates an array of integers from start to end, inclusive
 */
export function range(start: number, end: number) {
  const array = [];

  for (let i = start; i <= end; i++) {
    array.push(i);
  }

  return array;
}

/**
 * Sorts an array of numbers
 */
export function sort(array: number[]) {
  array.sort(function(a, b) {
    return a - b;
  });

  return array;
}

/**
 * Removes duplicate entries from an array
 */
export function dedup(array: number[]) {
  const result: number[] = [];

  array.forEach(function(i) {
    if (result.indexOf(i) < 0) {
      result.push(i);
    }
  });

  return result;
}

/**
 * Replace all 7 with 0 as Sunday can be represented by both
 */
function fixSunday(values: number[], unit: Unit) {
  if (unit.type === CronUnitType.WeekDays) {
    values = values.map(function(value) {
      if (value === 7) {
        return 0;
      }

      return value;
    });
  }

  return values;
}

/**
 * Parses a range string
 */
function parseRange(rangeStr: string, context: string, unit: Unit) {
  const subparts = rangeStr.split("-");

  if (subparts.length === 1) {
    const value = parseInt(subparts[0], 10);

    if (isNaN(value)) {
      throw new Error(`Invalid value "${context}" for ${unit.type}`);
    }

    return [value];
  } else if (subparts.length === 2) {
    const minValue = parseInt(subparts[0], 10);
    const maxValue = parseInt(subparts[1], 10);

    if (maxValue <= minValue) {
      throw new Error(`Max range is less than min range in "${rangeStr}" for ${unit.type}`);
    }

    return range(minValue, maxValue);
  } else {
    throw new Error(`Invalid value "${rangeStr}" for ${unit.type}`);
  }
}
/**
 * Finds an element from values that is outside of the range of unit
 */
function outOfRange(values: number[], unit: Unit) {
  const first = values[0];
  const last = values[values.length - 1];

  if (first < unit.min) {
    return first;
  } else if (last > unit.max) {
    return last;
  }

  // @ts-ignore
  return;
}

/**
 * Parses the step from a part string
 */
// @ts-ignore
function parseStep(step: string, unit: Unit) {
  if (typeof step !== "undefined") {
    const parsedStep = parseInt(step, 10);

    if (isNaN(parsedStep) || parsedStep < 1) {
      throw new Error(`Invalid interval step value "${step}" for ${unit.type}`);
    }

    return parsedStep;
  }
}

/**
 * Applies an interval step to a collection of values
 */
function applyInterval(values: number[], step?: number) {
  if (step) {
    const minVal = values[0];

    values = values.filter(value => {
      return value % step === minVal % step || value === minVal;
    });
  }

  return values;
}
