// @ts-nocheck
/** Описание полей продукта */
export interface ProductEditorSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Product";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    MarketingProduct: {
      IsBackward: false;
      Content: {
        ContentId: number;
        ContentPath: string;
        ContentName: "MarketingProduct";
        ContentTitle: string;
        ContentDescription: string;
        ObjectShape: any;
        Fields: {
          Title: {
            FieldId: number;
            FieldName: "Title";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "String";
          };
          Alias: {
            FieldId: number;
            FieldName: "Alias";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "String";
          };
          Description: {
            FieldId: number;
            FieldName: "Description";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Textbox";
          };
          OldSiteId: {
            FieldId: number;
            FieldName: "OldSiteId";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Numeric";
          };
          OldCorpSiteId: {
            FieldId: number;
            FieldName: "OldCorpSiteId";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Numeric";
          };
          ListImage: {
            FieldId: number;
            FieldName: "ListImage";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Image";
          };
          DetailsImage: {
            FieldId: number;
            FieldName: "DetailsImage";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Image";
          };
          ArchiveDate: {
            FieldId: number;
            FieldName: "ArchiveDate";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Date";
          };
          Modifiers: {
            IsBackward: false;
            Content: ProductModiferSchema;
            FieldId: number;
            FieldName: "Modifiers";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "M2MRelation";
          };
          SortOrder: {
            FieldId: number;
            FieldName: "SortOrder";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Numeric";
          };
          Priority: {
            FieldId: number;
            FieldName: "Priority";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Numeric";
          };
          Advantages: {
            IsBackward: false;
            Content: AdvantageSchema;
            FieldId: number;
            FieldName: "Advantages";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "M2MRelation";
          };
          Type: {
            Contents: {
              MarketingTariff: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingTariff";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {};
              };
              MarketingService: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingService";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {};
              };
              MarketingAction: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingAction";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {};
              };
              MarketingRoamingScale: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingRoamingScale";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {};
              };
              MarketingDevice: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingDevice";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {
                  DeviceType: {
                    IsBackward: false;
                    Content: {
                      ContentId: number;
                      ContentPath: string;
                      ContentName: "EquipmentType";
                      ContentTitle: string;
                      ContentDescription: string;
                      ObjectShape: any;
                      Fields: {
                        ConnectionType: {
                          IsBackward: false;
                          Content: FixedTypeSchema;
                          FieldId: number;
                          FieldName: "ConnectionType";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "O2MRelation";
                        };
                        Title: {
                          FieldId: number;
                          FieldName: "Title";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "String";
                        };
                        Alias: {
                          FieldId: number;
                          FieldName: "Alias";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "String";
                        };
                        Order: {
                          FieldId: number;
                          FieldName: "Order";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Numeric";
                        };
                      };
                      include: (
                        selector: (
                          fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingDevice"]["Fields"]["DeviceType"]["Content"]["Fields"]
                        ) => Selection[]
                      ) => string[];
                    };
                    FieldId: number;
                    FieldName: "DeviceType";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingDevice"]["Fields"]["DeviceType"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  Segments: {
                    IsBackward: false;
                    Content: SegmentSchema;
                    FieldId: number;
                    FieldName: "Segments";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "M2MRelation";
                  };
                  CommunicationType: {
                    IsBackward: false;
                    Content: {
                      ContentId: number;
                      ContentPath: string;
                      ContentName: "CommunicationType";
                      ContentTitle: string;
                      ContentDescription: string;
                      ObjectShape: any;
                      Fields: {
                        Title: {
                          FieldId: number;
                          FieldName: "Title";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "String";
                        };
                        Alias: {
                          FieldId: number;
                          FieldName: "Alias";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "String";
                        };
                      };
                    };
                    FieldId: number;
                    FieldName: "CommunicationType";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                  };
                };
                include: (
                  selector: (
                    fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingDevice"]["Fields"]
                  ) => Selection[]
                ) => string[];
              };
              MarketingFixConnectAction: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingFixConnectAction";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {
                  Segment: {
                    IsBackward: false;
                    Content: SegmentSchema;
                    FieldId: number;
                    FieldName: "Segment";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "M2MRelation";
                  };
                  MarketingAction: {
                    IsBackward: false;
                    Content: MarketingProduct3Schema;
                    FieldId: number;
                    FieldName: "MarketingAction";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                  };
                  StartDate: {
                    FieldId: number;
                    FieldName: "StartDate";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "Date";
                  };
                  EndDate: {
                    FieldId: number;
                    FieldName: "EndDate";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "Date";
                  };
                  PromoPeriod: {
                    FieldId: number;
                    FieldName: "PromoPeriod";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "String";
                  };
                  AfterPromo: {
                    FieldId: number;
                    FieldName: "AfterPromo";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "String";
                  };
                };
                include: (
                  selector: (
                    fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingFixConnectAction"]["Fields"]
                  ) => Selection[]
                ) => string[];
              };
              MarketingTvPackage: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingTvPackage";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {
                  Channels: {
                    IsBackward: false;
                    Content: {
                      ContentId: number;
                      ContentPath: string;
                      ContentName: "TvChannel";
                      ContentTitle: string;
                      ContentDescription: string;
                      ObjectShape: any;
                      Fields: {
                        Title: {
                          FieldId: number;
                          FieldName: "Title";
                          FieldTitle: string;
                          FieldDescription: "title";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "String";
                        };
                        ShortDescription: {
                          FieldId: number;
                          FieldName: "ShortDescription";
                          FieldTitle: string;
                          FieldDescription: "short_descr";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Textbox";
                        };
                        Logo150: {
                          FieldId: number;
                          FieldName: "Logo150";
                          FieldTitle: string;
                          FieldDescription: "logo150";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Image";
                        };
                        IsRegional: {
                          FieldId: number;
                          FieldName: "IsRegional";
                          FieldTitle: string;
                          FieldDescription: "regional_tv";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Boolean";
                        };
                        Parent: {
                          IsBackward: false;
                          Content: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "TvChannel";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Logo150: {
                                FieldId: number;
                                FieldName: "Logo150";
                                FieldTitle: string;
                                FieldDescription: "logo150";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Image";
                              };
                            };
                          };
                          FieldId: number;
                          FieldName: "Parent";
                          FieldTitle: string;
                          FieldDescription: "ch_parent";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "O2MRelation";
                        };
                        Cities: {
                          IsBackward: false;
                          Content: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "NetworkCity";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              City: {
                                IsBackward: false;
                                Content: RegionSchema;
                                FieldId: number;
                                FieldName: "City";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "O2MRelation";
                              };
                              HasIpTv: {
                                FieldId: number;
                                FieldName: "HasIpTv";
                                FieldTitle: "IPTV";
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Boolean";
                              };
                            };
                            include: (
                              selector: (
                                fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]["Cities"]["Content"]["Fields"]
                              ) => Selection[]
                            ) => string[];
                          };
                          FieldId: number;
                          FieldName: "Cities";
                          FieldTitle: string;
                          FieldDescription: "cities";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "M2MRelation";
                          include: (
                            selector: (
                              fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]["Cities"]["Content"]["Fields"]
                            ) => Selection[]
                          ) => string[];
                        };
                        ChannelType: {
                          IsBackward: false;
                          Content: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "ChannelType";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Title: {
                                FieldId: number;
                                FieldName: "Title";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                            };
                          };
                          FieldId: number;
                          FieldName: "ChannelType";
                          FieldTitle: string;
                          FieldDescription: "ch_type";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "O2MRelation";
                        };
                        Category: {
                          IsBackward: false;
                          Content: ChannelCategorySchema;
                          FieldId: number;
                          FieldName: "Category";
                          FieldTitle: string;
                          FieldDescription: "ch_category";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "O2MRelation";
                        };
                        IsMtsMsk: {
                          FieldId: number;
                          FieldName: "IsMtsMsk";
                          FieldTitle: string;
                          FieldDescription: "test_inMSK_mgts_XML";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Boolean";
                        };
                        LcnDvbC: {
                          FieldId: number;
                          FieldName: "LcnDvbC";
                          FieldTitle: string;
                          FieldDescription: "lcn_dvbc";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Numeric";
                        };
                        LcnIpTv: {
                          FieldId: number;
                          FieldName: "LcnIpTv";
                          FieldTitle: string;
                          FieldDescription: "lcn_iptv";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Numeric";
                        };
                        LcnDvbS: {
                          FieldId: number;
                          FieldName: "LcnDvbS";
                          FieldTitle: string;
                          FieldDescription: "lcn_dvbs";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Numeric";
                        };
                        Disabled: {
                          FieldId: number;
                          FieldName: "Disabled";
                          FieldTitle: string;
                          FieldDescription: "offair";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Boolean";
                        };
                        Children: {
                          IsBackward: false;
                          Content: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "TvChannel";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Title: {
                                FieldId: number;
                                FieldName: "Title";
                                FieldTitle: string;
                                FieldDescription: "title";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                              Category: {
                                IsBackward: false;
                                Content: ChannelCategorySchema;
                                FieldId: number;
                                FieldName: "Category";
                                FieldTitle: string;
                                FieldDescription: "ch_category";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "O2MRelation";
                              };
                              ChannelType: {
                                IsBackward: false;
                                Content: {
                                  ContentId: number;
                                  ContentPath: string;
                                  ContentName: "ChannelType";
                                  ContentTitle: string;
                                  ContentDescription: string;
                                  ObjectShape: any;
                                  Fields: {
                                    Title: {
                                      FieldId: number;
                                      FieldName: "Title";
                                      FieldTitle: string;
                                      FieldDescription: string;
                                      FieldOrder: number;
                                      IsRequired: false;
                                      FieldType: "String";
                                    };
                                    OldSiteId: {
                                      FieldId: number;
                                      FieldName: "OldSiteId";
                                      FieldTitle: string;
                                      FieldDescription: string;
                                      FieldOrder: number;
                                      IsRequired: false;
                                      FieldType: "Numeric";
                                    };
                                  };
                                };
                                FieldId: number;
                                FieldName: "ChannelType";
                                FieldTitle: string;
                                FieldDescription: "ch_type";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "O2MRelation";
                              };
                              ShortDescription: {
                                FieldId: number;
                                FieldName: "ShortDescription";
                                FieldTitle: string;
                                FieldDescription: "short_descr";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Textbox";
                              };
                              Cities: {
                                IsBackward: false;
                                Content: {
                                  ContentId: number;
                                  ContentPath: string;
                                  ContentName: "NetworkCity";
                                  ContentTitle: string;
                                  ContentDescription: string;
                                  ObjectShape: any;
                                  Fields: {
                                    City: {
                                      IsBackward: false;
                                      Content: RegionSchema;
                                      FieldId: number;
                                      FieldName: "City";
                                      FieldTitle: string;
                                      FieldDescription: string;
                                      FieldOrder: number;
                                      IsRequired: false;
                                      FieldType: "O2MRelation";
                                    };
                                  };
                                  include: (
                                    selector: (
                                      fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]["Children"]["Content"]["Fields"]["Cities"]["Content"]["Fields"]
                                    ) => Selection[]
                                  ) => string[];
                                };
                                FieldId: number;
                                FieldName: "Cities";
                                FieldTitle: string;
                                FieldDescription: "cities";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "M2MRelation";
                                include: (
                                  selector: (
                                    fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]["Children"]["Content"]["Fields"]["Cities"]["Content"]["Fields"]
                                  ) => Selection[]
                                ) => string[];
                              };
                              Disabled: {
                                FieldId: number;
                                FieldName: "Disabled";
                                FieldTitle: string;
                                FieldDescription: "offair";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Boolean";
                              };
                              IsMtsMsk: {
                                FieldId: number;
                                FieldName: "IsMtsMsk";
                                FieldTitle: string;
                                FieldDescription: "test_inMSK_mgts_XML";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Boolean";
                              };
                              IsRegional: {
                                FieldId: number;
                                FieldName: "IsRegional";
                                FieldTitle: string;
                                FieldDescription: "regional_tv";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Boolean";
                              };
                              Logo150: {
                                FieldId: number;
                                FieldName: "Logo150";
                                FieldTitle: string;
                                FieldDescription: "logo150";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Image";
                              };
                              LcnDvbC: {
                                FieldId: number;
                                FieldName: "LcnDvbC";
                                FieldTitle: string;
                                FieldDescription: "lcn_dvbc";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                              LcnIpTv: {
                                FieldId: number;
                                FieldName: "LcnIpTv";
                                FieldTitle: string;
                                FieldDescription: "lcn_iptv";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                              LcnDvbS: {
                                FieldId: number;
                                FieldName: "LcnDvbS";
                                FieldTitle: string;
                                FieldDescription: "lcn_dvbs";
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                              Format: {
                                IsBackward: false;
                                Content: ChannelFormatSchema;
                                FieldId: number;
                                FieldName: "Format";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "O2MRelation";
                              };
                            };
                            include: (
                              selector: (
                                fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]["Children"]["Content"]["Fields"]
                              ) => Selection[]
                            ) => string[];
                          };
                          FieldId: number;
                          FieldName: "Children";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "M2ORelation";
                          include: (
                            selector: (
                              fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]["Children"]["Content"]["Fields"]
                            ) => Selection[]
                          ) => string[];
                        };
                        Format: {
                          IsBackward: false;
                          Content: ChannelFormatSchema;
                          FieldId: number;
                          FieldName: "Format";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "O2MRelation";
                        };
                        Logo40x30: {
                          FieldId: number;
                          FieldName: "Logo40x30";
                          FieldTitle: string;
                          FieldDescription: "logo40x30";
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Image";
                        };
                        TimeZone: {
                          IsBackward: false;
                          Content: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "TimeZone";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Name: {
                                FieldId: number;
                                FieldName: "Name";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                              Code: {
                                FieldId: number;
                                FieldName: "Code";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                              UTC: {
                                FieldId: number;
                                FieldName: "UTC";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                              MSK: {
                                FieldId: number;
                                FieldName: "MSK";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                              OldSiteId: {
                                FieldId: number;
                                FieldName: "OldSiteId";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                            };
                          };
                          FieldId: number;
                          FieldName: "TimeZone";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "O2MRelation";
                        };
                      };
                      include: (
                        selector: (
                          fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]
                        ) => Selection[]
                      ) => string[];
                    };
                    FieldId: number;
                    FieldName: "Channels";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "M2MRelation";
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]["Channels"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  TitleForSite: {
                    FieldId: number;
                    FieldName: "TitleForSite";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "String";
                  };
                  PackageType: {
                    Items: [
                      {
                        Value: "Base";
                        Alias: string;
                        IsDefault: false;
                        Invalid: false;
                      },
                      {
                        Value: "Additional";
                        Alias: string;
                        IsDefault: false;
                        Invalid: false;
                      }
                    ];
                    FieldId: number;
                    FieldName: "PackageType";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "StringEnum";
                  };
                };
                include: (
                  selector: (
                    fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingTvPackage"]["Fields"]
                  ) => Selection[]
                ) => string[];
              };
              MarketingFixConnectTariff: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingFixConnectTariff";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {
                  Segment: {
                    IsBackward: false;
                    Content: SegmentSchema;
                    FieldId: number;
                    FieldName: "Segment";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                  };
                  Category: {
                    IsBackward: false;
                    Content: {
                      ContentId: number;
                      ContentPath: string;
                      ContentName: "TariffCategory";
                      ContentTitle: string;
                      ContentDescription: string;
                      ObjectShape: any;
                      Fields: {
                        ConnectionTypes: {
                          IsBackward: false;
                          Content: FixedTypeSchema;
                          FieldId: number;
                          FieldName: "ConnectionTypes";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "M2MRelation";
                        };
                        Title: {
                          FieldId: number;
                          FieldName: "Title";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "String";
                        };
                        Alias: {
                          FieldId: number;
                          FieldName: "Alias";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "String";
                        };
                        Image: {
                          FieldId: number;
                          FieldName: "Image";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Image";
                        };
                        Order: {
                          FieldId: number;
                          FieldName: "Order";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "Numeric";
                        };
                        ImageSvg: {
                          FieldId: number;
                          FieldName: "ImageSvg";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "File";
                        };
                        TemplateType: {
                          Items: [
                            {
                              Value: "Tv";
                              Alias: string;
                              IsDefault: false;
                              Invalid: false;
                            },
                            {
                              Value: "Phone";
                              Alias: string;
                              IsDefault: false;
                              Invalid: false;
                            }
                          ];
                          FieldId: number;
                          FieldName: "TemplateType";
                          FieldTitle: string;
                          FieldDescription: string;
                          FieldOrder: number;
                          IsRequired: false;
                          FieldType: "StringEnum";
                        };
                      };
                      include: (
                        selector: (
                          fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingFixConnectTariff"]["Fields"]["Category"]["Content"]["Fields"]
                        ) => Selection[]
                      ) => string[];
                    };
                    FieldId: number;
                    FieldName: "Category";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingFixConnectTariff"]["Fields"]["Category"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  MarketingDevices: {
                    IsBackward: false;
                    Content: MarketingProductSchema;
                    FieldId: number;
                    FieldName: "MarketingDevices";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "M2MRelation";
                  };
                  BonusTVPackages: {
                    IsBackward: false;
                    Content: MarketingProductSchema;
                    FieldId: number;
                    FieldName: "BonusTVPackages";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "M2MRelation";
                  };
                  MarketingPhoneTariff: {
                    IsBackward: false;
                    Content: MarketingProduct1Schema;
                    FieldId: number;
                    FieldName: "MarketingPhoneTariff";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                  };
                  MarketingInternetTariff: {
                    IsBackward: false;
                    Content: MarketingProduct1Schema;
                    FieldId: number;
                    FieldName: "MarketingInternetTariff";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                  };
                  MarketingTvPackage: {
                    IsBackward: false;
                    Content: MarketingProduct2Schema;
                    FieldId: number;
                    FieldName: "MarketingTvPackage";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                  };
                  TitleForSite: {
                    FieldId: number;
                    FieldName: "TitleForSite";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "String";
                  };
                };
                include: (
                  selector: (
                    fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]["MarketingFixConnectTariff"]["Fields"]
                  ) => Selection[]
                ) => string[];
              };
              MarketingPhoneTariff: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingPhoneTariff";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {};
              };
              MarketingInternetTariff: {
                ContentId: number;
                ContentPath: string;
                ContentName: "MarketingInternetTariff";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {};
              };
            };
            FieldId: number;
            FieldName: "Type";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Classifier";
            include: (
              selector: (
                contents: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Type"]["Contents"]
              ) => string[][]
            ) => string[];
          };
          FullDescription: {
            FieldId: number;
            FieldName: "FullDescription";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "VisualEdit";
          };
          Parameters: {
            IsBackward: false;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "MarketingProductParameter";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                Group: {
                  IsBackward: false;
                  Content: ProductParameterGroupSchema;
                  FieldId: number;
                  FieldName: "Group";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                BaseParameter: {
                  IsBackward: false;
                  Content: BaseParameterSchema;
                  FieldId: number;
                  FieldName: "BaseParameter";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                Zone: {
                  IsBackward: false;
                  Content: TariffZoneSchema;
                  FieldId: number;
                  FieldName: "Zone";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                Direction: {
                  IsBackward: false;
                  Content: DirectionSchema;
                  FieldId: number;
                  FieldName: "Direction";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                BaseParameterModifiers: {
                  IsBackward: false;
                  Content: BaseParameterModifierSchema;
                  FieldId: number;
                  FieldName: "BaseParameterModifiers";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "M2MRelation";
                };
                Modifiers: {
                  IsBackward: false;
                  Content: ParameterModifierSchema;
                  FieldId: number;
                  FieldName: "Modifiers";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "M2MRelation";
                };
                Unit: {
                  IsBackward: false;
                  Content: UnitSchema;
                  FieldId: number;
                  FieldName: "Unit";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                Choice: {
                  IsBackward: false;
                  Content: ParameterChoiceSchema;
                  FieldId: number;
                  FieldName: "Choice";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                Title: {
                  FieldId: number;
                  FieldName: "Title";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "String";
                };
                SortOrder: {
                  FieldId: number;
                  FieldName: "SortOrder";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "Numeric";
                };
                NumValue: {
                  FieldId: number;
                  FieldName: "NumValue";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "Numeric";
                };
                Value: {
                  FieldId: number;
                  FieldName: "Value";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "VisualEdit";
                };
                Description: {
                  FieldId: number;
                  FieldName: "Description";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "VisualEdit";
                };
              };
              include: (
                selector: (
                  fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                ) => Selection[]
              ) => string[];
            };
            FieldId: number;
            FieldName: "Parameters";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "M2ORelation";
            include: (
              selector: (
                fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
              ) => Selection[]
            ) => string[];
          };
          TariffsOnMarketingDevice: {
            IsBackward: true;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "DeviceOnTariffs";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                Parent: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "ProductRelation";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Modifiers: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "LinkModifier";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Modifiers";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2MRelation";
                      };
                      Parameters: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "LinkParameter";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Group: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ProductParameterGroup";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  SortOrder: {
                                    FieldId: number;
                                    FieldName: "SortOrder";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                  ImageSvg: {
                                    FieldId: number;
                                    FieldName: "ImageSvg";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "File";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Group";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            BaseParameter: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "BaseParameter";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  AllowZone: {
                                    FieldId: number;
                                    FieldName: "AllowZone";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Boolean";
                                  };
                                  AllowDirection: {
                                    FieldId: number;
                                    FieldName: "AllowDirection";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Boolean";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "BaseParameter";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Zone: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "TariffZone";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Zone";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Direction: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "Direction";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Direction";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            BaseParameterModifiers: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "BaseParameterModifier";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Type: {
                                    Items: [
                                      {
                                        Value: "Step";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Package";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Zone";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Direction";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Refining";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "Type";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "BaseParameterModifiers";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "M2MRelation";
                            };
                            Modifiers: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ParameterModifier";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Modifiers";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "M2MRelation";
                            };
                            SortOrder: {
                              FieldId: number;
                              FieldName: "SortOrder";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            NumValue: {
                              FieldId: number;
                              FieldName: "NumValue";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            Value: {
                              FieldId: number;
                              FieldName: "Value";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "VisualEdit";
                            };
                            Description: {
                              FieldId: number;
                              FieldName: "Description";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "VisualEdit";
                            };
                            Unit: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "Unit";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Display: {
                                    FieldId: number;
                                    FieldName: "Display";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  QuotaUnit: {
                                    Items: [
                                      {
                                        Value: "mb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "gb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "kb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "tb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "min";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "message";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "rub";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "sms";
                                        Alias: "SMS";
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "mms";
                                        Alias: "MMS";
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "mbit";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "step";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "QuotaUnit";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                  QuotaPeriod: {
                                    Items: [
                                      {
                                        Value: "daily";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "weekly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "monthly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "hourly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "minutely";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "every_second";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "annually";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "QuotaPeriod";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Unit";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            ProductGroup: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "Group";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {};
                              };
                              FieldId: number;
                              FieldName: "ProductGroup";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Choice: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ParameterChoice";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  OldSiteId: {
                                    FieldId: number;
                                    FieldName: "OldSiteId";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Choice";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                          };
                          include: (
                            selector: (
                              fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["TariffsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                            ) => Selection[]
                          ) => string[];
                        };
                        FieldId: number;
                        FieldName: "Parameters";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2ORelation";
                        include: (
                          selector: (
                            fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["TariffsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                          ) => Selection[]
                        ) => string[];
                      };
                      Type: {
                        Contents: {
                          TariffTransfer: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "TariffTransfer";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {};
                          };
                          MutualGroup: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "MutualGroup";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {};
                          };
                          ServiceOnTariff: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "ServiceOnTariff";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Description: {
                                FieldId: number;
                                FieldName: "Description";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Textbox";
                              };
                            };
                          };
                          ServicesUpsale: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "ServicesUpsale";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Order: {
                                FieldId: number;
                                FieldName: "Order";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                            };
                          };
                          TariffOptionPackage: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "TariffOptionPackage";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              SubTitle: {
                                FieldId: number;
                                FieldName: "SubTitle";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Textbox";
                              };
                              Description: {
                                FieldId: number;
                                FieldName: "Description";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "VisualEdit";
                              };
                              Alias: {
                                FieldId: number;
                                FieldName: "Alias";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                              Link: {
                                FieldId: number;
                                FieldName: "Link";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "String";
                              };
                            };
                          };
                          ServiceRelation: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "ServiceRelation";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {};
                          };
                          RoamingScaleOnTariff: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "RoamingScaleOnTariff";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {};
                          };
                          ServiceOnRoamingScale: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "ServiceOnRoamingScale";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {};
                          };
                          CrossSale: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "CrossSale";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Order: {
                                FieldId: number;
                                FieldName: "Order";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                            };
                          };
                          MarketingCrossSale: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "MarketingCrossSale";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Order: {
                                FieldId: number;
                                FieldName: "Order";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                            };
                          };
                          DeviceOnTariffs: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "DeviceOnTariffs";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Order: {
                                FieldId: number;
                                FieldName: "Order";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                            };
                          };
                          DevicesForFixConnectAction: {
                            ContentId: number;
                            ContentPath: string;
                            ContentName: "DevicesForFixConnectAction";
                            ContentTitle: string;
                            ContentDescription: string;
                            ObjectShape: any;
                            Fields: {
                              Order: {
                                FieldId: number;
                                FieldName: "Order";
                                FieldTitle: string;
                                FieldDescription: string;
                                FieldOrder: number;
                                IsRequired: false;
                                FieldType: "Numeric";
                              };
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Type";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Classifier";
                        include: (
                          selector: (
                            contents: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["TariffsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Type"]["Contents"]
                          ) => string[][]
                        ) => string[];
                      };
                    };
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["TariffsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  FieldId: number;
                  FieldName: "Parent";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                  include: (
                    selector: (
                      fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["TariffsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
                    ) => Selection[]
                  ) => string[];
                };
                MarketingDevice: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "MarketingProduct";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Alias: {
                        FieldId: number;
                        FieldName: "Alias";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                    };
                  };
                  FieldId: number;
                  FieldName: "MarketingDevice";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                MarketingTariffs: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "MarketingProduct";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Alias: {
                        FieldId: number;
                        FieldName: "Alias";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                    };
                  };
                  FieldId: number;
                  FieldName: "MarketingTariffs";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "M2MRelation";
                };
                Cities: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "Region";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Alias: {
                        FieldId: number;
                        FieldName: "Alias";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                    };
                  };
                  FieldId: number;
                  FieldName: "Cities";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "M2MRelation";
                };
                Order: {
                  FieldId: number;
                  FieldName: "Order";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "Numeric";
                };
              };
              include: (
                selector: (
                  fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["TariffsOnMarketingDevice"]["Content"]["Fields"]
                ) => Selection[]
              ) => string[];
            };
            FieldId: number;
            FieldName: "TariffsOnMarketingDevice";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
            include: (
              selector: (
                fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["TariffsOnMarketingDevice"]["Content"]["Fields"]
              ) => Selection[]
            ) => string[];
          };
          DevicesOnMarketingTariff: {
            IsBackward: true;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "DeviceOnTariffs";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                Parent: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "ProductRelation";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Parameters: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "LinkParameter";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            SortOrder: {
                              FieldId: number;
                              FieldName: "SortOrder";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            NumValue: {
                              FieldId: number;
                              FieldName: "NumValue";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            Value: {
                              FieldId: number;
                              FieldName: "Value";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "VisualEdit";
                            };
                            Description: {
                              FieldId: number;
                              FieldName: "Description";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "VisualEdit";
                            };
                            Unit: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "Unit";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Display: {
                                    FieldId: number;
                                    FieldName: "Display";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  QuotaUnit: {
                                    Items: [
                                      {
                                        Value: "mb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "gb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "kb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "tb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "min";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "message";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "rub";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "sms";
                                        Alias: "SMS";
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "mms";
                                        Alias: "MMS";
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "mbit";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "step";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "QuotaUnit";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                  QuotaPeriod: {
                                    Items: [
                                      {
                                        Value: "daily";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "weekly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "monthly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "hourly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "minutely";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "every_second";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "annually";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "QuotaPeriod";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Unit";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Modifiers: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ParameterModifier";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Modifiers";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "M2MRelation";
                            };
                            BaseParameterModifiers: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "BaseParameterModifier";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Type: {
                                    Items: [
                                      {
                                        Value: "Step";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Package";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Zone";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Direction";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Refining";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "Type";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "BaseParameterModifiers";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "M2MRelation";
                            };
                            Direction: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "Direction";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Direction";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Zone: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "TariffZone";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Zone";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            BaseParameter: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "BaseParameter";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  AllowZone: {
                                    FieldId: number;
                                    FieldName: "AllowZone";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Boolean";
                                  };
                                  AllowDirection: {
                                    FieldId: number;
                                    FieldName: "AllowDirection";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Boolean";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "BaseParameter";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Group: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ProductParameterGroup";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  SortOrder: {
                                    FieldId: number;
                                    FieldName: "SortOrder";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  ImageSvg: {
                                    FieldId: number;
                                    FieldName: "ImageSvg";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "File";
                                  };
                                  Type: {
                                    FieldId: number;
                                    FieldName: "Type";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Group";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Choice: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ParameterChoice";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  OldSiteId: {
                                    FieldId: number;
                                    FieldName: "OldSiteId";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Choice";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                          };
                          include: (
                            selector: (
                              fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["DevicesOnMarketingTariff"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                            ) => Selection[]
                          ) => string[];
                        };
                        FieldId: number;
                        FieldName: "Parameters";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2ORelation";
                        include: (
                          selector: (
                            fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["DevicesOnMarketingTariff"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                          ) => Selection[]
                        ) => string[];
                      };
                      Modifiers: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "LinkModifier";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Modifiers";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2MRelation";
                      };
                    };
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["DevicesOnMarketingTariff"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  FieldId: number;
                  FieldName: "Parent";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                  include: (
                    selector: (
                      fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["DevicesOnMarketingTariff"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
                    ) => Selection[]
                  ) => string[];
                };
                MarketingDevice: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "MarketingProduct";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Alias: {
                        FieldId: number;
                        FieldName: "Alias";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                    };
                  };
                  FieldId: number;
                  FieldName: "MarketingDevice";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                };
                Cities: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "Region";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Alias: {
                        FieldId: number;
                        FieldName: "Alias";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                    };
                  };
                  FieldId: number;
                  FieldName: "Cities";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "M2MRelation";
                };
                Order: {
                  FieldId: number;
                  FieldName: "Order";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "Numeric";
                };
              };
              include: (
                selector: (
                  fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["DevicesOnMarketingTariff"]["Content"]["Fields"]
                ) => Selection[]
              ) => string[];
            };
            FieldId: number;
            FieldName: "DevicesOnMarketingTariff";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "M2MRelation";
            include: (
              selector: (
                fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["DevicesOnMarketingTariff"]["Content"]["Fields"]
              ) => Selection[]
            ) => string[];
          };
          ActionsOnMarketingDevice: {
            IsBackward: true;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "DevicesForFixConnectAction";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                FixConnectAction: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "Product";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      MarketingProduct: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "MarketingProduct";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "MarketingProduct";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "O2MRelation";
                      };
                      GlobalCode: {
                        FieldId: number;
                        FieldName: "GlobalCode";
                        FieldTitle: "GlobalCode";
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Type: {
                        FieldId: number;
                        FieldName: "Type";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Classifier";
                      };
                      Description: {
                        FieldId: number;
                        FieldName: "Description";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Textbox";
                      };
                      FullDescription: {
                        FieldId: number;
                        FieldName: "FullDescription";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "VisualEdit";
                      };
                      Notes: {
                        FieldId: number;
                        FieldName: "Notes";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Textbox";
                      };
                      Link: {
                        FieldId: number;
                        FieldName: "Link";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      SortOrder: {
                        FieldId: number;
                        FieldName: "SortOrder";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Numeric";
                      };
                      ForisID: {
                        FieldId: number;
                        FieldName: "ForisID";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Icon: {
                        FieldId: number;
                        FieldName: "Icon";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Image";
                      };
                      PDF: {
                        FieldId: number;
                        FieldName: "PDF";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "File";
                      };
                      PdfFixedAlias: {
                        FieldId: number;
                        FieldName: "PdfFixedAlias";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      PdfFixedLinks: {
                        FieldId: number;
                        FieldName: "PdfFixedLinks";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Textbox";
                      };
                      StartDate: {
                        FieldId: number;
                        FieldName: "StartDate";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Date";
                      };
                      EndDate: {
                        FieldId: number;
                        FieldName: "EndDate";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Date";
                      };
                      OldSiteId: {
                        FieldId: number;
                        FieldName: "OldSiteId";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Numeric";
                      };
                      OldId: {
                        FieldId: number;
                        FieldName: "OldId";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Numeric";
                      };
                      OldSiteInvId: {
                        FieldId: number;
                        FieldName: "OldSiteInvId";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      OldCorpSiteId: {
                        FieldId: number;
                        FieldName: "OldCorpSiteId";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Numeric";
                      };
                      OldAliasId: {
                        FieldId: number;
                        FieldName: "OldAliasId";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Priority: {
                        FieldId: number;
                        FieldName: "Priority";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Numeric";
                      };
                      ListImage: {
                        FieldId: number;
                        FieldName: "ListImage";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Image";
                      };
                      ArchiveDate: {
                        FieldId: number;
                        FieldName: "ArchiveDate";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Date";
                      };
                    };
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]["FixConnectAction"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  FieldId: number;
                  FieldName: "FixConnectAction";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                  include: (
                    selector: (
                      fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]["FixConnectAction"]["Content"]["Fields"]
                    ) => Selection[]
                  ) => string[];
                };
                Parent: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "ProductRelation";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Type: {
                        FieldId: number;
                        FieldName: "Type";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Classifier";
                      };
                      Parameters: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "LinkParameter";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Unit: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "Unit";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Display: {
                                    FieldId: number;
                                    FieldName: "Display";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  QuotaUnit: {
                                    Items: [
                                      {
                                        Value: "mb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "gb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "kb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "tb";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "min";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "message";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "rub";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "sms";
                                        Alias: "SMS";
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "mms";
                                        Alias: "MMS";
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "mbit";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "step";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "QuotaUnit";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                  QuotaPeriod: {
                                    Items: [
                                      {
                                        Value: "daily";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "weekly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "monthly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "hourly";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "minutely";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "every_second";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "annually";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "QuotaPeriod";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                  QuotaPeriodicity: {
                                    FieldId: number;
                                    FieldName: "QuotaPeriodicity";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  PeriodMultiplier: {
                                    FieldId: number;
                                    FieldName: "PeriodMultiplier";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                  Type: {
                                    FieldId: number;
                                    FieldName: "Type";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Unit";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            BaseParameterModifiers: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "BaseParameterModifier";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Type: {
                                    Items: [
                                      {
                                        Value: "Step";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Package";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Zone";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Direction";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      },
                                      {
                                        Value: "Refining";
                                        Alias: string;
                                        IsDefault: false;
                                        Invalid: false;
                                      }
                                    ];
                                    FieldId: number;
                                    FieldName: "Type";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "StringEnum";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "BaseParameterModifiers";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "M2MRelation";
                            };
                            Modifiers: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ParameterModifier";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Modifiers";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "M2MRelation";
                            };
                            Direction: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "Direction";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Direction";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Zone: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "TariffZone";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Zone";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            BaseParameter: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "BaseParameter";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  AllowZone: {
                                    FieldId: number;
                                    FieldName: "AllowZone";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Boolean";
                                  };
                                  AllowDirection: {
                                    FieldId: number;
                                    FieldName: "AllowDirection";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Boolean";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "BaseParameter";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Group: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ProductParameterGroup";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  SortOrder: {
                                    FieldId: number;
                                    FieldName: "SortOrder";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                  OldSiteId: {
                                    FieldId: number;
                                    FieldName: "OldSiteId";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                  OldCorpSiteId: {
                                    FieldId: number;
                                    FieldName: "OldCorpSiteId";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                  ImageSvg: {
                                    FieldId: number;
                                    FieldName: "ImageSvg";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "File";
                                  };
                                  Type: {
                                    FieldId: number;
                                    FieldName: "Type";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  TitleForIcin: {
                                    FieldId: number;
                                    FieldName: "TitleForIcin";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Group";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Choice: {
                              IsBackward: false;
                              Content: {
                                ContentId: number;
                                ContentPath: string;
                                ContentName: "ParameterChoice";
                                ContentTitle: string;
                                ContentDescription: string;
                                ObjectShape: any;
                                Fields: {
                                  Title: {
                                    FieldId: number;
                                    FieldName: "Title";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  Alias: {
                                    FieldId: number;
                                    FieldName: "Alias";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "String";
                                  };
                                  OldSiteId: {
                                    FieldId: number;
                                    FieldName: "OldSiteId";
                                    FieldTitle: string;
                                    FieldDescription: string;
                                    FieldOrder: number;
                                    IsRequired: false;
                                    FieldType: "Numeric";
                                  };
                                };
                              };
                              FieldId: number;
                              FieldName: "Choice";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "O2MRelation";
                            };
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            SortOrder: {
                              FieldId: number;
                              FieldName: "SortOrder";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            NumValue: {
                              FieldId: number;
                              FieldName: "NumValue";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            Value: {
                              FieldId: number;
                              FieldName: "Value";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "VisualEdit";
                            };
                            Description: {
                              FieldId: number;
                              FieldName: "Description";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "VisualEdit";
                            };
                            OldSiteId: {
                              FieldId: number;
                              FieldName: "OldSiteId";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            OldCorpSiteId: {
                              FieldId: number;
                              FieldName: "OldCorpSiteId";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            OldPointId: {
                              FieldId: number;
                              FieldName: "OldPointId";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                            OldCorpPointId: {
                              FieldId: number;
                              FieldName: "OldCorpPointId";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Numeric";
                            };
                          };
                          include: (
                            selector: (
                              fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                            ) => Selection[]
                          ) => string[];
                        };
                        FieldId: number;
                        FieldName: "Parameters";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2ORelation";
                        include: (
                          selector: (
                            fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                          ) => Selection[]
                        ) => string[];
                      };
                      Modifiers: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "LinkModifier";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Modifiers";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2MRelation";
                      };
                    };
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  FieldId: number;
                  FieldName: "Parent";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "O2MRelation";
                  include: (
                    selector: (
                      fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
                    ) => Selection[]
                  ) => string[];
                };
                Order: {
                  FieldId: number;
                  FieldName: "Order";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "Numeric";
                };
              };
              include: (
                selector: (
                  fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]
                ) => Selection[]
              ) => string[];
            };
            FieldId: number;
            FieldName: "ActionsOnMarketingDevice";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
            include: (
              selector: (
                fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]["ActionsOnMarketingDevice"]["Content"]["Fields"]
              ) => Selection[]
            ) => string[];
          };
        };
        include: (
          selector: (
            fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]
          ) => Selection[]
        ) => string[];
      };
      FieldId: number;
      FieldName: "MarketingProduct";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "O2MRelation";
      include: (
        selector: (
          fields: ProductEditorSchema["Fields"]["MarketingProduct"]["Content"]["Fields"]
        ) => Selection[]
      ) => string[];
    };
    GlobalCode: {
      FieldId: number;
      FieldName: "GlobalCode";
      FieldTitle: "GlobalCode";
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Description: {
      FieldId: number;
      FieldName: "Description";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Textbox";
    };
    FullDescription: {
      FieldId: number;
      FieldName: "FullDescription";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "VisualEdit";
    };
    Notes: {
      FieldId: number;
      FieldName: "Notes";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Textbox";
    };
    Link: {
      FieldId: number;
      FieldName: "Link";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    SortOrder: {
      FieldId: number;
      FieldName: "SortOrder";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    Icon: {
      FieldId: number;
      FieldName: "Icon";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Image";
    };
    PDF: {
      FieldId: number;
      FieldName: "PDF";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "File";
    };
    StartDate: {
      FieldId: number;
      FieldName: "StartDate";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Date";
    };
    EndDate: {
      FieldId: number;
      FieldName: "EndDate";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Date";
    };
    Priority: {
      FieldId: number;
      FieldName: "Priority";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    ListImage: {
      FieldId: number;
      FieldName: "ListImage";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Image";
    };
    ArchiveDate: {
      FieldId: number;
      FieldName: "ArchiveDate";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Date";
    };
    Modifiers: {
      IsBackward: false;
      Content: ProductModiferSchema;
      FieldId: number;
      FieldName: "Modifiers";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "M2MRelation";
    };
    Parameters: {
      IsBackward: false;
      Content: {
        ContentId: number;
        ContentPath: string;
        ContentName: "ProductParameter";
        ContentTitle: string;
        ContentDescription: string;
        ObjectShape: any;
        Fields: {
          Group: {
            IsBackward: false;
            Content: ProductParameterGroup1Schema;
            FieldId: number;
            FieldName: "Group";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          Parent: {
            IsBackward: false;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "ProductParameter";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                Title: {
                  FieldId: number;
                  FieldName: "Title";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "String";
                };
              };
            };
            FieldId: number;
            FieldName: "Parent";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          BaseParameter: {
            IsBackward: false;
            Content: BaseParameterSchema;
            FieldId: number;
            FieldName: "BaseParameter";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          Zone: {
            IsBackward: false;
            Content: TariffZoneSchema;
            FieldId: number;
            FieldName: "Zone";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          Direction: {
            IsBackward: false;
            Content: DirectionSchema;
            FieldId: number;
            FieldName: "Direction";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          BaseParameterModifiers: {
            IsBackward: false;
            Content: BaseParameterModifierSchema;
            FieldId: number;
            FieldName: "BaseParameterModifiers";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "M2MRelation";
          };
          Modifiers: {
            IsBackward: false;
            Content: ParameterModifierSchema;
            FieldId: number;
            FieldName: "Modifiers";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "M2MRelation";
          };
          Unit: {
            IsBackward: false;
            Content: UnitSchema;
            FieldId: number;
            FieldName: "Unit";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          Title: {
            FieldId: number;
            FieldName: "Title";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "String";
          };
          SortOrder: {
            FieldId: number;
            FieldName: "SortOrder";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Numeric";
          };
          NumValue: {
            FieldId: number;
            FieldName: "NumValue";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Numeric";
          };
          Value: {
            FieldId: number;
            FieldName: "Value";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "VisualEdit";
          };
          Description: {
            FieldId: number;
            FieldName: "Description";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "VisualEdit";
          };
          Image: {
            FieldId: number;
            FieldName: "Image";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Image";
          };
          ProductGroup: {
            IsBackward: false;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "Group";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                Title: {
                  FieldId: number;
                  FieldName: "Title";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "String";
                };
                Alias: {
                  FieldId: number;
                  FieldName: "Alias";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "String";
                };
              };
            };
            FieldId: number;
            FieldName: "ProductGroup";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          Choice: {
            IsBackward: false;
            Content: ParameterChoiceSchema;
            FieldId: number;
            FieldName: "Choice";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
        };
        include: (
          selector: (
            fields: ProductEditorSchema["Fields"]["Parameters"]["Content"]["Fields"]
          ) => Selection[]
        ) => string[];
      };
      FieldId: number;
      FieldName: "Parameters";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "M2ORelation";
      include: (
        selector: (
          fields: ProductEditorSchema["Fields"]["Parameters"]["Content"]["Fields"]
        ) => Selection[]
      ) => string[];
    };
    Regions: {
      IsBackward: false;
      Content: {
        ContentId: number;
        ContentPath: string;
        ContentName: "Region";
        ContentTitle: string;
        ContentDescription: string;
        ObjectShape: any;
        Fields: {
          Title: {
            FieldId: number;
            FieldName: "Title";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "String";
          };
          Alias: {
            FieldId: number;
            FieldName: "Alias";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "String";
          };
          Parent: {
            IsBackward: false;
            Content: Region2Schema;
            FieldId: number;
            FieldName: "Parent";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          IsMainCity: {
            FieldId: number;
            FieldName: "IsMainCity";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Boolean";
          };
        };
        include: (
          selector: (
            fields: ProductEditorSchema["Fields"]["Regions"]["Content"]["Fields"]
          ) => Selection[]
        ) => string[];
      };
      FieldId: number;
      FieldName: "Regions";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "M2MRelation";
      include: (
        selector: (
          fields: ProductEditorSchema["Fields"]["Regions"]["Content"]["Fields"]
        ) => Selection[]
      ) => string[];
    };
    Type: {
      Contents: {
        Tariff: {
          ContentId: number;
          ContentPath: string;
          ContentName: "Tariff";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {};
        };
        Service: {
          ContentId: number;
          ContentPath: string;
          ContentName: "Service";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {};
        };
        Action: {
          ContentId: number;
          ContentPath: string;
          ContentName: "Action";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {};
        };
        RoamingScale: {
          ContentId: number;
          ContentPath: string;
          ContentName: "RoamingScale";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {};
        };
        Device: {
          ContentId: number;
          ContentPath: string;
          ContentName: "Device";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {
            Downloads: {
              IsBackward: false;
              Content: {
                ContentId: number;
                ContentPath: string;
                ContentName: "EquipmentDownload";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {
                  Title: {
                    FieldId: number;
                    FieldName: "Title";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "String";
                  };
                  File: {
                    FieldId: number;
                    FieldName: "File";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "File";
                  };
                };
              };
              FieldId: number;
              FieldName: "Downloads";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "M2MRelation";
            };
            Inners: {
              IsBackward: false;
              Content: {
                ContentId: number;
                ContentPath: string;
                ContentName: "Product";
                ContentTitle: string;
                ContentDescription: string;
                ObjectShape: any;
                Fields: {
                  MarketingProduct: {
                    IsBackward: false;
                    Content: MarketingProduct3Schema;
                    FieldId: number;
                    FieldName: "MarketingProduct";
                    FieldTitle: string;
                    FieldDescription: string;
                    FieldOrder: number;
                    IsRequired: false;
                    FieldType: "O2MRelation";
                  };
                };
                include: (
                  selector: (
                    fields: ProductEditorSchema["Fields"]["Type"]["Contents"]["Device"]["Fields"]["Inners"]["Content"]["Fields"]
                  ) => Selection[]
                ) => string[];
              };
              FieldId: number;
              FieldName: "Inners";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "M2MRelation";
              include: (
                selector: (
                  fields: ProductEditorSchema["Fields"]["Type"]["Contents"]["Device"]["Fields"]["Inners"]["Content"]["Fields"]
                ) => Selection[]
              ) => string[];
            };
            FreezeDate: {
              FieldId: number;
              FieldName: "FreezeDate";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "DateTime";
            };
            FullUserGuide: {
              FieldId: number;
              FieldName: "FullUserGuide";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "File";
            };
            QuickStartGuide: {
              FieldId: number;
              FieldName: "QuickStartGuide";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "File";
            };
          };
          include: (
            selector: (
              fields: ProductEditorSchema["Fields"]["Type"]["Contents"]["Device"]["Fields"]
            ) => Selection[]
          ) => string[];
        };
        FixConnectAction: {
          ContentId: number;
          ContentPath: string;
          ContentName: "FixConnectAction";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {
            MarketingOffers: {
              IsBackward: false;
              Content: MarketingProductSchema;
              FieldId: number;
              FieldName: "MarketingOffers";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "M2MRelation";
            };
            PromoPeriod: {
              FieldId: number;
              FieldName: "PromoPeriod";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "String";
            };
            AfterPromo: {
              FieldId: number;
              FieldName: "AfterPromo";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "String";
            };
          };
          include: (
            selector: (
              fields: ProductEditorSchema["Fields"]["Type"]["Contents"]["FixConnectAction"]["Fields"]
            ) => Selection[]
          ) => string[];
        };
        TvPackage: {
          ContentId: number;
          ContentPath: string;
          ContentName: "TvPackage";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {};
        };
        FixConnectTariff: {
          ContentId: number;
          ContentPath: string;
          ContentName: "FixConnectTariff";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {
            TitleForSite: {
              FieldId: number;
              FieldName: "TitleForSite";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "String";
            };
          };
        };
        PhoneTariff: {
          ContentId: number;
          ContentPath: string;
          ContentName: "PhoneTariff";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {
            RostelecomLink: {
              FieldId: number;
              FieldName: "RostelecomLink";
              FieldTitle: string;
              FieldDescription: string;
              FieldOrder: number;
              IsRequired: false;
              FieldType: "String";
            };
          };
        };
        InternetTariff: {
          ContentId: number;
          ContentPath: string;
          ContentName: "InternetTariff";
          ContentTitle: string;
          ContentDescription: string;
          ObjectShape: any;
          Fields: {};
        };
      };
      FieldId: number;
      FieldName: "Type";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Classifier";
      include: (
        selector: (contents: ProductEditorSchema["Fields"]["Type"]["Contents"]) => string[][]
      ) => string[];
    };
    FixConnectAction: {
      IsBackward: true;
      Content: {
        ContentId: number;
        ContentPath: string;
        ContentName: "DevicesForFixConnectAction";
        ContentTitle: string;
        ContentDescription: string;
        ObjectShape: any;
        Fields: {
          MarketingDevice: {
            IsBackward: false;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "MarketingProduct";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                Title: {
                  FieldId: number;
                  FieldName: "Title";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "String";
                };
                Alias: {
                  FieldId: number;
                  FieldName: "Alias";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "String";
                };
                Priority: {
                  FieldId: number;
                  FieldName: "Priority";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "Numeric";
                };
              };
            };
            FieldId: number;
            FieldName: "MarketingDevice";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
          };
          Parent: {
            IsBackward: false;
            Content: {
              ContentId: number;
              ContentPath: string;
              ContentName: "ProductRelation";
              ContentTitle: string;
              ContentDescription: string;
              ObjectShape: any;
              Fields: {
                Title: {
                  FieldId: number;
                  FieldName: "Title";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "String";
                };
                Parameters: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "LinkParameter";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      BaseParameter: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "BaseParameter";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            AllowZone: {
                              FieldId: number;
                              FieldName: "AllowZone";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Boolean";
                            };
                            AllowDirection: {
                              FieldId: number;
                              FieldName: "AllowDirection";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "Boolean";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "BaseParameter";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "O2MRelation";
                      };
                      Zone: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "TariffZone";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Zone";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "O2MRelation";
                      };
                      Direction: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "Direction";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Direction";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "O2MRelation";
                      };
                      BaseParameterModifiers: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "BaseParameterModifier";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Type: {
                              Items: [
                                {
                                  Value: "Step";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "Package";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "Zone";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "Direction";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "Refining";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                }
                              ];
                              FieldId: number;
                              FieldName: "Type";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "StringEnum";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "BaseParameterModifiers";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2MRelation";
                      };
                      Modifiers: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "ParameterModifier";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Modifiers";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "M2MRelation";
                      };
                      Unit: {
                        IsBackward: false;
                        Content: {
                          ContentId: number;
                          ContentPath: string;
                          ContentName: "Unit";
                          ContentTitle: string;
                          ContentDescription: string;
                          ObjectShape: any;
                          Fields: {
                            Alias: {
                              FieldId: number;
                              FieldName: "Alias";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Title: {
                              FieldId: number;
                              FieldName: "Title";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            Display: {
                              FieldId: number;
                              FieldName: "Display";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "String";
                            };
                            QuotaUnit: {
                              Items: [
                                {
                                  Value: "mb";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "gb";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "kb";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "tb";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "min";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "message";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "rub";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "sms";
                                  Alias: "SMS";
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "mms";
                                  Alias: "MMS";
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "mbit";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "step";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                }
                              ];
                              FieldId: number;
                              FieldName: "QuotaUnit";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "StringEnum";
                            };
                            QuotaPeriod: {
                              Items: [
                                {
                                  Value: "daily";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "weekly";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "monthly";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "hourly";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "minutely";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "every_second";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                },
                                {
                                  Value: "annually";
                                  Alias: string;
                                  IsDefault: false;
                                  Invalid: false;
                                }
                              ];
                              FieldId: number;
                              FieldName: "QuotaPeriod";
                              FieldTitle: string;
                              FieldDescription: string;
                              FieldOrder: number;
                              IsRequired: false;
                              FieldType: "StringEnum";
                            };
                          };
                        };
                        FieldId: number;
                        FieldName: "Unit";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "O2MRelation";
                      };
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      SortOrder: {
                        FieldId: number;
                        FieldName: "SortOrder";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Numeric";
                      };
                      NumValue: {
                        FieldId: number;
                        FieldName: "NumValue";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "Numeric";
                      };
                      Value: {
                        FieldId: number;
                        FieldName: "Value";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "VisualEdit";
                      };
                      Description: {
                        FieldId: number;
                        FieldName: "Description";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "VisualEdit";
                      };
                    };
                    include: (
                      selector: (
                        fields: ProductEditorSchema["Fields"]["FixConnectAction"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                      ) => Selection[]
                    ) => string[];
                  };
                  FieldId: number;
                  FieldName: "Parameters";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "M2ORelation";
                  include: (
                    selector: (
                      fields: ProductEditorSchema["Fields"]["FixConnectAction"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]["Parameters"]["Content"]["Fields"]
                    ) => Selection[]
                  ) => string[];
                };
                Modifiers: {
                  IsBackward: false;
                  Content: {
                    ContentId: number;
                    ContentPath: string;
                    ContentName: "LinkModifier";
                    ContentTitle: string;
                    ContentDescription: string;
                    ObjectShape: any;
                    Fields: {
                      Title: {
                        FieldId: number;
                        FieldName: "Title";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                      Alias: {
                        FieldId: number;
                        FieldName: "Alias";
                        FieldTitle: string;
                        FieldDescription: string;
                        FieldOrder: number;
                        IsRequired: false;
                        FieldType: "String";
                      };
                    };
                  };
                  FieldId: number;
                  FieldName: "Modifiers";
                  FieldTitle: string;
                  FieldDescription: string;
                  FieldOrder: number;
                  IsRequired: false;
                  FieldType: "M2MRelation";
                };
              };
              include: (
                selector: (
                  fields: ProductEditorSchema["Fields"]["FixConnectAction"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
                ) => Selection[]
              ) => string[];
            };
            FieldId: number;
            FieldName: "Parent";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "O2MRelation";
            include: (
              selector: (
                fields: ProductEditorSchema["Fields"]["FixConnectAction"]["Content"]["Fields"]["Parent"]["Content"]["Fields"]
              ) => Selection[]
            ) => string[];
          };
          Order: {
            FieldId: number;
            FieldName: "Order";
            FieldTitle: string;
            FieldDescription: string;
            FieldOrder: number;
            IsRequired: false;
            FieldType: "Numeric";
          };
        };
        include: (
          selector: (
            fields: ProductEditorSchema["Fields"]["FixConnectAction"]["Content"]["Fields"]
          ) => Selection[]
        ) => string[];
      };
      FieldId: number;
      FieldName: "FixConnectAction";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "O2MRelation";
      include: (
        selector: (
          fields: ProductEditorSchema["Fields"]["FixConnectAction"]["Content"]["Fields"]
        ) => Selection[]
      ) => string[];
    };
    Advantages: {
      IsBackward: false;
      Content: AdvantageSchema;
      FieldId: number;
      FieldName: "Advantages";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "M2MRelation";
    };
  };
  include: (selector: (fields: ProductEditorSchema["Fields"]) => Selection[]) => string[];
}
interface SegmentSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Segment";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface ChannelCategorySchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "ChannelCategory";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Name: {
      FieldId: number;
      FieldName: "Name";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Segments: {
      FieldId: number;
      FieldName: "Segments";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Icon: {
      FieldId: number;
      FieldName: "Icon";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Image";
    };
    Order: {
      FieldId: number;
      FieldName: "Order";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    OldSiteId: {
      FieldId: number;
      FieldName: "OldSiteId";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
  };
}
interface RegionSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Region";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Parent: {
      IsBackward: false;
      Content: Region2Schema;
      FieldId: number;
      FieldName: "Parent";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "O2MRelation";
    };
    IsMainCity: {
      FieldId: number;
      FieldName: "IsMainCity";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Boolean";
    };
  };
  include: (selector: (fields: RegionSchema["Fields"]) => Selection[]) => string[];
}
interface Region1Schema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Region";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface Region2Schema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Region";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface ChannelFormatSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "ChannelFormat";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Image: {
      FieldId: number;
      FieldName: "Image";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Image";
    };
    Message: {
      FieldId: number;
      FieldName: "Message";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    OldSiteId: {
      FieldId: number;
      FieldName: "OldSiteId";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
  };
}
interface FixedTypeSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "FixedType";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface MarketingProductSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "MarketingProduct";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Priority: {
      FieldId: number;
      FieldName: "Priority";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
  };
}
interface MarketingProduct1Schema {
  ContentId: number;
  ContentPath: string;
  ContentName: "MarketingProduct";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Link: {
      FieldId: number;
      FieldName: "Link";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Description: {
      FieldId: number;
      FieldName: "Description";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Textbox";
    };
    DetailedDescription: {
      FieldId: number;
      FieldName: "DetailedDescription";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "VisualEdit";
    };
    FullDescription: {
      FieldId: number;
      FieldName: "FullDescription";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "VisualEdit";
    };
    SortOrder: {
      FieldId: number;
      FieldName: "SortOrder";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    Type: {
      FieldId: number;
      FieldName: "Type";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Classifier";
    };
    OldSiteId: {
      FieldId: number;
      FieldName: "OldSiteId";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    OldCorpSiteId: {
      FieldId: number;
      FieldName: "OldCorpSiteId";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    ListImage: {
      FieldId: number;
      FieldName: "ListImage";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Image";
    };
    DetailsImage: {
      FieldId: number;
      FieldName: "DetailsImage";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Image";
    };
    Priority: {
      FieldId: number;
      FieldName: "Priority";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    ArchiveDate: {
      FieldId: number;
      FieldName: "ArchiveDate";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Date";
    };
  };
}
interface MarketingProduct2Schema {
  ContentId: number;
  ContentPath: string;
  ContentName: "MarketingProduct";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface MarketingProduct3Schema {
  ContentId: number;
  ContentPath: string;
  ContentName: "MarketingProduct";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface BaseParameterSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "BaseParameter";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    AllowZone: {
      FieldId: number;
      FieldName: "AllowZone";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Boolean";
    };
    AllowDirection: {
      FieldId: number;
      FieldName: "AllowDirection";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Boolean";
    };
  };
}
interface TariffZoneSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "TariffZone";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface DirectionSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Direction";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface BaseParameterModifierSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "BaseParameterModifier";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Type: {
      Items: [
        {
          Value: "Step";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "Package";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "Zone";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "Direction";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "Refining";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        }
      ];
      FieldId: number;
      FieldName: "Type";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "StringEnum";
    };
  };
}
interface ParameterModifierSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "ParameterModifier";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface UnitSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Unit";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Display: {
      FieldId: number;
      FieldName: "Display";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    QuotaUnit: {
      Items: [
        {
          Value: "mb";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "gb";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "kb";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "tb";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "min";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "message";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "rub";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "sms";
          Alias: "SMS";
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "mms";
          Alias: "MMS";
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "mbit";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "step";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        }
      ];
      FieldId: number;
      FieldName: "QuotaUnit";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "StringEnum";
    };
    QuotaPeriod: {
      Items: [
        {
          Value: "daily";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "weekly";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "monthly";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "hourly";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "minutely";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "every_second";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        },
        {
          Value: "annually";
          Alias: string;
          IsDefault: false;
          Invalid: false;
        }
      ];
      FieldId: number;
      FieldName: "QuotaPeriod";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "StringEnum";
    };
  };
}
interface ParameterChoiceSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "ParameterChoice";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    OldSiteId: {
      FieldId: number;
      FieldName: "OldSiteId";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
  };
}
interface ProductParameterGroupSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "ProductParameterGroup";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    SortOrder: {
      FieldId: number;
      FieldName: "SortOrder";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    ImageSvg: {
      FieldId: number;
      FieldName: "ImageSvg";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "File";
    };
    Type: {
      FieldId: number;
      FieldName: "Type";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface ProductParameterGroup1Schema {
  ContentId: number;
  ContentPath: string;
  ContentName: "ProductParameterGroup";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    SortOrder: {
      FieldId: number;
      FieldName: "SortOrder";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    ImageSvg: {
      FieldId: number;
      FieldName: "ImageSvg";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "File";
    };
  };
}
interface LinkModifierSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "LinkModifier";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface ProductModiferSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "ProductModifer";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Alias: {
      FieldId: number;
      FieldName: "Alias";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
  };
}
interface AdvantageSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: "Advantage";
  ContentTitle: string;
  ContentDescription: string;
  ObjectShape: any;
  Fields: {
    Title: {
      FieldId: number;
      FieldName: "Title";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Text: {
      FieldId: number;
      FieldName: "Text";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "String";
    };
    Description: {
      FieldId: number;
      FieldName: "Description";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Textbox";
    };
    ImageSvg: {
      FieldId: number;
      FieldName: "ImageSvg";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "File";
    };
    SortOrder: {
      FieldId: number;
      FieldName: "SortOrder";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
    IsGift: {
      FieldId: number;
      FieldName: "IsGift";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Boolean";
    };
    OldSiteId: {
      FieldId: number;
      FieldName: "OldSiteId";
      FieldTitle: string;
      FieldDescription: string;
      FieldOrder: number;
      IsRequired: false;
      FieldType: "Numeric";
    };
  };
}

const objectShapes: any = {
  "290": {
    Id: null,
    Title: null,
    Alias: null,
    Parent: [],
    IsMainCity: null
  },
  "339": {
    Id: null,
    MarketingProduct: [],
    GlobalCode: null,
    Description: null,
    FullDescription: null,
    Notes: null,
    Link: null,
    SortOrder: null,
    Icon: null,
    PDF: {
      Name: null,
      AbsoluteUrl: null
    },
    StartDate: null,
    EndDate: null,
    Priority: null,
    ListImage: null,
    ArchiveDate: null,
    Modifiers: [],
    Parameters: [],
    Regions: [],
    Type: {
      Value: null,
      Contents: {
        Tariff: {
          Id: null
        },
        Service: {
          Id: null
        },
        Action: {
          Id: null
        },
        RoamingScale: {
          Id: null
        },
        Device: {
          Id: null,
          Downloads: [],
          Inners: [],
          FreezeDate: null,
          FullUserGuide: {
            Name: null,
            AbsoluteUrl: null
          },
          QuickStartGuide: {
            Name: null,
            AbsoluteUrl: null
          }
        },
        FixConnectAction: {
          Id: null,
          MarketingOffers: [],
          PromoPeriod: null,
          AfterPromo: null
        },
        TvPackage: {
          Id: null
        },
        FixConnectTariff: {
          Id: null,
          TitleForSite: null
        },
        PhoneTariff: {
          Id: null,
          RostelecomLink: null
        },
        InternetTariff: {
          Id: null
        }
      }
    },
    FixConnectAction: null,
    Advantages: []
  },
  "340": {
    Id: null,
    Title: null,
    Alias: null
  },
  "342": {
    Id: null,
    Title: null,
    Alias: null
  },
  "343": {
    Id: null
  },
  "346": {
    Id: null,
    Title: null,
    Alias: null
  },
  "347": {
    Id: null,
    Title: null,
    Alias: null
  },
  "350": {
    Id: null,
    Title: null,
    Alias: null,
    AllowZone: null,
    AllowDirection: null
  },
  "351": {
    Id: null,
    Title: null,
    Alias: null,
    Type: null
  },
  "352": {
    Id: null,
    Title: null,
    Alias: null
  },
  "354": {
    Id: null,
    Group: [],
    Title: null,
    Parent: [],
    BaseParameter: [],
    Zone: [],
    Direction: [],
    BaseParameterModifiers: [],
    Modifiers: [],
    Unit: [],
    SortOrder: null,
    NumValue: null,
    Value: null,
    Description: null,
    Image: null,
    ProductGroup: [],
    Choice: []
  },
  "355": {
    Id: null,
    Alias: null,
    Title: null,
    Display: null,
    QuotaUnit: null,
    QuotaPeriod: null
  },
  "360": {
    Id: null,
    Title: null,
    Alias: null
  },
  "378": {
    Id: null,
    SortOrder: null,
    Title: null,
    Alias: null,
    ImageSvg: {
      Name: null,
      AbsoluteUrl: null
    },
    Type: null
  },
  "383": {
    Id: null,
    Title: null,
    Alias: null,
    Description: null,
    OldSiteId: null,
    OldCorpSiteId: null,
    ListImage: null,
    DetailsImage: null,
    ArchiveDate: null,
    Modifiers: [],
    SortOrder: null,
    Priority: null,
    Advantages: [],
    Type: {
      Value: null,
      Contents: {
        MarketingTariff: {
          Id: null
        },
        MarketingService: {
          Id: null
        },
        MarketingAction: {
          Id: null
        },
        MarketingRoamingScale: {
          Id: null
        },
        MarketingDevice: {
          Id: null,
          DeviceType: [],
          Segments: [],
          CommunicationType: []
        },
        MarketingFixConnectAction: {
          Id: null,
          Segment: [],
          MarketingAction: [],
          StartDate: null,
          EndDate: null,
          PromoPeriod: null,
          AfterPromo: null
        },
        MarketingTvPackage: {
          Id: null,
          Channels: [],
          TitleForSite: null,
          PackageType: null
        },
        MarketingFixConnectTariff: {
          Id: null,
          Segment: [],
          Category: [],
          MarketingDevices: [],
          BonusTVPackages: [],
          MarketingPhoneTariff: [],
          MarketingInternetTariff: [],
          MarketingTvPackage: [],
          TitleForSite: null
        },
        MarketingPhoneTariff: {
          Id: null
        },
        MarketingInternetTariff: {
          Id: null
        }
      }
    },
    FullDescription: null,
    Parameters: [],
    TariffsOnMarketingDevice: null,
    DevicesOnMarketingTariff: [],
    ActionsOnMarketingDevice: null,
    Link: null,
    DetailedDescription: null
  },
  "385": {
    Id: null
  },
  "402": {
    Id: null
  },
  "403": {
    Id: null
  },
  "415": {
    Id: null,
    Title: null,
    Alias: null
  },
  "416": {
    Id: null,
    Title: null,
    Alias: null
  },
  "419": {
    Id: null
  },
  "420": {
    Id: null
  },
  "424": {
    Id: null,
    Group: [],
    BaseParameter: [],
    Zone: [],
    Direction: [],
    BaseParameterModifiers: [],
    Modifiers: [],
    Unit: [],
    Choice: [],
    Title: null,
    SortOrder: null,
    NumValue: null,
    Value: null,
    Description: null
  },
  "434": {
    Id: null
  },
  "435": {
    Id: null
  },
  "441": {
    Id: null,
    ConnectionTypes: [],
    Title: null,
    Alias: null,
    Image: null,
    Order: null,
    ImageSvg: {
      Name: null,
      AbsoluteUrl: null
    },
    TemplateType: null
  },
  "446": {
    Id: null,
    Title: null,
    Text: null,
    Description: null,
    ImageSvg: {
      Name: null,
      AbsoluteUrl: null
    },
    SortOrder: null,
    IsGift: null,
    OldSiteId: null
  },
  "471": {
    Id: null,
    Name: null,
    Code: null,
    UTC: null,
    MSK: null,
    OldSiteId: null
  },
  "472": {
    Id: null,
    City: [],
    HasIpTv: null
  },
  "478": {
    Id: null,
    Name: null,
    Alias: null,
    Segments: null,
    Icon: null,
    Order: null,
    OldSiteId: null
  },
  "479": {
    Id: null,
    Title: null,
    OldSiteId: null
  },
  "480": {
    Id: null,
    Title: null,
    Image: null,
    Message: null,
    OldSiteId: null
  },
  "482": {
    Id: null,
    Title: null,
    Logo150: null,
    Category: [],
    ChannelType: [],
    ShortDescription: null,
    Cities: [],
    Disabled: null,
    IsMtsMsk: null,
    IsRegional: null,
    LcnDvbC: null,
    LcnIpTv: null,
    LcnDvbS: null,
    Format: [],
    Parent: [],
    Children: [],
    Logo40x30: null,
    TimeZone: []
  },
  "488": {
    Id: null,
    Title: null,
    Alias: null,
    OldSiteId: null
  },
  "489": {
    Id: null,
    DeviceType: [],
    Segments: [],
    CommunicationType: []
  },
  "490": {
    Id: null,
    Downloads: [],
    Inners: [],
    FreezeDate: null,
    FullUserGuide: {
      Name: null,
      AbsoluteUrl: null
    },
    QuickStartGuide: {
      Name: null,
      AbsoluteUrl: null
    }
  },
  "491": {
    Id: null,
    Title: null
  },
  "493": {
    Id: null,
    ConnectionType: [],
    Title: null,
    Alias: null,
    Order: null
  },
  "494": {
    Id: null,
    Title: null,
    File: {
      Name: null,
      AbsoluteUrl: null
    }
  },
  "498": {
    Id: null,
    Segment: [],
    MarketingAction: [],
    StartDate: null,
    EndDate: null,
    PromoPeriod: null,
    AfterPromo: null
  },
  "500": {
    Id: null,
    MarketingOffers: [],
    PromoPeriod: null,
    AfterPromo: null
  },
  "502": {
    Id: null,
    Channels: [],
    TitleForSite: null,
    PackageType: null
  },
  "503": {
    Id: null
  },
  "504": {
    Id: null,
    Segment: [],
    Category: [],
    MarketingDevices: [],
    BonusTVPackages: [],
    MarketingPhoneTariff: [],
    MarketingInternetTariff: [],
    MarketingTvPackage: [],
    TitleForSite: null
  },
  "505": {
    Id: null,
    TitleForSite: null
  },
  "506": {
    Id: null
  },
  "507": {
    Id: null,
    RostelecomLink: null
  },
  "509": {
    Id: null
  },
  "510": {
    Id: null
  }
};

/** Описание полей продукта */
export const productEditorSchema = linkJsonRefs<ProductEditorSchema>({
  Content: {
    ContentId: 339,
    ContentPath: "/339",
    ContentName: "Product",
    ContentTitle: "Продукты",
    ContentDescription: "",
    ObjectShape: null,
    Fields: {
      MarketingProduct: {
        IsBackward: false,
        Content: {
          ContentId: 383,
          ContentPath: "/339:1542/383",
          ContentName: "MarketingProduct",
          ContentTitle: "Маркетинговые продукты",
          ContentDescription: "",
          ObjectShape: null,
          Fields: {
            Title: {
              FieldId: 1534,
              FieldName: "Title",
              FieldTitle: "Название",
              FieldDescription: "",
              FieldOrder: 1,
              IsRequired: false,
              FieldType: "String"
            },
            Alias: {
              FieldId: 1753,
              FieldName: "Alias",
              FieldTitle: "Псевдоним",
              FieldDescription: "",
              FieldOrder: 2,
              IsRequired: false,
              FieldType: "String"
            },
            Description: {
              FieldId: 1558,
              FieldName: "Description",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 4,
              IsRequired: false,
              FieldType: "Textbox"
            },
            OldSiteId: {
              FieldId: 1645,
              FieldName: "OldSiteId",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 14,
              IsRequired: false,
              FieldType: "Numeric"
            },
            OldCorpSiteId: {
              FieldId: 1779,
              FieldName: "OldCorpSiteId",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 15,
              IsRequired: false,
              FieldType: "Numeric"
            },
            ListImage: {
              FieldId: 2030,
              FieldName: "ListImage",
              FieldTitle: "Изображение в списке",
              FieldDescription: "Изображение в общем списке",
              FieldOrder: 17,
              IsRequired: false,
              FieldType: "Image"
            },
            DetailsImage: {
              FieldId: 2031,
              FieldName: "DetailsImage",
              FieldTitle: "Изображение",
              FieldDescription: "Изображение в описании на странице",
              FieldOrder: 18,
              IsRequired: false,
              FieldType: "Image"
            },
            ArchiveDate: {
              FieldId: 2124,
              FieldName: "ArchiveDate",
              FieldTitle: "Дата закрытия продукта (Архив)",
              FieldDescription: "",
              FieldOrder: 23,
              IsRequired: false,
              FieldType: "Date"
            },
            Modifiers: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/ProductModifer"
              },
              FieldId: 1653,
              FieldName: "Modifiers",
              FieldTitle: "Модификаторы",
              FieldDescription: "",
              FieldOrder: 12,
              IsRequired: false,
              FieldType: "M2MRelation"
            },
            SortOrder: {
              FieldId: 1752,
              FieldName: "SortOrder",
              FieldTitle: "Порядок",
              FieldDescription: "",
              FieldOrder: 7,
              IsRequired: false,
              FieldType: "Numeric"
            },
            Priority: {
              FieldId: 2032,
              FieldName: "Priority",
              FieldTitle: "Приоритет (популярность)",
              FieldDescription: "Сортировка по возрастанию значения приоритета",
              FieldOrder: 19,
              IsRequired: false,
              FieldType: "Numeric"
            },
            Advantages: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/Advantage"
              },
              FieldId: 2028,
              FieldName: "Advantages",
              FieldTitle: "Преимущества",
              FieldDescription: "",
              FieldOrder: 16,
              IsRequired: false,
              FieldType: "M2MRelation"
            },
            Type: {
              Contents: {
                MarketingTariff: {
                  ContentId: 385,
                  ContentPath: "/339:1542/383:1540/385",
                  ContentName: "MarketingTariff",
                  ContentTitle: "Маркетинговые тарифы",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {}
                },
                MarketingService: {
                  ContentId: 402,
                  ContentPath: "/339:1542/383:1540/402",
                  ContentName: "MarketingService",
                  ContentTitle: "Маркетинговые услуги",
                  ContentDescription: 'Универсальная "опция". Голосовая, дата и что еще появится.',
                  ObjectShape: null,
                  Fields: {}
                },
                MarketingAction: {
                  ContentId: 420,
                  ContentPath: "/339:1542/383:1540/420",
                  ContentName: "MarketingAction",
                  ContentTitle: "Маркетинговые акции",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {}
                },
                MarketingRoamingScale: {
                  ContentId: 435,
                  ContentPath: "/339:1542/383:1540/435",
                  ContentName: "MarketingRoamingScale",
                  ContentTitle: "Маркетинговые роуминговые сетки",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {}
                },
                MarketingDevice: {
                  ContentId: 489,
                  ContentPath: "/339:1542/383:1540/489",
                  ContentName: "MarketingDevice",
                  ContentTitle: "Маркетинговое оборудование",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {
                    DeviceType: {
                      IsBackward: false,
                      Content: {
                        ContentId: 493,
                        ContentPath: "/339:1542/383:1540/489:2403/493",
                        ContentName: "EquipmentType",
                        ContentTitle: "Типы оборудования",
                        ContentDescription: "",
                        ObjectShape: null,
                        Fields: {
                          ConnectionType: {
                            IsBackward: false,
                            Content: {
                              $ref: "#/Definitions/FixedType"
                            },
                            FieldId: 2402,
                            FieldName: "ConnectionType",
                            FieldTitle: "Тип связи",
                            FieldDescription: "",
                            FieldOrder: 5,
                            IsRequired: false,
                            FieldType: "O2MRelation"
                          },
                          Title: {
                            FieldId: 2399,
                            FieldName: "Title",
                            FieldTitle: "",
                            FieldDescription: "",
                            FieldOrder: 1,
                            IsRequired: false,
                            FieldType: "String"
                          },
                          Alias: {
                            FieldId: 2400,
                            FieldName: "Alias",
                            FieldTitle: "",
                            FieldDescription: "",
                            FieldOrder: 2,
                            IsRequired: false,
                            FieldType: "String"
                          },
                          Order: {
                            FieldId: 2648,
                            FieldName: "Order",
                            FieldTitle: "Порядок",
                            FieldDescription: "",
                            FieldOrder: 3,
                            IsRequired: false,
                            FieldType: "Numeric"
                          }
                        }
                      },
                      FieldId: 2403,
                      FieldName: "DeviceType",
                      FieldTitle: "Тип оборудования",
                      FieldDescription: "",
                      FieldOrder: 2,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    },
                    Segments: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/Segment"
                      },
                      FieldId: 2404,
                      FieldName: "Segments",
                      FieldTitle: "Сегменты",
                      FieldDescription: "",
                      FieldOrder: 3,
                      IsRequired: false,
                      FieldType: "M2MRelation"
                    },
                    CommunicationType: {
                      IsBackward: false,
                      Content: {
                        ContentId: 415,
                        ContentPath: "/339:1542/383:1540/489:2509/415",
                        ContentName: "CommunicationType",
                        ContentTitle: "Виды связи",
                        ContentDescription: "",
                        ObjectShape: null,
                        Fields: {
                          Title: {
                            FieldId: 1789,
                            FieldName: "Title",
                            FieldTitle: "",
                            FieldDescription: "",
                            FieldOrder: 1,
                            IsRequired: false,
                            FieldType: "String"
                          },
                          Alias: {
                            FieldId: 1791,
                            FieldName: "Alias",
                            FieldTitle: "Псевдоним",
                            FieldDescription: "",
                            FieldOrder: 2,
                            IsRequired: false,
                            FieldType: "String"
                          }
                        }
                      },
                      FieldId: 2509,
                      FieldName: "CommunicationType",
                      FieldTitle: "Вид связи",
                      FieldDescription: "",
                      FieldOrder: 4,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    }
                  }
                },
                MarketingFixConnectAction: {
                  ContentId: 498,
                  ContentPath: "/339:1542/383:1540/498",
                  ContentName: "MarketingFixConnectAction",
                  ContentTitle: "Маркетинговые акции фиксированной связи",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {
                    Segment: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/Segment"
                      },
                      FieldId: 2458,
                      FieldName: "Segment",
                      FieldTitle: "Сегмент",
                      FieldDescription: "",
                      FieldOrder: 2,
                      IsRequired: false,
                      FieldType: "M2MRelation"
                    },
                    MarketingAction: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/MarketingProduct3"
                      },
                      FieldId: 2564,
                      FieldName: "MarketingAction",
                      FieldTitle: "Акция в Каталоге акций",
                      FieldDescription: "",
                      FieldOrder: 7,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    },
                    StartDate: {
                      FieldId: 2459,
                      FieldName: "StartDate",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 3,
                      IsRequired: false,
                      FieldType: "Date"
                    },
                    EndDate: {
                      FieldId: 2460,
                      FieldName: "EndDate",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 4,
                      IsRequired: false,
                      FieldType: "Date"
                    },
                    PromoPeriod: {
                      FieldId: 2461,
                      FieldName: "PromoPeriod",
                      FieldTitle: "",
                      FieldDescription: "Описание промо-периода.",
                      FieldOrder: 5,
                      IsRequired: false,
                      FieldType: "String"
                    },
                    AfterPromo: {
                      FieldId: 2462,
                      FieldName: "AfterPromo",
                      FieldTitle: "",
                      FieldDescription: "Описание момента начала действия обычной цены.",
                      FieldOrder: 6,
                      IsRequired: false,
                      FieldType: "String"
                    }
                  }
                },
                MarketingTvPackage: {
                  ContentId: 502,
                  ContentPath: "/339:1542/383:1540/502",
                  ContentName: "MarketingTvPackage",
                  ContentTitle: "Маркетинговые ТВ-пакеты",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {
                    Channels: {
                      IsBackward: false,
                      Content: {
                        ContentId: 482,
                        ContentPath: "/339:1542/383:1540/502:2497/482",
                        ContentName: "TvChannel",
                        ContentTitle: "ТВ-каналы",
                        ContentDescription: "",
                        ObjectShape: null,
                        Fields: {
                          Title: {
                            FieldId: 2274,
                            FieldName: "Title",
                            FieldTitle: "Название телеканала",
                            FieldDescription: "title",
                            FieldOrder: 1,
                            IsRequired: false,
                            FieldType: "String"
                          },
                          ShortDescription: {
                            FieldId: 2281,
                            FieldName: "ShortDescription",
                            FieldTitle: "Короткое описание",
                            FieldDescription: "short_descr",
                            FieldOrder: 7,
                            IsRequired: false,
                            FieldType: "Textbox"
                          },
                          Logo150: {
                            FieldId: 2306,
                            FieldName: "Logo150",
                            FieldTitle: "Лого 150x150",
                            FieldDescription: "logo150",
                            FieldOrder: 32,
                            IsRequired: false,
                            FieldType: "Image"
                          },
                          IsRegional: {
                            FieldId: 2298,
                            FieldName: "IsRegional",
                            FieldTitle: "Регионал. канал",
                            FieldDescription: "regional_tv",
                            FieldOrder: 24,
                            IsRequired: false,
                            FieldType: "Boolean"
                          },
                          Parent: {
                            IsBackward: false,
                            Content: {
                              ContentId: 482,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2287/482",
                              ContentName: "TvChannel",
                              ContentTitle: "ТВ-каналы",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Logo150: {
                                  FieldId: 2306,
                                  FieldName: "Logo150",
                                  FieldTitle: "Лого 150x150",
                                  FieldDescription: "logo150",
                                  FieldOrder: 32,
                                  IsRequired: false,
                                  FieldType: "Image"
                                }
                              }
                            },
                            FieldId: 2287,
                            FieldName: "Parent",
                            FieldTitle: "Родительский канал",
                            FieldDescription: "ch_parent",
                            FieldOrder: 13,
                            IsRequired: false,
                            FieldType: "O2MRelation"
                          },
                          Cities: {
                            IsBackward: false,
                            Content: {
                              ContentId: 472,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2286/472",
                              ContentName: "NetworkCity",
                              ContentTitle: "Города сети",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                City: {
                                  IsBackward: false,
                                  Content: {
                                    $ref: "#/Definitions/Region"
                                  },
                                  FieldId: 2211,
                                  FieldName: "City",
                                  FieldTitle: "Город",
                                  FieldDescription: "",
                                  FieldOrder: 1,
                                  IsRequired: false,
                                  FieldType: "O2MRelation"
                                },
                                HasIpTv: {
                                  FieldId: 2218,
                                  FieldName: "HasIpTv",
                                  FieldTitle: "IPTV",
                                  FieldDescription: "Города где есть IPTV",
                                  FieldOrder: 9,
                                  IsRequired: false,
                                  FieldType: "Boolean"
                                }
                              }
                            },
                            FieldId: 2286,
                            FieldName: "Cities",
                            FieldTitle: "Города вещания",
                            FieldDescription: "cities",
                            FieldOrder: 12,
                            IsRequired: false,
                            FieldType: "M2MRelation"
                          },
                          ChannelType: {
                            IsBackward: false,
                            Content: {
                              ContentId: 479,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2285/479",
                              ContentName: "ChannelType",
                              ContentTitle: "Типы каналов",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Title: {
                                  FieldId: 2258,
                                  FieldName: "Title",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 1,
                                  IsRequired: false,
                                  FieldType: "String"
                                }
                              }
                            },
                            FieldId: 2285,
                            FieldName: "ChannelType",
                            FieldTitle: "Тип канала",
                            FieldDescription: "ch_type",
                            FieldOrder: 11,
                            IsRequired: false,
                            FieldType: "O2MRelation"
                          },
                          Category: {
                            IsBackward: false,
                            Content: {
                              $ref: "#/Definitions/ChannelCategory"
                            },
                            FieldId: 2283,
                            FieldName: "Category",
                            FieldTitle: "Основная категория телеканала",
                            FieldDescription: "ch_category",
                            FieldOrder: 9,
                            IsRequired: false,
                            FieldType: "O2MRelation"
                          },
                          IsMtsMsk: {
                            FieldId: 2297,
                            FieldName: "IsMtsMsk",
                            FieldTitle: "МТС Москва",
                            FieldDescription: "test_inMSK_mgts_XML",
                            FieldOrder: 23,
                            IsRequired: false,
                            FieldType: "Boolean"
                          },
                          LcnDvbC: {
                            FieldId: 2312,
                            FieldName: "LcnDvbC",
                            FieldTitle: "LCN DVB-C",
                            FieldDescription: "lcn_dvbc",
                            FieldOrder: 36,
                            IsRequired: false,
                            FieldType: "Numeric"
                          },
                          LcnIpTv: {
                            FieldId: 2314,
                            FieldName: "LcnIpTv",
                            FieldTitle: "LCN IPTV",
                            FieldDescription: "lcn_iptv",
                            FieldOrder: 37,
                            IsRequired: false,
                            FieldType: "Numeric"
                          },
                          LcnDvbS: {
                            FieldId: 2313,
                            FieldName: "LcnDvbS",
                            FieldTitle: "LCN DVB-S",
                            FieldDescription: "lcn_dvbs",
                            FieldOrder: 38,
                            IsRequired: false,
                            FieldType: "Numeric"
                          },
                          Disabled: {
                            FieldId: 2289,
                            FieldName: "Disabled",
                            FieldTitle: "Приостановлено вещание",
                            FieldDescription: "offair",
                            FieldOrder: 17,
                            IsRequired: false,
                            FieldType: "Boolean"
                          },
                          Children: {
                            IsBackward: false,
                            Content: {
                              ContentId: 482,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2619/482",
                              ContentName: "TvChannel",
                              ContentTitle: "ТВ-каналы",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Title: {
                                  FieldId: 2274,
                                  FieldName: "Title",
                                  FieldTitle: "Название телеканала",
                                  FieldDescription: "title",
                                  FieldOrder: 1,
                                  IsRequired: false,
                                  FieldType: "String"
                                },
                                Category: {
                                  IsBackward: false,
                                  Content: {
                                    $ref: "#/Definitions/ChannelCategory"
                                  },
                                  FieldId: 2283,
                                  FieldName: "Category",
                                  FieldTitle: "Основная категория телеканала",
                                  FieldDescription: "ch_category",
                                  FieldOrder: 9,
                                  IsRequired: false,
                                  FieldType: "O2MRelation"
                                },
                                ChannelType: {
                                  IsBackward: false,
                                  Content: {
                                    ContentId: 479,
                                    ContentPath:
                                      "/339:1542/383:1540/502:2497/482:2619/482:2285/479",
                                    ContentName: "ChannelType",
                                    ContentTitle: "Типы каналов",
                                    ContentDescription: "",
                                    ObjectShape: null,
                                    Fields: {
                                      Title: {
                                        FieldId: 2258,
                                        FieldName: "Title",
                                        FieldTitle: "",
                                        FieldDescription: "",
                                        FieldOrder: 1,
                                        IsRequired: false,
                                        FieldType: "String"
                                      },
                                      OldSiteId: {
                                        FieldId: 2261,
                                        FieldName: "OldSiteId",
                                        FieldTitle: "",
                                        FieldDescription: "",
                                        FieldOrder: 2,
                                        IsRequired: false,
                                        FieldType: "Numeric"
                                      }
                                    }
                                  },
                                  FieldId: 2285,
                                  FieldName: "ChannelType",
                                  FieldTitle: "Тип канала",
                                  FieldDescription: "ch_type",
                                  FieldOrder: 11,
                                  IsRequired: false,
                                  FieldType: "O2MRelation"
                                },
                                ShortDescription: {
                                  FieldId: 2281,
                                  FieldName: "ShortDescription",
                                  FieldTitle: "Короткое описание",
                                  FieldDescription: "short_descr",
                                  FieldOrder: 7,
                                  IsRequired: false,
                                  FieldType: "Textbox"
                                },
                                Cities: {
                                  IsBackward: false,
                                  Content: {
                                    ContentId: 472,
                                    ContentPath:
                                      "/339:1542/383:1540/502:2497/482:2619/482:2286/472",
                                    ContentName: "NetworkCity",
                                    ContentTitle: "Города сети",
                                    ContentDescription: "",
                                    ObjectShape: null,
                                    Fields: {
                                      City: {
                                        IsBackward: false,
                                        Content: {
                                          $ref: "#/Definitions/Region"
                                        },
                                        FieldId: 2211,
                                        FieldName: "City",
                                        FieldTitle: "Город",
                                        FieldDescription: "",
                                        FieldOrder: 1,
                                        IsRequired: false,
                                        FieldType: "O2MRelation"
                                      }
                                    }
                                  },
                                  FieldId: 2286,
                                  FieldName: "Cities",
                                  FieldTitle: "Города вещания",
                                  FieldDescription: "cities",
                                  FieldOrder: 12,
                                  IsRequired: false,
                                  FieldType: "M2MRelation"
                                },
                                Disabled: {
                                  FieldId: 2289,
                                  FieldName: "Disabled",
                                  FieldTitle: "Приостановлено вещание",
                                  FieldDescription: "offair",
                                  FieldOrder: 17,
                                  IsRequired: false,
                                  FieldType: "Boolean"
                                },
                                IsMtsMsk: {
                                  FieldId: 2297,
                                  FieldName: "IsMtsMsk",
                                  FieldTitle: "МТС Москва",
                                  FieldDescription: "test_inMSK_mgts_XML",
                                  FieldOrder: 23,
                                  IsRequired: false,
                                  FieldType: "Boolean"
                                },
                                IsRegional: {
                                  FieldId: 2298,
                                  FieldName: "IsRegional",
                                  FieldTitle: "Регионал. канал",
                                  FieldDescription: "regional_tv",
                                  FieldOrder: 24,
                                  IsRequired: false,
                                  FieldType: "Boolean"
                                },
                                Logo150: {
                                  FieldId: 2306,
                                  FieldName: "Logo150",
                                  FieldTitle: "Лого 150x150",
                                  FieldDescription: "logo150",
                                  FieldOrder: 32,
                                  IsRequired: false,
                                  FieldType: "Image"
                                },
                                LcnDvbC: {
                                  FieldId: 2312,
                                  FieldName: "LcnDvbC",
                                  FieldTitle: "LCN DVB-C",
                                  FieldDescription: "lcn_dvbc",
                                  FieldOrder: 36,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                },
                                LcnIpTv: {
                                  FieldId: 2314,
                                  FieldName: "LcnIpTv",
                                  FieldTitle: "LCN IPTV",
                                  FieldDescription: "lcn_iptv",
                                  FieldOrder: 37,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                },
                                LcnDvbS: {
                                  FieldId: 2313,
                                  FieldName: "LcnDvbS",
                                  FieldTitle: "LCN DVB-S",
                                  FieldDescription: "lcn_dvbs",
                                  FieldOrder: 38,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                },
                                Format: {
                                  IsBackward: false,
                                  Content: {
                                    $ref: "#/Definitions/ChannelFormat"
                                  },
                                  FieldId: 2524,
                                  FieldName: "Format",
                                  FieldTitle: "Формат",
                                  FieldDescription: "",
                                  FieldOrder: 16,
                                  IsRequired: false,
                                  FieldType: "O2MRelation"
                                }
                              }
                            },
                            FieldId: 2619,
                            FieldName: "Children",
                            FieldTitle: "Дочерние каналы",
                            FieldDescription: "",
                            FieldOrder: 14,
                            IsRequired: false,
                            FieldType: "M2ORelation"
                          },
                          Format: {
                            IsBackward: false,
                            Content: {
                              $ref: "#/Definitions/ChannelFormat"
                            },
                            FieldId: 2524,
                            FieldName: "Format",
                            FieldTitle: "Формат",
                            FieldDescription: "",
                            FieldOrder: 16,
                            IsRequired: false,
                            FieldType: "O2MRelation"
                          },
                          Logo40x30: {
                            FieldId: 2303,
                            FieldName: "Logo40x30",
                            FieldTitle: "Лого 40х30",
                            FieldDescription: "logo40x30",
                            FieldOrder: 29,
                            IsRequired: false,
                            FieldType: "Image"
                          },
                          TimeZone: {
                            IsBackward: false,
                            Content: {
                              ContentId: 471,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2288/471",
                              ContentName: "TimeZone",
                              ContentTitle: "Часовые зоны",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Name: {
                                  FieldId: 2203,
                                  FieldName: "Name",
                                  FieldTitle: "Название часовой зоны",
                                  FieldDescription: "",
                                  FieldOrder: 1,
                                  IsRequired: false,
                                  FieldType: "String"
                                },
                                Code: {
                                  FieldId: 2204,
                                  FieldName: "Code",
                                  FieldTitle: "Код зоны",
                                  FieldDescription: "",
                                  FieldOrder: 2,
                                  IsRequired: false,
                                  FieldType: "String"
                                },
                                UTC: {
                                  FieldId: 2205,
                                  FieldName: "UTC",
                                  FieldTitle: "Значение по UTC",
                                  FieldDescription: "",
                                  FieldOrder: 3,
                                  IsRequired: false,
                                  FieldType: "String"
                                },
                                MSK: {
                                  FieldId: 2206,
                                  FieldName: "MSK",
                                  FieldTitle: "Значение от Московского времени",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  IsRequired: false,
                                  FieldType: "String"
                                },
                                OldSiteId: {
                                  FieldId: 2207,
                                  FieldName: "OldSiteId",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 5,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                }
                              }
                            },
                            FieldId: 2288,
                            FieldName: "TimeZone",
                            FieldTitle: "Часовая зона (UTC)",
                            FieldDescription: "utc_tz\ndesc",
                            FieldOrder: 15,
                            IsRequired: false,
                            FieldType: "O2MRelation"
                          }
                        }
                      },
                      FieldId: 2497,
                      FieldName: "Channels",
                      FieldTitle: "Каналы",
                      FieldDescription: "",
                      FieldOrder: 4,
                      IsRequired: false,
                      FieldType: "M2MRelation"
                    },
                    TitleForSite: {
                      FieldId: 2482,
                      FieldName: "TitleForSite",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 2,
                      IsRequired: false,
                      FieldType: "String"
                    },
                    PackageType: {
                      Items: [
                        {
                          Value: "Base",
                          Alias: "Базовый пакет",
                          IsDefault: false,
                          Invalid: false
                        },
                        {
                          Value: "Additional",
                          Alias: "Дополнительный пакет",
                          IsDefault: false,
                          Invalid: false
                        }
                      ],
                      FieldId: 2483,
                      FieldName: "PackageType",
                      FieldTitle: "Тип пакета",
                      FieldDescription: "",
                      FieldOrder: 3,
                      IsRequired: false,
                      FieldType: "StringEnum"
                    }
                  }
                },
                MarketingFixConnectTariff: {
                  ContentId: 504,
                  ContentPath: "/339:1542/383:1540/504",
                  ContentName: "MarketingFixConnectTariff",
                  ContentTitle: "Маркетинговые тарифы фиксированной связи",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {
                    Segment: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/Segment"
                      },
                      FieldId: 2492,
                      FieldName: "Segment",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 2,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    },
                    Category: {
                      IsBackward: false,
                      Content: {
                        ContentId: 441,
                        ContentPath: "/339:1542/383:1540/504:2494/441",
                        ContentName: "TariffCategory",
                        ContentTitle: "Категории тарифов",
                        ContentDescription: "",
                        ObjectShape: null,
                        Fields: {
                          ConnectionTypes: {
                            IsBackward: false,
                            Content: {
                              $ref: "#/Definitions/FixedType"
                            },
                            FieldId: 2449,
                            FieldName: "ConnectionTypes",
                            FieldTitle: "Типы связи",
                            FieldDescription: "",
                            FieldOrder: 10,
                            IsRequired: false,
                            FieldType: "M2MRelation"
                          },
                          Title: {
                            FieldId: 1989,
                            FieldName: "Title",
                            FieldTitle: "Название",
                            FieldDescription: "",
                            FieldOrder: 1,
                            IsRequired: false,
                            FieldType: "String"
                          },
                          Alias: {
                            FieldId: 1990,
                            FieldName: "Alias",
                            FieldTitle: "Алиас",
                            FieldDescription: "",
                            FieldOrder: 2,
                            IsRequired: false,
                            FieldType: "String"
                          },
                          Image: {
                            FieldId: 1991,
                            FieldName: "Image",
                            FieldTitle: "Картинка",
                            FieldDescription: "",
                            FieldOrder: 3,
                            IsRequired: false,
                            FieldType: "Image"
                          },
                          Order: {
                            FieldId: 2001,
                            FieldName: "Order",
                            FieldTitle: "Порядок",
                            FieldDescription: "",
                            FieldOrder: 7,
                            IsRequired: false,
                            FieldType: "Numeric"
                          },
                          ImageSvg: {
                            FieldId: 2020,
                            FieldName: "ImageSvg",
                            FieldTitle: "Векторное изображение",
                            FieldDescription: "",
                            FieldOrder: 9,
                            IsRequired: false,
                            FieldType: "File"
                          },
                          TemplateType: {
                            Items: [
                              {
                                Value: "Tv",
                                Alias: "ТВ",
                                IsDefault: false,
                                Invalid: false
                              },
                              {
                                Value: "Phone",
                                Alias: "Телефон",
                                IsDefault: false,
                                Invalid: false
                              }
                            ],
                            FieldId: 2450,
                            FieldName: "TemplateType",
                            FieldTitle: "Тип шаблона страницы",
                            FieldDescription: "",
                            FieldOrder: 11,
                            IsRequired: false,
                            FieldType: "StringEnum"
                          }
                        }
                      },
                      FieldId: 2494,
                      FieldName: "Category",
                      FieldTitle: "Тип предложения (Категория тарифа)",
                      FieldDescription: "",
                      FieldOrder: 3,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    },
                    MarketingDevices: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/MarketingProduct"
                      },
                      FieldId: 2519,
                      FieldName: "MarketingDevices",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 8,
                      IsRequired: false,
                      FieldType: "M2MRelation"
                    },
                    BonusTVPackages: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/MarketingProduct"
                      },
                      FieldId: 2518,
                      FieldName: "BonusTVPackages",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 7,
                      IsRequired: false,
                      FieldType: "M2MRelation"
                    },
                    MarketingPhoneTariff: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/MarketingProduct1"
                      },
                      FieldId: 2517,
                      FieldName: "MarketingPhoneTariff",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 6,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    },
                    MarketingInternetTariff: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/MarketingProduct1"
                      },
                      FieldId: 2516,
                      FieldName: "MarketingInternetTariff",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 5,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    },
                    MarketingTvPackage: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/MarketingProduct2"
                      },
                      FieldId: 2493,
                      FieldName: "MarketingTvPackage",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 4,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    },
                    TitleForSite: {
                      FieldId: 2491,
                      FieldName: "TitleForSite",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 1,
                      IsRequired: false,
                      FieldType: "String"
                    }
                  }
                },
                MarketingPhoneTariff: {
                  ContentId: 506,
                  ContentPath: "/339:1542/383:1540/506",
                  ContentName: "MarketingPhoneTariff",
                  ContentTitle: "Маркетинговые тарифы телефонии",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {}
                },
                MarketingInternetTariff: {
                  ContentId: 509,
                  ContentPath: "/339:1542/383:1540/509",
                  ContentName: "MarketingInternetTariff",
                  ContentTitle: "Маркетинговые тарифы интернет",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {}
                }
              },
              FieldId: 1540,
              FieldName: "Type",
              FieldTitle: "Тип",
              FieldDescription: "",
              FieldOrder: 11,
              IsRequired: false,
              FieldType: "Classifier"
            },
            FullDescription: {
              FieldId: 1740,
              FieldName: "FullDescription",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 6,
              IsRequired: false,
              FieldType: "VisualEdit"
            },
            Parameters: {
              IsBackward: false,
              Content: {
                ContentId: 424,
                ContentPath: "/339:1542/383:1869/424",
                ContentName: "MarketingProductParameter",
                ContentTitle: "Параметры маркетинговых продуктов",
                ContentDescription: "",
                ObjectShape: null,
                Fields: {
                  Group: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/ProductParameterGroup"
                    },
                    FieldId: 1852,
                    FieldName: "Group",
                    FieldTitle: "Группа параметров",
                    FieldDescription: "",
                    FieldOrder: 3,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  BaseParameter: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/BaseParameter"
                    },
                    FieldId: 1854,
                    FieldName: "BaseParameter",
                    FieldTitle: "Базовый параметр",
                    FieldDescription: "",
                    FieldOrder: 5,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  Zone: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/TariffZone"
                    },
                    FieldId: 1855,
                    FieldName: "Zone",
                    FieldTitle: "Зона действия базового параметра",
                    FieldDescription: "",
                    FieldOrder: 6,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  Direction: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/Direction"
                    },
                    FieldId: 1856,
                    FieldName: "Direction",
                    FieldTitle: "Направление действия базового параметра",
                    FieldDescription: "",
                    FieldOrder: 7,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  BaseParameterModifiers: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/BaseParameterModifier"
                    },
                    FieldId: 1857,
                    FieldName: "BaseParameterModifiers",
                    FieldTitle: "Модификаторы базового параметра",
                    FieldDescription: "",
                    FieldOrder: 8,
                    IsRequired: false,
                    FieldType: "M2MRelation"
                  },
                  Modifiers: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/ParameterModifier"
                    },
                    FieldId: 1858,
                    FieldName: "Modifiers",
                    FieldTitle: "Модификаторы",
                    FieldDescription: "",
                    FieldOrder: 9,
                    IsRequired: false,
                    FieldType: "M2MRelation"
                  },
                  Unit: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/Unit"
                    },
                    FieldId: 1862,
                    FieldName: "Unit",
                    FieldTitle: "Единица измерения",
                    FieldDescription: "",
                    FieldOrder: 13,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  Choice: {
                    IsBackward: false,
                    Content: {
                      $ref: "#/Definitions/ParameterChoice"
                    },
                    FieldId: 2685,
                    FieldName: "Choice",
                    FieldTitle: "Выбор",
                    FieldDescription: "",
                    FieldOrder: 15,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  Title: {
                    FieldId: 1849,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "String"
                  },
                  SortOrder: {
                    FieldId: 1859,
                    FieldName: "SortOrder",
                    FieldTitle: "Порядок",
                    FieldDescription: "",
                    FieldOrder: 10,
                    IsRequired: false,
                    FieldType: "Numeric"
                  },
                  NumValue: {
                    FieldId: 1860,
                    FieldName: "NumValue",
                    FieldTitle: "Числовое значение",
                    FieldDescription: "",
                    FieldOrder: 11,
                    IsRequired: false,
                    FieldType: "Numeric"
                  },
                  Value: {
                    FieldId: 1861,
                    FieldName: "Value",
                    FieldTitle: "Текстовое значение",
                    FieldDescription: "",
                    FieldOrder: 12,
                    IsRequired: false,
                    FieldType: "VisualEdit"
                  },
                  Description: {
                    FieldId: 1863,
                    FieldName: "Description",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 14,
                    IsRequired: false,
                    FieldType: "VisualEdit"
                  }
                }
              },
              FieldId: 1869,
              FieldName: "Parameters",
              FieldTitle: "Параметры маркетингового продукта",
              FieldDescription: "",
              FieldOrder: 9,
              IsRequired: false,
              FieldType: "M2ORelation"
            },
            TariffsOnMarketingDevice: {
              IsBackward: true,
              Content: {
                ContentId: 511,
                ContentPath: "/339:1542/383:2531/511",
                ContentName: "DeviceOnTariffs",
                ContentTitle: "Оборудование на тарифах",
                ContentDescription: "",
                ObjectShape: null,
                Fields: {
                  Parent: {
                    IsBackward: false,
                    Content: {
                      ContentId: 361,
                      ContentPath: "/339:1542/383:2531/511:2530/361",
                      ContentName: "ProductRelation",
                      ContentTitle: "Матрица связей",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Title: {
                          FieldId: 1416,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            ContentId: 360,
                            ContentPath: "/339:1542/383:2531/511:2530/361:1450/360",
                            ContentName: "LinkModifier",
                            ContentTitle: "Модификаторы связей",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1413,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1414,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              }
                            }
                          },
                          FieldId: 1450,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 4,
                          IsRequired: false,
                          FieldType: "M2MRelation"
                        },
                        Parameters: {
                          IsBackward: false,
                          Content: {
                            ContentId: 362,
                            ContentPath: "/339:1542/383:2531/511:2530/361:1431/362",
                            ContentName: "LinkParameter",
                            ContentTitle: "Параметры связей",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1418,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Group: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 378,
                                  ContentPath: "/339:1542/383:2531/511:2530/361:1431/362:1678/378",
                                  ContentName: "ProductParameterGroup",
                                  ContentTitle: "Группы параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1496,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 2049,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    SortOrder: {
                                      FieldId: 1500,
                                      FieldName: "SortOrder",
                                      FieldTitle: "Порядок",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    },
                                    ImageSvg: {
                                      FieldId: 2029,
                                      FieldName: "ImageSvg",
                                      FieldTitle: "Изображение",
                                      FieldDescription: "",
                                      FieldOrder: 7,
                                      IsRequired: false,
                                      FieldType: "File"
                                    }
                                  }
                                },
                                FieldId: 1678,
                                FieldName: "Group",
                                FieldTitle: "Группа параметров",
                                FieldDescription: "",
                                FieldOrder: 3,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              BaseParameter: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 350,
                                  ContentPath: "/339:1542/383:1869/424:1854/350",
                                  ContentName: "BaseParameter",
                                  ContentTitle: "Базовые параметры продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1358,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1359,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    AllowZone: {
                                      FieldId: 2683,
                                      FieldName: "AllowZone",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "Boolean"
                                    },
                                    AllowDirection: {
                                      FieldId: 2684,
                                      FieldName: "AllowDirection",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 6,
                                      IsRequired: false,
                                      FieldType: "Boolean"
                                    }
                                  }
                                },
                                FieldId: 1420,
                                FieldName: "BaseParameter",
                                FieldTitle: "Базовый параметр",
                                FieldDescription: "",
                                FieldOrder: 7,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Zone: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 346,
                                  ContentPath: "/339:1542/383:1869/424:1855/346",
                                  ContentName: "TariffZone",
                                  ContentTitle: "Тарифные зоны",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1346,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1347,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1421,
                                FieldName: "Zone",
                                FieldTitle: "Зона действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 8,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Direction: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 347,
                                  ContentPath: "/339:1542/383:1869/424:1856/347",
                                  ContentName: "Direction",
                                  ContentTitle: "Направления соединения",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1349,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1350,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1422,
                                FieldName: "Direction",
                                FieldTitle: "Направление действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 9,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              BaseParameterModifiers: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 351,
                                  ContentPath: "/339:1542/383:1869/424:1857/351",
                                  ContentName: "BaseParameterModifier",
                                  ContentTitle: "Модификаторы базовых параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1361,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1362,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Type: {
                                      Items: [
                                        {
                                          Value: "Step",
                                          Alias: "Ступенчатая тарификация",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Package",
                                          Alias: "Пакет",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Zone",
                                          Alias: "Зона",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Direction",
                                          Alias: "Направление",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Refining",
                                          Alias: "Уточнение",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1894,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    }
                                  }
                                },
                                FieldId: 1423,
                                FieldName: "BaseParameterModifiers",
                                FieldTitle: "Модификаторы базового параметра",
                                FieldDescription: "",
                                FieldOrder: 10,
                                IsRequired: false,
                                FieldType: "M2MRelation"
                              },
                              Modifiers: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 352,
                                  ContentPath: "/339:1542/383:1869/424:1858/352",
                                  ContentName: "ParameterModifier",
                                  ContentTitle: "Модификаторы параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1364,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1365,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1424,
                                FieldName: "Modifiers",
                                FieldTitle: "Модификаторы",
                                FieldDescription: "",
                                FieldOrder: 11,
                                IsRequired: false,
                                FieldType: "M2MRelation"
                              },
                              SortOrder: {
                                FieldId: 1425,
                                FieldName: "SortOrder",
                                FieldTitle: "Порядок",
                                FieldDescription: "",
                                FieldOrder: 12,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              NumValue: {
                                FieldId: 1426,
                                FieldName: "NumValue",
                                FieldTitle: "Числовое значение",
                                FieldDescription: "",
                                FieldOrder: 13,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              Value: {
                                FieldId: 1427,
                                FieldName: "Value",
                                FieldTitle: "Текстовое значение",
                                FieldDescription: "",
                                FieldOrder: 14,
                                IsRequired: false,
                                FieldType: "VisualEdit"
                              },
                              Description: {
                                FieldId: 1429,
                                FieldName: "Description",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 16,
                                IsRequired: false,
                                FieldType: "VisualEdit"
                              },
                              Unit: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 355,
                                  ContentPath: "/339:1542/383:1869/424:1862/355",
                                  ContentName: "Unit",
                                  ContentTitle: "Единицы измерения",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Alias: {
                                      FieldId: 2062,
                                      FieldName: "Alias",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Title: {
                                      FieldId: 1384,
                                      FieldName: "Title",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Display: {
                                      FieldId: 1385,
                                      FieldName: "Display",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    QuotaUnit: {
                                      Items: [
                                        {
                                          Value: "mb",
                                          Alias: "Мегабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "gb",
                                          Alias: "Гигабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "kb",
                                          Alias: "Килобайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "tb",
                                          Alias: "Терабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "min",
                                          Alias: "Минута",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "message",
                                          Alias: "Сообщение",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "rub",
                                          Alias: "Рублей",
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
                                          Alias: "Мегабит",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "step",
                                          Alias: "Шаг",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1904,
                                      FieldName: "QuotaUnit",
                                      FieldTitle: "Размерность",
                                      FieldDescription: "",
                                      FieldOrder: 4,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    },
                                    QuotaPeriod: {
                                      Items: [
                                        {
                                          Value: "daily",
                                          Alias: "Сутки",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "weekly",
                                          Alias: "Неделя",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "monthly",
                                          Alias: "Месяц",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "hourly",
                                          Alias: "Час",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "minutely",
                                          Alias: "Минута",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "every_second",
                                          Alias: "Секунда",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "annually",
                                          Alias: "Год",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1905,
                                      FieldName: "QuotaPeriod",
                                      FieldTitle: "Период",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    }
                                  }
                                },
                                FieldId: 1428,
                                FieldName: "Unit",
                                FieldTitle: "Единица измерения",
                                FieldDescription: "",
                                FieldOrder: 15,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              ProductGroup: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 340,
                                  ContentPath: "/339:1542/383:2531/511:2530/361:1431/362:1657/340",
                                  ContentName: "Group",
                                  ContentTitle: "Группы продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {}
                                },
                                FieldId: 1657,
                                FieldName: "ProductGroup",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 19,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Choice: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 488,
                                  ContentPath: "/339:1542/383:1869/424:2685/488",
                                  ContentName: "ParameterChoice",
                                  ContentTitle: "Варианты выбора для параметров",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 2379,
                                      FieldName: "Title",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 2380,
                                      FieldName: "Alias",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    OldSiteId: {
                                      FieldId: 2382,
                                      FieldName: "OldSiteId",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 4,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    }
                                  }
                                },
                                FieldId: 2687,
                                FieldName: "Choice",
                                FieldTitle: "Выбор",
                                FieldDescription: "",
                                FieldOrder: 17,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              }
                            }
                          },
                          FieldId: 1431,
                          FieldName: "Parameters",
                          FieldTitle: "Параметры",
                          FieldDescription: "",
                          FieldOrder: 3,
                          IsRequired: false,
                          FieldType: "M2ORelation"
                        },
                        Type: {
                          Contents: {
                            TariffTransfer: {
                              ContentId: 364,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/364",
                              ContentName: "TariffTransfer",
                              ContentTitle: "Переходы с тарифа на тариф",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {}
                            },
                            MutualGroup: {
                              ContentId: 365,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/365",
                              ContentName: "MutualGroup",
                              ContentTitle: "Группы несовместимости услуг",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {}
                            },
                            ServiceOnTariff: {
                              ContentId: 404,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/404",
                              ContentName: "ServiceOnTariff",
                              ContentTitle: "Услуги на тарифе",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Description: {
                                  FieldId: 2044,
                                  FieldName: "Description",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  IsRequired: false,
                                  FieldType: "Textbox"
                                }
                              }
                            },
                            ServicesUpsale: {
                              ContentId: 406,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/406",
                              ContentName: "ServicesUpsale",
                              ContentTitle: "Матрица предложений услуг Upsale",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Order: {
                                  FieldId: 1700,
                                  FieldName: "Order",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                }
                              }
                            },
                            TariffOptionPackage: {
                              ContentId: 407,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/407",
                              ContentName: "TariffOptionPackage",
                              ContentTitle: "Пакеты опций на тарифах",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                SubTitle: {
                                  FieldId: 1708,
                                  FieldName: "SubTitle",
                                  FieldTitle: "Подзаголовок",
                                  FieldDescription: "",
                                  FieldOrder: 5,
                                  IsRequired: false,
                                  FieldType: "Textbox"
                                },
                                Description: {
                                  FieldId: 1707,
                                  FieldName: "Description",
                                  FieldTitle: "Описание",
                                  FieldDescription: "",
                                  FieldOrder: 6,
                                  IsRequired: false,
                                  FieldType: "VisualEdit"
                                },
                                Alias: {
                                  FieldId: 1709,
                                  FieldName: "Alias",
                                  FieldTitle: "Псевдоним",
                                  FieldDescription: "",
                                  FieldOrder: 7,
                                  IsRequired: false,
                                  FieldType: "String"
                                },
                                Link: {
                                  FieldId: 1727,
                                  FieldName: "Link",
                                  FieldTitle: "Ссылка",
                                  FieldDescription: "",
                                  FieldOrder: 8,
                                  IsRequired: false,
                                  FieldType: "String"
                                }
                              }
                            },
                            ServiceRelation: {
                              ContentId: 413,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/413",
                              ContentName: "ServiceRelation",
                              ContentTitle: "Связи между услугами",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {}
                            },
                            RoamingScaleOnTariff: {
                              ContentId: 438,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/438",
                              ContentName: "RoamingScaleOnTariff",
                              ContentTitle: "Роуминговые сетки для тарифа",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {}
                            },
                            ServiceOnRoamingScale: {
                              ContentId: 444,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/444",
                              ContentName: "ServiceOnRoamingScale",
                              ContentTitle: "Услуги на роуминговой сетке",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {}
                            },
                            CrossSale: {
                              ContentId: 468,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/468",
                              ContentName: "CrossSale",
                              ContentTitle: "Матрица предложений CrossSale",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Order: {
                                  FieldId: 2197,
                                  FieldName: "Order",
                                  FieldTitle: "Порядок",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                }
                              }
                            },
                            MarketingCrossSale: {
                              ContentId: 469,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/469",
                              ContentName: "MarketingCrossSale",
                              ContentTitle: "Матрица маркетинговых предложений CrossSale",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Order: {
                                  FieldId: 2201,
                                  FieldName: "Order",
                                  FieldTitle: "Порядок",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                }
                              }
                            },
                            DeviceOnTariffs: {
                              ContentId: 511,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/511",
                              ContentName: "DeviceOnTariffs",
                              ContentTitle: "Оборудование на тарифах",
                              ContentDescription: "",
                              ObjectShape: null,
                              Fields: {
                                Order: {
                                  FieldId: 2534,
                                  FieldName: "Order",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 5,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                }
                              }
                            },
                            DevicesForFixConnectAction: {
                              ContentId: 512,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/512",
                              ContentName: "DevicesForFixConnectAction",
                              ContentTitle: "Акционное оборудование",
                              ContentDescription: "Оборудование для акций фиксированной связи",
                              ObjectShape: null,
                              Fields: {
                                Order: {
                                  FieldId: 2539,
                                  FieldName: "Order",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 3,
                                  IsRequired: false,
                                  FieldType: "Numeric"
                                }
                              }
                            }
                          },
                          FieldId: 1417,
                          FieldName: "Type",
                          FieldTitle: "Тип",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "Classifier"
                        }
                      }
                    },
                    FieldId: 2530,
                    FieldName: "Parent",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  MarketingDevice: {
                    IsBackward: false,
                    Content: {
                      ContentId: 383,
                      ContentPath: "/339:1542/383:1540/504:2493/383",
                      ContentName: "MarketingProduct",
                      ContentTitle: "Маркетинговые продукты",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Alias: {
                          FieldId: 1753,
                          FieldName: "Alias",
                          FieldTitle: "Псевдоним",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "String"
                        }
                      }
                    },
                    FieldId: 2531,
                    FieldName: "MarketingDevice",
                    FieldTitle: "Маркетинговое устройство",
                    FieldDescription: "",
                    FieldOrder: 2,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  MarketingTariffs: {
                    IsBackward: false,
                    Content: {
                      ContentId: 383,
                      ContentPath: "/339:1542/383:1540/498:2564/383",
                      ContentName: "MarketingProduct",
                      ContentTitle: "Маркетинговые продукты",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Title: {
                          FieldId: 1534,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Alias: {
                          FieldId: 1753,
                          FieldName: "Alias",
                          FieldTitle: "Псевдоним",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "String"
                        }
                      }
                    },
                    FieldId: 2532,
                    FieldName: "MarketingTariffs",
                    FieldTitle: "Маркетинговые тарифы",
                    FieldDescription: "",
                    FieldOrder: 3,
                    IsRequired: false,
                    FieldType: "M2MRelation"
                  },
                  Cities: {
                    IsBackward: false,
                    Content: {
                      ContentId: 290,
                      ContentPath: "/339:1542/383:2531/511:2533/290",
                      ContentName: "Region",
                      ContentTitle: "Регионы",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Alias: {
                          FieldId: 1532,
                          FieldName: "Alias",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Title: {
                          FieldId: 1114,
                          FieldName: "Title",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        }
                      }
                    },
                    FieldId: 2533,
                    FieldName: "Cities",
                    FieldTitle: "Города",
                    FieldDescription: "",
                    FieldOrder: 4,
                    IsRequired: false,
                    FieldType: "M2MRelation"
                  },
                  Order: {
                    FieldId: 2534,
                    FieldName: "Order",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 5,
                    IsRequired: false,
                    FieldType: "Numeric"
                  }
                }
              },
              FieldId: 2531,
              FieldName: "TariffsOnMarketingDevice",
              FieldTitle: "Маркетинговое устройство",
              FieldDescription: "",
              FieldOrder: 2,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            DevicesOnMarketingTariff: {
              IsBackward: true,
              Content: {
                ContentId: 511,
                ContentPath: "/339:1542/383:2532/511",
                ContentName: "DeviceOnTariffs",
                ContentTitle: "Оборудование на тарифах",
                ContentDescription: "",
                ObjectShape: null,
                Fields: {
                  Parent: {
                    IsBackward: false,
                    Content: {
                      ContentId: 361,
                      ContentPath: "/339:1542/383:2532/511:2530/361",
                      ContentName: "ProductRelation",
                      ContentTitle: "Матрица связей",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Title: {
                          FieldId: 1416,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Parameters: {
                          IsBackward: false,
                          Content: {
                            ContentId: 362,
                            ContentPath: "/339:1542/383:2532/511:2530/361:1431/362",
                            ContentName: "LinkParameter",
                            ContentTitle: "Параметры связей",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1418,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              SortOrder: {
                                FieldId: 1425,
                                FieldName: "SortOrder",
                                FieldTitle: "Порядок",
                                FieldDescription: "",
                                FieldOrder: 12,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              NumValue: {
                                FieldId: 1426,
                                FieldName: "NumValue",
                                FieldTitle: "Числовое значение",
                                FieldDescription: "",
                                FieldOrder: 13,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              Value: {
                                FieldId: 1427,
                                FieldName: "Value",
                                FieldTitle: "Текстовое значение",
                                FieldDescription: "",
                                FieldOrder: 14,
                                IsRequired: false,
                                FieldType: "VisualEdit"
                              },
                              Description: {
                                FieldId: 1429,
                                FieldName: "Description",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 16,
                                IsRequired: false,
                                FieldType: "VisualEdit"
                              },
                              Unit: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 355,
                                  ContentPath: "/339:1542/383:1869/424:1862/355",
                                  ContentName: "Unit",
                                  ContentTitle: "Единицы измерения",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Alias: {
                                      FieldId: 2062,
                                      FieldName: "Alias",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Title: {
                                      FieldId: 1384,
                                      FieldName: "Title",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Display: {
                                      FieldId: 1385,
                                      FieldName: "Display",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    QuotaUnit: {
                                      Items: [
                                        {
                                          Value: "mb",
                                          Alias: "Мегабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "gb",
                                          Alias: "Гигабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "kb",
                                          Alias: "Килобайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "tb",
                                          Alias: "Терабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "min",
                                          Alias: "Минута",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "message",
                                          Alias: "Сообщение",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "rub",
                                          Alias: "Рублей",
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
                                          Alias: "Мегабит",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "step",
                                          Alias: "Шаг",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1904,
                                      FieldName: "QuotaUnit",
                                      FieldTitle: "Размерность",
                                      FieldDescription: "",
                                      FieldOrder: 4,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    },
                                    QuotaPeriod: {
                                      Items: [
                                        {
                                          Value: "daily",
                                          Alias: "Сутки",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "weekly",
                                          Alias: "Неделя",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "monthly",
                                          Alias: "Месяц",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "hourly",
                                          Alias: "Час",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "minutely",
                                          Alias: "Минута",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "every_second",
                                          Alias: "Секунда",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "annually",
                                          Alias: "Год",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1905,
                                      FieldName: "QuotaPeriod",
                                      FieldTitle: "Период",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    }
                                  }
                                },
                                FieldId: 1428,
                                FieldName: "Unit",
                                FieldTitle: "Единица измерения",
                                FieldDescription: "",
                                FieldOrder: 15,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Modifiers: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 352,
                                  ContentPath: "/339:1542/383:1869/424:1858/352",
                                  ContentName: "ParameterModifier",
                                  ContentTitle: "Модификаторы параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1364,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1365,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1424,
                                FieldName: "Modifiers",
                                FieldTitle: "Модификаторы",
                                FieldDescription: "",
                                FieldOrder: 11,
                                IsRequired: false,
                                FieldType: "M2MRelation"
                              },
                              BaseParameterModifiers: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 351,
                                  ContentPath: "/339:1542/383:1869/424:1857/351",
                                  ContentName: "BaseParameterModifier",
                                  ContentTitle: "Модификаторы базовых параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1361,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1362,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Type: {
                                      Items: [
                                        {
                                          Value: "Step",
                                          Alias: "Ступенчатая тарификация",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Package",
                                          Alias: "Пакет",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Zone",
                                          Alias: "Зона",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Direction",
                                          Alias: "Направление",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Refining",
                                          Alias: "Уточнение",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1894,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    }
                                  }
                                },
                                FieldId: 1423,
                                FieldName: "BaseParameterModifiers",
                                FieldTitle: "Модификаторы базового параметра",
                                FieldDescription: "",
                                FieldOrder: 10,
                                IsRequired: false,
                                FieldType: "M2MRelation"
                              },
                              Direction: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 347,
                                  ContentPath: "/339:1542/383:1869/424:1856/347",
                                  ContentName: "Direction",
                                  ContentTitle: "Направления соединения",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1349,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1350,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1422,
                                FieldName: "Direction",
                                FieldTitle: "Направление действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 9,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Zone: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 346,
                                  ContentPath: "/339:1542/383:1869/424:1855/346",
                                  ContentName: "TariffZone",
                                  ContentTitle: "Тарифные зоны",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1346,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1347,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1421,
                                FieldName: "Zone",
                                FieldTitle: "Зона действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 8,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              BaseParameter: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 350,
                                  ContentPath: "/339:1542/383:1869/424:1854/350",
                                  ContentName: "BaseParameter",
                                  ContentTitle: "Базовые параметры продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1358,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1359,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    AllowZone: {
                                      FieldId: 2683,
                                      FieldName: "AllowZone",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "Boolean"
                                    },
                                    AllowDirection: {
                                      FieldId: 2684,
                                      FieldName: "AllowDirection",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 6,
                                      IsRequired: false,
                                      FieldType: "Boolean"
                                    }
                                  }
                                },
                                FieldId: 1420,
                                FieldName: "BaseParameter",
                                FieldTitle: "Базовый параметр",
                                FieldDescription: "",
                                FieldOrder: 7,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Group: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 378,
                                  ContentPath: "/339:1542/383:1869/424:1852/378",
                                  ContentName: "ProductParameterGroup",
                                  ContentTitle: "Группы параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    SortOrder: {
                                      FieldId: 1500,
                                      FieldName: "SortOrder",
                                      FieldTitle: "Порядок",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    },
                                    Title: {
                                      FieldId: 1496,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 2049,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    ImageSvg: {
                                      FieldId: 2029,
                                      FieldName: "ImageSvg",
                                      FieldTitle: "Изображение",
                                      FieldDescription: "",
                                      FieldOrder: 7,
                                      IsRequired: false,
                                      FieldType: "File"
                                    },
                                    Type: {
                                      FieldId: 2061,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 8,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1678,
                                FieldName: "Group",
                                FieldTitle: "Группа параметров",
                                FieldDescription: "",
                                FieldOrder: 3,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Choice: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 488,
                                  ContentPath: "/339:1542/383:1869/424:2685/488",
                                  ContentName: "ParameterChoice",
                                  ContentTitle: "Варианты выбора для параметров",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 2379,
                                      FieldName: "Title",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 2380,
                                      FieldName: "Alias",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    OldSiteId: {
                                      FieldId: 2382,
                                      FieldName: "OldSiteId",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 4,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    }
                                  }
                                },
                                FieldId: 2687,
                                FieldName: "Choice",
                                FieldTitle: "Выбор",
                                FieldDescription: "",
                                FieldOrder: 17,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              }
                            }
                          },
                          FieldId: 1431,
                          FieldName: "Parameters",
                          FieldTitle: "Параметры",
                          FieldDescription: "",
                          FieldOrder: 3,
                          IsRequired: false,
                          FieldType: "M2ORelation"
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            ContentId: 360,
                            ContentPath: "/339:1542/383:2531/511:2530/361:1450/360",
                            ContentName: "LinkModifier",
                            ContentTitle: "Модификаторы связей",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1413,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1414,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              }
                            }
                          },
                          FieldId: 1450,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 4,
                          IsRequired: false,
                          FieldType: "M2MRelation"
                        }
                      }
                    },
                    FieldId: 2530,
                    FieldName: "Parent",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  MarketingDevice: {
                    IsBackward: false,
                    Content: {
                      ContentId: 383,
                      ContentPath: "/339:1542/383:1540/498:2564/383",
                      ContentName: "MarketingProduct",
                      ContentTitle: "Маркетинговые продукты",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Title: {
                          FieldId: 1534,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Alias: {
                          FieldId: 1753,
                          FieldName: "Alias",
                          FieldTitle: "Псевдоним",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "String"
                        }
                      }
                    },
                    FieldId: 2531,
                    FieldName: "MarketingDevice",
                    FieldTitle: "Маркетинговое устройство",
                    FieldDescription: "",
                    FieldOrder: 2,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  Cities: {
                    IsBackward: false,
                    Content: {
                      ContentId: 290,
                      ContentPath: "/339:1542/383:2531/511:2533/290",
                      ContentName: "Region",
                      ContentTitle: "Регионы",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Alias: {
                          FieldId: 1532,
                          FieldName: "Alias",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Title: {
                          FieldId: 1114,
                          FieldName: "Title",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        }
                      }
                    },
                    FieldId: 2533,
                    FieldName: "Cities",
                    FieldTitle: "Города",
                    FieldDescription: "",
                    FieldOrder: 4,
                    IsRequired: false,
                    FieldType: "M2MRelation"
                  },
                  Order: {
                    FieldId: 2534,
                    FieldName: "Order",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 5,
                    IsRequired: false,
                    FieldType: "Numeric"
                  }
                }
              },
              FieldId: 2532,
              FieldName: "DevicesOnMarketingTariff",
              FieldTitle: "Маркетинговые тарифы",
              FieldDescription: "",
              FieldOrder: 3,
              IsRequired: false,
              FieldType: "M2MRelation"
            },
            ActionsOnMarketingDevice: {
              IsBackward: true,
              Content: {
                ContentId: 512,
                ContentPath: "/339:1542/383:2538/512",
                ContentName: "DevicesForFixConnectAction",
                ContentTitle: "Акционное оборудование",
                ContentDescription: "Оборудование для акций фиксированной связи",
                ObjectShape: null,
                Fields: {
                  FixConnectAction: {
                    IsBackward: false,
                    Content: {
                      ContentId: 339,
                      ContentPath: "/339:1542/383:2538/512:2537/339",
                      ContentName: "Product",
                      ContentTitle: "Продукты",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        MarketingProduct: {
                          IsBackward: false,
                          Content: {
                            ContentId: 383,
                            ContentPath: "/339:1542/383:2538/512:2537/339:1542/383",
                            ContentName: "MarketingProduct",
                            ContentTitle: "Маркетинговые продукты",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1534,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              }
                            }
                          },
                          FieldId: 1542,
                          FieldName: "MarketingProduct",
                          FieldTitle: "Маркетинговый продукт",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "O2MRelation"
                        },
                        GlobalCode: {
                          FieldId: 2033,
                          FieldName: "GlobalCode",
                          FieldTitle: "GlobalCode",
                          FieldDescription: "",
                          FieldOrder: 3,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Type: {
                          FieldId: 1341,
                          FieldName: "Type",
                          FieldTitle: "Тип",
                          FieldDescription: "",
                          FieldOrder: 5,
                          IsRequired: false,
                          FieldType: "Classifier"
                        },
                        Description: {
                          FieldId: 1551,
                          FieldName: "Description",
                          FieldTitle: "Описание",
                          FieldDescription: "",
                          FieldOrder: 6,
                          IsRequired: false,
                          FieldType: "Textbox"
                        },
                        FullDescription: {
                          FieldId: 1552,
                          FieldName: "FullDescription",
                          FieldTitle: "Полное описание",
                          FieldDescription: "",
                          FieldOrder: 7,
                          IsRequired: false,
                          FieldType: "VisualEdit"
                        },
                        Notes: {
                          FieldId: 1640,
                          FieldName: "Notes",
                          FieldTitle: "Примечания",
                          FieldDescription: "",
                          FieldOrder: 8,
                          IsRequired: false,
                          FieldType: "Textbox"
                        },
                        Link: {
                          FieldId: 1572,
                          FieldName: "Link",
                          FieldTitle: "Ссылка",
                          FieldDescription: "",
                          FieldOrder: 9,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        SortOrder: {
                          FieldId: 1476,
                          FieldName: "SortOrder",
                          FieldTitle: "Порядок",
                          FieldDescription: "",
                          FieldOrder: 10,
                          IsRequired: false,
                          FieldType: "Numeric"
                        },
                        ForisID: {
                          FieldId: 1470,
                          FieldName: "ForisID",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 11,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Icon: {
                          FieldId: 1581,
                          FieldName: "Icon",
                          FieldTitle: "Иконка",
                          FieldDescription: "",
                          FieldOrder: 15,
                          IsRequired: false,
                          FieldType: "Image"
                        },
                        PDF: {
                          FieldId: 1582,
                          FieldName: "PDF",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 18,
                          IsRequired: false,
                          FieldType: "File"
                        },
                        PdfFixedAlias: {
                          FieldId: 2677,
                          FieldName: "PdfFixedAlias",
                          FieldTitle: "Алиас фиксированной ссылки на Pdf",
                          FieldDescription: "",
                          FieldOrder: 19,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        PdfFixedLinks: {
                          FieldId: 2680,
                          FieldName: "PdfFixedLinks",
                          FieldTitle: "Фиксированные ссылки на Pdf",
                          FieldDescription: "",
                          FieldOrder: 20,
                          IsRequired: false,
                          FieldType: "Textbox"
                        },
                        StartDate: {
                          FieldId: 1407,
                          FieldName: "StartDate",
                          FieldTitle: "Дата начала публикации",
                          FieldDescription: "",
                          FieldOrder: 21,
                          IsRequired: false,
                          FieldType: "Date"
                        },
                        EndDate: {
                          FieldId: 1410,
                          FieldName: "EndDate",
                          FieldTitle: "Дата снятия с публикации",
                          FieldDescription: "",
                          FieldOrder: 22,
                          IsRequired: false,
                          FieldType: "Date"
                        },
                        OldSiteId: {
                          FieldId: 1477,
                          FieldName: "OldSiteId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 24,
                          IsRequired: false,
                          FieldType: "Numeric"
                        },
                        OldId: {
                          FieldId: 1655,
                          FieldName: "OldId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 25,
                          IsRequired: false,
                          FieldType: "Numeric"
                        },
                        OldSiteInvId: {
                          FieldId: 1763,
                          FieldName: "OldSiteInvId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 26,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        OldCorpSiteId: {
                          FieldId: 1764,
                          FieldName: "OldCorpSiteId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 27,
                          IsRequired: false,
                          FieldType: "Numeric"
                        },
                        OldAliasId: {
                          FieldId: 1644,
                          FieldName: "OldAliasId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 28,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Priority: {
                          FieldId: 2132,
                          FieldName: "Priority",
                          FieldTitle: "Приоритет (популярность)",
                          FieldDescription: "Сортировка по возрастанию значения приоритета",
                          FieldOrder: 31,
                          IsRequired: false,
                          FieldType: "Numeric"
                        },
                        ListImage: {
                          FieldId: 2498,
                          FieldName: "ListImage",
                          FieldTitle: "Изображение в списке",
                          FieldDescription: "Изображение в общем списке",
                          FieldOrder: 33,
                          IsRequired: false,
                          FieldType: "Image"
                        },
                        ArchiveDate: {
                          FieldId: 2526,
                          FieldName: "ArchiveDate",
                          FieldTitle: "Дата перевода в архив",
                          FieldDescription: "",
                          FieldOrder: 34,
                          IsRequired: false,
                          FieldType: "Date"
                        }
                      }
                    },
                    FieldId: 2537,
                    FieldName: "FixConnectAction",
                    FieldTitle: "Акция фиксированной связи",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  Parent: {
                    IsBackward: false,
                    Content: {
                      ContentId: 361,
                      ContentPath: "/339:1542/383:2538/512:2536/361",
                      ContentName: "ProductRelation",
                      ContentTitle: "Матрица связей",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Title: {
                          FieldId: 1416,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Type: {
                          FieldId: 1417,
                          FieldName: "Type",
                          FieldTitle: "Тип",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "Classifier"
                        },
                        Parameters: {
                          IsBackward: false,
                          Content: {
                            ContentId: 362,
                            ContentPath: "/339:1542/383:2538/512:2536/361:1431/362",
                            ContentName: "LinkParameter",
                            ContentTitle: "Параметры связей",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Unit: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 355,
                                  ContentPath: "/339:1542/383:2538/512:2536/361:1431/362:1428/355",
                                  ContentName: "Unit",
                                  ContentTitle: "Единицы измерения",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Alias: {
                                      FieldId: 2062,
                                      FieldName: "Alias",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Title: {
                                      FieldId: 1384,
                                      FieldName: "Title",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Display: {
                                      FieldId: 1385,
                                      FieldName: "Display",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    QuotaUnit: {
                                      Items: [
                                        {
                                          Value: "mb",
                                          Alias: "Мегабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "gb",
                                          Alias: "Гигабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "kb",
                                          Alias: "Килобайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "tb",
                                          Alias: "Терабайт",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "min",
                                          Alias: "Минута",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "message",
                                          Alias: "Сообщение",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "rub",
                                          Alias: "Рублей",
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
                                          Alias: "Мегабит",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "step",
                                          Alias: "Шаг",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1904,
                                      FieldName: "QuotaUnit",
                                      FieldTitle: "Размерность",
                                      FieldDescription: "",
                                      FieldOrder: 4,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    },
                                    QuotaPeriod: {
                                      Items: [
                                        {
                                          Value: "daily",
                                          Alias: "Сутки",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "weekly",
                                          Alias: "Неделя",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "monthly",
                                          Alias: "Месяц",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "hourly",
                                          Alias: "Час",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "minutely",
                                          Alias: "Минута",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "every_second",
                                          Alias: "Секунда",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "annually",
                                          Alias: "Год",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1905,
                                      FieldName: "QuotaPeriod",
                                      FieldTitle: "Период",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    },
                                    QuotaPeriodicity: {
                                      FieldId: 2177,
                                      FieldName: "QuotaPeriodicity",
                                      FieldTitle: "Название периодичности",
                                      FieldDescription: "",
                                      FieldOrder: 6,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    PeriodMultiplier: {
                                      FieldId: 2114,
                                      FieldName: "PeriodMultiplier",
                                      FieldTitle: "Множитель периода",
                                      FieldDescription: "",
                                      FieldOrder: 7,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    },
                                    Type: {
                                      FieldId: 2568,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 8,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1428,
                                FieldName: "Unit",
                                FieldTitle: "Единица измерения",
                                FieldDescription: "",
                                FieldOrder: 15,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              BaseParameterModifiers: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 351,
                                  ContentPath: "/339:1542/383:1869/424:1857/351",
                                  ContentName: "BaseParameterModifier",
                                  ContentTitle: "Модификаторы базовых параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1361,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1362,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Type: {
                                      Items: [
                                        {
                                          Value: "Step",
                                          Alias: "Ступенчатая тарификация",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Package",
                                          Alias: "Пакет",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Zone",
                                          Alias: "Зона",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Direction",
                                          Alias: "Направление",
                                          IsDefault: false,
                                          Invalid: false
                                        },
                                        {
                                          Value: "Refining",
                                          Alias: "Уточнение",
                                          IsDefault: false,
                                          Invalid: false
                                        }
                                      ],
                                      FieldId: 1894,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "StringEnum"
                                    }
                                  }
                                },
                                FieldId: 1423,
                                FieldName: "BaseParameterModifiers",
                                FieldTitle: "Модификаторы базового параметра",
                                FieldDescription: "",
                                FieldOrder: 10,
                                IsRequired: false,
                                FieldType: "M2MRelation"
                              },
                              Modifiers: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 352,
                                  ContentPath: "/339:1542/383:1869/424:1858/352",
                                  ContentName: "ParameterModifier",
                                  ContentTitle: "Модификаторы параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1364,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1365,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1424,
                                FieldName: "Modifiers",
                                FieldTitle: "Модификаторы",
                                FieldDescription: "",
                                FieldOrder: 11,
                                IsRequired: false,
                                FieldType: "M2MRelation"
                              },
                              Direction: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 347,
                                  ContentPath: "/339:1542/383:1869/424:1856/347",
                                  ContentName: "Direction",
                                  ContentTitle: "Направления соединения",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1349,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1350,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1422,
                                FieldName: "Direction",
                                FieldTitle: "Направление действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 9,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Zone: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 346,
                                  ContentPath: "/339:1542/383:1869/424:1855/346",
                                  ContentName: "TariffZone",
                                  ContentTitle: "Тарифные зоны",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1346,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1347,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1421,
                                FieldName: "Zone",
                                FieldTitle: "Зона действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 8,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              BaseParameter: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 350,
                                  ContentPath: "/339:1542/383:1869/424:1854/350",
                                  ContentName: "BaseParameter",
                                  ContentTitle: "Базовые параметры продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1358,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 1359,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    AllowZone: {
                                      FieldId: 2683,
                                      FieldName: "AllowZone",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "Boolean"
                                    },
                                    AllowDirection: {
                                      FieldId: 2684,
                                      FieldName: "AllowDirection",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 6,
                                      IsRequired: false,
                                      FieldType: "Boolean"
                                    }
                                  }
                                },
                                FieldId: 1420,
                                FieldName: "BaseParameter",
                                FieldTitle: "Базовый параметр",
                                FieldDescription: "",
                                FieldOrder: 7,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Group: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 378,
                                  ContentPath: "/339:1542/383:2538/512:2536/361:1431/362:1678/378",
                                  ContentName: "ProductParameterGroup",
                                  ContentTitle: "Группы параметров продуктов",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 1496,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 2049,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    SortOrder: {
                                      FieldId: 1500,
                                      FieldName: "SortOrder",
                                      FieldTitle: "Порядок",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    },
                                    OldSiteId: {
                                      FieldId: 1588,
                                      FieldName: "OldSiteId",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    },
                                    OldCorpSiteId: {
                                      FieldId: 1771,
                                      FieldName: "OldCorpSiteId",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 6,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    },
                                    ImageSvg: {
                                      FieldId: 2029,
                                      FieldName: "ImageSvg",
                                      FieldTitle: "Изображение",
                                      FieldDescription: "",
                                      FieldOrder: 7,
                                      IsRequired: false,
                                      FieldType: "File"
                                    },
                                    Type: {
                                      FieldId: 2061,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 8,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    TitleForIcin: {
                                      FieldId: 2116,
                                      FieldName: "TitleForIcin",
                                      FieldTitle: "Название для МГМН",
                                      FieldDescription: "",
                                      FieldOrder: 9,
                                      IsRequired: false,
                                      FieldType: "String"
                                    }
                                  }
                                },
                                FieldId: 1678,
                                FieldName: "Group",
                                FieldTitle: "Группа параметров",
                                FieldDescription: "",
                                FieldOrder: 3,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Choice: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 488,
                                  ContentPath: "/339:1542/383:1869/424:2685/488",
                                  ContentName: "ParameterChoice",
                                  ContentTitle: "Варианты выбора для параметров",
                                  ContentDescription: "",
                                  ObjectShape: null,
                                  Fields: {
                                    Title: {
                                      FieldId: 2379,
                                      FieldName: "Title",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    Alias: {
                                      FieldId: 2380,
                                      FieldName: "Alias",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      IsRequired: false,
                                      FieldType: "String"
                                    },
                                    OldSiteId: {
                                      FieldId: 2382,
                                      FieldName: "OldSiteId",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 4,
                                      IsRequired: false,
                                      FieldType: "Numeric"
                                    }
                                  }
                                },
                                FieldId: 2687,
                                FieldName: "Choice",
                                FieldTitle: "Выбор",
                                FieldDescription: "",
                                FieldOrder: 17,
                                IsRequired: false,
                                FieldType: "O2MRelation"
                              },
                              Title: {
                                FieldId: 1418,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              SortOrder: {
                                FieldId: 1425,
                                FieldName: "SortOrder",
                                FieldTitle: "Порядок",
                                FieldDescription: "",
                                FieldOrder: 12,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              NumValue: {
                                FieldId: 1426,
                                FieldName: "NumValue",
                                FieldTitle: "Числовое значение",
                                FieldDescription: "",
                                FieldOrder: 13,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              Value: {
                                FieldId: 1427,
                                FieldName: "Value",
                                FieldTitle: "Текстовое значение",
                                FieldDescription: "",
                                FieldOrder: 14,
                                IsRequired: false,
                                FieldType: "VisualEdit"
                              },
                              Description: {
                                FieldId: 1429,
                                FieldName: "Description",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 16,
                                IsRequired: false,
                                FieldType: "VisualEdit"
                              },
                              OldSiteId: {
                                FieldId: 1656,
                                FieldName: "OldSiteId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 20,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              OldCorpSiteId: {
                                FieldId: 1772,
                                FieldName: "OldCorpSiteId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 21,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              OldPointId: {
                                FieldId: 1658,
                                FieldName: "OldPointId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 22,
                                IsRequired: false,
                                FieldType: "Numeric"
                              },
                              OldCorpPointId: {
                                FieldId: 1774,
                                FieldName: "OldCorpPointId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 23,
                                IsRequired: false,
                                FieldType: "Numeric"
                              }
                            }
                          },
                          FieldId: 1431,
                          FieldName: "Parameters",
                          FieldTitle: "Параметры",
                          FieldDescription: "",
                          FieldOrder: 3,
                          IsRequired: false,
                          FieldType: "M2ORelation"
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            ContentId: 360,
                            ContentPath: "/339:1542/383:2531/511:2530/361:1450/360",
                            ContentName: "LinkModifier",
                            ContentTitle: "Модификаторы связей",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1413,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1414,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              }
                            }
                          },
                          FieldId: 1450,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 4,
                          IsRequired: false,
                          FieldType: "M2MRelation"
                        }
                      }
                    },
                    FieldId: 2536,
                    FieldName: "Parent",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 0,
                    IsRequired: false,
                    FieldType: "O2MRelation"
                  },
                  Order: {
                    FieldId: 2539,
                    FieldName: "Order",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 3,
                    IsRequired: false,
                    FieldType: "Numeric"
                  }
                }
              },
              FieldId: 2538,
              FieldName: "ActionsOnMarketingDevice",
              FieldTitle: "Маркетинговое оборудование",
              FieldDescription: "",
              FieldOrder: 2,
              IsRequired: false,
              FieldType: "O2MRelation"
            }
          }
        },
        FieldId: 1542,
        FieldName: "MarketingProduct",
        FieldTitle: "Маркетинговый продукт",
        FieldDescription: "",
        FieldOrder: 1,
        IsRequired: false,
        FieldType: "O2MRelation"
      },
      GlobalCode: {
        FieldId: 2033,
        FieldName: "GlobalCode",
        FieldTitle: "GlobalCode",
        FieldDescription: "",
        FieldOrder: 3,
        IsRequired: false,
        FieldType: "String"
      },
      Description: {
        FieldId: 1551,
        FieldName: "Description",
        FieldTitle: "Описание",
        FieldDescription: "",
        FieldOrder: 6,
        IsRequired: false,
        FieldType: "Textbox"
      },
      FullDescription: {
        FieldId: 1552,
        FieldName: "FullDescription",
        FieldTitle: "Полное описание",
        FieldDescription: "",
        FieldOrder: 7,
        IsRequired: false,
        FieldType: "VisualEdit"
      },
      Notes: {
        FieldId: 1640,
        FieldName: "Notes",
        FieldTitle: "Примечания",
        FieldDescription: "",
        FieldOrder: 8,
        IsRequired: false,
        FieldType: "Textbox"
      },
      Link: {
        FieldId: 1572,
        FieldName: "Link",
        FieldTitle: "Ссылка",
        FieldDescription: "",
        FieldOrder: 9,
        IsRequired: false,
        FieldType: "String"
      },
      SortOrder: {
        FieldId: 1476,
        FieldName: "SortOrder",
        FieldTitle: "Порядок",
        FieldDescription: "",
        FieldOrder: 10,
        IsRequired: false,
        FieldType: "Numeric"
      },
      Icon: {
        FieldId: 1581,
        FieldName: "Icon",
        FieldTitle: "Иконка",
        FieldDescription: "",
        FieldOrder: 15,
        IsRequired: false,
        FieldType: "Image"
      },
      PDF: {
        FieldId: 1582,
        FieldName: "PDF",
        FieldTitle: "",
        FieldDescription: "",
        FieldOrder: 18,
        IsRequired: false,
        FieldType: "File"
      },
      StartDate: {
        FieldId: 1407,
        FieldName: "StartDate",
        FieldTitle: "Дата начала публикации",
        FieldDescription: "",
        FieldOrder: 21,
        IsRequired: false,
        FieldType: "Date"
      },
      EndDate: {
        FieldId: 1410,
        FieldName: "EndDate",
        FieldTitle: "Дата снятия с публикации",
        FieldDescription: "",
        FieldOrder: 22,
        IsRequired: false,
        FieldType: "Date"
      },
      Priority: {
        FieldId: 2132,
        FieldName: "Priority",
        FieldTitle: "Приоритет (популярность)",
        FieldDescription: "Сортировка по возрастанию значения приоритета",
        FieldOrder: 31,
        IsRequired: false,
        FieldType: "Numeric"
      },
      ListImage: {
        FieldId: 2498,
        FieldName: "ListImage",
        FieldTitle: "Изображение в списке",
        FieldDescription: "Изображение в общем списке",
        FieldOrder: 33,
        IsRequired: false,
        FieldType: "Image"
      },
      ArchiveDate: {
        FieldId: 2526,
        FieldName: "ArchiveDate",
        FieldTitle: "Дата перевода в архив",
        FieldDescription: "",
        FieldOrder: 34,
        IsRequired: false,
        FieldType: "Date"
      },
      Modifiers: {
        IsBackward: false,
        Content: {
          $ref: "#/Definitions/ProductModifer"
        },
        FieldId: 1523,
        FieldName: "Modifiers",
        FieldTitle: "Модификаторы",
        FieldDescription: "",
        FieldOrder: 4,
        IsRequired: false,
        FieldType: "M2MRelation"
      },
      Parameters: {
        IsBackward: false,
        Content: {
          ContentId: 354,
          ContentPath: "/339:1403/354",
          ContentName: "ProductParameter",
          ContentTitle: "Параметры продуктов",
          ContentDescription: "",
          ObjectShape: null,
          Fields: {
            Group: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/ProductParameterGroup1"
              },
              FieldId: 1506,
              FieldName: "Group",
              FieldTitle: "Группа параметров",
              FieldDescription: "",
              FieldOrder: 4,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            Parent: {
              IsBackward: false,
              Content: {
                ContentId: 354,
                ContentPath: "/339:1403/354:1642/354",
                ContentName: "ProductParameter",
                ContentTitle: "Параметры продуктов",
                ContentDescription: "",
                ObjectShape: null,
                Fields: {
                  Title: {
                    FieldId: 1373,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "String"
                  }
                }
              },
              FieldId: 1642,
              FieldName: "Parent",
              FieldTitle: "Родительский параметр",
              FieldDescription: "",
              FieldOrder: 5,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            BaseParameter: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/BaseParameter"
              },
              FieldId: 1375,
              FieldName: "BaseParameter",
              FieldTitle: "Базовый параметр",
              FieldDescription: "",
              FieldOrder: 6,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            Zone: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/TariffZone"
              },
              FieldId: 1377,
              FieldName: "Zone",
              FieldTitle: "Зона действия базового параметра",
              FieldDescription: "",
              FieldOrder: 7,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            Direction: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/Direction"
              },
              FieldId: 1378,
              FieldName: "Direction",
              FieldTitle: "Направление действия базового параметра",
              FieldDescription: "",
              FieldOrder: 8,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            BaseParameterModifiers: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/BaseParameterModifier"
              },
              FieldId: 1379,
              FieldName: "BaseParameterModifiers",
              FieldTitle: "Модификаторы базового параметра",
              FieldDescription: "",
              FieldOrder: 9,
              IsRequired: false,
              FieldType: "M2MRelation"
            },
            Modifiers: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/ParameterModifier"
              },
              FieldId: 1380,
              FieldName: "Modifiers",
              FieldTitle: "Модификаторы",
              FieldDescription: "",
              FieldOrder: 10,
              IsRequired: false,
              FieldType: "M2MRelation"
            },
            Unit: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/Unit"
              },
              FieldId: 1386,
              FieldName: "Unit",
              FieldTitle: "Единица измерения",
              FieldDescription: "",
              FieldOrder: 14,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            Title: {
              FieldId: 1373,
              FieldName: "Title",
              FieldTitle: "Название",
              FieldDescription: "",
              FieldOrder: 1,
              IsRequired: false,
              FieldType: "String"
            },
            SortOrder: {
              FieldId: 1381,
              FieldName: "SortOrder",
              FieldTitle: "Порядок",
              FieldDescription: "",
              FieldOrder: 11,
              IsRequired: false,
              FieldType: "Numeric"
            },
            NumValue: {
              FieldId: 1382,
              FieldName: "NumValue",
              FieldTitle: "Числовое значение",
              FieldDescription: "",
              FieldOrder: 12,
              IsRequired: false,
              FieldType: "Numeric"
            },
            Value: {
              FieldId: 1383,
              FieldName: "Value",
              FieldTitle: "Текстовое значение",
              FieldDescription: "",
              FieldOrder: 13,
              IsRequired: false,
              FieldType: "VisualEdit"
            },
            Description: {
              FieldId: 1387,
              FieldName: "Description",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 15,
              IsRequired: false,
              FieldType: "VisualEdit"
            },
            Image: {
              FieldId: 2022,
              FieldName: "Image",
              FieldTitle: "Изображение параметра",
              FieldDescription: "",
              FieldOrder: 19,
              IsRequired: false,
              FieldType: "Image"
            },
            ProductGroup: {
              IsBackward: false,
              Content: {
                ContentId: 340,
                ContentPath: "/339:1403/354:1758/340",
                ContentName: "Group",
                ContentTitle: "Группы продуктов",
                ContentDescription: "",
                ObjectShape: null,
                Fields: {
                  Title: {
                    FieldId: 1329,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "String"
                  },
                  Alias: {
                    FieldId: 1754,
                    FieldName: "Alias",
                    FieldTitle: "Псевдоним",
                    FieldDescription: "",
                    FieldOrder: 2,
                    IsRequired: false,
                    FieldType: "String"
                  }
                }
              },
              FieldId: 1758,
              FieldName: "ProductGroup",
              FieldTitle: "Группа продуктов",
              FieldDescription: "",
              FieldOrder: 16,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            Choice: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/ParameterChoice"
              },
              FieldId: 2445,
              FieldName: "Choice",
              FieldTitle: "Выбор",
              FieldDescription: "",
              FieldOrder: 17,
              IsRequired: false,
              FieldType: "O2MRelation"
            }
          }
        },
        FieldId: 1403,
        FieldName: "Parameters",
        FieldTitle: "Параметры продукта",
        FieldDescription: "",
        FieldOrder: 17,
        IsRequired: false,
        FieldType: "M2ORelation"
      },
      Regions: {
        IsBackward: false,
        Content: {
          ContentId: 290,
          ContentPath: "/339:1326/290",
          ContentName: "Region",
          ContentTitle: "Регионы",
          ContentDescription: "",
          ObjectShape: null,
          Fields: {
            Title: {
              FieldId: 1114,
              FieldName: "Title",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 1,
              IsRequired: false,
              FieldType: "String"
            },
            Alias: {
              FieldId: 1532,
              FieldName: "Alias",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 2,
              IsRequired: false,
              FieldType: "String"
            },
            Parent: {
              IsBackward: false,
              Content: {
                $ref: "#/Definitions/Region2"
              },
              FieldId: 1115,
              FieldName: "Parent",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 4,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            IsMainCity: {
              FieldId: 2239,
              FieldName: "IsMainCity",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 12,
              IsRequired: false,
              FieldType: "Boolean"
            }
          }
        },
        FieldId: 1326,
        FieldName: "Regions",
        FieldTitle: "Регионы",
        FieldDescription: "",
        FieldOrder: 2,
        IsRequired: false,
        FieldType: "M2MRelation"
      },
      Type: {
        Contents: {
          Tariff: {
            ContentId: 343,
            ContentPath: "/339:1341/343",
            ContentName: "Tariff",
            ContentTitle: "Тарифы",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {}
          },
          Service: {
            ContentId: 403,
            ContentPath: "/339:1341/403",
            ContentName: "Service",
            ContentTitle: "Услуги",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {}
          },
          Action: {
            ContentId: 419,
            ContentPath: "/339:1341/419",
            ContentName: "Action",
            ContentTitle: "Акции",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {}
          },
          RoamingScale: {
            ContentId: 434,
            ContentPath: "/339:1341/434",
            ContentName: "RoamingScale",
            ContentTitle: "Роуминговые сетки",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {}
          },
          Device: {
            ContentId: 490,
            ContentPath: "/339:1341/490",
            ContentName: "Device",
            ContentTitle: "Оборудование",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {
              Downloads: {
                IsBackward: false,
                Content: {
                  ContentId: 494,
                  ContentPath: "/339:1341/490:2407/494",
                  ContentName: "EquipmentDownload",
                  ContentTitle: "Загрузки для оборудования",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {
                    Title: {
                      FieldId: 2405,
                      FieldName: "Title",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 1,
                      IsRequired: false,
                      FieldType: "String"
                    },
                    File: {
                      FieldId: 2406,
                      FieldName: "File",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 2,
                      IsRequired: false,
                      FieldType: "File"
                    }
                  }
                },
                FieldId: 2407,
                FieldName: "Downloads",
                FieldTitle: "Загрузки",
                FieldDescription: "",
                FieldOrder: 2,
                IsRequired: false,
                FieldType: "M2MRelation"
              },
              Inners: {
                IsBackward: false,
                Content: {
                  ContentId: 339,
                  ContentPath: "/339:1341/490:2447/339",
                  ContentName: "Product",
                  ContentTitle: "Продукты",
                  ContentDescription: "",
                  ObjectShape: null,
                  Fields: {
                    MarketingProduct: {
                      IsBackward: false,
                      Content: {
                        $ref: "#/Definitions/MarketingProduct3"
                      },
                      FieldId: 1542,
                      FieldName: "MarketingProduct",
                      FieldTitle: "Маркетинговый продукт",
                      FieldDescription: "",
                      FieldOrder: 1,
                      IsRequired: false,
                      FieldType: "O2MRelation"
                    }
                  }
                },
                FieldId: 2447,
                FieldName: "Inners",
                FieldTitle: "Состав комплекта",
                FieldDescription: "",
                FieldOrder: 6,
                IsRequired: false,
                FieldType: "M2MRelation"
              },
              FreezeDate: {
                FieldId: 2390,
                FieldName: "FreezeDate",
                FieldTitle: "Отложенная публикация на",
                FieldDescription:
                  "Продукт будет опубликован в течение 2,5 часов после наступления даты публикации",
                FieldOrder: 3,
                IsRequired: false,
                FieldType: "DateTime"
              },
              FullUserGuide: {
                FieldId: 2409,
                FieldName: "FullUserGuide",
                FieldTitle: "Полное руководство пользователя (User guide)",
                FieldDescription: "",
                FieldOrder: 4,
                IsRequired: false,
                FieldType: "File"
              },
              QuickStartGuide: {
                FieldId: 2410,
                FieldName: "QuickStartGuide",
                FieldTitle: "Краткое руководство пользователя (Quick start guide)",
                FieldDescription: "",
                FieldOrder: 5,
                IsRequired: false,
                FieldType: "File"
              }
            }
          },
          FixConnectAction: {
            ContentId: 500,
            ContentPath: "/339:1341/500",
            ContentName: "FixConnectAction",
            ContentTitle: "Акции фиксированной связи",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {
              MarketingOffers: {
                IsBackward: false,
                Content: {
                  $ref: "#/Definitions/MarketingProduct"
                },
                FieldId: 2528,
                FieldName: "MarketingOffers",
                FieldTitle: "",
                FieldDescription: "",
                FieldOrder: 1,
                IsRequired: false,
                FieldType: "M2MRelation"
              },
              PromoPeriod: {
                FieldId: 2472,
                FieldName: "PromoPeriod",
                FieldTitle: "",
                FieldDescription: "Описание промо-периода.",
                FieldOrder: 2,
                IsRequired: false,
                FieldType: "String"
              },
              AfterPromo: {
                FieldId: 2473,
                FieldName: "AfterPromo",
                FieldTitle: "",
                FieldDescription: "Описание момента начала действия обычной цены.",
                FieldOrder: 3,
                IsRequired: false,
                FieldType: "String"
              }
            }
          },
          TvPackage: {
            ContentId: 503,
            ContentPath: "/339:1341/503",
            ContentName: "TvPackage",
            ContentTitle: "ТВ-пакеты",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {}
          },
          FixConnectTariff: {
            ContentId: 505,
            ContentPath: "/339:1341/505",
            ContentName: "FixConnectTariff",
            ContentTitle: "Тарифы фиксированной связи",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {
              TitleForSite: {
                FieldId: 2525,
                FieldName: "TitleForSite",
                FieldTitle: "",
                FieldDescription: "",
                FieldOrder: 2,
                IsRequired: false,
                FieldType: "String"
              }
            }
          },
          PhoneTariff: {
            ContentId: 507,
            ContentPath: "/339:1341/507",
            ContentName: "PhoneTariff",
            ContentTitle: "Тарифы телефонии",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {
              RostelecomLink: {
                FieldId: 2522,
                FieldName: "RostelecomLink",
                FieldTitle: "ВЗ вызовы (ссылка на Ростелеком)",
                FieldDescription: "Тарифы Ростелеком распространяются только на ВЗ вызовы.",
                FieldOrder: 1,
                IsRequired: false,
                FieldType: "String"
              }
            }
          },
          InternetTariff: {
            ContentId: 510,
            ContentPath: "/339:1341/510",
            ContentName: "InternetTariff",
            ContentTitle: "Тарифы Интернет",
            ContentDescription: "",
            ObjectShape: null,
            Fields: {}
          }
        },
        FieldId: 1341,
        FieldName: "Type",
        FieldTitle: "Тип",
        FieldDescription: "",
        FieldOrder: 5,
        IsRequired: false,
        FieldType: "Classifier"
      },
      FixConnectAction: {
        IsBackward: true,
        Content: {
          ContentId: 512,
          ContentPath: "/339:2537/512",
          ContentName: "DevicesForFixConnectAction",
          ContentTitle: "Акционное оборудование",
          ContentDescription: "Оборудование для акций фиксированной связи",
          ObjectShape: null,
          Fields: {
            MarketingDevice: {
              IsBackward: false,
              Content: {
                ContentId: 383,
                ContentPath: "/339:1542/383:1540/504:2519/383",
                ContentName: "MarketingProduct",
                ContentTitle: "Маркетинговые продукты",
                ContentDescription: "",
                ObjectShape: null,
                Fields: {
                  Title: {
                    FieldId: 1534,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "String"
                  },
                  Alias: {
                    FieldId: 1753,
                    FieldName: "Alias",
                    FieldTitle: "Псевдоним",
                    FieldDescription: "",
                    FieldOrder: 2,
                    IsRequired: false,
                    FieldType: "String"
                  },
                  Priority: {
                    FieldId: 2032,
                    FieldName: "Priority",
                    FieldTitle: "Приоритет (популярность)",
                    FieldDescription: "Сортировка по возрастанию значения приоритета",
                    FieldOrder: 19,
                    IsRequired: false,
                    FieldType: "Numeric"
                  }
                }
              },
              FieldId: 2538,
              FieldName: "MarketingDevice",
              FieldTitle: "Маркетинговое оборудование",
              FieldDescription: "",
              FieldOrder: 2,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            Parent: {
              IsBackward: false,
              Content: {
                ContentId: 361,
                ContentPath: "/339:2537/512:2536/361",
                ContentName: "ProductRelation",
                ContentTitle: "Матрица связей",
                ContentDescription: "",
                ObjectShape: null,
                Fields: {
                  Title: {
                    FieldId: 1416,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    IsRequired: false,
                    FieldType: "String"
                  },
                  Parameters: {
                    IsBackward: false,
                    Content: {
                      ContentId: 362,
                      ContentPath: "/339:2537/512:2536/361:1431/362",
                      ContentName: "LinkParameter",
                      ContentTitle: "Параметры связей",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        BaseParameter: {
                          IsBackward: false,
                          Content: {
                            ContentId: 350,
                            ContentPath: "/339:1542/383:1869/424:1854/350",
                            ContentName: "BaseParameter",
                            ContentTitle: "Базовые параметры продуктов",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1358,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1359,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              AllowZone: {
                                FieldId: 2683,
                                FieldName: "AllowZone",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 5,
                                IsRequired: false,
                                FieldType: "Boolean"
                              },
                              AllowDirection: {
                                FieldId: 2684,
                                FieldName: "AllowDirection",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 6,
                                IsRequired: false,
                                FieldType: "Boolean"
                              }
                            }
                          },
                          FieldId: 1420,
                          FieldName: "BaseParameter",
                          FieldTitle: "Базовый параметр",
                          FieldDescription: "",
                          FieldOrder: 7,
                          IsRequired: false,
                          FieldType: "O2MRelation"
                        },
                        Zone: {
                          IsBackward: false,
                          Content: {
                            ContentId: 346,
                            ContentPath: "/339:1542/383:1869/424:1855/346",
                            ContentName: "TariffZone",
                            ContentTitle: "Тарифные зоны",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1346,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1347,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              }
                            }
                          },
                          FieldId: 1421,
                          FieldName: "Zone",
                          FieldTitle: "Зона действия базового параметра",
                          FieldDescription: "",
                          FieldOrder: 8,
                          IsRequired: false,
                          FieldType: "O2MRelation"
                        },
                        Direction: {
                          IsBackward: false,
                          Content: {
                            ContentId: 347,
                            ContentPath: "/339:1542/383:1869/424:1856/347",
                            ContentName: "Direction",
                            ContentTitle: "Направления соединения",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1349,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1350,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              }
                            }
                          },
                          FieldId: 1422,
                          FieldName: "Direction",
                          FieldTitle: "Направление действия базового параметра",
                          FieldDescription: "",
                          FieldOrder: 9,
                          IsRequired: false,
                          FieldType: "O2MRelation"
                        },
                        BaseParameterModifiers: {
                          IsBackward: false,
                          Content: {
                            ContentId: 351,
                            ContentPath: "/339:1542/383:1869/424:1857/351",
                            ContentName: "BaseParameterModifier",
                            ContentTitle: "Модификаторы базовых параметров продуктов",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1361,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1362,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Type: {
                                Items: [
                                  {
                                    Value: "Step",
                                    Alias: "Ступенчатая тарификация",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "Package",
                                    Alias: "Пакет",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "Zone",
                                    Alias: "Зона",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "Direction",
                                    Alias: "Направление",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "Refining",
                                    Alias: "Уточнение",
                                    IsDefault: false,
                                    Invalid: false
                                  }
                                ],
                                FieldId: 1894,
                                FieldName: "Type",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 5,
                                IsRequired: false,
                                FieldType: "StringEnum"
                              }
                            }
                          },
                          FieldId: 1423,
                          FieldName: "BaseParameterModifiers",
                          FieldTitle: "Модификаторы базового параметра",
                          FieldDescription: "",
                          FieldOrder: 10,
                          IsRequired: false,
                          FieldType: "M2MRelation"
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            ContentId: 352,
                            ContentPath: "/339:1542/383:1869/424:1858/352",
                            ContentName: "ParameterModifier",
                            ContentTitle: "Модификаторы параметров продуктов",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Title: {
                                FieldId: 1364,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Alias: {
                                FieldId: 1365,
                                FieldName: "Alias",
                                FieldTitle: "Псевдоним",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              }
                            }
                          },
                          FieldId: 1424,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 11,
                          IsRequired: false,
                          FieldType: "M2MRelation"
                        },
                        Unit: {
                          IsBackward: false,
                          Content: {
                            ContentId: 355,
                            ContentPath: "/339:1542/383:1869/424:1862/355",
                            ContentName: "Unit",
                            ContentTitle: "Единицы измерения",
                            ContentDescription: "",
                            ObjectShape: null,
                            Fields: {
                              Alias: {
                                FieldId: 2062,
                                FieldName: "Alias",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 1,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Title: {
                                FieldId: 1384,
                                FieldName: "Title",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 2,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              Display: {
                                FieldId: 1385,
                                FieldName: "Display",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 3,
                                IsRequired: false,
                                FieldType: "String"
                              },
                              QuotaUnit: {
                                Items: [
                                  {
                                    Value: "mb",
                                    Alias: "Мегабайт",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "gb",
                                    Alias: "Гигабайт",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "kb",
                                    Alias: "Килобайт",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "tb",
                                    Alias: "Терабайт",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "min",
                                    Alias: "Минута",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "message",
                                    Alias: "Сообщение",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "rub",
                                    Alias: "Рублей",
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
                                    Alias: "Мегабит",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "step",
                                    Alias: "Шаг",
                                    IsDefault: false,
                                    Invalid: false
                                  }
                                ],
                                FieldId: 1904,
                                FieldName: "QuotaUnit",
                                FieldTitle: "Размерность",
                                FieldDescription: "",
                                FieldOrder: 4,
                                IsRequired: false,
                                FieldType: "StringEnum"
                              },
                              QuotaPeriod: {
                                Items: [
                                  {
                                    Value: "daily",
                                    Alias: "Сутки",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "weekly",
                                    Alias: "Неделя",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "monthly",
                                    Alias: "Месяц",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "hourly",
                                    Alias: "Час",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "minutely",
                                    Alias: "Минута",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "every_second",
                                    Alias: "Секунда",
                                    IsDefault: false,
                                    Invalid: false
                                  },
                                  {
                                    Value: "annually",
                                    Alias: "Год",
                                    IsDefault: false,
                                    Invalid: false
                                  }
                                ],
                                FieldId: 1905,
                                FieldName: "QuotaPeriod",
                                FieldTitle: "Период",
                                FieldDescription: "",
                                FieldOrder: 5,
                                IsRequired: false,
                                FieldType: "StringEnum"
                              }
                            }
                          },
                          FieldId: 1428,
                          FieldName: "Unit",
                          FieldTitle: "Единица измерения",
                          FieldDescription: "",
                          FieldOrder: 15,
                          IsRequired: false,
                          FieldType: "O2MRelation"
                        },
                        Title: {
                          FieldId: 1418,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        SortOrder: {
                          FieldId: 1425,
                          FieldName: "SortOrder",
                          FieldTitle: "Порядок",
                          FieldDescription: "",
                          FieldOrder: 12,
                          IsRequired: false,
                          FieldType: "Numeric"
                        },
                        NumValue: {
                          FieldId: 1426,
                          FieldName: "NumValue",
                          FieldTitle: "Числовое значение",
                          FieldDescription: "",
                          FieldOrder: 13,
                          IsRequired: false,
                          FieldType: "Numeric"
                        },
                        Value: {
                          FieldId: 1427,
                          FieldName: "Value",
                          FieldTitle: "Текстовое значение",
                          FieldDescription: "",
                          FieldOrder: 14,
                          IsRequired: false,
                          FieldType: "VisualEdit"
                        },
                        Description: {
                          FieldId: 1429,
                          FieldName: "Description",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 16,
                          IsRequired: false,
                          FieldType: "VisualEdit"
                        }
                      }
                    },
                    FieldId: 1431,
                    FieldName: "Parameters",
                    FieldTitle: "Параметры",
                    FieldDescription: "",
                    FieldOrder: 3,
                    IsRequired: false,
                    FieldType: "M2ORelation"
                  },
                  Modifiers: {
                    IsBackward: false,
                    Content: {
                      ContentId: 360,
                      ContentPath: "/339:1542/383:2531/511:2530/361:1450/360",
                      ContentName: "LinkModifier",
                      ContentTitle: "Модификаторы связей",
                      ContentDescription: "",
                      ObjectShape: null,
                      Fields: {
                        Title: {
                          FieldId: 1413,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          IsRequired: false,
                          FieldType: "String"
                        },
                        Alias: {
                          FieldId: 1414,
                          FieldName: "Alias",
                          FieldTitle: "Псевдоним",
                          FieldDescription: "",
                          FieldOrder: 2,
                          IsRequired: false,
                          FieldType: "String"
                        }
                      }
                    },
                    FieldId: 1450,
                    FieldName: "Modifiers",
                    FieldTitle: "Модификаторы",
                    FieldDescription: "",
                    FieldOrder: 4,
                    IsRequired: false,
                    FieldType: "M2MRelation"
                  }
                }
              },
              FieldId: 2536,
              FieldName: "Parent",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 0,
              IsRequired: false,
              FieldType: "O2MRelation"
            },
            Order: {
              FieldId: 2539,
              FieldName: "Order",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 3,
              IsRequired: false,
              FieldType: "Numeric"
            }
          }
        },
        FieldId: 2537,
        FieldName: "FixConnectAction",
        FieldTitle: "Акция фиксированной связи",
        FieldDescription: "",
        FieldOrder: 1,
        IsRequired: false,
        FieldType: "O2MRelation"
      },
      Advantages: {
        IsBackward: false,
        Content: {
          $ref: "#/Definitions/Advantage"
        },
        FieldId: 2133,
        FieldName: "Advantages",
        FieldTitle: "Преимущества",
        FieldDescription: "",
        FieldOrder: 32,
        IsRequired: false,
        FieldType: "M2MRelation"
      }
    }
  },
  Definitions: {
    Segment: {
      ContentId: 416,
      ContentPath: "/339:1542/383:1540/489:2404/416",
      ContentName: "Segment",
      ContentTitle: "Сегменты",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1792,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1793,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    ChannelCategory: {
      ContentId: 478,
      ContentPath: "/339:1542/383:1540/502:2497/482:2283/478",
      ContentName: "ChannelCategory",
      ContentTitle: "Категории каналов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Name: {
          FieldId: 2257,
          FieldName: "Name",
          FieldTitle: "Название для сайта",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 2271,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Segments: {
          FieldId: 2267,
          FieldName: "Segments",
          FieldTitle: "Сегменты",
          FieldDescription: "",
          FieldOrder: 3,
          IsRequired: false,
          FieldType: "String"
        },
        Icon: {
          FieldId: 2269,
          FieldName: "Icon",
          FieldTitle: "Иконка",
          FieldDescription: "",
          FieldOrder: 5,
          IsRequired: false,
          FieldType: "Image"
        },
        Order: {
          FieldId: 2270,
          FieldName: "Order",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          IsRequired: false,
          FieldType: "Numeric"
        },
        OldSiteId: {
          FieldId: 2262,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 7,
          IsRequired: false,
          FieldType: "Numeric"
        }
      }
    },
    Region: {
      ContentId: 290,
      ContentPath: "/339:1542/383:1540/502:2497/482:2286/472:2211/290",
      ContentName: "Region",
      ContentTitle: "Регионы",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Alias: {
          FieldId: 1532,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Parent: {
          IsBackward: false,
          Content: {
            $ref: "#/Definitions/Region2"
          },
          FieldId: 1115,
          FieldName: "Parent",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          IsRequired: false,
          FieldType: "O2MRelation"
        },
        IsMainCity: {
          FieldId: 2239,
          FieldName: "IsMainCity",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 12,
          IsRequired: false,
          FieldType: "Boolean"
        }
      }
    },
    Region1: {
      ContentId: 290,
      ContentPath: "/339:1542/383:2531/511:2533/290",
      ContentName: "Region",
      ContentTitle: "Регионы",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Alias: {
          FieldId: 1532,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Title: {
          FieldId: 1114,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    Region2: {
      ContentId: 290,
      ContentPath: "/339:1542/383:1540/502:2497/482:2286/472:2211/290:1115/290",
      ContentName: "Region",
      ContentTitle: "Регионы",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Alias: {
          FieldId: 1532,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    ChannelFormat: {
      ContentId: 480,
      ContentPath: "/339:1542/383:1540/502:2497/482:2619/482:2524/480",
      ContentName: "ChannelFormat",
      ContentTitle: "Форматы каналов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 2263,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Image: {
          FieldId: 2265,
          FieldName: "Image",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "Image"
        },
        Message: {
          FieldId: 2266,
          FieldName: "Message",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 3,
          IsRequired: false,
          FieldType: "String"
        },
        OldSiteId: {
          FieldId: 2264,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          IsRequired: false,
          FieldType: "Numeric"
        }
      }
    },
    FixedType: {
      ContentId: 491,
      ContentPath: "/339:1542/383:1540/489:2403/493:2402/491",
      ContentName: "FixedType",
      ContentTitle: "Типы фиксированной связи",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 2392,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    MarketingProduct: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/504:2519/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1534,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Priority: {
          FieldId: 2032,
          FieldName: "Priority",
          FieldTitle: "Приоритет (популярность)",
          FieldDescription: "Сортировка по возрастанию значения приоритета",
          FieldOrder: 19,
          IsRequired: false,
          FieldType: "Numeric"
        }
      }
    },
    MarketingProduct1: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/504:2517/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1534,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Link: {
          FieldId: 1755,
          FieldName: "Link",
          FieldTitle: "Ссылка",
          FieldDescription: "",
          FieldOrder: 3,
          IsRequired: false,
          FieldType: "String"
        },
        Description: {
          FieldId: 1558,
          FieldName: "Description",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          IsRequired: false,
          FieldType: "Textbox"
        },
        DetailedDescription: {
          FieldId: 2023,
          FieldName: "DetailedDescription",
          FieldTitle: "Подробное описание",
          FieldDescription: "",
          FieldOrder: 5,
          IsRequired: false,
          FieldType: "VisualEdit"
        },
        FullDescription: {
          FieldId: 1740,
          FieldName: "FullDescription",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          IsRequired: false,
          FieldType: "VisualEdit"
        },
        SortOrder: {
          FieldId: 1752,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 7,
          IsRequired: false,
          FieldType: "Numeric"
        },
        Type: {
          FieldId: 1540,
          FieldName: "Type",
          FieldTitle: "Тип",
          FieldDescription: "",
          FieldOrder: 11,
          IsRequired: false,
          FieldType: "Classifier"
        },
        OldSiteId: {
          FieldId: 1645,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 14,
          IsRequired: false,
          FieldType: "Numeric"
        },
        OldCorpSiteId: {
          FieldId: 1779,
          FieldName: "OldCorpSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 15,
          IsRequired: false,
          FieldType: "Numeric"
        },
        ListImage: {
          FieldId: 2030,
          FieldName: "ListImage",
          FieldTitle: "Изображение в списке",
          FieldDescription: "Изображение в общем списке",
          FieldOrder: 17,
          IsRequired: false,
          FieldType: "Image"
        },
        DetailsImage: {
          FieldId: 2031,
          FieldName: "DetailsImage",
          FieldTitle: "Изображение",
          FieldDescription: "Изображение в описании на странице",
          FieldOrder: 18,
          IsRequired: false,
          FieldType: "Image"
        },
        Priority: {
          FieldId: 2032,
          FieldName: "Priority",
          FieldTitle: "Приоритет (популярность)",
          FieldDescription: "Сортировка по возрастанию значения приоритета",
          FieldOrder: 19,
          IsRequired: false,
          FieldType: "Numeric"
        },
        ArchiveDate: {
          FieldId: 2124,
          FieldName: "ArchiveDate",
          FieldTitle: "Дата закрытия продукта (Архив)",
          FieldDescription: "",
          FieldOrder: 23,
          IsRequired: false,
          FieldType: "Date"
        }
      }
    },
    MarketingProduct2: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/504:2493/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    MarketingProduct3: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/498:2564/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1534,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    BaseParameter: {
      ContentId: 350,
      ContentPath: "/339:1542/383:1869/424:1854/350",
      ContentName: "BaseParameter",
      ContentTitle: "Базовые параметры продуктов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1358,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1359,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        AllowZone: {
          FieldId: 2683,
          FieldName: "AllowZone",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 5,
          IsRequired: false,
          FieldType: "Boolean"
        },
        AllowDirection: {
          FieldId: 2684,
          FieldName: "AllowDirection",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          IsRequired: false,
          FieldType: "Boolean"
        }
      }
    },
    TariffZone: {
      ContentId: 346,
      ContentPath: "/339:1542/383:1869/424:1855/346",
      ContentName: "TariffZone",
      ContentTitle: "Тарифные зоны",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1346,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1347,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    Direction: {
      ContentId: 347,
      ContentPath: "/339:1542/383:1869/424:1856/347",
      ContentName: "Direction",
      ContentTitle: "Направления соединения",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1349,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1350,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    BaseParameterModifier: {
      ContentId: 351,
      ContentPath: "/339:1542/383:1869/424:1857/351",
      ContentName: "BaseParameterModifier",
      ContentTitle: "Модификаторы базовых параметров продуктов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1361,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1362,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Type: {
          Items: [
            {
              Value: "Step",
              Alias: "Ступенчатая тарификация",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "Package",
              Alias: "Пакет",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "Zone",
              Alias: "Зона",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "Direction",
              Alias: "Направление",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "Refining",
              Alias: "Уточнение",
              IsDefault: false,
              Invalid: false
            }
          ],
          FieldId: 1894,
          FieldName: "Type",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 5,
          IsRequired: false,
          FieldType: "StringEnum"
        }
      }
    },
    ParameterModifier: {
      ContentId: 352,
      ContentPath: "/339:1542/383:1869/424:1858/352",
      ContentName: "ParameterModifier",
      ContentTitle: "Модификаторы параметров продуктов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1364,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1365,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    Unit: {
      ContentId: 355,
      ContentPath: "/339:1542/383:1869/424:1862/355",
      ContentName: "Unit",
      ContentTitle: "Единицы измерения",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Alias: {
          FieldId: 2062,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Title: {
          FieldId: 1384,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Display: {
          FieldId: 1385,
          FieldName: "Display",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 3,
          IsRequired: false,
          FieldType: "String"
        },
        QuotaUnit: {
          Items: [
            {
              Value: "mb",
              Alias: "Мегабайт",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "gb",
              Alias: "Гигабайт",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "kb",
              Alias: "Килобайт",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "tb",
              Alias: "Терабайт",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "min",
              Alias: "Минута",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "message",
              Alias: "Сообщение",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "rub",
              Alias: "Рублей",
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
              Alias: "Мегабит",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "step",
              Alias: "Шаг",
              IsDefault: false,
              Invalid: false
            }
          ],
          FieldId: 1904,
          FieldName: "QuotaUnit",
          FieldTitle: "Размерность",
          FieldDescription: "",
          FieldOrder: 4,
          IsRequired: false,
          FieldType: "StringEnum"
        },
        QuotaPeriod: {
          Items: [
            {
              Value: "daily",
              Alias: "Сутки",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "weekly",
              Alias: "Неделя",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "monthly",
              Alias: "Месяц",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "hourly",
              Alias: "Час",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "minutely",
              Alias: "Минута",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "every_second",
              Alias: "Секунда",
              IsDefault: false,
              Invalid: false
            },
            {
              Value: "annually",
              Alias: "Год",
              IsDefault: false,
              Invalid: false
            }
          ],
          FieldId: 1905,
          FieldName: "QuotaPeriod",
          FieldTitle: "Период",
          FieldDescription: "",
          FieldOrder: 5,
          IsRequired: false,
          FieldType: "StringEnum"
        }
      }
    },
    ParameterChoice: {
      ContentId: 488,
      ContentPath: "/339:1542/383:1869/424:2685/488",
      ContentName: "ParameterChoice",
      ContentTitle: "Варианты выбора для параметров",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 2379,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 2380,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        OldSiteId: {
          FieldId: 2382,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          IsRequired: false,
          FieldType: "Numeric"
        }
      }
    },
    ProductParameterGroup: {
      ContentId: 378,
      ContentPath: "/339:1542/383:1869/424:1852/378",
      ContentName: "ProductParameterGroup",
      ContentTitle: "Группы параметров продуктов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        SortOrder: {
          FieldId: 1500,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 3,
          IsRequired: false,
          FieldType: "Numeric"
        },
        Title: {
          FieldId: 1496,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 2049,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        ImageSvg: {
          FieldId: 2029,
          FieldName: "ImageSvg",
          FieldTitle: "Изображение",
          FieldDescription: "",
          FieldOrder: 7,
          IsRequired: false,
          FieldType: "File"
        },
        Type: {
          FieldId: 2061,
          FieldName: "Type",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 8,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    ProductParameterGroup1: {
      ContentId: 378,
      ContentPath: "/339:1542/383:2531/511:2530/361:1431/362:1678/378",
      ContentName: "ProductParameterGroup",
      ContentTitle: "Группы параметров продуктов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1496,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 2049,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        SortOrder: {
          FieldId: 1500,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 3,
          IsRequired: false,
          FieldType: "Numeric"
        },
        ImageSvg: {
          FieldId: 2029,
          FieldName: "ImageSvg",
          FieldTitle: "Изображение",
          FieldDescription: "",
          FieldOrder: 7,
          IsRequired: false,
          FieldType: "File"
        }
      }
    },
    LinkModifier: {
      ContentId: 360,
      ContentPath: "/339:1542/383:2531/511:2530/361:1450/360",
      ContentName: "LinkModifier",
      ContentTitle: "Модификаторы связей",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1413,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1414,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    ProductModifer: {
      ContentId: 342,
      ContentPath: "/339:1542/383:1653/342",
      ContentName: "ProductModifer",
      ContentTitle: "Модификаторы продуктов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 1339,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Alias: {
          FieldId: 1340,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        }
      }
    },
    Advantage: {
      ContentId: 446,
      ContentPath: "/339:1542/383:2028/446",
      ContentName: "Advantage",
      ContentTitle: "Преимущества маркетинговых продуктов",
      ContentDescription: "",
      ObjectShape: null,
      Fields: {
        Title: {
          FieldId: 2024,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          IsRequired: false,
          FieldType: "String"
        },
        Text: {
          FieldId: 2025,
          FieldName: "Text",
          FieldTitle: "Текстовые данные",
          FieldDescription: "",
          FieldOrder: 2,
          IsRequired: false,
          FieldType: "String"
        },
        Description: {
          FieldId: 2362,
          FieldName: "Description",
          FieldTitle: "Описание",
          FieldDescription: "",
          FieldOrder: 3,
          IsRequired: false,
          FieldType: "Textbox"
        },
        ImageSvg: {
          FieldId: 2026,
          FieldName: "ImageSvg",
          FieldTitle: "Изображение",
          FieldDescription: "",
          FieldOrder: 4,
          IsRequired: false,
          FieldType: "File"
        },
        SortOrder: {
          FieldId: 2027,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 5,
          IsRequired: false,
          FieldType: "Numeric"
        },
        IsGift: {
          FieldId: 2514,
          FieldName: "IsGift",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          IsRequired: false,
          FieldType: "Boolean"
        },
        OldSiteId: {
          FieldId: 2515,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 7,
          IsRequired: false,
          FieldType: "Numeric"
        }
      }
    }
  }
});

function linkJsonRefs<T>(schema: any): T {
  const definitions = schema.Definitions;
  delete schema.Definitions;
  visitObject(schema, definitions, new Set());
  return schema.Content;
}

function visitObject(object: any, definitions: object, visited: Set<object>): void {
  if (typeof object === "object" && object !== null) {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveRef(value, definitions);
        visitObject(object[index], definitions, visited);
      });
    } else {
      Object.keys(object).forEach(key => {
        object[key] = resolveRef(object[key], definitions);
        visitObject(object[key], definitions, visited);
      });
      if (object.ContentId) {
        object.ObjectShape = objectShapes[object.ContentId];
        object.include = includeContent;
      } else if (object.FieldId) {
        if (
          object.Content &&
          (object.FieldType === "M2ORelation" ||
            object.FieldType === "O2MRelation" ||
            object.FieldType === "M2MRelation")
        ) {
          object.include = includeRelation;
        } else if (object.Contents && object.FieldType === "Classifier") {
          object.include = includeExtension;
        }
      }
    }
  }
}

function resolveRef(object: any, definitions: object): any {
  return typeof object === "object" && object !== null && "$ref" in object
    ? definitions[object.$ref.slice(14)]
    : object;
}

type Selection = { Content: object } | { Contents: object } | string[];

function includeContent(selector: (fields: object) => Selection[]): string[] {
  const paths = [this.ContentPath];
  selector(this.Fields).forEach((field: any, i) => {
    if (field.Content) {
      paths.push(field.Content.ContentPath);
    } else if (field.Contents) {
      const contentsPaths = Object.keys(field.Contents).map(key => field.Contents[key].ContentPath);
      paths.push(...contentsPaths);
    } else if (Array.isArray(field) && typeof (field[0] === "string")) {
      paths.push(...field);
    } else {
      throw new TypeError("Invalid field selection [" + i + "]: " + field);
    }
  });
  return paths;
}

function includeRelation(selector: (fields: object) => Selection[]): string[] {
  return includeContent.call(this.Content, selector);
}

function includeExtension(selector: (contents: object) => string[][]): string[] {
  const paths = Object.keys(this.Contents).map(key => this.Contents[key].ContentPath);
  selector(this.Contents).forEach((content: any, i) => {
    if (Array.isArray(content) && typeof (content[0] === "string")) {
      paths.push(...content);
    } else {
      throw new TypeError("Invalid content selection [" + i + "]: " + content);
    }
  });
  return paths;
}
