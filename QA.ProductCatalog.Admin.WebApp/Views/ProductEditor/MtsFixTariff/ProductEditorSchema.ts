import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";

/** Типизация хранилища данных */
export interface ProductEntities {
  Region: Region;
  Product: Product;
  Group: Group;
  ProductModifer: ProductModifer;
  TariffZone: TariffZone;
  Direction: Direction;
  BaseParameter: BaseParameter;
  BaseParameterModifier: BaseParameterModifier;
  ParameterModifier: ParameterModifier;
  ProductParameter: ProductParameter;
  Unit: Unit;
  LinkModifier: LinkModifier;
  ProductRelation: ProductRelation;
  LinkParameter: LinkParameter;
  ProductParameterGroup: ProductParameterGroup;
  MarketingProduct: MarketingProduct;
  CommunicationType: CommunicationType;
  Segment: Segment;
  MarketingProductParameter: MarketingProductParameter;
  TariffCategory: TariffCategory;
  Advantage: Advantage;
  TimeZone: TimeZone;
  NetworkCity: NetworkCity;
  ChannelCategory: ChannelCategory;
  ChannelType: ChannelType;
  ChannelFormat: ChannelFormat;
  TvChannel: TvChannel;
  ParameterChoice: ParameterChoice;
  FixedType: FixedType;
  EquipmentType: EquipmentType;
  EquipmentDownload: EquipmentDownload;
  DeviceOnTariffs: DeviceOnTariffs;
  DevicesForFixConnectAction: DevicesForFixConnectAction;
}

export interface Region extends ArticleObject {
  Title: string;
  Alias: string;
  Parent: Region;
  IsMainCity: boolean;
}

export interface Product extends ArticleObject {
  /** Маркетинговый продукт */
  MarketingProduct: MarketingProduct;
  /** GlobalCode */
  GlobalCode: string;
  GlobalCode_Value: string;
  GlobalCode_Version: string;
  /** Тип */
  Type: 
    | "Tariff"
    | "Service"
    | "Action"
    | "RoamingScale"
    | "Device"
    | "FixConnectAction"
    | "TvPackage"
    | "FixConnectTariff"
    | "PhoneTariff"
    | "InternetTariff";
  Type_Contents: {
    Tariff: Tariff;
    Service: Service;
    Action: Action;
    RoamingScale: RoamingScale;
    Device: Device;
    FixConnectAction: FixConnectAction;
    TvPackage: TvPackage;
    FixConnectTariff: FixConnectTariff;
    PhoneTariff: PhoneTariff;
    InternetTariff: InternetTariff;
  };
  /** Описание */
  Description: string;
  /** Полное описание */
  FullDescription: string;
  /** Примечания */
  Notes: string;
  /** Ссылка */
  Link: string;
  /** Порядок */
  SortOrder: number;
  ForisID: string;
  /** Иконка */
  Icon: string;
  PDF: string;
  /** Алиас фиксированной ссылки на Pdf */
  PdfFixedAlias: string;
  /** Фиксированные ссылки на Pdf */
  PdfFixedLinks: string;
  /** Дата начала публикации */
  StartDate: Date;
  /** Дата снятия с публикации */
  EndDate: Date;
  OldSiteId: number;
  OldId: number;
  OldSiteInvId: string;
  OldCorpSiteId: number;
  OldAliasId: string;
  /** Приоритет (популярность) */
  Priority: number;
  /** Изображение в списке */
  ListImage: string;
  /** Дата перевода в архив */
  ArchiveDate: Date;
  /** Модификаторы */
  Modifiers: ProductModifer[];
  /** Параметры продукта */
  Parameters: ProductParameter[];
  /** Регионы */
  Regions: Region[];
  /** Акционное оборудование */
  FixConnectAction: DevicesForFixConnectAction[];
  /** Преимущества */
  Advantages: Advantage[];
}

export interface Group extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface ProductModifer extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface Tariff extends ExtensionObject {
}

export interface TariffZone extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface Direction extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface BaseParameter extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
  AllowZone: boolean;
  AllowDirection: boolean;
}

export interface BaseParameterModifier extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
  Type: 
    | "Step"
    | "Package"
    | "Zone"
    | "Direction"
    | "Refining";
}

export interface ParameterModifier extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface ProductParameter extends ArticleObject {
  /** Группа параметров */
  Group: ProductParameterGroup;
  /** Название */
  Title: string;
  /** Родительский параметр */
  Parent: ProductParameter;
  /** Базовый параметр */
  BaseParameter: BaseParameter;
  /** Зона действия базового параметра */
  Zone: TariffZone;
  /** Направление действия базового параметра */
  Direction: Direction;
  /** Модификаторы базового параметра */
  BaseParameterModifiers: BaseParameterModifier[];
  /** Модификаторы */
  Modifiers: ParameterModifier[];
  /** Единица измерения */
  Unit: Unit;
  /** Порядок */
  SortOrder: number;
  /** Числовое значение */
  NumValue: number;
  /** Текстовое значение */
  Value: string;
  Description: string;
  /** Изображение параметра */
  Image: string;
  /** Группа продуктов */
  ProductGroup: Group;
  /** Выбор */
  Choice: ParameterChoice;
}

export interface Unit extends ArticleObject {
  Alias: string;
  Title: string;
  Display: string;
  /** Размерность */
  QuotaUnit: 
    | "mb"
    | "gb"
    | "kb"
    | "tb"
    | "min"
    | "message"
    | "rub"
    | "sms"
    | "mms"
    | "mbit"
    | "step";
  /** Период */
  QuotaPeriod: 
    | "daily"
    | "weekly"
    | "monthly"
    | "hourly"
    | "minutely"
    | "every_second"
    | "annually";
  /** Название периодичности */
  QuotaPeriodicity: string;
  /** Множитель периода */
  PeriodMultiplier: number;
  Type: string;
}

export interface LinkModifier extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface ProductRelation extends ArticleObject {
  /** Название */
  Title: string;
  /** Модификаторы */
  Modifiers: LinkModifier[];
  /** Параметры */
  Parameters: LinkParameter[];
  /** Тип */
  Type: 
    | "TariffTransfer"
    | "MutualGroup"
    | "ServiceOnTariff"
    | "ServicesUpsale"
    | "TariffOptionPackage"
    | "ServiceRelation"
    | "RoamingScaleOnTariff"
    | "ServiceOnRoamingScale"
    | "CrossSale"
    | "MarketingCrossSale"
    | "DeviceOnTariffs"
    | "DevicesForFixConnectAction";
  Type_Contents: {
    TariffTransfer: TariffTransfer;
    MutualGroup: MutualGroup;
    ServiceOnTariff: ServiceOnTariff;
    ServicesUpsale: ServicesUpsale;
    TariffOptionPackage: TariffOptionPackage;
    ServiceRelation: ServiceRelation;
    RoamingScaleOnTariff: RoamingScaleOnTariff;
    ServiceOnRoamingScale: ServiceOnRoamingScale;
    CrossSale: CrossSale;
    MarketingCrossSale: MarketingCrossSale;
    DeviceOnTariffs: DeviceOnTariffs;
    DevicesForFixConnectAction: DevicesForFixConnectAction;
  };
}

export interface LinkParameter extends ArticleObject {
  /** Название */
  Title: string;
  /** Группа параметров */
  Group: ProductParameterGroup;
  /** Базовый параметр */
  BaseParameter: BaseParameter;
  /** Зона действия базового параметра */
  Zone: TariffZone;
  /** Направление действия базового параметра */
  Direction: Direction;
  /** Модификаторы базового параметра */
  BaseParameterModifiers: BaseParameterModifier[];
  /** Модификаторы */
  Modifiers: ParameterModifier[];
  /** Порядок */
  SortOrder: number;
  /** Числовое значение */
  NumValue: number;
  /** Текстовое значение */
  Value: string;
  Description: string;
  /** Единица измерения */
  Unit: Unit;
  ProductGroup: Group;
  /** Множественный выбор */
  Choice: ParameterChoice[];
  OldSiteId: number;
  OldCorpSiteId: number;
  OldPointId: number;
  OldCorpPointId: number;
}

export interface TariffTransfer extends ExtensionObject {
}

export interface MutualGroup extends ExtensionObject {
}

export interface ProductParameterGroup extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
  /** Порядок */
  SortOrder: number;
  OldSiteId: number;
  OldCorpSiteId: number;
  /** Изображение */
  ImageSvg: string;
  Type: string;
  /** Название для МГМН */
  TitleForIcin: string;
}

export interface MarketingProduct extends ArticleObject {
  /** Название */
  Title: string;
  /** Псевдоним */
  Alias: string;
  Description: string;
  OldSiteId: number;
  OldCorpSiteId: number;
  /** Изображение в списке */
  ListImage: string;
  /** Изображение */
  DetailsImage: string;
  /** Дата закрытия продукта (Архив) */
  ArchiveDate: Date;
  /** Модификаторы */
  Modifiers: ProductModifer[];
  /** Порядок */
  SortOrder: number;
  /** Приоритет (популярность) */
  Priority: number;
  /** Преимущества */
  Advantages: Advantage[];
  /** Тип */
  Type: 
    | "MarketingTariff"
    | "MarketingService"
    | "MarketingAction"
    | "MarketingRoamingScale"
    | "MarketingDevice"
    | "MarketingFixConnectAction"
    | "MarketingTvPackage"
    | "MarketingFixConnectTariff"
    | "MarketingPhoneTariff"
    | "MarketingInternetTariff";
  Type_Contents: {
    MarketingTariff: MarketingTariff;
    MarketingService: MarketingService;
    MarketingAction: MarketingAction;
    MarketingRoamingScale: MarketingRoamingScale;
    MarketingDevice: MarketingDevice;
    MarketingFixConnectAction: MarketingFixConnectAction;
    MarketingTvPackage: MarketingTvPackage;
    MarketingFixConnectTariff: MarketingFixConnectTariff;
    MarketingPhoneTariff: MarketingPhoneTariff;
    MarketingInternetTariff: MarketingInternetTariff;
  };
  FullDescription: string;
  /** Параметры маркетингового продукта */
  Parameters: MarketingProductParameter[];
  /** Тарифы для оборудования */
  TariffsOnMarketingDevice: DeviceOnTariffs[];
  /** Оборудование на тарифе */
  DevicesOnMarketingTariff: DeviceOnTariffs[];
  /** Акции для оборудования */
  ActionsOnMarketingDevice: DevicesForFixConnectAction[];
  /** Ссылка */
  Link: string;
  /** Подробное описание */
  DetailedDescription: string;
}

export interface MarketingTariff extends ExtensionObject {
}

export interface MarketingService extends ExtensionObject {
}

export interface Service extends ExtensionObject {
}

export interface ServiceOnTariff extends ExtensionObject {
  Description: string;
}

export interface ServicesUpsale extends ExtensionObject {
  Order: number;
}

export interface TariffOptionPackage extends ExtensionObject {
  /** Подзаголовок */
  SubTitle: string;
  /** Описание */
  Description: string;
  /** Псевдоним */
  Alias: string;
  /** Ссылка */
  Link: string;
}

export interface ServiceRelation extends ExtensionObject {
}

export interface CommunicationType extends ArticleObject {
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface Segment extends ArticleObject {
  Title: string;
  /** Псевдоним */
  Alias: string;
}

export interface Action extends ExtensionObject {
}

export interface MarketingAction extends ExtensionObject {
}

export interface MarketingProductParameter extends ArticleObject {
  /** Группа параметров */
  Group: ProductParameterGroup;
  /** Базовый параметр */
  BaseParameter: BaseParameter;
  /** Зона действия базового параметра */
  Zone: TariffZone;
  /** Направление действия базового параметра */
  Direction: Direction;
  /** Модификаторы базового параметра */
  BaseParameterModifiers: BaseParameterModifier[];
  /** Модификаторы */
  Modifiers: ParameterModifier[];
  /** Единица измерения */
  Unit: Unit;
  /** Множественный выбор */
  Choice: ParameterChoice[];
  /** Название */
  Title: string;
  /** Порядок */
  SortOrder: number;
  /** Числовое значение */
  NumValue: number;
  /** Текстовое значение */
  Value: string;
  Description: string;
  /** Изображение параметра */
  Image: string;
}

export interface RoamingScale extends ExtensionObject {
}

export interface MarketingRoamingScale extends ExtensionObject {
}

export interface RoamingScaleOnTariff extends ExtensionObject {
}

export interface TariffCategory extends ArticleObject {
  /** Типы связи */
  ConnectionTypes: FixedType[];
  /** Название */
  Title: string;
  /** Алиас */
  Alias: string;
  /** Картинка */
  Image: string;
  /** Порядок */
  Order: number;
  /** Векторное изображение */
  ImageSvg: string;
  /** Тип шаблона страницы */
  TemplateType: 
    | "Tv"
    | "Phone";
}

export interface ServiceOnRoamingScale extends ExtensionObject {
}

export interface Advantage extends ArticleObject {
  Title: string;
  /** Текстовые данные */
  Text: string;
  /** Описание */
  Description: string;
  /** Изображение */
  ImageSvg: string;
  /** Порядок */
  SortOrder: number;
  IsGift: boolean;
  OldSiteId: number;
}

export interface CrossSale extends ExtensionObject {
  /** Порядок */
  Order: number;
}

export interface MarketingCrossSale extends ExtensionObject {
  /** Порядок */
  Order: number;
}

export interface TimeZone extends ArticleObject {
  /** Название часовой зоны */
  Name: string;
  /** Код зоны */
  Code: string;
  /** Значение по UTC */
  UTC: string;
  /** Значение от Московского времени */
  MSK: string;
  OldSiteId: number;
}

export interface NetworkCity extends ArticleObject {
  /** Город */
  City: Region;
  /** IPTV */
  HasIpTv: boolean;
}

export interface ChannelCategory extends ArticleObject {
  /** Название для сайта */
  Name: string;
  Alias: string;
  /** Сегменты */
  Segments: string;
  /** Иконка */
  Icon: string;
  Order: number;
  OldSiteId: number;
}

export interface ChannelType extends ArticleObject {
  Title: string;
  OldSiteId: number;
}

export interface ChannelFormat extends ArticleObject {
  Title: string;
  Image: string;
  Message: string;
  OldSiteId: number;
}

export interface TvChannel extends ArticleObject {
  /** Название телеканала */
  Title: string;
  /** Лого 150x150 */
  Logo150: string;
  /** Основная категория телеканала */
  Category: ChannelCategory;
  /** Тип канала */
  ChannelType: ChannelType;
  /** Короткое описание */
  ShortDescription: string;
  /** Города вещания */
  Cities: NetworkCity[];
  /** Приостановлено вещание */
  Disabled: boolean;
  /** МТС Москва */
  IsMtsMsk: boolean;
  /** Регионал. канал */
  IsRegional: boolean;
  /** LCN DVB-C */
  LcnDvbC: number;
  /** LCN IPTV */
  LcnIpTv: number;
  /** LCN DVB-S */
  LcnDvbS: number;
  /** Формат */
  Format: ChannelFormat;
  /** Родительский канал */
  Parent: TvChannel;
  /** Дочерние каналы */
  Children: TvChannel[];
  /** Лого 40х30 */
  Logo40x30: string;
  /** Часовая зона (UTC) */
  TimeZone: TimeZone;
  /** LCN_IPTV_R */
  LcnIpTvR: number;
}

export interface ParameterChoice extends ArticleObject {
  Title: string;
  Alias: string;
  OldSiteId: number;
}

export interface MarketingDevice extends ExtensionObject {
  /** Тип оборудования */
  DeviceType: EquipmentType;
  /** Сегменты */
  Segments: Segment[];
  /** Вид связи */
  CommunicationType: CommunicationType;
}

export interface Device extends ExtensionObject {
  /** Загрузки */
  Downloads: EquipmentDownload[];
  /** Состав комплекта */
  Inners: Product[];
  /** Отложенная публикация на */
  FreezeDate: Date;
  /** Полное руководство пользователя (User guide) */
  FullUserGuide: string;
  /** Краткое руководство пользователя (Quick start guide) */
  QuickStartGuide: string;
}

export interface FixedType extends ArticleObject {
  Title: string;
}

export interface EquipmentType extends ArticleObject {
  /** Тип связи */
  ConnectionType: FixedType;
  Title: string;
  Alias: string;
  /** Порядок */
  Order: number;
}

export interface EquipmentDownload extends ArticleObject {
  Title: string;
  File: string;
}

export interface MarketingFixConnectAction extends ExtensionObject {
  /** Сегмент */
  Segment: Segment[];
  /** Акция в Каталоге акций */
  MarketingAction: MarketingProduct;
  StartDate: Date;
  EndDate: Date;
  /** Описание промо-периода. */
  PromoPeriod: string;
  /** Описание момента начала действия обычной цены. */
  AfterPromo: string;
}

export interface FixConnectAction extends ExtensionObject {
  /** Маркетинговые предложения */
  MarketingOffers: MarketingProduct[];
  /** Описание промо-периода. */
  PromoPeriod: string;
  /** Описание момента начала действия обычной цены. */
  AfterPromo: string;
}

export interface MarketingTvPackage extends ExtensionObject {
  /** Каналы */
  Channels: TvChannel[];
  TitleForSite: string;
  /** Тип пакета */
  PackageType: 
    | "Base"
    | "Additional";
}

export interface TvPackage extends ExtensionObject {
}

export interface MarketingFixConnectTariff extends ExtensionObject {
  Segment: Segment;
  /** Тип предложения (Категория тарифа) */
  Category: TariffCategory;
  MarketingDevices: MarketingProduct[];
  BonusTVPackages: MarketingProduct[];
  MarketingPhoneTariff: MarketingProduct;
  MarketingInternetTariff: MarketingProduct;
  MarketingTvPackage: MarketingProduct;
  TitleForSite: string;
}

export interface FixConnectTariff extends ExtensionObject {
  TitleForSite: string;
}

export interface MarketingPhoneTariff extends ExtensionObject {
}

export interface PhoneTariff extends ExtensionObject {
  /** ВЗ вызовы (ссылка на Ростелеком) */
  RostelecomLink: string;
}

export interface MarketingInternetTariff extends ExtensionObject {
}

export interface InternetTariff extends ExtensionObject {
}

export interface DeviceOnTariffs extends ArticleObject {
  Parent: ProductRelation;
  Order: number;
  /** Маркетинговое устройство */
  MarketingDevice: MarketingProduct;
  /** Маркетинговые тарифы */
  MarketingTariffs: MarketingProduct[];
  /** Города */
  Cities: Region[];
}

export interface DevicesForFixConnectAction extends ArticleObject {
  Order: number;
  /** Акция фиксированной связи */
  FixConnectAction: Product;
  Parent: ProductRelation;
  /** Маркетинговое оборудование */
  MarketingDevice: MarketingProduct;
}
