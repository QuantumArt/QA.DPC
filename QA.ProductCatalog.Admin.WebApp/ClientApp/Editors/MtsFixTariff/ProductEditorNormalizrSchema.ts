import { schema } from "normalizr";
import { deepMerge } from "Utils/DeepMerge";

const options = { idAttribute: "Id", mergeStrategy: deepMerge };

/** Регионы */
export const RegionShape = new schema.Entity("Region", {}, options);
/** Продукты */
export const ProductShape = new schema.Entity("Product", {}, options);
/** Группы продуктов */
export const GroupShape = new schema.Entity("Group", {}, options);
/** Модификаторы продуктов */
export const ProductModiferShape = new schema.Entity("ProductModifer", {}, options);
/** Тарифные зоны */
export const TariffZoneShape = new schema.Entity("TariffZone", {}, options);
/** Направления соединения */
export const DirectionShape = new schema.Entity("Direction", {}, options);
/** Базовые параметры продуктов */
export const BaseParameterShape = new schema.Entity("BaseParameter", {}, options);
/** Модификаторы базовых параметров продуктов */
export const BaseParameterModifierShape = new schema.Entity("BaseParameterModifier", {}, options);
/** Модификаторы параметров продуктов */
export const ParameterModifierShape = new schema.Entity("ParameterModifier", {}, options);
/** Параметры продуктов */
export const ProductParameterShape = new schema.Entity("ProductParameter", {}, options);
/** Единицы измерения */
export const UnitShape = new schema.Entity("Unit", {}, options);
/** Модификаторы связей */
export const LinkModifierShape = new schema.Entity("LinkModifier", {}, options);
/** Матрица связей */
export const ProductRelationShape = new schema.Entity("ProductRelation", {}, options);
/** Параметры связей */
export const LinkParameterShape = new schema.Entity("LinkParameter", {}, options);
/** Группы параметров продуктов */
export const ProductParameterGroupShape = new schema.Entity("ProductParameterGroup", {}, options);
/** Маркетинговые продукты */
export const MarketingProductShape = new schema.Entity("MarketingProduct", {}, options);
/** Виды связи */
export const CommunicationTypeShape = new schema.Entity("CommunicationType", {}, options);
/** Сегменты */
export const SegmentShape = new schema.Entity("Segment", {}, options);
/** Параметры маркетинговых продуктов */
export const MarketingProductParameterShape = new schema.Entity("MarketingProductParameter", {}, options);
/** Категории тарифов */
export const TariffCategoryShape = new schema.Entity("TariffCategory", {}, options);
/** Преимущества маркетинговых продуктов */
export const AdvantageShape = new schema.Entity("Advantage", {}, options);
/** Часовые зоны */
export const TimeZoneShape = new schema.Entity("TimeZone", {}, options);
/** Города сети */
export const NetworkCityShape = new schema.Entity("NetworkCity", {}, options);
/** Категории каналов */
export const ChannelCategoryShape = new schema.Entity("ChannelCategory", {}, options);
/** Типы каналов */
export const ChannelTypeShape = new schema.Entity("ChannelType", {}, options);
/** Форматы каналов */
export const ChannelFormatShape = new schema.Entity("ChannelFormat", {}, options);
/** ТВ-каналы */
export const TvChannelShape = new schema.Entity("TvChannel", {}, options);
/** Варианты выбора для параметров */
export const ParameterChoiceShape = new schema.Entity("ParameterChoice", {}, options);
/** Типы фиксированной связи */
export const FixedTypeShape = new schema.Entity("FixedType", {}, options);
/** Типы оборудования */
export const EquipmentTypeShape = new schema.Entity("EquipmentType", {}, options);
/** Загрузки для оборудования */
export const EquipmentDownloadShape = new schema.Entity("EquipmentDownload", {}, options);
/** Оборудование на тарифах */
export const DeviceOnTariffsShape = new schema.Entity("DeviceOnTariffs", {}, options);

// Extensions

const TariffShape = new schema.Object({}); // Тарифы
const TariffTransferShape = new schema.Object({}); // Переходы с тарифа на тариф
const MutualGroupShape = new schema.Object({}); // Группы несовместимости услуг
const MarketingTariffShape = new schema.Object({}); // Маркетинговые тарифы
const MarketingServiceShape = new schema.Object({}); // Маркетинговые услуги
const ServiceShape = new schema.Object({}); // Услуги
const ServiceOnTariffShape = new schema.Object({}); // Услуги на тарифе
const ServicesUpsaleShape = new schema.Object({}); // Матрица предложений услуг Upsale
const TariffOptionPackageShape = new schema.Object({}); // Пакеты опций на тарифах
const ServiceRelationShape = new schema.Object({}); // Связи между услугами
const ActionShape = new schema.Object({}); // Акции
const MarketingActionShape = new schema.Object({}); // Маркетинговые акции
const RoamingScaleShape = new schema.Object({}); // Роуминговые сетки
const MarketingRoamingScaleShape = new schema.Object({}); // Маркетинговые роуминговые сетки
const RoamingScaleOnTariffShape = new schema.Object({}); // Роуминговые сетки для тарифа
const ServiceOnRoamingScaleShape = new schema.Object({}); // Услуги на роуминговой сетке
const CrossSaleShape = new schema.Object({}); // Матрица предложений CrossSale
const MarketingCrossSaleShape = new schema.Object({}); // Матрица маркетинговых предложений CrossSale
const MarketingDeviceShape = new schema.Object({}); // Маркетинговое оборудование
const DeviceShape = new schema.Object({}); // Оборудование
const MarketingFixConnectActionShape = new schema.Object({}); // Маркетинговые акции фиксированной связи
const FixConnectActionShape = new schema.Object({}); // Акции фиксированной связи
const MarketingTvPackageShape = new schema.Object({}); // Маркетинговые ТВ-пакеты
const TvPackageShape = new schema.Object({}); // ТВ-пакеты
const MarketingFixConnectTariffShape = new schema.Object({}); // Маркетинговые тарифы фиксированной связи
const FixConnectTariffShape = new schema.Object({}); // Тарифы фиксированной связи
const MarketingPhoneTariffShape = new schema.Object({}); // Маркетинговые тарифы телефонии
const PhoneTariffShape = new schema.Object({}); // Тарифы телефонии
const MarketingInternetTariffShape = new schema.Object({}); // Маркетинговые тарифы интернет
const InternetTariffShape = new schema.Object({}); // Тарифы Интернет
const DevicesForFixConnectActionShape = new schema.Object({}); // Акционное оборудование


// Регионы
RegionShape.define({
  Parent: RegionShape,
});

// Продукты
ProductShape.define({
  MarketingProduct: MarketingProductShape,
  Type_Contents: {
    Tariff: TariffShape,
    Service: ServiceShape,
    Action: ActionShape,
    RoamingScale: RoamingScaleShape,
    Device: DeviceShape,
    FixConnectAction: FixConnectActionShape,
    TvPackage: TvPackageShape,
    FixConnectTariff: FixConnectTariffShape,
    PhoneTariff: PhoneTariffShape,
    InternetTariff: InternetTariffShape,
  },
  Modifiers: [ProductModiferShape],
  Parameters: [ProductParameterShape],
  Regions: [RegionShape],
  FixConnectAction: [DevicesForFixConnectActionShape],
  Advantages: [AdvantageShape],
});

// Параметры продуктов
ProductParameterShape.define({
  Group: ProductParameterGroupShape,
  Parent: ProductParameterShape,
  BaseParameter: BaseParameterShape,
  Zone: TariffZoneShape,
  Direction: DirectionShape,
  BaseParameterModifiers: [BaseParameterModifierShape],
  Modifiers: [ParameterModifierShape],
  Unit: UnitShape,
  ProductGroup: GroupShape,
  Choice: ParameterChoiceShape,
});

// Матрица связей
ProductRelationShape.define({
  Modifiers: [LinkModifierShape],
  Parameters: [LinkParameterShape],
  Type_Contents: {
    TariffTransfer: TariffTransferShape,
    MutualGroup: MutualGroupShape,
    ServiceOnTariff: ServiceOnTariffShape,
    ServicesUpsale: ServicesUpsaleShape,
    TariffOptionPackage: TariffOptionPackageShape,
    ServiceRelation: ServiceRelationShape,
    RoamingScaleOnTariff: RoamingScaleOnTariffShape,
    ServiceOnRoamingScale: ServiceOnRoamingScaleShape,
    CrossSale: CrossSaleShape,
    MarketingCrossSale: MarketingCrossSaleShape,
    DeviceOnTariffs: DeviceOnTariffsShape,
    DevicesForFixConnectAction: DevicesForFixConnectActionShape,
  },
});

// Параметры связей
LinkParameterShape.define({
  Group: ProductParameterGroupShape,
  BaseParameter: BaseParameterShape,
  Zone: TariffZoneShape,
  Direction: DirectionShape,
  BaseParameterModifiers: [BaseParameterModifierShape],
  Modifiers: [ParameterModifierShape],
  Unit: UnitShape,
  ProductGroup: GroupShape,
  Choice: ParameterChoiceShape,
});

// Маркетинговые продукты
MarketingProductShape.define({
  Modifiers: [ProductModiferShape],
  Advantages: [AdvantageShape],
  Type_Contents: {
    MarketingTariff: MarketingTariffShape,
    MarketingService: MarketingServiceShape,
    MarketingAction: MarketingActionShape,
    MarketingRoamingScale: MarketingRoamingScaleShape,
    MarketingDevice: MarketingDeviceShape,
    MarketingFixConnectAction: MarketingFixConnectActionShape,
    MarketingTvPackage: MarketingTvPackageShape,
    MarketingFixConnectTariff: MarketingFixConnectTariffShape,
    MarketingPhoneTariff: MarketingPhoneTariffShape,
    MarketingInternetTariff: MarketingInternetTariffShape,
  },
  Parameters: [MarketingProductParameterShape],
  TariffsOnMarketingDevice: [DeviceOnTariffsShape],
  DevicesOnMarketingTariff: [DeviceOnTariffsShape],
  ActionsOnMarketingDevice: [DevicesForFixConnectActionShape],
});

// Параметры маркетинговых продуктов
MarketingProductParameterShape.define({
  Group: ProductParameterGroupShape,
  BaseParameter: BaseParameterShape,
  Zone: TariffZoneShape,
  Direction: DirectionShape,
  BaseParameterModifiers: [BaseParameterModifierShape],
  Modifiers: [ParameterModifierShape],
  Unit: UnitShape,
  Choice: ParameterChoiceShape,
});

// Категории тарифов
TariffCategoryShape.define({
  ConnectionTypes: [FixedTypeShape],
});

// Города сети
NetworkCityShape.define({
  City: RegionShape,
});

// ТВ-каналы
TvChannelShape.define({
  Category: ChannelCategoryShape,
  ChannelType: ChannelTypeShape,
  Cities: [NetworkCityShape],
  Format: ChannelFormatShape,
  Parent: TvChannelShape,
  Children: [TvChannelShape],
  TimeZone: TimeZoneShape,
});

// Маркетинговое оборудование
MarketingDeviceShape.define({
  DeviceType: EquipmentTypeShape,
  Segments: [SegmentShape],
  CommunicationType: CommunicationTypeShape,
});

// Оборудование
DeviceShape.define({
  Downloads: [EquipmentDownloadShape],
  Inners: [ProductShape],
});

// Типы оборудования
EquipmentTypeShape.define({
  ConnectionType: FixedTypeShape,
});

// Маркетинговые акции фиксированной связи
MarketingFixConnectActionShape.define({
  Segment: [SegmentShape],
  MarketingAction: MarketingProductShape,
});

// Акции фиксированной связи
FixConnectActionShape.define({
  MarketingOffers: [MarketingProductShape],
});

// Маркетинговые ТВ-пакеты
MarketingTvPackageShape.define({
  Channels: [TvChannelShape],
});

// Маркетинговые тарифы фиксированной связи
MarketingFixConnectTariffShape.define({
  Segment: SegmentShape,
  Category: TariffCategoryShape,
  MarketingDevices: [MarketingProductShape],
  BonusTVPackages: [MarketingProductShape],
  MarketingPhoneTariff: MarketingProductShape,
  MarketingInternetTariff: MarketingProductShape,
  MarketingTvPackage: MarketingProductShape,
});

// Оборудование на тарифах
DeviceOnTariffsShape.define({
  Parent: ProductRelationShape,
  MarketingDevice: MarketingProductShape,
  MarketingTariffs: [MarketingProductShape],
  Cities: [RegionShape],
});

// Акционное оборудование
DevicesForFixConnectActionShape.define({
  FixConnectAction: ProductShape,
  Parent: ProductRelationShape,
  MarketingDevice: MarketingProductShape,
});


/** Shapes by ContentName */
export default {
  Region: RegionShape,
  Product: ProductShape,
  Group: GroupShape,
  ProductModifer: ProductModiferShape,
  TariffZone: TariffZoneShape,
  Direction: DirectionShape,
  BaseParameter: BaseParameterShape,
  BaseParameterModifier: BaseParameterModifierShape,
  ParameterModifier: ParameterModifierShape,
  ProductParameter: ProductParameterShape,
  Unit: UnitShape,
  LinkModifier: LinkModifierShape,
  ProductRelation: ProductRelationShape,
  LinkParameter: LinkParameterShape,
  ProductParameterGroup: ProductParameterGroupShape,
  MarketingProduct: MarketingProductShape,
  CommunicationType: CommunicationTypeShape,
  Segment: SegmentShape,
  MarketingProductParameter: MarketingProductParameterShape,
  TariffCategory: TariffCategoryShape,
  Advantage: AdvantageShape,
  TimeZone: TimeZoneShape,
  NetworkCity: NetworkCityShape,
  ChannelCategory: ChannelCategoryShape,
  ChannelType: ChannelTypeShape,
  ChannelFormat: ChannelFormatShape,
  TvChannel: TvChannelShape,
  ParameterChoice: ParameterChoiceShape,
  FixedType: FixedTypeShape,
  EquipmentType: EquipmentTypeShape,
  EquipmentDownload: EquipmentDownloadShape,
  DeviceOnTariffs: DeviceOnTariffsShape,
};
