
namespace QA.ProductCatalog.HighloadFront.Options
{
    public class SonicElasticStoreOptions
    {

        public SonicElasticStoreOptions()
        {
            DisjunctionMark = "|";
            NegationMark = "!";
            EscapeCharacter = "@";
            DateFormat = "yyyy-MM-dd'T'HH:mm:ss.fffzzz";
        }

        public int DefaultSize { get; set; }

        public string IdPath { get; set; }

        public string TypePath { get; set; }

        public bool UseCamelCase { get; set; }

        public string DefaultType { get; set; }

        public string[] DefaultFields { get; set; }

        public int MaxResultWindow { get; set; }

        public int TotalFieldsLimit { get; set; }

        public string[] Types { get; set; }

        public string[] DynamicDateFormats { get; set; }

        public string[] NotAnalyzedFields { get; set; }

        public string CreationDateField { get; set; }
        public string DateFormat { get; set; }

        public string ValueSeparator { get; set; }

        public string NegationMark { get; set; }

        public string DisjunctionMark { get; set; }

        public string WildcardStarMark { get; set; }

        public string WildcardQuestionMark { get; set; }

        public string EscapeCharacter { get; set; }

        public ArrayIndexingSettings[] IndexingOptions { get; set; }

    }
}
