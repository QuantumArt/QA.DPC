import { getDateValueWithZeroAhead } from "Shared/Utils";

export const DateGridCell = ({ value }) => {
  const date = new Date(value);
  let dd: number | string = date.getDate();
  let mm: number | string = date.getMonth() + 1;
  if (mm < 10) {
    mm = getDateValueWithZeroAhead(mm);
  }
  if (dd < 10) {
    dd = getDateValueWithZeroAhead(dd);
  }
  return `${dd}.${mm}.${date.getFullYear()} ${date.getHours()}:${date.getMinutes()}`;
};
