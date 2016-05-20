
namespace QA.Core.Models.Entities
{
    public class FieldMetadata
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public virtual FieldType FieldType { get; set; }

        public int? ContentId { get; set; }
        public string ContentName { get; set; }
    }
}
