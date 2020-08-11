using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using QA.Core.DPC.Resources;
using Portable.Xaml.Markup;



namespace QA.Core.Models.Configuration
{
    /// <summary>
    /// класс, который описывает обратную сторону связи.
    /// в контенте может не быть такого поля, для него есть ответное поле
    /// </summary>
    [ContentProperty("Content")]
	public class BackwardRelationField : EntityField
    {
        private Tuple<string> _relationGroupName;

		[Display(Name="FieldNameForCard", ResourceType = typeof(ControlStrings))]
		[DefaultValue(null)]
		public string DisplayName
        {
            get { return FieldTitle; }
            set { FieldTitle = value; }
        }

        /// <summary>
        /// Имя группы для данной связи. Может быть несколько связей, которые могут потом объединяться в одну связь. 
        /// Для этого можно заполнить это поле одним значением.
        /// Если не указано иное, это поле совпадает с значением поля FieldName (т.е. группировка не используется)
        /// </summary>
        [Obsolete]
		public string RelationGroupName
        {
            get
            {
                return _relationGroupName != null ? _relationGroupName.Item1 : FieldName;
            }
            set
            {
                _relationGroupName = new Tuple<string>(value);
            }
        }
    }
}
