using System.Linq;
using QA.Core.DPC.UI.Controls.QP;
using QA.Core.Models.UI;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.UI.Controls
{
    public class ActionLink : Label
    {
        /// <summary>
        /// Экшн. Если не указано, то используется 
        /// </summary>
        private string _actionName;

        #region Properties
        public string ActionName
        {
            get
            {
                if (ResolveActionCode && !string.IsNullOrEmpty(_actionName))
                {
                    var code = ObjectFactoryBase.Resolve<ISettingsService>()
                        .GetActionCode(_actionName);

                    return code;
                }
                return _actionName;
            }
            set { _actionName = value; }
        }


        public QPBehavior Behavior { get; set; }

        public ItemList<InitFieldValue> FieldValues { get; private set; }

        public bool ShowIcon { get; set; }
        public string IconClass { get; set; }
        public bool ResolveActionCode { get; set; }
        public bool ShowInTab { get; set; }

        public object EntityId
        {
            get { return (object)GetValue(EntityIdProperty); }
            set { SetValue(EntityIdProperty, value); }
        }


        public object ParentEntityId
        {
            get { return (object)GetValue(ParentEntityIdProperty); }
            set { SetValue(ParentEntityIdProperty, value); }
        }
        #endregion

        static ActionLink() { }
        public ActionLink()
        {
            FieldValues = new ItemList<InitFieldValue>(this);
            ActionName = "edit_article";
        }

        #region Methods
        public FieldValue[] GetInitFieldValues()
        {
            if (ActionName != "new_article")
                return null;

            return FieldValues.Select(x => new FieldValue
            {
                fieldName = FieldValue.GetName(x),
                value = x.AsArray ? new object[] { x.Value } : x.Value
            }).ToArray();
        }

        public string[] GetFieldsToBlock()
        {
            if (CurrentItem == null || Behavior == null || Behavior.FieldsToBlock == null)
                return new string[] { };

            return Behavior.FieldsToBlock.ToArray();
        }

        public string[] GetFieldsToHide()
        {
            if (CurrentItem == null || Behavior == null || Behavior.FieldsToHide == null)
                return new string[] { };

            return Behavior.FieldsToHide.ToArray();
        }
        #endregion



        // Using a DependencyProperty as the backing store for ParentEntityId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentEntityIdProperty =
            DependencyProperty.Register("ParentEntityId", typeof(object), typeof(ActionLink));


        // Using a DependencyProperty as the backing store for EntityId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EntityIdProperty =
            DependencyProperty.Register("EntityId", typeof(object), typeof(ActionLink));
    }
}
