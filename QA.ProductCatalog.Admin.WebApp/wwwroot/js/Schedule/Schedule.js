function GetCronExprFromDate(date) {
  if (date == null) return "";

  return (
    date.getMinutes() +
    " " +
    date.getHours() +
    " " +
    date.getDate() +
    " " +
    (date.getMonth() + 1) +
    " ? " +
    date.getFullYear()
  );
}

function GetDateFromCron(cronExpr) {
  var cronParts = cronExpr.split(" ");

  var dateVal = new Date(
    cronParts[5],
    cronParts[3] - 1,
    cronParts[2],
    cronParts[1],
    cronParts[0],
    0,
    0
  );

  if (isNaN(dateVal.getTime())) {
    dateVal = new Date();

    dateVal.setDate(new Date().getDate() + 1);
  }

  return dateVal;
}
