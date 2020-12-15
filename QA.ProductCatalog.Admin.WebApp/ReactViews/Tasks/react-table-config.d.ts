import { Task } from "Tasks/ApiServices/DataContracts";

declare module "react-table" {
  export interface ColumnModel {
    fixedWidth?: number;
    /**
     * Параметр для EnableSchedule
     */
    getClassNameByEnableSchedule?: (task: Task) => string;
    /**
     * Параметры схлопывания
     */
    truncate?: {
      /**
       *  Схлопывать при указанной ширине
       */
      onWidth?: number;
      /**
       * Возвращает реакт-элемент который не будет схлопываться
       */
      noTruncateElement?: (task: Task) => JSX.Element;
      /**
       * Ширина несхлопывающегося элемента
       */
      noTruncateElementWidth?: number;
    };
  }

  export interface TableOptions<D extends Record<string, unknown>> extends Record<string, any> {}

  export interface Hooks<D extends Record<string, unknown> = Record<string, unknown>>
    extends Record<string, any> {}

  export interface TableInstance<D extends Record<string, unknown> = Record<string, unknown>>
    extends Record<string, any> {}

  export interface TableState<D extends Record<string, unknown> = Record<string, unknown>>
    extends Record<string, any> {}

  export interface ColumnInterface<D extends Record<string, unknown> = Record<string, unknown>>
    extends Record<string, any> {}

  export interface ColumnInstance<D extends Record<string, unknown> = Record<string, unknown>>
    extends ColumnModel,
      Record<string, any> {}

  export interface Cell<D extends Record<string, unknown> = Record<string, unknown>, V = any>
    extends Record<string, any> {}

  export interface Row<D extends Record<string, unknown> = Record<string, unknown>>
    extends Record<string, any> {}
}
