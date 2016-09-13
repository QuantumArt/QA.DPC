using QA.Core.Models.Entities;

namespace QA.Core.Models.UI
{
    public class ModelObjectWithParent
    {
        public IModelObject ModelObject { get; set; }

        public IModelObject Parent { get; set; }
    }
}