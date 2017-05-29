
namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class SonicElasticStoreOptions
    {
        public int DefaultSize { get; set; }

        public string IdPath { get; set; }

        public string TypePath { get; set; }

        public bool UseCamelCase { get; set; }

        public string DefaultType { get; set; }

        public string[] DefaultFields { get; set; }

        public int MaxResultWindow { get; set; }

        public string[] Types { get; set; }

        public string[] NotAnalyzedFields { get; set; }

        public string CreationDateField { get; set; }

        public string ValueSeparator { get; set; }

        public string NegationMark { get; set; }

        public ArrayIndexingSettings[] ArrayIndexingSettings;

    }
}
