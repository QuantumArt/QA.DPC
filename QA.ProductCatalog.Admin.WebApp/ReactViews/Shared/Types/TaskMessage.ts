export interface Message {
  Extra: any;
  Message: string | null;
  Parameters: any;
  ResourceClass: string | null;
  ResourceName: any;
}

export interface TaskMessage {
  FailedIds: number[];
  IsSuccess: boolean;
  Messages: Message[];
}
