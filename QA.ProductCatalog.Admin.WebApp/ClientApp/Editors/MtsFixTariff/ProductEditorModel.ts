import { types as t } from "mobx-state-tree";

/** Файл, загружаемый в QPublishing */
export const FileModel = t.model("FileModel", {
  /** Имя файла */
  Name: t.string,
  /** URL файла */
  AbsoluteUrl: t.string,
});


type _IRegion = typeof Region.Type;
/** Регионы */
export interface IRegion extends _IRegion {}
/** Регионы */
export const Region = t.model("Region_290", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /**  */
  Alias: t.maybe(t.string),
  /**  */
  Parent: t.maybe(t.reference(t.late(() => Region))),
  /**  */
  IsMainCity: t.maybe(t.boolean),
});

type _IProduct = typeof Product.Type;
/** Продукты */
export interface IProduct extends _IProduct {}
/** Продукты */
export const Product = t.model("Product_339", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Маркетинговый продукт */
  MarketingProduct: t.maybe(t.reference(t.late(() => MarketingProduct))),
  /** GlobalCode */
  GlobalCode: t.maybe(t.string),
  /** Тип */
  Type: t.maybe(t.enumeration("Type", [
    "Tariff",
    "Service",
    "Action",
    "RoamingScale",
    "Device",
    "FixConnectAction",
    "TvPackage",
    "FixConnectTariff",
    "PhoneTariff",
    "InternetTariff",
  ])),
  /** Контенты поля-классификатора */
  Type_Contents: t.maybe(t.model({
    /** Тарифы */
    Tariff: t.maybe(t.late(() => Tariff)),
    /** Услуги */
    Service: t.maybe(t.late(() => Service)),
    /** Акции */
    Action: t.maybe(t.late(() => Action)),
    /** Роуминговые сетки */
    RoamingScale: t.maybe(t.late(() => RoamingScale)),
    /** Оборудование */
    Device: t.maybe(t.late(() => Device)),
    /** Акции фиксированной связи */
    FixConnectAction: t.maybe(t.late(() => FixConnectAction)),
    /** ТВ-пакеты */
    TvPackage: t.maybe(t.late(() => TvPackage)),
    /** Тарифы фиксированной связи */
    FixConnectTariff: t.maybe(t.late(() => FixConnectTariff)),
    /** Тарифы телефонии */
    PhoneTariff: t.maybe(t.late(() => PhoneTariff)),
    /** Тарифы Интернет */
    InternetTariff: t.maybe(t.late(() => InternetTariff)),
  })),
  /** Описание */
  Description: t.maybe(t.string),
  /** Полное описание */
  FullDescription: t.maybe(t.string),
  /** Примечания */
  Notes: t.maybe(t.string),
  /** Ссылка */
  Link: t.maybe(t.string),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /**  */
  ForisID: t.maybe(t.string),
  /** Иконка */
  Icon: t.maybe(t.string),
  /**  */
  PDF: t.maybe(FileModel),
  /** Алиас фиксированной ссылки на Pdf */
  PdfFixedAlias: t.maybe(t.string),
  /** Фиксированные ссылки на Pdf */
  PdfFixedLinks: t.maybe(t.string),
  /** Дата начала публикации */
  StartDate: t.maybe(t.Date),
  /** Дата снятия с публикации */
  EndDate: t.maybe(t.Date),
  /**  */
  OldSiteId: t.maybe(t.number),
  /**  */
  OldId: t.maybe(t.number),
  /**  */
  OldSiteInvId: t.maybe(t.string),
  /**  */
  OldCorpSiteId: t.maybe(t.number),
  /**  */
  OldAliasId: t.maybe(t.string),
  /** Приоритет (популярность) */
  Priority: t.maybe(t.number),
  /** Изображение в списке */
  ListImage: t.maybe(t.string),
  /** Дата перевода в архив */
  ArchiveDate: t.maybe(t.Date),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.late(() => ProductModifer)), []),
  /** Параметры продукта */
  Parameters: t.optional(t.array(t.late(() => ProductParameter)), []),
  /** Регионы */
  Regions: t.optional(t.array(t.late(() => Region)), []),
  /** Акция фиксированной связи */
  FixConnectAction: t.optional(t.array(t.late(() => DevicesForFixConnectAction)), []),
  /** Преимущества */
  Advantages: t.optional(t.array(t.late(() => Advantage)), []),
});

type _IGroup = typeof Group.Type;
/** Группы продуктов */
export interface IGroup extends _IGroup {}
/** Группы продуктов */
export const Group = t.model("Group_340", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _IProductModifer = typeof ProductModifer.Type;
/** Модификаторы продуктов */
export interface IProductModifer extends _IProductModifer {}
/** Модификаторы продуктов */
export const ProductModifer = t.model("ProductModifer_342", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _ITariffZone = typeof TariffZone.Type;
/** Тарифные зоны */
export interface ITariffZone extends _ITariffZone {}
/** Тарифные зоны */
export const TariffZone = t.model("TariffZone_346", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _IDirection = typeof Direction.Type;
/** Направления соединения */
export interface IDirection extends _IDirection {}
/** Направления соединения */
export const Direction = t.model("Direction_347", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _IBaseParameter = typeof BaseParameter.Type;
/** Базовые параметры продуктов */
export interface IBaseParameter extends _IBaseParameter {}
/** Базовые параметры продуктов */
export const BaseParameter = t.model("BaseParameter_350", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
  /**  */
  AllowZone: t.maybe(t.boolean),
  /**  */
  AllowDirection: t.maybe(t.boolean),
});

type _IBaseParameterModifier = typeof BaseParameterModifier.Type;
/** Модификаторы базовых параметров продуктов */
export interface IBaseParameterModifier extends _IBaseParameterModifier {}
/** Модификаторы базовых параметров продуктов */
export const BaseParameterModifier = t.model("BaseParameterModifier_351", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
  /**  */
  Type: t.maybe(t.enumeration("Type", [
    "Step",
    "Package",
    "Zone",
    "Direction",
    "Refining",
  ])),
});

type _IParameterModifier = typeof ParameterModifier.Type;
/** Модификаторы параметров продуктов */
export interface IParameterModifier extends _IParameterModifier {}
/** Модификаторы параметров продуктов */
export const ParameterModifier = t.model("ParameterModifier_352", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _IProductParameter = typeof ProductParameter.Type;
/** Параметры продуктов */
export interface IProductParameter extends _IProductParameter {}
/** Параметры продуктов */
export const ProductParameter = t.model("ProductParameter_354", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Группа параметров */
  Group: t.maybe(t.reference(t.late(() => ProductParameterGroup))),
  /** Название */
  Title: t.maybe(t.string),
  /** Родительский параметр */
  Parent: t.maybe(t.reference(t.late(() => ProductParameter))),
  /** Базовый параметр */
  BaseParameter: t.maybe(t.reference(t.late(() => BaseParameter))),
  /** Зона действия базового параметра */
  Zone: t.maybe(t.reference(t.late(() => TariffZone))),
  /** Направление действия базового параметра */
  Direction: t.maybe(t.reference(t.late(() => Direction))),
  /** Модификаторы базового параметра */
  BaseParameterModifiers: t.optional(t.array(t.late(() => BaseParameterModifier)), []),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.late(() => ParameterModifier)), []),
  /** Единица измерения */
  Unit: t.maybe(t.reference(t.late(() => Unit))),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /** Числовое значение */
  NumValue: t.maybe(t.number),
  /** Текстовое значение */
  Value: t.maybe(t.string),
  /**  */
  Description: t.maybe(t.string),
  /** Изображение параметра */
  Image: t.maybe(t.string),
  /** Группа продуктов */
  ProductGroup: t.maybe(t.reference(t.late(() => Group))),
  /** Выбор */
  Choice: t.maybe(t.reference(t.late(() => ParameterChoice))),
});

type _IUnit = typeof Unit.Type;
/** Единицы измерения */
export interface IUnit extends _IUnit {}
/** Единицы измерения */
export const Unit = t.model("Unit_355", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Alias: t.maybe(t.string),
  /**  */
  Title: t.maybe(t.string),
  /**  */
  Display: t.maybe(t.string),
  /** Размерность */
  QuotaUnit: t.maybe(t.enumeration("QuotaUnit", [
    "mb",
    "gb",
    "kb",
    "tb",
    "min",
    "message",
    "rub",
    "sms",
    "mms",
    "mbit",
    "step",
  ])),
  /** Период */
  QuotaPeriod: t.maybe(t.enumeration("QuotaPeriod", [
    "daily",
    "weekly",
    "monthly",
    "hourly",
    "minutely",
    "every_second",
    "annually",
  ])),
  /** Название периодичности */
  QuotaPeriodicity: t.maybe(t.string),
  /** Множитель периода */
  PeriodMultiplier: t.maybe(t.number),
  /**  */
  Type: t.maybe(t.string),
});

type _ILinkModifier = typeof LinkModifier.Type;
/** Модификаторы связей */
export interface ILinkModifier extends _ILinkModifier {}
/** Модификаторы связей */
export const LinkModifier = t.model("LinkModifier_360", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _IProductRelation = typeof ProductRelation.Type;
/** Матрица связей */
export interface IProductRelation extends _IProductRelation {}
/** Матрица связей */
export const ProductRelation = t.model("ProductRelation_361", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.late(() => LinkModifier)), []),
  /** Параметры */
  Parameters: t.optional(t.array(t.late(() => LinkParameter)), []),
  /** Тип */
  Type: t.maybe(t.enumeration("Type", [
    "TariffTransfer",
    "MutualGroup",
    "ServiceOnTariff",
    "ServicesUpsale",
    "TariffOptionPackage",
    "ServiceRelation",
    "RoamingScaleOnTariff",
    "ServiceOnRoamingScale",
    "CrossSale",
    "MarketingCrossSale",
    "DeviceOnTariffs",
    "DevicesForFixConnectAction",
  ])),
  /** Контенты поля-классификатора */
  Type_Contents: t.maybe(t.model({
    /** Переходы с тарифа на тариф */
    TariffTransfer: t.maybe(t.late(() => TariffTransfer)),
    /** Группы несовместимости услуг */
    MutualGroup: t.maybe(t.late(() => MutualGroup)),
    /** Услуги на тарифе */
    ServiceOnTariff: t.maybe(t.late(() => ServiceOnTariff)),
    /** Матрица предложений услуг Upsale */
    ServicesUpsale: t.maybe(t.late(() => ServicesUpsale)),
    /** Пакеты опций на тарифах */
    TariffOptionPackage: t.maybe(t.late(() => TariffOptionPackage)),
    /** Связи между услугами */
    ServiceRelation: t.maybe(t.late(() => ServiceRelation)),
    /** Роуминговые сетки для тарифа */
    RoamingScaleOnTariff: t.maybe(t.late(() => RoamingScaleOnTariff)),
    /** Услуги на роуминговой сетке */
    ServiceOnRoamingScale: t.maybe(t.late(() => ServiceOnRoamingScale)),
    /** Матрица предложений CrossSale */
    CrossSale: t.maybe(t.late(() => CrossSale)),
    /** Матрица маркетинговых предложений CrossSale */
    MarketingCrossSale: t.maybe(t.late(() => MarketingCrossSale)),
    /** Оборудование на тарифах */
    DeviceOnTariffs: t.maybe(t.late(() => DeviceOnTariffs)),
    /** Акционное оборудование */
    DevicesForFixConnectAction: t.maybe(t.late(() => DevicesForFixConnectAction)),
  })),
});

type _ILinkParameter = typeof LinkParameter.Type;
/** Параметры связей */
export interface ILinkParameter extends _ILinkParameter {}
/** Параметры связей */
export const LinkParameter = t.model("LinkParameter_362", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Группа параметров */
  Group: t.maybe(t.reference(t.late(() => ProductParameterGroup))),
  /** Базовый параметр */
  BaseParameter: t.maybe(t.reference(t.late(() => BaseParameter))),
  /** Зона действия базового параметра */
  Zone: t.maybe(t.reference(t.late(() => TariffZone))),
  /** Направление действия базового параметра */
  Direction: t.maybe(t.reference(t.late(() => Direction))),
  /** Модификаторы базового параметра */
  BaseParameterModifiers: t.optional(t.array(t.late(() => BaseParameterModifier)), []),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.late(() => ParameterModifier)), []),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /** Числовое значение */
  NumValue: t.maybe(t.number),
  /** Текстовое значение */
  Value: t.maybe(t.string),
  /**  */
  Description: t.maybe(t.string),
  /** Единица измерения */
  Unit: t.maybe(t.reference(t.late(() => Unit))),
  /**  */
  ProductGroup: t.maybe(t.reference(t.late(() => Group))),
  /** Выбор */
  Choice: t.maybe(t.reference(t.late(() => ParameterChoice))),
  /**  */
  OldSiteId: t.maybe(t.number),
  /**  */
  OldCorpSiteId: t.maybe(t.number),
  /**  */
  OldPointId: t.maybe(t.number),
  /**  */
  OldCorpPointId: t.maybe(t.number),
});

type _IProductParameterGroup = typeof ProductParameterGroup.Type;
/** Группы параметров продуктов */
export interface IProductParameterGroup extends _IProductParameterGroup {}
/** Группы параметров продуктов */
export const ProductParameterGroup = t.model("ProductParameterGroup_378", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /**  */
  OldSiteId: t.maybe(t.number),
  /**  */
  OldCorpSiteId: t.maybe(t.number),
  /** Изображение */
  ImageSvg: t.maybe(FileModel),
  /**  */
  Type: t.maybe(t.string),
  /** Название для МГМН */
  TitleForIcin: t.maybe(t.string),
});

type _IMarketingProduct = typeof MarketingProduct.Type;
/** Маркетинговые продукты */
export interface IMarketingProduct extends _IMarketingProduct {}
/** Маркетинговые продукты */
export const MarketingProduct = t.model("MarketingProduct_383", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
  /**  */
  Description: t.maybe(t.string),
  /**  */
  OldSiteId: t.maybe(t.number),
  /**  */
  OldCorpSiteId: t.maybe(t.number),
  /** Изображение в списке */
  ListImage: t.maybe(t.string),
  /** Изображение */
  DetailsImage: t.maybe(t.string),
  /** Дата закрытия продукта (Архив) */
  ArchiveDate: t.maybe(t.Date),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.late(() => ProductModifer)), []),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /** Приоритет (популярность) */
  Priority: t.maybe(t.number),
  /** Преимущества */
  Advantages: t.optional(t.array(t.late(() => Advantage)), []),
  /** Тип */
  Type: t.maybe(t.enumeration("Type", [
    "MarketingTariff",
    "MarketingService",
    "MarketingAction",
    "MarketingRoamingScale",
    "MarketingDevice",
    "MarketingFixConnectAction",
    "MarketingTvPackage",
    "MarketingFixConnectTariff",
    "MarketingPhoneTariff",
    "MarketingInternetTariff",
  ])),
  /** Контенты поля-классификатора */
  Type_Contents: t.maybe(t.model({
    /** Маркетинговые тарифы */
    MarketingTariff: t.maybe(t.late(() => MarketingTariff)),
    /** Маркетинговые услуги */
    MarketingService: t.maybe(t.late(() => MarketingService)),
    /** Маркетинговые акции */
    MarketingAction: t.maybe(t.late(() => MarketingAction)),
    /** Маркетинговые роуминговые сетки */
    MarketingRoamingScale: t.maybe(t.late(() => MarketingRoamingScale)),
    /** Маркетинговое оборудование */
    MarketingDevice: t.maybe(t.late(() => MarketingDevice)),
    /** Маркетинговые акции фиксированной связи */
    MarketingFixConnectAction: t.maybe(t.late(() => MarketingFixConnectAction)),
    /** Маркетинговые ТВ-пакеты */
    MarketingTvPackage: t.maybe(t.late(() => MarketingTvPackage)),
    /** Маркетинговые тарифы фиксированной связи */
    MarketingFixConnectTariff: t.maybe(t.late(() => MarketingFixConnectTariff)),
    /** Маркетинговые тарифы телефонии */
    MarketingPhoneTariff: t.maybe(t.late(() => MarketingPhoneTariff)),
    /** Маркетинговые тарифы интернет */
    MarketingInternetTariff: t.maybe(t.late(() => MarketingInternetTariff)),
  })),
  /**  */
  FullDescription: t.maybe(t.string),
  /** Параметры маркетингового продукта */
  Parameters: t.optional(t.array(t.late(() => MarketingProductParameter)), []),
  /** Маркетинговое устройство */
  TariffsOnMarketingDevice: t.optional(t.array(t.late(() => DeviceOnTariffs)), []),
  /** Маркетинговые тарифы */
  DevicesOnMarketingTariff: t.optional(t.array(t.late(() => DeviceOnTariffs)), []),
  /** Маркетинговое оборудование */
  ActionsOnMarketingDevice: t.optional(t.array(t.late(() => DevicesForFixConnectAction)), []),
  /** Ссылка */
  Link: t.maybe(t.string),
  /** Подробное описание */
  DetailedDescription: t.maybe(t.string),
});

type _ICommunicationType = typeof CommunicationType.Type;
/** Виды связи */
export interface ICommunicationType extends _ICommunicationType {}
/** Виды связи */
export const CommunicationType = t.model("CommunicationType_415", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _ISegment = typeof Segment.Type;
/** Сегменты */
export interface ISegment extends _ISegment {}
/** Сегменты */
export const Segment = t.model("Segment_416", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
});

type _IMarketingProductParameter = typeof MarketingProductParameter.Type;
/** Параметры маркетинговых продуктов */
export interface IMarketingProductParameter extends _IMarketingProductParameter {}
/** Параметры маркетинговых продуктов */
export const MarketingProductParameter = t.model("MarketingProductParameter_424", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Группа параметров */
  Group: t.maybe(t.reference(t.late(() => ProductParameterGroup))),
  /** Базовый параметр */
  BaseParameter: t.maybe(t.reference(t.late(() => BaseParameter))),
  /** Зона действия базового параметра */
  Zone: t.maybe(t.reference(t.late(() => TariffZone))),
  /** Направление действия базового параметра */
  Direction: t.maybe(t.reference(t.late(() => Direction))),
  /** Модификаторы базового параметра */
  BaseParameterModifiers: t.optional(t.array(t.late(() => BaseParameterModifier)), []),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.late(() => ParameterModifier)), []),
  /** Единица измерения */
  Unit: t.maybe(t.reference(t.late(() => Unit))),
  /** Выбор */
  Choice: t.maybe(t.reference(t.late(() => ParameterChoice))),
  /** Название */
  Title: t.maybe(t.string),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /** Числовое значение */
  NumValue: t.maybe(t.number),
  /** Текстовое значение */
  Value: t.maybe(t.string),
  /**  */
  Description: t.maybe(t.string),
});

type _ITariffCategory = typeof TariffCategory.Type;
/** Категории тарифов */
export interface ITariffCategory extends _ITariffCategory {}
/** Категории тарифов */
export const TariffCategory = t.model("TariffCategory_441", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Типы связи */
  ConnectionTypes: t.optional(t.array(t.late(() => FixedType)), []),
  /** Название */
  Title: t.maybe(t.string),
  /** Алиас */
  Alias: t.maybe(t.string),
  /** Картинка */
  Image: t.maybe(t.string),
  /** Порядок */
  Order: t.maybe(t.number),
  /** Векторное изображение */
  ImageSvg: t.maybe(FileModel),
  /** Тип шаблона страницы */
  TemplateType: t.maybe(t.enumeration("TemplateType", [
    "Tv",
    "Phone",
  ])),
});

type _IAdvantage = typeof Advantage.Type;
/** Преимущества маркетинговых продуктов */
export interface IAdvantage extends _IAdvantage {}
/** Преимущества маркетинговых продуктов */
export const Advantage = t.model("Advantage_446", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /** Текстовые данные */
  Text: t.maybe(t.string),
  /** Описание */
  Description: t.maybe(t.string),
  /** Изображение */
  ImageSvg: t.maybe(FileModel),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /**  */
  IsGift: t.maybe(t.boolean),
  /**  */
  OldSiteId: t.maybe(t.number),
});

type _ITimeZone = typeof TimeZone.Type;
/** Часовые зоны */
export interface ITimeZone extends _ITimeZone {}
/** Часовые зоны */
export const TimeZone = t.model("TimeZone_471", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название часовой зоны */
  Name: t.maybe(t.string),
  /** Код зоны */
  Code: t.maybe(t.string),
  /** Значение по UTC */
  UTC: t.maybe(t.string),
  /** Значение от Московского времени */
  MSK: t.maybe(t.string),
  /**  */
  OldSiteId: t.maybe(t.number),
});

type _INetworkCity = typeof NetworkCity.Type;
/** Города сети */
export interface INetworkCity extends _INetworkCity {}
/** Города сети */
export const NetworkCity = t.model("NetworkCity_472", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Город */
  City: t.maybe(t.reference(t.late(() => Region))),
  /** IPTV */
  HasIpTv: t.maybe(t.boolean),
});

type _IChannelCategory = typeof ChannelCategory.Type;
/** Категории каналов */
export interface IChannelCategory extends _IChannelCategory {}
/** Категории каналов */
export const ChannelCategory = t.model("ChannelCategory_478", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название для сайта */
  Name: t.maybe(t.string),
  /**  */
  Alias: t.maybe(t.string),
  /** Сегменты */
  Segments: t.maybe(t.string),
  /** Иконка */
  Icon: t.maybe(t.string),
  /**  */
  Order: t.maybe(t.number),
  /**  */
  OldSiteId: t.maybe(t.number),
});

type _IChannelType = typeof ChannelType.Type;
/** Типы каналов */
export interface IChannelType extends _IChannelType {}
/** Типы каналов */
export const ChannelType = t.model("ChannelType_479", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /**  */
  OldSiteId: t.maybe(t.number),
});

type _IChannelFormat = typeof ChannelFormat.Type;
/** Форматы каналов */
export interface IChannelFormat extends _IChannelFormat {}
/** Форматы каналов */
export const ChannelFormat = t.model("ChannelFormat_480", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /**  */
  Image: t.maybe(t.string),
  /**  */
  Message: t.maybe(t.string),
  /**  */
  OldSiteId: t.maybe(t.number),
});

type _ITvChannel = typeof TvChannel.Type;
/** ТВ-каналы */
export interface ITvChannel extends _ITvChannel {}
/** ТВ-каналы */
export const TvChannel = t.model("TvChannel_482", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название телеканала */
  Title: t.maybe(t.string),
  /** Лого 150x150 */
  Logo150: t.maybe(t.string),
  /** Основная категория телеканала */
  Category: t.maybe(t.reference(t.late(() => ChannelCategory))),
  /** Тип канала */
  ChannelType: t.maybe(t.reference(t.late(() => ChannelType))),
  /** Короткое описание */
  ShortDescription: t.maybe(t.string),
  /** Города вещания */
  Cities: t.optional(t.array(t.late(() => NetworkCity)), []),
  /** Приостановлено вещание */
  Disabled: t.maybe(t.boolean),
  /** МТС Москва */
  IsMtsMsk: t.maybe(t.boolean),
  /** Регионал. канал */
  IsRegional: t.maybe(t.boolean),
  /** LCN DVB-C */
  LcnDvbC: t.maybe(t.number),
  /** LCN IPTV */
  LcnIpTv: t.maybe(t.number),
  /** LCN DVB-S */
  LcnDvbS: t.maybe(t.number),
  /** Формат */
  Format: t.maybe(t.reference(t.late(() => ChannelFormat))),
  /** Родительский канал */
  Parent: t.maybe(t.reference(t.late(() => TvChannel))),
  /** Дочерние каналы */
  Children: t.optional(t.array(t.late(() => TvChannel)), []),
  /** Лого 40х30 */
  Logo40x30: t.maybe(t.string),
  /** Часовая зона (UTC) */
  TimeZone: t.maybe(t.reference(t.late(() => TimeZone))),
});

type _IParameterChoice = typeof ParameterChoice.Type;
/** Варианты выбора для параметров */
export interface IParameterChoice extends _IParameterChoice {}
/** Варианты выбора для параметров */
export const ParameterChoice = t.model("ParameterChoice_488", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /**  */
  Alias: t.maybe(t.string),
  /**  */
  OldSiteId: t.maybe(t.number),
});

type _IFixedType = typeof FixedType.Type;
/** Типы фиксированной связи */
export interface IFixedType extends _IFixedType {}
/** Типы фиксированной связи */
export const FixedType = t.model("FixedType_491", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
});

type _IEquipmentType = typeof EquipmentType.Type;
/** Типы оборудования */
export interface IEquipmentType extends _IEquipmentType {}
/** Типы оборудования */
export const EquipmentType = t.model("EquipmentType_493", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Тип связи */
  ConnectionType: t.maybe(t.reference(t.late(() => FixedType))),
  /**  */
  Title: t.maybe(t.string),
  /**  */
  Alias: t.maybe(t.string),
  /** Порядок */
  Order: t.maybe(t.number),
});

type _IEquipmentDownload = typeof EquipmentDownload.Type;
/** Загрузки для оборудования */
export interface IEquipmentDownload extends _IEquipmentDownload {}
/** Загрузки для оборудования */
export const EquipmentDownload = t.model("EquipmentDownload_494", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Title: t.maybe(t.string),
  /**  */
  File: t.maybe(FileModel),
});

type _IDeviceOnTariffs = typeof DeviceOnTariffs.Type;
/** Оборудование на тарифах */
export interface IDeviceOnTariffs extends _IDeviceOnTariffs {}
/** Оборудование на тарифах */
export const DeviceOnTariffs = t.model("DeviceOnTariffs_511", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /**  */
  Parent: t.maybe(t.reference(t.late(() => ProductRelation))),
  /**  */
  Order: t.maybe(t.number),
  /** Маркетинговое устройство */
  MarketingDevice: t.maybe(t.reference(t.late(() => MarketingProduct))),
  /** Маркетинговые тарифы */
  MarketingTariffs: t.optional(t.array(t.late(() => MarketingProduct)), []),
  /** Города */
  Cities: t.optional(t.array(t.late(() => Region)), []),
});



type _ITariff = typeof Tariff.Type;
/** Тарифы (Extension) */
export interface ITariff extends _ITariff {}
/** Тарифы (Extension) */
export const Tariff = t.model("Tariff_343", {
});

type _ITariffTransfer = typeof TariffTransfer.Type;
/** Переходы с тарифа на тариф (Extension) */
export interface ITariffTransfer extends _ITariffTransfer {}
/** Переходы с тарифа на тариф (Extension) */
export const TariffTransfer = t.model("TariffTransfer_364", {
});

type _IMutualGroup = typeof MutualGroup.Type;
/** Группы несовместимости услуг (Extension) */
export interface IMutualGroup extends _IMutualGroup {}
/** Группы несовместимости услуг (Extension) */
export const MutualGroup = t.model("MutualGroup_365", {
});

type _IMarketingTariff = typeof MarketingTariff.Type;
/** Маркетинговые тарифы (Extension) */
export interface IMarketingTariff extends _IMarketingTariff {}
/** Маркетинговые тарифы (Extension) */
export const MarketingTariff = t.model("MarketingTariff_385", {
});

type _IMarketingService = typeof MarketingService.Type;
/** Маркетинговые услуги (Extension) */
export interface IMarketingService extends _IMarketingService {}
/** Маркетинговые услуги (Extension) */
export const MarketingService = t.model("MarketingService_402", {
});

type _IService = typeof Service.Type;
/** Услуги (Extension) */
export interface IService extends _IService {}
/** Услуги (Extension) */
export const Service = t.model("Service_403", {
});

type _IServiceOnTariff = typeof ServiceOnTariff.Type;
/** Услуги на тарифе (Extension) */
export interface IServiceOnTariff extends _IServiceOnTariff {}
/** Услуги на тарифе (Extension) */
export const ServiceOnTariff = t.model("ServiceOnTariff_404", {
  /**  */
  Description: t.maybe(t.string),
});

type _IServicesUpsale = typeof ServicesUpsale.Type;
/** Матрица предложений услуг Upsale (Extension) */
export interface IServicesUpsale extends _IServicesUpsale {}
/** Матрица предложений услуг Upsale (Extension) */
export const ServicesUpsale = t.model("ServicesUpsale_406", {
  /**  */
  Order: t.maybe(t.number),
});

type _ITariffOptionPackage = typeof TariffOptionPackage.Type;
/** Пакеты опций на тарифах (Extension) */
export interface ITariffOptionPackage extends _ITariffOptionPackage {}
/** Пакеты опций на тарифах (Extension) */
export const TariffOptionPackage = t.model("TariffOptionPackage_407", {
  /** Подзаголовок */
  SubTitle: t.maybe(t.string),
  /** Описание */
  Description: t.maybe(t.string),
  /** Псевдоним */
  Alias: t.maybe(t.string),
  /** Ссылка */
  Link: t.maybe(t.string),
});

type _IServiceRelation = typeof ServiceRelation.Type;
/** Связи между услугами (Extension) */
export interface IServiceRelation extends _IServiceRelation {}
/** Связи между услугами (Extension) */
export const ServiceRelation = t.model("ServiceRelation_413", {
});

type _IAction = typeof Action.Type;
/** Акции (Extension) */
export interface IAction extends _IAction {}
/** Акции (Extension) */
export const Action = t.model("Action_419", {
});

type _IMarketingAction = typeof MarketingAction.Type;
/** Маркетинговые акции (Extension) */
export interface IMarketingAction extends _IMarketingAction {}
/** Маркетинговые акции (Extension) */
export const MarketingAction = t.model("MarketingAction_420", {
});

type _IRoamingScale = typeof RoamingScale.Type;
/** Роуминговые сетки (Extension) */
export interface IRoamingScale extends _IRoamingScale {}
/** Роуминговые сетки (Extension) */
export const RoamingScale = t.model("RoamingScale_434", {
});

type _IMarketingRoamingScale = typeof MarketingRoamingScale.Type;
/** Маркетинговые роуминговые сетки (Extension) */
export interface IMarketingRoamingScale extends _IMarketingRoamingScale {}
/** Маркетинговые роуминговые сетки (Extension) */
export const MarketingRoamingScale = t.model("MarketingRoamingScale_435", {
});

type _IRoamingScaleOnTariff = typeof RoamingScaleOnTariff.Type;
/** Роуминговые сетки для тарифа (Extension) */
export interface IRoamingScaleOnTariff extends _IRoamingScaleOnTariff {}
/** Роуминговые сетки для тарифа (Extension) */
export const RoamingScaleOnTariff = t.model("RoamingScaleOnTariff_438", {
});

type _IServiceOnRoamingScale = typeof ServiceOnRoamingScale.Type;
/** Услуги на роуминговой сетке (Extension) */
export interface IServiceOnRoamingScale extends _IServiceOnRoamingScale {}
/** Услуги на роуминговой сетке (Extension) */
export const ServiceOnRoamingScale = t.model("ServiceOnRoamingScale_444", {
});

type _ICrossSale = typeof CrossSale.Type;
/** Матрица предложений CrossSale (Extension) */
export interface ICrossSale extends _ICrossSale {}
/** Матрица предложений CrossSale (Extension) */
export const CrossSale = t.model("CrossSale_468", {
  /** Порядок */
  Order: t.maybe(t.number),
});

type _IMarketingCrossSale = typeof MarketingCrossSale.Type;
/** Матрица маркетинговых предложений CrossSale (Extension) */
export interface IMarketingCrossSale extends _IMarketingCrossSale {}
/** Матрица маркетинговых предложений CrossSale (Extension) */
export const MarketingCrossSale = t.model("MarketingCrossSale_469", {
  /** Порядок */
  Order: t.maybe(t.number),
});

type _IMarketingDevice = typeof MarketingDevice.Type;
/** Маркетинговое оборудование (Extension) */
export interface IMarketingDevice extends _IMarketingDevice {}
/** Маркетинговое оборудование (Extension) */
export const MarketingDevice = t.model("MarketingDevice_489", {
  /** Тип оборудования */
  DeviceType: t.maybe(t.reference(t.late(() => EquipmentType))),
  /** Сегменты */
  Segments: t.optional(t.array(t.late(() => Segment)), []),
  /** Вид связи */
  CommunicationType: t.maybe(t.reference(t.late(() => CommunicationType))),
});

type _IDevice = typeof Device.Type;
/** Оборудование (Extension) */
export interface IDevice extends _IDevice {}
/** Оборудование (Extension) */
export const Device = t.model("Device_490", {
  /** Загрузки */
  Downloads: t.optional(t.array(t.late(() => EquipmentDownload)), []),
  /** Состав комплекта */
  Inners: t.optional(t.array(t.late(() => Product)), []),
  /** Отложенная публикация на */
  FreezeDate: t.maybe(t.Date),
  /** Полное руководство пользователя (User guide) */
  FullUserGuide: t.maybe(FileModel),
  /** Краткое руководство пользователя (Quick start guide) */
  QuickStartGuide: t.maybe(FileModel),
});

type _IMarketingFixConnectAction = typeof MarketingFixConnectAction.Type;
/** Маркетинговые акции фиксированной связи (Extension) */
export interface IMarketingFixConnectAction extends _IMarketingFixConnectAction {}
/** Маркетинговые акции фиксированной связи (Extension) */
export const MarketingFixConnectAction = t.model("MarketingFixConnectAction_498", {
  /** Сегмент */
  Segment: t.optional(t.array(t.late(() => Segment)), []),
  /** Акция в Каталоге акций */
  MarketingAction: t.maybe(t.reference(t.late(() => MarketingProduct))),
  /**  */
  StartDate: t.maybe(t.Date),
  /**  */
  EndDate: t.maybe(t.Date),
  /**  */
  PromoPeriod: t.maybe(t.string),
  /**  */
  AfterPromo: t.maybe(t.string),
});

type _IFixConnectAction = typeof FixConnectAction.Type;
/** Акции фиксированной связи (Extension) */
export interface IFixConnectAction extends _IFixConnectAction {}
/** Акции фиксированной связи (Extension) */
export const FixConnectAction = t.model("FixConnectAction_500", {
  /**  */
  MarketingOffers: t.optional(t.array(t.late(() => MarketingProduct)), []),
  /**  */
  PromoPeriod: t.maybe(t.string),
  /**  */
  AfterPromo: t.maybe(t.string),
});

type _IMarketingTvPackage = typeof MarketingTvPackage.Type;
/** Маркетинговые ТВ-пакеты (Extension) */
export interface IMarketingTvPackage extends _IMarketingTvPackage {}
/** Маркетинговые ТВ-пакеты (Extension) */
export const MarketingTvPackage = t.model("MarketingTvPackage_502", {
  /** Каналы */
  Channels: t.optional(t.array(t.late(() => TvChannel)), []),
  /**  */
  TitleForSite: t.maybe(t.string),
  /** Тип пакета */
  PackageType: t.maybe(t.enumeration("PackageType", [
    "Base",
    "Additional",
  ])),
});

type _ITvPackage = typeof TvPackage.Type;
/** ТВ-пакеты (Extension) */
export interface ITvPackage extends _ITvPackage {}
/** ТВ-пакеты (Extension) */
export const TvPackage = t.model("TvPackage_503", {
});

type _IMarketingFixConnectTariff = typeof MarketingFixConnectTariff.Type;
/** Маркетинговые тарифы фиксированной связи (Extension) */
export interface IMarketingFixConnectTariff extends _IMarketingFixConnectTariff {}
/** Маркетинговые тарифы фиксированной связи (Extension) */
export const MarketingFixConnectTariff = t.model("MarketingFixConnectTariff_504", {
  /**  */
  Segment: t.maybe(t.reference(t.late(() => Segment))),
  /** Тип предложения (Категория тарифа) */
  Category: t.maybe(t.reference(t.late(() => TariffCategory))),
  /**  */
  MarketingDevices: t.optional(t.array(t.late(() => MarketingProduct)), []),
  /**  */
  BonusTVPackages: t.optional(t.array(t.late(() => MarketingProduct)), []),
  /**  */
  MarketingPhoneTariff: t.maybe(t.reference(t.late(() => MarketingProduct))),
  /**  */
  MarketingInternetTariff: t.maybe(t.reference(t.late(() => MarketingProduct))),
  /**  */
  MarketingTvPackage: t.maybe(t.reference(t.late(() => MarketingProduct))),
  /**  */
  TitleForSite: t.maybe(t.string),
});

type _IFixConnectTariff = typeof FixConnectTariff.Type;
/** Тарифы фиксированной связи (Extension) */
export interface IFixConnectTariff extends _IFixConnectTariff {}
/** Тарифы фиксированной связи (Extension) */
export const FixConnectTariff = t.model("FixConnectTariff_505", {
  /**  */
  TitleForSite: t.maybe(t.string),
});

type _IMarketingPhoneTariff = typeof MarketingPhoneTariff.Type;
/** Маркетинговые тарифы телефонии (Extension) */
export interface IMarketingPhoneTariff extends _IMarketingPhoneTariff {}
/** Маркетинговые тарифы телефонии (Extension) */
export const MarketingPhoneTariff = t.model("MarketingPhoneTariff_506", {
});

type _IPhoneTariff = typeof PhoneTariff.Type;
/** Тарифы телефонии (Extension) */
export interface IPhoneTariff extends _IPhoneTariff {}
/** Тарифы телефонии (Extension) */
export const PhoneTariff = t.model("PhoneTariff_507", {
  /** ВЗ вызовы (ссылка на Ростелеком) */
  RostelecomLink: t.maybe(t.string),
});

type _IMarketingInternetTariff = typeof MarketingInternetTariff.Type;
/** Маркетинговые тарифы интернет (Extension) */
export interface IMarketingInternetTariff extends _IMarketingInternetTariff {}
/** Маркетинговые тарифы интернет (Extension) */
export const MarketingInternetTariff = t.model("MarketingInternetTariff_509", {
});

type _IInternetTariff = typeof InternetTariff.Type;
/** Тарифы Интернет (Extension) */
export interface IInternetTariff extends _IInternetTariff {}
/** Тарифы Интернет (Extension) */
export const InternetTariff = t.model("InternetTariff_510", {
});

type _IDevicesForFixConnectAction = typeof DevicesForFixConnectAction.Type;
/** Акционное оборудование (Extension) */
export interface IDevicesForFixConnectAction extends _IDevicesForFixConnectAction {}
/** Акционное оборудование (Extension) */
export const DevicesForFixConnectAction = t.model("DevicesForFixConnectAction_512", {
  /**  */
  Order: t.maybe(t.number),
  /** Акция фиксированной связи */
  FixConnectAction: t.maybe(t.reference(t.late(() => Product))),
  /**  */
  Parent: t.maybe(t.reference(t.late(() => ProductRelation))),
  /** Маркетинговое оборудование */
  MarketingDevice: t.maybe(t.reference(t.late(() => MarketingProduct))),
});


export default {
  290: Region,
  339: Product,
  340: Group,
  342: ProductModifer,
  343: Tariff,
  346: TariffZone,
  347: Direction,
  350: BaseParameter,
  351: BaseParameterModifier,
  352: ParameterModifier,
  354: ProductParameter,
  355: Unit,
  360: LinkModifier,
  361: ProductRelation,
  362: LinkParameter,
  364: TariffTransfer,
  365: MutualGroup,
  378: ProductParameterGroup,
  383: MarketingProduct,
  385: MarketingTariff,
  402: MarketingService,
  403: Service,
  404: ServiceOnTariff,
  406: ServicesUpsale,
  407: TariffOptionPackage,
  413: ServiceRelation,
  415: CommunicationType,
  416: Segment,
  419: Action,
  420: MarketingAction,
  424: MarketingProductParameter,
  434: RoamingScale,
  435: MarketingRoamingScale,
  438: RoamingScaleOnTariff,
  441: TariffCategory,
  444: ServiceOnRoamingScale,
  446: Advantage,
  468: CrossSale,
  469: MarketingCrossSale,
  471: TimeZone,
  472: NetworkCity,
  478: ChannelCategory,
  479: ChannelType,
  480: ChannelFormat,
  482: TvChannel,
  488: ParameterChoice,
  489: MarketingDevice,
  490: Device,
  491: FixedType,
  493: EquipmentType,
  494: EquipmentDownload,
  498: MarketingFixConnectAction,
  500: FixConnectAction,
  502: MarketingTvPackage,
  503: TvPackage,
  504: MarketingFixConnectTariff,
  505: FixConnectTariff,
  506: MarketingPhoneTariff,
  507: PhoneTariff,
  509: MarketingInternetTariff,
  510: InternetTariff,
  511: DeviceOnTariffs,
  512: DevicesForFixConnectAction,
};
