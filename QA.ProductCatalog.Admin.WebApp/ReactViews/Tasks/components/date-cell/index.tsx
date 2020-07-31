export const DateCell = ({ value }) => {
  const date = new Date(value);
  let dd: string | number = date.getDate();
  let mm: string | number = date.getMonth() + 1;
  if (mm < 10) {
    mm = "0" + mm;
  }
  if (dd < 10) {
    dd = "0" + dd;
  }
  return `${dd}.${mm}.${date.getFullYear()} ${date.getHours()}:${date.getMinutes()}`;
};
