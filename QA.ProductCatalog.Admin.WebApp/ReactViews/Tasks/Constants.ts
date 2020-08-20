import { PaginationOptions } from "Tasks/ApiServices/DataContracts";

export const SESSION_EXPIRED: string = "session expired";
export const FETCH_TIMEOUT: number = 2000;
export const FETCH_ON_ERROR_TIMEOUT: number = 10000;
export const INIT_PAGINATION_OPTIONS: PaginationOptions = {
  skip: 0,
  take: 10,
  showOnlyMine: true
};
