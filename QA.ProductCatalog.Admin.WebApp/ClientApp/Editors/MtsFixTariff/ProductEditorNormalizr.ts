import { schema as s } from "normalizr";

/** Регионы */
export const region = new s.Entity("regions", {}, { idAttribute: "Id" });
/** Продукты */
export const product = new s.Entity("products", {}, { idAttribute: "Id" });
/** Группы продуктов */
export const group = new s.Entity("groups", {}, { idAttribute: "Id" });
/** Модификаторы продуктов */
export const productModifer = new s.Entity("productModifers", {}, { idAttribute: "Id" });
/** Тарифные зоны */
export const tariffZone = new s.Entity("tariffZones", {}, { idAttribute: "Id" });
/** Направления соединения */
export const direction = new s.Entity("directions", {}, { idAttribute: "Id" });
/** Базовые параметры продуктов */
export const baseParameter = new s.Entity("baseParameters", {}, { idAttribute: "Id" });
/** Модификаторы базовых параметров продуктов */
export const baseParameterModifier = new s.Entity(
  "baseParameterModifiers",
  {},
  { idAttribute: "Id" }
);
/** Модификаторы параметров продуктов */
export const parameterModifier = new s.Entity("parameterModifiers", {}, { idAttribute: "Id" });
/** Параметры продуктов */
export const productParameter = new s.Entity("productParameters", {}, { idAttribute: "Id" });
/** Единицы измерения */
export const unit = new s.Entity("units", {}, { idAttribute: "Id" });
/** Модификаторы связей */
export const linkModifier = new s.Entity("linkModifiers", {}, { idAttribute: "Id" });
/** Матрица связей */
export const productRelation = new s.Entity("productRelations", {}, { idAttribute: "Id" });
/** Параметры связей */
export const linkParameter = new s.Entity("linkParameters", {}, { idAttribute: "Id" });
/** Группы параметров продуктов */
export const productParameterGroup = new s.Entity(
  "productParameterGroups",
  {},
  { idAttribute: "Id" }
);
/** Маркетинговые продукты */
export const marketingProduct = new s.Entity("marketingProducts", {}, { idAttribute: "Id" });
/** Виды связи */
export const communicationType = new s.Entity("communicationTypes", {}, { idAttribute: "Id" });
/** Сегменты */
export const segment = new s.Entity("segments", {}, { idAttribute: "Id" });
/** Параметры маркетинговых продуктов */
export const marketingProductParameter = new s.Entity(
  "marketingProductParameters",
  {},
  { idAttribute: "Id" }
);
/** Категории тарифов */
export const tariffCategory = new s.Entity("tariffCategorys", {}, { idAttribute: "Id" });
/** Преимущества маркетинговых продуктов */
export const advantage = new s.Entity("advantages", {}, { idAttribute: "Id" });
/** Часовые зоны */
export const timeZone = new s.Entity("timeZones", {}, { idAttribute: "Id" });
/** Города сети */
export const networkCity = new s.Entity("networkCitys", {}, { idAttribute: "Id" });
/** Категории каналов */
export const channelCategory = new s.Entity("channelCategorys", {}, { idAttribute: "Id" });
/** Типы каналов */
export const channelType = new s.Entity("channelTypes", {}, { idAttribute: "Id" });
/** Форматы каналов */
export const channelFormat = new s.Entity("channelFormats", {}, { idAttribute: "Id" });
/** ТВ-каналы */
export const tvChannel = new s.Entity("tvChannels", {}, { idAttribute: "Id" });
/** Варианты выбора для параметров */
export const parameterChoice = new s.Entity("parameterChoices", {}, { idAttribute: "Id" });
/** Типы фиксированной связи */
export const fixedType = new s.Entity("fixedTypes", {}, { idAttribute: "Id" });
/** Типы оборудования */
export const equipmentType = new s.Entity("equipmentTypes", {}, { idAttribute: "Id" });
/** Загрузки для оборудования */
export const equipmentDownload = new s.Entity("equipmentDownloads", {}, { idAttribute: "Id" });
/** Оборудование на тарифах */
export const deviceOnTariffs = new s.Entity("deviceOnTariffss", {}, { idAttribute: "Id" });

// Extensions

const tariff = new s.Object({}); // Тарифы
const tariffTransfer = new s.Object({}); // Переходы с тарифа на тариф
const mutualGroup = new s.Object({}); // Группы несовместимости услуг
const marketingTariff = new s.Object({}); // Маркетинговые тарифы
const marketingService = new s.Object({}); // Маркетинговые услуги
const service = new s.Object({}); // Услуги
const serviceOnTariff = new s.Object({}); // Услуги на тарифе
const servicesUpsale = new s.Object({}); // Матрица предложений услуг Upsale
const tariffOptionPackage = new s.Object({}); // Пакеты опций на тарифах
const serviceRelation = new s.Object({}); // Связи между услугами
const action = new s.Object({}); // Акции
const marketingAction = new s.Object({}); // Маркетинговые акции
const roamingScale = new s.Object({}); // Роуминговые сетки
const marketingRoamingScale = new s.Object({}); // Маркетинговые роуминговые сетки
const roamingScaleOnTariff = new s.Object({}); // Роуминговые сетки для тарифа
const serviceOnRoamingScale = new s.Object({}); // Услуги на роуминговой сетке
const crossSale = new s.Object({}); // Матрица предложений CrossSale
const marketingCrossSale = new s.Object({}); // Матрица маркетинговых предложений CrossSale
const marketingDevice = new s.Object({}); // Маркетинговое оборудование
const device = new s.Object({}); // Оборудование
const marketingFixConnectAction = new s.Object({}); // Маркетинговые акции фиксированной связи
const fixConnectAction = new s.Object({}); // Акции фиксированной связи
const marketingTvPackage = new s.Object({}); // Маркетинговые ТВ-пакеты
const tvPackage = new s.Object({}); // ТВ-пакеты
const marketingFixConnectTariff = new s.Object({}); // Маркетинговые тарифы фиксированной связи
const fixConnectTariff = new s.Object({}); // Тарифы фиксированной связи
const marketingPhoneTariff = new s.Object({}); // Маркетинговые тарифы телефонии
const phoneTariff = new s.Object({}); // Тарифы телефонии
const marketingInternetTariff = new s.Object({}); // Маркетинговые тарифы интернет
const internetTariff = new s.Object({}); // Тарифы Интернет
const devicesForFixConnectAction = new s.Object({}); // Акционное оборудование

// Регионы
region.define({
  Parent: region
});

// Продукты
product.define({
  MarketingProduct: marketingProduct,
  Type_Contents: {
    tariff: tariff,
    service: service,
    action: action,
    roamingScale: roamingScale,
    device: device,
    fixConnectAction: fixConnectAction,
    tvPackage: tvPackage,
    fixConnectTariff: fixConnectTariff,
    phoneTariff: phoneTariff,
    internetTariff: internetTariff
  },
  Modifiers: [productModifer],
  Parameters: [productParameter],
  Regions: [region],
  FixConnectAction: [devicesForFixConnectAction],
  Advantages: [advantage]
});

// Параметры продуктов
productParameter.define({
  Group: productParameterGroup,
  Parent: productParameter,
  BaseParameter: baseParameter,
  Zone: tariffZone,
  Direction: direction,
  BaseParameterModifiers: [baseParameterModifier],
  Modifiers: [parameterModifier],
  Unit: unit,
  ProductGroup: group,
  Choice: parameterChoice
});

// Матрица связей
productRelation.define({
  Modifiers: [linkModifier],
  Parameters: [linkParameter],
  Type_Contents: {
    tariffTransfer: tariffTransfer,
    mutualGroup: mutualGroup,
    serviceOnTariff: serviceOnTariff,
    servicesUpsale: servicesUpsale,
    tariffOptionPackage: tariffOptionPackage,
    serviceRelation: serviceRelation,
    roamingScaleOnTariff: roamingScaleOnTariff,
    serviceOnRoamingScale: serviceOnRoamingScale,
    crossSale: crossSale,
    marketingCrossSale: marketingCrossSale,
    deviceOnTariffs: deviceOnTariffs,
    devicesForFixConnectAction: devicesForFixConnectAction
  }
});

// Параметры связей
linkParameter.define({
  Group: productParameterGroup,
  BaseParameter: baseParameter,
  Zone: tariffZone,
  Direction: direction,
  BaseParameterModifiers: [baseParameterModifier],
  Modifiers: [parameterModifier],
  Unit: unit,
  ProductGroup: group,
  Choice: parameterChoice
});

// Маркетинговые продукты
marketingProduct.define({
  Modifiers: [productModifer],
  Advantages: [advantage],
  Type_Contents: {
    marketingTariff: marketingTariff,
    marketingService: marketingService,
    marketingAction: marketingAction,
    marketingRoamingScale: marketingRoamingScale,
    marketingDevice: marketingDevice,
    marketingFixConnectAction: marketingFixConnectAction,
    marketingTvPackage: marketingTvPackage,
    marketingFixConnectTariff: marketingFixConnectTariff,
    marketingPhoneTariff: marketingPhoneTariff,
    marketingInternetTariff: marketingInternetTariff
  },
  Parameters: [marketingProductParameter],
  TariffsOnMarketingDevice: [deviceOnTariffs],
  DevicesOnMarketingTariff: [deviceOnTariffs],
  ActionsOnMarketingDevice: [devicesForFixConnectAction]
});

// Параметры маркетинговых продуктов
marketingProductParameter.define({
  Group: productParameterGroup,
  BaseParameter: baseParameter,
  Zone: tariffZone,
  Direction: direction,
  BaseParameterModifiers: [baseParameterModifier],
  Modifiers: [parameterModifier],
  Unit: unit,
  Choice: parameterChoice
});

// Категории тарифов
tariffCategory.define({
  ConnectionTypes: [fixedType]
});

// Города сети
networkCity.define({
  City: region
});

// ТВ-каналы
tvChannel.define({
  Category: channelCategory,
  ChannelType: channelType,
  Cities: [networkCity],
  Format: channelFormat,
  Parent: tvChannel,
  Children: [tvChannel],
  TimeZone: timeZone
});

// Маркетинговое оборудование
marketingDevice.define({
  DeviceType: equipmentType,
  Segments: [segment],
  CommunicationType: communicationType
});

// Оборудование
device.define({
  Downloads: [equipmentDownload],
  Inners: [product]
});

// Типы оборудования
equipmentType.define({
  ConnectionType: fixedType
});

// Маркетинговые акции фиксированной связи
marketingFixConnectAction.define({
  Segment: [segment],
  MarketingAction: marketingProduct
});

// Акции фиксированной связи
fixConnectAction.define({
  MarketingOffers: [marketingProduct]
});

// Маркетинговые ТВ-пакеты
marketingTvPackage.define({
  Channels: [tvChannel]
});

// Маркетинговые тарифы фиксированной связи
marketingFixConnectTariff.define({
  Segment: segment,
  Category: tariffCategory,
  MarketingDevices: [marketingProduct],
  BonusTVPackages: [marketingProduct],
  MarketingPhoneTariff: marketingProduct,
  MarketingInternetTariff: marketingProduct,
  MarketingTvPackage: marketingProduct
});

// Оборудование на тарифах
deviceOnTariffs.define({
  Parent: productRelation,
  MarketingDevice: marketingProduct,
  MarketingTariffs: [marketingProduct],
  Cities: [region]
});

// Акционное оборудование
devicesForFixConnectAction.define({
  FixConnectAction: product,
  Parent: productRelation,
  MarketingDevice: marketingProduct
});

export default {
  290: region,
  339: product,
  340: group,
  342: productModifer,
  346: tariffZone,
  347: direction,
  350: baseParameter,
  351: baseParameterModifier,
  352: parameterModifier,
  354: productParameter,
  355: unit,
  360: linkModifier,
  361: productRelation,
  362: linkParameter,
  378: productParameterGroup,
  383: marketingProduct,
  415: communicationType,
  416: segment,
  424: marketingProductParameter,
  441: tariffCategory,
  446: advantage,
  471: timeZone,
  472: networkCity,
  478: channelCategory,
  479: channelType,
  480: channelFormat,
  482: tvChannel,
  488: parameterChoice,
  491: fixedType,
  493: equipmentType,
  494: equipmentDownload,
  511: deviceOnTariffs
};
