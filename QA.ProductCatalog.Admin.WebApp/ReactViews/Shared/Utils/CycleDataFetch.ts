function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

export class CycleDataFetch<T> {
  constructor(
    readonly onSuccess: null | ((data: T) => void) = null,
    readonly getDataCb: () => Promise<T>,
    timeout,
    onErrorTimeout,
    maxFetchAttempts = 3
  ) {
    this.getDataCb = getDataCb;
    this.onSuccess = onSuccess;
    this.timeout = timeout;
    this.onErrorTimeout = onErrorTimeout;
    this.maxFetchAttempts = maxFetchAttempts;
  }
  public data: T;
  private readonly timeout: number;
  private readonly onErrorTimeout: number;
  private isError: boolean = false;
  private fetchAttempts: number = 0;
  private readonly maxFetchAttempts: number;

  private readonly getData = async () => {
    try {
      const data = await this.getDataCb();
      if (this.onSuccess) this.onSuccess(data);
      this.isError = false;
    } catch (e) {
      this.isError = true;
      throw e;
    }
  };

  initCyclingFetch = async (): Promise<void> => {
    let isFirstRequest = true;

    const cycling = async () => {
      try {
        if (this.fetchAttempts === this.maxFetchAttempts) return;
        if (!isFirstRequest) await delay(this.isError ? this.onErrorTimeout : this.timeout);
        await this.getData();
        if (isFirstRequest) isFirstRequest = false;
        return cycling();
      } catch (e) {
        this.fetchAttempts += 1;
        if (this.fetchAttempts === this.maxFetchAttempts) {
          throw new Error(e);
        }
        console.error(e, `${this.fetchAttempts} error of ${this.maxFetchAttempts} in cycle fetch`);
        return cycling();
      }
    };

    await cycling();
  };

  public breakCycling = (): void => {
    this.fetchAttempts = this.maxFetchAttempts;
  };

  public continueCycling = (): void => {
    this.fetchAttempts = 0;
    this.initCyclingFetch();
  };
}
export default CycleDataFetch;
