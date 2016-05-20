
namespace QA.Core.Models.Entities
{
    public class RegionTag
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Value { get; set; }
        /// <summary>
        /// Идентификатор региона, для которого в итоге было выбрано значение
        /// </summary>
        public int RegionId { get; set; }
    }

    public class RegionTagValue : TagValue
    {
        public int RegionTagId { get; set; }

        public int Id { get; set; }
    }

    public class TagValue
    {
        public string Value { get; set; }
        public int[] RegionsId { get; set; }
    }

    public class TagWithValues
    {
        public string Title { get; set; }

        public TagValue[] Values { get; set; }
    }
}
