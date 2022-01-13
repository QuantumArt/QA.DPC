namespace QA.Core.DPC.Loader.Services
{
    public class DefinitionSearchResult
    {
        public bool Found { get; set; }
		
        public bool FoundExplicitly { get; set; }
		
        public bool LoadAllPlainFieldsAtContentLevel { get; set; }
    }
}