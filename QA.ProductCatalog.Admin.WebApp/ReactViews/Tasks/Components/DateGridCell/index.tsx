import React from "react";
import { format, isValid } from "date-fns";

export const DateGridCell = React.memo(({ value }: { value: string }) => {
  const date = new Date(value);

  return <>{isValid(date) && format(date, "DD.MM.YYYY HH:mm:ss")}</>;
});
