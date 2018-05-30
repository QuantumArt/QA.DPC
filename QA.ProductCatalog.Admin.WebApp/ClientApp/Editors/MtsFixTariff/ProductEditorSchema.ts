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

export interface Region {
  Id: number;
  ContentName: "Region";
  Timestamp: Date;
  Title: string;
  Alias: string;
  Parent: Region;
  IsMainCity: boolean;
}

export interface Product {
  Id: number;
  ContentName: "Product";
  Timestamp: Date;
  MarketingProduct: MarketingProduct;
  GlobalCode: string;
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
  Icon: {
    Name: string;
    AbsoluteUrl: string;
  };
  PDF: {
    Name: string;
    AbsoluteUrl: string;
  };
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
  ListImage: {
    Name: string;
    AbsoluteUrl: string;
  };
  ArchiveDate: Date;
  Modifiers: ProductModifer[];
  Parameters: ProductParameter[];
  Regions: Region[];
  FixConnectAction: DevicesForFixConnectAction;
  Advantages: Advantage[];
}

export interface Group {
  Id: number;
  ContentName: "Group";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface ProductModifer {
  Id: number;
  ContentName: "ProductModifer";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Tariff {
  ContentName: "Tariff";
}

export interface TariffZone {
  Id: number;
  ContentName: "TariffZone";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Direction {
  Id: number;
  ContentName: "Direction";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface BaseParameter {
  Id: number;
  ContentName: "BaseParameter";
  Timestamp: Date;
  Title: string;
  Alias: string;
  AllowZone: boolean;
  AllowDirection: boolean;
}

export interface BaseParameterModifier {
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

export interface ParameterModifier {
  Id: number;
  ContentName: "ParameterModifier";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface ProductParameter {
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
  Image: {
    Name: string;
    AbsoluteUrl: string;
  };
  ProductGroup: Group;
  Choice: ParameterChoice;
}

export interface Unit {
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

export interface LinkModifier {
  Id: number;
  ContentName: "LinkModifier";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface ProductRelation {
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

export interface LinkParameter {
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
  Choice: ParameterChoice;
  OldSiteId: number;
  OldCorpSiteId: number;
  OldPointId: number;
  OldCorpPointId: number;
}

export interface TariffTransfer {
  ContentName: "TariffTransfer";
}

export interface MutualGroup {
  ContentName: "MutualGroup";
}

export interface ProductParameterGroup {
  Id: number;
  ContentName: "ProductParameterGroup";
  Timestamp: Date;
  Title: string;
  Alias: string;
  SortOrder: number;
  OldSiteId: number;
  OldCorpSiteId: number;
  ImageSvg: {
    Name: string;
    AbsoluteUrl: string;
  };
  Type: string;
  TitleForIcin: string;
}

export interface MarketingProduct {
  Id: number;
  ContentName: "MarketingProduct";
  Timestamp: Date;
  Title: string;
  Alias: string;
  Description: string;
  OldSiteId: number;
  OldCorpSiteId: number;
  ListImage: {
    Name: string;
    AbsoluteUrl: string;
  };
  DetailsImage: {
    Name: string;
    AbsoluteUrl: string;
  };
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
  TariffsOnMarketingDevice: DeviceOnTariffs;
  DevicesOnMarketingTariff: DeviceOnTariffs[];
  ActionsOnMarketingDevice: DevicesForFixConnectAction;
  Link: string;
  DetailedDescription: string;
}

export interface MarketingTariff {
  ContentName: "MarketingTariff";
}

export interface MarketingService {
  ContentName: "MarketingService";
}

export interface Service {
  ContentName: "Service";
}

export interface ServiceOnTariff {
  ContentName: "ServiceOnTariff";
  Description: string;
}

export interface ServicesUpsale {
  ContentName: "ServicesUpsale";
  Order: number;
}

export interface TariffOptionPackage {
  ContentName: "TariffOptionPackage";
  SubTitle: string;
  Description: string;
  Alias: string;
  Link: string;
}

export interface ServiceRelation {
  ContentName: "ServiceRelation";
}

export interface CommunicationType {
  Id: number;
  ContentName: "CommunicationType";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Segment {
  Id: number;
  ContentName: "Segment";
  Timestamp: Date;
  Title: string;
  Alias: string;
}

export interface Action {
  ContentName: "Action";
}

export interface MarketingAction {
  ContentName: "MarketingAction";
}

export interface MarketingProductParameter {
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
  Choice: ParameterChoice;
  Title: string;
  SortOrder: number;
  NumValue: number;
  Value: string;
  Description: string;
}

export interface RoamingScale {
  ContentName: "RoamingScale";
}

export interface MarketingRoamingScale {
  ContentName: "MarketingRoamingScale";
}

export interface RoamingScaleOnTariff {
  ContentName: "RoamingScaleOnTariff";
}

export interface TariffCategory {
  Id: number;
  ContentName: "TariffCategory";
  Timestamp: Date;
  ConnectionTypes: FixedType[];
  Title: string;
  Alias: string;
  Image: {
    Name: string;
    AbsoluteUrl: string;
  };
  Order: number;
  ImageSvg: {
    Name: string;
    AbsoluteUrl: string;
  };
  TemplateType: 
    | "Tv"
    | "Phone";
}

export interface ServiceOnRoamingScale {
  ContentName: "ServiceOnRoamingScale";
}

export interface Advantage {
  Id: number;
  ContentName: "Advantage";
  Timestamp: Date;
  Title: string;
  Text: string;
  Description: string;
  ImageSvg: {
    Name: string;
    AbsoluteUrl: string;
  };
  SortOrder: number;
  IsGift: boolean;
  OldSiteId: number;
}

export interface CrossSale {
  ContentName: "CrossSale";
  Order: number;
}

export interface MarketingCrossSale {
  ContentName: "MarketingCrossSale";
  Order: number;
}

export interface TimeZone {
  Id: number;
  ContentName: "TimeZone";
  Timestamp: Date;
  Name: string;
  Code: string;
  UTC: string;
  MSK: string;
  OldSiteId: number;
}

export interface NetworkCity {
  Id: number;
  ContentName: "NetworkCity";
  Timestamp: Date;
  City: Region;
  HasIpTv: boolean;
}

export interface ChannelCategory {
  Id: number;
  ContentName: "ChannelCategory";
  Timestamp: Date;
  Name: string;
  Alias: string;
  Segments: string;
  Icon: {
    Name: string;
    AbsoluteUrl: string;
  };
  Order: number;
  OldSiteId: number;
}

export interface ChannelType {
  Id: number;
  ContentName: "ChannelType";
  Timestamp: Date;
  Title: string;
  OldSiteId: number;
}

export interface ChannelFormat {
  Id: number;
  ContentName: "ChannelFormat";
  Timestamp: Date;
  Title: string;
  Image: {
    Name: string;
    AbsoluteUrl: string;
  };
  Message: string;
  OldSiteId: number;
}

export interface TvChannel {
  Id: number;
  ContentName: "TvChannel";
  Timestamp: Date;
  Title: string;
  Logo150: {
    Name: string;
    AbsoluteUrl: string;
  };
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
  Logo40x30: {
    Name: string;
    AbsoluteUrl: string;
  };
  TimeZone: TimeZone;
}

export interface ParameterChoice {
  Id: number;
  ContentName: "ParameterChoice";
  Timestamp: Date;
  Title: string;
  Alias: string;
  OldSiteId: number;
}

export interface MarketingDevice {
  ContentName: "MarketingDevice";
  DeviceType: EquipmentType;
  Segments: Segment[];
  CommunicationType: CommunicationType;
}

export interface Device {
  ContentName: "Device";
  Downloads: EquipmentDownload[];
  Inners: Product[];
  FreezeDate: Date;
  FullUserGuide: {
    Name: string;
    AbsoluteUrl: string;
  };
  QuickStartGuide: {
    Name: string;
    AbsoluteUrl: string;
  };
}

export interface FixedType {
  Id: number;
  ContentName: "FixedType";
  Timestamp: Date;
  Title: string;
}

export interface EquipmentType {
  Id: number;
  ContentName: "EquipmentType";
  Timestamp: Date;
  ConnectionType: FixedType;
  Title: string;
  Alias: string;
  Order: number;
}

export interface EquipmentDownload {
  Id: number;
  ContentName: "EquipmentDownload";
  Timestamp: Date;
  Title: string;
  File: {
    Name: string;
    AbsoluteUrl: string;
  };
}

export interface MarketingFixConnectAction {
  ContentName: "MarketingFixConnectAction";
  Segment: Segment[];
  MarketingAction: MarketingProduct;
  StartDate: Date;
  EndDate: Date;
  PromoPeriod: string;
  AfterPromo: string;
}

export interface FixConnectAction {
  ContentName: "FixConnectAction";
  MarketingOffers: MarketingProduct[];
  PromoPeriod: string;
  AfterPromo: string;
}

export interface MarketingTvPackage {
  ContentName: "MarketingTvPackage";
  Channels: TvChannel[];
  TitleForSite: string;
  PackageType: 
    | "Base"
    | "Additional";
}

export interface TvPackage {
  ContentName: "TvPackage";
}

export interface MarketingFixConnectTariff {
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

export interface FixConnectTariff {
  ContentName: "FixConnectTariff";
  TitleForSite: string;
}

export interface MarketingPhoneTariff {
  ContentName: "MarketingPhoneTariff";
}

export interface PhoneTariff {
  ContentName: "PhoneTariff";
  RostelecomLink: string;
}

export interface MarketingInternetTariff {
  ContentName: "MarketingInternetTariff";
}

export interface InternetTariff {
  ContentName: "InternetTariff";
}

export interface DeviceOnTariffs {
  Id: number;
  ContentName: "DeviceOnTariffs";
  Timestamp: Date;
  Parent: ProductRelation;
  Order: number;
  MarketingDevice: MarketingProduct;
  MarketingTariffs: MarketingProduct[];
  Cities: Region[];
}

export interface DevicesForFixConnectAction {
  ContentName: "DevicesForFixConnectAction";
  Order: number;
  FixConnectAction: Product;
  Parent: ProductRelation;
  MarketingDevice: MarketingProduct;
}
