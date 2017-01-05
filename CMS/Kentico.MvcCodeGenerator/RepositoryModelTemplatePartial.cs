using CMS.Base;
using CMS.FormEngine;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.MvcCodeGenerator.Kentico.MvcCodeGenerator
{
    public partial class RepositoryModelTemplate
    {
        #region Variables

        private readonly Infrastructure _infrastructure;

        #endregion

        #region Constructors

        public RepositoryModelTemplate(Infrastructure infrastructure)
        {
            _infrastructure = infrastructure;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Produces a string with all supported <see cref="System.ComponentModel.DataAnnotation"/> attributes of a field.
        /// </summary>
        /// <param name="field"><see cref="FormFieldInfo"/> field to produce the attributes for</param>
        /// <param name="indentationChars">Indentation characters put in front of each attribute</param>
        /// <returns>String with all the attribute code</returns>
        private string GetAttributes(FormFieldInfo field, string indentationChars = "")
        {
            StringBuilder sb = new StringBuilder();
            bool firstLineRendered = false;

            if (Infrastructure.IsHidden(field))
            {
                Infrastructure.AppendNewLine(sb, "[HiddenInput]", firstLineRendered, indentationChars);
                firstLineRendered = true;
            }
            else
            {
                // Parse resource macros in field's title
                string resolvedResource = _infrastructure.ContextResolver.ResolveMacros(field.Caption);
                Infrastructure.AppendNewLine(sb, $"[Display(Name = \"{resolvedResource}\")]", firstLineRendered, indentationChars);
                firstLineRendered = true;
            }

            // Only supported data types
            if (GetAnnotationDataType(field) != null)
                Infrastructure.AppendNewLine(sb, $"[DataType({GetAnnotationDataType(field)})]", firstLineRendered, indentationChars);

            // May a BizForm field be disabled?
            //if (!field.Enabled)
            //    AppendNewLine(sb, "[ReadOnly]", firstLineRendered, indentationChars);

            // Save space in the database
            if (Infrastructure.GetDataType(field) == "string")
                Infrastructure.AppendNewLine(sb, "[DisplayFormat(ConvertEmptyStringToNull = true)]", firstLineRendered, indentationChars);

            // Expression needed by the RangeAttribute when only one boundary value is specified
            string rangeAttributeExpressions = string.Empty;

            if (Infrastructure.IsRangeableFieldtype(field))
            {
                rangeAttributeExpressions = ComposeRangeAttributes(field, indentationChars, sb, firstLineRendered);
            }

            // Produce the RegularExpressionAttribute
            // TODO: Check if the RegEx expression can be copied without transforming
            if (!string.IsNullOrEmpty(field.RegularExpression))
            {
                Infrastructure.AppendNewLine(sb, $"[RegularExpression(@\"{field.RegularExpression}\")]", firstLineRendered, indentationChars);
            }

            // Produce the RequiredAttribute
            if (!field.AllowEmpty)
                Infrastructure.AppendNewLine(sb, "[Required]", firstLineRendered, indentationChars);

            // Produce the StringLengthAttribute
            // The MaxLengthAttribute has no advantages when used with Kentico; hence the StringLengthAttribute
            if (Infrastructure.GetDataType(field) == "string")
                Infrastructure.AppendNewLine(sb, $"[StringLength({field.Size})]", firstLineRendered, indentationChars);

            // TODO: Implement the MembershipPasswordAttribute ???
            //if (field.FieldType == FormFieldControlTypeEnum.EncryptedPassword)

            return sb.ToString();
        }

        /// <summary>
        /// Produces the <see cref="System.ComponentModel.DataAnnotations.DataType"/> enum expression for the <see cref="System.ComponentModel.DataAnnotations.DataTypeAttribute"/> according to the field's form control type
        /// </summary>
        /// <param name="field">Field to examine for a control type</param>
        /// <returns>The <see cref="System.ComponentModel.DataAnnotations.DataType"/> enum expression</returns>
        /// <remarks>Except for the basic form controls, values comming from MVC forms may require additional transforming in their [HttpPost] action methods.</remarks>
        /// <remarks>Due to C# 'case' statement limitations the <see cref="FormFieldInfo.Settings"/> values cannot be evaluated dynamically via <see cref="Common.FormControlNames"/>.</remarks>
        private string GetAnnotationDataType(FormFieldInfo field)
        {
            string controlName = field.Settings["controlname"].ToString().ToLowerCSafe();

            switch (controlName)
            {
                // Setting the DataType.Custom attribute depends on current circumstances
                //case "customusercontrol":
                //    return "DataType.Custom";

                case "calendarcontrol":
                case "due_date_selector":
                    if ((string)field.Settings["EditTime"] == "True")
                    {
                        return "DataType.DateTime";
                    }
                    else
                    {
                        return "DataType.Date";
                    }

                case "dateintervalselector":
                case "timeintervalselector":
                    return "DataType.Duration";

                case "emailinput":
                    return "DataType.EmailAddress";

                case "htmlareacontrol":
                    return "DataType.Html";

                case "imagedialogselector":
                case "imageselectioncontrol":
                case "productimageselector":
                    return "DataType.ImageUrl";

                case "largetextarea":
                case "textareacontrol":
                    return "DataType.MultilineText";

                case "encryptedpassword":
                case "formpassword":
                case "password":
                case "passwordconfirmator":
                    return "DataType.Password";

                case "internationalphone":
                case "usphone":
                    return "DataType.PhoneNumber";

                case "uszipcode":
                    return "DataType.PostalCode";

                case "textboxcontrol":
                    return "DataType.Text";

                case "time_selector":
                    return "DataType.Time";

                case "directuploadcontrol":
                case "uploadcontrol":
                case "uploadfile":
                    return "DataType.Upload";

                case "go_to_external_url":
                case "urlselector":
                    return "DataType.Url";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Produces the literal value for the expression
        /// </summary>
        /// <param name="field">Field to examine for a control type</param>
        /// <param name="value">Value to reformat to the literal notation</param>
        /// <returns></returns>
        private string FormatValue(FormFieldInfo field, object value)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            switch (field.FieldType)
            {
                case FormFieldControlTypeEnum.DecimalNumberTextBox:
                    {
                        decimal retval = ValidationHelper.GetDecimal(value, 0m, culture);
                        return Infrastructure.EncloseInQuotes(retval > 0m ? retval.ToString(culture) : "0.0m");
                    }
                case FormFieldControlTypeEnum.IntegerNumberTextBox:
                case FormFieldControlTypeEnum.LongNumberTextBox:
                default:
                    {
                        return Infrastructure.EncloseInQuotes(value?.ToString());
                    }
            }
        }

        /// <summary>
        /// Produces the minimum and maximum value expressions for the specific type of the field.
        /// </summary>
        /// <param name="field">Field to examine for a control type</param>
        /// <param name="maxValue">Flag denoting if minimum or maximum value expression should be returned</param>
        /// <returns></returns>
        private string GetBoundaryValueExpression(FormFieldInfo field, bool maxValue)
        {
            if (Infrastructure.IsRangeableFieldtype(field))
            {
                switch (field.FieldType)
                {
                    case FormFieldControlTypeEnum.DecimalNumberTextBox:
                        {
                            return (maxValue) ? "Double.MaxValue" : "Double.MinValue";
                        }
                    case FormFieldControlTypeEnum.IntegerNumberTextBox:
                        {
                            return (maxValue) ? "Int32.MaxValue" : "Int32.MinValue";
                        }
                    case FormFieldControlTypeEnum.LongNumberTextBox:
                        {
                            return (maxValue) ? "Int64.MaxValue" : "Int64.MinValue";
                        }
                    default:
                        {
                            return null;
                        }
                }
            }
            else
            {
                throw new ArgumentException("The type of the field cannot have a minimum and maximum value.", "field");
            }
        }

        /// <summary>
        /// Composes the <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/> expression.
        /// </summary>
        /// <param name="field">Field to inspect for minimum and maximum value</param>
        /// <param name="indentationChars">Characters to put in front of the expression</param>
        /// <param name="sb">The <see cref="StringBuilder"/> instance to work with</param>
        /// <param name="firstLineRendered">Flag indicating if first line of the field's expression was already rendered</param>
        /// <returns>The resulting expression</returns>
        private string ComposeRangeAttributes(FormFieldInfo field, string indentationChars, StringBuilder sb, bool firstLineRendered)
        {
            // Prepare the typeof expression
            string typeofExpression = "typeof(" + Infrastructure.GetDataType(field) + "), ";

            // If only MaxValue is specified, infer the MinValue expression for this type
            if (!string.IsNullOrEmpty(field.MinValue) && string.IsNullOrEmpty(field.MaxValue))
            {
                Infrastructure.AppendNewLine(sb, $"[Range({typeofExpression}{FormatValue(field, field.MinValue)}, {GetBoundaryValueExpression(field, true)})]", firstLineRendered, indentationChars);
            }
            // If only MinValue is specified, infer the MaxValue expression for this type
            else if (string.IsNullOrEmpty(field.MinValue) && !string.IsNullOrEmpty(field.MaxValue))
            {
                Infrastructure.AppendNewLine(sb, $"[Range({typeofExpression}{GetBoundaryValueExpression(field, false)}, {FormatValue(field, field.MaxValue)})]", firstLineRendered, indentationChars);
            }
            // If both are specified, nothing has to be inferred at all
            else if (!string.IsNullOrEmpty(field.MinValue) && !string.IsNullOrEmpty(field.MaxValue))
            {
                Infrastructure.AppendNewLine(sb, $"[Range({typeofExpression}{FormatValue(field, field.MinValue)}, {FormatValue(field, field.MaxValue)})]", firstLineRendered, indentationChars);
            }

            return typeofExpression;
        }

        #endregion
    }
}
