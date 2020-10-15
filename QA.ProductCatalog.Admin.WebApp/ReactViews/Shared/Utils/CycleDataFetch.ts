function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

export class CycleDataFetch<T> {
  constructor(
    readonly onSuccess: (data: T) => void,
    readonly getDataCb: () => T,
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
      this.onSuccess(data);
      this.isError = false;
    } catch (e) {
      this.isError = true;
      throw e;
    }
  };

  initCyclingFetch = async (): Promise<void> => {
    const cycling = async () => {
      try {
        if (this.fetchAttempts === this.maxFetchAttempts) return;
        await delay(this.isError ? this.onErrorTimeout : this.timeout);
        await this.getData();
      } catch (e) {
        this.fetchAttempts += 1;
        if (this.fetchAttempts === this.maxFetchAttempts) {
          throw new Error(e);
        }
        cycling();
        throw new Error(e);
      }
    };
    await cycling();
  };

  public breakCycling = (): void => {
    this.fetchAttempts = this.maxFetchAttempts;
  };

  public continueCycling = (): void => {
    console.log("cont!!!");
    this.fetchAttempts = 0;
    this.initCyclingFetch();
  };
}
export default CycleDataFetch;
