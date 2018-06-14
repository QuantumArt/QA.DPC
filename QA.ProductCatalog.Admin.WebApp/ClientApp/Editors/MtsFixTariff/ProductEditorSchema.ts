import { ValidatableMixin } from "Models/ValidatableMixin";

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
}

export interface Region extends ValidatableMixin {
  Id: number;
  ContentName: "Region";
  Timestamp: Date;
  Title: string;
  Alias: string;
  Parent: Region;
  IsMainCity: boolean;
}

export interface Product extends ValidatableMixin {
  Id: number;
  ContentName: "Product";
  Timestamp: Date;
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

export interface Group extends ValidatableMixin {
  Id: number;
  ContentName: "Group";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface ProductModifer extends ValidatableMixin {
  Id: number;
  ContentName: "ProductModifer";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Tariff extends ValidatableMixin {
  ContentName: "Tariff";
}

export interface TariffZone extends ValidatableMixin {
  Id: number;
  ContentName: "TariffZone";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Direction extends ValidatableMixin {
  Id: number;
  ContentName: "Direction";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface BaseParameter extends ValidatableMixin {
  Id: number;
  ContentName: "BaseParameter";
  Timestamp: Date;
  Title: string;
  Alias: string;
  AllowZone: boolean;
  AllowDirection: boolean;
}

export interface BaseParameterModifier extends ValidatableMixin {
  Id: number;
  ContentName: "BaseParameterModifier";
  Timestamp: Date;
  Title: string;
  Alias: string;
  Type: 
    | "Step"
    | "Package"
    | "Zone"
    | "Direction"
    | "Refining";
}

export interface ParameterModifier extends ValidatableMixin {
  Id: number;
  ContentName: "ParameterModifier";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface ProductParameter extends ValidatableMixin {
  Id: number;
  ContentName: "ProductParameter";
  Timestamp: Date;
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

export interface Unit extends ValidatableMixin {
  Id: number;
  ContentName: "Unit";
  Timestamp: Date;
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

export interface LinkModifier extends ValidatableMixin {
  Id: number;
  ContentName: "LinkModifier";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface ProductRelation extends ValidatableMixin {
  Id: number;
  ContentName: "ProductRelation";
  Timestamp: Date;
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

export interface LinkParameter extends ValidatableMixin {
  Id: number;
  ContentName: "LinkParameter";
  Timestamp: Date;
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

export interface TariffTransfer extends ValidatableMixin {
  ContentName: "TariffTransfer";
}

export interface MutualGroup extends ValidatableMixin {
  ContentName: "MutualGroup";
}

export interface ProductParameterGroup extends ValidatableMixin {
  Id: number;
  ContentName: "ProductParameterGroup";
  Timestamp: Date;
  Title: string;
  Alias: string;
  SortOrder: number;
  OldSiteId: number;
  OldCorpSiteId: number;
  ImageSvg: string;
  Type: string;
  TitleForIcin: string;
}

export interface MarketingProduct extends ValidatableMixin {
  Id: number;
  ContentName: "MarketingProduct";
  Timestamp: Date;
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

export interface MarketingTariff extends ValidatableMixin {
  ContentName: "MarketingTariff";
}

export interface MarketingService extends ValidatableMixin {
  ContentName: "MarketingService";
}

export interface Service extends ValidatableMixin {
  ContentName: "Service";
}

export interface ServiceOnTariff extends ValidatableMixin {
  ContentName: "ServiceOnTariff";
  Description: string;
}

export interface ServicesUpsale extends ValidatableMixin {
  ContentName: "ServicesUpsale";
  Order: number;
}

export interface TariffOptionPackage extends ValidatableMixin {
  ContentName: "TariffOptionPackage";
  SubTitle: string;
  Description: string;
  Alias: string;
  Link: string;
}

export interface ServiceRelation extends ValidatableMixin {
  ContentName: "ServiceRelation";
}

export interface CommunicationType extends ValidatableMixin {
  Id: number;
  ContentName: "CommunicationType";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Segment extends ValidatableMixin {
  Id: number;
  ContentName: "Segment";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Action extends ValidatableMixin {
  ContentName: "Action";
}

export interface MarketingAction extends ValidatableMixin {
  ContentName: "MarketingAction";
}

export interface MarketingProductParameter extends ValidatableMixin {
  Id: number;
  ContentName: "MarketingProductParameter";
  Timestamp: Date;
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

export interface RoamingScale extends ValidatableMixin {
  ContentName: "RoamingScale";
}

export interface MarketingRoamingScale extends ValidatableMixin {
  ContentName: "MarketingRoamingScale";
}

export interface RoamingScaleOnTariff extends ValidatableMixin {
  ContentName: "RoamingScaleOnTariff";
}

export interface TariffCategory extends ValidatableMixin {
  Id: number;
  ContentName: "TariffCategory";
  Timestamp: Date;
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

export interface ServiceOnRoamingScale extends ValidatableMixin {
  ContentName: "ServiceOnRoamingScale";
}

export interface Advantage extends ValidatableMixin {
  Id: number;
  ContentName: "Advantage";
  Timestamp: Date;
  Title: string;
  Text: string;
  Description: string;
  ImageSvg: string;
  SortOrder: number;
  IsGift: boolean;
  OldSiteId: number;
}

export interface CrossSale extends ValidatableMixin {
  ContentName: "CrossSale";
  Order: number;
}

export interface MarketingCrossSale extends ValidatableMixin {
  ContentName: "MarketingCrossSale";
  Order: number;
}

export interface TimeZone extends ValidatableMixin {
  Id: number;
  ContentName: "TimeZone";
  Timestamp: Date;
  Name: string;
  Code: string;
  UTC: string;
  MSK: string;
  OldSiteId: number;
}

export interface NetworkCity extends ValidatableMixin {
  Id: number;
  ContentName: "NetworkCity";
  Timestamp: Date;
  City: Region;
  HasIpTv: boolean;
}

export interface ChannelCategory extends ValidatableMixin {
  Id: number;
  ContentName: "ChannelCategory";
  Timestamp: Date;
  Name: string;
  Alias: string;
  Segments: string;
  Icon: string;
  Order: number;
  OldSiteId: number;
}

export interface ChannelType extends ValidatableMixin {
  Id: number;
  ContentName: "ChannelType";
  Timestamp: Date;
  Title: string;
  OldSiteId: number;
}

export interface ChannelFormat extends ValidatableMixin {
  Id: number;
  ContentName: "ChannelFormat";
  Timestamp: Date;
  Title: string;
  Image: string;
  Message: string;
  OldSiteId: number;
}

export interface TvChannel extends ValidatableMixin {
  Id: number;
  ContentName: "TvChannel";
  Timestamp: Date;
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
}

export interface ParameterChoice extends ValidatableMixin {
  Id: number;
  ContentName: "ParameterChoice";
  Timestamp: Date;
  Title: string;
  Alias: string;
  OldSiteId: number;
}

export interface MarketingDevice extends ValidatableMixin {
  ContentName: "MarketingDevice";
  DeviceType: EquipmentType;
  Segments: Segment[];
  CommunicationType: CommunicationType;
}

export interface Device extends ValidatableMixin {
  ContentName: "Device";
  Downloads: EquipmentDownload[];
  Inners: Product[];
  FreezeDate: Date;
  FullUserGuide: string;
  QuickStartGuide: string;
}

export interface FixedType extends ValidatableMixin {
  Id: number;
  ContentName: "FixedType";
  Timestamp: Date;
  Title: string;
}

export interface EquipmentType extends ValidatableMixin {
  Id: number;
  ContentName: "EquipmentType";
  Timestamp: Date;
  ConnectionType: FixedType;
  Title: string;
  Alias: string;
  Order: number;
}

export interface EquipmentDownload extends ValidatableMixin {
  Id: number;
  ContentName: "EquipmentDownload";
  Timestamp: Date;
  Title: string;
  File: string;
}

export interface MarketingFixConnectAction extends ValidatableMixin {
  ContentName: "MarketingFixConnectAction";
  Segment: Segment[];
  MarketingAction: MarketingProduct;
  StartDate: Date;
  EndDate: Date;
  PromoPeriod: string;
  AfterPromo: string;
}

export interface FixConnectAction extends ValidatableMixin {
  ContentName: "FixConnectAction";
  MarketingOffers: MarketingProduct[];
  PromoPeriod: string;
  AfterPromo: string;
}

export interface MarketingTvPackage extends ValidatableMixin {
  ContentName: "MarketingTvPackage";
  Channels: TvChannel[];
  TitleForSite: string;
  PackageType: 
    | "Base"
    | "Additional";
}

export interface TvPackage extends ValidatableMixin {
  ContentName: "TvPackage";
}

export interface MarketingFixConnectTariff extends ValidatableMixin {
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

export interface FixConnectTariff extends ValidatableMixin {
  ContentName: "FixConnectTariff";
  TitleForSite: string;
}

export interface MarketingPhoneTariff extends ValidatableMixin {
  ContentName: "MarketingPhoneTariff";
}

export interface PhoneTariff extends ValidatableMixin {
  ContentName: "PhoneTariff";
  RostelecomLink: string;
}

export interface MarketingInternetTariff extends ValidatableMixin {
  ContentName: "MarketingInternetTariff";
}

export interface InternetTariff extends ValidatableMixin {
  ContentName: "InternetTariff";
}

export interface DeviceOnTariffs extends ValidatableMixin {
  Id: number;
  ContentName: "DeviceOnTariffs";
  Timestamp: Date;
  Parent: ProductRelation;
  Order: number;
  MarketingDevice: MarketingProduct;
  MarketingTariffs: MarketingProduct[];
  Cities: Region[];
}

export interface DevicesForFixConnectAction extends ValidatableMixin {
  ContentName: "DevicesForFixConnectAction";
  Order: number;
  FixConnectAction: Product;
  Parent: ProductRelation;
  MarketingDevice: MarketingProduct;
}
