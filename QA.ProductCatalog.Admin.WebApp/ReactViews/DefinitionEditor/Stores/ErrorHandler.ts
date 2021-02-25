import { OperationState } from "Shared/Enums";

export default abstract class ErrorHandler {
  operationState: OperationState;
  errorText: string;
  errorLog: string;
  abstract setError: () => void;
  abstract resetErrorState: () => void;
  abstract logError: (e: any, fallbackText: string) => Promise<void>;
}
