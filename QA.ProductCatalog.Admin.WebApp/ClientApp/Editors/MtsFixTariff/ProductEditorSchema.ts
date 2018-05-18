
/** Описание полей продукта */
export interface ProductSchema {
  ContentId: number,
  ContentPath: string,
  ContentName: "Product",
  ContentTitle: string,
  ContentDescription: string,
  IsExtension: false,
  Fields: {
    MarketingProduct: {
      IsBackward: false,
      Content: {
        ContentId: number,
        ContentPath: string,
        ContentName: "MarketingProduct",
        ContentTitle: string,
        ContentDescription: string,
        IsExtension: false,
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
                IsExtension: true,
                Fields: {}
              },
              MarketingService: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingService",
                ContentTitle: string,
                ContentDescription: string,
                IsExtension: true,
                Fields: {}
              },
              MarketingAction: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingAction",
                ContentTitle: string,
                ContentDescription: string,
                IsExtension: true,
                Fields: {}
              },
              MarketingRoamingScale: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingRoamingScale",
                ContentTitle: string,
                ContentDescription: string,
                IsExtension: true,
                Fields: {}
              },
              MarketingDevice: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingDevice",
                ContentTitle: string,
                ContentDescription: string,
                IsExtension: true,
                Fields: {
                  DeviceType: {
                    IsBackward: false,
                    Content: {
                      ContentId: number,
                      ContentPath: string,
                      ContentName: "EquipmentType",
                      ContentTitle: string,
                      ContentDescription: string,
                      IsExtension: false,
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
                      IsExtension: false,
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
                IsExtension: true,
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
                IsExtension: true,
                Fields: {
                  Channels: {
                    IsBackward: false,
                    Content: {
                      ContentId: number,
                      ContentPath: string,
                      ContentName: "TvChannel",
                      ContentTitle: string,
                      ContentDescription: string,
                      IsExtension: false,
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
                            IsExtension: false,
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
                            IsExtension: false,
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
                            IsExtension: false,
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
                            IsExtension: false,
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
                                  IsExtension: false,
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
                                  IsExtension: false,
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
                            IsExtension: false,
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
                IsExtension: true,
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
                      IsExtension: false,
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
                IsExtension: true,
                Fields: {}
              },
              MarketingInternetTariff: {
                ContentId: number,
                ContentPath: string,
                ContentName: "MarketingInternetTariff",
                ContentTitle: string,
                ContentDescription: string,
                IsExtension: true,
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
              IsExtension: false,
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
              IsExtension: false,
              Fields: {
                Parent: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "ProductRelation",
                    ContentTitle: string,
                    ContentDescription: string,
                    IsExtension: false,
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
                          IsExtension: false,
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
                                IsExtension: false,
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
                            IsExtension: true,
                            Fields: {}
                          },
                          MutualGroup: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "MutualGroup",
                            ContentTitle: string,
                            ContentDescription: string,
                            IsExtension: true,
                            Fields: {}
                          },
                          ServiceOnTariff: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "ServiceOnTariff",
                            ContentTitle: string,
                            ContentDescription: string,
                            IsExtension: true,
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
                            IsExtension: true,
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
                            IsExtension: true,
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
                            IsExtension: true,
                            Fields: {}
                          },
                          RoamingScaleOnTariff: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "RoamingScaleOnTariff",
                            ContentTitle: string,
                            ContentDescription: string,
                            IsExtension: true,
                            Fields: {}
                          },
                          ServiceOnRoamingScale: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "ServiceOnRoamingScale",
                            ContentTitle: string,
                            ContentDescription: string,
                            IsExtension: true,
                            Fields: {}
                          },
                          CrossSale: {
                            ContentId: number,
                            ContentPath: string,
                            ContentName: "CrossSale",
                            ContentTitle: string,
                            ContentDescription: string,
                            IsExtension: true,
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
                            IsExtension: true,
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
                            IsExtension: true,
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
                            IsExtension: true,
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
              IsExtension: false,
              Fields: {
                Parent: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "ProductRelation",
                    ContentTitle: string,
                    ContentDescription: string,
                    IsExtension: false,
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
                          IsExtension: false,
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
              IsExtension: false,
              Fields: {
                FixConnectAction: {
                  IsBackward: false,
                  Content: {
                    ContentId: number,
                    ContentPath: string,
                    ContentName: "Product",
                    ContentTitle: string,
                    ContentDescription: string,
                    IsExtension: false,
                    Fields: {
                      MarketingProduct: {
                        IsBackward: false,
                        Content: {
                          ContentId: number,
                          ContentPath: string,
                          ContentName: "MarketingProduct",
                          ContentTitle: string,
                          ContentDescription: string,
                          IsExtension: false,
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
                    IsExtension: false,
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
                          IsExtension: false,
                          Fields: {
                            Unit: {
                              IsBackward: false,
                              Content: {
                                ContentId: number,
                                ContentPath: string,
                                ContentName: "Unit",
                                ContentTitle: string,
                                ContentDescription: string,
                                IsExtension: false,
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
                                IsExtension: false,
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
        IsExtension: false,
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
              IsExtension: false,
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
              IsExtension: false,
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
        IsExtension: false,
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
          IsExtension: true,
          Fields: {}
        },
        Service: {
          ContentId: number,
          ContentPath: string,
          ContentName: "Service",
          ContentTitle: string,
          ContentDescription: string,
          IsExtension: true,
          Fields: {}
        },
        Action: {
          ContentId: number,
          ContentPath: string,
          ContentName: "Action",
          ContentTitle: string,
          ContentDescription: string,
          IsExtension: true,
          Fields: {}
        },
        RoamingScale: {
          ContentId: number,
          ContentPath: string,
          ContentName: "RoamingScale",
          ContentTitle: string,
          ContentDescription: string,
          IsExtension: true,
          Fields: {}
        },
        Device: {
          ContentId: number,
          ContentPath: string,
          ContentName: "Device",
          ContentTitle: string,
          ContentDescription: string,
          IsExtension: true,
          Fields: {
            Downloads: {
              IsBackward: false,
              Content: {
                ContentId: number,
                ContentPath: string,
                ContentName: "EquipmentDownload",
                ContentTitle: string,
                ContentDescription: string,
                IsExtension: false,
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
                IsExtension: false,
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
          IsExtension: true,
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
          IsExtension: true,
          Fields: {}
        },
        FixConnectTariff: {
          ContentId: number,
          ContentPath: string,
          ContentName: "FixConnectTariff",
          ContentTitle: string,
          ContentDescription: string,
          IsExtension: true,
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
          IsExtension: true,
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
          IsExtension: true,
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
        IsExtension: false,
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
              IsExtension: false,
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
                    IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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
  IsExtension: false,
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

/** Описание полей продукта */
export default linkJsonRefs<ProductSchema>({
  Content: {
    ContentId: 339,
    ContentPath: "/339",
    ContentName: "Product",
    ContentTitle: "Продукты",
    ContentDescription: "",
    IsExtension: false,
    Fields: {
      MarketingProduct: {
        IsBackward: false,
        Content: {
          ContentId: 383,
          ContentPath: "/339:1542/383",
          ContentName: "MarketingProduct",
          ContentTitle: "Маркетинговые продукты",
          ContentDescription: "",
          IsExtension: false,
          Fields: {
            Title: {
              FieldId: 1534,
              FieldName: "Title",
              FieldTitle: "Название",
              FieldDescription: "",
              FieldOrder: 1,
              FieldType: "String",
              IsRequired: false
            },
            Alias: {
              FieldId: 1753,
              FieldName: "Alias",
              FieldTitle: "Псевдоним",
              FieldDescription: "",
              FieldOrder: 2,
              FieldType: "String",
              IsRequired: false
            },
            Description: {
              FieldId: 1558,
              FieldName: "Description",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 4,
              FieldType: "Textbox",
              IsRequired: false
            },
            OldSiteId: {
              FieldId: 1645,
              FieldName: "OldSiteId",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 14,
              FieldType: "Numeric",
              IsRequired: false
            },
            OldCorpSiteId: {
              FieldId: 1779,
              FieldName: "OldCorpSiteId",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 15,
              FieldType: "Numeric",
              IsRequired: false
            },
            ListImage: {
              FieldId: 2030,
              FieldName: "ListImage",
              FieldTitle: "Изображение в списке",
              FieldDescription: "Изображение в общем списке",
              FieldOrder: 17,
              FieldType: "Image",
              IsRequired: false
            },
            DetailsImage: {
              FieldId: 2031,
              FieldName: "DetailsImage",
              FieldTitle: "Изображение",
              FieldDescription: "Изображение в описании на странице",
              FieldOrder: 18,
              FieldType: "Image",
              IsRequired: false
            },
            ArchiveDate: {
              FieldId: 2124,
              FieldName: "ArchiveDate",
              FieldTitle: "Дата закрытия продукта (Архив)",
              FieldDescription: "",
              FieldOrder: 23,
              FieldType: "Date",
              IsRequired: false
            },
            Modifiers: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/ProductModifer"
              },
              FieldId: 1653,
              FieldName: "Modifiers",
              FieldTitle: "Модификаторы",
              FieldDescription: "",
              FieldOrder: 12,
              FieldType: "M2MRelation",
              IsRequired: false
            },
            SortOrder: {
              FieldId: 1752,
              FieldName: "SortOrder",
              FieldTitle: "Порядок",
              FieldDescription: "",
              FieldOrder: 7,
              FieldType: "Numeric",
              IsRequired: false
            },
            Priority: {
              FieldId: 2032,
              FieldName: "Priority",
              FieldTitle: "Приоритет (популярность)",
              FieldDescription: "Сортировка по возрастанию значения приоритета",
              FieldOrder: 19,
              FieldType: "Numeric",
              IsRequired: false
            },
            Advantages: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/Advantage"
              },
              FieldId: 2028,
              FieldName: "Advantages",
              FieldTitle: "Преимущества",
              FieldDescription: "",
              FieldOrder: 16,
              FieldType: "M2MRelation",
              IsRequired: false
            },
            Type: {
              Contents: {
                MarketingTariff: {
                  ContentId: 385,
                  ContentPath: "/339:1542/383:1540/385",
                  ContentName: "MarketingTariff",
                  ContentTitle: "Маркетинговые тарифы",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {}
                },
                MarketingService: {
                  ContentId: 402,
                  ContentPath: "/339:1542/383:1540/402",
                  ContentName: "MarketingService",
                  ContentTitle: "Маркетинговые услуги",
                  ContentDescription: "Универсальная \"опция\". Голосовая, дата и что еще появится.",
                  IsExtension: true,
                  Fields: {}
                },
                MarketingAction: {
                  ContentId: 420,
                  ContentPath: "/339:1542/383:1540/420",
                  ContentName: "MarketingAction",
                  ContentTitle: "Маркетинговые акции",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {}
                },
                MarketingRoamingScale: {
                  ContentId: 435,
                  ContentPath: "/339:1542/383:1540/435",
                  ContentName: "MarketingRoamingScale",
                  ContentTitle: "Маркетинговые роуминговые сетки",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {}
                },
                MarketingDevice: {
                  ContentId: 489,
                  ContentPath: "/339:1542/383:1540/489",
                  ContentName: "MarketingDevice",
                  ContentTitle: "Маркетинговое оборудование",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {
                    DeviceType: {
                      IsBackward: false,
                      Content: {
                        ContentId: 493,
                        ContentPath: "/339:1542/383:1540/489:2403/493",
                        ContentName: "EquipmentType",
                        ContentTitle: "Типы оборудования",
                        ContentDescription: "",
                        IsExtension: false,
                        Fields: {
                          ConnectionType: {
                            IsBackward: false,
                            Content: {
                              "$ref": "#/Definitions/FixedType"
                            },
                            FieldId: 2402,
                            FieldName: "ConnectionType",
                            FieldTitle: "Тип связи",
                            FieldDescription: "",
                            FieldOrder: 5,
                            FieldType: "O2MRelation",
                            IsRequired: false
                          },
                          Title: {
                            FieldId: 2399,
                            FieldName: "Title",
                            FieldTitle: "",
                            FieldDescription: "",
                            FieldOrder: 1,
                            FieldType: "String",
                            IsRequired: false
                          },
                          Alias: {
                            FieldId: 2400,
                            FieldName: "Alias",
                            FieldTitle: "",
                            FieldDescription: "",
                            FieldOrder: 2,
                            FieldType: "String",
                            IsRequired: false
                          },
                          Order: {
                            FieldId: 2648,
                            FieldName: "Order",
                            FieldTitle: "Порядок",
                            FieldDescription: "",
                            FieldOrder: 3,
                            FieldType: "Numeric",
                            IsRequired: false
                          }
                        }
                      },
                      FieldId: 2403,
                      FieldName: "DeviceType",
                      FieldTitle: "Тип оборудования",
                      FieldDescription: "",
                      FieldOrder: 2,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    },
                    Segments: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/Segment"
                      },
                      FieldId: 2404,
                      FieldName: "Segments",
                      FieldTitle: "Сегменты",
                      FieldDescription: "",
                      FieldOrder: 3,
                      FieldType: "M2MRelation",
                      IsRequired: false
                    },
                    CommunicationType: {
                      IsBackward: false,
                      Content: {
                        ContentId: 415,
                        ContentPath: "/339:1542/383:1540/489:2509/415",
                        ContentName: "CommunicationType",
                        ContentTitle: "Виды связи",
                        ContentDescription: "",
                        IsExtension: false,
                        Fields: {
                          Title: {
                            FieldId: 1789,
                            FieldName: "Title",
                            FieldTitle: "",
                            FieldDescription: "",
                            FieldOrder: 1,
                            FieldType: "String",
                            IsRequired: false
                          },
                          Alias: {
                            FieldId: 1791,
                            FieldName: "Alias",
                            FieldTitle: "Псевдоним",
                            FieldDescription: "",
                            FieldOrder: 2,
                            FieldType: "String",
                            IsRequired: false
                          }
                        }
                      },
                      FieldId: 2509,
                      FieldName: "CommunicationType",
                      FieldTitle: "Вид связи",
                      FieldDescription: "",
                      FieldOrder: 4,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    }
                  }
                },
                MarketingFixConnectAction: {
                  ContentId: 498,
                  ContentPath: "/339:1542/383:1540/498",
                  ContentName: "MarketingFixConnectAction",
                  ContentTitle: "Маркетинговые акции фиксированной связи",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {
                    Segment: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/Segment"
                      },
                      FieldId: 2458,
                      FieldName: "Segment",
                      FieldTitle: "Сегмент",
                      FieldDescription: "",
                      FieldOrder: 2,
                      FieldType: "M2MRelation",
                      IsRequired: false
                    },
                    MarketingAction: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/MarketingProduct3"
                      },
                      FieldId: 2564,
                      FieldName: "MarketingAction",
                      FieldTitle: "Акция в Каталоге акций",
                      FieldDescription: "",
                      FieldOrder: 7,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    },
                    StartDate: {
                      FieldId: 2459,
                      FieldName: "StartDate",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 3,
                      FieldType: "Date",
                      IsRequired: false
                    },
                    EndDate: {
                      FieldId: 2460,
                      FieldName: "EndDate",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 4,
                      FieldType: "Date",
                      IsRequired: false
                    },
                    PromoPeriod: {
                      FieldId: 2461,
                      FieldName: "PromoPeriod",
                      FieldTitle: "",
                      FieldDescription: "Описание промо-периода.",
                      FieldOrder: 5,
                      FieldType: "String",
                      IsRequired: false
                    },
                    AfterPromo: {
                      FieldId: 2462,
                      FieldName: "AfterPromo",
                      FieldTitle: "",
                      FieldDescription: "Описание момента начала действия обычной цены.",
                      FieldOrder: 6,
                      FieldType: "String",
                      IsRequired: false
                    }
                  }
                },
                MarketingTvPackage: {
                  ContentId: 502,
                  ContentPath: "/339:1542/383:1540/502",
                  ContentName: "MarketingTvPackage",
                  ContentTitle: "Маркетинговые ТВ-пакеты",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {
                    Channels: {
                      IsBackward: false,
                      Content: {
                        ContentId: 482,
                        ContentPath: "/339:1542/383:1540/502:2497/482",
                        ContentName: "TvChannel",
                        ContentTitle: "ТВ-каналы",
                        ContentDescription: "",
                        IsExtension: false,
                        Fields: {
                          Title: {
                            FieldId: 2274,
                            FieldName: "Title",
                            FieldTitle: "Название телеканала",
                            FieldDescription: "title",
                            FieldOrder: 1,
                            FieldType: "String",
                            IsRequired: false
                          },
                          ShortDescription: {
                            FieldId: 2281,
                            FieldName: "ShortDescription",
                            FieldTitle: "Короткое описание",
                            FieldDescription: "short_descr",
                            FieldOrder: 7,
                            FieldType: "Textbox",
                            IsRequired: false
                          },
                          Logo150: {
                            FieldId: 2306,
                            FieldName: "Logo150",
                            FieldTitle: "Лого 150x150",
                            FieldDescription: "logo150",
                            FieldOrder: 32,
                            FieldType: "Image",
                            IsRequired: false
                          },
                          IsRegional: {
                            FieldId: 2298,
                            FieldName: "IsRegional",
                            FieldTitle: "Регионал. канал",
                            FieldDescription: "regional_tv",
                            FieldOrder: 24,
                            FieldType: "Boolean",
                            IsRequired: false
                          },
                          Parent: {
                            IsBackward: false,
                            Content: {
                              ContentId: 482,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2287/482",
                              ContentName: "TvChannel",
                              ContentTitle: "ТВ-каналы",
                              ContentDescription: "",
                              IsExtension: false,
                              Fields: {
                                Logo150: {
                                  FieldId: 2306,
                                  FieldName: "Logo150",
                                  FieldTitle: "Лого 150x150",
                                  FieldDescription: "logo150",
                                  FieldOrder: 32,
                                  FieldType: "Image",
                                  IsRequired: false
                                }
                              }
                            },
                            FieldId: 2287,
                            FieldName: "Parent",
                            FieldTitle: "Родительский канал",
                            FieldDescription: "ch_parent",
                            FieldOrder: 13,
                            FieldType: "O2MRelation",
                            IsRequired: false
                          },
                          Cities: {
                            IsBackward: false,
                            Content: {
                              ContentId: 472,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2286/472",
                              ContentName: "NetworkCity",
                              ContentTitle: "Города сети",
                              ContentDescription: "",
                              IsExtension: false,
                              Fields: {
                                City: {
                                  IsBackward: false,
                                  Content: {
                                    "$ref": "#/Definitions/Region"
                                  },
                                  FieldId: 2211,
                                  FieldName: "City",
                                  FieldTitle: "Город",
                                  FieldDescription: "",
                                  FieldOrder: 1,
                                  FieldType: "O2MRelation",
                                  IsRequired: false
                                },
                                HasIpTv: {
                                  FieldId: 2218,
                                  FieldName: "HasIpTv",
                                  FieldTitle: "IPTV",
                                  FieldDescription: "Города где есть IPTV",
                                  FieldOrder: 9,
                                  FieldType: "Boolean",
                                  IsRequired: false
                                }
                              }
                            },
                            FieldId: 2286,
                            FieldName: "Cities",
                            FieldTitle: "Города вещания",
                            FieldDescription: "cities",
                            FieldOrder: 12,
                            FieldType: "M2MRelation",
                            IsRequired: false
                          },
                          ChannelType: {
                            IsBackward: false,
                            Content: {
                              ContentId: 479,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2285/479",
                              ContentName: "ChannelType",
                              ContentTitle: "Типы каналов",
                              ContentDescription: "",
                              IsExtension: false,
                              Fields: {
                                Title: {
                                  FieldId: 2258,
                                  FieldName: "Title",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 1,
                                  FieldType: "String",
                                  IsRequired: false
                                }
                              }
                            },
                            FieldId: 2285,
                            FieldName: "ChannelType",
                            FieldTitle: "Тип канала",
                            FieldDescription: "ch_type",
                            FieldOrder: 11,
                            FieldType: "O2MRelation",
                            IsRequired: false
                          },
                          Category: {
                            IsBackward: false,
                            Content: {
                              "$ref": "#/Definitions/ChannelCategory"
                            },
                            FieldId: 2283,
                            FieldName: "Category",
                            FieldTitle: "Основная категория телеканала",
                            FieldDescription: "ch_category",
                            FieldOrder: 9,
                            FieldType: "O2MRelation",
                            IsRequired: false
                          },
                          IsMtsMsk: {
                            FieldId: 2297,
                            FieldName: "IsMtsMsk",
                            FieldTitle: "МТС Москва",
                            FieldDescription: "test_inMSK_mgts_XML",
                            FieldOrder: 23,
                            FieldType: "Boolean",
                            IsRequired: false
                          },
                          LcnDvbC: {
                            FieldId: 2312,
                            FieldName: "LcnDvbC",
                            FieldTitle: "LCN DVB-C",
                            FieldDescription: "lcn_dvbc",
                            FieldOrder: 36,
                            FieldType: "Numeric",
                            IsRequired: false
                          },
                          LcnIpTv: {
                            FieldId: 2314,
                            FieldName: "LcnIpTv",
                            FieldTitle: "LCN IPTV",
                            FieldDescription: "lcn_iptv",
                            FieldOrder: 37,
                            FieldType: "Numeric",
                            IsRequired: false
                          },
                          LcnDvbS: {
                            FieldId: 2313,
                            FieldName: "LcnDvbS",
                            FieldTitle: "LCN DVB-S",
                            FieldDescription: "lcn_dvbs",
                            FieldOrder: 38,
                            FieldType: "Numeric",
                            IsRequired: false
                          },
                          Disabled: {
                            FieldId: 2289,
                            FieldName: "Disabled",
                            FieldTitle: "Приостановлено вещание",
                            FieldDescription: "offair",
                            FieldOrder: 17,
                            FieldType: "Boolean",
                            IsRequired: false
                          },
                          Children: {
                            IsBackward: false,
                            Content: {
                              ContentId: 482,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2619/482",
                              ContentName: "TvChannel",
                              ContentTitle: "ТВ-каналы",
                              ContentDescription: "",
                              IsExtension: false,
                              Fields: {
                                Title: {
                                  FieldId: 2274,
                                  FieldName: "Title",
                                  FieldTitle: "Название телеканала",
                                  FieldDescription: "title",
                                  FieldOrder: 1,
                                  FieldType: "String",
                                  IsRequired: false
                                },
                                Category: {
                                  IsBackward: false,
                                  Content: {
                                    "$ref": "#/Definitions/ChannelCategory"
                                  },
                                  FieldId: 2283,
                                  FieldName: "Category",
                                  FieldTitle: "Основная категория телеканала",
                                  FieldDescription: "ch_category",
                                  FieldOrder: 9,
                                  FieldType: "O2MRelation",
                                  IsRequired: false
                                },
                                ChannelType: {
                                  IsBackward: false,
                                  Content: {
                                    ContentId: 479,
                                    ContentPath: "/339:1542/383:1540/502:2497/482:2619/482:2285/479",
                                    ContentName: "ChannelType",
                                    ContentTitle: "Типы каналов",
                                    ContentDescription: "",
                                    IsExtension: false,
                                    Fields: {
                                      Title: {
                                        FieldId: 2258,
                                        FieldName: "Title",
                                        FieldTitle: "",
                                        FieldDescription: "",
                                        FieldOrder: 1,
                                        FieldType: "String",
                                        IsRequired: false
                                      },
                                      OldSiteId: {
                                        FieldId: 2261,
                                        FieldName: "OldSiteId",
                                        FieldTitle: "",
                                        FieldDescription: "",
                                        FieldOrder: 2,
                                        FieldType: "Numeric",
                                        IsRequired: false
                                      }
                                    }
                                  },
                                  FieldId: 2285,
                                  FieldName: "ChannelType",
                                  FieldTitle: "Тип канала",
                                  FieldDescription: "ch_type",
                                  FieldOrder: 11,
                                  FieldType: "O2MRelation",
                                  IsRequired: false
                                },
                                ShortDescription: {
                                  FieldId: 2281,
                                  FieldName: "ShortDescription",
                                  FieldTitle: "Короткое описание",
                                  FieldDescription: "short_descr",
                                  FieldOrder: 7,
                                  FieldType: "Textbox",
                                  IsRequired: false
                                },
                                Cities: {
                                  IsBackward: false,
                                  Content: {
                                    ContentId: 472,
                                    ContentPath: "/339:1542/383:1540/502:2497/482:2619/482:2286/472",
                                    ContentName: "NetworkCity",
                                    ContentTitle: "Города сети",
                                    ContentDescription: "",
                                    IsExtension: false,
                                    Fields: {
                                      City: {
                                        IsBackward: false,
                                        Content: {
                                          "$ref": "#/Definitions/Region"
                                        },
                                        FieldId: 2211,
                                        FieldName: "City",
                                        FieldTitle: "Город",
                                        FieldDescription: "",
                                        FieldOrder: 1,
                                        FieldType: "O2MRelation",
                                        IsRequired: false
                                      }
                                    }
                                  },
                                  FieldId: 2286,
                                  FieldName: "Cities",
                                  FieldTitle: "Города вещания",
                                  FieldDescription: "cities",
                                  FieldOrder: 12,
                                  FieldType: "M2MRelation",
                                  IsRequired: false
                                },
                                Disabled: {
                                  FieldId: 2289,
                                  FieldName: "Disabled",
                                  FieldTitle: "Приостановлено вещание",
                                  FieldDescription: "offair",
                                  FieldOrder: 17,
                                  FieldType: "Boolean",
                                  IsRequired: false
                                },
                                IsMtsMsk: {
                                  FieldId: 2297,
                                  FieldName: "IsMtsMsk",
                                  FieldTitle: "МТС Москва",
                                  FieldDescription: "test_inMSK_mgts_XML",
                                  FieldOrder: 23,
                                  FieldType: "Boolean",
                                  IsRequired: false
                                },
                                IsRegional: {
                                  FieldId: 2298,
                                  FieldName: "IsRegional",
                                  FieldTitle: "Регионал. канал",
                                  FieldDescription: "regional_tv",
                                  FieldOrder: 24,
                                  FieldType: "Boolean",
                                  IsRequired: false
                                },
                                Logo150: {
                                  FieldId: 2306,
                                  FieldName: "Logo150",
                                  FieldTitle: "Лого 150x150",
                                  FieldDescription: "logo150",
                                  FieldOrder: 32,
                                  FieldType: "Image",
                                  IsRequired: false
                                },
                                LcnDvbC: {
                                  FieldId: 2312,
                                  FieldName: "LcnDvbC",
                                  FieldTitle: "LCN DVB-C",
                                  FieldDescription: "lcn_dvbc",
                                  FieldOrder: 36,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                },
                                LcnIpTv: {
                                  FieldId: 2314,
                                  FieldName: "LcnIpTv",
                                  FieldTitle: "LCN IPTV",
                                  FieldDescription: "lcn_iptv",
                                  FieldOrder: 37,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                },
                                LcnDvbS: {
                                  FieldId: 2313,
                                  FieldName: "LcnDvbS",
                                  FieldTitle: "LCN DVB-S",
                                  FieldDescription: "lcn_dvbs",
                                  FieldOrder: 38,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                },
                                Format: {
                                  IsBackward: false,
                                  Content: {
                                    "$ref": "#/Definitions/ChannelFormat"
                                  },
                                  FieldId: 2524,
                                  FieldName: "Format",
                                  FieldTitle: "Формат",
                                  FieldDescription: "",
                                  FieldOrder: 16,
                                  FieldType: "O2MRelation",
                                  IsRequired: false
                                }
                              }
                            },
                            FieldId: 2619,
                            FieldName: "Children",
                            FieldTitle: "Дочерние каналы",
                            FieldDescription: "",
                            FieldOrder: 14,
                            FieldType: "M2ORelation",
                            IsRequired: false
                          },
                          Format: {
                            IsBackward: false,
                            Content: {
                              "$ref": "#/Definitions/ChannelFormat"
                            },
                            FieldId: 2524,
                            FieldName: "Format",
                            FieldTitle: "Формат",
                            FieldDescription: "",
                            FieldOrder: 16,
                            FieldType: "O2MRelation",
                            IsRequired: false
                          },
                          Logo40x30: {
                            FieldId: 2303,
                            FieldName: "Logo40x30",
                            FieldTitle: "Лого 40х30",
                            FieldDescription: "logo40x30",
                            FieldOrder: 29,
                            FieldType: "Image",
                            IsRequired: false
                          },
                          TimeZone: {
                            IsBackward: false,
                            Content: {
                              ContentId: 471,
                              ContentPath: "/339:1542/383:1540/502:2497/482:2288/471",
                              ContentName: "TimeZone",
                              ContentTitle: "Часовые зоны",
                              ContentDescription: "",
                              IsExtension: false,
                              Fields: {
                                Name: {
                                  FieldId: 2203,
                                  FieldName: "Name",
                                  FieldTitle: "Название часовой зоны",
                                  FieldDescription: "",
                                  FieldOrder: 1,
                                  FieldType: "String",
                                  IsRequired: false
                                },
                                Code: {
                                  FieldId: 2204,
                                  FieldName: "Code",
                                  FieldTitle: "Код зоны",
                                  FieldDescription: "",
                                  FieldOrder: 2,
                                  FieldType: "String",
                                  IsRequired: false
                                },
                                UTC: {
                                  FieldId: 2205,
                                  FieldName: "UTC",
                                  FieldTitle: "Значение по UTC",
                                  FieldDescription: "",
                                  FieldOrder: 3,
                                  FieldType: "String",
                                  IsRequired: false
                                },
                                MSK: {
                                  FieldId: 2206,
                                  FieldName: "MSK",
                                  FieldTitle: "Значение от Московского времени",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  FieldType: "String",
                                  IsRequired: false
                                },
                                OldSiteId: {
                                  FieldId: 2207,
                                  FieldName: "OldSiteId",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 5,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                }
                              }
                            },
                            FieldId: 2288,
                            FieldName: "TimeZone",
                            FieldTitle: "Часовая зона (UTC)",
                            FieldDescription: "utc_tz\ndesc",
                            FieldOrder: 15,
                            FieldType: "O2MRelation",
                            IsRequired: false
                          }
                        }
                      },
                      FieldId: 2497,
                      FieldName: "Channels",
                      FieldTitle: "Каналы",
                      FieldDescription: "",
                      FieldOrder: 4,
                      FieldType: "M2MRelation",
                      IsRequired: false
                    },
                    TitleForSite: {
                      FieldId: 2482,
                      FieldName: "TitleForSite",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 2,
                      FieldType: "String",
                      IsRequired: false
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
                      FieldType: "StringEnum",
                      IsRequired: false
                    }
                  }
                },
                MarketingFixConnectTariff: {
                  ContentId: 504,
                  ContentPath: "/339:1542/383:1540/504",
                  ContentName: "MarketingFixConnectTariff",
                  ContentTitle: "Маркетинговые тарифы фиксированной связи",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {
                    Segment: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/Segment"
                      },
                      FieldId: 2492,
                      FieldName: "Segment",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 2,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    },
                    Category: {
                      IsBackward: false,
                      Content: {
                        ContentId: 441,
                        ContentPath: "/339:1542/383:1540/504:2494/441",
                        ContentName: "TariffCategory",
                        ContentTitle: "Категории тарифов",
                        ContentDescription: "",
                        IsExtension: false,
                        Fields: {
                          ConnectionTypes: {
                            IsBackward: false,
                            Content: {
                              "$ref": "#/Definitions/FixedType"
                            },
                            FieldId: 2449,
                            FieldName: "ConnectionTypes",
                            FieldTitle: "Типы связи",
                            FieldDescription: "",
                            FieldOrder: 10,
                            FieldType: "M2MRelation",
                            IsRequired: false
                          },
                          Title: {
                            FieldId: 1989,
                            FieldName: "Title",
                            FieldTitle: "Название",
                            FieldDescription: "",
                            FieldOrder: 1,
                            FieldType: "String",
                            IsRequired: false
                          },
                          Alias: {
                            FieldId: 1990,
                            FieldName: "Alias",
                            FieldTitle: "Алиас",
                            FieldDescription: "",
                            FieldOrder: 2,
                            FieldType: "String",
                            IsRequired: false
                          },
                          Image: {
                            FieldId: 1991,
                            FieldName: "Image",
                            FieldTitle: "Картинка",
                            FieldDescription: "",
                            FieldOrder: 3,
                            FieldType: "Image",
                            IsRequired: false
                          },
                          Order: {
                            FieldId: 2001,
                            FieldName: "Order",
                            FieldTitle: "Порядок",
                            FieldDescription: "",
                            FieldOrder: 7,
                            FieldType: "Numeric",
                            IsRequired: false
                          },
                          ImageSvg: {
                            FieldId: 2020,
                            FieldName: "ImageSvg",
                            FieldTitle: "Векторное изображение",
                            FieldDescription: "",
                            FieldOrder: 9,
                            FieldType: "File",
                            IsRequired: false
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
                            FieldType: "StringEnum",
                            IsRequired: false
                          }
                        }
                      },
                      FieldId: 2494,
                      FieldName: "Category",
                      FieldTitle: "Тип предложения (Категория тарифа)",
                      FieldDescription: "",
                      FieldOrder: 3,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    },
                    MarketingDevices: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/MarketingProduct"
                      },
                      FieldId: 2519,
                      FieldName: "MarketingDevices",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 8,
                      FieldType: "M2MRelation",
                      IsRequired: false
                    },
                    BonusTVPackages: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/MarketingProduct"
                      },
                      FieldId: 2518,
                      FieldName: "BonusTVPackages",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 7,
                      FieldType: "M2MRelation",
                      IsRequired: false
                    },
                    MarketingPhoneTariff: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/MarketingProduct1"
                      },
                      FieldId: 2517,
                      FieldName: "MarketingPhoneTariff",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 6,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    },
                    MarketingInternetTariff: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/MarketingProduct1"
                      },
                      FieldId: 2516,
                      FieldName: "MarketingInternetTariff",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 5,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    },
                    MarketingTvPackage: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/MarketingProduct2"
                      },
                      FieldId: 2493,
                      FieldName: "MarketingTvPackage",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 4,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    },
                    TitleForSite: {
                      FieldId: 2491,
                      FieldName: "TitleForSite",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 1,
                      FieldType: "String",
                      IsRequired: false
                    }
                  }
                },
                MarketingPhoneTariff: {
                  ContentId: 506,
                  ContentPath: "/339:1542/383:1540/506",
                  ContentName: "MarketingPhoneTariff",
                  ContentTitle: "Маркетинговые тарифы телефонии",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {}
                },
                MarketingInternetTariff: {
                  ContentId: 509,
                  ContentPath: "/339:1542/383:1540/509",
                  ContentName: "MarketingInternetTariff",
                  ContentTitle: "Маркетинговые тарифы интернет",
                  ContentDescription: "",
                  IsExtension: true,
                  Fields: {}
                }
              },
              FieldId: 1540,
              FieldName: "Type",
              FieldTitle: "Тип",
              FieldDescription: "",
              FieldOrder: 11,
              FieldType: "Classifier",
              IsRequired: false
            },
            FullDescription: {
              FieldId: 1740,
              FieldName: "FullDescription",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 6,
              FieldType: "VisualEdit",
              IsRequired: false
            },
            Parameters: {
              IsBackward: false,
              Content: {
                ContentId: 424,
                ContentPath: "/339:1542/383:1869/424",
                ContentName: "MarketingProductParameter",
                ContentTitle: "Параметры маркетинговых продуктов",
                ContentDescription: "",
                IsExtension: false,
                Fields: {
                  Group: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/ProductParameterGroup"
                    },
                    FieldId: 1852,
                    FieldName: "Group",
                    FieldTitle: "Группа параметров",
                    FieldDescription: "",
                    FieldOrder: 3,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  BaseParameter: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/BaseParameter"
                    },
                    FieldId: 1854,
                    FieldName: "BaseParameter",
                    FieldTitle: "Базовый параметр",
                    FieldDescription: "",
                    FieldOrder: 5,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Zone: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/TariffZone"
                    },
                    FieldId: 1855,
                    FieldName: "Zone",
                    FieldTitle: "Зона действия базового параметра",
                    FieldDescription: "",
                    FieldOrder: 6,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Direction: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/Direction"
                    },
                    FieldId: 1856,
                    FieldName: "Direction",
                    FieldTitle: "Направление действия базового параметра",
                    FieldDescription: "",
                    FieldOrder: 7,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  BaseParameterModifiers: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/BaseParameterModifier"
                    },
                    FieldId: 1857,
                    FieldName: "BaseParameterModifiers",
                    FieldTitle: "Модификаторы базового параметра",
                    FieldDescription: "",
                    FieldOrder: 8,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  Modifiers: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/ParameterModifier"
                    },
                    FieldId: 1858,
                    FieldName: "Modifiers",
                    FieldTitle: "Модификаторы",
                    FieldDescription: "",
                    FieldOrder: 9,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  Unit: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/Unit"
                    },
                    FieldId: 1862,
                    FieldName: "Unit",
                    FieldTitle: "Единица измерения",
                    FieldDescription: "",
                    FieldOrder: 13,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Choice: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/ParameterChoice"
                    },
                    FieldId: 2685,
                    FieldName: "Choice",
                    FieldTitle: "Выбор",
                    FieldDescription: "",
                    FieldOrder: 15,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Title: {
                    FieldId: 1849,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    FieldType: "String",
                    IsRequired: false
                  },
                  SortOrder: {
                    FieldId: 1859,
                    FieldName: "SortOrder",
                    FieldTitle: "Порядок",
                    FieldDescription: "",
                    FieldOrder: 10,
                    FieldType: "Numeric",
                    IsRequired: false
                  },
                  NumValue: {
                    FieldId: 1860,
                    FieldName: "NumValue",
                    FieldTitle: "Числовое значение",
                    FieldDescription: "",
                    FieldOrder: 11,
                    FieldType: "Numeric",
                    IsRequired: false
                  },
                  Value: {
                    FieldId: 1861,
                    FieldName: "Value",
                    FieldTitle: "Текстовое значение",
                    FieldDescription: "",
                    FieldOrder: 12,
                    FieldType: "VisualEdit",
                    IsRequired: false
                  },
                  Description: {
                    FieldId: 1863,
                    FieldName: "Description",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 14,
                    FieldType: "VisualEdit",
                    IsRequired: false
                  }
                }
              },
              FieldId: 1869,
              FieldName: "Parameters",
              FieldTitle: "Параметры маркетингового продукта",
              FieldDescription: "",
              FieldOrder: 9,
              FieldType: "M2ORelation",
              IsRequired: false
            },
            TariffsOnMarketingDevice: {
              IsBackward: true,
              Content: {
                ContentId: 511,
                ContentPath: "/339:1542/383:2531/511",
                ContentName: "DeviceOnTariffs",
                ContentTitle: "Оборудование на тарифах",
                ContentDescription: "",
                IsExtension: false,
                Fields: {
                  Parent: {
                    IsBackward: false,
                    Content: {
                      ContentId: 361,
                      ContentPath: "/339:1542/383:2531/511:2530/361",
                      ContentName: "ProductRelation",
                      ContentTitle: "Матрица связей",
                      ContentDescription: "",
                      IsExtension: false,
                      Fields: {
                        Title: {
                          FieldId: 1416,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/LinkModifier"
                          },
                          FieldId: 1450,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 4,
                          FieldType: "M2MRelation",
                          IsRequired: false
                        },
                        Parameters: {
                          IsBackward: false,
                          Content: {
                            ContentId: 362,
                            ContentPath: "/339:1542/383:2531/511:2530/361:1431/362",
                            ContentName: "LinkParameter",
                            ContentTitle: "Параметры связей",
                            ContentDescription: "",
                            IsExtension: false,
                            Fields: {
                              Title: {
                                FieldId: 1418,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                FieldType: "String",
                                IsRequired: false
                              },
                              Group: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ProductParameterGroup1"
                                },
                                FieldId: 1678,
                                FieldName: "Group",
                                FieldTitle: "Группа параметров",
                                FieldDescription: "",
                                FieldOrder: 3,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              BaseParameter: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/BaseParameter"
                                },
                                FieldId: 1420,
                                FieldName: "BaseParameter",
                                FieldTitle: "Базовый параметр",
                                FieldDescription: "",
                                FieldOrder: 7,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Zone: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/TariffZone"
                                },
                                FieldId: 1421,
                                FieldName: "Zone",
                                FieldTitle: "Зона действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 8,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Direction: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/Direction"
                                },
                                FieldId: 1422,
                                FieldName: "Direction",
                                FieldTitle: "Направление действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 9,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              BaseParameterModifiers: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/BaseParameterModifier"
                                },
                                FieldId: 1423,
                                FieldName: "BaseParameterModifiers",
                                FieldTitle: "Модификаторы базового параметра",
                                FieldDescription: "",
                                FieldOrder: 10,
                                FieldType: "M2MRelation",
                                IsRequired: false
                              },
                              Modifiers: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ParameterModifier"
                                },
                                FieldId: 1424,
                                FieldName: "Modifiers",
                                FieldTitle: "Модификаторы",
                                FieldDescription: "",
                                FieldOrder: 11,
                                FieldType: "M2MRelation",
                                IsRequired: false
                              },
                              SortOrder: {
                                FieldId: 1425,
                                FieldName: "SortOrder",
                                FieldTitle: "Порядок",
                                FieldDescription: "",
                                FieldOrder: 12,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              NumValue: {
                                FieldId: 1426,
                                FieldName: "NumValue",
                                FieldTitle: "Числовое значение",
                                FieldDescription: "",
                                FieldOrder: 13,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              Value: {
                                FieldId: 1427,
                                FieldName: "Value",
                                FieldTitle: "Текстовое значение",
                                FieldDescription: "",
                                FieldOrder: 14,
                                FieldType: "VisualEdit",
                                IsRequired: false
                              },
                              Description: {
                                FieldId: 1429,
                                FieldName: "Description",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 16,
                                FieldType: "VisualEdit",
                                IsRequired: false
                              },
                              Unit: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/Unit"
                                },
                                FieldId: 1428,
                                FieldName: "Unit",
                                FieldTitle: "Единица измерения",
                                FieldDescription: "",
                                FieldOrder: 15,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              ProductGroup: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 340,
                                  ContentPath: "/339:1542/383:2531/511:2530/361:1431/362:1657/340",
                                  ContentName: "Group",
                                  ContentTitle: "Группы продуктов",
                                  ContentDescription: "",
                                  IsExtension: false,
                                  Fields: {}
                                },
                                FieldId: 1657,
                                FieldName: "ProductGroup",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 19,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Choice: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ParameterChoice"
                                },
                                FieldId: 2687,
                                FieldName: "Choice",
                                FieldTitle: "Выбор",
                                FieldDescription: "",
                                FieldOrder: 17,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              }
                            }
                          },
                          FieldId: 1431,
                          FieldName: "Parameters",
                          FieldTitle: "Параметры",
                          FieldDescription: "",
                          FieldOrder: 3,
                          FieldType: "M2ORelation",
                          IsRequired: false
                        },
                        Type: {
                          Contents: {
                            TariffTransfer: {
                              ContentId: 364,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/364",
                              ContentName: "TariffTransfer",
                              ContentTitle: "Переходы с тарифа на тариф",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {}
                            },
                            MutualGroup: {
                              ContentId: 365,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/365",
                              ContentName: "MutualGroup",
                              ContentTitle: "Группы несовместимости услуг",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {}
                            },
                            ServiceOnTariff: {
                              ContentId: 404,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/404",
                              ContentName: "ServiceOnTariff",
                              ContentTitle: "Услуги на тарифе",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {
                                Description: {
                                  FieldId: 2044,
                                  FieldName: "Description",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  FieldType: "Textbox",
                                  IsRequired: false
                                }
                              }
                            },
                            ServicesUpsale: {
                              ContentId: 406,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/406",
                              ContentName: "ServicesUpsale",
                              ContentTitle: "Матрица предложений услуг Upsale",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {
                                Order: {
                                  FieldId: 1700,
                                  FieldName: "Order",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                }
                              }
                            },
                            TariffOptionPackage: {
                              ContentId: 407,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/407",
                              ContentName: "TariffOptionPackage",
                              ContentTitle: "Пакеты опций на тарифах",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {
                                SubTitle: {
                                  FieldId: 1708,
                                  FieldName: "SubTitle",
                                  FieldTitle: "Подзаголовок",
                                  FieldDescription: "",
                                  FieldOrder: 5,
                                  FieldType: "Textbox",
                                  IsRequired: false
                                },
                                Description: {
                                  FieldId: 1707,
                                  FieldName: "Description",
                                  FieldTitle: "Описание",
                                  FieldDescription: "",
                                  FieldOrder: 6,
                                  FieldType: "VisualEdit",
                                  IsRequired: false
                                },
                                Alias: {
                                  FieldId: 1709,
                                  FieldName: "Alias",
                                  FieldTitle: "Псевдоним",
                                  FieldDescription: "",
                                  FieldOrder: 7,
                                  FieldType: "String",
                                  IsRequired: false
                                },
                                Link: {
                                  FieldId: 1727,
                                  FieldName: "Link",
                                  FieldTitle: "Ссылка",
                                  FieldDescription: "",
                                  FieldOrder: 8,
                                  FieldType: "String",
                                  IsRequired: false
                                }
                              }
                            },
                            ServiceRelation: {
                              ContentId: 413,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/413",
                              ContentName: "ServiceRelation",
                              ContentTitle: "Связи между услугами",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {}
                            },
                            RoamingScaleOnTariff: {
                              ContentId: 438,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/438",
                              ContentName: "RoamingScaleOnTariff",
                              ContentTitle: "Роуминговые сетки для тарифа",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {}
                            },
                            ServiceOnRoamingScale: {
                              ContentId: 444,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/444",
                              ContentName: "ServiceOnRoamingScale",
                              ContentTitle: "Услуги на роуминговой сетке",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {}
                            },
                            CrossSale: {
                              ContentId: 468,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/468",
                              ContentName: "CrossSale",
                              ContentTitle: "Матрица предложений CrossSale",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {
                                Order: {
                                  FieldId: 2197,
                                  FieldName: "Order",
                                  FieldTitle: "Порядок",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                }
                              }
                            },
                            MarketingCrossSale: {
                              ContentId: 469,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/469",
                              ContentName: "MarketingCrossSale",
                              ContentTitle: "Матрица маркетинговых предложений CrossSale",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {
                                Order: {
                                  FieldId: 2201,
                                  FieldName: "Order",
                                  FieldTitle: "Порядок",
                                  FieldDescription: "",
                                  FieldOrder: 4,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                }
                              }
                            },
                            DeviceOnTariffs: {
                              ContentId: 511,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/511",
                              ContentName: "DeviceOnTariffs",
                              ContentTitle: "Оборудование на тарифах",
                              ContentDescription: "",
                              IsExtension: true,
                              Fields: {
                                Order: {
                                  FieldId: 2534,
                                  FieldName: "Order",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 5,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                }
                              }
                            },
                            DevicesForFixConnectAction: {
                              ContentId: 512,
                              ContentPath: "/339:1542/383:2531/511:2530/361:1417/512",
                              ContentName: "DevicesForFixConnectAction",
                              ContentTitle: "Акционное оборудование",
                              ContentDescription: "Оборудование для акций фиксированной связи",
                              IsExtension: true,
                              Fields: {
                                Order: {
                                  FieldId: 2539,
                                  FieldName: "Order",
                                  FieldTitle: "",
                                  FieldDescription: "",
                                  FieldOrder: 3,
                                  FieldType: "Numeric",
                                  IsRequired: false
                                }
                              }
                            }
                          },
                          FieldId: 1417,
                          FieldName: "Type",
                          FieldTitle: "Тип",
                          FieldDescription: "",
                          FieldOrder: 2,
                          FieldType: "Classifier",
                          IsRequired: false
                        }
                      }
                    },
                    FieldId: 2530,
                    FieldName: "Parent",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 1,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  MarketingDevice: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/MarketingProduct2"
                    },
                    FieldId: 2531,
                    FieldName: "MarketingDevice",
                    FieldTitle: "Маркетинговое устройство",
                    FieldDescription: "",
                    FieldOrder: 2,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  MarketingTariffs: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/MarketingProduct3"
                    },
                    FieldId: 2532,
                    FieldName: "MarketingTariffs",
                    FieldTitle: "Маркетинговые тарифы",
                    FieldDescription: "",
                    FieldOrder: 3,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  Cities: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/Region1"
                    },
                    FieldId: 2533,
                    FieldName: "Cities",
                    FieldTitle: "Города",
                    FieldDescription: "",
                    FieldOrder: 4,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  Order: {
                    FieldId: 2534,
                    FieldName: "Order",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 5,
                    FieldType: "Numeric",
                    IsRequired: false
                  }
                }
              },
              FieldId: 2531,
              FieldName: "TariffsOnMarketingDevice",
              FieldTitle: "Маркетинговое устройство",
              FieldDescription: "",
              FieldOrder: 2,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            DevicesOnMarketingTariff: {
              IsBackward: true,
              Content: {
                ContentId: 511,
                ContentPath: "/339:1542/383:2532/511",
                ContentName: "DeviceOnTariffs",
                ContentTitle: "Оборудование на тарифах",
                ContentDescription: "",
                IsExtension: false,
                Fields: {
                  Parent: {
                    IsBackward: false,
                    Content: {
                      ContentId: 361,
                      ContentPath: "/339:1542/383:2532/511:2530/361",
                      ContentName: "ProductRelation",
                      ContentTitle: "Матрица связей",
                      ContentDescription: "",
                      IsExtension: false,
                      Fields: {
                        Title: {
                          FieldId: 1416,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Parameters: {
                          IsBackward: false,
                          Content: {
                            ContentId: 362,
                            ContentPath: "/339:1542/383:2532/511:2530/361:1431/362",
                            ContentName: "LinkParameter",
                            ContentTitle: "Параметры связей",
                            ContentDescription: "",
                            IsExtension: false,
                            Fields: {
                              Title: {
                                FieldId: 1418,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                FieldType: "String",
                                IsRequired: false
                              },
                              SortOrder: {
                                FieldId: 1425,
                                FieldName: "SortOrder",
                                FieldTitle: "Порядок",
                                FieldDescription: "",
                                FieldOrder: 12,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              NumValue: {
                                FieldId: 1426,
                                FieldName: "NumValue",
                                FieldTitle: "Числовое значение",
                                FieldDescription: "",
                                FieldOrder: 13,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              Value: {
                                FieldId: 1427,
                                FieldName: "Value",
                                FieldTitle: "Текстовое значение",
                                FieldDescription: "",
                                FieldOrder: 14,
                                FieldType: "VisualEdit",
                                IsRequired: false
                              },
                              Description: {
                                FieldId: 1429,
                                FieldName: "Description",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 16,
                                FieldType: "VisualEdit",
                                IsRequired: false
                              },
                              Unit: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/Unit"
                                },
                                FieldId: 1428,
                                FieldName: "Unit",
                                FieldTitle: "Единица измерения",
                                FieldDescription: "",
                                FieldOrder: 15,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Modifiers: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ParameterModifier"
                                },
                                FieldId: 1424,
                                FieldName: "Modifiers",
                                FieldTitle: "Модификаторы",
                                FieldDescription: "",
                                FieldOrder: 11,
                                FieldType: "M2MRelation",
                                IsRequired: false
                              },
                              BaseParameterModifiers: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/BaseParameterModifier"
                                },
                                FieldId: 1423,
                                FieldName: "BaseParameterModifiers",
                                FieldTitle: "Модификаторы базового параметра",
                                FieldDescription: "",
                                FieldOrder: 10,
                                FieldType: "M2MRelation",
                                IsRequired: false
                              },
                              Direction: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/Direction"
                                },
                                FieldId: 1422,
                                FieldName: "Direction",
                                FieldTitle: "Направление действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 9,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Zone: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/TariffZone"
                                },
                                FieldId: 1421,
                                FieldName: "Zone",
                                FieldTitle: "Зона действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 8,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              BaseParameter: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/BaseParameter"
                                },
                                FieldId: 1420,
                                FieldName: "BaseParameter",
                                FieldTitle: "Базовый параметр",
                                FieldDescription: "",
                                FieldOrder: 7,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Group: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ProductParameterGroup"
                                },
                                FieldId: 1678,
                                FieldName: "Group",
                                FieldTitle: "Группа параметров",
                                FieldDescription: "",
                                FieldOrder: 3,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Choice: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ParameterChoice"
                                },
                                FieldId: 2687,
                                FieldName: "Choice",
                                FieldTitle: "Выбор",
                                FieldDescription: "",
                                FieldOrder: 17,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              }
                            }
                          },
                          FieldId: 1431,
                          FieldName: "Parameters",
                          FieldTitle: "Параметры",
                          FieldDescription: "",
                          FieldOrder: 3,
                          FieldType: "M2ORelation",
                          IsRequired: false
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/LinkModifier"
                          },
                          FieldId: 1450,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 4,
                          FieldType: "M2MRelation",
                          IsRequired: false
                        }
                      }
                    },
                    FieldId: 2530,
                    FieldName: "Parent",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 1,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  MarketingDevice: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/MarketingProduct3"
                    },
                    FieldId: 2531,
                    FieldName: "MarketingDevice",
                    FieldTitle: "Маркетинговое устройство",
                    FieldDescription: "",
                    FieldOrder: 2,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Cities: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/Region1"
                    },
                    FieldId: 2533,
                    FieldName: "Cities",
                    FieldTitle: "Города",
                    FieldDescription: "",
                    FieldOrder: 4,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  },
                  Order: {
                    FieldId: 2534,
                    FieldName: "Order",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 5,
                    FieldType: "Numeric",
                    IsRequired: false
                  }
                }
              },
              FieldId: 2532,
              FieldName: "DevicesOnMarketingTariff",
              FieldTitle: "Маркетинговые тарифы",
              FieldDescription: "",
              FieldOrder: 3,
              FieldType: "M2MRelation",
              IsRequired: false
            },
            ActionsOnMarketingDevice: {
              IsBackward: true,
              Content: {
                ContentId: 512,
                ContentPath: "/339:1542/383:2538/512",
                ContentName: "DevicesForFixConnectAction",
                ContentTitle: "Акционное оборудование",
                ContentDescription: "Оборудование для акций фиксированной связи",
                IsExtension: false,
                Fields: {
                  FixConnectAction: {
                    IsBackward: false,
                    Content: {
                      ContentId: 339,
                      ContentPath: "/339:1542/383:2538/512:2537/339",
                      ContentName: "Product",
                      ContentTitle: "Продукты",
                      ContentDescription: "",
                      IsExtension: false,
                      Fields: {
                        MarketingProduct: {
                          IsBackward: false,
                          Content: {
                            ContentId: 383,
                            ContentPath: "/339:1542/383:2538/512:2537/339:1542/383",
                            ContentName: "MarketingProduct",
                            ContentTitle: "Маркетинговые продукты",
                            ContentDescription: "",
                            IsExtension: false,
                            Fields: {
                              Title: {
                                FieldId: 1534,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                FieldType: "String",
                                IsRequired: false
                              }
                            }
                          },
                          FieldId: 1542,
                          FieldName: "MarketingProduct",
                          FieldTitle: "Маркетинговый продукт",
                          FieldDescription: "",
                          FieldOrder: 1,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        GlobalCode: {
                          FieldId: 2033,
                          FieldName: "GlobalCode",
                          FieldTitle: "GlobalCode",
                          FieldDescription: "",
                          FieldOrder: 3,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Type: {
                          FieldId: 1341,
                          FieldName: "Type",
                          FieldTitle: "Тип",
                          FieldDescription: "",
                          FieldOrder: 5,
                          FieldType: "Classifier",
                          IsRequired: false
                        },
                        Description: {
                          FieldId: 1551,
                          FieldName: "Description",
                          FieldTitle: "Описание",
                          FieldDescription: "",
                          FieldOrder: 6,
                          FieldType: "Textbox",
                          IsRequired: false
                        },
                        FullDescription: {
                          FieldId: 1552,
                          FieldName: "FullDescription",
                          FieldTitle: "Полное описание",
                          FieldDescription: "",
                          FieldOrder: 7,
                          FieldType: "VisualEdit",
                          IsRequired: false
                        },
                        Notes: {
                          FieldId: 1640,
                          FieldName: "Notes",
                          FieldTitle: "Примечания",
                          FieldDescription: "",
                          FieldOrder: 8,
                          FieldType: "Textbox",
                          IsRequired: false
                        },
                        Link: {
                          FieldId: 1572,
                          FieldName: "Link",
                          FieldTitle: "Ссылка",
                          FieldDescription: "",
                          FieldOrder: 9,
                          FieldType: "String",
                          IsRequired: false
                        },
                        SortOrder: {
                          FieldId: 1476,
                          FieldName: "SortOrder",
                          FieldTitle: "Порядок",
                          FieldDescription: "",
                          FieldOrder: 10,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        ForisID: {
                          FieldId: 1470,
                          FieldName: "ForisID",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 11,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Icon: {
                          FieldId: 1581,
                          FieldName: "Icon",
                          FieldTitle: "Иконка",
                          FieldDescription: "",
                          FieldOrder: 15,
                          FieldType: "Image",
                          IsRequired: false
                        },
                        PDF: {
                          FieldId: 1582,
                          FieldName: "PDF",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 18,
                          FieldType: "File",
                          IsRequired: false
                        },
                        PdfFixedAlias: {
                          FieldId: 2677,
                          FieldName: "PdfFixedAlias",
                          FieldTitle: "Алиас фиксированной ссылки на Pdf",
                          FieldDescription: "",
                          FieldOrder: 19,
                          FieldType: "String",
                          IsRequired: false
                        },
                        PdfFixedLinks: {
                          FieldId: 2680,
                          FieldName: "PdfFixedLinks",
                          FieldTitle: "Фиксированные ссылки на Pdf",
                          FieldDescription: "",
                          FieldOrder: 20,
                          FieldType: "Textbox",
                          IsRequired: false
                        },
                        StartDate: {
                          FieldId: 1407,
                          FieldName: "StartDate",
                          FieldTitle: "Дата начала публикации",
                          FieldDescription: "",
                          FieldOrder: 21,
                          FieldType: "Date",
                          IsRequired: false
                        },
                        EndDate: {
                          FieldId: 1410,
                          FieldName: "EndDate",
                          FieldTitle: "Дата снятия с публикации",
                          FieldDescription: "",
                          FieldOrder: 22,
                          FieldType: "Date",
                          IsRequired: false
                        },
                        OldSiteId: {
                          FieldId: 1477,
                          FieldName: "OldSiteId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 24,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        OldId: {
                          FieldId: 1655,
                          FieldName: "OldId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 25,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        OldSiteInvId: {
                          FieldId: 1763,
                          FieldName: "OldSiteInvId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 26,
                          FieldType: "String",
                          IsRequired: false
                        },
                        OldCorpSiteId: {
                          FieldId: 1764,
                          FieldName: "OldCorpSiteId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 27,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        OldAliasId: {
                          FieldId: 1644,
                          FieldName: "OldAliasId",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 28,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Priority: {
                          FieldId: 2132,
                          FieldName: "Priority",
                          FieldTitle: "Приоритет (популярность)",
                          FieldDescription: "Сортировка по возрастанию значения приоритета",
                          FieldOrder: 31,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        ListImage: {
                          FieldId: 2498,
                          FieldName: "ListImage",
                          FieldTitle: "Изображение в списке",
                          FieldDescription: "Изображение в общем списке",
                          FieldOrder: 33,
                          FieldType: "Image",
                          IsRequired: false
                        },
                        ArchiveDate: {
                          FieldId: 2526,
                          FieldName: "ArchiveDate",
                          FieldTitle: "Дата перевода в архив",
                          FieldDescription: "",
                          FieldOrder: 34,
                          FieldType: "Date",
                          IsRequired: false
                        }
                      }
                    },
                    FieldId: 2537,
                    FieldName: "FixConnectAction",
                    FieldTitle: "Акция фиксированной связи",
                    FieldDescription: "",
                    FieldOrder: 1,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Parent: {
                    IsBackward: false,
                    Content: {
                      ContentId: 361,
                      ContentPath: "/339:1542/383:2538/512:2536/361",
                      ContentName: "ProductRelation",
                      ContentTitle: "Матрица связей",
                      ContentDescription: "",
                      IsExtension: false,
                      Fields: {
                        Title: {
                          FieldId: 1416,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          FieldType: "String",
                          IsRequired: false
                        },
                        Type: {
                          FieldId: 1417,
                          FieldName: "Type",
                          FieldTitle: "Тип",
                          FieldDescription: "",
                          FieldOrder: 2,
                          FieldType: "Classifier",
                          IsRequired: false
                        },
                        Parameters: {
                          IsBackward: false,
                          Content: {
                            ContentId: 362,
                            ContentPath: "/339:1542/383:2538/512:2536/361:1431/362",
                            ContentName: "LinkParameter",
                            ContentTitle: "Параметры связей",
                            ContentDescription: "",
                            IsExtension: false,
                            Fields: {
                              Unit: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 355,
                                  ContentPath: "/339:1542/383:2538/512:2536/361:1431/362:1428/355",
                                  ContentName: "Unit",
                                  ContentTitle: "Единицы измерения",
                                  ContentDescription: "",
                                  IsExtension: false,
                                  Fields: {
                                    Alias: {
                                      FieldId: 2062,
                                      FieldName: "Alias",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      FieldType: "String",
                                      IsRequired: false
                                    },
                                    Title: {
                                      FieldId: 1384,
                                      FieldName: "Title",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      FieldType: "String",
                                      IsRequired: false
                                    },
                                    Display: {
                                      FieldId: 1385,
                                      FieldName: "Display",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      FieldType: "String",
                                      IsRequired: false
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
                                      FieldType: "StringEnum",
                                      IsRequired: false
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
                                      FieldType: "StringEnum",
                                      IsRequired: false
                                    },
                                    QuotaPeriodicity: {
                                      FieldId: 2177,
                                      FieldName: "QuotaPeriodicity",
                                      FieldTitle: "Название периодичности",
                                      FieldDescription: "",
                                      FieldOrder: 6,
                                      FieldType: "String",
                                      IsRequired: false
                                    },
                                    PeriodMultiplier: {
                                      FieldId: 2114,
                                      FieldName: "PeriodMultiplier",
                                      FieldTitle: "Множитель периода",
                                      FieldDescription: "",
                                      FieldOrder: 7,
                                      FieldType: "Numeric",
                                      IsRequired: false
                                    },
                                    Type: {
                                      FieldId: 2568,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 8,
                                      FieldType: "String",
                                      IsRequired: false
                                    }
                                  }
                                },
                                FieldId: 1428,
                                FieldName: "Unit",
                                FieldTitle: "Единица измерения",
                                FieldDescription: "",
                                FieldOrder: 15,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              BaseParameterModifiers: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/BaseParameterModifier"
                                },
                                FieldId: 1423,
                                FieldName: "BaseParameterModifiers",
                                FieldTitle: "Модификаторы базового параметра",
                                FieldDescription: "",
                                FieldOrder: 10,
                                FieldType: "M2MRelation",
                                IsRequired: false
                              },
                              Modifiers: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ParameterModifier"
                                },
                                FieldId: 1424,
                                FieldName: "Modifiers",
                                FieldTitle: "Модификаторы",
                                FieldDescription: "",
                                FieldOrder: 11,
                                FieldType: "M2MRelation",
                                IsRequired: false
                              },
                              Direction: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/Direction"
                                },
                                FieldId: 1422,
                                FieldName: "Direction",
                                FieldTitle: "Направление действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 9,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Zone: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/TariffZone"
                                },
                                FieldId: 1421,
                                FieldName: "Zone",
                                FieldTitle: "Зона действия базового параметра",
                                FieldDescription: "",
                                FieldOrder: 8,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              BaseParameter: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/BaseParameter"
                                },
                                FieldId: 1420,
                                FieldName: "BaseParameter",
                                FieldTitle: "Базовый параметр",
                                FieldDescription: "",
                                FieldOrder: 7,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Group: {
                                IsBackward: false,
                                Content: {
                                  ContentId: 378,
                                  ContentPath: "/339:1542/383:2538/512:2536/361:1431/362:1678/378",
                                  ContentName: "ProductParameterGroup",
                                  ContentTitle: "Группы параметров продуктов",
                                  ContentDescription: "",
                                  IsExtension: false,
                                  Fields: {
                                    Title: {
                                      FieldId: 1496,
                                      FieldName: "Title",
                                      FieldTitle: "Название",
                                      FieldDescription: "",
                                      FieldOrder: 1,
                                      FieldType: "String",
                                      IsRequired: false
                                    },
                                    Alias: {
                                      FieldId: 2049,
                                      FieldName: "Alias",
                                      FieldTitle: "Псевдоним",
                                      FieldDescription: "",
                                      FieldOrder: 2,
                                      FieldType: "String",
                                      IsRequired: false
                                    },
                                    SortOrder: {
                                      FieldId: 1500,
                                      FieldName: "SortOrder",
                                      FieldTitle: "Порядок",
                                      FieldDescription: "",
                                      FieldOrder: 3,
                                      FieldType: "Numeric",
                                      IsRequired: false
                                    },
                                    OldSiteId: {
                                      FieldId: 1588,
                                      FieldName: "OldSiteId",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 5,
                                      FieldType: "Numeric",
                                      IsRequired: false
                                    },
                                    OldCorpSiteId: {
                                      FieldId: 1771,
                                      FieldName: "OldCorpSiteId",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 6,
                                      FieldType: "Numeric",
                                      IsRequired: false
                                    },
                                    ImageSvg: {
                                      FieldId: 2029,
                                      FieldName: "ImageSvg",
                                      FieldTitle: "Изображение",
                                      FieldDescription: "",
                                      FieldOrder: 7,
                                      FieldType: "File",
                                      IsRequired: false
                                    },
                                    Type: {
                                      FieldId: 2061,
                                      FieldName: "Type",
                                      FieldTitle: "",
                                      FieldDescription: "",
                                      FieldOrder: 8,
                                      FieldType: "String",
                                      IsRequired: false
                                    },
                                    TitleForIcin: {
                                      FieldId: 2116,
                                      FieldName: "TitleForIcin",
                                      FieldTitle: "Название для МГМН",
                                      FieldDescription: "",
                                      FieldOrder: 9,
                                      FieldType: "String",
                                      IsRequired: false
                                    }
                                  }
                                },
                                FieldId: 1678,
                                FieldName: "Group",
                                FieldTitle: "Группа параметров",
                                FieldDescription: "",
                                FieldOrder: 3,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Choice: {
                                IsBackward: false,
                                Content: {
                                  "$ref": "#/Definitions/ParameterChoice"
                                },
                                FieldId: 2687,
                                FieldName: "Choice",
                                FieldTitle: "Выбор",
                                FieldDescription: "",
                                FieldOrder: 17,
                                FieldType: "O2MRelation",
                                IsRequired: false
                              },
                              Title: {
                                FieldId: 1418,
                                FieldName: "Title",
                                FieldTitle: "Название",
                                FieldDescription: "",
                                FieldOrder: 1,
                                FieldType: "String",
                                IsRequired: false
                              },
                              SortOrder: {
                                FieldId: 1425,
                                FieldName: "SortOrder",
                                FieldTitle: "Порядок",
                                FieldDescription: "",
                                FieldOrder: 12,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              NumValue: {
                                FieldId: 1426,
                                FieldName: "NumValue",
                                FieldTitle: "Числовое значение",
                                FieldDescription: "",
                                FieldOrder: 13,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              Value: {
                                FieldId: 1427,
                                FieldName: "Value",
                                FieldTitle: "Текстовое значение",
                                FieldDescription: "",
                                FieldOrder: 14,
                                FieldType: "VisualEdit",
                                IsRequired: false
                              },
                              Description: {
                                FieldId: 1429,
                                FieldName: "Description",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 16,
                                FieldType: "VisualEdit",
                                IsRequired: false
                              },
                              OldSiteId: {
                                FieldId: 1656,
                                FieldName: "OldSiteId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 20,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              OldCorpSiteId: {
                                FieldId: 1772,
                                FieldName: "OldCorpSiteId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 21,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              OldPointId: {
                                FieldId: 1658,
                                FieldName: "OldPointId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 22,
                                FieldType: "Numeric",
                                IsRequired: false
                              },
                              OldCorpPointId: {
                                FieldId: 1774,
                                FieldName: "OldCorpPointId",
                                FieldTitle: "",
                                FieldDescription: "",
                                FieldOrder: 23,
                                FieldType: "Numeric",
                                IsRequired: false
                              }
                            }
                          },
                          FieldId: 1431,
                          FieldName: "Parameters",
                          FieldTitle: "Параметры",
                          FieldDescription: "",
                          FieldOrder: 3,
                          FieldType: "M2ORelation",
                          IsRequired: false
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/LinkModifier"
                          },
                          FieldId: 1450,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 4,
                          FieldType: "M2MRelation",
                          IsRequired: false
                        }
                      }
                    },
                    FieldId: 2536,
                    FieldName: "Parent",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 0,
                    FieldType: "O2MRelation",
                    IsRequired: false
                  },
                  Order: {
                    FieldId: 2539,
                    FieldName: "Order",
                    FieldTitle: "",
                    FieldDescription: "",
                    FieldOrder: 3,
                    FieldType: "Numeric",
                    IsRequired: false
                  }
                }
              },
              FieldId: 2538,
              FieldName: "ActionsOnMarketingDevice",
              FieldTitle: "Маркетинговое оборудование",
              FieldDescription: "",
              FieldOrder: 2,
              FieldType: "O2MRelation",
              IsRequired: false
            }
          }
        },
        FieldId: 1542,
        FieldName: "MarketingProduct",
        FieldTitle: "Маркетинговый продукт",
        FieldDescription: "",
        FieldOrder: 1,
        FieldType: "O2MRelation",
        IsRequired: false
      },
      GlobalCode: {
        FieldId: 2033,
        FieldName: "GlobalCode",
        FieldTitle: "GlobalCode",
        FieldDescription: "",
        FieldOrder: 3,
        FieldType: "String",
        IsRequired: false
      },
      Description: {
        FieldId: 1551,
        FieldName: "Description",
        FieldTitle: "Описание",
        FieldDescription: "",
        FieldOrder: 6,
        FieldType: "Textbox",
        IsRequired: false
      },
      FullDescription: {
        FieldId: 1552,
        FieldName: "FullDescription",
        FieldTitle: "Полное описание",
        FieldDescription: "",
        FieldOrder: 7,
        FieldType: "VisualEdit",
        IsRequired: false
      },
      Notes: {
        FieldId: 1640,
        FieldName: "Notes",
        FieldTitle: "Примечания",
        FieldDescription: "",
        FieldOrder: 8,
        FieldType: "Textbox",
        IsRequired: false
      },
      Link: {
        FieldId: 1572,
        FieldName: "Link",
        FieldTitle: "Ссылка",
        FieldDescription: "",
        FieldOrder: 9,
        FieldType: "String",
        IsRequired: false
      },
      SortOrder: {
        FieldId: 1476,
        FieldName: "SortOrder",
        FieldTitle: "Порядок",
        FieldDescription: "",
        FieldOrder: 10,
        FieldType: "Numeric",
        IsRequired: false
      },
      Icon: {
        FieldId: 1581,
        FieldName: "Icon",
        FieldTitle: "Иконка",
        FieldDescription: "",
        FieldOrder: 15,
        FieldType: "Image",
        IsRequired: false
      },
      PDF: {
        FieldId: 1582,
        FieldName: "PDF",
        FieldTitle: "",
        FieldDescription: "",
        FieldOrder: 18,
        FieldType: "File",
        IsRequired: false
      },
      StartDate: {
        FieldId: 1407,
        FieldName: "StartDate",
        FieldTitle: "Дата начала публикации",
        FieldDescription: "",
        FieldOrder: 21,
        FieldType: "Date",
        IsRequired: false
      },
      EndDate: {
        FieldId: 1410,
        FieldName: "EndDate",
        FieldTitle: "Дата снятия с публикации",
        FieldDescription: "",
        FieldOrder: 22,
        FieldType: "Date",
        IsRequired: false
      },
      Priority: {
        FieldId: 2132,
        FieldName: "Priority",
        FieldTitle: "Приоритет (популярность)",
        FieldDescription: "Сортировка по возрастанию значения приоритета",
        FieldOrder: 31,
        FieldType: "Numeric",
        IsRequired: false
      },
      ListImage: {
        FieldId: 2498,
        FieldName: "ListImage",
        FieldTitle: "Изображение в списке",
        FieldDescription: "Изображение в общем списке",
        FieldOrder: 33,
        FieldType: "Image",
        IsRequired: false
      },
      ArchiveDate: {
        FieldId: 2526,
        FieldName: "ArchiveDate",
        FieldTitle: "Дата перевода в архив",
        FieldDescription: "",
        FieldOrder: 34,
        FieldType: "Date",
        IsRequired: false
      },
      Modifiers: {
        IsBackward: false,
        Content: {
          "$ref": "#/Definitions/ProductModifer"
        },
        FieldId: 1523,
        FieldName: "Modifiers",
        FieldTitle: "Модификаторы",
        FieldDescription: "",
        FieldOrder: 4,
        FieldType: "M2MRelation",
        IsRequired: false
      },
      Parameters: {
        IsBackward: false,
        Content: {
          ContentId: 354,
          ContentPath: "/339:1403/354",
          ContentName: "ProductParameter",
          ContentTitle: "Параметры продуктов",
          ContentDescription: "",
          IsExtension: false,
          Fields: {
            Group: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/ProductParameterGroup1"
              },
              FieldId: 1506,
              FieldName: "Group",
              FieldTitle: "Группа параметров",
              FieldDescription: "",
              FieldOrder: 4,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            Parent: {
              IsBackward: false,
              Content: {
                ContentId: 354,
                ContentPath: "/339:1403/354:1642/354",
                ContentName: "ProductParameter",
                ContentTitle: "Параметры продуктов",
                ContentDescription: "",
                IsExtension: false,
                Fields: {
                  Title: {
                    FieldId: 1373,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    FieldType: "String",
                    IsRequired: false
                  }
                }
              },
              FieldId: 1642,
              FieldName: "Parent",
              FieldTitle: "Родительский параметр",
              FieldDescription: "",
              FieldOrder: 5,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            BaseParameter: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/BaseParameter"
              },
              FieldId: 1375,
              FieldName: "BaseParameter",
              FieldTitle: "Базовый параметр",
              FieldDescription: "",
              FieldOrder: 6,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            Zone: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/TariffZone"
              },
              FieldId: 1377,
              FieldName: "Zone",
              FieldTitle: "Зона действия базового параметра",
              FieldDescription: "",
              FieldOrder: 7,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            Direction: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/Direction"
              },
              FieldId: 1378,
              FieldName: "Direction",
              FieldTitle: "Направление действия базового параметра",
              FieldDescription: "",
              FieldOrder: 8,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            BaseParameterModifiers: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/BaseParameterModifier"
              },
              FieldId: 1379,
              FieldName: "BaseParameterModifiers",
              FieldTitle: "Модификаторы базового параметра",
              FieldDescription: "",
              FieldOrder: 9,
              FieldType: "M2MRelation",
              IsRequired: false
            },
            Modifiers: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/ParameterModifier"
              },
              FieldId: 1380,
              FieldName: "Modifiers",
              FieldTitle: "Модификаторы",
              FieldDescription: "",
              FieldOrder: 10,
              FieldType: "M2MRelation",
              IsRequired: false
            },
            Unit: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/Unit"
              },
              FieldId: 1386,
              FieldName: "Unit",
              FieldTitle: "Единица измерения",
              FieldDescription: "",
              FieldOrder: 14,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            Title: {
              FieldId: 1373,
              FieldName: "Title",
              FieldTitle: "Название",
              FieldDescription: "",
              FieldOrder: 1,
              FieldType: "String",
              IsRequired: false
            },
            SortOrder: {
              FieldId: 1381,
              FieldName: "SortOrder",
              FieldTitle: "Порядок",
              FieldDescription: "",
              FieldOrder: 11,
              FieldType: "Numeric",
              IsRequired: false
            },
            NumValue: {
              FieldId: 1382,
              FieldName: "NumValue",
              FieldTitle: "Числовое значение",
              FieldDescription: "",
              FieldOrder: 12,
              FieldType: "Numeric",
              IsRequired: false
            },
            Value: {
              FieldId: 1383,
              FieldName: "Value",
              FieldTitle: "Текстовое значение",
              FieldDescription: "",
              FieldOrder: 13,
              FieldType: "VisualEdit",
              IsRequired: false
            },
            Description: {
              FieldId: 1387,
              FieldName: "Description",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 15,
              FieldType: "VisualEdit",
              IsRequired: false
            },
            Image: {
              FieldId: 2022,
              FieldName: "Image",
              FieldTitle: "Изображение параметра",
              FieldDescription: "",
              FieldOrder: 19,
              FieldType: "Image",
              IsRequired: false
            },
            ProductGroup: {
              IsBackward: false,
              Content: {
                ContentId: 340,
                ContentPath: "/339:1403/354:1758/340",
                ContentName: "Group",
                ContentTitle: "Группы продуктов",
                ContentDescription: "",
                IsExtension: false,
                Fields: {
                  Title: {
                    FieldId: 1329,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    FieldType: "String",
                    IsRequired: false
                  },
                  Alias: {
                    FieldId: 1754,
                    FieldName: "Alias",
                    FieldTitle: "Псевдоним",
                    FieldDescription: "",
                    FieldOrder: 2,
                    FieldType: "String",
                    IsRequired: false
                  }
                }
              },
              FieldId: 1758,
              FieldName: "ProductGroup",
              FieldTitle: "Группа продуктов",
              FieldDescription: "",
              FieldOrder: 16,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            Choice: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/ParameterChoice"
              },
              FieldId: 2445,
              FieldName: "Choice",
              FieldTitle: "Выбор",
              FieldDescription: "",
              FieldOrder: 17,
              FieldType: "O2MRelation",
              IsRequired: false
            }
          }
        },
        FieldId: 1403,
        FieldName: "Parameters",
        FieldTitle: "Параметры продукта",
        FieldDescription: "",
        FieldOrder: 17,
        FieldType: "M2ORelation",
        IsRequired: false
      },
      Regions: {
        IsBackward: false,
        Content: {
          ContentId: 290,
          ContentPath: "/339:1326/290",
          ContentName: "Region",
          ContentTitle: "Регионы",
          ContentDescription: "",
          IsExtension: false,
          Fields: {
            Title: {
              FieldId: 1114,
              FieldName: "Title",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 1,
              FieldType: "String",
              IsRequired: false
            },
            Alias: {
              FieldId: 1532,
              FieldName: "Alias",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 2,
              FieldType: "String",
              IsRequired: false
            },
            Parent: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/Region2"
              },
              FieldId: 1115,
              FieldName: "Parent",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 4,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            IsMainCity: {
              FieldId: 2239,
              FieldName: "IsMainCity",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 12,
              FieldType: "Boolean",
              IsRequired: false
            }
          }
        },
        FieldId: 1326,
        FieldName: "Regions",
        FieldTitle: "Регионы",
        FieldDescription: "",
        FieldOrder: 2,
        FieldType: "M2MRelation",
        IsRequired: false
      },
      Type: {
        Contents: {
          Tariff: {
            ContentId: 343,
            ContentPath: "/339:1341/343",
            ContentName: "Tariff",
            ContentTitle: "Тарифы",
            ContentDescription: "",
            IsExtension: true,
            Fields: {}
          },
          Service: {
            ContentId: 403,
            ContentPath: "/339:1341/403",
            ContentName: "Service",
            ContentTitle: "Услуги",
            ContentDescription: "",
            IsExtension: true,
            Fields: {}
          },
          Action: {
            ContentId: 419,
            ContentPath: "/339:1341/419",
            ContentName: "Action",
            ContentTitle: "Акции",
            ContentDescription: "",
            IsExtension: true,
            Fields: {}
          },
          RoamingScale: {
            ContentId: 434,
            ContentPath: "/339:1341/434",
            ContentName: "RoamingScale",
            ContentTitle: "Роуминговые сетки",
            ContentDescription: "",
            IsExtension: true,
            Fields: {}
          },
          Device: {
            ContentId: 490,
            ContentPath: "/339:1341/490",
            ContentName: "Device",
            ContentTitle: "Оборудование",
            ContentDescription: "",
            IsExtension: true,
            Fields: {
              Downloads: {
                IsBackward: false,
                Content: {
                  ContentId: 494,
                  ContentPath: "/339:1341/490:2407/494",
                  ContentName: "EquipmentDownload",
                  ContentTitle: "Загрузки для оборудования",
                  ContentDescription: "",
                  IsExtension: false,
                  Fields: {
                    Title: {
                      FieldId: 2405,
                      FieldName: "Title",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 1,
                      FieldType: "String",
                      IsRequired: false
                    },
                    File: {
                      FieldId: 2406,
                      FieldName: "File",
                      FieldTitle: "",
                      FieldDescription: "",
                      FieldOrder: 2,
                      FieldType: "File",
                      IsRequired: false
                    }
                  }
                },
                FieldId: 2407,
                FieldName: "Downloads",
                FieldTitle: "Загрузки",
                FieldDescription: "",
                FieldOrder: 2,
                FieldType: "M2MRelation",
                IsRequired: false
              },
              Inners: {
                IsBackward: false,
                Content: {
                  ContentId: 339,
                  ContentPath: "/339:1341/490:2447/339",
                  ContentName: "Product",
                  ContentTitle: "Продукты",
                  ContentDescription: "",
                  IsExtension: false,
                  Fields: {
                    MarketingProduct: {
                      IsBackward: false,
                      Content: {
                        "$ref": "#/Definitions/MarketingProduct3"
                      },
                      FieldId: 1542,
                      FieldName: "MarketingProduct",
                      FieldTitle: "Маркетинговый продукт",
                      FieldDescription: "",
                      FieldOrder: 1,
                      FieldType: "O2MRelation",
                      IsRequired: false
                    }
                  }
                },
                FieldId: 2447,
                FieldName: "Inners",
                FieldTitle: "Состав комплекта",
                FieldDescription: "",
                FieldOrder: 6,
                FieldType: "M2MRelation",
                IsRequired: false
              },
              FreezeDate: {
                FieldId: 2390,
                FieldName: "FreezeDate",
                FieldTitle: "Отложенная публикация на",
                FieldDescription: "Продукт будет опубликован в течение 2,5 часов после наступления даты публикации",
                FieldOrder: 3,
                FieldType: "DateTime",
                IsRequired: false
              },
              FullUserGuide: {
                FieldId: 2409,
                FieldName: "FullUserGuide",
                FieldTitle: "Полное руководство пользователя (User guide)",
                FieldDescription: "",
                FieldOrder: 4,
                FieldType: "File",
                IsRequired: false
              },
              QuickStartGuide: {
                FieldId: 2410,
                FieldName: "QuickStartGuide",
                FieldTitle: "Краткое руководство пользователя (Quick start guide)",
                FieldDescription: "",
                FieldOrder: 5,
                FieldType: "File",
                IsRequired: false
              }
            }
          },
          FixConnectAction: {
            ContentId: 500,
            ContentPath: "/339:1341/500",
            ContentName: "FixConnectAction",
            ContentTitle: "Акции фиксированной связи",
            ContentDescription: "",
            IsExtension: true,
            Fields: {
              MarketingOffers: {
                IsBackward: false,
                Content: {
                  "$ref": "#/Definitions/MarketingProduct"
                },
                FieldId: 2528,
                FieldName: "MarketingOffers",
                FieldTitle: "",
                FieldDescription: "",
                FieldOrder: 1,
                FieldType: "M2MRelation",
                IsRequired: false
              },
              PromoPeriod: {
                FieldId: 2472,
                FieldName: "PromoPeriod",
                FieldTitle: "",
                FieldDescription: "Описание промо-периода.",
                FieldOrder: 2,
                FieldType: "String",
                IsRequired: false
              },
              AfterPromo: {
                FieldId: 2473,
                FieldName: "AfterPromo",
                FieldTitle: "",
                FieldDescription: "Описание момента начала действия обычной цены.",
                FieldOrder: 3,
                FieldType: "String",
                IsRequired: false
              }
            }
          },
          TvPackage: {
            ContentId: 503,
            ContentPath: "/339:1341/503",
            ContentName: "TvPackage",
            ContentTitle: "ТВ-пакеты",
            ContentDescription: "",
            IsExtension: true,
            Fields: {}
          },
          FixConnectTariff: {
            ContentId: 505,
            ContentPath: "/339:1341/505",
            ContentName: "FixConnectTariff",
            ContentTitle: "Тарифы фиксированной связи",
            ContentDescription: "",
            IsExtension: true,
            Fields: {
              TitleForSite: {
                FieldId: 2525,
                FieldName: "TitleForSite",
                FieldTitle: "",
                FieldDescription: "",
                FieldOrder: 2,
                FieldType: "String",
                IsRequired: false
              }
            }
          },
          PhoneTariff: {
            ContentId: 507,
            ContentPath: "/339:1341/507",
            ContentName: "PhoneTariff",
            ContentTitle: "Тарифы телефонии",
            ContentDescription: "",
            IsExtension: true,
            Fields: {
              RostelecomLink: {
                FieldId: 2522,
                FieldName: "RostelecomLink",
                FieldTitle: "ВЗ вызовы (ссылка на Ростелеком)",
                FieldDescription: "Тарифы Ростелеком распространяются только на ВЗ вызовы.",
                FieldOrder: 1,
                FieldType: "String",
                IsRequired: false
              }
            }
          },
          InternetTariff: {
            ContentId: 510,
            ContentPath: "/339:1341/510",
            ContentName: "InternetTariff",
            ContentTitle: "Тарифы Интернет",
            ContentDescription: "",
            IsExtension: true,
            Fields: {}
          }
        },
        FieldId: 1341,
        FieldName: "Type",
        FieldTitle: "Тип",
        FieldDescription: "",
        FieldOrder: 5,
        FieldType: "Classifier",
        IsRequired: false
      },
      FixConnectAction: {
        IsBackward: true,
        Content: {
          ContentId: 512,
          ContentPath: "/339:2537/512",
          ContentName: "DevicesForFixConnectAction",
          ContentTitle: "Акционное оборудование",
          ContentDescription: "Оборудование для акций фиксированной связи",
          IsExtension: false,
          Fields: {
            MarketingDevice: {
              IsBackward: false,
              Content: {
                "$ref": "#/Definitions/MarketingProduct"
              },
              FieldId: 2538,
              FieldName: "MarketingDevice",
              FieldTitle: "Маркетинговое оборудование",
              FieldDescription: "",
              FieldOrder: 2,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            Parent: {
              IsBackward: false,
              Content: {
                ContentId: 361,
                ContentPath: "/339:2537/512:2536/361",
                ContentName: "ProductRelation",
                ContentTitle: "Матрица связей",
                ContentDescription: "",
                IsExtension: false,
                Fields: {
                  Title: {
                    FieldId: 1416,
                    FieldName: "Title",
                    FieldTitle: "Название",
                    FieldDescription: "",
                    FieldOrder: 1,
                    FieldType: "String",
                    IsRequired: false
                  },
                  Parameters: {
                    IsBackward: false,
                    Content: {
                      ContentId: 362,
                      ContentPath: "/339:2537/512:2536/361:1431/362",
                      ContentName: "LinkParameter",
                      ContentTitle: "Параметры связей",
                      ContentDescription: "",
                      IsExtension: false,
                      Fields: {
                        BaseParameter: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/BaseParameter"
                          },
                          FieldId: 1420,
                          FieldName: "BaseParameter",
                          FieldTitle: "Базовый параметр",
                          FieldDescription: "",
                          FieldOrder: 7,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        Zone: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/TariffZone"
                          },
                          FieldId: 1421,
                          FieldName: "Zone",
                          FieldTitle: "Зона действия базового параметра",
                          FieldDescription: "",
                          FieldOrder: 8,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        Direction: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/Direction"
                          },
                          FieldId: 1422,
                          FieldName: "Direction",
                          FieldTitle: "Направление действия базового параметра",
                          FieldDescription: "",
                          FieldOrder: 9,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        BaseParameterModifiers: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/BaseParameterModifier"
                          },
                          FieldId: 1423,
                          FieldName: "BaseParameterModifiers",
                          FieldTitle: "Модификаторы базового параметра",
                          FieldDescription: "",
                          FieldOrder: 10,
                          FieldType: "M2MRelation",
                          IsRequired: false
                        },
                        Modifiers: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/ParameterModifier"
                          },
                          FieldId: 1424,
                          FieldName: "Modifiers",
                          FieldTitle: "Модификаторы",
                          FieldDescription: "",
                          FieldOrder: 11,
                          FieldType: "M2MRelation",
                          IsRequired: false
                        },
                        Unit: {
                          IsBackward: false,
                          Content: {
                            "$ref": "#/Definitions/Unit"
                          },
                          FieldId: 1428,
                          FieldName: "Unit",
                          FieldTitle: "Единица измерения",
                          FieldDescription: "",
                          FieldOrder: 15,
                          FieldType: "O2MRelation",
                          IsRequired: false
                        },
                        Title: {
                          FieldId: 1418,
                          FieldName: "Title",
                          FieldTitle: "Название",
                          FieldDescription: "",
                          FieldOrder: 1,
                          FieldType: "String",
                          IsRequired: false
                        },
                        SortOrder: {
                          FieldId: 1425,
                          FieldName: "SortOrder",
                          FieldTitle: "Порядок",
                          FieldDescription: "",
                          FieldOrder: 12,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        NumValue: {
                          FieldId: 1426,
                          FieldName: "NumValue",
                          FieldTitle: "Числовое значение",
                          FieldDescription: "",
                          FieldOrder: 13,
                          FieldType: "Numeric",
                          IsRequired: false
                        },
                        Value: {
                          FieldId: 1427,
                          FieldName: "Value",
                          FieldTitle: "Текстовое значение",
                          FieldDescription: "",
                          FieldOrder: 14,
                          FieldType: "VisualEdit",
                          IsRequired: false
                        },
                        Description: {
                          FieldId: 1429,
                          FieldName: "Description",
                          FieldTitle: "",
                          FieldDescription: "",
                          FieldOrder: 16,
                          FieldType: "VisualEdit",
                          IsRequired: false
                        }
                      }
                    },
                    FieldId: 1431,
                    FieldName: "Parameters",
                    FieldTitle: "Параметры",
                    FieldDescription: "",
                    FieldOrder: 3,
                    FieldType: "M2ORelation",
                    IsRequired: false
                  },
                  Modifiers: {
                    IsBackward: false,
                    Content: {
                      "$ref": "#/Definitions/LinkModifier"
                    },
                    FieldId: 1450,
                    FieldName: "Modifiers",
                    FieldTitle: "Модификаторы",
                    FieldDescription: "",
                    FieldOrder: 4,
                    FieldType: "M2MRelation",
                    IsRequired: false
                  }
                }
              },
              FieldId: 2536,
              FieldName: "Parent",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 0,
              FieldType: "O2MRelation",
              IsRequired: false
            },
            Order: {
              FieldId: 2539,
              FieldName: "Order",
              FieldTitle: "",
              FieldDescription: "",
              FieldOrder: 3,
              FieldType: "Numeric",
              IsRequired: false
            }
          }
        },
        FieldId: 2537,
        FieldName: "FixConnectAction",
        FieldTitle: "Акция фиксированной связи",
        FieldDescription: "",
        FieldOrder: 1,
        FieldType: "O2MRelation",
        IsRequired: false
      },
      Advantages: {
        IsBackward: false,
        Content: {
          "$ref": "#/Definitions/Advantage"
        },
        FieldId: 2133,
        FieldName: "Advantages",
        FieldTitle: "Преимущества",
        FieldDescription: "",
        FieldOrder: 32,
        FieldType: "M2MRelation",
        IsRequired: false
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
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1792,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1793,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    ChannelCategory: {
      ContentId: 478,
      ContentPath: "/339:1542/383:1540/502:2497/482:2283/478",
      ContentName: "ChannelCategory",
      ContentTitle: "Категории каналов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Name: {
          FieldId: 2257,
          FieldName: "Name",
          FieldTitle: "Название для сайта",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 2271,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        Segments: {
          FieldId: 2267,
          FieldName: "Segments",
          FieldTitle: "Сегменты",
          FieldDescription: "",
          FieldOrder: 3,
          FieldType: "String",
          IsRequired: false
        },
        Icon: {
          FieldId: 2269,
          FieldName: "Icon",
          FieldTitle: "Иконка",
          FieldDescription: "",
          FieldOrder: 5,
          FieldType: "Image",
          IsRequired: false
        },
        Order: {
          FieldId: 2270,
          FieldName: "Order",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          FieldType: "Numeric",
          IsRequired: false
        },
        OldSiteId: {
          FieldId: 2262,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 7,
          FieldType: "Numeric",
          IsRequired: false
        }
      }
    },
    Region: {
      ContentId: 290,
      ContentPath: "/339:1542/383:1540/502:2497/482:2286/472:2211/290",
      ContentName: "Region",
      ContentTitle: "Регионы",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Alias: {
          FieldId: 1532,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        Parent: {
          IsBackward: false,
          Content: {
            "$ref": "#/Definitions/Region2"
          },
          FieldId: 1115,
          FieldName: "Parent",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          FieldType: "O2MRelation",
          IsRequired: false
        },
        IsMainCity: {
          FieldId: 2239,
          FieldName: "IsMainCity",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 12,
          FieldType: "Boolean",
          IsRequired: false
        }
      }
    },
    Region1: {
      ContentId: 290,
      ContentPath: "/339:1542/383:2531/511:2533/290",
      ContentName: "Region",
      ContentTitle: "Регионы",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Alias: {
          FieldId: 1532,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        Title: {
          FieldId: 1114,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    Region2: {
      ContentId: 290,
      ContentPath: "/339:1542/383:1540/502:2497/482:2286/472:2211/290:1115/290",
      ContentName: "Region",
      ContentTitle: "Регионы",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Alias: {
          FieldId: 1532,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    ChannelFormat: {
      ContentId: 480,
      ContentPath: "/339:1542/383:1540/502:2497/482:2619/482:2524/480",
      ContentName: "ChannelFormat",
      ContentTitle: "Форматы каналов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 2263,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Image: {
          FieldId: 2265,
          FieldName: "Image",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "Image",
          IsRequired: false
        },
        Message: {
          FieldId: 2266,
          FieldName: "Message",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 3,
          FieldType: "String",
          IsRequired: false
        },
        OldSiteId: {
          FieldId: 2264,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          FieldType: "Numeric",
          IsRequired: false
        }
      }
    },
    FixedType: {
      ContentId: 491,
      ContentPath: "/339:1542/383:1540/489:2403/493:2402/491",
      ContentName: "FixedType",
      ContentTitle: "Типы фиксированной связи",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 2392,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    MarketingProduct: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/504:2519/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1534,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        Priority: {
          FieldId: 2032,
          FieldName: "Priority",
          FieldTitle: "Приоритет (популярность)",
          FieldDescription: "Сортировка по возрастанию значения приоритета",
          FieldOrder: 19,
          FieldType: "Numeric",
          IsRequired: false
        }
      }
    },
    MarketingProduct1: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/504:2517/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1534,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        Link: {
          FieldId: 1755,
          FieldName: "Link",
          FieldTitle: "Ссылка",
          FieldDescription: "",
          FieldOrder: 3,
          FieldType: "String",
          IsRequired: false
        },
        Description: {
          FieldId: 1558,
          FieldName: "Description",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          FieldType: "Textbox",
          IsRequired: false
        },
        DetailedDescription: {
          FieldId: 2023,
          FieldName: "DetailedDescription",
          FieldTitle: "Подробное описание",
          FieldDescription: "",
          FieldOrder: 5,
          FieldType: "VisualEdit",
          IsRequired: false
        },
        FullDescription: {
          FieldId: 1740,
          FieldName: "FullDescription",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          FieldType: "VisualEdit",
          IsRequired: false
        },
        SortOrder: {
          FieldId: 1752,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 7,
          FieldType: "Numeric",
          IsRequired: false
        },
        Type: {
          FieldId: 1540,
          FieldName: "Type",
          FieldTitle: "Тип",
          FieldDescription: "",
          FieldOrder: 11,
          FieldType: "Classifier",
          IsRequired: false
        },
        OldSiteId: {
          FieldId: 1645,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 14,
          FieldType: "Numeric",
          IsRequired: false
        },
        OldCorpSiteId: {
          FieldId: 1779,
          FieldName: "OldCorpSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 15,
          FieldType: "Numeric",
          IsRequired: false
        },
        ListImage: {
          FieldId: 2030,
          FieldName: "ListImage",
          FieldTitle: "Изображение в списке",
          FieldDescription: "Изображение в общем списке",
          FieldOrder: 17,
          FieldType: "Image",
          IsRequired: false
        },
        DetailsImage: {
          FieldId: 2031,
          FieldName: "DetailsImage",
          FieldTitle: "Изображение",
          FieldDescription: "Изображение в описании на странице",
          FieldOrder: 18,
          FieldType: "Image",
          IsRequired: false
        },
        Priority: {
          FieldId: 2032,
          FieldName: "Priority",
          FieldTitle: "Приоритет (популярность)",
          FieldDescription: "Сортировка по возрастанию значения приоритета",
          FieldOrder: 19,
          FieldType: "Numeric",
          IsRequired: false
        },
        ArchiveDate: {
          FieldId: 2124,
          FieldName: "ArchiveDate",
          FieldTitle: "Дата закрытия продукта (Архив)",
          FieldDescription: "",
          FieldOrder: 23,
          FieldType: "Date",
          IsRequired: false
        }
      }
    },
    MarketingProduct2: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/504:2493/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    MarketingProduct3: {
      ContentId: 383,
      ContentPath: "/339:1542/383:1540/498:2564/383",
      ContentName: "MarketingProduct",
      ContentTitle: "Маркетинговые продукты",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1534,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1753,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    BaseParameter: {
      ContentId: 350,
      ContentPath: "/339:1542/383:1869/424:1854/350",
      ContentName: "BaseParameter",
      ContentTitle: "Базовые параметры продуктов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1358,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1359,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        AllowZone: {
          FieldId: 2683,
          FieldName: "AllowZone",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 5,
          FieldType: "Boolean",
          IsRequired: false
        },
        AllowDirection: {
          FieldId: 2684,
          FieldName: "AllowDirection",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          FieldType: "Boolean",
          IsRequired: false
        }
      }
    },
    TariffZone: {
      ContentId: 346,
      ContentPath: "/339:1542/383:1869/424:1855/346",
      ContentName: "TariffZone",
      ContentTitle: "Тарифные зоны",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1346,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1347,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    Direction: {
      ContentId: 347,
      ContentPath: "/339:1542/383:1869/424:1856/347",
      ContentName: "Direction",
      ContentTitle: "Направления соединения",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1349,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1350,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    BaseParameterModifier: {
      ContentId: 351,
      ContentPath: "/339:1542/383:1869/424:1857/351",
      ContentName: "BaseParameterModifier",
      ContentTitle: "Модификаторы базовых параметров продуктов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1361,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1362,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
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
          FieldType: "StringEnum",
          IsRequired: false
        }
      }
    },
    ParameterModifier: {
      ContentId: 352,
      ContentPath: "/339:1542/383:1869/424:1858/352",
      ContentName: "ParameterModifier",
      ContentTitle: "Модификаторы параметров продуктов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1364,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1365,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    Unit: {
      ContentId: 355,
      ContentPath: "/339:1542/383:1869/424:1862/355",
      ContentName: "Unit",
      ContentTitle: "Единицы измерения",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Alias: {
          FieldId: 2062,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Title: {
          FieldId: 1384,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        Display: {
          FieldId: 1385,
          FieldName: "Display",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 3,
          FieldType: "String",
          IsRequired: false
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
          FieldType: "StringEnum",
          IsRequired: false
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
          FieldType: "StringEnum",
          IsRequired: false
        }
      }
    },
    ParameterChoice: {
      ContentId: 488,
      ContentPath: "/339:1542/383:1869/424:2685/488",
      ContentName: "ParameterChoice",
      ContentTitle: "Варианты выбора для параметров",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 2379,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 2380,
          FieldName: "Alias",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        OldSiteId: {
          FieldId: 2382,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 4,
          FieldType: "Numeric",
          IsRequired: false
        }
      }
    },
    ProductParameterGroup: {
      ContentId: 378,
      ContentPath: "/339:1542/383:1869/424:1852/378",
      ContentName: "ProductParameterGroup",
      ContentTitle: "Группы параметров продуктов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        SortOrder: {
          FieldId: 1500,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 3,
          FieldType: "Numeric",
          IsRequired: false
        },
        Title: {
          FieldId: 1496,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 2049,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        ImageSvg: {
          FieldId: 2029,
          FieldName: "ImageSvg",
          FieldTitle: "Изображение",
          FieldDescription: "",
          FieldOrder: 7,
          FieldType: "File",
          IsRequired: false
        },
        Type: {
          FieldId: 2061,
          FieldName: "Type",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 8,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    ProductParameterGroup1: {
      ContentId: 378,
      ContentPath: "/339:1542/383:2531/511:2530/361:1431/362:1678/378",
      ContentName: "ProductParameterGroup",
      ContentTitle: "Группы параметров продуктов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1496,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 2049,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        SortOrder: {
          FieldId: 1500,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 3,
          FieldType: "Numeric",
          IsRequired: false
        },
        ImageSvg: {
          FieldId: 2029,
          FieldName: "ImageSvg",
          FieldTitle: "Изображение",
          FieldDescription: "",
          FieldOrder: 7,
          FieldType: "File",
          IsRequired: false
        }
      }
    },
    LinkModifier: {
      ContentId: 360,
      ContentPath: "/339:1542/383:2531/511:2530/361:1450/360",
      ContentName: "LinkModifier",
      ContentTitle: "Модификаторы связей",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1413,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1414,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    ProductModifer: {
      ContentId: 342,
      ContentPath: "/339:1542/383:1653/342",
      ContentName: "ProductModifer",
      ContentTitle: "Модификаторы продуктов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 1339,
          FieldName: "Title",
          FieldTitle: "Название",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Alias: {
          FieldId: 1340,
          FieldName: "Alias",
          FieldTitle: "Псевдоним",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        }
      }
    },
    Advantage: {
      ContentId: 446,
      ContentPath: "/339:1542/383:2028/446",
      ContentName: "Advantage",
      ContentTitle: "Преимущества маркетинговых продуктов",
      ContentDescription: "",
      IsExtension: false,
      Fields: {
        Title: {
          FieldId: 2024,
          FieldName: "Title",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 1,
          FieldType: "String",
          IsRequired: false
        },
        Text: {
          FieldId: 2025,
          FieldName: "Text",
          FieldTitle: "Текстовые данные",
          FieldDescription: "",
          FieldOrder: 2,
          FieldType: "String",
          IsRequired: false
        },
        Description: {
          FieldId: 2362,
          FieldName: "Description",
          FieldTitle: "Описание",
          FieldDescription: "",
          FieldOrder: 3,
          FieldType: "Textbox",
          IsRequired: false
        },
        ImageSvg: {
          FieldId: 2026,
          FieldName: "ImageSvg",
          FieldTitle: "Изображение",
          FieldDescription: "",
          FieldOrder: 4,
          FieldType: "File",
          IsRequired: false
        },
        SortOrder: {
          FieldId: 2027,
          FieldName: "SortOrder",
          FieldTitle: "Порядок",
          FieldDescription: "",
          FieldOrder: 5,
          FieldType: "Numeric",
          IsRequired: false
        },
        IsGift: {
          FieldId: 2514,
          FieldName: "IsGift",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 6,
          FieldType: "Boolean",
          IsRequired: false
        },
        OldSiteId: {
          FieldId: 2515,
          FieldName: "OldSiteId",
          FieldTitle: "",
          FieldDescription: "",
          FieldOrder: 7,
          FieldType: "Numeric",
          IsRequired: false
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
        object.include = includeContent;
      } else if (object.FieldId) {
        if (
          object.Content && (
            object.FieldType === "M2ORelation" ||
            object.FieldType === "O2MRelation" ||
            object.FieldType === "M2MRelation"
          )
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