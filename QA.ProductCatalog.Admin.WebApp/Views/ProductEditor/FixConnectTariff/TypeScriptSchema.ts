/**
 * DO NOT MODIFY THIS FILE BECAUSE IT WAS GENERATED AUTOMATICALLY
 * @see ProductEditorController.TypeScriptSchema()
 */
import { IMSTArray, IMSTMap } from "mobx-state-tree";
import { EntityObject, ExtensionObject, TablesObject } from "Models/EditorDataModels";

type IArray<T> = IMSTArray<any, any, T>;
type IMap<T> = IMSTMap<any, any, T>;

/** Типизация хранилища данных */
export interface Tables extends TablesObject {
  Region: IMap<Region>;
  Product: IMap<Product>;
  ProductModifer: IMap<ProductModifer>;
  Direction: IMap<Direction>;
  BaseParameter: IMap<BaseParameter>;
  BaseParameterModifier: IMap<BaseParameterModifier>;
  ParameterModifier: IMap<ParameterModifier>;
  ProductParameter: IMap<ProductParameter>;
  Unit: IMap<Unit>;
  ProductRelation: IMap<ProductRelation>;
  LinkParameter: IMap<LinkParameter>;
  MarketingProduct: IMap<MarketingProduct>;
  Segment: IMap<Segment>;
  TariffCategory: IMap<TariffCategory>;
  Advantage: IMap<Advantage>;
  FixConnectAction: IMap<FixConnectAction>;
  DeviceOnTariffs: IMap<DeviceOnTariffs>;
  DevicesForFixConnectAction: IMap<DevicesForFixConnectAction>;
}

export interface Region extends EntityObject {
  readonly Title: string;
}

export interface Product extends EntityObject {
  /** Маркетинговый продукт */
  MarketingProduct: MarketingProduct;
  /** Регионы */
  Regions: IArray<Region>;
  /** Модификаторы */
  Modifiers: IArray<ProductModifer>;
  /** Тип */
  Type: "Device" | "PhoneTariff" | "InternetTariff" | "FixConnectTariff";
  Type_Extension: {
    Device: Device;
    PhoneTariff: PhoneTariff;
    InternetTariff: InternetTariff;
    FixConnectTariff: FixConnectTariff;
  };
  /** Параметры продукта */
  Parameters: IArray<ProductParameter>;
  /** Акционное оборудование */
  ActionMarketingDevices: IArray<DevicesForFixConnectAction>;
  /** Описание */
  Description: string;
  /** Преимущества */
  Advantages: IArray<Advantage>;
  PDF: string;
  /** Дата начала публикации */
  StartDate: Date;
  /** Дата снятия с публикации */
  EndDate: Date;
  /** Порядок */
  SortOrder: number;
  /** Приоритет (популярность) */
  Priority: number;
  /** Изображение в списке */
  ListImage: string;
}

export interface ProductModifer extends EntityObject {
  /** Псевдоним */
  readonly Alias: string;
  /** Название */
  readonly Title: string;
}

export interface Direction extends EntityObject {
  /** Псевдоним */
  readonly Alias: string;
}

export interface BaseParameter extends EntityObject {
  /** Псевдоним */
  readonly Alias: string;
}

export interface BaseParameterModifier extends EntityObject {
  /** Псевдоним */
  readonly Alias: string;
}

export interface ParameterModifier extends EntityObject {
  /** Псевдоним */
  readonly Alias: string;
}

export interface ProductParameter extends EntityObject {
  /** Название */
  Title: string;
  /** Числовое значение */
  NumValue: number;
  /** Текстовое значение */
  Value: string;
  /** Единица измерения */
  Unit: Unit;
  /** Базовый параметр */
  BaseParameter: BaseParameter;
  /** Модификаторы базового параметра */
  BaseParameterModifiers: IArray<BaseParameterModifier>;
  /** Направление действия базового параметра */
  Direction: Direction;
  /** Модификаторы */
  Modifiers: IArray<ParameterModifier>;
}

export interface Unit extends EntityObject {
  readonly Title: string;
  readonly Alias: string;
}

export interface ProductRelation extends EntityObject {
  /** Название */
  Title: string;
  /** Параметры */
  Parameters: IArray<LinkParameter>;
}

export interface LinkParameter extends EntityObject {
  /** Название */
  Title: string;
  /** Числовое значение */
  NumValue: number;
  /** Текстовое значение */
  Value: string;
  /** Единица измерения */
  Unit: Unit;
  /** Базовый параметр */
  BaseParameter: BaseParameter;
  /** Модификаторы базового параметра */
  BaseParameterModifiers: IArray<BaseParameterModifier>;
  /** Направление действия базового параметра */
  Direction: Direction;
}

export interface MarketingProduct extends EntityObject {
  /** Тип */
  Type: "MarketingFixConnectTariff";
  Type_Extension: {
    MarketingFixConnectTariff: MarketingFixConnectTariff;
  };
  /** Название */
  Title: string;
  /** Продукты, тип "Оборудование" */
  Products: IArray<Product>;
  /** Матрица связей "Оборудование на тарифах" */
  DevicesOnTariffs: IArray<DeviceOnTariffs>;
  /** Модификаторы */
  Modifiers: IArray<ProductModifer>;
  Description: string;
  /** Порядок */
  SortOrder: number;
  /** Дата закрытия продукта (Архив) */
  ArchiveDate: Date;
  /** Преимущества */
  Advantages: IArray<Advantage>;
  /** Акции фиксированной связи */
  FixConnectActions: IArray<FixConnectAction>;
}

export interface Segment extends EntityObject {
  readonly Title: string;
}

export interface TariffCategory extends EntityObject {
  /** Название */
  readonly Title: string;
}

export interface Advantage extends EntityObject {
  readonly Title: string;
  readonly IsGift: boolean;
  /** Изображение */
  readonly ImageSvg: string;
}

export interface Device extends ExtensionObject {}

export interface FixConnectAction extends EntityObject {
  Parent: Product;
  /** Маркетинговые тарифы фиксированной связи */
  MarketingOffers: IArray<MarketingProduct>;
  /** Описание промо-периода. */
  PromoPeriod: string;
  /** Описание момента начала действия обычной цены. */
  AfterPromo: string;
}

export interface MarketingFixConnectTariff extends ExtensionObject {
  MarketingTvPackage: MarketingProduct;
  MarketingInternetTariff: MarketingProduct;
  MarketingPhoneTariff: MarketingProduct;
  MarketingDevices: IArray<MarketingProduct>;
  /** Тип предложения (Категория тарифа) */
  Category: TariffCategory;
  BonusTVPackages: IArray<MarketingProduct>;
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
  MarketingTariffs: IArray<MarketingProduct>;
  /** Города */
  Cities: IArray<Region>;
  Parent: ProductRelation;
  /** Маркетинговое устройство */
  MarketingDevice: MarketingProduct;
}

export interface DevicesForFixConnectAction extends EntityObject {
  /** Маркетинговое оборудование */
  MarketingDevice: MarketingProduct;
  Parent: ProductRelation;
  /** Акция фиксированной связи */
  FixConnectAction: Product;
}
