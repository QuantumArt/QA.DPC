import { observable, action } from "mobx";

import { TaskModel } from "../Shared/Types";
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
      const parsedIds = [...new Set(this.ids.split(/[,;\s\r?\n]/).filter(id => id.length > 0))];
      if (parsedIds.length > 0) {
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
    const { getTaskUrl } = window.partialSend.result;
    const response = await fetch(`${getTaskUrl}?taskId=${this.taskId}`);
    if (response.ok) {
      const json = await response.json();
      const { taskProcessingFinished, taskModel } = json as TaskResponse;
      this.setTaskProcessingFinished(taskProcessingFinished);
      this.setTask(taskModel);
    }
  };

  fetchActiveTask = async (): Promise<void> => {
    const { getActiveTaskIdUrl } = window.partialSend;
    const response = await fetch(getActiveTaskIdUrl);
    if (response.ok) {
      const json = await response.json();
      const { taskId } = json as TaskIdResponse;
      this.setTaskId(taskId);
    }
  };

  cyclicFetchTask = async (): Promise<void> => {
    await this.fetchTask();
    if (!this.taskProcessingFinished) {
      let timerId = setTimeout(this.fetchTask, 2000);
      this.setTimerId(timerId);
    } else {
      clearTimeout(this.timerId);
    }
  };

  partialSendRefreshData = async (): Promise<void> => {
    await this.fetchActiveTask();
    if (this.taskId) {
      this.setCurrentStep(CurrentStep.Result);
    } else {
      this.setCurrentStep(CurrentStep.SendForm);
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

  handleSendNewPackage = (): void => {
    this.setCurrentStep(CurrentStep.SendForm);
    this.setTaskId(null);
    this.setTask(null);
    this.setIds("");
    this.setProcessSpecialStatuses(false);
    this.setSendOnStageOnly(false);
  };
}
