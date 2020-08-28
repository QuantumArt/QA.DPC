import { Intent } from "@blueprintjs/core";

export const getClassnameByIntent = (
  className: string,
  intent: Intent,
  delimeter: string = "-"
) => {
  return `${className}${delimeter}${intent}`;
};
