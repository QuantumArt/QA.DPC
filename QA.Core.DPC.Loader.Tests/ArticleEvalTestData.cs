using System.Collections.Generic;
using System.Linq;
using QA.Core.Models.Entities;

namespace QA.Core.DPC.Loader.Tests
{
    public partial class ArticleEvalTests
    {
        private static readonly List<DPathFilterData> EmptyFilterData = Enumerable.Empty<DPathFilterData>().ToList();

        public static IEnumerable<object[]> GetDPathExpressionData
        {
            get
            {
                yield return new object[]
                {
                    "Parameters[!Modifiers/Alias='ExcludeFromPdf']",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = "Parameters",
                            FiltersData = new List<DPathFilterData>
                            {
                               new DPathFilterData
                               {
                                   Expression = "Modifiers/Alias",
                                   Value = "ExcludeFromPdf",
                                   IsInversed = true
                               }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    "PDF",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = "PDF",
                            FiltersData = EmptyFilterData
                        }
                    }
                };

                yield return new object[]
                {
                    "[ SortOrder ='300'][ !SortOrder= '200']",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = string.Empty,
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "SortOrder",
                                    Value = "300",
                                    IsInversed = false,
                                    IsDisjuncted = false
                                },
                                new DPathFilterData
                                {
                                    Expression = "SortOrder",
                                    Value = "200",
                                    IsInversed = true,
                                    IsDisjuncted = false
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    "[!SortOrder= '200'][SortOrder  ='300']",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = string.Empty,
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "SortOrder",
                                    Value = "200",
                                    IsInversed = true
                                },
                                new DPathFilterData
                                {
                                    Expression = "SortOrder",
                                    Value = "300"
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    "[Type=' 305 ']",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = string.Empty,
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "Type",
                                    Value = "305"
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    "[Type='305'][Type='305']",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = string.Empty,
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "Type",
                                    Value = "305"
                                },
                                new DPathFilterData
                                {
                                    Expression = "Type",
                                    Value = "305"
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    "[!Type='306'][|!Type='306  ']",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = string.Empty,
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "Type",
                                    Value = "306",
                                    IsInversed = true
                                },
                                new DPathFilterData
                                {
                                    Expression = "Type",
                                    Value = "306",
                                    IsInversed = true,
                                    IsDisjuncted = true
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    "   MarketingProduct [ ProductType = '289' ]",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = "MarketingProduct",
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "ProductType",
                                    Value = "289"
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    " MarketingProduct [ ProductType / ProductFilters / SortOrder = ' 1 ' ] ",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = "MarketingProduct",
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "ProductType / ProductFilters / SortOrder",
                                    Value = "1"
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    "MarketingProduct/ ProductType /ProductFilters[SortOrder='1']   ",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = "MarketingProduct",
                            FiltersData = EmptyFilterData
                        },
                        new DPathArticleData
                        {
                            FieldName = "ProductType",
                            FiltersData = EmptyFilterData
                        },
                        new DPathArticleData
                        {
                            FieldName = "ProductFilters",
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "SortOrder",
                                    Value = "1"
                                }
                            }
                        }
                    }
                };

                yield return new object[]
                {
                    " MarketingProduct/ ProductType/  ProductFilters[ SortOrder =  '  1 ' ] / Title ",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = "MarketingProduct",
                            FiltersData = EmptyFilterData
                        },
                        new DPathArticleData
                        {
                            FieldName = "ProductType",
                            FiltersData = EmptyFilterData
                        },
                        new DPathArticleData
                        {
                            FieldName = "ProductFilters",
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "SortOrder",
                                    Value = "1"
                                }
                            }
                        },
                        new DPathArticleData
                        {
                            FieldName = "Title",
                            FiltersData = EmptyFilterData
                        }
                    }
                };

                yield return new object[]
                {
                    " field1 [ ! filter1_1_1 / filter1_1_2 / filter1_1_3 = ' value1_1 ' ] [ & filter1_2_1 / filter1_2_2 / filter1_2_3 = \n ' value1_2 ' ] \r   / field2 / field3 [ filter3_1_1/ filter3_1_2 = ' value3_1 ' ] [    |     !   filter3_2_1    =    '    value3_2    '    ]   \r\n    /    field4   ",
                    new[]
                    {
                        new DPathArticleData
                        {
                            FieldName = "field1",
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "filter1_1_1 / filter1_1_2 / filter1_1_3",
                                    Value = "value1_1",
                                    IsInversed = true
                                },
                                new DPathFilterData
                                {
                                    Expression = "filter1_2_1 / filter1_2_2 / filter1_2_3",
                                    Value = "value1_2"
                                }
                            }
                        },
                        new DPathArticleData
                        {
                            FieldName = "field2",
                            FiltersData = EmptyFilterData
                        },
                        new DPathArticleData
                        {
                            FieldName = "field3",
                            FiltersData = new List<DPathFilterData>
                            {
                                new DPathFilterData
                                {
                                    Expression = "filter3_1_1/ filter3_1_2",
                                    Value = "value3_1"
                                },
                                new DPathFilterData
                                {
                                    Expression = "filter3_2_1",
                                    Value = "value3_2",
                                    IsInversed = true,
                                    IsDisjuncted = true
                                }
                            }
                        },
                        new DPathArticleData
                        {
                            FieldName = "field4",
                            FiltersData = EmptyFilterData
                        }
                    }
                };
            }
        }
    }
}
