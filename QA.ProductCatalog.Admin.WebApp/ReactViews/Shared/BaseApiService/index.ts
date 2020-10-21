export default abstract class BaseApiService {
  protected readonly urlFromHead = document.head.getAttribute("root-url") || "";
  protected readonly rootUrl = this.urlFromHead.endsWith("/")
    ? this.urlFromHead.slice(0, -1)
    : this.urlFromHead;

  protected mapResponse = async <TIn, TOut>(
    response: Response,
    mapper: (resp: TIn) => TOut
  ): Promise<TOut> => {
    try {
      const data: TIn = await this.tryGetResponse(response);
      return mapper(data);
    } catch (e) {
      throw e;
    }
  };

  protected tryGetResponse = async <TOut>(response: Response): Promise<TOut> => {
    const { status } = response;
    if (status !== 200) {
      throw response;
    }
    return await response.json();
  };
}
