using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("D7693C00-7DCF-4e58-A783-0F00BA1F771F")]


  public class PathManager
  {
    /// <summary>
    /// A public event for sending process error messages back to the calling application.
    /// </summary>
    public event SendErrorMessage ErrorMessage;


    /// <summary>
    /// Constructor Method.
    /// </summary>
		public PathManager()
		{
      string                                                     applicationDirectory = null;
      string                                                     xmlSettingsFile      = null;
      PDX.BTS.DataMaintenance.MaintTools.XMLSetttingsFileManager xmlFileManager       = null;

      //  Set the Name of the XML Settings File for this Application.
      applicationDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
      while (applicationDirectory.EndsWith(@"\") == false)
      {
        //  Drop the right-most character from the string.
        applicationDirectory = applicationDirectory.Substring(0, (applicationDirectory.Length - 1));
      }
      xmlSettingsFile = applicationDirectory + @"DataMaintenanceUtilitiesSettings.xml";


      //  Initialize the XML File Manager.
      xmlFileManager = new PDX.BTS.DataMaintenance.MaintTools.XMLSetttingsFileManager();
      xmlFileManager.XMLFile(xmlSettingsFile);

      if (!System.IO.File.Exists(xmlSettingsFile))
      {
        //  Let the user know that the Parameter Table could not be initialized.
        if (ErrorMessage != null)
        {
          ErrorMessage("     Could not find the XML Settings File!  Initialization Failed!");
        }

        //  Exit this process.
        return;

      }

      //  Get the connection parameters for the Monitor Database from the XML Settings File.
      _monitorDatabaseServerName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "MonitorDatabaseServer");
      _monitorDatabaseName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "MonitorDatabase");


      //  Get the Name of the Load Monitor Table in the Monitor Database.
      _monitorTableName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "MonitorTableName");


			//  Set the Connection String that will be used to Connect to the Import Monitor Database.
			_importMonitorDatabaseConnectionString = "Data Source=" + _monitorDatabaseServerName + ";"
                                             + "Initial Catalog=" + _monitorDatabaseName + ";"
				                                     + "Integrated Security=SSPI";


      //  Exit this process.
      return;


		}   //  PathManagerClass


    public string BuildShapefileDirectoryPath(string ShareDirectory, string ShapefileDirectory, string ShapefileName)
    {
      try
      {
        //  Call the overload of this method that requires an indicator of whether the path should be built for the Publish Directory defaulting the value to FALSE.
        return BuildShapefileDirectoryPath(ShareDirectory, ShapefileDirectory, ShapefileName, false);

      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.BuildShapefileDirectoryPath() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  BuildShapefileDirectoryPath()


    public string BuildShapefileDirectoryPath(string ShareDirectory, string ShapefileDirectory, string ShapefileName, bool PublishPath)
    {
      try
      {
        //  If the dataset is in the "AppData$" share, make sure the "$" is at the end of the string.
        string displayShareDirectory = null;
        if (ShareDirectory.ToUpper().IndexOf("APPDATA") != -1)
        {
          //  If the "$" is not at the end of the Share Directory, add it.
          if (ShareDirectory.IndexOf("$") == -1)
          {
            //  Add the "$" to the end of the Share Directory.
            displayShareDirectory = ShareDirectory + "$";
          }
          else
          {
            displayShareDirectory = ShareDirectory;
          }

        }
        else
        {
          displayShareDirectory = ShareDirectory;

        }


        //  If the user is seeking the Publish Directory, include the "Publish$" share in the path.
        if (PublishPath)
        {
          //  Set the Appropriate Publish Share.
          if (displayShareDirectory.ToUpper().IndexOf(@"APPDATA$") != -1)
          {
            //  Set the Publish Share for "App Data" Shapefiles.
            displayShareDirectory = @"Publish$\AppData";
          }
          else
          {
            //  Set the Publish Share for the Public Shapefiles.
            displayShareDirectory = @"Publish$\Data";
          }

        }


        //  Get the Feature Class Name that this Shapefile Loads to from the Load Status Database.
        string featureClassName = GetFeatureClassName(ShareDirectory, ShapefileDirectory, ShapefileName);


        //  Look for special case Shapefile Directories (i.e., those that do not exist directly under
        //  the "Shapes" directory) and adjust the path for them.
        if ((ShapefileDirectory.ToUpper() == "ACCIDENT") || (ShapefileDirectory.ToUpper() == "STREETS") ||
            (ShapefileDirectory.ToUpper() == "PAVING") || (ShapefileDirectory.ToUpper() == "INFRASTRUCTURE") ||
            (ShapefileDirectory.ToUpper() == "STREETFEATURES") || (ShapefileDirectory.ToUpper() == "TRANSIT") ||
            (ShapefileDirectory.ToUpper() == "TRAILS"))
        {
          //  If the Feature Class is a GDT Dataset, set the path to the GDT Shapefile Location.  Otherwise,
          //  set the path to the Transportation Location.
          if (featureClassName.ToUpper().IndexOf("_GDT") != -1)
          {
            ShapefileDirectory = @"GDT\Shapes\" + ShapefileDirectory;
          }
          else
          {
            ShapefileDirectory = @"Transportation\" + ShapefileDirectory;
          }
        }

        else
        {
          if ((ShapefileDirectory.ToUpper() == "WATER") || (ShapefileDirectory.ToUpper() == "COMMUNICATIONS") ||
            (ShapefileDirectory.ToUpper() == "SEWER") || (ShapefileDirectory.ToUpper() == "VITAL"))
          {
            //  Set the path to the Utilities Location.
            ShapefileDirectory = @"Utilities\" + ShapefileDirectory;
          }
          else
          {
            if ((ShapefileDirectory.ToUpper() == "BOEC") || (ShapefileDirectory.ToUpper() == "FIRE") ||
                (ShapefileDirectory.ToUpper() == "POEM") || (ShapefileDirectory.ToUpper() == "POLICE"))
            {
              //  Set the path to the Public Safety Location.
              ShapefileDirectory = @"Public_Safety\" + ShapefileDirectory;
            }
            else
            {
              if ((ShapefileDirectory.ToUpper() == "FIREMAPS") ||
                  ((ShapefileDirectory.ToUpper() == "PARKS") && (displayShareDirectory.ToUpper() == "APPDATA$")))
              {
                //  Set the path to the Other Datasets Location.
                ShapefileDirectory = @"Other\" + ShapefileDirectory;
              }
              else
              {
                if (ShapefileDirectory.ToUpper() == "SHAPES")
                {
                  //  If hte Feature Class is a Navteq Dataset, set the path to the NavTeq Location.
                  if ((featureClassName.ToUpper().IndexOf("_NAV") != -1) ||
                      ((featureClassName.ToUpper().IndexOf("NT_ARTERIALS") != -1) &&
                       (featureClassName.ToUpper().IndexOf("ANNO_PDX") != -1)) ||
                      (featureClassName.ToUpper().IndexOf("HIGHWAYS_PDX") != -1))
                  {
                    ShapefileDirectory = @"NavTech\Shapes";
                  }
                }
              }
            }
          }

        }



        //  Build the Full Directory Path for the Shapefile.  If the Feature Class is a NavTeq, GDT or Other special dataset;
        //  do not include the "Shapes" directory in the full path.  Otherwise, include the "Shapes" directory in
        //  the path.
        string fullShapefileDirectoryPath = null;
        if ((featureClassName.ToUpper().IndexOf("_NAV") != -1) ||
            ((featureClassName.ToUpper().IndexOf("NT_ARTERIALS") != -1) &&
             (featureClassName.ToUpper().IndexOf("ANNO_PDX") != -1)) ||
            (featureClassName.ToUpper().IndexOf("HIGHWAYS_PDX") != -1) ||
            (featureClassName.ToUpper().IndexOf("_GDT") != -1) ||
            (ShapefileDirectory.ToUpper().IndexOf(@"OTHER\") != -1))
        {
          //  Do not include the "Shapes" directory in the path.
          fullShapefileDirectoryPath = @"\\CGISFILE\" + displayShareDirectory.ToLower() + @"\" + ShapefileDirectory.ToLower();

        }
        else
        {
          //  Include the "Shapes" directory in the path.
          fullShapefileDirectoryPath = @"\\CGISFILE\" + displayShareDirectory.ToLower() + @"\Shapes\" + ShapefileDirectory.ToLower();

        }


        //  Return the Full Shapefile Path to the Calling Routine.
        return fullShapefileDirectoryPath;

      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.BuildShapefileDirectoryPath() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  BuildShapefileDirectoryPath()


    public string BuildShapefilePath(string ShareDirectory, string ShapefileDirectory, string ShapefileName)
    {
      System.String shapefilePath = null;

      try
      {
        //  Call the Overload of this method requiring an indicator of whether or not the path
        //  being sought is the Publish or Production Path.  Default the PublishPath value to 
        //  FALSE so that the production path will be returned.
        shapefilePath = BuildShapefilePath(ShareDirectory, ShapefileDirectory, ShapefileName, false);


        //  Return the Shapefile Path that was generated to the calling method.
        return shapefilePath;

      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.PathManager.BuildShapefilePath() process FAILED with message:  " + caught.Message + "(Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  BuildShapefilePath()


    public string BuildShapefilePath(string ShareDirectory, string ShapefileDirectory, string ShapefileName, bool PublishPath)
    {
      try
      {
        //  Make sure the Shapefile Name ends with "*.shp".
        if (ShapefileName.ToUpper().IndexOf(".SHP") == -1)
        {
          ShapefileName = ShapefileName + ".shp";
        }


        //  Get the Directory Path for the Shapefile.
        string shapefileDirectoryPath = BuildShapefileDirectoryPath(ShareDirectory, ShapefileDirectory, ShapefileName, PublishPath);

        //  Make sure the Shapefile Directory Path was returned successfully.
        if (System.String.IsNullOrEmpty(shapefileDirectoryPath))
        {
          //  Let the user know that Shapefile Directory Path could not be determine.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Maintools.PathManager.BuildShapefilePath() process FAILED while determining the Shapefile Directory Path!");
          }

          //  Return a NULL String to the calling method to indicate that this method failed.
          return null;

        }


        //  Return the Full Shapefile Path to the Calling Routine.
        return shapefileDirectoryPath + @"\" + ShapefileName.ToLower();

      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.BuildShapefilePath() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  BuildShapefilePath()


    public string GetFeatureDataset(string ShapefilePath)
    {
      string shareDirectory     = null;
      string shapefileName      = null;
      string shapefileDirectory = null;
      string featureDatasetName = null;

      try
      {
        //  Make sure a valid Shapefile was passed to this method before moving on.
        if (ShapefilePath.IndexOf(".") != (ShapefilePath.Length - 4))
        {
          //  Since the file passed was not necessarily a Shapefile, return a NULL string to indicate that the
          //  Feature Dataset could not be determined.
          return "";

        }


        //  Get the Share Directory from the Shapefile Path.
        shareDirectory = ParseShapefilePath(ShapefilePath, "ShareDirectory");


        //  Get the File Name of the Shapefile from the Shapefile Path.
        shapefileName = ParseShapefilePath(ShapefilePath, "ShapefileName");


        //  Get the name of the directory that houses the Shapefile from the Shapefile Path.
        shapefileDirectory = ParseShapefilePath(ShapefilePath, "ShapefileDirectory");


        //  Call the overload of the GetFeatureDataset Method that requires the Shapefile Parameters.
        featureDatasetName = GetFeatureDataset(shareDirectory, shapefileDirectory, shapefileName);


        //  Return the Feature Dataset value to the calling routine.
        return featureDatasetName;

      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetFeatureDataset() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  GetFeatureDataset()


		public string GetFeatureDataset(string ShareDirectory, string ShapefileDirectory, string ShapefileName)
		{
      string                              sqlStatement          = null;
      System.Data.SqlClient.SqlCommand    featureDatasetCommand = null;
      System.Data.SqlClient.SqlDataReader featureDatasetReader  = null;
      string                              featureDatasetName    = null;

			try
			{
        //  Confirm that there is a valid connection to the database before attempting to query tables in it.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Let the user know that there was a problem.
          if (ErrorMessage != null)
          {
            ErrorMessage("A connection to the ImportMonitor Database Could NOT be established!  MaintTools.PathManager.GetFeatureDataset() Failed!");
          }

          //  Return a NULL string to indicate that this process failed.
          return null;
        }


        //  Build the SQL Statement that will be used to retrieve the Feature Dataset from Database.
        sqlStatement = "SELECT Feature_Dataset "
                     + "FROM " + _monitorTableName + " "
                     + "WHERE Data_Source = '" + ShareDirectory + "' AND "
                     + "      Dataset = '" + ShapefileDirectory + "' AND "
                     + "      Input_Dataset_Name = '" + ShapefileName + "'";


        //  Build the Command Object that will be used to retrieve the information from the Database.
        featureDatasetCommand = new System.Data.SqlClient.SqlCommand();
        featureDatasetCommand.Connection = _importMonitorDatabaseConnection;
        featureDatasetCommand.CommandText = sqlStatement;


        //  Open a Data Reader and attempt to pull the Feature Dataset Information from the Database.
        featureDatasetReader = featureDatasetCommand.ExecuteReader();

        //  Close the Command Object since it is no longer needed.
        featureDatasetCommand = null;


        //  If a Feture Dataset Name was return from the Database, return it to the calling routine.
        if (featureDatasetReader.HasRows)
        {
          //  Pull the information from the Database.
          featureDatasetReader.Read();

          //  Get the Feature Dataset name from the record that was just retrieved from the Database.
          featureDatasetName = (string)featureDatasetReader["Feature_Dataset"];

          //  Close the Data Reader.
          featureDatasetReader.Close();

          //  Return the value to the calling routine.
          return featureDatasetName;

        }
        else
        {
          //  Close the Data Reader.
          featureDatasetReader.Close();

        }


        //  Since the Feature Dataset Name was not successfully retrieved from the Database, attempt to find it without
        //  specifying the "Share Directory" in the query.
        sqlStatement = "SELECT Feature_Dataset "
                     + "FROM " + _monitorTableName + " "
                     + "WHERE Dataset = '" + ShapefileDirectory + "' AND "
                     + "      Input_Dataset_Name = '" + ShapefileName + "'";


        //  Build the Command Object that will be used to retrieve the information from the Database.
        featureDatasetCommand = new System.Data.SqlClient.SqlCommand();
        featureDatasetCommand.Connection = _importMonitorDatabaseConnection;
        featureDatasetCommand.CommandText = sqlStatement;


        //  Open a Data Reader and attempt to pull the Feature Dataset Information from the Database.
        featureDatasetReader = featureDatasetCommand.ExecuteReader();

        //  Close the Command Object since it is no longer needed.
        featureDatasetCommand = null;


        //  If a Feture Dataset Name was return from the Database, return it to the calling routine.
        if (featureDatasetReader.HasRows)
        {
          //  Pull the information from the Database.
          featureDatasetReader.Read();

          //  Get the Feature Dataset name from the record that was just retrieved from the Database.
          featureDatasetName = (string)featureDatasetReader["Feature_Dataset"];

          //  Close the Data Reader.
          featureDatasetReader.Close();

          //  Return the value to the calling routine.
          return featureDatasetName;

        }
        else
        {
          //  Close the Data Reader.
          featureDatasetReader.Close();

          //  Return a NULL string to indicate that the Feature Dataset could not be determined.
          return null;
        }

			}
			catch (Exception caught)
			{
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetFeatureDataset() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

			}

		}   //  GetFeatureDataset()


		public string GetFullLayerFilePath(string ShareDirectory, string ShapefileDirectory, string LayerFileName, string ShapefileName)
		{
			try
			{

				return "";

			}
			catch (Exception caught)
			{
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetFullLayerFilePath() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

			}

		}   //  GetFullLayerFilePath()


    public string GetSourceShapefileDirectoryfromFeatureClass(string FeatureClass)
    {
      string shapefileDirectory = null;

      try
      {
        //  Default the Feature Dataset to "STAND-ALONE" and call the overload of this method that requires a Feature
        //  Dataset argument.
        shapefileDirectory = GetSourceShapefileDirectoryfromFeatureClass("Stand-Alone", FeatureClass);


        //  Return the Shapefile Directory name that was just determined to the calling routine.
        return shapefileDirectory;

      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetSourceShapefileDirectoryfromFeatureClass() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

		}   //  GetSourceShapefileDirectoryfromFeatureClass()


		public string GetSourceShapefileDirectoryfromFeatureClass(string FeatureDataset, string FeatureClass)
		{

			string                              sqlStatement              = null;
			System.Data.SqlClient.SqlCommand    shapefileDirectoryCommand = null;
			System.Data.SqlClient.SqlDataReader shapefileDirectoryReader  = null;
			string                              shapefileDirectory        = null;

			try
			{
        //  Confirm that there is a valid connection to the database before attempting to query tables in it.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Let the user know that there was a problem.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not establish a connection to the ImportMonitor Database, MaintTools.PathManager.GetSourceShapefileDirectoryfromFeatureClass() FAILED!");
          }

          //  Return a NULL string to indicate that this process failed.
          return null;
        }


        //  Build the SQL Statement that will be used to Retrieve the Feature Class Name from the Table.
				sqlStatement = "SELECT Dataset "
                     + "FROM " + _monitorTableName + " "
					           + "WHERE ((Feature_Class_Name = '" + FeatureClass + "') AND "
					           + "       (Feature_Dataset = '" + FeatureDataset + "'))";

				//  Build the Command Object that will be used to retrieve the Shapefile Directory Name from the Table.
				shapefileDirectoryCommand = new System.Data.SqlClient.SqlCommand();
				shapefileDirectoryCommand.Connection = _importMonitorDatabaseConnection;
				shapefileDirectoryCommand.CommandText = sqlStatement;

				//  Populate a Data Reader using the Command Object.
				shapefileDirectoryReader = shapefileDirectoryCommand.ExecuteReader();


				//  If any information was retrieved, get the Shapefile Directory Name that was retrieved.
				if (shapefileDirectoryReader.HasRows)
				{
					//  Retrieve the data pulled from the Table.
					shapefileDirectoryReader.Read();

					//  Get the Shapefile Directory Name from the Data Reader.
					shapefileDirectory = (string) shapefileDirectoryReader["Dataset"];

				}


				//  Close the Data Reader.
				shapefileDirectoryReader.Close();


				//  Return the Feature Class Name to the calling routine.  If the name was not found, the returned
				//  string will be NULL.
				return shapefileDirectory;

			}
			catch (Exception caught)
			{
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetSourceShapefileDirectoryfromFeatureClass() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

			}

		}   //  GetSourceShapefileDirectoryfromFeatureClass()


    public string GetSourceShapefileShareDirectoryfromFeatureClass(string FeatureClass)
    {
      string shareDirectory = null;

      try
      {
        //  Default the Feature Dataset to "STAND-ALONE" and call the overload of this method that requires a Feature Dataset
        //  argument.
        shareDirectory = GetSourceShapefileShareDirectoryfromFeatureClass("Stand-Alone", FeatureClass);


        //  Return the Share Directory value that was just determined.
        return shareDirectory;


      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetSourceShapefileShareDirectoryfromFeatureClass() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  GetSourceShapefileShareDirectoryfromFeatureClass()


		public string GetSourceShapefileShareDirectoryfromFeatureClass(string FeatureDataset, string FeatureClass)
		{

			string                              sqlStatement          = null;
			System.Data.SqlClient.SqlCommand    shareDirectoryCommand = null;
			System.Data.SqlClient.SqlDataReader shareDirectoryReader  = null;
			string                              shareDirectory        = null;

			try
			{
        //  Confirm that there is a valid connection to the database before attempting to query tables in it.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Let the user know that there was a problem.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not connect to the Import Monitor Database, MaintTools.PathManager.GetSourceShapefileShareDirectoryfromFeatureClass() FAILED!");
          }

          //  Return a NULL string to indicate that this process failed.
          return null;
        }


        //  Build the SQL Statement that will be used to Retrieve the Share Directory Name from the Table.
				sqlStatement = "SELECT Data_Source "
                     + "FROM " + _monitorTableName + " "
					           + "WHERE ((Feature_Class_Name = '" + FeatureClass + "') AND "
					           + "       (Feature_Dataset = '" + FeatureDataset + "'))";


				//  Build the Command Object that will be used to retrieve the Share Directory Name from the Table.
				shareDirectoryCommand = new System.Data.SqlClient.SqlCommand();
				shareDirectoryCommand.Connection = _importMonitorDatabaseConnection;
				shareDirectoryCommand.CommandText = sqlStatement;

				//  Populate a Data Reader using the Command Object.
				shareDirectoryReader = shareDirectoryCommand.ExecuteReader();


				//  If any information was retrieved, get the Shapefile Directory Name that was retrieved.
				if (shareDirectoryReader.HasRows)
				{
					//  Retrieve the data pulled from the Table.
					shareDirectoryReader.Read();

					//  Get the Shapefile Directory Name from the Data Reader.
					shareDirectory = (string) shareDirectoryReader["Data_Source"];

				}


				//  Close the Data Reader.
				shareDirectoryReader.Close();


				//  Return the Feature Class Name to the calling routine.  If the name was not found, the returned
				//  string will be NULL.
				return shareDirectory;

			}
			catch (Exception caught)
			{
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.PathManager.GetSourceShapefileShareDirectoryfromFeatureClass() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

			}

		}   //  GetSourceShapefileShareDirectoryfromFeatureClass()


    public string GetSourceShapefileNamefromFeatureClass(string FeatureClass)
    {
      string shapefileName = null;

      try
      {
        //  Default the Feature Dataset to "STAND-ALONE" and call the overload of this method that requires a Feature
        //  Dataset argument.
        shapefileName = GetSourceShapefileNamefromFeatureClass("Stand-Alone", FeatureClass);


        //  Return the Shapefile Name that was just determined to the calling routine.
        return shapefileName;

      }
			catch (Exception caught)
			{
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetSourceShapefileNamefromFeatureClass() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

			}

    }   //  GetSourceShapefileNamefromFeatureClass()


    public string GetSourceShapefileNamefromFeatureClass(string FeatureDataset, string FeatureClass)
    {
      string                              sqlStatement         = null;
      System.Data.SqlClient.SqlCommand    shapefileNameCommand = null;
      System.Data.SqlClient.SqlDataReader shapefileNameReader  = null;
      string                              shapefileName        = null;

      try
      {
        //  Confirm that there is a valid connection to the database before attempting to query tables in it.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Let the user know that there was a problem.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not connect to the Import Monitor Database, MainTools.PathManager.GetSourceShapefileNamefromFeatureClass() FAILED!");
          }

          //  Return a NULL string to indicate that this process failed.
          return null;
        }


        //  Build the SQL Statement that will be used to determine the Shapefile Name.
        sqlStatement = "SELECT Input_Dataset_Name "
                     + "FROM " + _monitorTableName + " "
                     + "WHERE ((Feature_Class_Name = '" + FeatureClass + "') AND "
                     + "       (Feature_Dataset = '" + FeatureDataset + "'))";


        //  Build the Command Object that will be used to retrieve the Shapefile Name from the Table.
        shapefileNameCommand = new System.Data.SqlClient.SqlCommand();
        shapefileNameCommand.Connection = _importMonitorDatabaseConnection;
        shapefileNameCommand.CommandText = sqlStatement;

        //  Populate a Data Reader using the Command Object.
        shapefileNameReader = shapefileNameCommand.ExecuteReader();


        //  If any information was retrieved, get the Shapefile Name that was retrieved.
        if (shapefileNameReader.HasRows)
        {
          //  Retrieve the data pulled from the Table.
          shapefileNameReader.Read();

          //  Get the Shapefile Name from the Data Reader.
          shapefileName = (string)shapefileNameReader["Input_Dataset_Name"];

        }
        else
        {
          //  Set the Shapefile Name to a NULL String.
          shapefileName = null;

        }


        //  Close the Data Reader.
        shapefileNameReader.Close();


        //  Return the Shapefile Name to the calling routine.  If the name was not found, the returned
        //  string will be NULL.
        return shapefileName;

      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MainTools.PathManager.GetSourceShapefileNamefromFeatureClass() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  GetSourceShapefileNamefromFeatureClass()


    public string GetSourcePersonalGeoDatabasefromFeatureClass(string FeatureDataset, string FeatureClass)
		{
			try
			{
        //  Confirm that there is a valid connection to the database before attempting to query tables in it.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Let the user know that there was a problem.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not connect to the Import Monitor Database, Maintools.PathManager.GetSourcePersonalGeoDatabasefromFeatureClass() FAILED!");
          }

          //  Return a NULL string to indicate that this process failed.
          return null;
        }



				return "";

			}
			catch (Exception caught)
			{
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetSourcePersonalGeoDatabasefromFeatureClass() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

			}

		}   //  GetSourcePersonalGeoDatabasefromFeatureClass()


    public string GetFeatureClassName(string ShapefilePath)
    {
      string shareDirectory     = null;
      string shapefileName      = null;
      string shapefileDirectory = null;
      string featureClassName   = null;

      try
      {
        //  Make sure a valid Shapefile was passed to this method before moving on.
        if (ShapefilePath.IndexOf(".shp") != (ShapefilePath.Length - 4))
        {
          //  Since the file passed was not necessarily a Shapefile, return a NULL string to indicate that the
          //  Feature Dataset could not be determined.
          return "";

        }


        //  Get the Share Directory from the Shapefile Path.
        shareDirectory = ParseShapefilePath(ShapefilePath, "ShareDirectory");


        //  Get the File Name of the Shapefile from the Shapefile Path.
        shapefileName = ParseShapefilePath(ShapefilePath, "ShapefileName");


        //  Get the name of the directory that houses the Shapefile from the Shapefile Path.
        shapefileDirectory = ParseShapefilePath(ShapefilePath, "ShapefileDirectory");


        //  Call the overload of the GetFeatureClassName Method that requires the Shapefile Parameters.
        featureClassName = GetFeatureClassName(shareDirectory, shapefileDirectory, shapefileName);


        //  Return the Feature Class Name value to the calling routine.
        return featureClassName;


      }
      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetFeatureClassName() process FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  GetFeatureClassName()


		public string GetFeatureClassName(string ShareDirectory, string ShapefileDirectory, String ShapefileName)
		{
			string                              sqlStatement            = null;
			System.Data.SqlClient.SqlCommand    featureClassNameCommand = null;
			System.Data.SqlClient.SqlDataReader featureClassNameReader  = null;
			string                              featureClassName        = null;

			try
			{
        //  Make sure that there is a valid connection to the Database.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Let the user know that there was a problem.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not connect to the Import Monitor Database, Maintools.PathManager.GetFeatureClassName() FAILED!");
          }

          //  Return a NULL string to indicate that this process failed.
          return null;
        }


        //  Build the SQL Statement that will be used to retrieve the Feature Class Name from the
				//  GeoDatabase_Load_Status Table in the ImportMonitor Database.
				sqlStatement = "SELECT Feature_Class_Name "
                     + "FROM " + _monitorTableName + " "
                     + "WHERE Data_Source = '" + ShareDirectory + "' AND "
                     + "      Dataset = '" + ShapefileDirectory + "' AND "
                     + "      Input_Dataset_Name = '" + ShapefileName + "'";

				//  Create the Command Object that will be used to retrieve the Feature Class Name from the
				//  Table.
				featureClassNameCommand = new System.Data.SqlClient.SqlCommand();
				featureClassNameCommand.Connection = _importMonitorDatabaseConnection;
				featureClassNameCommand.CommandText = sqlStatement;

				//  Populate a the Feature Class Name Data Reader using the Command Object that was just created.
				featureClassNameReader = featureClassNameCommand.ExecuteReader();

        //  Close the Command Object since it is no longer needed.
        featureClassNameCommand = null;

				//  If the Feature Class Name was found in the Table, return it to the calling routine.
        if (featureClassNameReader.HasRows)
        {
          //  Retrieve the data pulled from the Table.
          featureClassNameReader.Read();

          //  Get the Feature Class Name from the Reader.
          featureClassName = (string)featureClassNameReader["Feature_Class_Name"];

          //  Close the Data Reader.
          featureClassNameReader.Close();

          //  Return the Feature Class Name to the calling routine.
          return featureClassName;

        }
        else
        {
          //  Close the Data Reader.
          featureClassNameReader.Close();

        }


        //  Since the Feature Class Name was not successfully retrieved from the Database attempt to find it without
        //  specifying the "Share Directory" in the query.
        sqlStatement = "SELECT Feature_Class_Name "
                     + "FROM " + _monitorTableName + " "
                     + "WHERE Dataset = '" + ShapefileDirectory + "' AND "
                     + "      Input_Dataset_Name = '" + ShapefileName + "'";

				//  Create the Command Object that will be used to retrieve the Feature Class Name from the
				//  Table.
				featureClassNameCommand = new System.Data.SqlClient.SqlCommand();
				featureClassNameCommand.Connection = _importMonitorDatabaseConnection;
				featureClassNameCommand.CommandText = sqlStatement;

				//  Populate a the Feature Class Name Data Reader using the Command Object that was just created.
				featureClassNameReader = featureClassNameCommand.ExecuteReader();

        //  Close the Command Object since it is no longer needed.
        featureClassNameCommand = null;

				//  If the Feature Class Name was found in the Table, return it to the calling routine.
        if (featureClassNameReader.HasRows)
        {
          //  Retrieve the data pulled from the Table.
          featureClassNameReader.Read();

          //  Get the Feature Class Name from the Reader.
          featureClassName = (string)featureClassNameReader["Feature_Class_Name"];

          //  Close the Data Reader.
          featureClassNameReader.Close();

          //  Return the Feature Class Name to the calling routine.
          return featureClassName;

        }
        else
        {
          //  Close the Data Reader.
          featureClassNameReader.Close();

          //  Return a NULL string to the calling routine to indicate that the Feature Class Name could not be determined.
          return null;

        }

			}
			catch(Exception caught)
			{
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.GetFeatureClassName() Method FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

			}

		}   //  GetFeatureClassName()


		private static bool ConnecttoImportMonitorDatabase()
		{

			try
			{
				//  If the connection is not currently open, create the connection and open it.  Otherwise, report
				//  that there is a valid connection to the database.
				if (_importMonitorDatabaseConnection != null)
				{

					//  If the connection is currently open, do nothing.  Otherwise, close the current connection, create
					//  a new one and open it.
					if (_importMonitorDatabaseConnection.State != System.Data.ConnectionState.Open)
					{
						//  If the Connection Object is instantiated but the state is not "OPEN", close the connection.
						_importMonitorDatabaseConnection.Close();
						//  Create and open a new connection to the database.
						_importMonitorDatabaseConnection = new SqlConnection();
						_importMonitorDatabaseConnection.ConnectionString = _importMonitorDatabaseConnectionString;
						_importMonitorDatabaseConnection.Open();
					}

				}
				else
				{

					//  Establish a connection to the database.
					_importMonitorDatabaseConnection = new SqlConnection();
					_importMonitorDatabaseConnection.ConnectionString = _importMonitorDatabaseConnectionString;
					_importMonitorDatabaseConnection.Open();

				}


				//  If there is a valid connection to the database, return a "TRUE" to the calling routine.  Otherwise,
				//  return a "FALSE" to indicate that the database is not available.
				if (_importMonitorDatabaseConnection.State == System.Data.ConnectionState.Open)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch
			{
				//  Return "FALSE to the calling routine to indicate that this process failed.
				return false;

			}

		}   //  ConnecttoImportMonitorDatabase


    private string ParseShapefilePath(string ShapefilePath, string ReturnElement)
    {
      string returnValue   = null;
      string directoryPath = null;

      try
      {
        //  Retrieve the piece of information the calling routine is asking for.
        switch (ReturnElement)
        {
          case "ShapefileName":
            //  Retrieve the File Name part of the Shapefile Path.
            returnValue = System.IO.Path.GetFileName(ShapefilePath);
            //  End this CASE.
            break;
          case "ShapefileDirectory":
            //  Retrieve the directory path from the Shapefile Path.
            directoryPath = System.IO.Path.GetDirectoryName(ShapefilePath);
            //  Strip the Path down to retrieve the name of the directory that houses the Shapefile.
            while (directoryPath.IndexOf(@"\") > -1)
            {
              //  Strip one character from the begining of the path.
              directoryPath = directoryPath.Substring(1);
            }
            //  Set the return value for the method.
            returnValue = directoryPath;
            //  End this CASE.
            break;
          case "ShareDirectory":
            //  Retrieve the File Path of the Shapefile.
            directoryPath = System.IO.Path.GetPathRoot(ShapefilePath);
            //  Get the Share Name from the path.
            while (directoryPath.IndexOf(@"\") > -1)
            {
              //  Drop one character from the Root Path.
              directoryPath = directoryPath.Substring(1);
            }
            //  Set the return value for the method.
            returnValue = directoryPath;
            //  End this CASE.
            break;
          default:
            //  End the DEFAULT CASE.
            break;
        }


        //  Return the desired value to the calling routine.
        return returnValue;

      }

      catch (Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.PathManager.ParseShapefilePath() Method FAILED with message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")");
        }

        //  Return a NULL string to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  ParseShapefilePath()

		//  The connection parameters for the Import Monitor Database.
    static string                                      _monitorDatabaseServerName             = null;
    static string                                      _monitorDatabaseName                   = null;
    static string                                      _monitorTableName                      = null;
		private static System.Data.SqlClient.SqlConnection _importMonitorDatabaseConnection       = null;
		static string                                      _importMonitorDatabaseConnectionString = null;


  }   //  Class:  PathManager

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools
