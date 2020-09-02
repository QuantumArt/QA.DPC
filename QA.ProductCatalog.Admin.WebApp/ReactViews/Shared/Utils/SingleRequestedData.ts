export interface ISingleRequestedData<T> {
  getData: () => Promise<T>;
}

class SingleRequestedData<T> implements ISingleRequestedData<T> {
  constructor(
    getDataMethod: () => Promise<T>,
    maxOnErrorRequestAttempts = 3,
    onErrorTimeout = 2000
  ) {
    this.apiMethod = getDataMethod;
    this.onErrorRequestAttempts = maxOnErrorRequestAttempts;
    this.onErrorTimeout = onErrorTimeout;
  }
  private readonly apiMethod: () => Promise<T>;
  private data: T;
  private readonly onErrorRequestAttempts: number;
  private requestAttemptsWere: number = 0;
  private readonly onErrorTimeout: number;

  getData = async (): Promise<T> => {
    if (this.data) return this.data;
    try {
      this.data = await this.apiMethod();
      return await this.getData();
    } catch (e) {
      this.requestAttemptsWere++;
      if (this.requestAttemptsWere === this.onErrorRequestAttempts) {
        throw e;
      }
      setTimeout(async () => await this.getData(), this.onErrorTimeout);
      return null;
    }
  };
}

export const singleRequestedData = SingleRequestedData;
