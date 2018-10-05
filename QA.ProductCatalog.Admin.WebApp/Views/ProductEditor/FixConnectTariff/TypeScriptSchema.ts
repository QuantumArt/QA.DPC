import { IMSTMap } from "mobx-state-tree";
import { EntityObject, ExtensionObject, TablesObject } from "Models/EditorDataModels";

/** Типизация хранилища данных */
export interface Tables extends TablesObject {
  Region: IMSTMap<any, any, Region>;
  Product: IMSTMap<any, any, Product>;
  ProductModifer: IMSTMap<any, any, ProductModifer>;
  BaseParameter: IMSTMap<any, any, BaseParameter>;
  ProductParameter: IMSTMap<any, any, ProductParameter>;
  Unit: IMSTMap<any, any, Unit>;
  ProductRelation: IMSTMap<any, any, ProductRelation>;
  LinkParameter: IMSTMap<any, any, LinkParameter>;
  MarketingProduct: IMSTMap<any, any, MarketingProduct>;
  Segment: IMSTMap<any, any, Segment>;
  TariffCategory: IMSTMap<any, any, TariffCategory>;
  Advantage: IMSTMap<any, any, Advantage>;
  FixConnectAction: IMSTMap<any, any, FixConnectAction>;
  DeviceOnTariffs: IMSTMap<any, any, DeviceOnTariffs>;
  DevicesForFixConnectAction: IMSTMap<any, any, DevicesForFixConnectAction>;
}

export interface Region extends EntityObject {
  Title: string;
}

export interface Product extends EntityObject {
  /** Маркетинговый продукт */
  MarketingProduct: MarketingProduct;
  /** Регионы */
  Regions: Region[];
  /** Модификаторы */
  Modifiers: ProductModifer[];
  /** Тип */
  Type: "Device" | "PhoneTariff" | "InternetTariff" | "FixConnectTariff";
  Type_Contents: {
    Device: Device;
    PhoneTariff: PhoneTariff;
    InternetTariff: InternetTariff;
    FixConnectTariff: FixConnectTariff;
  };
  /** Параметры продукта */
  Parameters: ProductParameter[];
  /** Акция фиксированной связи */
  ActionMarketingDevices: DevicesForFixConnectAction[];
  /** Описание */
  Description: string;
  /** Преимущества */
  Advantages: Advantage[];
  PDF: string;
  /** Дата начала публикации */
  StartDate: Date;
  /** Дата снятия с публикации */
  EndDate: Date;
  /** Приоритет (популярность) */
  Priority: number;
  /** Изображение в списке */
  ListImage: string;
  /** Порядок */
  SortOrder: number;
}

export interface ProductModifer extends EntityObject {
  /** Название */
  Title: string;
}

export interface BaseParameter extends EntityObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface ProductParameter extends EntityObject {
  /** Название */
  Title: string;
  /** Числовое значение */
  NumValue: number;
  /** Единица измерения */
  Unit: Unit;
  /** Базовый параметр */
  BaseParameter: BaseParameter;
}

export interface Unit extends EntityObject {
  Title: string;
  Alias: string;
}

export interface ProductRelation extends EntityObject {
  /** Название */
  Title: string;
  /** Параметры */
  Parameters: LinkParameter[];
}

export interface LinkParameter extends EntityObject {
  /** Название */
  Title: string;
  /** Числовое значение */
  NumValue: number;
  /** Единица измерения */
  Unit: Unit;
  /** Базовый параметр */
  BaseParameter: BaseParameter;
}

export interface MarketingProduct extends EntityObject {
  /** Тип */
  Type: "MarketingFixConnectTariff";
  Type_Contents: {
    MarketingFixConnectTariff: MarketingFixConnectTariff;
  };
  /** Название */
  Title: string;
  /** Продукты, тип "Оборудование" */
  Products: Product[];
  /** Матрица связей "Оборудование на тарифах" */
  DevicesOnTariffs: DeviceOnTariffs[];
  Description: string;
  /** Порядок */
  SortOrder: number;
  /** Дата закрытия продукта (Архив) */
  ArchiveDate: Date;
  /** Преимущества */
  Advantages: Advantage[];
  /** Модификаторы */
  Modifiers: ProductModifer[];
  /** FixConnectActions */
  FixConnectActions: FixConnectAction[];
}

export interface Segment extends EntityObject {
  Title: string;
}

export interface TariffCategory extends EntityObject {
  /** Название */
  Title: string;
}

export interface Advantage extends EntityObject {
  Title: string;
  IsGift: boolean;
  /** Изображение */
  ImageSvg: string;
}

export interface Device extends ExtensionObject {}

export interface FixConnectAction extends EntityObject {
  Parent: Product;
  /** Описание промо-периода. */
  PromoPeriod: string;
  /** Описание момента начала действия обычной цены. */
  AfterPromo: string;
}

export interface MarketingFixConnectTariff extends ExtensionObject {
  MarketingTvPackage: MarketingProduct;
  MarketingInternetTariff: MarketingProduct;
  MarketingPhoneTariff: MarketingProduct;
  MarketingDevices: MarketingProduct[];
  /** Тип предложения (Категория тарифа) */
  Category: TariffCategory;
  BonusTVPackages: MarketingProduct[];
  Segment: Segment;
  TitleForSite: string;
}

export interface FixConnectTariff extends ExtensionObject {
  TitleForSite: string;
}

export interface PhoneTariff extends ExtensionObject {}

export interface InternetTariff extends ExtensionObject {}

export interface DeviceOnTariffs extends EntityObject {
  /** Маркетинговые тарифы фиксированной связи */
  MarketingTariffs: MarketingProduct[];
  /** Города */
  Cities: Region[];
  Parent: ProductRelation;
  /** Маркетинговое устройство */
  MarketingDevice: MarketingProduct;
}

export interface DevicesForFixConnectAction extends EntityObject {
  /** Маркетинговое оборудование */
  MarketingDevice: MarketingProduct;
  Parent: ProductRelation;
}
