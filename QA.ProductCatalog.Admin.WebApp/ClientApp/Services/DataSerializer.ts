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
        value._ClientId = this._idMappingDict[value._ServerId] || value._ServerId;
        return value;
      } else if (isIsoDateString(value)) {
        return Number(new Date(value));
      }
      return value;
    });
  }
}
