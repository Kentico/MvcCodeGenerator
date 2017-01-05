using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.OnlineForms;
using CMS.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Kentico.MvcCodeGenerator.Kentico.MvcCodeGenerator
{
    [EditedObject(BizFormInfo.OBJECT_TYPE, "formId")]
    [UIElement("Kentico.MvcCodeGenerator", "OnlineFormGenerator")]
    public partial class OnlineFormGenerator : GlobalAdminPage
    {
        #region Constants

        private const string LogSourceName = "MVC code generator";

        // The following can be eventually replaced with the module settings functionality in Kentico
        private const string OutputFolderPath = "~/MvcCodeGeneratorOutput/";
        private const string RepositoryModelRelativePath = "Models\\Generated\\Forms\\";
        private const string PartialViewRelativePath = "Views\\Shared\\";

        #endregion

        #region Variables

        private Infrastructure _infrastructure;
        protected BizFormInfo _bizFormInfo = null;
        private RepositoryModelTemplate _repositoryModelTemplate;
        private string _repositoryModelCodeContents;
        private PartialViewTemplate _partialViewTemplate;
        private string _partialViewCodeContents;

        private FormInfo mFormInfo;

        #endregion

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            btnGenerateCode.Click += btnGenerateCode_Click;
            btnSaveCode.Click += btnSaveCode_Click;

            _bizFormInfo = EditedObject as BizFormInfo;

            if (!IsPostBack)
            {
                fssSavePath.Value = OutputFolderPath;
            }
        }

        protected void btnGenerateCode_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateCode();
                PopulateUiBoxes();
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException(LogSourceName, "Generate", ex);
                ShowError(ex.Message);
            }
        }

        protected void btnSaveCode_Click(object sender, EventArgs e)
        {
            try
            {
                SaveCode();
                PopulateUiBoxes();
                ShowConfirmation(GetString("classes.code.filessavesuccess"));
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException(LogSourceName, "Save", ex);
                btnSaveCode.Enabled = false;
                ShowError(GetString("classes.code.filessaveerror") + $" Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Fills the text areas with the generated code.
        /// </summary>
        /// <remarks>Also replaces the text boxes with a proper names in an identifier format, if necessary.</remarks>
        private void PopulateUiBoxes()
        {
            // Re-populate the text boxes
            txtRepositoryModelNamespace.Text = _infrastructure.RepositoryModelNamespaceIdentifier;
            txtRepositoryModelClass.Text = _infrastructure.RepositoryModelClassIdentifier;
            txtActionResultMethod.Text = _infrastructure.ActionResultMethodIdentifier;
            txtControllerClass.Text = _infrastructure.ControllerClassIdentifier;

            // Populate the text areas
            txaRepositoryModelCode.Text = _repositoryModelCodeContents;
            txaPartialViewCode.Text = _partialViewCodeContents;
        }


        /// <summary>
        /// This method instantiates objects of the T4 templates, saves their references for later usage and populates the text areas with the generated code
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        protected void GenerateCode()
        {
            // Validation check
            if (_bizFormInfo == null)
            {
                throw new ArgumentOutOfRangeException(Infrastructure.ComposeErrorMessage("FormNotSelected"));
            }
            else
            {
                // Set the common infrastructure object
                _infrastructure = new Infrastructure(txtRepositoryModelNamespace.Text, txtRepositoryModelClass.Text, txtActionResultMethod.Text, txtControllerClass.Text, _bizFormInfo.FormClassID);

                // Get the object for the view model template
                GetRepositoryModelCode();

                // Do the same process for the partial view
                GetPartialViewCode();
            }
        }

        private void GetRepositoryModelCode()
        {
            _repositoryModelTemplate = new RepositoryModelTemplate(_infrastructure);

            // Get the generated code
            try
            {
                _repositoryModelCodeContents = _repositoryModelTemplate.TransformText();
            }
            catch (Exception ex)
            {
                string errorMessage = Infrastructure.ComposeErrorMessage("RepositoryModelTemplateTransformationFailed", ex);
                throw new Exception(errorMessage, ex);
            }
        }

        private void GetPartialViewCode()
        {
            // The second parameter might be bound to the 'chkMvcVersion' checkbox if needed. The checkbox is commented out currently.
            _partialViewTemplate = new PartialViewTemplate(_infrastructure, true);

            try
            {
                _partialViewCodeContents = _partialViewTemplate.TransformText();
            }
            catch (Exception ex)
            {
                string errorMessage = Infrastructure.ComposeErrorMessage("PartialViewTemplateTransformationFailed", ex);
                throw new Exception(errorMessage, ex);
            }
        }

        protected void PrepareFormInfo(int formClassId)
        {
            if (formClassId > 0)
            {
                DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(formClassId);

                if (dci != null)
                {
                    try
                    {
                        mFormInfo = new FormInfo(dci.ClassFormDefinition);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(Infrastructure.ComposeErrorMessage("IncorrectFormDefinition", ex), ex);
                    }
                }
                else
                {
                    throw new Exception(Infrastructure.ComposeErrorMessage("MissingDataClass"));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(Infrastructure.ComposeErrorMessage("FormNotSelected"));
            }
        }


        /// <summary>
        /// Generates the code and saves it using predefined relative paths
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        /// <remarks>The <see cref="OutputFolderPath"/> field can be replaced with the Kentico module settings functionality.</remarks>
        protected void SaveCode()
        {
            // Get the path from the text box
            string path = ValidationHelper.GetString(fssSavePath.Value, String.Empty);

            // Substitute missing path if necessary
            if (String.IsNullOrEmpty(path))
            {
                path = OutputFolderPath;
                fssSavePath.Value = path;
            }

            // Generate the code
            GenerateCode();

            try
            {
                // Write the view model code
                WriteFileContent(path, RepositoryModelRelativePath, $"{_infrastructure.RepositoryModelClassIdentifier}RepositoryModel.generated.cs", _repositoryModelCodeContents);

                // Write the partial view code
                WriteFileContent(path, PartialViewRelativePath, $"{_infrastructure.RepositoryModelClassIdentifier}Partial.cshtml", _partialViewCodeContents);
            }
            catch (Exception ex)
            {
                string errorMessage = Infrastructure.ComposeErrorMessage("FileWriteFailed", ex);
                throw new Exception(errorMessage, ex);
            }
        }


        /// <summary>
        /// Saves text content to a file using the specified path fragments.
        /// </summary>
        /// <param name="baseFolderPath">A path to the folder where code files should be created using naming conventions.</param>
        /// <param name="relativeFolderPath">A relative path within the base folder.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="content">Content to save.</param>
        /// <remarks>The path should always be a relative path in the application folder. Otherwise the <see cref="DirectoryHelper.EnsureDiskPath(string, string)"/> might not be able to work.</remarks>
        private void WriteFileContent(string baseFolderPath, string relativeFolderPath, string fileName, string content)
        {
            string baseFsPath = URLHelper.GetPhysicalPath(baseFolderPath);
            string folderPath = Path.Combine(baseFsPath, relativeFolderPath);
            string filePath = Path.Combine(baseFsPath, relativeFolderPath, fileName);

            DirectoryHelper.EnsureDiskPath(folderPath, SystemContext.WebApplicationPhysicalPath);

            using (var writer = File.CreateText(filePath))
            {
                writer.Write(content);
            }
        }

        #endregion
    }
}