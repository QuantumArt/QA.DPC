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
  Id: number;
  ContentName: "Region";
  Modified: Date;
  Title: string;
  Alias: string;
  Parent: Region;
  IsMainCity: boolean;
}

export interface Product extends ArticleObject {
  Id: number;
  ContentName: "Product";
  Modified: Date;
  MarketingProduct: MarketingProduct;
  GlobalCode: string;
  GlobalCode_Value: string;
  GlobalCode_Version: string;
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
  Description: string;
  FullDescription: string;
  Notes: string;
  Link: string;
  SortOrder: number;
  ForisID: string;
  Icon: string;
  PDF: string;
  PdfFixedAlias: string;
  PdfFixedLinks: string;
  StartDate: Date;
  EndDate: Date;
  OldSiteId: number;
  OldId: number;
  OldSiteInvId: string;
  OldCorpSiteId: number;
  OldAliasId: string;
  Priority: number;
  ListImage: string;
  ArchiveDate: Date;
  Modifiers: ProductModifer[];
  Parameters: ProductParameter[];
  Regions: Region[];
  FixConnectAction: DevicesForFixConnectAction[];
  Advantages: Advantage[];
}

export interface Group extends ArticleObject {
  Id: number;
  ContentName: "Group";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface ProductModifer extends ArticleObject {
  Id: number;
  ContentName: "ProductModifer";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface Tariff extends ExtensionObject {
  ContentName: "Tariff";
}

export interface TariffZone extends ArticleObject {
  Id: number;
  ContentName: "TariffZone";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface Direction extends ArticleObject {
  Id: number;
  ContentName: "Direction";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface BaseParameter extends ArticleObject {
  Id: number;
  ContentName: "BaseParameter";
  Modified: Date;
  Title: string;
  Alias: string;
  AllowZone: boolean;
  AllowDirection: boolean;
}

export interface BaseParameterModifier extends ArticleObject {
  Id: number;
  ContentName: "BaseParameterModifier";
  Modified: Date;
  Title: string;
  Alias: string;
  Type: 
    | "Step"
    | "Package"
    | "Zone"
    | "Direction"
    | "Refining";
}

export interface ParameterModifier extends ArticleObject {
  Id: number;
  ContentName: "ParameterModifier";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface ProductParameter extends ArticleObject {
  Id: number;
  ContentName: "ProductParameter";
  Modified: Date;
  Group: ProductParameterGroup;
  Title: string;
  Parent: ProductParameter;
  BaseParameter: BaseParameter;
  Zone: TariffZone;
  Direction: Direction;
  BaseParameterModifiers: BaseParameterModifier[];
  Modifiers: ParameterModifier[];
  Unit: Unit;
  SortOrder: number;
  NumValue: number;
  Value: string;
  Description: string;
  Image: string;
  ProductGroup: Group;
  Choice: ParameterChoice;
}

export interface Unit extends ArticleObject {
  Id: number;
  ContentName: "Unit";
  Modified: Date;
  Alias: string;
  Title: string;
  Display: string;
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
  QuotaPeriod: 
    | "daily"
    | "weekly"
    | "monthly"
    | "hourly"
    | "minutely"
    | "every_second"
    | "annually";
  QuotaPeriodicity: string;
  PeriodMultiplier: number;
  Type: string;
}

export interface LinkModifier extends ArticleObject {
  Id: number;
  ContentName: "LinkModifier";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface ProductRelation extends ArticleObject {
  Id: number;
  ContentName: "ProductRelation";
  Modified: Date;
  Title: string;
  Modifiers: LinkModifier[];
  Parameters: LinkParameter[];
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
  Id: number;
  ContentName: "LinkParameter";
  Modified: Date;
  Title: string;
  Group: ProductParameterGroup;
  BaseParameter: BaseParameter;
  Zone: TariffZone;
  Direction: Direction;
  BaseParameterModifiers: BaseParameterModifier[];
  Modifiers: ParameterModifier[];
  SortOrder: number;
  NumValue: number;
  Value: string;
  Description: string;
  Unit: Unit;
  ProductGroup: Group;
  Choice: ParameterChoice[];
  OldSiteId: number;
  OldCorpSiteId: number;
  OldPointId: number;
  OldCorpPointId: number;
}

export interface TariffTransfer extends ExtensionObject {
  ContentName: "TariffTransfer";
}

export interface MutualGroup extends ExtensionObject {
  ContentName: "MutualGroup";
}

export interface ProductParameterGroup extends ArticleObject {
  Id: number;
  ContentName: "ProductParameterGroup";
  Modified: Date;
  Title: string;
  Alias: string;
  SortOrder: number;
  OldSiteId: number;
  OldCorpSiteId: number;
  ImageSvg: string;
  Type: string;
  TitleForIcin: string;
}

export interface MarketingProduct extends ArticleObject {
  Id: number;
  ContentName: "MarketingProduct";
  Modified: Date;
  Title: string;
  Alias: string;
  Description: string;
  OldSiteId: number;
  OldCorpSiteId: number;
  ListImage: string;
  DetailsImage: string;
  ArchiveDate: Date;
  Modifiers: ProductModifer[];
  SortOrder: number;
  Priority: number;
  Advantages: Advantage[];
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
  Parameters: MarketingProductParameter[];
  TariffsOnMarketingDevice: DeviceOnTariffs[];
  DevicesOnMarketingTariff: DeviceOnTariffs[];
  ActionsOnMarketingDevice: DevicesForFixConnectAction[];
  Link: string;
  DetailedDescription: string;
}

export interface MarketingTariff extends ExtensionObject {
  ContentName: "MarketingTariff";
}

export interface MarketingService extends ExtensionObject {
  ContentName: "MarketingService";
}

export interface Service extends ExtensionObject {
  ContentName: "Service";
}

export interface ServiceOnTariff extends ExtensionObject {
  ContentName: "ServiceOnTariff";
  Description: string;
}

export interface ServicesUpsale extends ExtensionObject {
  ContentName: "ServicesUpsale";
  Order: number;
}

export interface TariffOptionPackage extends ExtensionObject {
  ContentName: "TariffOptionPackage";
  SubTitle: string;
  Description: string;
  Alias: string;
  Link: string;
}

export interface ServiceRelation extends ExtensionObject {
  ContentName: "ServiceRelation";
}

export interface CommunicationType extends ArticleObject {
  Id: number;
  ContentName: "CommunicationType";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface Segment extends ArticleObject {
  Id: number;
  ContentName: "Segment";
  Modified: Date;
  Title: string;
  Alias: string;
}

export interface Action extends ExtensionObject {
  ContentName: "Action";
}

export interface MarketingAction extends ExtensionObject {
  ContentName: "MarketingAction";
}

export interface MarketingProductParameter extends ArticleObject {
  Id: number;
  ContentName: "MarketingProductParameter";
  Modified: Date;
  Group: ProductParameterGroup;
  BaseParameter: BaseParameter;
  Zone: TariffZone;
  Direction: Direction;
  BaseParameterModifiers: BaseParameterModifier[];
  Modifiers: ParameterModifier[];
  Unit: Unit;
  Choice: ParameterChoice[];
  Title: string;
  SortOrder: number;
  NumValue: number;
  Value: string;
  Description: string;
  Image: string;
}

export interface RoamingScale extends ExtensionObject {
  ContentName: "RoamingScale";
}

export interface MarketingRoamingScale extends ExtensionObject {
  ContentName: "MarketingRoamingScale";
}

export interface RoamingScaleOnTariff extends ExtensionObject {
  ContentName: "RoamingScaleOnTariff";
}

export interface TariffCategory extends ArticleObject {
  Id: number;
  ContentName: "TariffCategory";
  Modified: Date;
  ConnectionTypes: FixedType[];
  Title: string;
  Alias: string;
  Image: string;
  Order: number;
  ImageSvg: string;
  TemplateType: 
    | "Tv"
    | "Phone";
}

export interface ServiceOnRoamingScale extends ExtensionObject {
  ContentName: "ServiceOnRoamingScale";
}

export interface Advantage extends ArticleObject {
  Id: number;
  ContentName: "Advantage";
  Modified: Date;
  Title: string;
  Text: string;
  Description: string;
  ImageSvg: string;
  SortOrder: number;
  IsGift: boolean;
  OldSiteId: number;
}

export interface CrossSale extends ExtensionObject {
  ContentName: "CrossSale";
  Order: number;
}

export interface MarketingCrossSale extends ExtensionObject {
  ContentName: "MarketingCrossSale";
  Order: number;
}

export interface TimeZone extends ArticleObject {
  Id: number;
  ContentName: "TimeZone";
  Modified: Date;
  Name: string;
  Code: string;
  UTC: string;
  MSK: string;
  OldSiteId: number;
}

export interface NetworkCity extends ArticleObject {
  Id: number;
  ContentName: "NetworkCity";
  Modified: Date;
  City: Region;
  HasIpTv: boolean;
}

export interface ChannelCategory extends ArticleObject {
  Id: number;
  ContentName: "ChannelCategory";
  Modified: Date;
  Name: string;
  Alias: string;
  Segments: string;
  Icon: string;
  Order: number;
  OldSiteId: number;
}

export interface ChannelType extends ArticleObject {
  Id: number;
  ContentName: "ChannelType";
  Modified: Date;
  Title: string;
  OldSiteId: number;
}

export interface ChannelFormat extends ArticleObject {
  Id: number;
  ContentName: "ChannelFormat";
  Modified: Date;
  Title: string;
  Image: string;
  Message: string;
  OldSiteId: number;
}

export interface TvChannel extends ArticleObject {
  Id: number;
  ContentName: "TvChannel";
  Modified: Date;
  Title: string;
  Logo150: string;
  Category: ChannelCategory;
  ChannelType: ChannelType;
  ShortDescription: string;
  Cities: NetworkCity[];
  Disabled: boolean;
  IsMtsMsk: boolean;
  IsRegional: boolean;
  LcnDvbC: number;
  LcnIpTv: number;
  LcnDvbS: number;
  Format: ChannelFormat;
  Parent: TvChannel;
  Children: TvChannel[];
  Logo40x30: string;
  TimeZone: TimeZone;
  LcnIpTvR: number;
}

export interface ParameterChoice extends ArticleObject {
  Id: number;
  ContentName: "ParameterChoice";
  Modified: Date;
  Title: string;
  Alias: string;
  OldSiteId: number;
}

export interface MarketingDevice extends ExtensionObject {
  ContentName: "MarketingDevice";
  DeviceType: EquipmentType;
  Segments: Segment[];
  CommunicationType: CommunicationType;
}

export interface Device extends ExtensionObject {
  ContentName: "Device";
  Downloads: EquipmentDownload[];
  Inners: Product[];
  FreezeDate: Date;
  FullUserGuide: string;
  QuickStartGuide: string;
}

export interface FixedType extends ArticleObject {
  Id: number;
  ContentName: "FixedType";
  Modified: Date;
  Title: string;
}

export interface EquipmentType extends ArticleObject {
  Id: number;
  ContentName: "EquipmentType";
  Modified: Date;
  ConnectionType: FixedType;
  Title: string;
  Alias: string;
  Order: number;
}

export interface EquipmentDownload extends ArticleObject {
  Id: number;
  ContentName: "EquipmentDownload";
  Modified: Date;
  Title: string;
  File: string;
}

export interface MarketingFixConnectAction extends ExtensionObject {
  ContentName: "MarketingFixConnectAction";
  Segment: Segment[];
  MarketingAction: MarketingProduct;
  StartDate: Date;
  EndDate: Date;
  PromoPeriod: string;
  AfterPromo: string;
}

export interface FixConnectAction extends ExtensionObject {
  ContentName: "FixConnectAction";
  MarketingOffers: MarketingProduct[];
  PromoPeriod: string;
  AfterPromo: string;
}

export interface MarketingTvPackage extends ExtensionObject {
  ContentName: "MarketingTvPackage";
  Channels: TvChannel[];
  TitleForSite: string;
  PackageType: 
    | "Base"
    | "Additional";
}

export interface TvPackage extends ExtensionObject {
  ContentName: "TvPackage";
}

export interface MarketingFixConnectTariff extends ExtensionObject {
  ContentName: "MarketingFixConnectTariff";
  Segment: Segment;
  Category: TariffCategory;
  MarketingDevices: MarketingProduct[];
  BonusTVPackages: MarketingProduct[];
  MarketingPhoneTariff: MarketingProduct;
  MarketingInternetTariff: MarketingProduct;
  MarketingTvPackage: MarketingProduct;
  TitleForSite: string;
}

export interface FixConnectTariff extends ExtensionObject {
  ContentName: "FixConnectTariff";
  TitleForSite: string;
}

export interface MarketingPhoneTariff extends ExtensionObject {
  ContentName: "MarketingPhoneTariff";
}

export interface PhoneTariff extends ExtensionObject {
  ContentName: "PhoneTariff";
  RostelecomLink: string;
}

export interface MarketingInternetTariff extends ExtensionObject {
  ContentName: "MarketingInternetTariff";
}

export interface InternetTariff extends ExtensionObject {
  ContentName: "InternetTariff";
}

export interface DeviceOnTariffs extends ArticleObject {
  Id: number;
  ContentName: "DeviceOnTariffs";
  Modified: Date;
  Parent: ProductRelation;
  Order: number;
  MarketingDevice: MarketingProduct;
  MarketingTariffs: MarketingProduct[];
  Cities: Region[];
}

export interface DevicesForFixConnectAction extends ArticleObject {
  Id: number;
  ContentName: "DevicesForFixConnectAction";
  Modified: Date;
  Order: number;
  FixConnectAction: Product;
  Parent: ProductRelation;
  MarketingDevice: MarketingProduct;
}
