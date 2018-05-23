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
  Icon: string;
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
  ListImage: string;
  ArchiveDate: Date;
  Modifiers: ProductModifer[];
  Parameters: ProductParameter[];
  Regions: Region[];
  FixConnectAction: DevicesForFixConnectAction[];
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
  Image: string;
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
  Image: string;
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
  Icon: string;
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
  Image: string;
  Message: string;
  OldSiteId: number;
}

export interface TvChannel {
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


/** Описание полей продукта */
export interface ProductSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Product",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    MarketingProduct: {
      IsBackward: false,
      Content: {
        ContentId: number,
        ContentPath: string,
        ContentName: "MarketingProduct",
        ContentTitle: string,
        ContentDescription: string,
        ForExtension: false,
        Fields: {
          Title: {
            FieldId: number,
            FieldName: "Title",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "String",
            IsRequired: false
          },
          Alias: {
            FieldId: number,
            FieldName: "Alias",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "String",
            IsRequired: false
          },
          Description: {
            FieldId: number,
            FieldName: "Description",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Textbox",
            IsRequired: false
          },
          OldSiteId: {
            FieldId: number,
            FieldName: "OldSiteId",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Numeric",
            IsRequired: false
          },
          OldCorpSiteId: {
            FieldId: number,
            FieldName: "OldCorpSiteId",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Numeric",
            IsRequired: false
          },
          ListImage: {
            FieldId: number,
            FieldName: "ListImage",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Image",
            IsRequired: false
          },
          DetailsImage: {
            FieldId: number,
            FieldName: "DetailsImage",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Image",
            IsRequired: false
          },
          ArchiveDate: {
            FieldId: number,
            FieldName: "ArchiveDate",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Date",
            IsRequired: false
          },
          Modifiers: {
            IsBackward: false,
            Content: ProductModiferSchema,
            FieldId: number,
            FieldName: "Modifiers",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "M2MRelation",
            IsRequired: false
          },
          SortOrder: {
            FieldId: number,
            FieldName: "SortOrder",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Numeric",
            IsRequired: false
          },
          Priority: {
            FieldId: number,
            FieldName: "Priority",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Numeric",
            IsRequired: false
          },
          Advantages: {
            IsBackward: false,
            Content: AdvantageSchema,
            FieldId: number,
            FieldName: "Advantages",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "M2MRelation",
            IsRequired: false
          },
          Type: {
            Contents: {
              MarketingTariff: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingTariff",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {}
              },
              MarketingService: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingService",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {}
              },
              MarketingAction: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingAction",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {}
              },
              MarketingRoamingScale: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingRoamingScale",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {}
              },
              MarketingDevice: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingDevice",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {
                  DeviceType: {
                    IsBackward: false,
                    Content: {
                      ContentId: number,
                      ContentPath: string,
                      ContentName: "EquipmentType",
                      ContentTitle: string,
                      ContentDescription: string,
                      ForExtension: false,
                      Fields: {
                        ConnectionType: {
                          IsBackward: false,
                          Content: FixedTypeSchema,
                          FieldId: number,
                          FieldName: "ConnectionType",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        Title: {
                          FieldId: number,
                          FieldName: "Title",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Alias: {
                          FieldId: number,
                          FieldName: "Alias",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Order: {
                          FieldId: number,
                          FieldName: "Order",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "Numeric",
                          IsRequired: false
                        }
                      },
                      include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingDevice']['Fields']['DeviceType']['Content']['Fields']) => Selection[]) => string[]
                    },
                    FieldId: number,
                    FieldName: "DeviceType",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false,
                    include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingDevice']['Fields']['DeviceType']['Content']['Fields']) => Selection[]) => string[]
                  },
                  Segments: {
                    IsBackward: false,
                    Content: SegmentSchema,
                    FieldId: number,
                    FieldName: "Segments",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  CommunicationType: {
                    IsBackward: false,
                    Content: {
                      ContentId: number,
                      ContentPath: string,
                      ContentName: "CommunicationType",
                      ContentTitle: string,
                      ContentDescription: string,
                      ForExtension: false,
                      Fields: {
                        Title: {
                          FieldId: number,
                          FieldName: "Title",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Alias: {
                          FieldId: number,
                          FieldName: "Alias",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "String",
                          IsRequired: false
                        }
                      }
                    },
                    FieldId: number,
                    FieldName: "CommunicationType",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  }
                },
                include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingDevice']['Fields']) => Selection[]) => string[]
              },
              MarketingFixConnectAction: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingFixConnectAction",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {
                  Segment: {
                    IsBackward: false,
                    Content: SegmentSchema,
                    FieldId: number,
                    FieldName: "Segment",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  MarketingAction: {
                    IsBackward: false,
                    Content: MarketingProduct3Schema,
                    FieldId: number,
                    FieldName: "MarketingAction",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  StartDate: {
                    FieldId: number,
                    FieldName: "StartDate",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "Date",
                    IsRequired: false
                  },
                  EndDate: {
                    FieldId: number,
                    FieldName: "EndDate",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "Date",
                    IsRequired: false
                  },
                  PromoPeriod: {
                    FieldId: number,
                    FieldName: "PromoPeriod",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "String",
                    IsRequired: false
                  },
                  AfterPromo: {
                    FieldId: number,
                    FieldName: "AfterPromo",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "String",
                    IsRequired: false
                  }
                },
                include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingFixConnectAction']['Fields']) => Selection[]) => string[]
              },
              MarketingTvPackage: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingTvPackage",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {
                  Channels: {
                    IsBackward: false,
                    Content: {
                      ContentId: number,
                      ContentPath: string,
                      ContentName: "TvChannel",
                      ContentTitle: string,
                      ContentDescription: string,
                      ForExtension: false,
                      Fields: {
                        Title: {
                          FieldId: number,
                          FieldName: "Title",
                          FieldTitle: string,
                          FieldDescription: "title",
                          FieldOrder: number,
                          FieldType: "String",
                          IsRequired: false
                        },
                        ShortDescription: {
                          FieldId: number,
                          FieldName: "ShortDescription",
                          FieldTitle: string,
                          FieldDescription: "short_descr",
                          FieldOrder: number,
                          FieldType: "Textbox",
                          IsRequired: false
                        },
                        Logo150: {
                          FieldId: number,
                          FieldName: "Logo150",
                          FieldTitle: string,
                          FieldDescription: "logo150",
                          FieldOrder: number,
                          FieldType: "Image",
                          IsRequired: false
                        },
                        IsRegional: {
                          FieldId: number,
                          FieldName: "IsRegional",
                          FieldTitle: string,
                          FieldDescription: "regional_tv",
                          FieldOrder: number,
                          FieldType: "Boolean",
                          IsRequired: false
                        },
                        Parent: {
                          IsBackward: false,
                          Content: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "TvChannel",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: false,
                            Fields: {
                              Logo150: {
                                FieldId: number,
                                FieldName: "Logo150",
                                FieldTitle: string,
                                FieldDescription: "logo150",
                                FieldOrder: number,
                                FieldType: "Image",
                                IsRequired: false
                              }
                            }
                          },
                          FieldId: number,
                          FieldName: "Parent",
                          FieldTitle: string,
                          FieldDescription: "ch_parent",
                          FieldOrder: number,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        Cities: {
                          IsBackward: false,
                          Content: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "NetworkCity",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: false,
                            Fields: {
                              City: {
                                IsBackward: false,
                                Content: RegionSchema,
                                FieldId: number,
                                FieldName: "City",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              HasIpTv: {
                                FieldId: number,
                                FieldName: "HasIpTv",
                                FieldTitle: "IPTV",
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Boolean",
                                IsRequired: false
                              }
                            },
                            include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']['Cities']['Content']['Fields']) => Selection[]) => string[]
                          },
                          FieldId: number,
                          FieldName: "Cities",
                          FieldTitle: string,
                          FieldDescription: "cities",
                          FieldOrder: number,
                          FieldType: "M2MRelation",
                          IsRequired: false,
                          include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']['Cities']['Content']['Fields']) => Selection[]) => string[]
                        },
                        ChannelType: {
                          IsBackward: false,
                          Content: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "ChannelType",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: false,
                            Fields: {
                              Title: {
                                FieldId: number,
                                FieldName: "Title",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              }
                            }
                          },
                          FieldId: number,
                          FieldName: "ChannelType",
                          FieldTitle: string,
                          FieldDescription: "ch_type",
                          FieldOrder: number,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        Category: {
                          IsBackward: false,
                          Content: ChannelCategorySchema,
                          FieldId: number,
                          FieldName: "Category",
                          FieldTitle: string,
                          FieldDescription: "ch_category",
                          FieldOrder: number,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        IsMtsMsk: {
                          FieldId: number,
                          FieldName: "IsMtsMsk",
                          FieldTitle: string,
                          FieldDescription: "test_inMSK_mgts_XML",
                          FieldOrder: number,
                          FieldType: "Boolean",
                          IsRequired: false
                        },
                        LcnDvbC: {
                          FieldId: number,
                          FieldName: "LcnDvbC",
                          FieldTitle: string,
                          FieldDescription: "lcn_dvbc",
                          FieldOrder: number,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        LcnIpTv: {
                          FieldId: number,
                          FieldName: "LcnIpTv",
                          FieldTitle: string,
                          FieldDescription: "lcn_iptv",
                          FieldOrder: number,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        LcnDvbS: {
                          FieldId: number,
                          FieldName: "LcnDvbS",
                          FieldTitle: string,
                          FieldDescription: "lcn_dvbs",
                          FieldOrder: number,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        Disabled: {
                          FieldId: number,
                          FieldName: "Disabled",
                          FieldTitle: string,
                          FieldDescription: "offair",
                          FieldOrder: number,
                          FieldType: "Boolean",
                          IsRequired: false
                        },
                        Children: {
                          IsBackward: false,
                          Content: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "TvChannel",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: false,
                            Fields: {
                              Title: {
                                FieldId: number,
                                FieldName: "Title",
                                FieldTitle: string,
                                FieldDescription: "title",
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              },
                              Category: {
                                IsBackward: false,
                                Content: ChannelCategorySchema,
                                FieldId: number,
                                FieldName: "Category",
                                FieldTitle: string,
                                FieldDescription: "ch_category",
                                FieldOrder: number,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              ChannelType: {
                                IsBackward: false,
                                Content: {
                                  ContentId: number,
                                  ContentPath: string,
                                  ContentName: "ChannelType",
                                  ContentTitle: string,
                                  ContentDescription: string,
                                  ForExtension: false,
                                  Fields: {
                                    Title: {
                                      FieldId: number,
                                      FieldName: "Title",
                                      FieldTitle: string,
                                      FieldDescription: string,
                                      FieldOrder: number,
                                      FieldType: "String",
                                      IsRequired: false
                                    },
                                    OldSiteId: {
                                      FieldId: number,
                                      FieldName: "OldSiteId",
                                      FieldTitle: string,
                                      FieldDescription: string,
                                      FieldOrder: number,
                                      FieldType: "Numeric",
                                      IsRequired: false
                                    }
                                  }
                                },
                                FieldId: number,
                                FieldName: "ChannelType",
                                FieldTitle: string,
                                FieldDescription: "ch_type",
                                FieldOrder: number,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              ShortDescription: {
                                FieldId: number,
                                FieldName: "ShortDescription",
                                FieldTitle: string,
                                FieldDescription: "short_descr",
                                FieldOrder: number,
                                FieldType: "Textbox",
                                IsRequired: false
                              },
                              Cities: {
                                IsBackward: false,
                                Content: {
                                  ContentId: number,
                                  ContentPath: string,
                                  ContentName: "NetworkCity",
                                  ContentTitle: string,
                                  ContentDescription: string,
                                  ForExtension: false,
                                  Fields: {
                                    City: {
                                      IsBackward: false,
                                      Content: RegionSchema,
                                      FieldId: number,
                                      FieldName: "City",
                                      FieldTitle: string,
                                      FieldDescription: string,
                                      FieldOrder: number,
                                      FieldType: "O2MRelation",
                                      IsRequired: false
                                    }
                                  },
                                  include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']['Children']['Content']['Fields']['Cities']['Content']['Fields']) => Selection[]) => string[]
                                },
                                FieldId: number,
                                FieldName: "Cities",
                                FieldTitle: string,
                                FieldDescription: "cities",
                                FieldOrder: number,
                                FieldType: "M2MRelation",
                                IsRequired: false,
                                include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']['Children']['Content']['Fields']['Cities']['Content']['Fields']) => Selection[]) => string[]
                              },
                              Disabled: {
                                FieldId: number,
                                FieldName: "Disabled",
                                FieldTitle: string,
                                FieldDescription: "offair",
                                FieldOrder: number,
                                FieldType: "Boolean",
                                IsRequired: false
                              },
                              IsMtsMsk: {
                                FieldId: number,
                                FieldName: "IsMtsMsk",
                                FieldTitle: string,
                                FieldDescription: "test_inMSK_mgts_XML",
                                FieldOrder: number,
                                FieldType: "Boolean",
                                IsRequired: false
                              },
                              IsRegional: {
                                FieldId: number,
                                FieldName: "IsRegional",
                                FieldTitle: string,
                                FieldDescription: "regional_tv",
                                FieldOrder: number,
                                FieldType: "Boolean",
                                IsRequired: false
                              },
                              Logo150: {
                                FieldId: number,
                                FieldName: "Logo150",
                                FieldTitle: string,
                                FieldDescription: "logo150",
                                FieldOrder: number,
                                FieldType: "Image",
                                IsRequired: false
                              },
                              LcnDvbC: {
                                FieldId: number,
                                FieldName: "LcnDvbC",
                                FieldTitle: string,
                                FieldDescription: "lcn_dvbc",
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              LcnIpTv: {
                                FieldId: number,
                                FieldName: "LcnIpTv",
                                FieldTitle: string,
                                FieldDescription: "lcn_iptv",
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              LcnDvbS: {
                                FieldId: number,
                                FieldName: "LcnDvbS",
                                FieldTitle: string,
                                FieldDescription: "lcn_dvbs",
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              Format: {
                                IsBackward: false,
                                Content: ChannelFormatSchema,
                                FieldId: number,
                                FieldName: "Format",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              }
                            },
                            include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']['Children']['Content']['Fields']) => Selection[]) => string[]
                          },
                          FieldId: number,
                          FieldName: "Children",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "M2ORelation",
                          IsRequired: false,
                          include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']['Children']['Content']['Fields']) => Selection[]) => string[]
                        },
                        Format: {
                          IsBackward: false,
                          Content: ChannelFormatSchema,
                          FieldId: number,
                          FieldName: "Format",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        Logo40x30: {
                          FieldId: number,
                          FieldName: "Logo40x30",
                          FieldTitle: string,
                          FieldDescription: "logo40x30",
                          FieldOrder: number,
                          FieldType: "Image",
                          IsRequired: false
                        },
                        TimeZone: {
                          IsBackward: false,
                          Content: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "TimeZone",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: false,
                            Fields: {
                              Name: {
                                FieldId: number,
                                FieldName: "Name",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              },
                              Code: {
                                FieldId: number,
                                FieldName: "Code",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              },
                              UTC: {
                                FieldId: number,
                                FieldName: "UTC",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              },
                              MSK: {
                                FieldId: number,
                                FieldName: "MSK",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              },
                              OldSiteId: {
                                FieldId: number,
                                FieldName: "OldSiteId",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              }
                            }
                          },
                          FieldId: number,
                          FieldName: "TimeZone",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        }
                      },
                      include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']) => Selection[]) => string[]
                    },
                    FieldId: number,
                    FieldName: "Channels",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "M2MRelation",
                    IsRequired: false,
                    include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']['Channels']['Content']['Fields']) => Selection[]) => string[]
                  },
                  TitleForSite: {
                    FieldId: number,
                    FieldName: "TitleForSite",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "String",
                    IsRequired: false
                  },
                  PackageType: {
                    Items: [
                      {
                        Value: "Base",
                        Alias: string,
                        IsDefault: false,
                        Invalid: false
                      },
                      {
                        Value: "Additional",
                        Alias: string,
                        IsDefault: false,
                        Invalid: false
                      }
                    ],
                    FieldId: number,
                    FieldName: "PackageType",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "StringEnum",
                    IsRequired: false
                  }
                },
                include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingTvPackage']['Fields']) => Selection[]) => string[]
              },
              MarketingFixConnectTariff: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingFixConnectTariff",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {
                  Segment: {
                    IsBackward: false,
                    Content: SegmentSchema,
                    FieldId: number,
                    FieldName: "Segment",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Category: {
                    IsBackward: false,
                    Content: {
                      ContentId: number,
                      ContentPath: string,
                      ContentName: "TariffCategory",
                      ContentTitle: string,
                      ContentDescription: string,
                      ForExtension: false,
                      Fields: {
                        ConnectionTypes: {
                          IsBackward: false,
                          Content: FixedTypeSchema,
                          FieldId: number,
                          FieldName: "ConnectionTypes",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "M2MRelation",
                          IsRequired: false
                        },
                        Title: {
                          FieldId: number,
                          FieldName: "Title",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Alias: {
                          FieldId: number,
                          FieldName: "Alias",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Image: {
                          FieldId: number,
                          FieldName: "Image",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "Image",
                          IsRequired: false
                        },
                        Order: {
                          FieldId: number,
                          FieldName: "Order",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        ImageSvg: {
                          FieldId: number,
                          FieldName: "ImageSvg",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "File",
                          IsRequired: false
                        },
                        TemplateType: {
                          Items: [
                            {
                              Value: "Tv",
                              Alias: string,
                              IsDefault: false,
                              Invalid: false
                            },
                            {
                              Value: "Phone",
                              Alias: string,
                              IsDefault: false,
                              Invalid: false
                            }
                          ],
                          FieldId: number,
                          FieldName: "TemplateType",
                          FieldTitle: string,
                          FieldDescription: string,
                          FieldOrder: number,
                          FieldType: "StringEnum",
                          IsRequired: false
                        }
                      },
                      include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingFixConnectTariff']['Fields']['Category']['Content']['Fields']) => Selection[]) => string[]
                    },
                    FieldId: number,
                    FieldName: "Category",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false,
                    include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingFixConnectTariff']['Fields']['Category']['Content']['Fields']) => Selection[]) => string[]
                  },
                  MarketingDevices: {
                    IsBackward: false,
                    Content: MarketingProductSchema,
                    FieldId: number,
                    FieldName: "MarketingDevices",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  BonusTVPackages: {
                    IsBackward: false,
                    Content: MarketingProductSchema,
                    FieldId: number,
                    FieldName: "BonusTVPackages",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  MarketingPhoneTariff: {
                    IsBackward: false,
                    Content: MarketingProduct1Schema,
                    FieldId: number,
                    FieldName: "MarketingPhoneTariff",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  MarketingInternetTariff: {
                    IsBackward: false,
                    Content: MarketingProduct1Schema,
                    FieldId: number,
                    FieldName: "MarketingInternetTariff",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  MarketingTvPackage: {
                    IsBackward: false,
                    Content: MarketingProduct2Schema,
                    FieldId: number,
                    FieldName: "MarketingTvPackage",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  TitleForSite: {
                    FieldId: number,
                    FieldName: "TitleForSite",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "String",
                    IsRequired: false
                  }
                },
                include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']['MarketingFixConnectTariff']['Fields']) => Selection[]) => string[]
              },
              MarketingPhoneTariff: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingPhoneTariff",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {}
              },
              MarketingInternetTariff: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingInternetTariff",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: true,
                Fields: {}
              }
            },
            FieldId: number,
            FieldName: "Type",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Classifier",
            IsRequired: false,
            include: (selector: (contents: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Type']['Contents']) => string[][]) => string[]
          },
          FullDescription: {
            FieldId: number,
            FieldName: "FullDescription",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "VisualEdit",
            IsRequired: false
          },
          Parameters: {
            IsBackward: false,
            Content: {
              ContentId: number,
              ContentPath: string,
              ContentName: "MarketingProductParameter",
              ContentTitle: string,
              ContentDescription: string,
              ForExtension: false,
              Fields: {
                Group: {
                  IsBackward: false,
                  Content: ProductParameterGroupSchema,
                  FieldId: number,
                  FieldName: "Group",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                BaseParameter: {
                  IsBackward: false,
                  Content: BaseParameterSchema,
                  FieldId: number,
                  FieldName: "BaseParameter",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                Zone: {
                  IsBackward: false,
                  Content: TariffZoneSchema,
                  FieldId: number,
                  FieldName: "Zone",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                Direction: {
                  IsBackward: false,
                  Content: DirectionSchema,
                  FieldId: number,
                  FieldName: "Direction",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                BaseParameterModifiers: {
                  IsBackward: false,
                  Content: BaseParameterModifierSchema,
                  FieldId: number,
                  FieldName: "BaseParameterModifiers",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "M2MRelation",
                  IsRequired: false
                },
                Modifiers: {
                  IsBackward: false,
                  Content: ParameterModifierSchema,
                  FieldId: number,
                  FieldName: "Modifiers",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "M2MRelation",
                  IsRequired: false
                },
                Unit: {
                  IsBackward: false,
                  Content: UnitSchema,
                  FieldId: number,
                  FieldName: "Unit",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                Choice: {
                  IsBackward: false,
                  Content: ParameterChoiceSchema,
                  FieldId: number,
                  FieldName: "Choice",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                Title: {
                  FieldId: number,
                  FieldName: "Title",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "String",
                  IsRequired: false
                },
                SortOrder: {
                  FieldId: number,
                  FieldName: "SortOrder",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "Numeric",
                  IsRequired: false
                },
                NumValue: {
                  FieldId: number,
                  FieldName: "NumValue",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "Numeric",
                  IsRequired: false
                },
                Value: {
                  FieldId: number,
                  FieldName: "Value",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "VisualEdit",
                  IsRequired: false
                },
                Description: {
                  FieldId: number,
                  FieldName: "Description",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "VisualEdit",
                  IsRequired: false
                }
              },
              include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
            },
            FieldId: number,
            FieldName: "Parameters",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "M2ORelation",
            IsRequired: false,
            include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
          },
          TariffsOnMarketingDevice: {
            IsBackward: true,
            Content: {
              ContentId: number,
              ContentPath: string,
              ContentName: "DeviceOnTariffs",
              ContentTitle: string,
              ContentDescription: string,
              ForExtension: false,
              Fields: {
                Parent: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "ProductRelation",
                    ContentTitle: string,
                    ContentDescription: string,
                    ForExtension: false,
                    Fields: {
                      Title: {
                        FieldId: number,
                        FieldName: "Title",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      Modifiers: {
                        IsBackward: false,
                        Content: LinkModifierSchema,
                        FieldId: number,
                        FieldName: "Modifiers",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2MRelation",
                        IsRequired: false
                      },
                      Parameters: {
                        IsBackward: false,
                        Content: {
                          ContentId: number,
                          ContentPath: string,
                          ContentName: "LinkParameter",
                          ContentTitle: string,
                          ContentDescription: string,
                          ForExtension: false,
                          Fields: {
                            Title: {
                              FieldId: number,
                              FieldName: "Title",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "String",
                              IsRequired: false
                            },
                            Group: {
                              IsBackward: false,
                              Content: ProductParameterGroup1Schema,
                              FieldId: number,
                              FieldName: "Group",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            BaseParameter: {
                              IsBackward: false,
                              Content: BaseParameterSchema,
                              FieldId: number,
                              FieldName: "BaseParameter",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Zone: {
                              IsBackward: false,
                              Content: TariffZoneSchema,
                              FieldId: number,
                              FieldName: "Zone",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Direction: {
                              IsBackward: false,
                              Content: DirectionSchema,
                              FieldId: number,
                              FieldName: "Direction",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            BaseParameterModifiers: {
                              IsBackward: false,
                              Content: BaseParameterModifierSchema,
                              FieldId: number,
                              FieldName: "BaseParameterModifiers",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "M2MRelation",
                              IsRequired: false
                            },
                            Modifiers: {
                              IsBackward: false,
                              Content: ParameterModifierSchema,
                              FieldId: number,
                              FieldName: "Modifiers",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "M2MRelation",
                              IsRequired: false
                            },
                            SortOrder: {
                              FieldId: number,
                              FieldName: "SortOrder",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            NumValue: {
                              FieldId: number,
                              FieldName: "NumValue",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            Value: {
                              FieldId: number,
                              FieldName: "Value",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "VisualEdit",
                              IsRequired: false
                            },
                            Description: {
                              FieldId: number,
                              FieldName: "Description",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "VisualEdit",
                              IsRequired: false
                            },
                            Unit: {
                              IsBackward: false,
                              Content: UnitSchema,
                              FieldId: number,
                              FieldName: "Unit",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            ProductGroup: {
                              IsBackward: false,
                              Content: {
                                ContentId: number,
                                ContentPath: string,
                                ContentName: "Group",
                                ContentTitle: string,
                                ContentDescription: string,
                                ForExtension: false,
                                Fields: {}
                              },
                              FieldId: number,
                              FieldName: "ProductGroup",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Choice: {
                              IsBackward: false,
                              Content: ParameterChoiceSchema,
                              FieldId: number,
                              FieldName: "Choice",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            }
                          },
                          include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['TariffsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                        },
                        FieldId: number,
                        FieldName: "Parameters",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2ORelation",
                        IsRequired: false,
                        include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['TariffsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                      },
                      Type: {
                        Contents: {
                          TariffTransfer: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "TariffTransfer",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {}
                          },
                          MutualGroup: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "MutualGroup",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {}
                          },
                          ServiceOnTariff: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "ServiceOnTariff",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {
                              Description: {
                                FieldId: number,
                                FieldName: "Description",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Textbox",
                                IsRequired: false
                              }
                            }
                          },
                          ServicesUpsale: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "ServicesUpsale",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {
                              Order: {
                                FieldId: number,
                                FieldName: "Order",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              }
                            }
                          },
                          TariffOptionPackage: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "TariffOptionPackage",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {
                              SubTitle: {
                                FieldId: number,
                                FieldName: "SubTitle",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Textbox",
                                IsRequired: false
                              },
                              Description: {
                                FieldId: number,
                                FieldName: "Description",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "VisualEdit",
                                IsRequired: false
                              },
                              Alias: {
                                FieldId: number,
                                FieldName: "Alias",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              },
                              Link: {
                                FieldId: number,
                                FieldName: "Link",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "String",
                                IsRequired: false
                              }
                            }
                          },
                          ServiceRelation: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "ServiceRelation",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {}
                          },
                          RoamingScaleOnTariff: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "RoamingScaleOnTariff",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {}
                          },
                          ServiceOnRoamingScale: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "ServiceOnRoamingScale",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {}
                          },
                          CrossSale: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "CrossSale",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {
                              Order: {
                                FieldId: number,
                                FieldName: "Order",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              }
                            }
                          },
                          MarketingCrossSale: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "MarketingCrossSale",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {
                              Order: {
                                FieldId: number,
                                FieldName: "Order",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              }
                            }
                          },
                          DeviceOnTariffs: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "DeviceOnTariffs",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {
                              Order: {
                                FieldId: number,
                                FieldName: "Order",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              }
                            }
                          },
                          DevicesForFixConnectAction: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "DevicesForFixConnectAction",
                            ContentTitle: string,
                            ContentDescription: string,
                            ForExtension: true,
                            Fields: {
                              Order: {
                                FieldId: number,
                                FieldName: "Order",
                                FieldTitle: string,
                                FieldDescription: string,
                                FieldOrder: number,
                                FieldType: "Numeric",
                                IsRequired: false
                              }
                            }
                          }
                        },
                        FieldId: number,
                        FieldName: "Type",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Classifier",
                        IsRequired: false,
                        include: (selector: (contents: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['TariffsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']['Type']['Contents']) => string[][]) => string[]
                      }
                    },
                    include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['TariffsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
                  },
                  FieldId: number,
                  FieldName: "Parent",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false,
                  include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['TariffsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
                },
                MarketingDevice: {
                  IsBackward: false,
                  Content: MarketingProduct2Schema,
                  FieldId: number,
                  FieldName: "MarketingDevice",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                MarketingTariffs: {
                  IsBackward: false,
                  Content: MarketingProduct3Schema,
                  FieldId: number,
                  FieldName: "MarketingTariffs",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "M2MRelation",
                  IsRequired: false
                },
                Cities: {
                  IsBackward: false,
                  Content: Region1Schema,
                  FieldId: number,
                  FieldName: "Cities",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "M2MRelation",
                  IsRequired: false
                },
                Order: {
                  FieldId: number,
                  FieldName: "Order",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "Numeric",
                  IsRequired: false
                }
              },
              include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['TariffsOnMarketingDevice']['Content']['Fields']) => Selection[]) => string[]
            },
            FieldId: number,
            FieldName: "TariffsOnMarketingDevice",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false,
            include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['TariffsOnMarketingDevice']['Content']['Fields']) => Selection[]) => string[]
          },
          DevicesOnMarketingTariff: {
            IsBackward: true,
            Content: {
              ContentId: number,
              ContentPath: string,
              ContentName: "DeviceOnTariffs",
              ContentTitle: string,
              ContentDescription: string,
              ForExtension: false,
              Fields: {
                Parent: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "ProductRelation",
                    ContentTitle: string,
                    ContentDescription: string,
                    ForExtension: false,
                    Fields: {
                      Title: {
                        FieldId: number,
                        FieldName: "Title",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      Parameters: {
                        IsBackward: false,
                        Content: {
                          ContentId: number,
                          ContentPath: string,
                          ContentName: "LinkParameter",
                          ContentTitle: string,
                          ContentDescription: string,
                          ForExtension: false,
                          Fields: {
                            Title: {
                              FieldId: number,
                              FieldName: "Title",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "String",
                              IsRequired: false
                            },
                            SortOrder: {
                              FieldId: number,
                              FieldName: "SortOrder",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            NumValue: {
                              FieldId: number,
                              FieldName: "NumValue",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            Value: {
                              FieldId: number,
                              FieldName: "Value",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "VisualEdit",
                              IsRequired: false
                            },
                            Description: {
                              FieldId: number,
                              FieldName: "Description",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "VisualEdit",
                              IsRequired: false
                            },
                            Unit: {
                              IsBackward: false,
                              Content: UnitSchema,
                              FieldId: number,
                              FieldName: "Unit",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Modifiers: {
                              IsBackward: false,
                              Content: ParameterModifierSchema,
                              FieldId: number,
                              FieldName: "Modifiers",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "M2MRelation",
                              IsRequired: false
                            },
                            BaseParameterModifiers: {
                              IsBackward: false,
                              Content: BaseParameterModifierSchema,
                              FieldId: number,
                              FieldName: "BaseParameterModifiers",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "M2MRelation",
                              IsRequired: false
                            },
                            Direction: {
                              IsBackward: false,
                              Content: DirectionSchema,
                              FieldId: number,
                              FieldName: "Direction",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Zone: {
                              IsBackward: false,
                              Content: TariffZoneSchema,
                              FieldId: number,
                              FieldName: "Zone",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            BaseParameter: {
                              IsBackward: false,
                              Content: BaseParameterSchema,
                              FieldId: number,
                              FieldName: "BaseParameter",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Group: {
                              IsBackward: false,
                              Content: ProductParameterGroupSchema,
                              FieldId: number,
                              FieldName: "Group",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Choice: {
                              IsBackward: false,
                              Content: ParameterChoiceSchema,
                              FieldId: number,
                              FieldName: "Choice",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            }
                          },
                          include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['DevicesOnMarketingTariff']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                        },
                        FieldId: number,
                        FieldName: "Parameters",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2ORelation",
                        IsRequired: false,
                        include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['DevicesOnMarketingTariff']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                      },
                      Modifiers: {
                        IsBackward: false,
                        Content: LinkModifierSchema,
                        FieldId: number,
                        FieldName: "Modifiers",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2MRelation",
                        IsRequired: false
                      }
                    },
                    include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['DevicesOnMarketingTariff']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
                  },
                  FieldId: number,
                  FieldName: "Parent",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false,
                  include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['DevicesOnMarketingTariff']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
                },
                MarketingDevice: {
                  IsBackward: false,
                  Content: MarketingProduct3Schema,
                  FieldId: number,
                  FieldName: "MarketingDevice",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false
                },
                Cities: {
                  IsBackward: false,
                  Content: Region1Schema,
                  FieldId: number,
                  FieldName: "Cities",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "M2MRelation",
                  IsRequired: false
                },
                Order: {
                  FieldId: number,
                  FieldName: "Order",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "Numeric",
                  IsRequired: false
                }
              },
              include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['DevicesOnMarketingTariff']['Content']['Fields']) => Selection[]) => string[]
            },
            FieldId: number,
            FieldName: "DevicesOnMarketingTariff",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "M2MRelation",
            IsRequired: false,
            include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['DevicesOnMarketingTariff']['Content']['Fields']) => Selection[]) => string[]
          },
          ActionsOnMarketingDevice: {
            IsBackward: true,
            Content: {
              ContentId: number,
              ContentPath: string,
              ContentName: "DevicesForFixConnectAction",
              ContentTitle: string,
              ContentDescription: string,
              ForExtension: false,
              Fields: {
                FixConnectAction: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "Product",
                    ContentTitle: string,
                    ContentDescription: string,
                    ForExtension: false,
                    Fields: {
                      MarketingProduct: {
                        IsBackward: false,
                        Content: {
                          ContentId: number,
                          ContentPath: string,
                          ContentName: "MarketingProduct",
                          ContentTitle: string,
                          ContentDescription: string,
                          ForExtension: false,
                          Fields: {
                            Title: {
                              FieldId: number,
                              FieldName: "Title",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "String",
                              IsRequired: false
                            }
                          }
                        },
                        FieldId: number,
                        FieldName: "MarketingProduct",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "O2MRelation",
                        IsRequired: false
                      },
                      GlobalCode: {
                        FieldId: number,
                        FieldName: "GlobalCode",
                        FieldTitle: "GlobalCode",
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      Type: {
                        FieldId: number,
                        FieldName: "Type",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Classifier",
                        IsRequired: false
                      },
                      Description: {
                        FieldId: number,
                        FieldName: "Description",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Textbox",
                        IsRequired: false
                      },
                      FullDescription: {
                        FieldId: number,
                        FieldName: "FullDescription",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "VisualEdit",
                        IsRequired: false
                      },
                      Notes: {
                        FieldId: number,
                        FieldName: "Notes",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Textbox",
                        IsRequired: false
                      },
                      Link: {
                        FieldId: number,
                        FieldName: "Link",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      SortOrder: {
                        FieldId: number,
                        FieldName: "SortOrder",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Numeric",
                        IsRequired: false
                      },
                      ForisID: {
                        FieldId: number,
                        FieldName: "ForisID",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      Icon: {
                        FieldId: number,
                        FieldName: "Icon",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Image",
                        IsRequired: false
                      },
                      PDF: {
                        FieldId: number,
                        FieldName: "PDF",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "File",
                        IsRequired: false
                      },
                      PdfFixedAlias: {
                        FieldId: number,
                        FieldName: "PdfFixedAlias",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      PdfFixedLinks: {
                        FieldId: number,
                        FieldName: "PdfFixedLinks",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Textbox",
                        IsRequired: false
                      },
                      StartDate: {
                        FieldId: number,
                        FieldName: "StartDate",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Date",
                        IsRequired: false
                      },
                      EndDate: {
                        FieldId: number,
                        FieldName: "EndDate",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Date",
                        IsRequired: false
                      },
                      OldSiteId: {
                        FieldId: number,
                        FieldName: "OldSiteId",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Numeric",
                        IsRequired: false
                      },
                      OldId: {
                        FieldId: number,
                        FieldName: "OldId",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Numeric",
                        IsRequired: false
                      },
                      OldSiteInvId: {
                        FieldId: number,
                        FieldName: "OldSiteInvId",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      OldCorpSiteId: {
                        FieldId: number,
                        FieldName: "OldCorpSiteId",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Numeric",
                        IsRequired: false
                      },
                      OldAliasId: {
                        FieldId: number,
                        FieldName: "OldAliasId",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      Priority: {
                        FieldId: number,
                        FieldName: "Priority",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Numeric",
                        IsRequired: false
                      },
                      ListImage: {
                        FieldId: number,
                        FieldName: "ListImage",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Image",
                        IsRequired: false
                      },
                      ArchiveDate: {
                        FieldId: number,
                        FieldName: "ArchiveDate",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Date",
                        IsRequired: false
                      }
                    },
                    include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']['FixConnectAction']['Content']['Fields']) => Selection[]) => string[]
                  },
                  FieldId: number,
                  FieldName: "FixConnectAction",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false,
                  include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']['FixConnectAction']['Content']['Fields']) => Selection[]) => string[]
                },
                Parent: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "ProductRelation",
                    ContentTitle: string,
                    ContentDescription: string,
                    ForExtension: false,
                    Fields: {
                      Title: {
                        FieldId: number,
                        FieldName: "Title",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      Type: {
                        FieldId: number,
                        FieldName: "Type",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Classifier",
                        IsRequired: false
                      },
                      Parameters: {
                        IsBackward: false,
                        Content: {
                          ContentId: number,
                          ContentPath: string,
                          ContentName: "LinkParameter",
                          ContentTitle: string,
                          ContentDescription: string,
                          ForExtension: false,
                          Fields: {
                            Unit: {
                              IsBackward: false,
                              Content: {
                                ContentId: number,
                                ContentPath: string,
                                ContentName: "Unit",
                                ContentTitle: string,
                                ContentDescription: string,
                                ForExtension: false,
                                Fields: {
                                  Alias: {
                                    FieldId: number,
                                    FieldName: "Alias",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  },
                                  Title: {
                                    FieldId: number,
                                    FieldName: "Title",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  },
                                  Display: {
                                    FieldId: number,
                                    FieldName: "Display",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  },
                                  QuotaUnit: {
                                    Items: [
                                      {
                                        Value: "mb",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "gb",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "kb",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "tb",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "min",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "message",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "rub",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "sms",
                                        Alias: "SMS",
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "mms",
                                        Alias: "MMS",
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "mbit",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "step",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      }
                                    ],
                                    FieldId: number,
                                    FieldName: "QuotaUnit",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "StringEnum",
                                    IsRequired: false
                                  },
                                  QuotaPeriod: {
                                    Items: [
                                      {
                                        Value: "daily",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "weekly",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "monthly",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "hourly",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "minutely",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "every_second",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      },
                                      {
                                        Value: "annually",
                                        Alias: string,
                                        IsDefault: false,
                                        Invalid: false
                                      }
                                    ],
                                    FieldId: number,
                                    FieldName: "QuotaPeriod",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "StringEnum",
                                    IsRequired: false
                                  },
                                  QuotaPeriodicity: {
                                    FieldId: number,
                                    FieldName: "QuotaPeriodicity",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  },
                                  PeriodMultiplier: {
                                    FieldId: number,
                                    FieldName: "PeriodMultiplier",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "Numeric",
                                    IsRequired: false
                                  },
                                  Type: {
                                    FieldId: number,
                                    FieldName: "Type",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  }
                                }
                              },
                              FieldId: number,
                              FieldName: "Unit",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            BaseParameterModifiers: {
                              IsBackward: false,
                              Content: BaseParameterModifierSchema,
                              FieldId: number,
                              FieldName: "BaseParameterModifiers",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "M2MRelation",
                              IsRequired: false
                            },
                            Modifiers: {
                              IsBackward: false,
                              Content: ParameterModifierSchema,
                              FieldId: number,
                              FieldName: "Modifiers",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "M2MRelation",
                              IsRequired: false
                            },
                            Direction: {
                              IsBackward: false,
                              Content: DirectionSchema,
                              FieldId: number,
                              FieldName: "Direction",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Zone: {
                              IsBackward: false,
                              Content: TariffZoneSchema,
                              FieldId: number,
                              FieldName: "Zone",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            BaseParameter: {
                              IsBackward: false,
                              Content: BaseParameterSchema,
                              FieldId: number,
                              FieldName: "BaseParameter",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Group: {
                              IsBackward: false,
                              Content: {
                                ContentId: number,
                                ContentPath: string,
                                ContentName: "ProductParameterGroup",
                                ContentTitle: string,
                                ContentDescription: string,
                                ForExtension: false,
                                Fields: {
                                  Title: {
                                    FieldId: number,
                                    FieldName: "Title",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  },
                                  Alias: {
                                    FieldId: number,
                                    FieldName: "Alias",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  },
                                  SortOrder: {
                                    FieldId: number,
                                    FieldName: "SortOrder",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "Numeric",
                                    IsRequired: false
                                  },
                                  OldSiteId: {
                                    FieldId: number,
                                    FieldName: "OldSiteId",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "Numeric",
                                    IsRequired: false
                                  },
                                  OldCorpSiteId: {
                                    FieldId: number,
                                    FieldName: "OldCorpSiteId",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "Numeric",
                                    IsRequired: false
                                  },
                                  ImageSvg: {
                                    FieldId: number,
                                    FieldName: "ImageSvg",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "File",
                                    IsRequired: false
                                  },
                                  Type: {
                                    FieldId: number,
                                    FieldName: "Type",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  },
                                  TitleForIcin: {
                                    FieldId: number,
                                    FieldName: "TitleForIcin",
                                    FieldTitle: string,
                                    FieldDescription: string,
                                    FieldOrder: number,
                                    FieldType: "String",
                                    IsRequired: false
                                  }
                                }
                              },
                              FieldId: number,
                              FieldName: "Group",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Choice: {
                              IsBackward: false,
                              Content: ParameterChoiceSchema,
                              FieldId: number,
                              FieldName: "Choice",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "O2MRelation",
                              IsRequired: false
                            },
                            Title: {
                              FieldId: number,
                              FieldName: "Title",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "String",
                              IsRequired: false
                            },
                            SortOrder: {
                              FieldId: number,
                              FieldName: "SortOrder",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            NumValue: {
                              FieldId: number,
                              FieldName: "NumValue",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            Value: {
                              FieldId: number,
                              FieldName: "Value",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "VisualEdit",
                              IsRequired: false
                            },
                            Description: {
                              FieldId: number,
                              FieldName: "Description",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "VisualEdit",
                              IsRequired: false
                            },
                            OldSiteId: {
                              FieldId: number,
                              FieldName: "OldSiteId",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            OldCorpSiteId: {
                              FieldId: number,
                              FieldName: "OldCorpSiteId",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            OldPointId: {
                              FieldId: number,
                              FieldName: "OldPointId",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            },
                            OldCorpPointId: {
                              FieldId: number,
                              FieldName: "OldCorpPointId",
                              FieldTitle: string,
                              FieldDescription: string,
                              FieldOrder: number,
                              FieldType: "Numeric",
                              IsRequired: false
                            }
                          },
                          include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                        },
                        FieldId: number,
                        FieldName: "Parameters",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2ORelation",
                        IsRequired: false,
                        include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                      },
                      Modifiers: {
                        IsBackward: false,
                        Content: LinkModifierSchema,
                        FieldId: number,
                        FieldName: "Modifiers",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2MRelation",
                        IsRequired: false
                      }
                    },
                    include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
                  },
                  FieldId: number,
                  FieldName: "Parent",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "O2MRelation",
                  IsRequired: false,
                  include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
                },
                Order: {
                  FieldId: number,
                  FieldName: "Order",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "Numeric",
                  IsRequired: false
                }
              },
              include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']) => Selection[]) => string[]
            },
            FieldId: number,
            FieldName: "ActionsOnMarketingDevice",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false,
            include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']['ActionsOnMarketingDevice']['Content']['Fields']) => Selection[]) => string[]
          }
        },
        include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']) => Selection[]) => string[]
      },
      FieldId: number,
      FieldName: "MarketingProduct",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "O2MRelation",
      IsRequired: false,
      include: (selector: (fields: ProductSchema['Fields']['MarketingProduct']['Content']['Fields']) => Selection[]) => string[]
    },
    GlobalCode: {
      FieldId: number,
      FieldName: "GlobalCode",
      FieldTitle: "GlobalCode",
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Description: {
      FieldId: number,
      FieldName: "Description",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Textbox",
      IsRequired: false
    },
    FullDescription: {
      FieldId: number,
      FieldName: "FullDescription",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "VisualEdit",
      IsRequired: false
    },
    Notes: {
      FieldId: number,
      FieldName: "Notes",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Textbox",
      IsRequired: false
    },
    Link: {
      FieldId: number,
      FieldName: "Link",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    SortOrder: {
      FieldId: number,
      FieldName: "SortOrder",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    Icon: {
      FieldId: number,
      FieldName: "Icon",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Image",
      IsRequired: false
    },
    PDF: {
      FieldId: number,
      FieldName: "PDF",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "File",
      IsRequired: false
    },
    StartDate: {
      FieldId: number,
      FieldName: "StartDate",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Date",
      IsRequired: false
    },
    EndDate: {
      FieldId: number,
      FieldName: "EndDate",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Date",
      IsRequired: false
    },
    Priority: {
      FieldId: number,
      FieldName: "Priority",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    ListImage: {
      FieldId: number,
      FieldName: "ListImage",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Image",
      IsRequired: false
    },
    ArchiveDate: {
      FieldId: number,
      FieldName: "ArchiveDate",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Date",
      IsRequired: false
    },
    Modifiers: {
      IsBackward: false,
      Content: ProductModiferSchema,
      FieldId: number,
      FieldName: "Modifiers",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "M2MRelation",
      IsRequired: false
    },
    Parameters: {
      IsBackward: false,
      Content: {
        ContentId: number,
        ContentPath: string,
        ContentName: "ProductParameter",
        ContentTitle: string,
        ContentDescription: string,
        ForExtension: false,
        Fields: {
          Group: {
            IsBackward: false,
            Content: ProductParameterGroup1Schema,
            FieldId: number,
            FieldName: "Group",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          Parent: {
            IsBackward: false,
            Content: {
              ContentId: number,
              ContentPath: string,
              ContentName: "ProductParameter",
              ContentTitle: string,
              ContentDescription: string,
              ForExtension: false,
              Fields: {
                Title: {
                  FieldId: number,
                  FieldName: "Title",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "String",
                  IsRequired: false
                }
              }
            },
            FieldId: number,
            FieldName: "Parent",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          BaseParameter: {
            IsBackward: false,
            Content: BaseParameterSchema,
            FieldId: number,
            FieldName: "BaseParameter",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          Zone: {
            IsBackward: false,
            Content: TariffZoneSchema,
            FieldId: number,
            FieldName: "Zone",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          Direction: {
            IsBackward: false,
            Content: DirectionSchema,
            FieldId: number,
            FieldName: "Direction",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          BaseParameterModifiers: {
            IsBackward: false,
            Content: BaseParameterModifierSchema,
            FieldId: number,
            FieldName: "BaseParameterModifiers",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "M2MRelation",
            IsRequired: false
          },
          Modifiers: {
            IsBackward: false,
            Content: ParameterModifierSchema,
            FieldId: number,
            FieldName: "Modifiers",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "M2MRelation",
            IsRequired: false
          },
          Unit: {
            IsBackward: false,
            Content: UnitSchema,
            FieldId: number,
            FieldName: "Unit",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          Title: {
            FieldId: number,
            FieldName: "Title",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "String",
            IsRequired: false
          },
          SortOrder: {
            FieldId: number,
            FieldName: "SortOrder",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Numeric",
            IsRequired: false
          },
          NumValue: {
            FieldId: number,
            FieldName: "NumValue",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Numeric",
            IsRequired: false
          },
          Value: {
            FieldId: number,
            FieldName: "Value",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "VisualEdit",
            IsRequired: false
          },
          Description: {
            FieldId: number,
            FieldName: "Description",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "VisualEdit",
            IsRequired: false
          },
          Image: {
            FieldId: number,
            FieldName: "Image",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Image",
            IsRequired: false
          },
          ProductGroup: {
            IsBackward: false,
            Content: {
              ContentId: number,
              ContentPath: string,
              ContentName: "Group",
              ContentTitle: string,
              ContentDescription: string,
              ForExtension: false,
              Fields: {
                Title: {
                  FieldId: number,
                  FieldName: "Title",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "String",
                  IsRequired: false
                },
                Alias: {
                  FieldId: number,
                  FieldName: "Alias",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "String",
                  IsRequired: false
                }
              }
            },
            FieldId: number,
            FieldName: "ProductGroup",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          Choice: {
            IsBackward: false,
            Content: ParameterChoiceSchema,
            FieldId: number,
            FieldName: "Choice",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          }
        },
        include: (selector: (fields: ProductSchema['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
      },
      FieldId: number,
      FieldName: "Parameters",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "M2ORelation",
      IsRequired: false,
      include: (selector: (fields: ProductSchema['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
    },
    Regions: {
      IsBackward: false,
      Content: {
        ContentId: number,
        ContentPath: string,
        ContentName: "Region",
        ContentTitle: string,
        ContentDescription: string,
        ForExtension: false,
        Fields: {
          Title: {
            FieldId: number,
            FieldName: "Title",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "String",
            IsRequired: false
          },
          Alias: {
            FieldId: number,
            FieldName: "Alias",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "String",
            IsRequired: false
          },
          Parent: {
            IsBackward: false,
            Content: Region2Schema,
            FieldId: number,
            FieldName: "Parent",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          IsMainCity: {
            FieldId: number,
            FieldName: "IsMainCity",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Boolean",
            IsRequired: false
          }
        },
        include: (selector: (fields: ProductSchema['Fields']['Regions']['Content']['Fields']) => Selection[]) => string[]
      },
      FieldId: number,
      FieldName: "Regions",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "M2MRelation",
      IsRequired: false,
      include: (selector: (fields: ProductSchema['Fields']['Regions']['Content']['Fields']) => Selection[]) => string[]
    },
    Type: {
      Contents: {
        Tariff: {
          ContentId: number,
          ContentPath: string,
          ContentName: "Tariff",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {}
        },
        Service: {
          ContentId: number,
          ContentPath: string,
          ContentName: "Service",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {}
        },
        Action: {
          ContentId: number,
          ContentPath: string,
          ContentName: "Action",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {}
        },
        RoamingScale: {
          ContentId: number,
          ContentPath: string,
          ContentName: "RoamingScale",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {}
        },
        Device: {
          ContentId: number,
          ContentPath: string,
          ContentName: "Device",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {
            Downloads: {
              IsBackward: false,
              Content: {
                ContentId: number,
                ContentPath: string,
                ContentName: "EquipmentDownload",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: false,
                Fields: {
                  Title: {
                    FieldId: number,
                    FieldName: "Title",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "String",
                    IsRequired: false
                  },
                  File: {
                    FieldId: number,
                    FieldName: "File",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "File",
                    IsRequired: false
                  }
                }
              },
              FieldId: number,
              FieldName: "Downloads",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "M2MRelation",
              IsRequired: false
            },
            Inners: {
              IsBackward: false,
              Content: {
                ContentId: number,
                ContentPath: string,
                ContentName: "Product",
                ContentTitle: string,
                ContentDescription: string,
                ForExtension: false,
                Fields: {
                  MarketingProduct: {
                    IsBackward: false,
                    Content: MarketingProduct3Schema,
                    FieldId: number,
                    FieldName: "MarketingProduct",
                    FieldTitle: string,
                    FieldDescription: string,
                    FieldOrder: number,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  }
                },
                include: (selector: (fields: ProductSchema['Fields']['Type']['Contents']['Device']['Fields']['Inners']['Content']['Fields']) => Selection[]) => string[]
              },
              FieldId: number,
              FieldName: "Inners",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "M2MRelation",
              IsRequired: false,
              include: (selector: (fields: ProductSchema['Fields']['Type']['Contents']['Device']['Fields']['Inners']['Content']['Fields']) => Selection[]) => string[]
            },
            FreezeDate: {
              FieldId: number,
              FieldName: "FreezeDate",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "DateTime",
              IsRequired: false
            },
            FullUserGuide: {
              FieldId: number,
              FieldName: "FullUserGuide",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "File",
              IsRequired: false
            },
            QuickStartGuide: {
              FieldId: number,
              FieldName: "QuickStartGuide",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "File",
              IsRequired: false
            }
          },
          include: (selector: (fields: ProductSchema['Fields']['Type']['Contents']['Device']['Fields']) => Selection[]) => string[]
        },
        FixConnectAction: {
          ContentId: number,
          ContentPath: string,
          ContentName: "FixConnectAction",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {
            MarketingOffers: {
              IsBackward: false,
              Content: MarketingProductSchema,
              FieldId: number,
              FieldName: "MarketingOffers",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "M2MRelation",
              IsRequired: false
            },
            PromoPeriod: {
              FieldId: number,
              FieldName: "PromoPeriod",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "String",
              IsRequired: false
            },
            AfterPromo: {
              FieldId: number,
              FieldName: "AfterPromo",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "String",
              IsRequired: false
            }
          },
          include: (selector: (fields: ProductSchema['Fields']['Type']['Contents']['FixConnectAction']['Fields']) => Selection[]) => string[]
        },
        TvPackage: {
          ContentId: number,
          ContentPath: string,
          ContentName: "TvPackage",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {}
        },
        FixConnectTariff: {
          ContentId: number,
          ContentPath: string,
          ContentName: "FixConnectTariff",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {
            TitleForSite: {
              FieldId: number,
              FieldName: "TitleForSite",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "String",
              IsRequired: false
            }
          }
        },
        PhoneTariff: {
          ContentId: number,
          ContentPath: string,
          ContentName: "PhoneTariff",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {
            RostelecomLink: {
              FieldId: number,
              FieldName: "RostelecomLink",
              FieldTitle: string,
              FieldDescription: string,
              FieldOrder: number,
              FieldType: "String",
              IsRequired: false
            }
          }
        },
        InternetTariff: {
          ContentId: number,
          ContentPath: string,
          ContentName: "InternetTariff",
          ContentTitle: string,
          ContentDescription: string,
          ForExtension: true,
          Fields: {}
        }
      },
      FieldId: number,
      FieldName: "Type",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Classifier",
      IsRequired: false,
      include: (selector: (contents: ProductSchema['Fields']['Type']['Contents']) => string[][]) => string[]
    },
    FixConnectAction: {
      IsBackward: true,
      Content: {
        ContentId: number,
        ContentPath: string,
        ContentName: "DevicesForFixConnectAction",
        ContentTitle: string,
        ContentDescription: string,
        ForExtension: false,
        Fields: {
          MarketingDevice: {
            IsBackward: false,
            Content: MarketingProductSchema,
            FieldId: number,
            FieldName: "MarketingDevice",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false
          },
          Parent: {
            IsBackward: false,
            Content: {
              ContentId: number,
              ContentPath: string,
              ContentName: "ProductRelation",
              ContentTitle: string,
              ContentDescription: string,
              ForExtension: false,
              Fields: {
                Title: {
                  FieldId: number,
                  FieldName: "Title",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "String",
                  IsRequired: false
                },
                Parameters: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "LinkParameter",
                    ContentTitle: string,
                    ContentDescription: string,
                    ForExtension: false,
                    Fields: {
                      BaseParameter: {
                        IsBackward: false,
                        Content: BaseParameterSchema,
                        FieldId: number,
                        FieldName: "BaseParameter",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "O2MRelation",
                        IsRequired: false
                      },
                      Zone: {
                        IsBackward: false,
                        Content: TariffZoneSchema,
                        FieldId: number,
                        FieldName: "Zone",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "O2MRelation",
                        IsRequired: false
                      },
                      Direction: {
                        IsBackward: false,
                        Content: DirectionSchema,
                        FieldId: number,
                        FieldName: "Direction",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "O2MRelation",
                        IsRequired: false
                      },
                      BaseParameterModifiers: {
                        IsBackward: false,
                        Content: BaseParameterModifierSchema,
                        FieldId: number,
                        FieldName: "BaseParameterModifiers",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2MRelation",
                        IsRequired: false
                      },
                      Modifiers: {
                        IsBackward: false,
                        Content: ParameterModifierSchema,
                        FieldId: number,
                        FieldName: "Modifiers",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "M2MRelation",
                        IsRequired: false
                      },
                      Unit: {
                        IsBackward: false,
                        Content: UnitSchema,
                        FieldId: number,
                        FieldName: "Unit",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "O2MRelation",
                        IsRequired: false
                      },
                      Title: {
                        FieldId: number,
                        FieldName: "Title",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "String",
                        IsRequired: false
                      },
                      SortOrder: {
                        FieldId: number,
                        FieldName: "SortOrder",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Numeric",
                        IsRequired: false
                      },
                      NumValue: {
                        FieldId: number,
                        FieldName: "NumValue",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "Numeric",
                        IsRequired: false
                      },
                      Value: {
                        FieldId: number,
                        FieldName: "Value",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "VisualEdit",
                        IsRequired: false
                      },
                      Description: {
                        FieldId: number,
                        FieldName: "Description",
                        FieldTitle: string,
                        FieldDescription: string,
                        FieldOrder: number,
                        FieldType: "VisualEdit",
                        IsRequired: false
                      }
                    },
                    include: (selector: (fields: ProductSchema['Fields']['FixConnectAction']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                  },
                  FieldId: number,
                  FieldName: "Parameters",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "M2ORelation",
                  IsRequired: false,
                  include: (selector: (fields: ProductSchema['Fields']['FixConnectAction']['Content']['Fields']['Parent']['Content']['Fields']['Parameters']['Content']['Fields']) => Selection[]) => string[]
                },
                Modifiers: {
                  IsBackward: false,
                  Content: LinkModifierSchema,
                  FieldId: number,
                  FieldName: "Modifiers",
                  FieldTitle: string,
                  FieldDescription: string,
                  FieldOrder: number,
                  FieldType: "M2MRelation",
                  IsRequired: false
                }
              },
              include: (selector: (fields: ProductSchema['Fields']['FixConnectAction']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
            },
            FieldId: number,
            FieldName: "Parent",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "O2MRelation",
            IsRequired: false,
            include: (selector: (fields: ProductSchema['Fields']['FixConnectAction']['Content']['Fields']['Parent']['Content']['Fields']) => Selection[]) => string[]
          },
          Order: {
            FieldId: number,
            FieldName: "Order",
            FieldTitle: string,
            FieldDescription: string,
            FieldOrder: number,
            FieldType: "Numeric",
            IsRequired: false
          }
        },
        include: (selector: (fields: ProductSchema['Fields']['FixConnectAction']['Content']['Fields']) => Selection[]) => string[]
      },
      FieldId: number,
      FieldName: "FixConnectAction",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "O2MRelation",
      IsRequired: false,
      include: (selector: (fields: ProductSchema['Fields']['FixConnectAction']['Content']['Fields']) => Selection[]) => string[]
    },
    Advantages: {
      IsBackward: false,
      Content: AdvantageSchema,
      FieldId: number,
      FieldName: "Advantages",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "M2MRelation",
      IsRequired: false
    }
  },
  include: (selector: (fields: ProductSchema['Fields']) => Selection[]) => string[]
}

interface SegmentSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Segment",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface ChannelCategorySchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "ChannelCategory",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Name: {
      FieldId: number,
      FieldName: "Name",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Segments: {
      FieldId: number,
      FieldName: "Segments",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Icon: {
      FieldId: number,
      FieldName: "Icon",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Image",
      IsRequired: false
    },
    Order: {
      FieldId: number,
      FieldName: "Order",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    OldSiteId: {
      FieldId: number,
      FieldName: "OldSiteId",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    }
  }
}
interface RegionSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Region",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Parent: {
      IsBackward: false,
      Content: Region2Schema,
      FieldId: number,
      FieldName: "Parent",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "O2MRelation",
      IsRequired: false
    },
    IsMainCity: {
      FieldId: number,
      FieldName: "IsMainCity",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Boolean",
      IsRequired: false
    }
  },
  include: (selector: (fields: RegionSchema['Fields']) => Selection[]) => string[]
}
interface Region1Schema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Region",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface Region2Schema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Region",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface ChannelFormatSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "ChannelFormat",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Image: {
      FieldId: number,
      FieldName: "Image",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Image",
      IsRequired: false
    },
    Message: {
      FieldId: number,
      FieldName: "Message",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    OldSiteId: {
      FieldId: number,
      FieldName: "OldSiteId",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    }
  }
}
interface FixedTypeSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "FixedType",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface MarketingProductSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "MarketingProduct",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Priority: {
      FieldId: number,
      FieldName: "Priority",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    }
  }
}
interface MarketingProduct1Schema {
  ContentId: number,
  ContentPath: string,
  ContentName: "MarketingProduct",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Link: {
      FieldId: number,
      FieldName: "Link",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Description: {
      FieldId: number,
      FieldName: "Description",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Textbox",
      IsRequired: false
    },
    DetailedDescription: {
      FieldId: number,
      FieldName: "DetailedDescription",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "VisualEdit",
      IsRequired: false
    },
    FullDescription: {
      FieldId: number,
      FieldName: "FullDescription",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "VisualEdit",
      IsRequired: false
    },
    SortOrder: {
      FieldId: number,
      FieldName: "SortOrder",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    Type: {
      FieldId: number,
      FieldName: "Type",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Classifier",
      IsRequired: false
    },
    OldSiteId: {
      FieldId: number,
      FieldName: "OldSiteId",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    OldCorpSiteId: {
      FieldId: number,
      FieldName: "OldCorpSiteId",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    ListImage: {
      FieldId: number,
      FieldName: "ListImage",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Image",
      IsRequired: false
    },
    DetailsImage: {
      FieldId: number,
      FieldName: "DetailsImage",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Image",
      IsRequired: false
    },
    Priority: {
      FieldId: number,
      FieldName: "Priority",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    ArchiveDate: {
      FieldId: number,
      FieldName: "ArchiveDate",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Date",
      IsRequired: false
    }
  }
}
interface MarketingProduct2Schema {
  ContentId: number,
  ContentPath: string,
  ContentName: "MarketingProduct",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface MarketingProduct3Schema {
  ContentId: number,
  ContentPath: string,
  ContentName: "MarketingProduct",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface BaseParameterSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "BaseParameter",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    AllowZone: {
      FieldId: number,
      FieldName: "AllowZone",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Boolean",
      IsRequired: false
    },
    AllowDirection: {
      FieldId: number,
      FieldName: "AllowDirection",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Boolean",
      IsRequired: false
    }
  }
}
interface TariffZoneSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "TariffZone",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface DirectionSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Direction",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface BaseParameterModifierSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "BaseParameterModifier",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Type: {
      Items: [
        {
          Value: "Step",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "Package",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "Zone",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "Direction",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "Refining",
          Alias: string,
          IsDefault: false,
          Invalid: false
        }
      ],
      FieldId: number,
      FieldName: "Type",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "StringEnum",
      IsRequired: false
    }
  }
}
interface ParameterModifierSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "ParameterModifier",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface UnitSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Unit",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Display: {
      FieldId: number,
      FieldName: "Display",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    QuotaUnit: {
      Items: [
        {
          Value: "mb",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "gb",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "kb",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "tb",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "min",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "message",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "rub",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "sms",
          Alias: "SMS",
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "mms",
          Alias: "MMS",
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "mbit",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "step",
          Alias: string,
          IsDefault: false,
          Invalid: false
        }
      ],
      FieldId: number,
      FieldName: "QuotaUnit",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "StringEnum",
      IsRequired: false
    },
    QuotaPeriod: {
      Items: [
        {
          Value: "daily",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "weekly",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "monthly",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "hourly",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "minutely",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "every_second",
          Alias: string,
          IsDefault: false,
          Invalid: false
        },
        {
          Value: "annually",
          Alias: string,
          IsDefault: false,
          Invalid: false
        }
      ],
      FieldId: number,
      FieldName: "QuotaPeriod",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "StringEnum",
      IsRequired: false
    }
  }
}
interface ParameterChoiceSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "ParameterChoice",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    OldSiteId: {
      FieldId: number,
      FieldName: "OldSiteId",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    }
  }
}
interface ProductParameterGroupSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "ProductParameterGroup",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    SortOrder: {
      FieldId: number,
      FieldName: "SortOrder",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    ImageSvg: {
      FieldId: number,
      FieldName: "ImageSvg",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "File",
      IsRequired: false
    },
    Type: {
      FieldId: number,
      FieldName: "Type",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface ProductParameterGroup1Schema {
  ContentId: number,
  ContentPath: string,
  ContentName: "ProductParameterGroup",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    SortOrder: {
      FieldId: number,
      FieldName: "SortOrder",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    ImageSvg: {
      FieldId: number,
      FieldName: "ImageSvg",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "File",
      IsRequired: false
    }
  }
}
interface LinkModifierSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "LinkModifier",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface ProductModiferSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "ProductModifer",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Alias: {
      FieldId: number,
      FieldName: "Alias",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    }
  }
}
interface AdvantageSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Advantage",
  ContentTitle: string,
  ContentDescription: string,
  ForExtension: false,
  Fields: {
    Title: {
      FieldId: number,
      FieldName: "Title",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Text: {
      FieldId: number,
      FieldName: "Text",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "String",
      IsRequired: false
    },
    Description: {
      FieldId: number,
      FieldName: "Description",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Textbox",
      IsRequired: false
    },
    ImageSvg: {
      FieldId: number,
      FieldName: "ImageSvg",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "File",
      IsRequired: false
    },
    SortOrder: {
      FieldId: number,
      FieldName: "SortOrder",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    },
    IsGift: {
      FieldId: number,
      FieldName: "IsGift",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Boolean",
      IsRequired: false
    },
    OldSiteId: {
      FieldId: number,
      FieldName: "OldSiteId",
      FieldTitle: string,
      FieldDescription: string,
      FieldOrder: number,
      FieldType: "Numeric",
      IsRequired: false
    }
  }
}
