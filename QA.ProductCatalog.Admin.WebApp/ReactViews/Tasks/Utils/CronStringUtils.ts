import _ from "lodash";
import { CronPeriodType, CronUnitType } from "Shared/Enums";
import { l } from "Tasks/Localization";

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

export const WEEK_UNITS = [l("sun"), l("mon"), l("tue"), l("wed"), l("fri"), l("sat")];
export const MONTH_UNITS = [
  l("jan"),
  l("feb"),
  l("mar"),
  l("apr"),
  l("may"),
  l("jun"),
  l("jul"),
  l("aug"),
  l("sep"),
  l("oct"),
  l("nov"),
  l("dec")
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
      min: 1,
      max: 12,
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
  let retval = [] as ICronsTagModel[];

  if (isFull(cronPart, unit)) {
    retval = [
      {
        label: l("all"),
        values: cronPart
      }
    ];
  } else {
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
  return retval;
}

/**
 * Set values from cron string
 */
export function getValuesFromCronString(
  cronString: string
): { cronParts: number[][]; period: CronPeriodType } | null {
  try {
    const cronParts = parseCronString(cronString);
    const period = getPeriodFromCronparts(cronParts);
    return { cronParts, period };
  } catch (err) {
    console.error(err);
    return null;
  }
}

/**
 * Find the period from cron parts
 */
function getPeriodFromCronparts(cronParts: number[][]): CronPeriodType {
  if (cronParts[3].length > 0) {
    return CronPeriodType.Year;
  } else if (cronParts[2].length > 0) {
    return CronPeriodType.Month;
  } else if (cronParts[4].length > 0) {
    return CronPeriodType.Week;
  } else if (cronParts[1].length > 0) {
    return CronPeriodType.Day;
  } else if (cronParts[0].length > 0) {
    return CronPeriodType.Hour;
  }
  return CronPeriodType.Minute;
}

/**
 * Parses a cron string to an array of parts
 */
function parseCronString(str: string): number[][] {
  if (typeof str !== "string") {
    throw new Error("Invalid cron string");
  }

  const parts = str
    .replace(/\s+/g, " ")
    .trim()
    .split(" ");
  if (parts.length === 5) {
    return parts.map((partStr, idx) => {
      return parsePartString(partStr, UNITS.get(getUnitByIndex(idx)));
    });
  }
  if (parts.length === 6) {
    return parts.map(partStr => {
      return [Number(partStr)];
    });
  }

  throw new Error("Invalid cron string format");
}

export const getUnitByIndex = (id: number) => {
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
function outOfRange(values: number[], unit: Unit): number {
  const first = values[0];
  const last = values[values.length - 1];

  if (first < unit.min) {
    return first;
  } else if (last > unit.max) {
    return last;
  }
  return undefined;
}

/**
 * Parses the step from a part string
 */
function parseStep(step: string, unit: Unit): number {
  if (step) {
    const parsedStep = parseInt(step, 10);

    if (isNaN(parsedStep) || parsedStep < 1) {
      throw new Error(`Invalid interval step value "${step}" for ${unit.type}`);
    }

    return parsedStep;
  }
  return undefined;
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
