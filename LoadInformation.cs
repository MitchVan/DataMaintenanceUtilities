using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("6A22F7A9-E909-459e-A8F8-F581732B7696")]

  
  public class LoadInformation
  {

    /// <summary>
    /// A public event for sending process error messages back to the calling application.
    /// </summary>
    public event SendErrorMessage ErrorMessage;


    //  Constructor Method.
		public LoadInformation()
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


      //  Get the Name of the Load Monitor Table in the Monitor Database.
      _parameterTableName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "LoadParemetersTableName");


      //  Set the Connection String that will be used to Connect to the Parameter Database.
      _parameterDatabaseConnectionString = "Data Source=" + _monitorDatabaseServerName + ";"
                                         + "Initial Catalog=" + _monitorDatabaseName + ";"
                                         + "Integrated Security=SSPI";


      //  Exit this process.
      return;


    }   //  LoadInformation


		public string GetFeatureDataset(string ShareDirectory, string ShapefileDirectory, string ShapefileName)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetFeatureDataset() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetFeatureDataset


		public string GetFeatureClassfromShapefile(string Shapefile)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetFeatureClassfromShapefile() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetFeatureClassfromShapefile


		public string GetFeatureClassfromShapefile(string ShapefileDirectoryPath, string ShapefileName)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetFeatureClassfromShapefile() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetFeatureClassfromShapefile


		public string GetFeatureDatasetfromShapefile(string Shapefile)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetFeatureDatasetfromShapefile() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetFeatureDatasetfromShapefile


		public string GetFeatureDatasetfromShapefile(string ShapefileDirectoryPath, string ShapefileName)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetFeatureDatasetfromShapefile() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetFeatureDatasetfromShapefile


		public string Get8XFeatureDatasetfromShapefile(string Shapefile)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The Get8XFeatureDatasetfromShapefile() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  Get8XFeatureDatasetfromShapefile


		public string Get8XFeatureDatasetfromShapefile(string ShapefileDirectoryPath, string ShapefileName)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The Get8XFeatureDatasetfromShapefile() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  Get8XFeatureDatasetfromShapefile


		public  string GetOutputFeatureDatasetfromFeatureClass(string InputFeatureDataset, string InputFeatureClass)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetOutputFeatureDatasetfromFeatureClass() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetOutputFeatureDatasetfromFeatureClass


		public string Get8XFeatureDatasetfromFeatureClass(string InputFeatureDataset, string InputFeatureClass)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The Get8XFeatureDatasetfromFeatureClass() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  Get8XFeatureDatasetfromFeatureClass


		public string GetFeatureClassfromFeatureClass(string InputFeatureDataset, string InputFeatureClass)
		{

			try
			{
				//  Return a blank string for now.
				return "";

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetFeatureClassfromFeatureClass() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetFeatureClassfromFeatureClass


    public string GetSourceTypefromFeatureClass(string FeatureClass)
    {
      try
      {
        //  Call the overload of this method requiring a Feature Dataset Parameter defaulting the parameter to an empty string.
        return GetSourceTypefromFeatureClass("", FeatureClass);

      }
      catch (Exception caught)
      {
        //  Return a message to the calling routine indicating that this process failed.
        return "The GetSourceTypefromFeatureClass() process FAILED with message:  " + caught.Message;

      }

    }


    public string GetSourceTypefromFeatureClass(string FeatureDataset, string FeatureClass)
		{
			System.Data.SqlClient.SqlCommand    sourceTypeCommand = null;
			System.Data.SqlClient.SqlDataReader sourceTypeReader  = null;

      try
      {
        //  Build the SQL Statement that will be used to Retrieve the Source Type from the Table.
        string getSourceInfoSQLStatement = null;
        if (!System.String.IsNullOrEmpty(FeatureDataset))
        {
          getSourceInfoSQLStatement = "SELECT Load_Source "
                                    + "FROM " + _monitorTableName + " "
                                    + "WHERE Feature_Class_Name = '" + FeatureClass + "' AND "
                                    + "      ((Feature_Dataset = '" + FeatureDataset + "') OR "
                                    + "       (Old_Feature_Dataset = '" + FeatureDataset + "'))";

        }
        else
        {
          getSourceInfoSQLStatement = "SELECT Load_Source "
                                    + "FROM " + _monitorTableName + " "
                                    + "WHERE Feature_Class_Name = '" + FeatureClass + "'";

        }


        //  Build the Command Object that will be used to retrieve the Source Type from the Table.
        sourceTypeCommand = new System.Data.SqlClient.SqlCommand();
        sourceTypeCommand.Connection = _importMonitorDatabaseConnection;
        sourceTypeCommand.CommandType = System.Data.CommandType.Text;
        sourceTypeCommand.CommandText = getSourceInfoSQLStatement;
        sourceTypeCommand.CommandTimeout = 30;


        //  Populate a Data Reader using the Command Object.
        sourceTypeReader = sourceTypeCommand.ExecuteReader();


        //  If any information was retrieved, get the Source Type that was retrieved.
        string sourceTypeValue = "Unknown";
        if (sourceTypeReader.HasRows)
        {
          //  Retrieve the data pulled from the Table.
          sourceTypeReader.Read();

          //  Get the Source Type from the Data Reader.
          sourceTypeValue = (string)sourceTypeReader["Load_Source"];

        }


        //  Return the Source Type to the calling routine.  If the name was not found, the returned string will be NULL.
        return sourceTypeValue;

      }
      catch (Exception caught)
      {
        //  Return a message to the calling routine indicating that this process failed.
        return "The GetSourceTypefromFeatureClass() process FAILED with message:  " + caught.Message;

      }
      finally
      {
        //  If the SQL Client Source Type SQL Data Reader Object was instantiated, close it.
        if (sourceTypeReader != null)
        {
          if (!sourceTypeReader.IsClosed)
          {
            sourceTypeReader.Close();
          }
          sourceTypeReader.Dispose();
          sourceTypeReader = null;
        }
        //  If the SQL Client Source Type SQL Command Object was instantiated, close it.
        if (sourceTypeCommand != null)
        {
          sourceTypeCommand.Dispose();
          sourceTypeCommand = null;
        }

      }

		}   //  GetSourceTypefromFeatureClass


		public string GetSourceFeatureClassfromFeatureClass(string FeatureDataset, string FeatureClass)
		{
			string                              sqlStatement        = null;
			System.Data.SqlClient.SqlCommand    featureClassCommand = null;
			System.Data.SqlClient.SqlDataReader featureClassReader  = null;
			string                              featureClassName    = null;

			//  Confirm that there is a valid connection to the database before attempting to query tables in it.
			if (!ConnecttoImportMonitorDatabase())
			{
				//  Return an explanation of the process failure.
				return "Could not connect to the Import Monitor Database, GetFeatureClassName() FAILED!";
			}


			try
			{
				//  Build the SQL Statement that will be used to Retrieve the Feature Class Name from the Table.
				sqlStatement = "SELECT Input_Dataset_Name "
                     + "FROM " + _monitorTableName + " "
					           + "WHERE Feature_Class_Name = '" + FeatureClass + "' AND "
					           + "      ((Feature_Dataset = '" + FeatureDataset + "') OR "
					           + "       (Old_Feature_Dataset = '" + FeatureDataset + "'))";


				//  Build the Command Object that will be used to retrieve the Feature Class Name from the Table.
				featureClassCommand = new System.Data.SqlClient.SqlCommand();
				featureClassCommand.Connection = _importMonitorDatabaseConnection;
				featureClassCommand.CommandText = sqlStatement;

				//  Populate a Data Reader using the Command Object.
				featureClassReader = featureClassCommand.ExecuteReader();


				//  If any information was retrieved, get the Feature Class Name that was retrieved.
				if (featureClassReader.HasRows)
				{
					//  Retrieve the data pulled from the Table.
					featureClassReader.Read();

					//  Get the Feature Class Name from the Data Reader.
					featureClassName = (string) featureClassReader["Input_Dataset_Name"];

				}


				//  Close the Data Reader.
				featureClassReader.Close();


				//  Return the Feature Class Name to the calling routine.  If the name was not found, the returned
				//  string will be NULL.
				return featureClassName;

			}
			catch (Exception caught)
			{
				//  Return a message to the calling routine indicating that this process failed.
				return "The GetSourceFeatureClassfromFeatureClass() process FAILED with message:  " + caught.Message;

			}

		}   //  GetSourceFeatureClassfromFeatureClass


		public string GetLastLoadDatefromFeatureClass(string FeatureDataset, string FeatureClass, string ServerName,
			                                            string SDEInstance)
		{
			string                              sqlStatement        = null;
			System.Data.SqlClient.SqlCommand    lastLoadDateCommand = null;
			System.Data.SqlClient.SqlDataReader lastLoadDateReader  = null;
			System.DateTime                     lastLoadDate        = System.DateTime.Now;

			//  Confirm that there is a valid connection to the database before attempting to query tables in it.
			if (!ConnecttoImportMonitorDatabase())
			{
				//  Return an explanation of the process failure.
				return "Could not connect to the Import Monitor Database, GetLastLoadDatefromFeatureClass() FAILED!";
			}

			try
			{
				//  Build the SQL Statement that will be used to Retrieve the Last Load Date from the Table.
				sqlStatement = "SELECT " + ServerName + "_" + SDEInstance + "_Last_Load_Date "
                     + "FROM " + _monitorTableName + " "
					           + "WHERE Feature_Class_Name = '" + FeatureClass + "' AND "
					           + "      ((Feature_Dataset = '" + FeatureDataset + "') OR "
					           + "       (Old_Feature_Dataset = '" + FeatureDataset + "'))";


				//  Build the Command Object that will be used to retrieve the Last Load Date from the Table.
				lastLoadDateCommand = new System.Data.SqlClient.SqlCommand();
				lastLoadDateCommand.Connection = _importMonitorDatabaseConnection;
				lastLoadDateCommand.CommandText = sqlStatement;

				//  Populate a Data Reader using the Command Object.
				lastLoadDateReader = lastLoadDateCommand.ExecuteReader();


				//  If any information was retrieved, get the Last Load Date that was retrieved.
				if (lastLoadDateReader.HasRows)
				{
					//  Retrieve the data pulled from the Table.
					lastLoadDateReader.Read();

					//  Get the Last Load Date from the Data Reader.
					lastLoadDate = (System.DateTime) lastLoadDateReader[ServerName + "_" + SDEInstance + "_Last_Load_Date"];

				}


				//  Close the Data Reader.
				lastLoadDateReader.Close();


				//  Return the Last Load Date to the calling routine.  If the date was not found, the returned
				//  string will be NULL.
				return lastLoadDate.ToString("d MMMM yyyy");

			}
			catch (Exception caught)
			{
				//  Let the user know that this process failed.
				return "The GetLastLoadDatefromFeatureClass() Method FAILED with message:  " + caught.Message.ToString();

			}

		}   //  GetLastLoadDatefromFeatureClass


    public System.DateTime GetFeatureClassSourceLastUpdateDate(string FeatureClassName, string OutputGeodatabaseName)
    {
      PDX.BTS.DataMaintenance.MaintTools.FeatureClassSourceInfo currentFeatureClassSourceInfo = null;
      PDX.BTS.DataMaintenance.MaintTools.PathManager            featureClassPathManager       = null;
      PDX.BTS.DataMaintenance.MaintTools.FeatureClassUtilities  featureClassUtilities         = null;

      try
      {
        //  Determine the Source Information for the specified Feature Class.
        currentFeatureClassSourceInfo = GetLayerSourceInfo(FeatureClassName, OutputGeodatabaseName);

        //  Make sure the Source Info was retrieved successfully before moving on.
        if (currentFeatureClassSourceInfo == null)
        {
          //  Let the User know that the Information was not retrieved successfully.
          if (ErrorMessage != null)
          {
            ErrorMessage("Failed to retrieve the Source Information for the " + FeatureClassName + " Feature Class.  The GetFeatureClassSourceUpdateDate() Method Failed!");
          }

          //  Return a date of "1/1/1900" to the calling method to indicate that this method failed.
          return System.DateTime.Parse("1/1/1900 00:00:00.000");

        }


        //  Instantiate the Feature Class Path Manager Object that will be used, where necessary, to determine the path to the specified
        //  Source Dataset.
        featureClassPathManager = new PDX.BTS.DataMaintenance.MaintTools.PathManager();


        //  Instantiate the Feature Class Utilities Object that will be used to determine the Source Feature Class Last Update Date for the
        //  specified Feature Class.
        featureClassUtilities = new PDX.BTS.DataMaintenance.MaintTools.FeatureClassUtilities();


        //  Determine the Last Update Date of the Source Dataset for the specified Feature Class.
        switch (currentFeatureClassSourceInfo.SourceType.ToUpper())
        {
          case "SHAPEFILE":
            //  Determine the Path to the Source Shapefile for the specified Feature Class.
            string sourceShapefilePath = featureClassPathManager.BuildShapefilePath(currentFeatureClassSourceInfo.SourceDataShare, currentFeatureClassSourceInfo.SourceDataset, currentFeatureClassSourceInfo.SourceFeatureClassName);
            //  Determine the Last Update of the Source Shapefile.
            return featureClassUtilities.GetShapefileLastUpdateDate(sourceShapefilePath);
          case "PERSONALGEODB":
            //  Determine the Last Update Date for the Source Personal Geodatabase Feature Class for the specified Output Feature Class.
            return featureClassUtilities.GetPersonalGeoDBLastUpdateDate(currentFeatureClassSourceInfo.SourceGeoDBFile);
          case "FILEGEODB":
            //  Determine the Last Update Date for the Source File Geodatabase Feature Class for the specified Output Feature Class.
            return featureClassUtilities.GetFileGeodatabaseLastUpdate(currentFeatureClassSourceInfo.SourceGeoDBFile);
          case "GEODATABASE":
            //  Determine the Last Update Date for the Source Enterprise Geodatabasde Feature Class for the specified Output Feature Class.
            return featureClassUtilities.GetEntGeoDBFeatureClassLastUpdate(currentFeatureClassSourceInfo.SourceFeatureClassName, currentFeatureClassSourceInfo.SourceServerName, currentFeatureClassSourceInfo.SourceDatabaseName, currentFeatureClassSourceInfo.SourceDatabaseUser);
          default:
            //  Return a date of "!/1/1900" to the calling method to indicate that the date could not be determined.
            return System.DateTime.Parse("1/1/1900 00:00:00.000");
        }

      }
      catch (System.Exception caught)
      {
        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("");
          ErrorMessage("");
          ErrorMessage("The FeatureClassUtilities.GetFeatureClassSourceLastUpdateDate() Method failed with error message - " + caught.Message + "!");
        }

        //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
        return new System.DateTime(1900, 1, 1);

      }
      finally
      {
        //  If the CGIS Data Maintenance Tools Feature Class Utilities Object was instantiated, close it.
        if (featureClassUtilities != null)
        {
          featureClassUtilities.Dispose();
          featureClassUtilities = null;
        }
        //  If the CGIS Data Maintenance Tools Feature Class Path Manager Object was instantiated, close it.
        if (featureClassPathManager != null)
        {
          featureClassPathManager = null;
        }
 
      }

    }   //  GetFeatureClassSourceLastUpdateDate()


    public PDX.BTS.DataMaintenance.MaintTools.FeatureClassSourceInfo GetLayerSourceInfo(string FeatureClassName)
    {
      try
      {
        //  Call the Overload of this method requiring an Output Server Name, an Output Instance Value and an Output Geodatabase Name
        //  defaulting the values to empty strings and returning the value to the calling method.
        return GetLayerSourceInfo(FeatureClassName, "");

      }
      catch (Exception caught)
      {
        //  Send a Message to the calling application to let the user know why this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("    The LoaderUtilities.DatabaseTools.GetLayerSourceInfo() Method Failed with error message:  " + caught.Message);
        }

        //  Return a NULL String to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  GetLayerSourceInfo()


    public PDX.BTS.DataMaintenance.MaintTools.FeatureClassSourceInfo GetLayerSourceInfo(string FeatureClassName, string OutputGeodatabaseName)
    {
      System.Data.SqlClient.SqlCommand                         getSourceInfoSQLCommand     = null;
      System.Data.SqlClient.SqlDataReader                      getSourceInfoSQLDataReader  = null;
      ESRI.ArcGIS.esriSystem.IPropertySet                      sourcePropertySet           = null;
      PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities      maintenanceGeneralUtilities = null;
      PDX.BTS.DataMaintenance.MaintTools.PathManager           shapefilePathGenerator      = null;
      PDX.BTS.DataMaintenance.MaintTools.FeatureClassUtilities featureClassUtilities       = null;

      try
      {
        //  Make sure there is a valid connection to the Load Metadata Database before attempting to update it.
        if (!ConnecttoImportMonitorDatabase())
        {
          //  Send a Message to the calling application to let the user know why this process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("     Could not establish a connection to the Load Metadata Database!  LoaderUtilities.DatabaseTools.GetLayerSourceInfo() Failed!");
          }

          //  Return a NULL String to the calling routine to indicate that this process failed.
          return null;

        }


        //  Build the SQL Statement that will be used to retrieve the Source Information for the Feature Class.
        string getSourceInfoSQLStatement = "SELECT [Load_Source], [Data_Source], [Dataset], [GeoDB_File], [Source_Server_Name],"
                                         + "       [Source_Instance], [Source_Database_Name], [Source_Database_User_Name],"
                                         + "       [Input_Dataset_Name], [Output_Database] "
                                         + "FROM " + _monitorTableName + " "
                                         + "WHERE [Feature_Class_Name] = '" + FeatureClassName + "'";


        //  If the Output Geodatabase Name was passed to this method, include it in the query.
        if (!System.String.IsNullOrEmpty(OutputGeodatabaseName))
        {
          //  Add the Output Geodatabase Name to the SQL Statement.
          getSourceInfoSQLStatement = getSourceInfoSQLStatement + " AND [Output_Database] = '" + OutputGeodatabaseName + "'";

        }


        //  Build the SQL Command Object that will be used to retrieve the Source Data Info for the specified Feature Class.
        getSourceInfoSQLCommand = new System.Data.SqlClient.SqlCommand();
        getSourceInfoSQLCommand.Connection = _importMonitorDatabaseConnection;
        getSourceInfoSQLCommand.CommandType = System.Data.CommandType.Text;
        getSourceInfoSQLCommand.CommandText = getSourceInfoSQLStatement;
        getSourceInfoSQLCommand.CommandTimeout = 30;


        //  Use the SQL Command Object that was just instantiated to populate a SQL Data Reader Object with the info about the specified
        //  Feature Class.
        getSourceInfoSQLDataReader = getSourceInfoSQLCommand.ExecuteReader();


        //  Instantiate a Feature Class Source Info Object.
        PDX.BTS.DataMaintenance.MaintTools.FeatureClassSourceInfo currentFeatureClassSourceInfo = new PDX.BTS.DataMaintenance.MaintTools.FeatureClassSourceInfo();

        //  If some information was retrieved, populate a Feature Class Source Info Object and return it to the calling method.  If not,
        //  return a NULL Pointer to indicate that this method failed.
        if (getSourceInfoSQLDataReader.HasRows)
        {
          if (getSourceInfoSQLDataReader.Read())
          {
            //  Populate the New Feature Class Source Info Object with the Source Info for the specified Feature Class.
            //  Populate the Source Type Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Load_Source")))
            {
              currentFeatureClassSourceInfo.SourceType = (string)getSourceInfoSQLDataReader["Load_Source"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceType = "";
            }
            //  Populate the Source Data Share Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Data_Source")))
            {
              currentFeatureClassSourceInfo.SourceDataShare = (string)getSourceInfoSQLDataReader["Data_Source"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceDataShare = "";
            }
            //  Populate the Source Dataset Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Dataset")))
            {
              currentFeatureClassSourceInfo.SourceDataset = (string)getSourceInfoSQLDataReader["Dataset"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceDataset = "";
            }
            //  Populate the Source Geodatabase File Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("GeoDB_File")))
            {
              currentFeatureClassSourceInfo.SourceGeoDBFile = (string)getSourceInfoSQLDataReader["GeoDB_File"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceGeoDBFile = "";
            }
            //  Populate the Source Server Name Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Source_Server_Name")))
            {
              currentFeatureClassSourceInfo.SourceServerName = (string)getSourceInfoSQLDataReader["Source_Server_Name"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceServerName = "";
            }
            //  Populate the Source Instance Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Source_Instance")))
            {
              currentFeatureClassSourceInfo.SourceInstance = (string)getSourceInfoSQLDataReader["Source_Instance"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceInstance = "";
            }
            //  Populate the Source Database Name Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Source_Database_Name")))
            {
              currentFeatureClassSourceInfo.SourceDatabaseName = (string)getSourceInfoSQLDataReader["Source_Database_Name"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceDatabaseName = "";
            }
            //  Populate the Source Database User Name Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Source_Database_User_Name")))
            {
              currentFeatureClassSourceInfo.SourceDatabaseUser = (string)getSourceInfoSQLDataReader["Source_Database_User_Name"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceDatabaseUser = "";
            }
            //  Populate the Input Feature Class Name Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Input_Dataset_Name")))
            {
              currentFeatureClassSourceInfo.SourceFeatureClassName = (string)getSourceInfoSQLDataReader["Input_Dataset_Name"];
            }
            else
            {
              currentFeatureClassSourceInfo.SourceFeatureClassName = "";
            }
            //  Populate the Output Geodatabase Property.
            if (!getSourceInfoSQLDataReader.IsDBNull(getSourceInfoSQLDataReader.GetOrdinal("Output_Database")))
            {
              currentFeatureClassSourceInfo.OutputGeodatabaseName = (string)getSourceInfoSQLDataReader["Output_Database"];
            }
            else
            {
              currentFeatureClassSourceInfo.OutputGeodatabaseName = "";
            }
          }
          else
          {
            //  Send a Message to the calling application to let the user know why this process failed.
            if (ErrorMessage != null)
            {
              ErrorMessage("     No Source Information for the Specified Feature Class was found in the Load Metadata Database!  LoaderUtilities.DatabaseTools.GetLayerSourceInfo() Failed!");
            }
            //  Return a NULL String to the calling routine to indicate that this process failed.
            return null;
          }

        }
        else
        {
          //  Send a Message to the calling application to let the user know why this process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("     The query to retrieve Source Information for the Specified Feature Class failed to retrieve any data from the Load Metadata Database!  LoaderUtilities.DatabaseTools.GetLayerSourceInfo() Failed!");
          }

          //  Return a NULL String to the calling routine to indicate that this process failed.
          return null;

        }


        //  Instantiate a Feature Class Utilities Object.
        featureClassUtilities = new PDX.BTS.DataMaintenance.MaintTools.FeatureClassUtilities();


        //  Build the Property Set for the Source Dataset and determine its Last Update Date.
        sourcePropertySet = new ESRI.ArcGIS.esriSystem.PropertySet();
        switch (currentFeatureClassSourceInfo.SourceType)
        {
          case "FILEGEODB":
            //  Build the File Geodatabase PropertySet.
            sourcePropertySet.SetProperty("DATABASE", currentFeatureClassSourceInfo.SourceGeoDBFile);
            //  Determine the File Geodatabase Feature Class Last Update Date.
            currentFeatureClassSourceInfo.SourceLastUpdateDate = featureClassUtilities.GetFileGeodatabaseFeatureClassLastUpdate(currentFeatureClassSourceInfo.SourceGeoDBFile, currentFeatureClassSourceInfo.SourceFeatureClassName);
            //  Exit this case.
            break;
          case "PERSONALGEODB":
            //  Build the Personal Geodatabase PropertySet.
            sourcePropertySet.SetProperty("DATABASE", currentFeatureClassSourceInfo.SourceGeoDBFile);
            //  Determine the Source Personal Geodatabase Last Update Date.
            currentFeatureClassSourceInfo.SourceLastUpdateDate = featureClassUtilities.GetPersonalGeoDBLastUpdateDate(currentFeatureClassSourceInfo.SourceGeoDBFile);
            //  Exit this case.
            break;
          case "GEODATABASE":
            //  Determine the Server User Password.
            maintenanceGeneralUtilities = new PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities();
            string serverUserPassword = maintenanceGeneralUtilities.RetrieveParameterValue(currentFeatureClassSourceInfo.SourceDatabaseUser + "_Password", _parameterTableName, _parameterDatabaseConnectionString, true);
            //  Build the Enterprise Geodatabase Property Set.
            sourcePropertySet.SetProperty("SERVER", currentFeatureClassSourceInfo.SourceServerName);
            sourcePropertySet.SetProperty("INSTANCE", currentFeatureClassSourceInfo.SourceInstance);
            sourcePropertySet.SetProperty("DATABASE", currentFeatureClassSourceInfo.SourceDatabaseName);
            sourcePropertySet.SetProperty("USER", currentFeatureClassSourceInfo.SourceDatabaseUser);
            sourcePropertySet.SetProperty("PASSWORD", serverUserPassword);
            sourcePropertySet.SetProperty("VERSION", "SDE.Default");
            //  Determine the Enterprise Geodatabase Feature Class Last Udpate Date.
            currentFeatureClassSourceInfo.SourceLastUpdateDate = featureClassUtilities.GetEntGeoDBFeatureClassLastUpdate(currentFeatureClassSourceInfo.SourceFeatureClassName, currentFeatureClassSourceInfo.SourceServerName, currentFeatureClassSourceInfo.SourceDatabaseName, currentFeatureClassSourceInfo.SourceDatabaseUser, serverUserPassword);
            //  Exit this case.
            break;
          case "SHAPEFILE":
            //  Instantiate a City of Portland Path Manager Object that will be used to determine the Path to the Source Shapefile.
            shapefilePathGenerator = new PDX.BTS.DataMaintenance.MaintTools.PathManager();
            string shapefileDirectoryPath = shapefilePathGenerator.BuildShapefileDirectoryPath(currentFeatureClassSourceInfo.SourceDataShare, currentFeatureClassSourceInfo.SourceDataset, currentFeatureClassSourceInfo.SourceFeatureClassName);
            //  Build the Shapefile PropertySet.
            sourcePropertySet.SetProperty("DATABASE", shapefileDirectoryPath);
            //  Determine the Source Shapefile Last Update Date.
            string fullShapefilePath = shapefilePathGenerator.BuildShapefilePath(currentFeatureClassSourceInfo.SourceDataShare, currentFeatureClassSourceInfo.SourceDataset, currentFeatureClassSourceInfo.SourceFeatureClassName);
            currentFeatureClassSourceInfo.SourceLastUpdateDate = featureClassUtilities.GetShapefileLastUpdateDate(fullShapefilePath);
            //  Exit this case.
            break;
          default:
            //  Exit this case.
            break;
        }

        //  Set the Source PropertySet Property of the Current Source Feature Class Info Object.
        currentFeatureClassSourceInfo.SourceDatasetPropertySet = sourcePropertySet;


        //  Return the populated Current Feature Class Source Info Object to the calling method.
        return currentFeatureClassSourceInfo;

      }
      catch (Exception caught)
      {
        //  Send a Message to the calling application to let the user know why this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("    The LoaderUtilities.DatabaseTools.GetLayerSourceInfo() Method Failed with error message:  " + caught.Message);
        }

        //  Return a NULL String to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the Get Source Info SQL Data Reader Object was instantiated, close it.
        if (getSourceInfoSQLDataReader != null)
        {
          if (!getSourceInfoSQLDataReader.IsClosed)
          {
            getSourceInfoSQLDataReader.Close();
          }
          getSourceInfoSQLDataReader.Dispose();
          getSourceInfoSQLDataReader = null;
        }
        //  If the Get Source Info SQL Command Object was instantiated, close it.
        if (getSourceInfoSQLCommand != null)
        {
          getSourceInfoSQLCommand.Dispose();
          getSourceInfoSQLCommand = null;
        }

      }

    }   //  GetLayerSourceInfo()


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

    //  The connection parameters for the Import Monitor Database.
    static string                                      _monitorDatabaseServerName             = null;
    static string                                      _monitorDatabaseName                   = null;
    static string                                      _monitorTableName                      = null;
    private static System.Data.SqlClient.SqlConnection _importMonitorDatabaseConnection       = null;
    static string                                      _importMonitorDatabaseConnectionString = null;
    static string                                      _parameterTableName                    = null;
    static string                                      _parameterDatabaseConnectionString     = null;

  }   //  Class:  LoadInformation

  public class FeatureClassSourceInfo
  {
    public string SourceType { get; set; }
    public string SourceDataShare { get; set; }
    public string SourceDataset { get; set; }
    public string SourceGeoDBFile { get; set; }
    public string SourceServerName { get; set; }
    public string SourceInstance { get; set; }
    public string SourceDatabaseName { get; set; }
    public string SourceDatabaseUser { get; set; }
    public string SourceFeatureClassName { get; set; }
    public System.DateTime SourceLastUpdateDate { get; set; }
    public string OutputGeodatabaseName { get; set; }
    public ESRI.ArcGIS.esriSystem.IPropertySet SourceDatasetPropertySet { get; set; }

  }   //  CLASS:  FeatureClassLoadInfo

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools
