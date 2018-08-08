import { isIsoDateString } from "Utils/TypeChecks";
import { isEntityObject } from "Models/EditorDataModels";

/**
 * Отображения положительных серверных Id на  отрицательные клиентские,
 * сгруппированные по имени контента.
 */
export interface IdMapping {
  ClientId: number;
  ServerId: number;
}

export class DataSerializer {
  private _idMappingDict: { [serverId: number]: number } = Object.create(null);

  /** Обновление соответствия положительные серверные Id => отрицательные клиентские Id  */
  public extendIdMappings(idMappings: IdMapping[]) {
    idMappings.forEach(pair => {
      this._idMappingDict[pair.ServerId] = pair.ClientId;
    });
  }

  /**
   * Заменяет все серверные Id на соответствующие им
   * клиентские отрицательные Id (если они определены).
   * Преобразует ISO Date strings в Unix time.
   */
  public deserialize<T = any>(json: string): T {
    return JSON.parse(json, (_key, value) => {
      if (isEntityObject(value)) {
        // @ts-ignore
        value._ClientId = this.getClientId(value._ServerId);
        return value;
      } else if (isIsoDateString(value)) {
        return Number(new Date(value));
      }
      return value;
    });
  }

  /**
   * Статья может быть создана на клиенте: `{ _ClientId: -1, _ServerId: -1 }`,
   * затем сохранена: `{ _ClientId: -1, _ServerId: 100500 }, _idMappingDict === { 100500: -1 }`,
   * затем удалена параллельно другим пользователем, и наконец, вновьс охранена  на клиенте
   * `{ _ClientId: -1, _ServerId: 100501 }, _idMappingDict === { 100501: 100500, 100500: -1 }`.
   * Таким образом, для получения исходного `_ClientId` мы должны пройти по цепочке отображения
   * _ServerId --> _ClientId.
   */
  private getClientId(serverId: number) {
    for (let i = 0; i < 9999; i++) {
      const clientId = this._idMappingDict[serverId];
      if (!clientId) {
        return serverId;
      }
      serverId = clientId;
    }
    throw new Error("There is a cycle in DataSerializer._idMappingDict");
  }
}
