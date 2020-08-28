import { observable, action } from "mobx";

import { TaskModel } from "Shared/Types";
import { MAX_FETCH_COUNT } from "Shared/Constants";
import { FetchStatus } from "Shared/Enums";

import { CurrentStep } from "./Enums";

type TaskResponse = {
  taskProcessingFinished: boolean;
  taskModel: TaskModel;
};

type TaskIdResponse = {
  taskId: number;
};

export default class PartialSendStore {
  @observable timerId: NodeJS.Timeout;
  @observable currentStep: CurrentStep = CurrentStep.Empty;
  @observable fetchStatus: FetchStatus = FetchStatus.Idle;
  @observable failureFetchCount: number = 0;
  @observable fetchError: string;
  @observable responseStatus: number;
  @observable cyclicFunctionEnded: boolean = false;

  @observable ids: string;
  @observable processSpecialStatuses: boolean = false;
  @observable sendOnStageOnly: boolean = false;

  @observable taskId: number;
  @observable task: TaskModel;
  @observable taskProcessingFinished: boolean = false;

  @observable isValidForm: boolean = true;
  @observable idsValidationError: string;

  @action setTimerId = (value: NodeJS.Timeout): void => {
    this.timerId = value;
  };

  @action setCurrentStep = (value: CurrentStep): void => {
    this.currentStep = value;
  };

  @action setFetchStatus = (value: FetchStatus): void => {
    this.fetchStatus = value;
  };

  @action setFailureFetchCount = (value: number): void => {
    this.failureFetchCount = value;
  };

  @action setFetchError = (value: string): void => {
    this.fetchError = value;
  };

  @action setResponseStatus = (value: number): void => {
    this.responseStatus = value;
  };

  @action setCyclicFunctionEnded = (value: boolean): void => {
    this.cyclicFunctionEnded = value;
  };

  @action setIds = (value: string): void => {
    this.ids = value;
  };

  @action setProcessSpecialStatuses = (value: boolean): void => {
    this.processSpecialStatuses = value;
  };

  @action setSendOnStageOnly = (value: boolean): void => {
    this.sendOnStageOnly = value;
  };

  @action setTaskId = (value: number): void => {
    this.taskId = value;
  };

  @action setTask = (value: TaskModel): void => {
    this.task = value;
  };

  @action setTaskProcessingFinished = (value: boolean): void => {
    this.taskProcessingFinished = value;
  };

  @action setIsValidForm = (value: boolean): void => {
    this.isValidForm = value;
  };

  @action setIdsValidationError = (value: string): void => {
    this.idsValidationError = value;
  };

  handleValidateForm = (): void => {
    if (this.ids) {
      const parsedIds = this.ids
        .split(/[,;\s\r?\n]/)
        .filter(id => id.length > 0)
        .map(id => parseInt(id));

      const notNumbers = parsedIds.filter(id => isNaN(id));
      const uniqueIds = [...new Set(parsedIds.filter(id => !isNaN(id)))];

      if (notNumbers.length > 0) {
        this.setIsValidForm(false);
        this.setIdsValidationError("Incorrect Ids list");
      } else if (uniqueIds.length > 0) {
        this.setIsValidForm(true);
        this.setIdsValidationError(null);
      } else {
        this.setIsValidForm(false);
        this.setIdsValidationError("Ids list should not be empty");
      }
    } else {
      this.setIsValidForm(false);
      this.setIdsValidationError("Incorrect Ids list");
    }
  };

  fetchTask = async (): Promise<void> => {
    this.setFetchStatus(FetchStatus.Loading);

    const { getTaskUrl } = window.partialSend.result;
    const response = await fetch(`${getTaskUrl}?taskId=${this.taskId}`);
    this.setResponseStatus(response.status);
    if (response.ok) {
      this.setFetchStatus(FetchStatus.Success);
      const json = await response.json();
      const { taskProcessingFinished, taskModel } = json as TaskResponse;
      this.setTaskProcessingFinished(taskProcessingFinished);
      this.setTask(taskModel);
    } else {
      this.setFetchStatus(FetchStatus.Failure);
      if (this.fetchStatus === 401) {
        this.setFetchError("Сессия устарела. Переоткройте или обновите вкладку.");
      } else {
        this.setFetchError("Сервер недоступен. Переоткройте или обновите вкладку.");
      }
      this.setFailureFetchCount(this.failureFetchCount + 1);
    }
  };

  cyclicFetchTask = async (): Promise<void> => {
    await this.fetchTask();
    if (this.fetchStatus === FetchStatus.Success) {
      if (!this.taskProcessingFinished) {
        let timerId = setTimeout(this.cyclicFetchTask, 2000);
        this.setTimerId(timerId);
      } else {
        this.setFailureFetchCount(0);
        this.setFetchError(null);
        clearTimeout(this.timerId);
      }
    }

    if (this.fetchStatus === FetchStatus.Failure) {
      if (this.failureFetchCount >= MAX_FETCH_COUNT) {
        clearTimeout(this.timerId);
      } else {
        let timerId = setTimeout(this.cyclicFetchTask, 10000);
        this.setTimerId(timerId);
      }
    }
  };

  fetchActiveTask = async (): Promise<void> => {
    const { getActiveTaskIdUrl } = window.partialSend;
    const response = await fetch(getActiveTaskIdUrl);
    this.setResponseStatus(response.status);
    if (response.ok) {
      this.setFetchStatus(FetchStatus.Success);
      const json = await response.json();
      const { taskId } = json as TaskIdResponse;
      this.setTaskId(taskId);
    } else {
      this.setFetchStatus(FetchStatus.Failure);
      if (this.fetchStatus === 401) {
        this.setFetchError("Сессия устарела. Переоткройте или обновите вкладку.");
      } else {
        this.setFetchError("Сервер недоступен. Переоткройте или обновите вкладку.");
      }
      this.setFailureFetchCount(this.failureFetchCount + 1);
    }
  };

  cyclicFetchActiveTask = async (): Promise<void> => {
    await this.fetchActiveTask();
    if (this.fetchStatus === FetchStatus.Success) {
      this.setFailureFetchCount(0);
      this.setFetchError(null);
      clearTimeout(this.timerId);
    }

    if (this.fetchStatus === FetchStatus.Failure) {
      if (this.failureFetchCount >= MAX_FETCH_COUNT) {
        clearTimeout(this.timerId);
      } else {
        let timerId = setTimeout(this.cyclicFetchActiveTask, 10000);
        this.setTimerId(timerId);
      }
    }
  };

  handleSubmit = async (form: HTMLFormElement): Promise<void> => {
    this.handleValidateForm();
    if (this.isValidForm) {
      const { sendUrl } = window.partialSend.sendForm;
      const response = await fetch(sendUrl, { method: "POST", body: new FormData(form) });
      if (response.ok) {
        const json = await response.json();
        const { taskId } = json as TaskIdResponse;
        this.setTaskId(taskId);
        this.setCurrentStep(CurrentStep.Result);
      }
    }
  };

  resetNetworkInfo = (): void => {
    this.setFetchStatus(FetchStatus.Idle);
    this.setFailureFetchCount(0);
    this.setFetchError(null);
    this.setResponseStatus(null);
  };

  resetTaskInfo = (): void => {
    this.setTaskId(null);
    this.setTask(null);
    this.setTaskProcessingFinished(false);
  };

  resetForm = (): void => {
    this.setIsValidForm(true);
    this.setIdsValidationError("");
    this.setIds("");
    this.setProcessSpecialStatuses(false);
    this.setSendOnStageOnly(false);
  };

  handleSendNewPackage = (): void => {
    this.setCurrentStep(CurrentStep.SendForm);
    this.resetNetworkInfo();
    this.resetTaskInfo();
    this.resetForm();
  };
}
