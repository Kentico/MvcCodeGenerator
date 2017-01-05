using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.MvcCodeGenerator
{
    public class Infrastructure
    {
        #region Variables

        private readonly string _repositoryModelNamespace;
        private readonly string _repositoryModelClass;
        private readonly string _controllerClass;
        private readonly string _actionResultMethod;
        private readonly FormInfo _formInfo;
        private readonly Dictionary<FormFieldInfo, string> _propertyNames;
        private readonly MacroResolver _contextResolver = MacroContext.CurrentResolver.CreateChild();

        private readonly List<string> _supportedFormControls = new List<string>
        {
            "decimalnumbertextbox",
            "emailinput",
            "encryptedpassword",
            "htmlareacontrol",
            "integernumbertextbox",
            "longnumbertextbox",
            "textareacontrol",
            "textboxcontrol",
        };

        #endregion

        #region Properties

        public string RepositoryModelNamespaceIdentifier
        {
            get
            {
                return Capitalize(_repositoryModelNamespace);
            }
        }

        public string RepositoryModelClassIdentifier
        {
            get
            {
                return GetCapitalizedIdentifier(_repositoryModelClass);
            }
        }

        public string ControllerClassIdentifier
        {
            get
            {
                return GetCapitalizedIdentifier(_controllerClass);
            }
        }

        public string ActionResultMethodIdentifier
        {
            get
            {
                return GetCapitalizedIdentifier(_actionResultMethod);
            }
        }

        /// <summary>
        /// Fields collection.
        /// </summary>
        /// <remarks>Should there be a need to include the primary key field, the last 'Where' call can be removed.</remarks>
        public IEnumerable<FormFieldInfo> Fields
        {
            get
            {
                return _formInfo.GetFields<FormFieldInfo>().Where(x => !x.System).Where(x => !x.PrimaryKey);
            }
        }

        public Dictionary<FormFieldInfo, string> PropertyNames
        {
            get
            {
                return _propertyNames;
            }
        }

        public MacroResolver ContextResolver
        {
            get
            {
                return _contextResolver;
            }
        }

        #endregion

        #region Constructors

        public Infrastructure(string repositoryModelNamespace, string repositoryModelClass, string actionResultMethod, string controller, int formClassId)
        {
            _repositoryModelNamespace = repositoryModelNamespace;
            _repositoryModelClass = repositoryModelClass;
            _actionResultMethod = actionResultMethod;
            _controllerClass = controller;
            _formInfo = SetFormInfo(formClassId);
            _propertyNames = CreateDictionaryWithPropertyNames();
        }

        #endregion

        #region Instance Methods

        private FormInfo SetFormInfo(int formClassId)
        {
            if (formClassId > 0)
            {
                DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(formClassId);

                if (dci != null)
                {
                    FormInfo formInfo;

                    try
                    {
                        formInfo = new FormInfo(dci.ClassFormDefinition);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ComposeErrorMessage("IncorrectFormDefinition", ex), ex);
                    }

                    return formInfo;
                }
                else
                {
                    throw new Exception(ComposeErrorMessage("MissingDataClass"));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(ComposeErrorMessage("FormNotSelected"));
            }
        }



        /// <summary>
        /// Checks if the field is supported for inclusion in the repository model and the partial view.
        /// </summary>
        /// <param name="field">Field to be checked</param>
        /// <returns>True if the field is supported</returns>
        public bool IsSupported(FormFieldInfo field)
        {
            string controlName;

            controlName = field.Settings["controlname"]?.ToString();

            if (controlName == null)
            {
                throw new NullReferenceException(ComposeErrorMessage("FieldControlNameMissing"));
            }

            return _supportedFormControls.Contains(controlName.ToLowerCSafe()) && GetDataType(field) != null;
        }


        /// <summary>
        /// Builds a dictionary for faster accessing of the properties using a <see cref="UniqueMemberNameGenerator"/> class.
        /// </summary>
        /// <returns>Dictionary of property names</returns>
        internal Dictionary<FormFieldInfo, string> CreateDictionaryWithPropertyNames()
        {
            var generator = new UniqueMemberNameGenerator(typeof(object), new string[] { RepositoryModelClassIdentifier }, string.Empty);

            return Fields.ToDictionary(f => f, f => generator.GetUniqueMemberName(f.Name, true));
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Composes a localized error message to be either displayed to the user or logged into the event log.
        /// </summary>
        /// <param name="suffix">Suffix to the &quot;ObjectType.Kentico.MvcCodeGenerator_Error&quot; prefix of the code name of the resource string key</param>
        /// <param name="ex">Optional exception object to be used to extract the exception message from</param>
        /// <returns>The localized error message</returns>
        public static string ComposeErrorMessage(string suffix, Exception ex = null)
        {
            string message;

            message = ResHelper.GetString("ObjectType.Kentico.MvcCodeGenerator_Error" + suffix);

            if (ex != null)
            {
                message += $" Details: {ex.Message}";
            }

            return message;
        }

        public static bool IsHidden(FormFieldInfo field)
        {
            return !field.Visible || !field.PublicField;
        }

        public static string EncloseInQuotes(string input)
        {
            return "\"" + input + "\"";
        }

        /// <summary>
        /// Takes a reference to an existing <see cref="StringBuilder"/> object and appends a text as a new line. 
        /// </summary>
        /// <param name="sbInstance">The <see cref="StringBuilder"/> object reference</param>
        /// <param name="input">Input text</param>
        /// <param name="firstLineRendered">Flag indicating if this is a first line (which shouldn't be appended as a new line)</param>
        /// <param name="indentationChars">Spaces or tabs used for indenting each line</param>
        public static void AppendNewLine(StringBuilder sbInstance, string input, bool firstLineRendered, string indentationChars = "")
        {
            string newLine = firstLineRendered ? Environment.NewLine : string.Empty;
            sbInstance.Append(newLine + indentationChars + input);
        }

        /// <summary>
        /// Creates an identifier name that contains only allowed types of characters.
        /// </summary>
        /// <param name="originalName">Original name to be parsed to the identifier format</param>
        /// <returns></returns>
        public static string GetCapitalizedIdentifier(string originalName)
        {
            int dotIndex = originalName.LastIndexOfCSafe('.');

            if (dotIndex >= 0)
            {
                originalName = originalName.Substring(dotIndex + 1);
            }

            originalName = ValidationHelper.GetIdentifier(originalName);

            return Capitalize(originalName);
        }

        public static string Capitalize(string originalName)
        {
            return originalName[0].ToString().ToUpperCSafe() + originalName.Substring(1);
        }

        /// <summary>
        /// Gets the corresponding CLR data type for the <see cref="FieldDataType"/> Kentico type. 
        /// </summary>
        /// <param name="field">The field to inspect for the data type</param>
        /// <returns>The CLR data type that corresponds to the <see cref="FieldDataType"/> type</returns>
        public static string GetDataType(FormFieldInfo field)
        {
            switch (field.DataType.ToLowerCSafe())
            {
                case FieldDataType.Text:
                case FieldDataType.LongText:
                case FieldDataType.DocAttachments:
                    return "string";

                case FieldDataType.Integer:
                    return "int";

                case FieldDataType.LongInteger:
                    return "long";

                case FieldDataType.Double:
                    return "double";

                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return "DateTime";

                case FieldDataType.Boolean:
                    return "bool";

                case FieldDataType.File:
                case FieldDataType.Guid:
                    return "Guid";

                case FieldDataType.Decimal:
                    return "decimal";

                case FieldDataType.TimeSpan:
                    return "TimeSpan";

                default:
                    throw new ArgumentOutOfRangeException("FormFieldInfo.DataType", "Specified datatype is not supported.");
            }
        }

        /// <summary>
        /// Checks whether the field is of one of the types that can have minimum and maximum values.
        /// </summary>
        /// <param name="field">The field to inspect for the range</param>
        /// <returns>True if the field can have minimum and maximum values</returns>
        public static bool IsRangeableFieldtype(FormFieldInfo field)
        {
            switch (field.DataType)
            {
                case FieldDataType.Date:
                case FieldDataType.DateTime:
                case FieldDataType.Decimal:
                case FieldDataType.Double:
                case FieldDataType.Integer:
                case FieldDataType.TimeSpan:
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }

        #endregion
    }
}
