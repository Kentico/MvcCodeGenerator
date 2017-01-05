using CMS.FormEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.MvcCodeGenerator.Kentico.MvcCodeGenerator
{
    public partial class PartialViewTemplate
    {
        #region Variables

        private readonly Infrastructure _infrastructure;
        private bool _isHtmlAttributesSupported;

        #endregion

        #region Constructors

        public PartialViewTemplate(Infrastructure infrastructure, bool isMvc51)
        {
            _infrastructure = infrastructure;
            _isHtmlAttributesSupported = isMvc51;
        }

        #endregion

        public string RenderHelperMethod(FormFieldInfo field)
        {
            if (!Infrastructure.IsHidden(field))
            {
                return $"@Html.ValidatedEditorFor(model => model.{_infrastructure.PropertyNames[field]})";
            }
            else
            {
                return $"@Html.HiddenFor(model => model.{_infrastructure.PropertyNames[field]})";
            }
        }
    }
}
