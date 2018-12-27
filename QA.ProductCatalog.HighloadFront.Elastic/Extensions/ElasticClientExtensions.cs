namespace QA.ProductCatalog.HighloadFront.Elastic.Extensions
{
    public static class ElasticClientExtensions
    {

  



//            if (types != null && fields != null && types.Any() && fields.Any())
//            {
//                foreach (var type in types)
//                {
//                    descriptor = descriptor.Map(type, m => m.DynamicTemplates(d => {
//                        foreach (var field in fields)
//                        {
//                            d = d.DynamicTemplate($"analyzed_{type}_{field}", t => t.Match(field).MatchMappingType("string").Mapping(mf => mf.Keyword(f => f)));
//                        }
//
//                        return d;
//                    }));
//                }
//            }
//
//    return descriptor;
//        }
//
//        public static MappingsDescriptor MapAnalyzed(this MappingsDescriptor descriptor, string[] types, string[] fields)
//        {
//            if (types != null && fields != null && types.Any() && fields.Any())
//            {
//                foreach (var type in types)
//                {
//                    descriptor = descriptor.Map(type, m => m.DynamicTemplates(d => {
//                        foreach (var field in fields)
//                        {
//                            d = d.DynamicTemplate($"analyzed_{type}_{field}", t => t.Match(field).MatchMappingType("string").Mapping(mf => mf.Text(f => f)));
//                        }
//
//                        d = d.DynamicTemplate($"analyzed_{type}_all", t => t.Match("*").MatchMappingType("string").Mapping(mf => mf.Keyword(f => f)));
//
//                        return d;
//                    }));
//                }
//            }
//
//            return descriptor;
//        }           
    }
}