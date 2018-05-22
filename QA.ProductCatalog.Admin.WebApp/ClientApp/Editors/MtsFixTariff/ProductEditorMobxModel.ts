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
export const Region = t.model("Region", {
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
export const Product = t.model("Product", {
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
  Type_Contents: t.optional(t.model({
    Tariff: t.optional(t.frozen, {}),
    Service: t.optional(t.frozen, {}),
    Action: t.optional(t.frozen, {}),
    RoamingScale: t.optional(t.frozen, {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // Device: t.optional(t.late(() => Device), {})
    /** Оборудование */
    Device: t.optional(t.model({
      Downloads: t.optional(t.array(t.reference(t.late(() => EquipmentDownload))), []),
      Inners: t.optional(t.array(t.reference(t.late(() => Product))), []),
      FreezeDate: t.maybe(t.Date),
      FullUserGuide: t.maybe(FileModel),
      QuickStartGuide: t.maybe(FileModel),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // FixConnectAction: t.optional(t.late(() => FixConnectAction), {})
    /** Акции фиксированной связи */
    FixConnectAction: t.optional(t.model({
      MarketingOffers: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
      PromoPeriod: t.maybe(t.string),
      AfterPromo: t.maybe(t.string),
    }), {}),
    TvPackage: t.optional(t.frozen, {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // FixConnectTariff: t.optional(t.late(() => FixConnectTariff), {})
    /** Тарифы фиксированной связи */
    FixConnectTariff: t.optional(t.model({
      TitleForSite: t.maybe(t.string),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // PhoneTariff: t.optional(t.late(() => PhoneTariff), {})
    /** Тарифы телефонии */
    PhoneTariff: t.optional(t.model({
      RostelecomLink: t.maybe(t.string),
    }), {}),
    InternetTariff: t.optional(t.frozen, {}),
  }), {}),
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
  Modifiers: t.optional(t.array(t.reference(t.late(() => ProductModifer))), []),
  /** Параметры продукта */
  Parameters: t.optional(t.array(t.reference(t.late(() => ProductParameter))), []),
  /** Регионы */
  Regions: t.optional(t.array(t.reference(t.late(() => Region))), []),
  /** Акция фиксированной связи */
  FixConnectAction: t.optional(t.array(t.reference(t.late(() => DevicesForFixConnectAction))), []),
  /** Преимущества */
  Advantages: t.optional(t.array(t.reference(t.late(() => Advantage))), []),
});

type _IGroup = typeof Group.Type;
/** Группы продуктов */
export interface IGroup extends _IGroup {}
/** Группы продуктов */
export const Group = t.model("Group", {
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
export const ProductModifer = t.model("ProductModifer", {
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
export const TariffZone = t.model("TariffZone", {
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
export const Direction = t.model("Direction", {
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
export const BaseParameter = t.model("BaseParameter", {
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
export const BaseParameterModifier = t.model("BaseParameterModifier", {
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
export const ParameterModifier = t.model("ParameterModifier", {
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
export const ProductParameter = t.model("ProductParameter", {
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
  BaseParameterModifiers: t.optional(t.array(t.reference(t.late(() => BaseParameterModifier))), []),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.reference(t.late(() => ParameterModifier))), []),
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
export const Unit = t.model("Unit", {
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
export const LinkModifier = t.model("LinkModifier", {
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
export const ProductRelation = t.model("ProductRelation", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Название */
  Title: t.maybe(t.string),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.reference(t.late(() => LinkModifier))), []),
  /** Параметры */
  Parameters: t.optional(t.array(t.reference(t.late(() => LinkParameter))), []),
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
  Type_Contents: t.optional(t.model({
    TariffTransfer: t.optional(t.frozen, {}),
    MutualGroup: t.optional(t.frozen, {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // ServiceOnTariff: t.optional(t.late(() => ServiceOnTariff), {})
    /** Услуги на тарифе */
    ServiceOnTariff: t.optional(t.model({
      Description: t.maybe(t.string),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // ServicesUpsale: t.optional(t.late(() => ServicesUpsale), {})
    /** Матрица предложений услуг Upsale */
    ServicesUpsale: t.optional(t.model({
      Order: t.maybe(t.number),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // TariffOptionPackage: t.optional(t.late(() => TariffOptionPackage), {})
    /** Пакеты опций на тарифах */
    TariffOptionPackage: t.optional(t.model({
      SubTitle: t.maybe(t.string),
      Description: t.maybe(t.string),
      Alias: t.maybe(t.string),
      Link: t.maybe(t.string),
    }), {}),
    ServiceRelation: t.optional(t.frozen, {}),
    RoamingScaleOnTariff: t.optional(t.frozen, {}),
    ServiceOnRoamingScale: t.optional(t.frozen, {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // CrossSale: t.optional(t.late(() => CrossSale), {})
    /** Матрица предложений CrossSale */
    CrossSale: t.optional(t.model({
      Order: t.maybe(t.number),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // MarketingCrossSale: t.optional(t.late(() => MarketingCrossSale), {})
    /** Матрица маркетинговых предложений CrossSale */
    MarketingCrossSale: t.optional(t.model({
      Order: t.maybe(t.number),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // DeviceOnTariffs: t.optional(t.late(() => DeviceOnTariffs), {})
    /** Оборудование на тарифах */
    DeviceOnTariffs: t.optional(t.model({
      Parent: t.maybe(t.reference(t.late(() => ProductRelation))),
      Order: t.maybe(t.number),
      MarketingDevice: t.maybe(t.reference(t.late(() => MarketingProduct))),
      MarketingTariffs: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
      Cities: t.optional(t.array(t.reference(t.late(() => Region))), []),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // DevicesForFixConnectAction: t.optional(t.late(() => DevicesForFixConnectAction), {})
    /** Акционное оборудование */
    DevicesForFixConnectAction: t.optional(t.model({
      Order: t.maybe(t.number),
      FixConnectAction: t.maybe(t.reference(t.late(() => Product))),
      Parent: t.maybe(t.reference(t.late(() => ProductRelation))),
      MarketingDevice: t.maybe(t.reference(t.late(() => MarketingProduct))),
    }), {}),
  }), {}),
});

type _ILinkParameter = typeof LinkParameter.Type;
/** Параметры связей */
export interface ILinkParameter extends _ILinkParameter {}
/** Параметры связей */
export const LinkParameter = t.model("LinkParameter", {
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
  BaseParameterModifiers: t.optional(t.array(t.reference(t.late(() => BaseParameterModifier))), []),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.reference(t.late(() => ParameterModifier))), []),
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
export const ProductParameterGroup = t.model("ProductParameterGroup", {
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
export const MarketingProduct = t.model("MarketingProduct", {
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
  Modifiers: t.optional(t.array(t.reference(t.late(() => ProductModifer))), []),
  /** Порядок */
  SortOrder: t.maybe(t.number),
  /** Приоритет (популярность) */
  Priority: t.maybe(t.number),
  /** Преимущества */
  Advantages: t.optional(t.array(t.reference(t.late(() => Advantage))), []),
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
  Type_Contents: t.optional(t.model({
    MarketingTariff: t.optional(t.frozen, {}),
    MarketingService: t.optional(t.frozen, {}),
    MarketingAction: t.optional(t.frozen, {}),
    MarketingRoamingScale: t.optional(t.frozen, {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // MarketingDevice: t.optional(t.late(() => MarketingDevice), {})
    /** Маркетинговое оборудование */
    MarketingDevice: t.optional(t.model({
      DeviceType: t.maybe(t.reference(t.late(() => EquipmentType))),
      Segments: t.optional(t.array(t.reference(t.late(() => Segment))), []),
      CommunicationType: t.maybe(t.reference(t.late(() => CommunicationType))),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // MarketingFixConnectAction: t.optional(t.late(() => MarketingFixConnectAction), {})
    /** Маркетинговые акции фиксированной связи */
    MarketingFixConnectAction: t.optional(t.model({
      Segment: t.optional(t.array(t.reference(t.late(() => Segment))), []),
      MarketingAction: t.maybe(t.reference(t.late(() => MarketingProduct))),
      StartDate: t.maybe(t.Date),
      EndDate: t.maybe(t.Date),
      PromoPeriod: t.maybe(t.string),
      AfterPromo: t.maybe(t.string),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // MarketingTvPackage: t.optional(t.late(() => MarketingTvPackage), {})
    /** Маркетинговые ТВ-пакеты */
    MarketingTvPackage: t.optional(t.model({
      Channels: t.optional(t.array(t.reference(t.late(() => TvChannel))), []),
      TitleForSite: t.maybe(t.string),
      PackageType: t.maybe(t.enumeration("PackageType", [
    "Base",
    "Additional",
  ])),
    }), {}),
    // https://github.com/mobxjs/mobx-state-tree/issues/825
    // MarketingFixConnectTariff: t.optional(t.late(() => MarketingFixConnectTariff), {})
    /** Маркетинговые тарифы фиксированной связи */
    MarketingFixConnectTariff: t.optional(t.model({
      Segment: t.maybe(t.reference(t.late(() => Segment))),
      Category: t.maybe(t.reference(t.late(() => TariffCategory))),
      MarketingDevices: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
      BonusTVPackages: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
      MarketingPhoneTariff: t.maybe(t.reference(t.late(() => MarketingProduct))),
      MarketingInternetTariff: t.maybe(t.reference(t.late(() => MarketingProduct))),
      MarketingTvPackage: t.maybe(t.reference(t.late(() => MarketingProduct))),
      TitleForSite: t.maybe(t.string),
    }), {}),
    MarketingPhoneTariff: t.optional(t.frozen, {}),
    MarketingInternetTariff: t.optional(t.frozen, {}),
  }), {}),
  /**  */
  FullDescription: t.maybe(t.string),
  /** Параметры маркетингового продукта */
  Parameters: t.optional(t.array(t.reference(t.late(() => MarketingProductParameter))), []),
  /** Маркетинговое устройство */
  TariffsOnMarketingDevice: t.optional(t.array(t.reference(t.late(() => DeviceOnTariffs))), []),
  /** Маркетинговые тарифы */
  DevicesOnMarketingTariff: t.optional(t.array(t.reference(t.late(() => DeviceOnTariffs))), []),
  /** Маркетинговое оборудование */
  ActionsOnMarketingDevice: t.optional(t.array(t.reference(t.late(() => DevicesForFixConnectAction))), []),
  /** Ссылка */
  Link: t.maybe(t.string),
  /** Подробное описание */
  DetailedDescription: t.maybe(t.string),
});

type _ICommunicationType = typeof CommunicationType.Type;
/** Виды связи */
export interface ICommunicationType extends _ICommunicationType {}
/** Виды связи */
export const CommunicationType = t.model("CommunicationType", {
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
export const Segment = t.model("Segment", {
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
export const MarketingProductParameter = t.model("MarketingProductParameter", {
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
  BaseParameterModifiers: t.optional(t.array(t.reference(t.late(() => BaseParameterModifier))), []),
  /** Модификаторы */
  Modifiers: t.optional(t.array(t.reference(t.late(() => ParameterModifier))), []),
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
export const TariffCategory = t.model("TariffCategory", {
  /** Идентификатор статьи */
  Id: t.identifier(t.number),
  /** Время последней модификации статьи */
  Timestamp: t.maybe(t.Date),
  /** Типы связи */
  ConnectionTypes: t.optional(t.array(t.reference(t.late(() => FixedType))), []),
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
export const Advantage = t.model("Advantage", {
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
export const TimeZone = t.model("TimeZone", {
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
export const NetworkCity = t.model("NetworkCity", {
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
export const ChannelCategory = t.model("ChannelCategory", {
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
export const ChannelType = t.model("ChannelType", {
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
export const ChannelFormat = t.model("ChannelFormat", {
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
export const TvChannel = t.model("TvChannel", {
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
  Cities: t.optional(t.array(t.reference(t.late(() => NetworkCity))), []),
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
  Children: t.optional(t.array(t.reference(t.late(() => TvChannel))), []),
  /** Лого 40х30 */
  Logo40x30: t.maybe(t.string),
  /** Часовая зона (UTC) */
  TimeZone: t.maybe(t.reference(t.late(() => TimeZone))),
});

type _IParameterChoice = typeof ParameterChoice.Type;
/** Варианты выбора для параметров */
export interface IParameterChoice extends _IParameterChoice {}
/** Варианты выбора для параметров */
export const ParameterChoice = t.model("ParameterChoice", {
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
export const FixedType = t.model("FixedType", {
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
export const EquipmentType = t.model("EquipmentType", {
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
export const EquipmentDownload = t.model("EquipmentDownload", {
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
export const DeviceOnTariffs = t.model("DeviceOnTariffs", {
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
  MarketingTariffs: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
  /** Города */
  Cities: t.optional(t.array(t.reference(t.late(() => Region))), []),
});


type _ITariff = typeof Tariff.Type;
/** Тарифы (Extension) */
export interface ITariff extends _ITariff {}
/** Тарифы (Extension) */
export const Tariff = t.model("Tariff", {
  // no fields
});

type _ITariffTransfer = typeof TariffTransfer.Type;
/** Переходы с тарифа на тариф (Extension) */
export interface ITariffTransfer extends _ITariffTransfer {}
/** Переходы с тарифа на тариф (Extension) */
export const TariffTransfer = t.model("TariffTransfer", {
  // no fields
});

type _IMutualGroup = typeof MutualGroup.Type;
/** Группы несовместимости услуг (Extension) */
export interface IMutualGroup extends _IMutualGroup {}
/** Группы несовместимости услуг (Extension) */
export const MutualGroup = t.model("MutualGroup", {
  // no fields
});

type _IMarketingTariff = typeof MarketingTariff.Type;
/** Маркетинговые тарифы (Extension) */
export interface IMarketingTariff extends _IMarketingTariff {}
/** Маркетинговые тарифы (Extension) */
export const MarketingTariff = t.model("MarketingTariff", {
  // no fields
});

type _IMarketingService = typeof MarketingService.Type;
/** Маркетинговые услуги (Extension) */
export interface IMarketingService extends _IMarketingService {}
/** Маркетинговые услуги (Extension) */
export const MarketingService = t.model("MarketingService", {
  // no fields
});

type _IService = typeof Service.Type;
/** Услуги (Extension) */
export interface IService extends _IService {}
/** Услуги (Extension) */
export const Service = t.model("Service", {
  // no fields
});

type _IServiceOnTariff = typeof ServiceOnTariff.Type;
/** Услуги на тарифе (Extension) */
export interface IServiceOnTariff extends _IServiceOnTariff {}
/** Услуги на тарифе (Extension) */
export const ServiceOnTariff = t.model("ServiceOnTariff", {
  /**  */
  Description: t.maybe(t.string),
});

type _IServicesUpsale = typeof ServicesUpsale.Type;
/** Матрица предложений услуг Upsale (Extension) */
export interface IServicesUpsale extends _IServicesUpsale {}
/** Матрица предложений услуг Upsale (Extension) */
export const ServicesUpsale = t.model("ServicesUpsale", {
  /**  */
  Order: t.maybe(t.number),
});

type _ITariffOptionPackage = typeof TariffOptionPackage.Type;
/** Пакеты опций на тарифах (Extension) */
export interface ITariffOptionPackage extends _ITariffOptionPackage {}
/** Пакеты опций на тарифах (Extension) */
export const TariffOptionPackage = t.model("TariffOptionPackage", {
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
export const ServiceRelation = t.model("ServiceRelation", {
  // no fields
});

type _IAction = typeof Action.Type;
/** Акции (Extension) */
export interface IAction extends _IAction {}
/** Акции (Extension) */
export const Action = t.model("Action", {
  // no fields
});

type _IMarketingAction = typeof MarketingAction.Type;
/** Маркетинговые акции (Extension) */
export interface IMarketingAction extends _IMarketingAction {}
/** Маркетинговые акции (Extension) */
export const MarketingAction = t.model("MarketingAction", {
  // no fields
});

type _IRoamingScale = typeof RoamingScale.Type;
/** Роуминговые сетки (Extension) */
export interface IRoamingScale extends _IRoamingScale {}
/** Роуминговые сетки (Extension) */
export const RoamingScale = t.model("RoamingScale", {
  // no fields
});

type _IMarketingRoamingScale = typeof MarketingRoamingScale.Type;
/** Маркетинговые роуминговые сетки (Extension) */
export interface IMarketingRoamingScale extends _IMarketingRoamingScale {}
/** Маркетинговые роуминговые сетки (Extension) */
export const MarketingRoamingScale = t.model("MarketingRoamingScale", {
  // no fields
});

type _IRoamingScaleOnTariff = typeof RoamingScaleOnTariff.Type;
/** Роуминговые сетки для тарифа (Extension) */
export interface IRoamingScaleOnTariff extends _IRoamingScaleOnTariff {}
/** Роуминговые сетки для тарифа (Extension) */
export const RoamingScaleOnTariff = t.model("RoamingScaleOnTariff", {
  // no fields
});

type _IServiceOnRoamingScale = typeof ServiceOnRoamingScale.Type;
/** Услуги на роуминговой сетке (Extension) */
export interface IServiceOnRoamingScale extends _IServiceOnRoamingScale {}
/** Услуги на роуминговой сетке (Extension) */
export const ServiceOnRoamingScale = t.model("ServiceOnRoamingScale", {
  // no fields
});

type _ICrossSale = typeof CrossSale.Type;
/** Матрица предложений CrossSale (Extension) */
export interface ICrossSale extends _ICrossSale {}
/** Матрица предложений CrossSale (Extension) */
export const CrossSale = t.model("CrossSale", {
  /** Порядок */
  Order: t.maybe(t.number),
});

type _IMarketingCrossSale = typeof MarketingCrossSale.Type;
/** Матрица маркетинговых предложений CrossSale (Extension) */
export interface IMarketingCrossSale extends _IMarketingCrossSale {}
/** Матрица маркетинговых предложений CrossSale (Extension) */
export const MarketingCrossSale = t.model("MarketingCrossSale", {
  /** Порядок */
  Order: t.maybe(t.number),
});

type _IMarketingDevice = typeof MarketingDevice.Type;
/** Маркетинговое оборудование (Extension) */
export interface IMarketingDevice extends _IMarketingDevice {}
/** Маркетинговое оборудование (Extension) */
export const MarketingDevice = t.model("MarketingDevice", {
  /** Тип оборудования */
  DeviceType: t.maybe(t.reference(t.late(() => EquipmentType))),
  /** Сегменты */
  Segments: t.optional(t.array(t.reference(t.late(() => Segment))), []),
  /** Вид связи */
  CommunicationType: t.maybe(t.reference(t.late(() => CommunicationType))),
});

type _IDevice = typeof Device.Type;
/** Оборудование (Extension) */
export interface IDevice extends _IDevice {}
/** Оборудование (Extension) */
export const Device = t.model("Device", {
  /** Загрузки */
  Downloads: t.optional(t.array(t.reference(t.late(() => EquipmentDownload))), []),
  /** Состав комплекта */
  Inners: t.optional(t.array(t.reference(t.late(() => Product))), []),
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
export const MarketingFixConnectAction = t.model("MarketingFixConnectAction", {
  /** Сегмент */
  Segment: t.optional(t.array(t.reference(t.late(() => Segment))), []),
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
export const FixConnectAction = t.model("FixConnectAction", {
  /**  */
  MarketingOffers: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
  /**  */
  PromoPeriod: t.maybe(t.string),
  /**  */
  AfterPromo: t.maybe(t.string),
});

type _IMarketingTvPackage = typeof MarketingTvPackage.Type;
/** Маркетинговые ТВ-пакеты (Extension) */
export interface IMarketingTvPackage extends _IMarketingTvPackage {}
/** Маркетинговые ТВ-пакеты (Extension) */
export const MarketingTvPackage = t.model("MarketingTvPackage", {
  /** Каналы */
  Channels: t.optional(t.array(t.reference(t.late(() => TvChannel))), []),
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
export const TvPackage = t.model("TvPackage", {
  // no fields
});

type _IMarketingFixConnectTariff = typeof MarketingFixConnectTariff.Type;
/** Маркетинговые тарифы фиксированной связи (Extension) */
export interface IMarketingFixConnectTariff extends _IMarketingFixConnectTariff {}
/** Маркетинговые тарифы фиксированной связи (Extension) */
export const MarketingFixConnectTariff = t.model("MarketingFixConnectTariff", {
  /**  */
  Segment: t.maybe(t.reference(t.late(() => Segment))),
  /** Тип предложения (Категория тарифа) */
  Category: t.maybe(t.reference(t.late(() => TariffCategory))),
  /**  */
  MarketingDevices: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
  /**  */
  BonusTVPackages: t.optional(t.array(t.reference(t.late(() => MarketingProduct))), []),
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
export const FixConnectTariff = t.model("FixConnectTariff", {
  /**  */
  TitleForSite: t.maybe(t.string),
});

type _IMarketingPhoneTariff = typeof MarketingPhoneTariff.Type;
/** Маркетинговые тарифы телефонии (Extension) */
export interface IMarketingPhoneTariff extends _IMarketingPhoneTariff {}
/** Маркетинговые тарифы телефонии (Extension) */
export const MarketingPhoneTariff = t.model("MarketingPhoneTariff", {
  // no fields
});

type _IPhoneTariff = typeof PhoneTariff.Type;
/** Тарифы телефонии (Extension) */
export interface IPhoneTariff extends _IPhoneTariff {}
/** Тарифы телефонии (Extension) */
export const PhoneTariff = t.model("PhoneTariff", {
  /** ВЗ вызовы (ссылка на Ростелеком) */
  RostelecomLink: t.maybe(t.string),
});

type _IMarketingInternetTariff = typeof MarketingInternetTariff.Type;
/** Маркетинговые тарифы интернет (Extension) */
export interface IMarketingInternetTariff extends _IMarketingInternetTariff {}
/** Маркетинговые тарифы интернет (Extension) */
export const MarketingInternetTariff = t.model("MarketingInternetTariff", {
  // no fields
});

type _IInternetTariff = typeof InternetTariff.Type;
/** Тарифы Интернет (Extension) */
export interface IInternetTariff extends _IInternetTariff {}
/** Тарифы Интернет (Extension) */
export const InternetTariff = t.model("InternetTariff", {
  // no fields
});

type _IDevicesForFixConnectAction = typeof DevicesForFixConnectAction.Type;
/** Акционное оборудование (Extension) */
export interface IDevicesForFixConnectAction extends _IDevicesForFixConnectAction {}
/** Акционное оборудование (Extension) */
export const DevicesForFixConnectAction = t.model("DevicesForFixConnectAction", {
  /**  */
  Order: t.maybe(t.number),
  /** Акция фиксированной связи */
  FixConnectAction: t.maybe(t.reference(t.late(() => Product))),
  /**  */
  Parent: t.maybe(t.reference(t.late(() => ProductRelation))),
  /** Маркетинговое оборудование */
  MarketingDevice: t.maybe(t.reference(t.late(() => MarketingProduct))),
});


export default t.model({
  Region: t.optional(t.map(Region), {}),
  Product: t.optional(t.map(Product), {}),
  Group: t.optional(t.map(Group), {}),
  ProductModifer: t.optional(t.map(ProductModifer), {}),
  TariffZone: t.optional(t.map(TariffZone), {}),
  Direction: t.optional(t.map(Direction), {}),
  BaseParameter: t.optional(t.map(BaseParameter), {}),
  BaseParameterModifier: t.optional(t.map(BaseParameterModifier), {}),
  ParameterModifier: t.optional(t.map(ParameterModifier), {}),
  ProductParameter: t.optional(t.map(ProductParameter), {}),
  Unit: t.optional(t.map(Unit), {}),
  LinkModifier: t.optional(t.map(LinkModifier), {}),
  ProductRelation: t.optional(t.map(ProductRelation), {}),
  LinkParameter: t.optional(t.map(LinkParameter), {}),
  ProductParameterGroup: t.optional(t.map(ProductParameterGroup), {}),
  MarketingProduct: t.optional(t.map(MarketingProduct), {}),
  CommunicationType: t.optional(t.map(CommunicationType), {}),
  Segment: t.optional(t.map(Segment), {}),
  MarketingProductParameter: t.optional(t.map(MarketingProductParameter), {}),
  TariffCategory: t.optional(t.map(TariffCategory), {}),
  Advantage: t.optional(t.map(Advantage), {}),
  TimeZone: t.optional(t.map(TimeZone), {}),
  NetworkCity: t.optional(t.map(NetworkCity), {}),
  ChannelCategory: t.optional(t.map(ChannelCategory), {}),
  ChannelType: t.optional(t.map(ChannelType), {}),
  ChannelFormat: t.optional(t.map(ChannelFormat), {}),
  TvChannel: t.optional(t.map(TvChannel), {}),
  ParameterChoice: t.optional(t.map(ParameterChoice), {}),
  FixedType: t.optional(t.map(FixedType), {}),
  EquipmentType: t.optional(t.map(EquipmentType), {}),
  EquipmentDownload: t.optional(t.map(EquipmentDownload), {}),
  DeviceOnTariffs: t.optional(t.map(DeviceOnTariffs), {}),
});
