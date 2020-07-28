import { Intent } from "@blueprintjs/core";

export const getClassNameByIntent = (
  className: string,
  intent: Intent,
  delimeter: string = "-"
) => {
  return `${className}${delimeter}${intent}`;
};
