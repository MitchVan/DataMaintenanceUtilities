using System;
using System.Collections.Generic;

namespace PDX.BTS.DataMaintenance.MaintTools
{
  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("0382ba15-d663-4efa-978b-d093ef684006")]


  public class FeatureClassUtilities:System.IDisposable
  {
    /// <summary>
    /// A public event for sending process messages back to the calling application.
    /// </summary>
    public event SendProcessMessage ProcessMessage;


    /// <summary>
    /// A public event for sending error messages back to the calling application.
    /// </summary>
    public event SendErrorMessage ErrorMessage;


    /// <summary>
    /// Class Constructor.
    /// </summary>
    public FeatureClassUtilities()
    {
      PDX.BTS.DataMaintenance.MaintTools.XMLSetttingsFileManager xmlFileManager    = null;
      PDX.BTS.DataMaintenance.MaintTools.EncryptionManager       encryptionManager = null;

      try
      {
        //  Set the Name of the XML Settings File for this Application.
        string applicationDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
        while (applicationDirectory.EndsWith(@"\") == false)
        {
          //  Drop the right-most character from the string.
          applicationDirectory = applicationDirectory.Substring(0, (applicationDirectory.Length - 1));
        }
        string xmlSettingsFile = applicationDirectory + @"DataMaintenanceUtilitiesSettings.xml";


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
        _parameterTableName = xmlFileManager.ReadUserSetting("DatabaseDefinitions", "LoadParemetersTableName");


        //  Set the Connection String that will be used to Connect to the Parameter Database.
        _parameterDatabaseConnectionString = "Data Source=" + _monitorDatabaseServerName +";"
                                           + "Initial Catalog=" + _monitorDatabaseName + ";"
                                           + "Integrated Security=SSPI";


        //  Instantiate the Component that will hold the Managed Resources used by this method.
        _component = new System.ComponentModel.Component();


        // Populate the Shapefile Extension Array.
        _shapefileExtensions = new System.Collections.ArrayList();
        _shapefileExtensions.Add("aih");
        _shapefileExtensions.Add("ain");
        _shapefileExtensions.Add("cpg");
        _shapefileExtensions.Add("dbf");
        _shapefileExtensions.Add("gcd");
        _shapefileExtensions.Add("ixs");
        _shapefileExtensions.Add("mxs");
        _shapefileExtensions.Add("prj");
        _shapefileExtensions.Add("sbn");
        _shapefileExtensions.Add("sbx");
        _shapefileExtensions.Add("shp");
        _shapefileExtensions.Add("shp.xml");
        _shapefileExtensions.Add("shx");
        _shapefileExtensions.Add("xml");


        //  Populate the Array of Shapefile Extensions that are necessary to have a valid Shapefile.
        _shapefileNecessaryExtensions = new System.Collections.ArrayList();
        _shapefileNecessaryExtensions.Add("shp");
        _shapefileNecessaryExtensions.Add("shx");
        _shapefileNecessaryExtensions.Add("dbf");

      }
      catch
      {
        //  Exit this method.
        return;

      }
      finally
      {
        //  If the CGIS Data Maintenance Encryption Manager Object was instantiated, close it.
        if (encryptionManager != null)
        {
          encryptionManager = null;
        }
        //  If the CGIS Data Maintenance XML File Manager Object was instantiated, close it.
        if (xmlFileManager != null)
        {
          xmlFileManager = null;
        }

      }

    }   //  FeatureClassUtilities()


    #region Shapefile Spatial Index Management
    public System.Boolean BuildShapefileSpatialIndex(System.String ShapefileName, System.String ShapefileDirectory)
    {
      System.Type                               indexShapefileFactoryType      = null;
      System.Object                             indexShapefileFactoryObject    = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceFactory indexShapefileWorkspaceFactory = null;
      ESRI.ArcGIS.Geodatabase.IWorkspace        indexShapefileWorkspace        = null;
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace indexShapefileFeatureWorkspace = null;
      ESRI.ArcGIS.Geodatabase.IFeatureClass     indexShapefileFeatureClass     = null;

      try
      {
        //  Make sure the Shapefile Name was passed correctly before moving on.
        if (ShapefileName == null)
        {
          //  Let the User know that a valid Shapefile Name MUST be passed before the index can
          //  be created.
          if (ErrorMessage != null)
          {
            ErrorMessage("A NULL Value cannot be passed for the Shapefile Name! The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;
        }
        else
        {
          if (ShapefileName.Length == 0)
          {
            //  Let the User know that a valid Shapefile Name MUST be passed before the index can
            //  be created.
            if (ErrorMessage != null)
            {
              ErrorMessage("An Empty String cannot be passed for the Shapefile Name! The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed!");
            }
            //  Return FALSE to the calling method to indicate that this method failed.
            return false;
          }

        }


        //  Make sure the Shapefile Directory was passed correctly before moving on.
        if (ShapefileDirectory == null)
        {
          //  Let the User know that a valid Shapefile Directory MUST be passed before the index
          //  can be created.
          if (ErrorMessage != null)
          {
            ErrorMessage("A NULL Value cannot be passed for the Shapefile Directory! The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;
        }
        else
        {
          if (ShapefileDirectory.Length == 0)
          {
            //  Let the User know that a valid Shapefile Directory MUST be passed before the index
            //  can be created.
            if (ErrorMessage != null)
            {
              ErrorMessage("An Empty String cannot be passed for the Shapefile Directory! The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed!");
            }
            //  Return FALSE to the calling method to indicate that this method failed.
            return false;
          }

        }


        //  If the Process Message Event has been instantiated, let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("      -  Opening the Workspace (directory) in which the Shapefile resides...");
        }

        //  Open the Workspace that houses the Feature Class in a try block so that any errors
        //  that are encountered can be handled.
        try
        {
          //  Attempt to open the Shapefile Directory.
          indexShapefileFactoryType = System.Type.GetTypeFromProgID("esriDataSourcesFile.ShapefileWorkspaceFactory");
          indexShapefileFactoryObject = System.Activator.CreateInstance(indexShapefileFactoryType);
          indexShapefileWorkspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory)indexShapefileFactoryObject;
          indexShapefileWorkspace = indexShapefileWorkspaceFactory.OpenFromFile(ShapefileDirectory, 0);
          indexShapefileFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)indexShapefileWorkspace;

        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed while opening the Shapefile Workspace with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
          }

          //  Return FALSE to the calling method to indicate that this methdo failed.
          return false;

        }


        //  If the Process Message Event has been instantiated, let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("      -  Opening the Shapefile Feature Class on which the Index will be created...");
        }

        //  Open the Feature Class in a try block so that any errors that are encountered can
        //  be handled.
        try
        {
          //  Attempt to open the Shapefile Feature Class.
          indexShapefileFeatureClass = indexShapefileFeatureWorkspace.OpenFeatureClass(ShapefileName);

        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed while opening the Shapefile Feature Class with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
          }

          //  Return FALSE to the calling method to indicate that this methdo failed.
          return false;

        }


        //  Call the overload that requires that a Feature Class Pointer be passed to create the
        //  Spatial Index on the Feature Class that was just opened.
        System.Boolean createdIndex = BuildShapefileSpatialIndex(indexShapefileFeatureClass);


        //  Return the result of the attempt to create the index to the calling method.
        return createdIndex;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling method to indicate that this method Failed.
        return false;

      }
      finally
      {
        //  Close any Object Pointers that were instantiated by this method.
        if (indexShapefileFactoryType != null)
        {
          //  Set the Object Pointer to NULL.
          indexShapefileFactoryType = null;
        }

        if (indexShapefileFactoryObject != null)
        {
          //  Set the Object Pointer to NULL.
          indexShapefileFactoryObject = null;
        }

        if (indexShapefileWorkspaceFactory != null)
        {
          //  Set the Object Pointer to NULL.
          indexShapefileWorkspaceFactory = null;
        }

        if (indexShapefileWorkspace != null)
        {
          //  Set the Object Pointer to NULL.
          indexShapefileWorkspace = null;
        }

        if (indexShapefileFeatureWorkspace != null)
        {
          //  Set the Object Pointer to NULL.
          indexShapefileFeatureWorkspace = null;
        }

        if (indexShapefileFeatureClass != null)
        {
          //  Set the Object Pointer to NULL.
          indexShapefileFeatureClass = null;
        }

      }

    }   //  BuildShapefileSpatialIndex()


    public System.Boolean BuildShapefileSpatialIndex(ESRI.ArcGIS.Geodatabase.IFeatureClass ShapefileFeatureClass)
    {
      ESRI.ArcGIS.Geodatabase.IFields     indexShapefileFields      = null;
      ESRI.ArcGIS.Geodatabase.IFieldsEdit indexShapefileFieldsEdit  = null;
      ESRI.ArcGIS.Geodatabase.IField      shapefileGeometryField    = null;
      ESRI.ArcGIS.Geodatabase.IIndex      shapefileSpatialIndex     = null;
      ESRI.ArcGIS.Geodatabase.IIndexEdit  shapefileSpatialIndexEdit = null;
      ESRI.ArcGIS.Geodatabase.IEnumIndex  shapefileExistingIndices  = null;
      ESRI.ArcGIS.Geodatabase.IIndex      shapefileDeleteIndex      = null;

      try
      {
        //  Make sure the Feature Class Object is not a NULL Pointer before moving on.
        if (ShapefileFeatureClass == null)
        {
          //  Let the User know that a valid Shapefile Name MUST be passed before the index can
          //  be created.
          if (ErrorMessage != null)
          {
            ErrorMessage("A Valid Shapefile Object must be passed to this method! The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  If the Process Message Event has been instantiated, let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("      -  Opening the Shapefile Geometry Field and preparing it to be indexed...");
        }

        //  Create a Fields Object and open an Editing Object on it so that new fields can be added.
        indexShapefileFields = new ESRI.ArcGIS.Geodatabase.FieldsClass();
        indexShapefileFieldsEdit = (ESRI.ArcGIS.Geodatabase.IFieldsEdit)indexShapefileFields;


        //  Find the Geometry Field in the Field in the Shapefile Fields Object and add it to the
        //  Fields Collection.
        indexShapefileFieldsEdit.FieldCount_2 = 1;
        int l = ShapefileFeatureClass.FindField(ShapefileFeatureClass.ShapeFieldName);
        if (l < 0)
        {
          //  Let the user know that the Geometry Field of the Shapefile Feature Class could not
          //  be found.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Shapefile Geometry Field could not be found! The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this process failed.
          return false;

        }
        shapefileGeometryField = ShapefileFeatureClass.Fields.get_Field(l);
        indexShapefileFieldsEdit.set_Field(0, shapefileGeometryField);


        //  If the Process Message Event has been instantiated, let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("      -  Creating the Index and applying it to the Shapefile Feature Class Geometry Field...");
        }

        //  Create a new index and prepare it to be added to the table.
        shapefileSpatialIndex = new ESRI.ArcGIS.Geodatabase.IndexClass();
        shapefileSpatialIndexEdit = (ESRI.ArcGIS.Geodatabase.IIndexEdit)shapefileSpatialIndex;
        shapefileSpatialIndexEdit.Fields_2 = indexShapefileFields;
        shapefileSpatialIndexEdit.Name_2 = "Idx_1";


        //  Remove all Indices from the table.
        shapefileExistingIndices = ShapefileFeatureClass.Indexes.FindIndexesByFieldName(ShapefileFeatureClass.ShapeFieldName);

        shapefileDeleteIndex = shapefileExistingIndices.Next();
        while (shapefileDeleteIndex != null)
        {
          ShapefileFeatureClass.DeleteIndex(shapefileDeleteIndex);
          shapefileDeleteIndex = shapefileExistingIndices.Next();
        }


        //  Add the index to the Shapefile.
        ShapefileFeatureClass.AddIndex(shapefileSpatialIndex);


        //  If the process made it to here it was successful so return TRUE to the calling method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.BuildShapefileSpatialIndex() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");

        }

        //  Return FALSE to the calling method to indicate that this method Failed.
        return false;

      }
      finally
      {
        //  Close any Object Pointers that were created by this method.
        if (indexShapefileFields != null)
        {
          //  Close the Object Pointer.
          indexShapefileFields = null;
        }

        if (indexShapefileFieldsEdit != null)
        {
          //  Close the Object Pointer.
          indexShapefileFieldsEdit = null;
        }

        if (shapefileGeometryField != null)
        {
          //  Close the Object Pointer.
          shapefileGeometryField = null;
        }

        if (shapefileSpatialIndex != null)
        {
          //  Close the Object Pointer.
          shapefileSpatialIndex = null;
        }

        if (shapefileSpatialIndexEdit != null)
        {
          //  Close the Object Pointer.
          shapefileSpatialIndexEdit = null;
        }

        if (shapefileExistingIndices != null)
        {
          //  Close the Object Pointer.
          shapefileExistingIndices = null;
        }

        if (shapefileDeleteIndex != null)
        {
          //  Close the Object Pointer.
          shapefileDeleteIndex = null;
        }

      }

    }   //  BuildShapefileSpatialIndex()
    #endregion Shapefile Spatial Index Management


    #region Delete Shapefile
    public System.Boolean DeleteExistingShapefile(System.String ShapefileDirectory, System.String ShapefileName)
    {
      try
      {
        //  If the Shapefile Name that was passed to this Method includes a Shapefile Part
        //  Extension, strip the extension before attempting to delete the Shapefile Parts.
        System.String deleteShapefileName = ShapefileName;
        //  If the Extension is included in the Delete Shapefile Name, strip it off.
        if (deleteShapefileName.IndexOf(".") != -1)
        {
          //  Strip the Extension from the Delete Shapefile Name.
          deleteShapefileName = deleteShapefileName.Substring(0, deleteShapefileName.IndexOf(".") - 1);
        }



        //  Build the Full Path to the Shapefile that is to be deleted.
        System.String deleteFullShapefilePath = System.IO.Path.Combine(ShapefileDirectory, deleteShapefileName);


        //  Call the Overload of this Method that expects a full Shapefile Path to delete the
        //  existing Shapefile.
        System.Boolean deletedShapefile = DeleteExistingShapefile(deleteFullShapefilePath);


        //  Return the result of the delete attempt to the calling method.
        return deletedShapefile;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          //  Let the User know that the Method faild.
          ErrorMessage("The Maintools.FeatureClassUtilities.DeleteExistingShapefile(Directory, Name) Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }

    }   //  DeleteExistingShapefile()


    public System.Boolean DeleteExistingShapefile(System.String DeleteShapefileName)
    {
      try
      {
        //  If the Shapefile Path that was passed to this Method includes a Shapefile Part
        //  Extension, strip the extension before attempting to delete the Shapefile Parts.
        System.String deleteShapefilePath = DeleteShapefileName;
        foreach (System.String currentSearchShapefileExtension in _shapefileExtensions)
        {
          //  If the Extension is included in the Delete Shapefile Path, strip it off.
          if (deleteShapefilePath.IndexOf("." + currentSearchShapefileExtension) != -1)
          {
            //  Strip the Extension from the Delete Shapefile Path.
            deleteShapefilePath = deleteShapefilePath.Substring(0, (deleteShapefilePath.Length - (currentSearchShapefileExtension.Length + 1)));
          }

        }


        //  If any pieces of the Shapefile exist, delete them.
        foreach (string currentShapefileExtension in _shapefileExtensions)
        {
          //  If the Current Shapefile part exists, delete it.
          if (System.IO.File.Exists(deleteShapefilePath + "." + currentShapefileExtension))
          {
            //  Attempt to delete the Shapefile Part.
            System.IO.File.Delete(deleteShapefilePath + "." + currentShapefileExtension);
          }

        }


        //  If any parts of the Shapefile exist in the Output directory, the delete failed so
        //  return FALSE to the calling method.
        foreach (string currentTestShapefileExtension in _shapefileExtensions)
        {
          //  if the Current Shapefile part exists, the delete failed so return FALSE to the
          //  calling method.
          if (System.IO.File.Exists(deleteShapefilePath + "." + currentTestShapefileExtension))
          {
            //  Return FALSE to the calling method.
            return false;
          }

        }


        //  If the process made it to here, it was successful so return TRUE to the calling method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          //  Let the User know that the Method faild.
          ErrorMessage("The Maintools.FeatureClassUtilities.DeleteExistingShapefile(FullName) Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }

    }   //  DeleteExistingShapefile()
    #endregion Delete Shapefile


    #region Copy Shapefile
    public System.Boolean CopyShapefile(System.String InputShapefileDirectory, System.String InputShapefileName, System.String OutputDirectory, System.String OutputName)
    {
      try
      {
        //  Call the Override of this Method that requires an Override value defaulting the value
        //  to false.
        System.Boolean copiedShapefile = CopyShapefile(InputShapefileDirectory, InputShapefileName, OutputDirectory, OutputName, false);


        //  Return the results of the Copy Attempt to the calling method.
        return copiedShapefile;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          //  Let the User know that the Method faild.
          ErrorMessage("The  Maintools.FeatureClassUtilities.CopyShapefile(InputDirectory, InputName, OutputDirectory, OutputName) Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }

    }   //  CopyShapefile()


    public System.Boolean CopyShapefile(System.String InputShapefileDirectory, System.String InputShapefileName, System.String OutputDirectory, System.String OutputName, System.Boolean OverwriteExixting)
    {
      try
      {
        //  Make sure the Input Shapefile Directory Exists.
        if (!System.IO.Directory.Exists(InputShapefileDirectory))
        {
          //  Let the User know that the Specified Input Directory does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified Input Shapefile Directory - " + InputShapefileDirectory + " - does not Exist.  Cannot Copy the Shapefile from that location!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Make sure that the Specified Input Shapefile Name is valid.
        if (InputShapefileName.Length == 0)
        {
          //  Let the User know that the Specified Input Shapefile Name is not valid.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified Input Shapefile Name - " + InputShapefileName + " - is not valid.");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  If the Input Shapefile Name Includes an Extension, remove the extension.
        System.String currentShapefileName = InputShapefileName;
        if (currentShapefileName.IndexOf(".") != -1)
        {
          foreach (System.String currentExtension in _shapefileExtensions)
          {
            //  Check to see if the Extension is part of the Shapefile Name.
            if (currentShapefileName.IndexOf(currentExtension, System.StringComparison.CurrentCultureIgnoreCase) != -1)
            {
              //  Remove the Extension from the Shapefile name.
              currentShapefileName = currentShapefileName.Substring(0, (currentShapefileName.Length - (currentExtension.Length + 1)));
            }
          }

        }


        //  Make sure the Specified Input Shapefile Exists.
        System.String inputShapefilePath = System.IO.Path.Combine(InputShapefileDirectory, currentShapefileName);
        if (!System.IO.File.Exists(inputShapefilePath + @".shp"))
        {
          //  Let the User know that the Specified Input Shapefile Name does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified Input Shapefile - " + inputShapefilePath + " - does not exist and cannot be copied.");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Make sure the Output Shapefile Directory Exists.
        if (!System.IO.Directory.Exists(OutputDirectory))
        {
          //  Attempt to create the Output Directory.
          System.IO.Directory.CreateDirectory(OutputDirectory);

          //  Make sure that the directory exists now.
          if (!System.IO.Directory.Exists(OutputDirectory))
          {
            //  Let the User know that the Specified Input Shapefile Name does not exist.
            if (ErrorMessage != null)
            {
              ErrorMessage("The specified Output Directory - " + OutputDirectory + " - does not exist the Shapefile cannot be copied to the directory.");
            }
            //  Return FALSE to the calling method to indicate that this method failed.
            return false;
          }

        }


        //  Call the override of this Method to requires a Full Input Shapefile Directory and a
        //  Full Output Shapefile Directory to actually perform the copy.
        System.String outputShapefilePath = System.IO.Path.Combine(OutputDirectory, OutputName);
        System.Boolean copiedShapefile = CopyShapefile(inputShapefilePath, outputShapefilePath, OverwriteExixting);


        //  Return the result of the Copy attempt to the calling method.
        return copiedShapefile;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          //  Let the User know that the Method faild.
          ErrorMessage("The  Maintools.FeatureClassUtilities.CopyShapefile(InputDirectory, InputName, OutputDirectory, OutputName, Overwrite) Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }

    }   //  CopyShapefile()


    public System.Boolean CopyShapefile(System.String InputShapefilePath, System.String OutputShapefilePath)
    {
      try
      {
        //  Call the Override of this Method that requires and Overwrite Existing value, defaulting
        //  the value to false.
        System.Boolean copiedShapefile = CopyShapefile(InputShapefilePath, OutputShapefilePath, false);


        //  Return the Results of the Copy Attempt to the calling method.
        return copiedShapefile;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          //  Let the User know that the Method faild.
          ErrorMessage("The  Maintools.FeatureClassUtilities.CopyShapefile(FullInputName, FullOutputName) Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }

    }   //  CopyShapefile()


    public System.Boolean CopyShapefile(System.String InputShapefilePath, System.String OutputShapefilePath, System.Boolean OverwriteExisting)
    {
      try
      {
        //  Make sure the Input Shapefile Path value is not zero length.
        if (InputShapefilePath.Length == 0)
        {
          //  Let the user know that the Input Shapefile Path is not valid.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Specified Input Shapefile Path cannot be zero length.  The Copy will be aborted!");
          }

          //  Return FALSE to the calling method to indicate that the Copy failed.
          return false;

        }


        //  Retrieve the Input Shapefile Name from the Input Shapefile Path.
        System.String inputShapefileName = InputShapefilePath;
        while (inputShapefileName.IndexOf(@"\") != -1)
        {
          inputShapefileName = inputShapefileName.Substring(1);
        }


        //  Retrieve the Output Shapefile Name from the Output Shapefile Path.
        System.String outputShapefileName = OutputShapefilePath;
        while (outputShapefileName.IndexOf(@"\") != -1)
        {
          outputShapefileName = outputShapefileName.Substring(1);
        }


        //  If the Input Shapefile Name Includes an Extension, remove the extension.
        System.String currentInputShapefilePath = InputShapefilePath;
        if (currentInputShapefilePath.IndexOf(".") != -1)
        {
          foreach (System.String currentExtension in _shapefileExtensions)
          {
            //  Check to see if the Extension is part of the Input Shapefile Path.
            if (currentInputShapefilePath.IndexOf(currentExtension, System.StringComparison.CurrentCultureIgnoreCase) != -1)
            {
              //  Remove the Extension from the Input Shapefile Path.
              currentInputShapefilePath = currentInputShapefilePath.Substring(0, (currentInputShapefilePath.Length - (currentExtension.Length + 1)));
            }
          }

        }


        //  Make sure the Input Shapefile Path Exists before Attempting to copy it to the Output
        //  Directory.
        if (!System.IO.File.Exists(currentInputShapefilePath + @".shp"))
        {
          //  Let the user know that the Input Shapefile does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Specified Input Shapefile Path - " + currentInputShapefilePath  + ".shp - does not exist.  The Copy will be aborted!");
          }

          //  Return FALSE to the calling method to indicate that the Copy failed.
          return false;

        }


        //  Strip the Output Shapefile Name from the Output Shapefile Directory and Make sure the
        //  Output Shapefile Directory exists before attempting to copy the Input Shapefile to it.
        System.String outputShapefileDirectory = OutputShapefilePath;
        while (!outputShapefileDirectory.EndsWith(@"\"))
        {
          //  Strip the last character from the Shapefile Path until the Output Shapefile Name has
          //  been removed from the path.
          outputShapefileDirectory = outputShapefileDirectory.Substring(0, outputShapefileDirectory.Length - 1);
        }
        //  Strip the final Backslash from the Output Directory Path.
        outputShapefileDirectory = outputShapefileDirectory.Substring(0, outputShapefileDirectory.Length - 1);
        //  Now make sure the output Directory exists.
        if (!System.IO.Directory.Exists(outputShapefileDirectory))
        {
          //  Let the user know that the directory is being created.
          if (ProcessMessage != null)
          {
            ProcessMessage("      -  Creating the Output Directory - " + outputShapefileDirectory + "...");
          }
          //  Attempt to create the Output Directory.
          System.IO.Directory.CreateDirectory(outputShapefileDirectory);
          //  Now if the Output Directory does not exist, abort the copy.
          if (!System.IO.Directory.Exists(outputShapefileDirectory))
          {
            //  Let the user know that the Output Directory does not exist.
            if (ErrorMessage != null)
            {
              ErrorMessage("The Specified Output Directory - " + outputShapefileDirectory + " - does not exist.  The Copy will be aborted!");
            }
            //  Return FALSE to the calling method to indicate that the Copy failed.
            return false;
          }
        }


        //  If the Output Shapefile Path includes an extension, remove it.
        string outputShapefileCopyPath = OutputShapefilePath;
        if (outputShapefileCopyPath.IndexOf(".") != -1)
        {
          foreach (System.String currentExtension in _shapefileExtensions)
          {
            //  Check to see if the Extension is part of the Shapefile Name.
            if (outputShapefileCopyPath.IndexOf(currentExtension, System.StringComparison.CurrentCultureIgnoreCase) != -1)
            {
              //  Remove the Extension from the Shapefile name.
              outputShapefileCopyPath = outputShapefileCopyPath.Substring(0, (outputShapefileCopyPath.Length - (currentExtension.Length + 1)));
            }
          }

        }
        

        //  If the Output Shapefile already exists and overwrite is set to false, this process
        //  cannot continue.  If Overwrite is set to TRUE, delete the existing output Files.
        foreach (System.String currentextension in _shapefileExtensions)
        {
          //  Determine if the output file already exists.  If it does and Overwrite Existing
          //  is set to TRUE, delete teh existing file.  Otherwise, abort the copy.
          if (System.IO.File.Exists(outputShapefileCopyPath + @"." + currentextension))
          {
            //  If Overwrite Existing is set to TRUE, attempt to delete the file.
            if (OverwriteExisting)
            {
              //  Make sure the File is not set to Read-Only.
              System.IO.FileAttributes currentFileAttributes = System.IO.File.GetAttributes(outputShapefileCopyPath + @"." + currentextension);
              if ((currentFileAttributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly)
              {
                //  Set the file to Read-Write.
                System.IO.File.SetAttributes(outputShapefileCopyPath + @"." + currentextension, System.IO.FileAttributes.Normal);
              }
              //  Attempt to delete the file.
              System.IO.File.Delete(outputShapefileCopyPath + @"." + currentextension);
              //  If the Output File Still Exists, abort the copy.
              if (System.IO.File.Exists(outputShapefileCopyPath + @"." + currentextension))
              {
                //  Let the user know that the Output file could not be over-written.
                if (ErrorMessage != null)
                {
                  ErrorMessage("Could not Over-Write the - " + outputShapefileCopyPath + @"." + currentextension + " - existing file.  Aborting the Copy!");
                }
                //  Return FALSE to the calling method to indicate that the copy failed.
                return false;
              }
            }
            else
            {
              //  Let the User know that the Output File already exists so the copy cannot continue.
              if (ErrorMessage != null)
              {
                ErrorMessage("The Output Shapefile - " + outputShapefileCopyPath + ".shp - Already Exists.  The copy cannot continue.");
              }
              //  Return FALSE to the calling method to indicate that this copy failed.
              return false;
            }
          }
        }


        //  Attempt to copy the Input Shapefile to the Output Directory.
        foreach (System.String currentExtension in _shapefileExtensions)
        {
          //  If the Input File Exists, attempt to copy it to the destination Directory.
          if (System.IO.File.Exists(InputShapefilePath + @"." + currentExtension))
          {
            //  Attempt to Copy the File to the Destination Directory.
            System.IO.File.Copy(InputShapefilePath + @"." + currentExtension, outputShapefileCopyPath + @"." + currentExtension);
            //  Make sure the file was copied successfully.
            if (!System.IO.File.Exists(outputShapefileCopyPath + @"." + currentExtension))
            {
              //  Let the User know that the Current Shapefile Part could not be copied.
              if (ErrorMessage != null)
              {
                ErrorMessage("Could not copy the - " + InputShapefilePath + @"." + currentExtension + " - file to the Ouput Directory.  Aborting the copy!");
              }
              //  Return FALSE to the calling method to indicate that this method failed.
              return false;
            }
            else
            {
              //  Let the user know that the Shapefile was copied successfully.
              if (ProcessMessage != null)
              {
                ProcessMessage("      -  Successfully Copied the " + inputShapefileName + @"." + currentExtension + " file to the output directory...");
              }
            }
          }
        }


        //  Make sure there is a Valid Shapefile in the Ouput Directory.
        foreach (System.String currentNecExtension in _shapefileNecessaryExtensions)
        {
          //  Make sure an output file with the current extension exists.
          if (!System.IO.File.Exists(outputShapefileCopyPath + @"." + currentNecExtension))
          {
            //  Let the User know that there is not a valid extension in the output directory.
            if (ErrorMessage != null)
            {
              ErrorMessage("There is not a Valid Shapefile in the Ouput Directory because the - " + outputShapefileCopyPath + @"." + currentNecExtension + " - file does not exist in the Output Directory.  The Copy failed!");
            }
            //  Return FALSE to the calling method to indicate that the copy failed.
            return false;
          }

        }


        //  Let the User know that the Shapefile was successfully copied to the destination
        //  directory.
        if (ProcessMessage != null)
        {
          ProcessMessage("      -  Successfully copied the Input Shapefile - " + inputShapefileName.ToUpper() + " - to destination directory - " + outputShapefileDirectory + " - as:  " + outputShapefileName.ToUpper() + ".");
        }


        //  If the process made it to here it was successful so return TRUE to the calling method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          //  Let the User know that the Method faild.
          ErrorMessage("The Maintools.FeatureClassUtilities.CopyShapefile(FullInputName, FullOutputName) Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }

    }   //  CopyShapefile()
    #endregion Copy Shapefile


    #region File Geodatabase Utilities
    public ESRI.ArcGIS.Geodatabase.IWorkspace EstablishFileGeodatabaseConnection(string FileGeodatabase)
    {
      ESRI.ArcGIS.esriSystem.IPropertySet fileGeodatabasePropertySet = null;

      try
      {
        //  Make sure the specified File Geodatabase Directory Exists.
        if (!System.IO.Directory.Exists(FileGeodatabase))
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified File Geodatabase - " + FileGeodatabase + " - does not extist!  Aborting the MaintTools.FeatureClassUtilities.EstablishFileGeodatabaseConnection() Method!");
          }

          //  Return a NULL Pointer to the calling method to indicate that this methdo failed.
          return null;

        }


        //  Build the File Geodatabase PropertySet based on the File Geodatabase Path.
        fileGeodatabasePropertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
        fileGeodatabasePropertySet.SetProperty("DATABASE", FileGeodatabase);


        //  Return the File Geodatabase Workspace pointer to the calling method.
        return EstablishFileGeodatabaseConnection(fileGeodatabasePropertySet);

      }
      catch (System.Runtime.InteropServices.COMException comException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EstablishFileGeodatabaseConnection() Method failed while opening the File Geodatabase Workspace with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Pointer to the calling method to indicate that this methdo failed.
        return null;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ProcessMessage("The MaintTools.FeatureClassUtilities.EstablishFileGeodatabaseConnection() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the ESRI System File Geodatabase Property Set Object was instantiated, close it.
        if (fileGeodatabasePropertySet != null)
        {
          fileGeodatabasePropertySet = null;
        }

      }

    }   //  EstablishFileGeodatabaseConnection()


    public ESRI.ArcGIS.Geodatabase.IWorkspace EstablishFileGeodatabaseConnection(ESRI.ArcGIS.esriSystem.IPropertySet FileGeodatabasePropertySet)
    {
      System.Type                               fileGeodatabaseFactoryType      = null;
      System.Object                             fileGeodatabaseFactoryObject    = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceFactory fileGeodatabaseWorkspaceFactory = null;
      ESRI.ArcGIS.Geodatabase.IWorkspace        fileGeodatabaseWorkspace        = null;

      try
      {
        //  Attempt to open the File Geodatabase Workspace.
        fileGeodatabaseFactoryType = System.Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
        fileGeodatabaseFactoryObject = System.Activator.CreateInstance(fileGeodatabaseFactoryType);
        fileGeodatabaseWorkspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory)fileGeodatabaseFactoryObject;
        fileGeodatabaseWorkspace = fileGeodatabaseWorkspaceFactory.Open(FileGeodatabasePropertySet, 0);

        //  Return the File Geodatabase Workspace pointer to the calling method.
        return fileGeodatabaseWorkspace;

      }
      catch (System.Runtime.InteropServices.COMException comException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EstablishFileGeodatabaseConnection() Method failed while opening the File Geodatabase Workspace with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Pointer to the calling method to indicate that this methdo failed.
        return null;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ProcessMessage("The MaintTools.FeatureClassUtilities.EstablishFileGeodatabaseConnection() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the File Geodatabase Workspace has been Instantiated, close it.
        if (fileGeodatabaseWorkspace != null)
        {
          //  Close the File Geodatabase Workspace.
          fileGeodatabaseWorkspace = null;
        }
        //  If the File Geodatabase Workspace Factory has been Instantiated, close it.
        if (fileGeodatabaseWorkspaceFactory != null)
        {
          //  Close the File Geodatabase Workspace Factory.
          fileGeodatabaseWorkspaceFactory = null;
        }
        //  If the File Geodatabase Factory Object has been Instantiated, close it.
        if (fileGeodatabaseFactoryObject != null)
        {
          //  Close the File Geodatabase Factory Object.
          fileGeodatabaseFactoryObject = null;
        }
        //  If the File Geodatabase Factory Type has been Instantiated, close it.
        if (fileGeodatabaseFactoryType != null)
        {
          //  Close the File Geodatabase Factory Type.
          fileGeodatabaseFactoryType = null;
        }

      }

    }   //  EstablishFileGeodatabaseConnection()


    public bool? FileGeoDBIsCompressed(string FileGeodatabase)
    {
      ESRI.ArcGIS.Geodatabase.IWorkspace              fileGeodatabaseWorkspace             = null;
      ESRI.ArcGIS.Geodatabase.IEnumDataset            fileGeodatabaseDatasets              = null;
      ESRI.ArcGIS.Geodatabase.IDataset                currentFileGeoDBDataset              = null;
      ESRI.ArcGIS.DataSourcesGDB.FgdbFeatureClassName currentFileGeoDBFeatureClassName     = null;
      ESRI.ArcGIS.Geodatabase.ICompressionInfo        fileGeoDBFeatureClassCompressionInfo = null;

      try
      {
        //  Attempt to open the File Geodatabase Feature Workspace.
        fileGeodatabaseWorkspace = EstablishFileGeodatabaseConnection(FileGeodatabase);

        if (fileGeodatabaseWorkspace == null)
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The MaintTools.FeatureClassUtilities.FileGeoDBIsCompressed() Method failed to establish a connection to the -" + FileGeodatabase + " File Geodatabase.");
          }

          //  Return FALSE to the calling method to indicate that this methdo failed.
          return false;

        }


        //  Now that the File Geodatabase is open, open a Dataset in it.
        fileGeodatabaseDatasets = fileGeodatabaseWorkspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass);
        fileGeodatabaseDatasets.Reset();
        currentFileGeoDBDataset = fileGeodatabaseDatasets.Next();

        //  Retrieve the Compression Info about the Current Feature Class.  If there is no feature Class ("currentFileGeoDBDataset"
        //  is NULL) the File Geodatabase cannot be compressed so return FALSE to the calling method.
        if (currentFileGeoDBDataset == null)
        {
          //  The File Geodatabase cannot be compressed if it does not have any Feature Classes in it so return FALSE to the calling method.
          return false;

        }
        else
        {
          currentFileGeoDBFeatureClassName = (ESRI.ArcGIS.DataSourcesGDB.FgdbFeatureClassName)currentFileGeoDBDataset.FullName;
          fileGeoDBFeatureClassCompressionInfo = (ESRI.ArcGIS.Geodatabase.ICompressionInfo)currentFileGeoDBFeatureClassName;

          //  Return the Compression value for the Current Feature Class to the calling method.
          return fileGeoDBFeatureClassCompressionInfo.IsCompressed;

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ProcessMessage("The MaintTools.FeatureClassUtilities.FileGeoDBIsCompressed() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the File Geodatabase Datasets Enumerator has been Instantiated, close it.
        if (fileGeodatabaseDatasets != null)
        {
          //  Close the File Geodatabase Datasets Enumerator.
          fileGeodatabaseDatasets = null;
        }
        //  If the File Geodatabase Dataset Object has been Instantiated, close it.
        if (currentFileGeoDBDataset != null)
        {
          //  Close the File Geodatabase Dataset Object.
          currentFileGeoDBDataset = null;
        }
        //  If the File Geodatabase Feature Class Name Object has been Instantiated, close it.
        if (currentFileGeoDBFeatureClassName != null)
        {
          //  Close the File Geodatabase Feature Class Name Object.
          currentFileGeoDBFeatureClassName = null;
        }
        //  If the File Geodatabase Feature Class Compression Info Object has been Instantiated, close it.
        if (fileGeoDBFeatureClassCompressionInfo != null)
        {
          //  Close the File Geodatabase Feature Class Compression Info Object.
          fileGeoDBFeatureClassCompressionInfo = null;
        }
        //  If the File Geodatabase Workspace has been Instantiated, close it.
        if (fileGeodatabaseWorkspace != null)
        {
          //  Close the File Geodatabase Workspace.
          fileGeodatabaseWorkspace = null;
        }
      }

    }   //  FileGeoDBIsCompressed()


    public bool? UnCompressFileGeoDB(string FileGeodatabase)
    {
      System.Collections.Generic.IEnumerable<ESRI.ArcGIS.RuntimeInfo> runtimeInfo              = null;
      ESRI.ArcGIS.esriSystem.IVariantArray                            geoprocessorVariantArray = null;
      ESRI.ArcGIS.Geoprocessing.GeoProcessor                          uncompressGeoprocessor   = null;

      try
      {
        //  Make sure the File Geodatabase exists before attempting to uncompress it.
        if (!System.IO.Directory.Exists(FileGeodatabase))
        {
          //  Let the user know that the File Geodatabase does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("      - The File Geodatabase - " + FileGeodatabase.ToUpper() + " does not exist...");
          }

          //  Return FALSE to the calling method to indicate that a File Geodatabase that does
          //  not exist, cannot be uncompressed.
          return false;

        }


        //  Let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - Attempting to uncompress the File Geodatabase - " + FileGeodatabase.ToUpper() + "...");
        }

        //  Determine the Install Path for the ArcGIS Software.
        runtimeInfo = ESRI.ArcGIS.RuntimeManager.InstalledRuntimes;
        string arcToolboxPath = null;
        foreach (ESRI.ArcGIS.RuntimeInfo currentRuntimeItem in runtimeInfo)
        {
          //  Retrieve the Install Path for the ArcGIS Desktop Runtime (ArcGIS Toolbox Items
          //  are found in this path).
          if (currentRuntimeItem.Product == ESRI.ArcGIS.ProductCode.Desktop)
          {
            arcToolboxPath = currentRuntimeItem.Path;
          }

        }

        //  If the ArcToolBox Directory Exists, add it to the Path.
        if (System.IO.Directory.Exists(System.IO.Path.Combine(arcToolboxPath, @"ArcToolbox")))
        {
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, @"ArcToolbox");
        }

        //  If the Toolboxes Directory Exists, add it to the Path.
        if (System.IO.Directory.Exists(System.IO.Path.Combine(arcToolboxPath, @"Toolboxes")))
        {
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, @"Toolboxes");
        }


        //  If the Toolbox File Exists, add it to the Toolbox path.
        if (System.IO.File.Exists(System.IO.Path.Combine(arcToolboxPath, "Data Management Tools.tbx")))
        {
          //  Add the File Name to the path.
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, "Data Management Tools.tbx");
        }


        //  Build the Geoprocessor Variant Array that will pass the arguments to the Geoprocessing Tool.
        geoprocessorVariantArray = new ESRI.ArcGIS.esriSystem.VarArrayClass();
        geoprocessorVariantArray.Add(FileGeodatabase);


        //  Initialize the Geoprocessing Object that will be used to uncompress the Geodatabase.
        uncompressGeoprocessor = new ESRI.ArcGIS.Geoprocessing.GeoProcessor();
        uncompressGeoprocessor.AddToolbox(arcToolboxPath);


        try
        {
          //  Perform the export.
          uncompressGeoprocessor.Execute("UncompressFileGeodatabaseData_management", geoprocessorVariantArray, null);
          //  Write the messages from the Feature Class to Feature Class tool log file.
          if (ProcessMessage != null)
          {
            int toolMessageCount = uncompressGeoprocessor.MessageCount;
            int currentToolMessageIndex = 0;
            ProcessMessage("");
            ProcessMessage("         - Successfully Uncompressed the File Geodatabase...");
            ProcessMessage("            - Uncompress File Geodatabase Operation Messages...");
            while (currentToolMessageIndex < toolMessageCount)
            {
              //  Write the current message to the log file.
             ProcessMessage("              + " + uncompressGeoprocessor.GetMessage(currentToolMessageIndex));
              //  Increment the Tool Message Index Counter.
              currentToolMessageIndex++;
            }
          }

        }
        catch (System.IO.IOException ioException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Dissolve Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Uncompress File Geodatabase Operation in the UnCompressFileGeoDB() Method Failed with error message - " + ioException.Message + " (" + ioException.Source + " Line:  " + lineNumber.ToString() + ")!");
            if (ProcessMessage != null)
            {
              int toolMessageCount = uncompressGeoprocessor.MessageCount;
              int currentToolMessageIndex = 0;
              ProcessMessage("The information from the Geoprocessor is:");
              while (currentToolMessageIndex < toolMessageCount)
              {
                //  Write the current message to the log file.
               ProcessMessage("   + " + uncompressGeoprocessor.GetMessage(currentToolMessageIndex));
                //  Increment to Toold Message Index Counter.
                currentToolMessageIndex++;
              }
            }
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;

        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Uncompress Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Uncompress File Geodatabase Operation in the UnCompressFileGeoDB() Method Failed with error message - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
            if (ProcessMessage != null)
            {
              int toolMessageCount = uncompressGeoprocessor.MessageCount;
              int currentToolMessageIndex = 0;
              ProcessMessage("The information from the Geoprocessor is:");
              while (currentToolMessageIndex < toolMessageCount)
              {
                //  Write the current message to the log file.
                ProcessMessage("   + " + uncompressGeoprocessor.GetMessage(currentToolMessageIndex));
                //  Increment to Toold Message Index Counter.
                currentToolMessageIndex++;
              }
            }
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;

        }


        //  If the Processor made it to here, it was successful so return TRUE to the calling
        //  method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.UnCompressFileGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the Installed Runtimes List has been instantiated, close it.
        if (runtimeInfo != null)
        {
          runtimeInfo = null;
        }

        //  If the Geoprocessor Parameters Array has been instantiated, close it.
        if (geoprocessorVariantArray != null)
        {
          geoprocessorVariantArray = null;
        }

        //  If the Uncompress Geoprocessor Object has been instantiated, close it.
        if (uncompressGeoprocessor != null)
        {
          uncompressGeoprocessor = null;
        }

      }

    }   //  UnCompressFileGeoDB()


    public bool? CompressFileGeoDB(string FileGeodatabase)
    {
      System.Collections.Generic.IEnumerable<ESRI.ArcGIS.RuntimeInfo> runtimeInfo              = null;
      ESRI.ArcGIS.esriSystem.IVariantArray                            geoprocessorVariantArray = null;
      ESRI.ArcGIS.Geoprocessing.GeoProcessor                          compressGeoprocessor     = null;

      try
      {
        //  Make sure the File Geodatabase exists before attempting to uncompress it.
        if (!System.IO.Directory.Exists(FileGeodatabase))
        {
          //  Let the user know that the File Geodatabase does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The File Geodatabase - " + FileGeodatabase.ToUpper() + " does not exist and thus was not compressed...");
          }


          //  Return FALSE to the calling routine to indicate that this process failed.
          return false;

        }


        //  Let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - Attempting to compress the File Geodatabase - " + FileGeodatabase.ToUpper() + "...");
        }

        //  Determine the Install Path for the ArcGIS Software.
        runtimeInfo = ESRI.ArcGIS.RuntimeManager.InstalledRuntimes;
        string arcToolboxPath = null;
        foreach (ESRI.ArcGIS.RuntimeInfo currentRuntimeItem in runtimeInfo)
        {
          //  Retrieve the Install Path for the ArcGIS Desktop Runtime (ArcGIS Toolbox Items
          //  are found in this path).
          if (currentRuntimeItem.Product == ESRI.ArcGIS.ProductCode.Desktop)
          {
            arcToolboxPath = currentRuntimeItem.Path;
          }

        }

        //  If the ArcToolBox Directory Exists, add it to the Path.
        if (System.IO.Directory.Exists(System.IO.Path.Combine(arcToolboxPath, @"ArcToolbox")))
        {
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, @"ArcToolbox");
        }

        //  If the Toolboxes Directory Exists, add it to the Path.
        if (System.IO.Directory.Exists(System.IO.Path.Combine(arcToolboxPath, @"Toolboxes")))
        {
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, @"Toolboxes");
        }


        //  If the Toolbox File Exists, add it to the Toolbox path.
        if (System.IO.File.Exists(System.IO.Path.Combine(arcToolboxPath, "Data Management Tools.tbx")))
        {
          //  Add the File Name to the path.
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, "Data Management Tools.tbx");
        }


        //  Build the Geoprocessor Variant Array that will pass the arguments to the Geoprocessing Tool.
        geoprocessorVariantArray = new ESRI.ArcGIS.esriSystem.VarArrayClass();
        geoprocessorVariantArray.Add(FileGeodatabase);


        //  Initialize the Geoprocessing Object that will be used to compress the Geodatabase.
        compressGeoprocessor = new ESRI.ArcGIS.Geoprocessing.GeoProcessor();
        compressGeoprocessor.AddToolbox(arcToolboxPath);


        try
        {
          //  Perform the export.
          compressGeoprocessor.Execute("CompressFileGeodatabaseData_management", geoprocessorVariantArray, null);
          //  Write the messages from the Compress Tool to the log file.
          if (ProcessMessage != null)
          {
            int toolMessageCount = compressGeoprocessor.MessageCount;
            int currentToolMessageIndex = 0;
            ProcessMessage("");
            ProcessMessage("         - Successfully Compressed the File Geodatabase...");
            ProcessMessage("            - Compress File Geodatabase Operation Messages...");
            while (currentToolMessageIndex < toolMessageCount)
            {
              //  Write the current message to the log file.
              ProcessMessage("              + " + compressGeoprocessor.GetMessage(currentToolMessageIndex));
              //  Increment the Tool Message Index Counter.
              currentToolMessageIndex++;
            }
          }

        }
        catch (System.IO.IOException ioException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Compress Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Compress File Geodatabase Operation in the FeatureClassUtilities.CompressFileGeoDB() Method Failed with error message - " + ioException.Message + " (" + ioException.Source + " Line:  " + lineNumber.ToString() + ")!");
            if (ProcessMessage != null)
            {
              int toolMessageCount = compressGeoprocessor.MessageCount;
              int currentToolMessageIndex = 0;
              ProcessMessage("The information from the Geoprocessor is:");
              while (currentToolMessageIndex < toolMessageCount)
              {
                //  Write the current message to the log file.
                ProcessMessage("   + " + compressGeoprocessor.GetMessage(currentToolMessageIndex));
                //  Increment to Toold Message Index Counter.
                currentToolMessageIndex++;
              }
            }
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;

        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Compress Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Compress File Geodatabase Operation in the CompressFileGeoDB() Method Failed with error message - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
            if (ProcessMessage != null)
            {
              int toolMessageCount = compressGeoprocessor.MessageCount;
              int currentToolMessageIndex = 0;
              ProcessMessage("The information from the Geoprocessor is:");
              while (currentToolMessageIndex < toolMessageCount)
              {
                //  Write the current message to the log file.
                ProcessMessage("   + " + compressGeoprocessor.GetMessage(currentToolMessageIndex));
                //  Increment to Toold Message Index Counter.
                currentToolMessageIndex++;
              }
            }
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;

        }


        //  If the process made it to here, it was successful so return TRUE to the calling
        //  method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.CompressFileGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the Installed Runtimes List has been instantiated, close it.
        if (runtimeInfo != null)
        {
          runtimeInfo = null;
        }

        //  If the Geoprocessor Parameters Array has been instantiated, close it.
        if (geoprocessorVariantArray != null)
        {
          geoprocessorVariantArray = null;
        }

        //  If the Uncompress Geoprocessor Object has been instantiated, close it.
        if (compressGeoprocessor != null)
        {
          compressGeoprocessor = null;
        }

      }

    }   //  CompressFileGeoDB()


    public bool CompactFileGeodatabase(string FileGeodatabase)
    {
      System.Collections.Generic.IEnumerable<ESRI.ArcGIS.RuntimeInfo> runtimeInfo              = null;
      ESRI.ArcGIS.esriSystem.IVariantArray                            geoprocessorVariantArray = null;
      ESRI.ArcGIS.Geoprocessing.GeoProcessor                          compactGeoprocessor      = null;

      try
      {
        //  Make sure the File Geodatabase exists before attempting to compact it.
        if (!System.IO.Directory.Exists(FileGeodatabase))
        {
          //  Let the user know that the File Geodatabase does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The File Geodatabase - " + FileGeodatabase.ToUpper() + " does not exist and thus was not compacted...");
          }

          //  Return FALSE to the calling routine to indicate that this process failed.
          return false;

        }


        //  Let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - Attempting to compact the File Geodatabase - " + FileGeodatabase.ToUpper() + "...");
        }

        //  Determine the Install Path for the ArcGIS Software.
        runtimeInfo = ESRI.ArcGIS.RuntimeManager.InstalledRuntimes;
        string arcToolboxPath = null;
        foreach (ESRI.ArcGIS.RuntimeInfo currentRuntimeItem in runtimeInfo)
        {
          //  Retrieve the Install Path for the ArcGIS Desktop Runtime (ArcGIS Toolbox Items are found in this path).
          if (currentRuntimeItem.Product == ESRI.ArcGIS.ProductCode.Desktop)
          {
            arcToolboxPath = currentRuntimeItem.Path;
          }

        }

        //  If the ArcToolBox Directory Exists, add it to the Path.
        if (System.IO.Directory.Exists(System.IO.Path.Combine(arcToolboxPath, @"ArcToolbox")))
        {
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, @"ArcToolbox");
        }

        //  If the Toolboxes Directory Exists, add it to the Path.
        if (System.IO.Directory.Exists(System.IO.Path.Combine(arcToolboxPath, @"Toolboxes")))
        {
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, @"Toolboxes");
        }


        //  If the Toolbox File Exists, add it to the Toolbox path.
        if (System.IO.File.Exists(System.IO.Path.Combine(arcToolboxPath, "Data Management Tools.tbx")))
        {
          //  Add the File Name to the path.
          arcToolboxPath = System.IO.Path.Combine(arcToolboxPath, "Data Management Tools.tbx");
        }


        //  Build the Geoprocessor Variant Array that will pass the arguments to the Geoprocessing Tool.
        geoprocessorVariantArray = new ESRI.ArcGIS.esriSystem.VarArrayClass();
        geoprocessorVariantArray.Add(FileGeodatabase);


        //  Initialize the Geoprocessing Object that will be used to compact the Geodatabase.
        compactGeoprocessor = new ESRI.ArcGIS.Geoprocessing.GeoProcessor();
        compactGeoprocessor.AddToolbox(arcToolboxPath);


        try
        {
          //  Perform the export.
          compactGeoprocessor.Execute("Compact_management", geoprocessorVariantArray, null);
          //  Write the messages from the Compress Tool to the log file.
          if (ProcessMessage != null)
          {
            int toolMessageCount = compactGeoprocessor.MessageCount;
            int currentToolMessageIndex = 0;
            ProcessMessage("");
            ProcessMessage("         - Successfully Compacted the File Geodatabase...");
            ProcessMessage("            - Compact File Geodatabase Operation Messages...");
            while (currentToolMessageIndex < toolMessageCount)
            {
              //  Write the current message to the log file.
              ProcessMessage("              + " + compactGeoprocessor.GetMessage(currentToolMessageIndex));
              //  Increment the Tool Message Index Counter.
              currentToolMessageIndex++;
            }
          }

        }
        catch (System.IO.IOException ioException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Compress Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Compact File Geodatabase Operation in the FeatureClassUtilities.CompactFileGeodatabase() Method Failed with error message - " + ioException.Message + " (" + ioException.Source + " Line:  " + lineNumber.ToString() + ")!");
            if (ProcessMessage != null)
            {
              int toolMessageCount = compactGeoprocessor.MessageCount;
              int currentToolMessageIndex = 0;
              ProcessMessage("The information from the Geoprocessor is:");
              while (currentToolMessageIndex < toolMessageCount)
              {
                //  Write the current message to the log file.
                ProcessMessage("   + " + compactGeoprocessor.GetMessage(currentToolMessageIndex));
                //  Increment to Toold Message Index Counter.
                currentToolMessageIndex++;
              }
            }
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;

        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Compress Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Compact File Geodatabase Operation in the ComactFileGeoDataBase() Method Failed with error message - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
            if (ProcessMessage != null)
            {
              int toolMessageCount = compactGeoprocessor.MessageCount;
              int currentToolMessageIndex = 0;
              ProcessMessage("The information from the Geoprocessor is:");
              while (currentToolMessageIndex < toolMessageCount)
              {
                //  Write the current message to the log file.
                ProcessMessage("   + " + compactGeoprocessor.GetMessage(currentToolMessageIndex));
                //  Increment to Toold Message Index Counter.
                currentToolMessageIndex++;
              }
            }
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;

        }


        //  If the process made it to here, it was successful so return TRUE to the calling method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.CompactFileGeodatabase() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the Installed Runtimes List has been instantiated, close it.
        if (runtimeInfo != null)
        {
          runtimeInfo = null;
        }

        //  If the Geoprocessor Parameters Array has been instantiated, close it.
        if (geoprocessorVariantArray != null)
        {
          geoprocessorVariantArray = null;
        }

        //  If the Compact Geoprocessor Object has been instantiated, close it.
        if (compactGeoprocessor != null)
        {
          compactGeoprocessor = null;
        }

      }

    }   //  CompactFileGeodatabase()


    public string[] ListFeatureClassesInFileGeoDB(string FileGeoDatabaseDirectory, string FileGeodatabaseName)
    {
      try
      {
        //  Build the Full Path to the File Geodatabase.
        string fullGeodatabasePath = System.IO.Path.Combine(FileGeoDatabaseDirectory, FileGeodatabaseName);

        //  If the File Geodatabase Path does not end with ".gdb", add that extension to the path.
        if (!fullGeodatabasePath.EndsWith(@".gdb", System.StringComparison.CurrentCultureIgnoreCase))
        {
          //  Add the Extension to the File Geodatabase Path.
          fullGeodatabasePath = fullGeodatabasePath + @".gdb";

        }


        //  Make sure the File Geodatabase Exists before attempting to open it.
        if (!System.IO.Directory.Exists(fullGeodatabasePath))
        {
          //  Let the User know that the File Geodatabase could not be found.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified File Geodatabase - " + fullGeodatabasePath + " does not exits.  The MaintTools.FeatureClassUtilities.ListFeatureClassesInFileGeoDB() Method has failed!");
          }

          //  Return a NULL Array to the calling method to indicate that this method failed.
          return null;

        }



        //  Call the Method that requires a Full File Geodatabase Path as an argument to get the list of Feature Classes
        //  in the File Geodatabase.
        string[] featureClassesInFileGeoDB = ListFeatureClassesInFileGeoDB(fullGeodatabasePath);


        //  Return the List of Feature classes in the File Geodatabase to the calling method.
        return featureClassesInFileGeoDB;


      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ListFeatureClassesInFileGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Array to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  ListFeatureClassesInFileGeoDB()


    public string[] ListFeatureClassesInFileGeoDB(string FileGeoDatabasePath)
    {
      ESRI.ArcGIS.Geodatabase.IWorkspace fileGeodatabaseWorkspace = null;

      try
      {
        //  Make sure the File Geodatabase Exists before attempting to open it.
        if (!System.IO.Directory.Exists(FileGeoDatabasePath))
        {
          //  Let the User know that the File Geodatabase could not be found.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified File Geodatabase - " + FileGeoDatabasePath + " does not exits.  The MaintTools.FeatureClassUtilities.ListFeatureClassesInFileGeoDB() Method has failed!");
          }

          //  Return a NULL Array to the calling method to indicate that this method failed.
          return null;

        }


        //  Attempt to open the File Geodatabase Feature Workspace.
        fileGeodatabaseWorkspace = EstablishFileGeodatabaseConnection(FileGeoDatabasePath);

        //  Make sure the connection was established successfully before moving on.
        if (fileGeodatabaseWorkspace == null)
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The MaintTools.FeatureClassUtilities.ListFeatureClassesInFileGeoDB() Method failed while opening the File Geodatabase Workspace!");
          }

          //  Return a NULL Array to the calling method to indicate that this methdo failed.
          return null;

        }


        //  Call the Method that requires an ESRI Workspace as an argument to get the list of Feature Classes
        //  in the File Geodatabase.
        string[] featureClassesInFileGeoDB = ListFeatureClassesInGeodatabase(fileGeodatabaseWorkspace);


        //  Return the List of Feature classes in the File Geodatabase to the calling method.
        return featureClassesInFileGeoDB;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ListFeatureClassinFileGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the File Geodatabase Workspace has been Instantiated, close it.
        if (fileGeodatabaseWorkspace != null)
        {
          //  Close the File Geodatabase Workspace.
          fileGeodatabaseWorkspace = null;
        }

      }

    }   //  ListFeatureClassesInFileGeoDB()


    public bool MirrorFileGeodatabases(string SourceFileGeodatabasePath, string SourceFileGeodatabaseName, string DestinationFileGeodatabasePath, string DestinationFileGeodatabaseName, string RobocopyExecutablePath)
    {
      try
      {
        //  Call the overload of this method that requires a set of RoboCopy Mirror Command Line Switches to be passed defaulting the parameter list to an empty string.
        return MirrorFileGeodatabases(SourceFileGeodatabasePath, SourceFileGeodatabaseName, DestinationFileGeodatabasePath, DestinationFileGeodatabaseName, RobocopyExecutablePath, "");

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

    }   //  MirrorFileGeodatabases()


    public bool MirrorFileGeodatabases(string SourceFileGeodatabasePath, string SourceFileGeodatabaseName, string DestinationFileGeodatabasePath, string DestinationFileGeodatabaseName, string RobocopyExecutablePath, string MirrorOperationCommandLineParams)
    {
      try
      {
        //  If the Source File Geodatabase Name does not include the ".gdb" extension, add it.
        string fullSourceFileGeoDBName = null;
        if (SourceFileGeodatabaseName.IndexOf(".gdb", System.StringComparison.CurrentCulture) == -1)
        {
          //  Add the Extension to the Source File Geodatabase Name.
          fullSourceFileGeoDBName = SourceFileGeodatabaseName + ".gdb";

        }
        else
        {
          //  Use the Source File Geodatabase Name as it was passed.
          fullSourceFileGeoDBName = SourceFileGeodatabaseName;

        }


        //  Build the Full Path to Source File Geodatabase.
        string sourceFileGeoDBPathName = System.IO.Path.Combine(SourceFileGeodatabasePath, fullSourceFileGeoDBName);


        //  If the Destination File Geodatabase Name does not include the ".gdb" extension, add it.
        string fullDestinationFileGeoDBName = null;
        if (DestinationFileGeodatabaseName.IndexOf(".gdb", System.StringComparison.CurrentCulture) == -1)
        {
          //  Add the Extension to the Destination File Geodatabase Name.
          fullDestinationFileGeoDBName = DestinationFileGeodatabaseName + ".gdb";

        }
        else
        {
          //  Use the Destination File Geodatabase Name as it was passed.
          fullDestinationFileGeoDBName = DestinationFileGeodatabaseName;

        }


        //  Build the Full Path to Destination File Geodatabase.
        string destinationFileGeoDBPathName = System.IO.Path.Combine(DestinationFileGeodatabasePath, fullDestinationFileGeoDBName);


        //  Call the Overload of this method that requires Full Source and Destination File Geodatabase Paths be passed to it, using the paths that were just built.
        return MirrorFileGeodatabases(sourceFileGeoDBPathName, destinationFileGeoDBPathName, RobocopyExecutablePath, MirrorOperationCommandLineParams);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

    }   //  MirrorFileGeodatabases()


    public bool MirrorFileGeodatabases(string SourceFileGeodatabasePathName, string DestinationFileGeodatabasePathName, string RobocopyExecutablePath)
    {
      try
      {
        //  Call the overload of this method that requires a set of RoboCopy Mirror Command Line Switches to be passed defaulting the parameter list to an empty string.
        return MirrorFileGeodatabases(SourceFileGeodatabasePathName, DestinationFileGeodatabasePathName, RobocopyExecutablePath, "");

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

    }   //  MirrorFileGeodatabases()



    public bool MirrorFileGeodatabases(string SourceFileGeodatabasePathName, string DestinationFileGeodatabasePathName, string RobocopyExecutablePath, string MirrorOperationCommandLineParams)
    {
      System.Diagnostics.Process fileGeoDBMirrorProcess = null;

      try
      {
        //  Let the User know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("            > Initializing the File Geodatabase Mirroring Operation...");
          ProcessMessage("               < Verifying that the Source and Destination File Geodatabases Exist...");
        }

        //  Confirm that the Source File Geodatabase exists.
        if (!System.IO.Directory.Exists(SourceFileGeodatabasePathName))
        {
          //  Let the user know that the Source File Geodatabase does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Source File Geodatabase - " + SourceFileGeodatabasePathName + " - does not exist.  Aborting the MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Confirm that the Destination File Geodatabase exists.
        if (!System.IO.Directory.Exists(DestinationFileGeodatabasePathName))
        {
          //  Let the user know that the Destination File Geodatabase does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Destination File Geodatabase - " + DestinationFileGeodatabasePathName + " - does not exist.  Aborting the MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Let the User know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("               < Confirming the Robocopy Executable Path for this Server...");
        }


        //  Confirm that the Robocopy Executable Path is valid.
        if (!System.IO.File.Exists(RobocopyExecutablePath))
        {
          //  Let the user know that the Robocopy Executable Path is not valid
          if (ErrorMessage != null)
          {
            ErrorMessage("The Robocopy Executable Path - " + RobocopyExecutablePath + " - is not valid.  Aborting the MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Determine the Command Line switches that will be used for the Mirror Operation.
        string finalCommandLineSwitches = null;
        if (System.String.IsNullOrEmpty(MirrorOperationCommandLineParams))
        {
          //  Let the user know that the default command Line switches will be used.
          if (ProcessMessage != null)
          {
            ProcessMessage("               < No command line switches were passed to this method.  The following default switches will be used -");
            ProcessMessage("                  - /MIR /XF *.lock /R:5 /W:2 /NP /TEE");
          }

          //  Set the command line switches that will be used.
          finalCommandLineSwitches = "/MIR /XF *.lock /R:5 /W:2 /NP /TEE";

        }
        else
        {
          //  Let the user know that the passed command Line switches will be used.
          if (ProcessMessage != null)
          {
            ProcessMessage("               < The passed command line switches -" + MirrorOperationCommandLineParams + " - will be used for this mirror operation.");
          }

          //  Set the command line switches that will be used.
          finalCommandLineSwitches = MirrorOperationCommandLineParams;

        }


        //  Let the User know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("            > Beginning the mirror of the " + SourceFileGeodatabasePathName + " File Geodatabase to the " + DestinationFileGeodatabasePathName + " File Geodatabase...");
          ProcessMessage("               < Buiilding the Process to complete the mirror operation and starting the update...");
        }


        //  Build the System process to Update the File Geodatabase.
        fileGeoDBMirrorProcess = new System.Diagnostics.Process();
        fileGeoDBMirrorProcess.EnableRaisingEvents = false;
        fileGeoDBMirrorProcess.StartInfo.Arguments = SourceFileGeodatabasePathName + " " + DestinationFileGeodatabasePathName + " " + finalCommandLineSwitches;
        fileGeoDBMirrorProcess.StartInfo.FileName = RobocopyExecutablePath;
        fileGeoDBMirrorProcess.Start();
        fileGeoDBMirrorProcess.WaitForExit();

        //  Wait for the process to exit.
        while (!fileGeoDBMirrorProcess.HasExited)
        {
          //  Pause for 10 seconds to wait for the process to exit.
          System.Threading.Thread.Sleep(10000);
        }


        //  Let the User know that the Mirror Operation has finished.
        if (ProcessMessage != null)
        {
          ProcessMessage("               < The process to mirror the File Geodatabases has finished.  The exit code from the process was - " + fileGeoDBMirrorProcess.ExitCode.ToString() + "...");
        }

        //  Determine if the copy Operation finished successfully.
        if ((fileGeoDBMirrorProcess.ExitCode != 0) && (fileGeoDBMirrorProcess.ExitCode != 1) && (fileGeoDBMirrorProcess.ExitCode != 3) && (fileGeoDBMirrorProcess.ExitCode != 2))
        {
          //  Let the User know that the File Geodatabase mirror operation failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Mirror of the " + SourceFileGeodatabasePathName + " File Geodatabase to the " + DestinationFileGeodatabasePathName + " File Geodatabase failed.  Aborting the MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method!");
          }

          //  Return FALSE to the calling method to indicate that this process failed.
          return false;

        }
        else
        {
          //  Let the User know that the Mirror Operation finished successfully.
          if (ProcessMessage != null)
          {
            ProcessMessage("               < Successfully mirrored the File Geodatabases...");
          }

          //  Return TRUE to the calling method to indicate that the mirror operation finished successfully.
          return true;

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.MirrorFileGeodatabases() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the System Process Object was instantiated, close it.
        if (fileGeoDBMirrorProcess != null)
        {
          fileGeoDBMirrorProcess.Dispose();
          fileGeoDBMirrorProcess = null;
        }

      }

    }   //  MirrorFileGeodatabases()


    public bool EmptyFileGeodatabaseFeatureClass(string FileGeodatabasedirectory, string FileGeodatabaseName, string FeatureClassName)
    {
      try
      {
        //  Build the File Geodatabase Path.
        string fileGeodatabasePathName = System.IO.Path.Combine(FileGeodatabasedirectory, FileGeodatabaseName);

        //  Make sure the path ends with ".gdb".
        if (!fileGeodatabasePathName.EndsWith(".gdb", System.StringComparison.CurrentCultureIgnoreCase))
        {
          //  Add the extension to the File Geodatabase Path and Name.
          fileGeodatabasePathName = fileGeodatabasePathName + ".gdb";
        }


        //  Attempt to delete the Feature Class from the File Geodatabase and return the result to the calling method.
        return EmptyFileGeodatabaseFeatureClass(fileGeodatabasePathName, FeatureClassName);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EmptyFileGeodatabaseFeatureClass() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }


    public bool EmptyFileGeodatabaseFeatureClass(string FileGeodatabase, string FeatureClassName)
    {
      ESRI.ArcGIS.Geodatabase.IWorkspace        fileGeodatabaseWorkspace = null;
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace fileGeodatabaseFeatureWorkspace = null;
      ESRI.ArcGIS.Geodatabase.IFeatureClass     fileGeodatabaseFeatureClass = null;

      try
      {
        //  Attempt to establish a connection to the specified File Geodatabas.
        fileGeodatabaseWorkspace = EstablishFileGeodatabaseConnection(FileGeodatabase);

        //  Make sure the connection was established successfully before moving on.
        if (fileGeodatabaseWorkspace == null)
        {
          //  Let the user know that the connection could not be established.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not establish a connection to the specified File Geodatabase - " + FileGeodatabase + ".  Aborting the MaintTools.FeatureClassUtilities.EmptyFileGeodatabaseFeatureClass() Method!");
          }
          //  Return FALSE to the calling method to indicate that this method failed.
          return false;
        }


        //  Open the Feature Class in the File Geodatabase.
        fileGeodatabaseFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)fileGeodatabaseWorkspace;
        fileGeodatabaseFeatureClass = fileGeodatabaseFeatureWorkspace.OpenFeatureClass(FeatureClassName);

        //  Make sure the Feature Class was opened successfully before moving on.
        if (fileGeodatabaseFeatureClass == null)
        {
          //  Let the user know that the connection could not be established.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not open the specified File Geodatabase Feature Class - " + FeatureClassName + ".  Aborting the MaintTools.FeatureClassUtilities.EmptyFileGeodatabaseFeatureClass() Method!");
          }
          //  Return FALSE to the calling method to indicate that this method failed.
          return false;
        }


        //  Empty the Feature Class and return the result to the calling method.
        return EmptyGeodatabaseFeatureClass(fileGeodatabaseFeatureClass);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EmptyFileGeodatabaseFeatureClass() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the File Geodatabase Feature Class Object was instantiated, close it.
        if (fileGeodatabaseFeatureClass != null)
        {
          fileGeodatabaseFeatureClass = null;
        }
        //  If the File Geodatabase Feature Workspace Object was instantiated, close it.
        if (fileGeodatabaseFeatureWorkspace != null)
        {
          fileGeodatabaseFeatureWorkspace = null;
        }
        //  If the File Geodatabase Workspace Object was instantiated, close it.
        if (fileGeodatabaseWorkspace != null)
        {
          fileGeodatabaseWorkspace = null;
        }

      }

    }   //  EmptyFileGeodatabaseFeatureClass()
    #endregion File Geodatabase Utilities


    #region General Geodatabase Utilities
    public ESRI.ArcGIS.Geodatabase.IWorkspace EstablishEnterpriseGeoDBConnection(string GeodatabaseServerName, string GeodatabaseInstance, string GeodatabaseName, string UserName = "#", string UserPassword = "#", string Version = "SDE.Default")
    {
      ESRI.ArcGIS.esriSystem.IPropertySet geodatabasePropertySet = null;

      try
      {
        //  If No User Name was passed to this method, use Operating System Authentication.  Otherwise, include the passed User Name and
        //  Password in the Property Set that will be used to connect to the Enterprise Geodatabase.
        if (UserName == "#")
        {
          //  Build the Property Set that will be used to connect to the Enterprise Geodatabase.
          geodatabasePropertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
          geodatabasePropertySet.SetProperty("SERVER", GeodatabaseServerName);
          geodatabasePropertySet.SetProperty("INSTANCE", GeodatabaseInstance);
          geodatabasePropertySet.SetProperty("DATABASE", GeodatabaseName);
          geodatabasePropertySet.SetProperty("AUTHENTICATION_MODE", "OSA");
          geodatabasePropertySet.SetProperty("VERSION", Version);

        }
        else
        {
          //  Build the Property Set (including the passed User Name and Password) that will be used to connect to the Enterprise Geodatabase.
          geodatabasePropertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
          geodatabasePropertySet.SetProperty("SERVER", GeodatabaseServerName);
          geodatabasePropertySet.SetProperty("INSTANCE", GeodatabaseInstance);
          geodatabasePropertySet.SetProperty("DATABASE", GeodatabaseName);
          geodatabasePropertySet.SetProperty("USER", UserName);
          geodatabasePropertySet.SetProperty("PASSWORD", UserPassword);
          geodatabasePropertySet.SetProperty("VERSION", Version);

        }


        //  Call the overload of this method that takes an ESRI Property Set as a parameter and return the result of the connection attempt
        //  to the calling method.
        return EstablishEnterpriseGeoDBConnection(geodatabasePropertySet);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EstablishEnterpriseGeoDBConnection() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the ESRI System Geodatabase Property Set Object was instantiated, close it.
        if (geodatabasePropertySet != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(geodatabasePropertySet);
        }

      }

    }   //  EstablishEnterpriseGeoDBConnection()


    public ESRI.ArcGIS.Geodatabase.IWorkspace EstablishEnterpriseGeoDBConnection(string GeodatabaseInstance, string GeodatabaseName, string UserName = "#", string UserPassword = "#", string Version = "")
    {
      ESRI.ArcGIS.esriSystem.IPropertySet geodatabasePropertySet = null;

      try
      {
        //  Retrieve the Server Name from the Instance Value.
        string serverName = GeodatabaseInstance;
        while (serverName.IndexOf(":") != -1)
        {
          serverName = serverName.Substring(1);
        }
        if (serverName.IndexOf(@"\") != -1)
        {
          serverName = serverName.Substring(0, (serverName.IndexOf(@"\")));
        }


        //  If No User Name was passed to this method, use Operating System Authentication.  Otherwise, include the passed User Name and
        //  Password in the Property Set that will be used to connect to the Enterprise Geodatabase.
        if (UserName == "#")
        {
          //  Build the Property Set that will be used to connect to the Enterprise Geodatabase.
          geodatabasePropertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
          geodatabasePropertySet.SetProperty("SERVER", serverName);
          geodatabasePropertySet.SetProperty("INSTANCE", GeodatabaseInstance);
          geodatabasePropertySet.SetProperty("DATABASE", GeodatabaseName);
          geodatabasePropertySet.SetProperty("AUTHENTICATION_MODE", "OSA");
          if (!System.String.IsNullOrEmpty(Version))
          {
            geodatabasePropertySet.SetProperty("VERSION", Version);
          }

        }
        else
        {
          //  Build the Property Set (including the passed User Name and Password) that will be used to connect to the Enterprise Geodatabase.
          geodatabasePropertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
          geodatabasePropertySet.SetProperty("SERVER", serverName);
          geodatabasePropertySet.SetProperty("INSTANCE", GeodatabaseInstance);
          geodatabasePropertySet.SetProperty("DATABASE", GeodatabaseName);
          geodatabasePropertySet.SetProperty("USER", UserName);
          geodatabasePropertySet.SetProperty("PASSWORD", UserPassword);
          if (!System.String.IsNullOrEmpty(Version))
          {
            geodatabasePropertySet.SetProperty("VERSION", Version);
          }

        }


        //  Call the overload of this method that takes an ESRI Property Set as a parameter and return the result of the connection attempt
        //  to the calling method.
        return EstablishEnterpriseGeoDBConnection(geodatabasePropertySet);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EstablishEnterpriseGeoDBConnection() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the ESRI System Geodatabase Property Set Object was instantiated, close it.
        if (geodatabasePropertySet != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(geodatabasePropertySet);
        }

      }

    }   //  EstablishEnterpriseGeoDBConnection()


    public ESRI.ArcGIS.Geodatabase.IWorkspace EstablishEnterpriseGeoDBConnection(ESRI.ArcGIS.esriSystem.IPropertySet GeodatabasePropertySet)
    {
      System.Type                               entGeodatabaseFactoryType      = null;
      System.Object                             entGeodatabaseFactoryObject    = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceFactory entGeodatabaseWorkspaceFactory = null;
      ESRI.ArcGIS.Geodatabase.IWorkspace        entGeodatabaseWorkspace        = null;

      try
      {
        //  Attempt to open the File Geodatabase Workspace.
        entGeodatabaseFactoryType = System.Type.GetTypeFromProgID("esriDataSourcesGDB.SDEWorkspaceFactory");
        entGeodatabaseFactoryObject = System.Activator.CreateInstance(entGeodatabaseFactoryType);
        entGeodatabaseWorkspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory)entGeodatabaseFactoryObject;
        entGeodatabaseWorkspace = entGeodatabaseWorkspaceFactory.Open(GeodatabasePropertySet, 0);


        //  Return the File Geodatabase Workspace pointer to the calling method.
        return entGeodatabaseWorkspace;

      }
      catch (System.Runtime.InteropServices.COMException comException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EstablishEnterpriseGeoDBConnection() Method failed while opening the Enterprise Geodatabase Workspace with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Pointer to the calling method to indicate that this methdo failed.
        return null;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EstablishEnterpriseGeoDBConnection() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the Enterprise Geodatabase Workspace Object was instantiated, close it.
        if (entGeodatabaseWorkspace != null)
        {
          entGeodatabaseWorkspace = null;
        }
        //  If the Enterprise Geodatabase Workspace Factory Object was instantiated, close it.
        if (entGeodatabaseWorkspaceFactory != null)
        {
          entGeodatabaseWorkspaceFactory = null;
        }

      }

    }   //  EstablishEnterpriseGeoDBConnection()

    
    public string[] ListFeatureClassesInGeodatabase(ESRI.ArcGIS.Geodatabase.IWorkspace GeoDatabaseWorkspace)
    {
      ESRI.ArcGIS.Geodatabase.IEnumDataset            geodatabaseDatasets                = null;
      ESRI.ArcGIS.Geodatabase.IDataset                currentGeoDataBaseDataset          = null;
      ESRI.ArcGIS.DataSourcesGDB.FgdbFeatureClassName currentGeoDataBaseFeatureClassName = null;

      try
      {
        //  Make sure the passed Workspace is valid before moving on.
        if (GeoDatabaseWorkspace != null)
        {
          //  Make sure the Workspace is a valid ESRI Workspace.
          if (!(GeoDatabaseWorkspace is ESRI.ArcGIS.Geodatabase.IWorkspace))
          {
            //  Let the user know that the Workspace is not valid and.
            if (ErrorMessage != null)
            {
              ErrorMessage("The specified Workspace is not a Valid Geodatabase Workspace.  The MaintTools.FeatureClassUtilities.ListFeatureClassesInGeodatabase() Method Failed!");
            }
            //  Return a NULL Array to the calling method to indicate that this method failed.
            return null;
          }

        }
        else
        {
          //  Let the user know that the Workspace is not valid and.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified Workspace is not a Valid Geodatabase Workspace.  The MaintTools.FeatureClassUtilities.ListFeatureClassesInGeodatabase() Method Failed!");
          }

          //  Return a NULL Array to the calling method to indicate that this method failed.
          return null;

        }


        //  Now that the File Geodatabase is open, open a Dataset in it.
        geodatabaseDatasets = GeoDatabaseWorkspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass);
        geodatabaseDatasets.Reset();

        //  Determine the Number of Datasets that were Found.
        int counter = 0;
        currentGeoDataBaseDataset = geodatabaseDatasets.Next();
        while (currentGeoDataBaseDataset != null)
        {
          //  Increment the Counter.
          counter++;

          //  Move to the Next Dataset in the Geodatabase Datasets Enumerator.
          currentGeoDataBaseDataset = geodatabaseDatasets.Next();

        }


        //  Go through the List of Datasets in the Geodatabase and add their names to the Array of Feature Class Names.
        string[] featureClassNames = new string[counter];
        geodatabaseDatasets.Reset();
        currentGeoDataBaseDataset = geodatabaseDatasets.Next();
        int arrayIndex = 0;
        while (currentGeoDataBaseDataset != null)
        {
          //  Add the Name to the Array of Feature Class Names.
          featureClassNames[arrayIndex] = currentGeoDataBaseDataset.BrowseName;

          //  Increment the Array Index.
          arrayIndex++;

          //  Move to the Next Dataset in the Geodatabase Datasets Enumerator.
          currentGeoDataBaseDataset = geodatabaseDatasets.Next();

        }


        //  Return the Array of Feature Class names to the Calling Method.
        return featureClassNames;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ListFeatureClassesInGeodatabase() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the Geodatabase Dataset Object has been Instantiated, close it.
        if (currentGeoDataBaseDataset != null)
        {
          //  Close the Geodatabase Dataset Object.
          currentGeoDataBaseDataset = null;
        }
        //  If the Geodatabase Feature Class Name Object has been Instantiated, close it.
        if (currentGeoDataBaseFeatureClassName != null)
        {
          //  Close the Geodatabase Feature Class Name Object.
          currentGeoDataBaseFeatureClassName = null;
        }
        //  If the Geodatabase Datasets Enumerator has been Instantiated, close it.
        if (geodatabaseDatasets != null)
        {
          //  Close the Geodatabase Datasets Enumerator.
          geodatabaseDatasets = null;
        }

      }

    }   //  ListFeatureClassinFileGeoDB()
    #endregion General Geodatabase Utilities


    #region Locator Management
    /// <summary>
    /// Updates any locators in the specified File Geodatabase that use the specified Feature Class as Primary Reference Table.  Any Alternate Names Tables that exist for the
    /// specified Feature Class will be copied from the specified Enterprise Geodatabase (InputWorkspace) to the File Geodatabase.
    /// </summary>
    /// <param name="OutputFileGeodatabaseName">
    /// The Name of the Output File Geodatabase that contains the Locators that are to be rebuilt.
    /// </param>
    /// <param name="OutputFileGeodatabasePath">
    /// The directory path to the directory that houses the specified Output File Geodatabase.
    /// </param>
    /// <param name="FeatureClassName">
    /// The Name of the Feature Class that has been updated in the Output File Geodatabase.
    /// </param>
    /// <param name="InputSDEInstance">
    /// The source SDE Instance that will be used to connect to the Source Enterprise Geodatabase Server.
    /// </param>
    /// <param name="InputSDEDatabaseName">
    /// The Name of the Geodatabase on the specified Source Enterprise Geodatabase Server.
    /// </param>
    /// <param name="InputSDEDataBaseSchemaOwnerName">
    /// The Name of the Schema Owner in the source Enterprise Geodatabase.
    /// </param>
    /// <returns>
    /// TRUE if all locators in which the udpated feature class participates have been rebuilt successfully.
    /// FALSE if the locators were not successfully rebuilt.
    /// </returns>
    public bool UpdateFileGeodatabaseLocators(string OutputFileGeodatabaseName, string OutputFileGeodatabasePath, string FeatureClassName, string InputSDEInstance, string InputSDEDatabaseName, string InputSDEDataBaseSchemaOwnerName)
    {
      try
      {
        //  Build the full Path for the Output File Geodatabase.
        string outputFileGeodatabasePathName = null;
        if (OutputFileGeodatabaseName.EndsWith(".gdb", System.StringComparison.CurrentCultureIgnoreCase))
        {
          outputFileGeodatabasePathName = System.IO.Path.Combine(OutputFileGeodatabasePath, OutputFileGeodatabaseName);
        }
        else
        {
          outputFileGeodatabasePathName = System.IO.Path.Combine(OutputFileGeodatabasePath, OutputFileGeodatabaseName + ".gdb");
        }


        //  Call the overload of this method that requires a Full Output File Geodatabase Path and Name as a parameter.
        return UpdateFileGeodatabaseLocators(outputFileGeodatabasePathName, FeatureClassName, InputSDEInstance, InputSDEDatabaseName, InputSDEDataBaseSchemaOwnerName);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The FeatureClassUtilities.UpdateFileGeodatabaseLocators() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

    }   //  UpdateFileGeodatabaseLocators()


    /// <summary>
    /// Updates any locators in the specified File Geodatabase that use the specified Feature Class as Primary Reference Table.  Any Alternate Names Tables that exist for the
    /// specified Feature Class will be copied from the specified Enterprise Geodatabase (InputWorkspace) to the File Geodatabase.
    /// </summary>
    /// <param name="OutputFileGeodatabasePathName">
    /// The Full Path to the Output File Geodatabase that contains the Locators that are to be rebuilt.
    /// </param>
    /// <param name="FeatureClassName">
    /// The Name of the Feature Class that has been updated in the Output File Geodatabase.
    /// </param>
    /// <param name="InputSDEInstance">
    /// The source SDE Instance that will be used to connect to the Source Enterprise Geodatabase Server.
    /// </param>
    /// <param name="InputSDEDatabaseName">
    /// The Name of the Geodatabase on the specified Source Enterprise Geodatabase Server.
    /// </param>
    /// <returns>
    /// TRUE if all locators in which the udpated feature class participates have been rebuilt successfully.
    /// FALSE if the locators were not successfully rebuilt.
    /// </returns>
    public bool UpdateFileGeodatabaseLocators(string OutputFileGeodatabasePathName, string FeatureClassName, string InputSDEInstance, string InputSDEDatabaseName)
    {
      try
      {
        //  Call the Overload of this Method that requires an Input SDE Geodatabase Connection Object to rebuild the locators.
        return UpdateFileGeodatabaseLocators(OutputFileGeodatabasePathName, FeatureClassName, InputSDEInstance, InputSDEDatabaseName, "ArcMap_Admin");

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The FeatureClassUtilities.UpdateFileGeodatabaseLocators() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

    }   //  UpdateFileGeodatabaseLocators()


    /// <summary>
    /// Updates any locators in the specified File Geodatabase that use the specified Feature Class as Primary Reference Table.  Any Alternate Names Tables that exist for the
    /// specified Feature Class will be copied from the specified Enterprise Geodatabase (InputWorkspace) to the File Geodatabase.
    /// </summary>
    /// <param name="OutputFileGeodatabasePathName">
    /// The Full Path to the Output File Geodatabase that contains the Locators that are to be rebuilt.
    /// </param>
    /// <param name="FeatureClassName">
    /// The Name of the Feature Class that has been updated in the Output File Geodatabase.
    /// </param>
    /// <param name="InputSDEInstance">
    /// The source SDE Instance that will be used to connect to the Source Enterprise Geodatabase Server.
    /// </param>
    /// <param name="InputSDEDatabaseName">
    /// The Name of the Geodatabase on the specified Source Enterprise Geodatabase Server.
    /// </param>
    /// <param name="InputSDEDataBaseSchemaOwnerName">
    /// The Name of the Schema Owner in the source Enterprise Geodatabase.
    /// </param>
    /// <returns>
    /// TRUE if all locators in which the udpated feature class participates have been rebuilt successfully.
    /// FALSE if the locators were not successfully rebuilt.
    /// </returns>
    public bool UpdateFileGeodatabaseLocators(string OutputFileGeodatabasePathName, string FeatureClassName, string InputSDEInstance, string InputSDEDatabaseName, string InputSDEDataBaseSchemaOwnerName)
    {
      PDX.BTS.DataMaintenance.LoaderUtilities.GeodatabaseTools geodatabaseTools  = null;
      ESRI.ArcGIS.Geodatabase.IWorkspace                       inputSDEWorkspace = null;

      try
      {
        //  Instantiate the CGIS Data Loader Utilities Geodatabase Tools Object that will be used to establish a Connection to the Input SDE Geodatabase.
        geodatabaseTools = new PDX.BTS.DataMaintenance.LoaderUtilities.GeodatabaseTools();


        //  Establish a Connection to the Input SDE Geodatabase.
        inputSDEWorkspace = geodatabaseTools.OpenGeodatabaseConnection(InputSDEInstance, InputSDEDatabaseName);

        //  Make sure the Input SDE Workspace was established successfully before moving on.
        if (inputSDEWorkspace == null)
        {
          //  Let the user know that this process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("Failed to establish a connection to the Input Enterprise SDE Geodatabase.  The Maintools.FeatureClassUtilities.UpdateFileGeodatabaseLocators() Method failed!");
          }

          //  Return false to the calling method to indicate that this method failed.
          return false;

        }


        //  Call the Overload of this Method that requires an Input SDE Geodatabase Connection Object to rebuild the locators.
        return UpdateFileGeodatabaseLocators(OutputFileGeodatabasePathName, FeatureClassName, inputSDEWorkspace, InputSDEDataBaseSchemaOwnerName);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The FeatureClassUtilities.UpdateFileGeodatabaseLocators() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the Input SDE Workspace Object was instantiated, close it.
        if (inputSDEWorkspace != null)
        {
          inputSDEWorkspace = null;
        }
        //  If the CGIS Data Loader Utilities Geodatabase Tools Object was instantiated, close it.
        if (geodatabaseTools != null)
        {
          geodatabaseTools = null;
        }

      }

    }   //  UpdateFileGeodatabaseLocators()


    /// <summary>
    /// Updates any locators in the specified File Geodatabase that use the specified Feature Class as Primary Reference Table.  Any Alternate Names Tables that exist for the
    /// specified Feature Class will be copied from the specified Enterprise Geodatabase (InputWorkspace) to the File Geodatabase.
    /// </summary>
    /// <param name="OutputFileGeodatabasePathName">
    /// The Full Path to the Output File Geodatabase that contains the Locators that are to be rebuilt.
    /// </param>
    /// <param name="FeatureClassName">
    /// The Name of the Feature Class that has been updated in the Output File Geodatabase.
    /// </param>
    /// <param name="InputWorkspace">
    /// An ESRI ArcGIS Workspace Object that points to the Enterprise Geodatabase that is being used as the source for updating the Output File Geodatabase datasets.
    /// </param>
    /// <returns>
    /// TRUE if all locators in which the udpated feature class participates have been rebuilt successfully.
    /// FALSE if the locators were not successfully rebuilt.
    /// </returns>
    public bool UpdateFileGeodatabaseLocators(string OutputFileGeodatabasePathName, string FeatureClassName, ESRI.ArcGIS.Geodatabase.IWorkspace InputWorkspace)
    {
      try
      {
        //  Call the Overload of this method that requires a CGIS Enterprise Geodatabase Schema Onwer Name, defaulting the name to "ArcMap_Admin".
        return UpdateFileGeodatabaseLocators(OutputFileGeodatabasePathName, FeatureClassName, InputWorkspace, "ArcMap_Admin");

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.FeatureClassUtilities.UpdateFileGeodatabaseLocators() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

    }   //  UpdateFileGeodatabaseLocators()


    /// <summary>
    /// Updates any locators in the specified File Geodatabase that use the specified Feature Class as Primary Reference Table.  Any Alternate Names Tables that exist for the
    /// specified Feature Class will be copied from the specified Enterprise Geodatabase (InputWorkspace) to the File Geodatabase.
    /// </summary>
    /// <param name="OutputFileGeodatabasePathName">
    /// The Full Path to the Output File Geodatabase that contains the Locators that are to be rebuilt.
    /// </param>
    /// <param name="FeatureClassName">
    /// The Name of the Feature Class that has been updated in the Output File Geodatabase.
    /// </param>
    /// <param name="InputWorkspace">
    /// An ESRI ArcGIS Workspace Object that points to the Enterprise Geodatabase that is being used as the source for updating the Output File Geodatabase datasets.
    /// </param>
    /// <param name="InputWorkspaceDBSchemaOwnerName">
    /// The Name of the Schema Owner in the source Enterprise Geodatabase.
    /// </param>
    /// <returns>
    /// TRUE if all locators in which the udpated feature class participates have been rebuilt successfully.
    /// FALSE if the locators were not successfully rebuilt.
    /// </returns>
    public bool UpdateFileGeodatabaseLocators(string OutputFileGeodatabasePathName, string FeatureClassName, ESRI.ArcGIS.Geodatabase.IWorkspace InputWorkspace, string InputWorkspaceDBSchemaOwnerName)
    {
      ESRI.ArcGIS.Geodatabase.IWorkspace              geodatabaseWorkspace         = null;
      ESRI.ArcGIS.Location.ILocatorManager            locatorManager               = null;
      ESRI.ArcGIS.Geodatabase.ILocatorWorkspace2      locatorWorkspace             = null;
      ESRI.ArcGIS.Geodatabase.IEnumLocatorName        locatorNamesEnum             = null;
      System.Collections.Specialized.StringCollection copiedReferenceTables        = null;
      ESRI.ArcGIS.Geodatabase.ILocatorName            currentLocatorName           = null;
      ESRI.ArcGIS.Geodatabase.ILocator                currentLocator               = null;
      ESRI.ArcGIS.Location.IReferenceDataTables       locatorReferenceTables       = null;
      ESRI.ArcGIS.Location.IEnumReferenceDataTable    locatorReferenceTablesEnum   = null;
      ESRI.ArcGIS.Location.IReferenceDataTable2       currentLocatorReferenceTable = null;
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace       geodatabaseFeatureWorkspace  = null;
      ESRI.ArcGIS.Geodatabase.ITable                  deleteTable                  = null;
      ESRI.ArcGIS.Geodatabase.IDataset                deleteDataset                = null;
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace       inputFeatureWorkspace        = null;
      ESRI.ArcGIS.Geodatabase.ITable                  inputAlternateNameTable      = null;
      ESRI.ArcGIS.Geodatabase.IDataset                inputTableDataset            = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceFactory       inputWorkspaceFactory        = null;
      ESRI.ArcGIS.Geoprocessing.GeoProcessor          geoProcessorObject           = null;
      ESRI.ArcGIS.esriSystem.IVariantArray            tableToTableParams           = null;
      ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult   geoprocessorResult           = null;
      ESRI.ArcGIS.Geoprocessing.GeoProcessor          rebuildGeoprocessorObject    = null;
      ESRI.ArcGIS.esriSystem.IVariantArray            rebuildLocatorParams         = null;
      ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult   rebuildGeoprocessorResult    = null;

      try
      {
        //  Let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("       - Initializing the process to rebuild any locators and determining if the specified Feature Class is a reference dataset for any locators in the File Geodatabase...");
        }

        //  Open the Locator Workspace.
        geodatabaseWorkspace = EstablishFileGeodatabaseConnection(OutputFileGeodatabasePathName);
        //  Make sure the Output File Geodatabase Workspace Connection was established successfully before moving on.
        if (geodatabaseWorkspace == null)
        {
          //  Let the user know that the connection could not be established.
          if (ErrorMessage != null)
          {
            ErrorMessage("Failed to open the Output File Geodatabase Workspace - " + OutputFileGeodatabasePathName + ".  Aborting the Locator Rebuild!");
          }
          //  Return FALSE to the calling method to indicate that this method failed.
          return false;
        }
        locatorManager = new ESRI.ArcGIS.Location.LocatorManagerClass();
        locatorWorkspace = (ESRI.ArcGIS.Geodatabase.ILocatorWorkspace2)locatorManager.GetLocatorWorkspace(geodatabaseWorkspace);


        //  Get the list of Address Locator Names from the File Geodatabase.
        locatorNamesEnum = locatorWorkspace.get_LocatorNames(ESRI.ArcGIS.Geodatabase.esriLocatorQuery.esriLocator, "Address");


        //  Create a List of Reference Tables that are copied by this process so that they will not be copied multiple times if it is used by multiple locators.
        copiedReferenceTables = new System.Collections.Specialized.StringCollection();

        //  Go through the list of associated locators and rebuild them.
        currentLocatorName = locatorNamesEnum.Next();
        while (currentLocatorName != null)
        {
          //  If there is a valid Locator Name, rebuild the locator.
          if (currentLocatorName.Name.Length > 1)
          {
            //  Let the User know that the locator is being rebuilt.
            if (ProcessMessage != null)
            {
              ProcessMessage("          + Determining if the " + FeatureClassName + " Feature Class participates in the " + currentLocatorName.Name.ToString() + " Locator...");
            }
            //  Open the Current Locator.
            currentLocator = locatorWorkspace.GetLocator(currentLocatorName.Name);

            //  If the Current Locator is a composite Locator, skip it.
            if (!(currentLocator is ESRI.ArcGIS.Location.ICompositeLocator))
            {
              //  Determine if the specified Feature Class is a reference Dataset for the locator.
              locatorReferenceTables = (ESRI.ArcGIS.Location.IReferenceDataTables)currentLocator;
              locatorReferenceTablesEnum = locatorReferenceTables.Tables;
              locatorReferenceTablesEnum.Reset();
              //  Retrieve the First Table from the Locator.
              currentLocatorReferenceTable = (ESRI.ArcGIS.Location.IReferenceDataTable2)locatorReferenceTablesEnum.Next();
              //  Default the Found the Locator Indicator to FALSE.
              bool foundIt = false;
              //  If the Updated Feature Class is the Primary Table for this locator, the locator needs to be rebuilt so default the 'FoundIt' indicator to true.
              while (currentLocatorReferenceTable != null)
              {
                //  Determine if the current table is the specified Feature Class Business Table.
                if (currentLocatorReferenceTable.DisplayName.ToUpper() == "PRIMARY TABLE")
                {
                  ESRI.ArcGIS.Geodatabase.IDatasetName currentFeatureClassName = null;
                  currentFeatureClassName = (ESRI.ArcGIS.Geodatabase.IDatasetName)currentLocatorReferenceTable.Name;
                  if (currentFeatureClassName.Name.ToString().ToUpper() == FeatureClassName.ToUpper())
                  {
                    //  Set the found the locator indicator to TRUE.
                    foundIt = true;
                  }
                }
                //  Retrieve the Next Table from the Locator.
                currentLocatorReferenceTable = (ESRI.ArcGIS.Location.IReferenceDataTable2)locatorReferenceTablesEnum.Next();
              }
              //  If the specified Feature Class does participate in the current Locator, rebuild the Locator.
              if (foundIt)
              {
                //  Let the User know that the locator is being rebuilt.
                if (ProcessMessage != null)
                {
                  ProcessMessage("          + Starting the rebuild of the " + currentLocatorName.Name.ToString() + " Locator...");
                }
                //  Reset the Locator Reference Tables enumerator.
                locatorReferenceTablesEnum.Reset();
                //  Retrieve the First Table from the Locator.
                currentLocatorReferenceTable = (ESRI.ArcGIS.Location.IReferenceDataTable2)locatorReferenceTablesEnum.Next();
                //  Go through the Locator Reference Tables and rebuild any Alternate Name Tables that are associated with the locator.
                while (currentLocatorReferenceTable != null)
                {
                  if (currentLocatorReferenceTable.DisplayName.ToUpper() == "ALTERNATE NAME TABLE")
                  {
                    ESRI.ArcGIS.Geodatabase.IDatasetName currentTableName = null;
                    currentTableName = (ESRI.ArcGIS.Geodatabase.IDatasetName)currentLocatorReferenceTable.Name;
                    //  Alternate Name Tables have the string "_ALT" inserted in the name before the "_PDX" so, remove the "_PDX" from the Feature Class Name so that it can be
                    //  found in the Alternate Table Name.
                    string searchName = FeatureClassName;
                    if (searchName.Substring(searchName.Length - 3).ToUpper() == "PDX")
                    {
                      searchName = searchName.Substring(0, searchName.Length - 4);
                    }
                    //  If the current Alternate Name Table is associated with this locator, delete it and  copy the most current version from the source Geodatabase.
                    if (currentTableName.Name.ToString().ToUpper().IndexOf(searchName.ToUpper()) != -1)
                    {
                      //  If the Table Name includes some prefix information (Database Name and Table Owner Name), drop it for attempting to find the table in the search Geodatabase.
                      string tableSearchName = currentTableName.Name.ToString();
                      if (tableSearchName.ToUpper().IndexOf(searchName.ToUpper()) > 0)
                      {
                        //  Drop the prefix from the Table Name.
                        tableSearchName = tableSearchName.Substring(tableSearchName.IndexOf(searchName.ToUpper()));
                      }
                      //  If the Table has not already been updated by this process, update it.
                      if (!copiedReferenceTables.Contains(tableSearchName))
                      {
                        //  Let the user know which locator is being rebuilt.
                        if (ProcessMessage != null)
                        {
                          ProcessMessage("             < Deleting the - " + tableSearchName + " Alternate Name Table for the " + currentLocatorName.Name + " locator...");
                        }
                        geodatabaseFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)geodatabaseWorkspace;
                        deleteTable = geodatabaseFeatureWorkspace.OpenTable(currentTableName.Name.ToString());
                        deleteDataset = (ESRI.ArcGIS.Geodatabase.IDataset)deleteTable;
                        deleteDataset.Delete();
                        //  Release the Delete Objects to remove locks on the reference table.
                        if (deleteDataset != null)
                        {
                          System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteDataset);
                        }
                        if (deleteTable != null)
                        {
                          System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteTable);
                        }
                        //  Attempt to open the Source Table in the Input Geodatabase.
                        inputFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)InputWorkspace;

                        inputAlternateNameTable = (ESRI.ArcGIS.Geodatabase.ITable)inputFeatureWorkspace.OpenTable(InputWorkspaceDBSchemaOwnerName + "." + tableSearchName);
                        inputTableDataset = (ESRI.ArcGIS.Geodatabase.IDataset)inputAlternateNameTable;
                        //  If the Table was opened successfully, Attempt to Copy it to the File Geodatabase.
                        string temporaryDirectory = null;
                        if (inputAlternateNameTable != null)
                        {
                          //  Determine in which directory the Temporary SDE Connection File should
                          //  be created.
                          if (System.IO.Directory.Exists(@"D:\Temp"))
                          {
                            //  Set the Temporary Directory to 'D:\TEMP\'.
                            temporaryDirectory = @"D:\Temp\";
                          }
                          else
                          {
                            //  Check to see if there is a 'C:\TEMP' Directory.
                            if (System.IO.Directory.Exists(@"C:\Temp"))
                            {
                              //  Set the Temporary Directory to 'C:\Temp\'
                              temporaryDirectory = @"C:\Temp\";
                            }
                            else
                            {
                              //  Set the Temporary Directory to 'C:\'.
                              temporaryDirectory = @"C:\";
                            }
                          }
                          //  Create a Connection File for the Input Enterprise Geodatabase Connection.
                          inputWorkspaceFactory = InputWorkspace.WorkspaceFactory;
                          inputWorkspaceFactory.Create(temporaryDirectory, "inputConnection.sde", InputWorkspace.ConnectionProperties, 0);
                          string connectionFile = temporaryDirectory + "\\inputConnection.sde";
                          //  Let the user know which locator is being rebuilt.
                          if (ProcessMessage != null)
                          {
                            ProcessMessage("             < Copying the - " + tableSearchName + " Alternate Name Table for the " + currentLocatorName.Name + " locator from the Source Geodatabase...");
                          }
                          //  Establish a Table Copy Geoprocessing Object to Copy the Enterprise
                          //  Enterprise Geodatabase Alternate Name Table to the File Geodatabase.
                          geoProcessorObject = new ESRI.ArcGIS.Geoprocessing.GeoProcessor();
                          tableToTableParams = new ESRI.ArcGIS.esriSystem.VarArray();
                          tableToTableParams.Add(connectionFile + @"\" + inputTableDataset.Name);
                          tableToTableParams.Add(OutputFileGeodatabasePathName);
                          tableToTableParams.Add(tableSearchName);
                          //  Copy the Enterprise Geodatabase Alternate Name table to the File
                          //  Geodatabase.
                          geoprocessorResult = (ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult)geoProcessorObject.Execute("TableToTable_conversion", tableToTableParams, null);
                          //  Delete the Connection File since it is no longer needed.
                          if (System.IO.File.Exists(temporaryDirectory + "\\inputConnection.sde"))
                          {
                            //  Delete the file.
                            System.IO.File.Delete(temporaryDirectory + "\\inputConnection.sde");
                          }
                          if (geoProcessorObject != null)
                          {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(geoProcessorObject);
                          }
                          if (tableToTableParams != null)
                          {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(tableToTableParams);
                          }
                          if (geoprocessorResult != null)
                          {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(geoprocessorResult);
                          }
                          //  Add the current table to the list of Tables that have been copied.
                          copiedReferenceTables.Add(tableSearchName);
                        }
                      }
                    }
                  }
                  //  Get the next Reference Table from the enumerator.
                  currentLocatorReferenceTable = (ESRI.ArcGIS.Location.IReferenceDataTable2)locatorReferenceTablesEnum.Next();
                }
                //  Let the user know which locator is being rebuilt.
                if (ProcessMessage != null)
                {
                  ProcessMessage("             < Rebuilding the - " + currentLocatorName.Name + " locator...");
                }
                //  Build the Parameter Set necessary to Rebuild the Locator.
                rebuildLocatorParams = new ESRI.ArcGIS.esriSystem.VarArray();
                rebuildLocatorParams.Add(OutputFileGeodatabasePathName + "\\" + currentLocatorName.Name);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(currentLocatorName);
                if (currentLocatorReferenceTable != null)
                {
                  System.Runtime.InteropServices.Marshal.ReleaseComObject(currentLocatorReferenceTable);
                }
                if (currentLocator != null)
                {
                  System.Runtime.InteropServices.Marshal.ReleaseComObject(currentLocator);
                }
                //  Attempt to rebuild the Locator.
                rebuildGeoprocessorObject = new ESRI.ArcGIS.Geoprocessing.GeoProcessor();
                rebuildGeoprocessorResult = (ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult)rebuildGeoprocessorObject.Execute("RebuildAddressLocator_geocoding", rebuildLocatorParams, null);
                //  Present the results of the process to the user.
                if (rebuildGeoprocessorResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                  //  Let the user know that the Locator was rebuilt successfully.
                  if (ProcessMessage != null)
                  {
                    ProcessMessage("               + Successfully rebuilt the - " + currentLocatorName.Name + " locator...");
                    if (rebuildGeoprocessorObject.MessageCount > 0)
                    {
                      ProcessMessage("               + The messages from the process were -");
                      for (int i = 0; i <= rebuildGeoprocessorObject.MessageCount - 1; i++)
                      {
                        ProcessMessage("                  - " + rebuildGeoprocessorObject.GetMessage(i));
                      }
                    }
                  }
                }
                else
                {
                  //  Let the user know that the Locator Failed to Rebuild.
                  if (ProcessMessage != null)
                  {
                    ProcessMessage("");
                    ProcessMessage("FAILED to rebuild the - " + currentLocatorName.Name + " locator...");
                    if (rebuildGeoprocessorObject.MessageCount > 0)
                    {
                      ProcessMessage("   + The messages from the process were -");
                      for (int i = 0; i <= rebuildGeoprocessorObject.MessageCount - 1; i++)
                      {
                        ProcessMessage("      - " + rebuildGeoprocessorObject.GetMessage(i));
                      }
                    }
                  }
                  //  Return FALSE to the calling method to indicate that this process failed.
                  return false;
                }
              }
            }
            else
            {
              //  Retrieve the list of locators that exist in the Composite Locator.
              ESRI.ArcGIS.Location.ICompositeLocator currentCompositeLocator = (ESRI.ArcGIS.Location.ICompositeLocator)currentLocator;
              string[] locatorNames = currentCompositeLocator.LocatorNames as string[];
              ESRI.ArcGIS.Geodatabase.ILocator[] locators = new ESRI.ArcGIS.Geodatabase.ILocator[locatorNames.Length];
              int i = 0;
              foreach (string currentSubLocatorName in locatorNames)
              {
                locators[i] = currentCompositeLocator.get_Locator(currentSubLocatorName);
                i++;
              }
              //  Determine if the Updated Feature Class participates in any of the locators in the Composite Locator.
              bool foundIt = false;
              foreach (ESRI.ArcGIS.Geodatabase.ILocator currentSubLocator in locators)
              {
                //  If the Current Locator is a Composite Locator, ignore it.
                if (!(currentSubLocator is ESRI.ArcGIS.Location.ICompositeLocator))
                {
                  //  Determine if the specified Feature Class is a reference Dataset for the locator.
                  locatorReferenceTables = (ESRI.ArcGIS.Location.IReferenceDataTables)currentSubLocator;
                  locatorReferenceTablesEnum = locatorReferenceTables.Tables;
                  locatorReferenceTablesEnum.Reset();
                  //  Retrieve the First Table from the Locator.
                  currentLocatorReferenceTable = (ESRI.ArcGIS.Location.IReferenceDataTable2)locatorReferenceTablesEnum.Next();
                  //  Go through the Primary Tables participating in the Locator and determine if the Updated Feature Class Table is one of them.
                  while ((currentLocatorReferenceTable != null) && (!foundIt))
                  {
                    //  Determine if the current table is the specified Feature Class Business Table.
                    if (currentLocatorReferenceTable.DisplayName.ToUpper() == "PRIMARY TABLE")
                    {
                      ESRI.ArcGIS.Geodatabase.IDatasetName currentFeatureClassName = null;
                      currentFeatureClassName = (ESRI.ArcGIS.Geodatabase.IDatasetName)currentLocatorReferenceTable.Name;
                      if (currentFeatureClassName.Name.ToString().ToUpper() == FeatureClassName.ToUpper())
                      {
                        //  Set the found the locator indicator to TRUE.
                        foundIt = true;
                      }
                    }
                    //  Retrieve the Next Table from the Locator.
                    currentLocatorReferenceTable = (ESRI.ArcGIS.Location.IReferenceDataTable2)locatorReferenceTablesEnum.Next();
                  }
                }
              }
              //  If the Updated Feature Class is a member of the Composite Locator, rebuild it.
              if (foundIt)
              {
                //  Let the user know which locator is being rebuilt.
                if (ProcessMessage != null)
                {
                  ProcessMessage("             < Rebuilding the - " + currentLocatorName.Name + " locator...");
                }
                //  Build the Parameter Set necessary to Rebuild the Locator.
                rebuildLocatorParams = new ESRI.ArcGIS.esriSystem.VarArray();
                rebuildLocatorParams.Add(OutputFileGeodatabasePathName + "\\" + currentLocatorName.Name);
                //  Attempt to rebuild the Locator.
                rebuildGeoprocessorObject = new ESRI.ArcGIS.Geoprocessing.GeoProcessor();
                rebuildGeoprocessorResult = (ESRI.ArcGIS.Geoprocessing.IGeoProcessorResult)rebuildGeoprocessorObject.Execute("RebuildAddressLocator_geocoding", rebuildLocatorParams, null);
                //  Present the results of the process to the user.
                if (rebuildGeoprocessorResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                {
                  //  Let the user know that the Locator was rebuilt successfully.
                  if (ProcessMessage != null)
                  {
                    ProcessMessage("               + Successfully rebuilt the - " + currentLocatorName.Name + " locator...");
                    if (rebuildGeoprocessorObject.MessageCount > 0)
                    {
                      ProcessMessage("               + The messages from the process were -");
                      for (int y = 0; y <= rebuildGeoprocessorObject.MessageCount - 1; y++)
                      {
                        ProcessMessage("                  - " + rebuildGeoprocessorObject.GetMessage(y));
                      }
                    }
                  }
                }
                else
                {
                  //  Let the user know that the Locator Failed to Rebuild.
                  if (ProcessMessage != null)
                  {
                    ProcessMessage("");
                    ProcessMessage("FAILED to rebuild the - " + currentLocatorName.Name + " locator...");
                    if (rebuildGeoprocessorObject.MessageCount > 0)
                    {
                      ProcessMessage("   + The messages from the process were -");
                      for (int y = 0; y <= rebuildGeoprocessorObject.MessageCount - 1; y++)
                      {
                        ProcessMessage("      - " + rebuildGeoprocessorObject.GetMessage(y));
                      }
                    }
                  }
                  //  Return FALSE to the calling method to indicate that this process failed.
                  return false;
                }
              }
            }

          }

          //  Retrieve the next Locator Name Object from the Locator Names Enumerator.
          currentLocatorName = locatorNamesEnum.Next();

        }


        //  If the process made it to here, it was successful so return TRUE to the calling routine.
        return true;

      }
      catch (System.Runtime.InteropServices.COMException comCaught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comCaught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The FeatureClassUtilities.UpdateFileGeodatabaseLocators() Method failed with error message:  " + comCaught.Message + "(" + comCaught.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The FeatureClassUtilities.UpdateFileGeodatabaseLocators() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the Geoprocessor Result Object was instantiated, close it.
        if (geoprocessorResult != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(geoprocessorResult);
        }
        //  If the Table to Table Variant Array was instantiated, close it.
        if (tableToTableParams != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(tableToTableParams);
        }
        //  If the Geoprocessor Object was instantiated, close it.
        if (geoProcessorObject != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(geoProcessorObject);
        }
        //  If the InputWorkspace Factory Object was instantiated, close it.
        if (inputWorkspaceFactory != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(inputWorkspaceFactory);
        }
        //  If the Input Table Dataset Object was instantiated, close it.
        if (inputTableDataset != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(inputTableDataset);
        }
        //  If the Input Alternate Name Table Object was instantiated, close it.
        if (inputAlternateNameTable != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(inputAlternateNameTable);
        }
        //  If the Input Feature Workspace Object was instantiated, close it.
        if (inputFeatureWorkspace != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(inputFeatureWorkspace);
        }
        //  If the Delete Dataset Object was instantiated, close it.
        if (deleteDataset != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteDataset);
        }
        //  If the Delete Table Object was instantiated, close it.
        if (deleteTable != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(deleteTable);
        }
        //  If the Geodatabase Feature Workspace Object was instantiated, close it.
        if (geodatabaseFeatureWorkspace != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(geodatabaseFeatureWorkspace);
        }
        //  If the Current Locator Reference Table Object was instantiated, close it.
        if (currentLocatorReferenceTable != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(currentLocatorReferenceTable);
        }
        //  If the Locator Reference Tables Enumerator was instantiated, close it.
        if (locatorReferenceTablesEnum != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(locatorReferenceTablesEnum);
        }
        //  If the Locator Reference Tables Object was instantiated, close it.
        if (locatorReferenceTables != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(locatorReferenceTables);
        }
        //  If the Current Locator Object was instantiated, close it.
        if (currentLocator != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(currentLocator);
        }
        //  If the Current Locator Name Object was instantiated, close it.
        if (currentLocatorName != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(currentLocatorName);
        }
        //  If the Locatore Names Enumerator Object was instantiated, close it.
        if (locatorNamesEnum != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(locatorNamesEnum);
        }
        //  If the Locator Workspace Object was instantiated, close it.
        if (locatorWorkspace != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(locatorWorkspace);
        }
        //  If the Locator Manager Object was instantiated, close it.
        if (locatorManager != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(locatorManager);
        }
        //  If the Geodatabase Workspace Object was instantiated, close it.
        if (geodatabaseWorkspace != null)
        {
          System.Runtime.InteropServices.Marshal.ReleaseComObject(geodatabaseWorkspace);
        }

      }

    }   //  UpdateFileGeodatabaseLocators()
    #endregion Locator Management


    #region General Feature Class Utilities
    public bool? EmptyFeatureClass(ESRI.ArcGIS.Geodatabase.IFeatureClass FeatureClass)
    {
      ESRI.ArcGIS.Geodatabase.ITable featureClassTable = null;

      try
      {
        //  Determine the number of features in the Feature Class.
        int startingFeatureCount = FeatureClass.FeatureCount(null);


        //  QI to the Feature Class Table and delete the records from it.
        featureClassTable = (ESRI.ArcGIS.Geodatabase.ITable)FeatureClass;
        featureClassTable.DeleteSearchedRows(null);

        //  Make sure all of the records were deleted.
        if (featureClassTable.RowCount(null) != 0)
        {
          //  Let the user know that the records could not be deleted.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not delete the data from the Feature class!");
          }

          //  Return FALSE to the calling method to indicate that the delete failed.
          return false;

        }


        //  Let the User know that the records were deleted.
        if (ProcessMessage != null)
        {
          ProcessMessage("      - Deleted " + startingFeatureCount.ToString() + " features from the Feature Class...");

        }


        //  If the process made it to here, it was successful so return TRUE to the calling method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.CompressFileGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return NULL to the calling routine to indicate that this process failed.
        return null;

      }
      finally
      {
        //  If the Feature Class Table Object was Instantiated, close it.
        if (featureClassTable != null)
        {
          featureClassTable = null;
        }

      }

    }


    /// <summary>
    /// Deletes all features from the specified feature class.
    /// </summary>
    /// <param name="DeleteFeatureClass">
    /// An Object Pointer to the Feature Class that is to be emptied.
    /// </param>
    /// <returns>
    /// TRUE if the features were successfully deleted.
    /// FALSE if the process failed to delete the features from the dataset.
    /// </returns>
    public bool EmptyGeodatabaseFeatureClass(ESRI.ArcGIS.Geodatabase.IFeatureClass DeleteFeatureClass)
    {
      PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities generalUtilities         = null;
      ESRI.ArcGIS.esriSystem.IVariantArray                geoprocessorVariantArray = null;
      ESRI.ArcGIS.Geoprocessing.IGeoProcessor             geoProcessor             = null;

      try
      {
        //  Determine the ArcGIS Install Path so that the Data Management Toolbox can be opened to be used.
        generalUtilities = new PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities();
        string installPath = generalUtilities.DetermineArcGISDesktopInstallPath();

        //  Make sure the Install Path was determined successfully before moving on.
        if (System.String.IsNullOrEmpty(installPath))
        {
          //  Let the user know that the ArcGIS Desktop Install Path could not be determined.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not Determine the ArcGIS Desktop Install Path to Initialize the Data Management Toolbox.  The MaintTools.FeatureClassUtilities.EmptyGeodatabaseFeatureClass() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Build the Variant Array that will be used to pass the parameters necessary for the delete to the toolbox object.
        geoprocessorVariantArray = new ESRI.ArcGIS.esriSystem.VarArrayClass();
        geoprocessorVariantArray.Add(DeleteFeatureClass);


        //  Instantiate the Geoprocessing Object that will be used to delete the features from the feature class.
        geoProcessor = new ESRI.ArcGIS.Geoprocessing.GeoProcessorClass();
        geoProcessor.AddToolbox(System.IO.Path.Combine(installPath, @"ArcToolbox\Toolboxes\Data Management Tools.tbx"));

        //  Perform the Export in a TRY Block so that any COM or IO Errors can be identified and handled.
        try
        {
          //  Perform the export.
          geoProcessor.Execute("DeleteFeatures_management", geoprocessorVariantArray, null);
          //  Write the messages from the Delete Features tool log file.
          int toolMessageCount = geoProcessor.MessageCount;
          int currentToolMessageIndex = 0;
          if (ProcessMessage != null)
          {
            ProcessMessage("         - Delete Features Operation Messages...");
          }
          while (currentToolMessageIndex < toolMessageCount)
          {
            //  Write the current message to the log file.
            if (ProcessMessage != null)
            {
              ProcessMessage("           + " + geoProcessor.GetMessage(currentToolMessageIndex));
            }
            //  Increment the Tool Message Index Counter.
            currentToolMessageIndex++;
          }

        }
        catch (System.IO.IOException ioException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Delete Features Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Delete Features Operation in the MaintTools.FeatureClassUtilities.EmptyGeodatabaseFeatureClass() Method Failed with error message - " + ioException.Message + " (" + ioException.Source + " Line:  " + lineNumber.ToString() + ")!");
          }
          int toolMessageCount = geoProcessor.MessageCount;
          int currentToolMessageIndex = 0;
          if (ProcessMessage != null)
          {
            ProcessMessage("The information from the Geoprocessor is:");
          }
          while (currentToolMessageIndex < toolMessageCount)
          {
            //  Write the current message to the log file.
            if (ProcessMessage != null)
            {
              ProcessMessage("   + " + geoProcessor.GetMessage(currentToolMessageIndex));
            }
            //  Increment to Toold Message Index Counter.
            currentToolMessageIndex++;
          }
          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;
        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Delete Features Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Delete Features Operation in the MaintTools.FeatureClassUtilities.EmptyGeodatabaseFeatureClass() Method Failed with error message - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
          }
          int toolMessageCount = geoProcessor.MessageCount;
          int currentToolMessageIndex = 0;
          if (ProcessMessage != null)
          {
            ProcessMessage("The information from the Geoprocessor is:");
          }
          while (currentToolMessageIndex < toolMessageCount)
          {
            //  Write the current message to the log file.
            if (ProcessMessage != null)
            {
              ProcessMessage("   + " + geoProcessor.GetMessage(currentToolMessageIndex));
            }
            //  Increment to Toold Message Index Counter.
            currentToolMessageIndex++;
          }
          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;
        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.EmptyGeodatabaseFeatureClass() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the Geoprocessor Object was instantiated, close it.
        if (geoProcessor != null)
        {
          geoProcessor = null;
        }
        //  If the Geoprocessor Variant Array Object was instantiated, close it.
        if (geoprocessorVariantArray != null)
        {
          geoprocessorVariantArray = null;
        }
        //  If the BTS General Utilities Object was instantiated, close it.
        if (generalUtilities != null)
        {
          generalUtilities = null;
        }

      }

      //  If the process made it to here, it was successful so return TRUE to the calling method.
      return true;

    }   //  EmptyFileGeodatabaseFeatureClass()


    /// <summary>
    /// Indexes the specified Field in the Feature Class.
    /// </summary>
    /// <param name="IndexFeatureClass">
    /// An ESRI Geodatabase Feature Class Object for the Feature Class that contains the Field that is to be indexed.
    /// </param>
    /// <param name="IndexFieldName">
    /// The Name of the Field in the Class that is to be indexed.
    /// </param>
    /// <returns>
    /// TRUE if the Field was successfully indexed.
    /// FALSE if the Field was not successfully indexed.
    /// </returns>
    public bool IndexedFieldInFeatureClass(ESRI.ArcGIS.Geodatabase.IFeatureClass IndexFeatureClass, string IndexFieldName)
    {
      PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities generalUtilities         = null;
      ESRI.ArcGIS.Geodatabase.ITable                      inputTable               = null;
      ESRI.ArcGIS.esriSystem.VarArray                     geoprocessorVariantArray = null;
      ESRI.ArcGIS.Geoprocessing.IGeoProcessor             geoprocessor             = null;

      try
      {
         //  If the specified Field exists in the Feature Class, index it.
        if (IndexFeatureClass.Fields.FindField(IndexFieldName) != -1)
        {
          //  Instantiate the CGIS General Utilities Object that will be used to determine the ArcGIS Install Path on the server.
          generalUtilities = new PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities();

          //  Determine the ArcGIS Install Path so that the Projector Toolbox can be opened to be used.
          generalUtilities = new PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities();
          string installPath = generalUtilities.DetermineArcGISDesktopInstallPath();
          //  Make sure the Install Path was determined successfully before moving on.
          if (System.String.IsNullOrEmpty(installPath))
          {
            //  Let the user know that the ArcGIS Desktop Install Path could not be determined.
            if (ErrorMessage != null)
            {
              ErrorMessage("Could not Determine the ArcGIS Desktop Install Path to Initialize the Projection Toolbox.  The MaintTools.FeatureClassUtilities.IndexedFieldInFeatureClass() Method failed!");
            }
            //  Return FALSE to the calling method to indicate that this method failed.
            return false;
          }

          //QI to the Input Table.
          inputTable = (ESRI.ArcGIS.Geodatabase.ITable)IndexFeatureClass;

          //  Build the Variant Array that will hold the parameters to be passed to the Geoprocessing Tool.
          geoprocessorVariantArray = new ESRI.ArcGIS.esriSystem.VarArrayClass();
          geoprocessorVariantArray.Add(inputTable);
          geoprocessorVariantArray.Add(IndexFieldName);
          //geoprocessorVariantArray.Add(indexFeatureClassIndexField);
          geoprocessorVariantArray.Add(IndexFieldName + @"_idx");
          geoprocessorVariantArray.Add("NON_UNIQUE");
          geoprocessorVariantArray.Add("NON_ASCENDING");

          //  Instantiate the Geoprocessing Object that will be used to Index the Specified Field in the Feature Class.
          geoprocessor = new ESRI.ArcGIS.Geoprocessing.GeoProcessorClass();
          geoprocessor.AddToolbox(System.IO.Path.Combine(installPath, @"ArcToolbox\Toolboxes\Data Management Tools.tbx"));

          //  Perform the Index Field Operation in a TRY Block so that any COM or IO Errors can be identified and handled.
          try
          {
            //  Perform the Add Index Operation.
            geoprocessor.Execute("AddIndex_Management", geoprocessorVariantArray, null);
            //  Write the messages from the Add Index tool log file.
            int toolMessageCount = geoprocessor.MessageCount;
            int currentToolMessageIndex = 0;
            if (ProcessMessage != null)
            {
              ProcessMessage("         - Add Index Operation Messages...");
            }
            while (currentToolMessageIndex < toolMessageCount)
            {
              //  Write the current message to the log file.
              if (ProcessMessage != null)
              {
                ProcessMessage("           + " + geoprocessor.GetMessage(currentToolMessageIndex));
              }
              //  Increment the Tool Message Index Counter.
              currentToolMessageIndex++;
            }
          }
          catch (System.IO.IOException ioException)
          {
            //  Determine the Line Number from which the exception was thrown.
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            int lineNumber = stackFrame.GetFileLineNumber();

            //  Let the User know that the Add Index Operation Failed.
            if (ErrorMessage != null)
            {
              ErrorMessage("The Add Attribute Index Operation in the MaintTools.FeatureClassUtilities.IndexedFieldInFeatureClass() Method Failed with error message - " + ioException.Message + " (" + ioException.Source + " Line:  " + lineNumber.ToString() + ")!");
            }
            int toolMessageCount = geoprocessor.MessageCount;
            int currentToolMessageIndex = 0;
            if (ProcessMessage != null)
            {
              ProcessMessage("The information from the Geoprocessor is:");
            }
            while (currentToolMessageIndex < toolMessageCount)
            {
              //  Write the current message to the log file.
              if (ProcessMessage != null)
              {
                ProcessMessage("   + " + geoprocessor.GetMessage(currentToolMessageIndex));
              }
              //  Increment to Tool Message Index Counter.
              currentToolMessageIndex++;
            }
            //  Return FALSE to the calling routine ito indicate that this process failed.
            return false;
          }
          catch (System.Runtime.InteropServices.COMException comException)
          {
            //  Determine the Line Number from which the exception was thrown.
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            int lineNumber = stackFrame.GetFileLineNumber();

            //  Let the User know that the Dissolve Operation Failed.
            if (ErrorMessage != null)
            {
              ErrorMessage("The Add Attribute Index Operation in the MaintTools.FeatureClassUtilities.IndexedFieldInFeatureClass() Method Failed with error message - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
            }
            int toolMessageCount = geoprocessor.MessageCount;
            int currentToolMessageIndex = 0;
            if (ProcessMessage != null)
            {
              ProcessMessage("The information from the Geoprocessor is:");
            }
            while (currentToolMessageIndex < toolMessageCount)
            {
              //  Write the current message to the log file.
              if (ProcessMessage != null)
              {
                ProcessMessage("   + " + geoprocessor.GetMessage(currentToolMessageIndex));
              }
              //  Increment to Tool Message Index Counter.
              currentToolMessageIndex++;
            }
            //  Return FALSE to the calling routine ito indicate that this process failed.
            return false;
          }

        }
        else
        {
          //  Let the user know that the Field does not exist in the Feature Class and will not be indexed.
          if (ProcessMessage != null)
          {
            ProcessMessage("               -  The field - " + IndexFieldName.ToUpper() + " could not be found in the " + IndexFeatureClass.AliasName + " Feature Class and will not be indexed...");
          }

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.IndexedFieldInFeatureClass() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }
      finally
      {
        //  If the Geoprocessor Object was instantiated, close it.
        if (geoprocessor != null)
        {
          geoprocessor = null;
        }
        //  If the Geoprocessor Variant Array Object was instantiated, close it.
        if (geoprocessorVariantArray != null)
        {
          geoprocessorVariantArray = null;
        }
        //  If the Input Table Object was instantiated, close it.
        if (inputTable != null)
        {
          inputTable = null;
        }
        //  If the CGIS Data Maintenance General Utilities Object was instantiated, close it.
        if (generalUtilities != null)
        {
          generalUtilities = null;
        }

      }

      //  If the process made it to here, it was successful so return a "TRUE" to the calling method.
      return true;

    }   //  IndexedFieldInFeatureClass()


    public bool UpdateFieldAliasesFromSourceDataset(ESRI.ArcGIS.esriSystem.IPropertySet SourcePropertySet, string SourceFeatureClassName, ESRI.ArcGIS.esriSystem.IPropertySet DestinationPropertySet, string DestinationFeatureClassName)
    {
      ESRI.ArcGIS.Geodatabase.IFeatureClass sourceFeatureClass      = null;
      ESRI.ArcGIS.Geodatabase.IFeatureClass destinationFeatureClass = null;

      try
      {
        //  Open the Source Feature Class.
        sourceFeatureClass = OpenFeatureClass(SourcePropertySet, SourceFeatureClassName);

        //  Make sure the Source Feature Class was opened successfully.
        if (sourceFeatureClass == null)
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Specified Source Feature Class could not be opened.  Aborting the MaintTools.FeatureClassUtilities.UpdateFieldAliasesFromSourceDataset() Method!");
          }

          //  Return FALSE to the calling Method to indicate that this Method failed.
          return false;

        }


        //  Open the Destination Feature Class.
        destinationFeatureClass = OpenFeatureClass(DestinationPropertySet, DestinationFeatureClassName);

        //  Make sure the Destination Feature Class was opened successfully.
        if (destinationFeatureClass == null)
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Specified Destination Feature Class could not be opened.  Aborting the MaintTools.FeatureClassUtilities.UpdateFieldAliasesFromSourceDataset() Method!");
          }

          //  Return FALSE to the calling Method to indicate that this Method failed.
          return false;

        }


        //  Update the Field Aliases.
        return UpdateFieldAliasesFromSourceDataset(sourceFeatureClass, destinationFeatureClass);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.UpdateFieldAliasesFromSourceDataset() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }
      finally
      {
        //  If the ESRI Geodatabase Destination Feature Class Object was instantiated, close it.
        if (destinationFeatureClass != null)
        {
          destinationFeatureClass = null;
        }
        //  If the ESRI Geodatabase Source Feature Class Object was instantiated, close it.
        if (sourceFeatureClass != null)
        {
          sourceFeatureClass = null;
        }

      }

    }   //  UpdateFieldAliasesFromSourceDataset()


    public bool UpdateFieldAliasesFromSourceDataset(ESRI.ArcGIS.Geodatabase.IFeatureClass SourceFeatureClass, ESRI.ArcGIS.Geodatabase.IFeatureClass DestinationFeatureClass)
    {
      ESRI.ArcGIS.Geodatabase.IFields    sourceFCFields              = null;
      ESRI.ArcGIS.Geodatabase.IFields    destinationFCFields         = null;
      ESRI.ArcGIS.Geodatabase.IField     currentDestinationField     = null;
      ESRI.ArcGIS.Geodatabase.IFieldEdit currentDestinationEditField = null;
      
      try
      {
        //  Retrieve the List of Fields from the Source Dataset.
        ESRI.ArcGIS.Geodatabase.ITable sourceFCTable = (ESRI.ArcGIS.Geodatabase.ITable)SourceFeatureClass;
        //sourceFCFields = SourceFeatureClass.Fields;
        sourceFCFields = sourceFCTable.Fields;


        //  Retrieve the Fields from the Destination Feature Class.
        destinationFCFields = DestinationFeatureClass.Fields;


        //  Go through the Destination Fields and update their Alias Values.
        for (int i = 0; i < destinationFCFields.FieldCount; i++)
        {
          //  Retrieve the Current Field from the Destination Fields Collection.
          currentDestinationField = destinationFCFields.get_Field(i);

          //  If the Field is not a Shape or Object ID Field, attempt to update its alias.
          if ((currentDestinationField.Type != ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeOID) &&
              (currentDestinationField.Type != ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeBlob) &&
              (System.String.Compare(currentDestinationField.Name, DestinationFeatureClass.ShapeFieldName, System.StringComparison.CurrentCultureIgnoreCase) != 0))
          {
            //  If a field with the same name exists in the Source Dataset attempt to retrieve the Alias from it an apply it to the field
            //  in the destination field.
            if (sourceFCFields.FindField(currentDestinationField.Name) != -1)
            {
              //  If the Source Field has an alias value, apply it to the Destination Field.
              if (!System.String.IsNullOrEmpty(sourceFCFields.get_Field(sourceFCFields.FindField(currentDestinationField.Name)).AliasName))
              {
                //  Apply the alias to the Destination Field.
                currentDestinationEditField = (ESRI.ArcGIS.Geodatabase.IFieldEdit)currentDestinationField;
                currentDestinationEditField.AliasName_2 = (string)sourceFCFields.get_Field(sourceFCFields.FindField(currentDestinationField.Name)).AliasName;
              }
            }
          }
        }
      
      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.UpdateFieldAliasesFromSourceDataset() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }
      finally
      {
        //  If the ESRI Geodatabase Current Destination Edit Field Object has been instantiated, close it.
        if (currentDestinationEditField != null)
        {
          currentDestinationEditField = null;
        }
        //  If the ESRI Geodatabae Current Destination Field Object has been instantiated, close it.
        if (currentDestinationField != null)
        {
          currentDestinationField = null;
        }
        //  If the ESRI Geodatabase Destination Fields Collection Object has been instantiated, close it.
        if (destinationFCFields != null)
        {
          destinationFCFields = null;
        }
        //  If the ESRI Geodatabase Source Fields Collection Object has been instantiated, close it.
        if (sourceFCFields != null)
        {
          sourceFCFields = null;
        }

      }

      //  If the process got to this point it was successful so return TRUE to the calling method.
      return true;

    }   //  UpdateFieldAliasesFromSourceDataset()


    public ESRI.ArcGIS.Geodatabase.IFeatureClass OpenShapefile(string ShapefileDirectory, string ShapefileName)
    {
      System.Type                               shapefileFactoryType      = null;
      System.Object                             shapefileFactoryObject    = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceFactory shapefileWorkspaceFactory = null;
      ESRI.ArcGIS.Geodatabase.IWorkspace        shapefileWorkspace        = null;
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace shapefileFeatureWorkspace = null;

      try
      {
        //  Confirm that the specified Shapefile Directory exists.
        if (!System.IO.Directory.Exists(ShapefileDirectory))
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified Shapefile Directory does NOT exist. Aborting the MaintTools.FeatureClassUtilities.OpenShapefile() Method!");
          }

          //  Return a NULL Pointer to the calling Method to indicate that this Method failed.
          return null;

        }


        //  Make sure the specified Shapefile Name includes the ".shp" extension.
        string fullShapeName = null;
        if (ShapefileName.IndexOf(".shp", 0, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
        {
          //  Use the Shapefile Name as it was passed.
          fullShapeName = ShapefileName;
        }
        else
        {
          //  Add the extension to the passed Shapefile Name.
          fullShapeName = ShapefileName + ".shp";
        }


        //  Make sure the specified Shapefile exists.
        if (!System.IO.File.Exists(System.IO.Path.Combine(ShapefileDirectory, fullShapeName)))
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified Shapefile does NOT exist. Aborting the MaintTools.FeatureClassUtilities.OpenShapefile() Method!");
          }

          //  Return a NULL Pointer to the calling Method to indicate that this Method failed.
          return null;

        }


        //  Attempt to open the Shapefile Feature Class.
        shapefileFactoryType = System.Type.GetTypeFromProgID("esriDataSourcesFile.ShapefileWorkspaceFactory");
        shapefileFactoryObject = System.Activator.CreateInstance(shapefileFactoryType);
        shapefileWorkspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory)shapefileFactoryObject;
        shapefileWorkspace = shapefileWorkspaceFactory.OpenFromFile(ShapefileDirectory, 0);
        shapefileFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)shapefileWorkspace;


        //  Open the Feature Class and return a pointer to it to the calling method.
        return shapefileFeatureWorkspace.OpenFeatureClass(ShapefileName);

      }
      catch (System.Runtime.InteropServices.COMException comException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.OpenShapefile() Method failed while opening the Shapefile in the Specified Workspace with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Pointer to the calling method to indicate that this methdo failed.
        return null;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.OpenShapefile() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Pointer to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {
        //  If the Enterprise Geodatabase Workspace Object was instantiated, close it.
        if (shapefileWorkspace != null)
        {
          shapefileWorkspace = null;
        }
        //  If the Enterprise Geodatabase Workspace Factory Object was instantiated, close it.
        if (shapefileWorkspaceFactory != null)
        {
          shapefileWorkspaceFactory = null;
        }

      }

    }   //  OpenShapefile()


    public ESRI.ArcGIS.Geodatabase.IFeatureClass OpenFeatureClass(ESRI.ArcGIS.esriSystem.IPropertySet DatabasePropertySet, string FeatureclassName)
    {
      ESRI.ArcGIS.Geodatabase.IWorkspace        sourceWorkspace        = null;
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace sourceFeatureWorkspace = null;

      try
      {
        //  Establish a Connection to the Geodatabase that houses the Feature Class.
        int propertiesCount = DatabasePropertySet.Count;
        System.Object[] propNamesArray = new System.Object[1];
        System.Object[] propValuesArray = new System.Object[1];
        DatabasePropertySet.GetAllProperties(out propNamesArray[0], out propValuesArray[0]);
        System.Object[] propertyNames = (System.Object[])propNamesArray[0];
        System.Object[] propertyValues = (System.Object[])propValuesArray[0];
        bool itsEnterprise = false;
        for (int i = 0; i < propertiesCount; i++)
        {
          if (System.String.Compare("INSTANCE", propertyNames[i].ToString(), System.StringComparison.CurrentCultureIgnoreCase) == 0)
          {
            if (!System.String.IsNullOrEmpty(propertyValues[i].ToString()))
            {
              itsEnterprise = true;
            }
          }
        }
        if (itsEnterprise)
        {
          //  The Property Set is an Enterprise Geodatabase Property Set, so establish an Enterprise Geodatabase Connection.
          sourceWorkspace = EstablishEnterpriseGeoDBConnection(DatabasePropertySet);
        }
        else
        {
          //  The Property Set is a File Geodatabase Property Set, so establish a File Geodatabase Connection.
          sourceWorkspace = EstablishFileGeodatabaseConnection(DatabasePropertySet);
        }

        //  Make sure the Source Workspace was opened successfully.
        if (sourceWorkspace == null)
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not establish a connection to the Specified Source Geodatabase. Aborting the MaintTools.FeatureClassUtilities.OpenFeatureClass() Method!");
          }

          //  Return a NULL Pointer to the calling Method to indicate that this Method failed.
          return null;

        }


        //  Attempt to open the Feature Class.
        sourceFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)sourceWorkspace;
        return sourceFeatureWorkspace.OpenFeatureClass(FeatureclassName);

      }
      catch (System.Runtime.InteropServices.COMException comException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.OpenFeatureClass() Method failed while opening the Feature Class in the Geodatabase Workspace with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Pointer to the calling method to indicate that this methdo failed.
        return null;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.OpenFeatureClass() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a NULL Pointer to the calling Method to indicate that this Method failed.
        return null;

      }
      finally
      {
        //  If the ESRI Source Geodatabase Feature Workspace Object was instantiated, close it.
        if (sourceFeatureWorkspace != null)
        {
          sourceFeatureWorkspace = null;
        }
        //  If the ESRI Source Goedatabase Workspace Object was instantiated, close it.
        if (sourceWorkspace != null)
        {
          sourceWorkspace = null;
        }

      }

    }   //  OpenFeatureClass()


    public bool CreateFeatureClass(ESRI.ArcGIS.Geodatabase.IWorkspace OutputWorkspace, string OutputFeatureClassName, ESRI.ArcGIS.Geodatabase.IFields OutputFeatureClassFields, ESRI.ArcGIS.Geodatabase.esriFeatureType OutputFeatureType)
    {
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace outputFeatureWorkspace = null;
      ESRI.ArcGIS.Geodatabase.IFeatureClass     newFeatureClass        = null;

      try
      {
        //  QI to the Output Feature Workspace.
        outputFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)OutputWorkspace;


        //  Determine the Name of the Shape Field in the Output Fields Collection.
        string shapeFieldName = null;
        for (int i = 0; i < OutputFeatureClassFields.FieldCount; i++)
        {
          //  Grab the Current Field.
          if (OutputFeatureClassFields.get_Field(i).Type == ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeGeometry)
          {
            //  Set the name of the current Field to be the "Shape Field Name".
            shapeFieldName = OutputFeatureClassFields.get_Field(i).Name;
          }

        }


        //  If the Shape Field was not found, abort the process.
        if (System.String.IsNullOrEmpty(shapeFieldName))
        {
          //  Let the User know that this method failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Output Fields Collection does not include a Shape Field. Aborting the MaintTools.FeatureClassUtilities.CreateFeatureClass() Method!");
          }

          //  Return FALSE to the calling Method to indicate that this Method failed.
          return false;

        }


        //  Attempt to Build the Feature Class.
        newFeatureClass = outputFeatureWorkspace.CreateFeatureClass(OutputFeatureClassName, OutputFeatureClassFields, null, null, OutputFeatureType, shapeFieldName, "");

        //  Make sure the Feature Class was created successfully.
        if (newFeatureClass != null)
        {
          //  The Feature Class was created successfully so return TRUE to the calling method.
          return true;

        }
        else
        {
          //  The Feature Class was not created successfully so retrun FALSE to the calling method.
          return false;

        }

      }
      catch (System.Runtime.InteropServices.COMException comException)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.CreateFeatureClass() Method failed while opening the Feature Class in the Geodatabase Workspace with COM Exception - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling method to indicate that this methdo failed.
        return false;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.CreateFeatureClass() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling Method to indicate that this Method failed.
        return false;

      }
      finally
      {
        //  If the Output ESRI Geodatabase Feature Workspace Object was instantiated, close it.
        if (outputFeatureWorkspace != null)
        {
          outputFeatureWorkspace = null;
        }

      }

    }   //  CreateFeatureClass()
    #endregion General Feature Class Utilities


    #region Update Dates
    public System.DateTime GetShapefileLastUpdateDate(string ShapefilePath)
    {
      try
      {
        //  Set the Path to be used to determine the last write time for all of the Shapefile files.
        string shapefileTestPath = ShapefilePath;
        if (ShapefilePath.IndexOf(".shp", System.StringComparison.CurrentCultureIgnoreCase) != -1)
        {
          //  Strip the extension from the file path.
          shapefileTestPath = shapefileTestPath.Substring(0, ShapefilePath.IndexOf(".shp", System.StringComparison.CurrentCultureIgnoreCase));
        }
        else
        {
          //  Use the path as it was passed to this method.
          shapefileTestPath = ShapefilePath;
        }


        //  Go through all of the possible Files in the Shapefile and determine the last write date of the most recently written file.
        System.DateTime lastUpdateDate = new System.DateTime(1900, 1, 1);
        foreach (string currentExtension in _shapefileExtensions)
        {
          //  If the file exists, determine if it is the most recently written file.
          if (System.IO.File.Exists(shapefileTestPath + "." + currentExtension))
          {
            //  Determine if this file was updated after the newest file that has been found so far.
            if (System.IO.File.GetLastWriteTime(shapefileTestPath + "." + currentExtension) > lastUpdateDate)
            {
              //  Set the Last Update Date Value to Last Write Time of the current file.
              lastUpdateDate = System.IO.File.GetLastWriteTime(shapefileTestPath + "." + currentExtension);
            }
          }
        }


        //  Return the Last Update Date of the Shapefile to the calling method.
        return lastUpdateDate;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("");
          ErrorMessage("");
          ErrorMessage("The Maintools.FeatureClassUtilities.GetShapefileLastUpdateDate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
        return new System.DateTime(1900, 1, 1);

      }

    }   //  GetShapefileLastUpdateDate


    public System.DateTime GetPersonalGeoDBLastUpdateDate(string PersonalGeodatabasePath)
    {
      try
      {
        //  If the Personal Geodatabase does not exist, return "1/1/1900" as the update date.
        if (!System.IO.File.Exists(PersonalGeodatabasePath))
        {
          return new System.DateTime(1900, 1, 1);
        }


        //  Determine the Last Write Time of the Personal Geodatabase file on the File System.
        return System.IO.File.GetLastWriteTime(PersonalGeodatabasePath);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.FeatureClassUtilities.GetPersonalGeoDBLastUpdateDate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
        return new System.DateTime(1900, 1, 1);

      }

    }   //  GetPersonalGeoDBLastUpdateDate


    public System.DateTime GetFileGeoDBLastUpdateDate(string FileGeoDBPath)
    {
      try
      {
        //  Call the newly named overload of this method.
        return GetFileGeodatabaseLastUpdate(FileGeoDBPath);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.FeatureClassUtilities.GetFileGeoDBLastUpdateDate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
        return new System.DateTime(1900, 1, 1);

      }

    }   //  GetFileGeoDBLastUpdateDate()


    public System.DateTime GetFileGeodatabaseLastUpdate(string FileGeoDatabasePath)
    {
      try
      {
        //  If the File Geodatabase does not exist, return "1/1/1900" as the update date.
        if (!System.IO.Directory.Exists(FileGeoDatabasePath))
        {
          return new System.DateTime(1900, 1, 1);
        }


        //  Go through the files in the File Geodatabase and determine the Last Update date for 
        //  the newest (non-lock) file in the File Geodatabase.
        string[] fileList = System.IO.Directory.GetFiles(FileGeoDatabasePath);
        System.DateTime lastUpdateDate = new System.DateTime(1900, 1, 1);
        foreach (string currentFile in fileList)
        {
          //  Make sure this is not a "Lock" file.
          if (currentFile.ToUpper().IndexOf(".LOCK") == -1)
          {
            //  Determine if this file was updated after the newest file that has been found so far.
            if (System.IO.File.GetLastWriteTime(currentFile) > lastUpdateDate)
            {
              //  Set the Last Update Date Value to Last Write Time of the current file.
              lastUpdateDate = System.IO.File.GetLastWriteTime(currentFile);
            }
          }
        }


        //  Return the determined Last Update Date to the calling method.
        return lastUpdateDate;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.FeatureClassUtilities.GetFileGeoDBLastUpdateDate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
        return new System.DateTime(1900, 1, 1);

      }

    }   //  GetFileGeoDBLastUpdateDate


    public System.DateTime GetFileGeodatabaseFeatureClassLastUpdate(string FileGeoDBPath, string FeatureClassName)
    {
      ESRI.ArcGIS.esriSystem.IPropertySet fileGeodatabasePropertySet = null;
      try
      {
        //  Build a Property Set for the File Geodatabase.
        fileGeodatabasePropertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
        fileGeodatabasePropertySet.SetProperty("DATABASE", FileGeoDBPath);


        //  Get the Last Modified Date of the File Geodatabase Feature Class.
        return GetFileGeodatabaseFeatureClassLastUpdate(fileGeodatabasePropertySet, FeatureClassName);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.FeatureClassUtilities.GetFileGeodatabaseFeatureClassLastUpdate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
        return new System.DateTime(1900, 1, 1);

      }
      finally
      {
        //  If the ESRI System File Geodatabase Property Set Object has been instantiated, close it.
        if (fileGeodatabasePropertySet == null)
        {
          fileGeodatabasePropertySet = null;
        }

      }

    }   //  GetFileGeodatabaseFeatureClassLastUpdate


    public System.DateTime GetFileGeodatabaseFeatureClassLastUpdate(ESRI.ArcGIS.esriSystem.IPropertySet FileGeodatabasePropertySet, string FeatureClassName)
    {
      ESRI.ArcGIS.Geodatabase.IWorkspace        fileGeodatabaseWorkspace        = null;
      ESRI.ArcGIS.Geodatabase.IFeatureWorkspace fileGeodatabaseFeatureWorkspace = null;
      ESRI.ArcGIS.Geodatabase.ITable            fileGeodatabaseTable            = null;
      ESRI.ArcGIS.Geodatabase.IDatasetFileStat2 fileGeodatabaseFileStat         = null;

      try
      {
        //  Attach to the File Geodatabase.
        fileGeodatabaseWorkspace = EstablishFileGeodatabaseConnection(FileGeodatabasePropertySet);

        //  Make sure the connection was established successfully.
        if (fileGeodatabaseWorkspace == null)
        {
          //  Let the user know that this process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Maintools.FeatureClassUtilities.GetFileGeodatabaseFeatureClassLastUpdate() Method failed to open the File Geodatabase Workspace!");
          }

          //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
          return new System.DateTime(1900, 1, 1);

        }


        //  Open the File Geodatabase Feature Class.
        fileGeodatabaseFeatureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)fileGeodatabaseWorkspace;
        fileGeodatabaseTable = fileGeodatabaseFeatureWorkspace.OpenTable(FeatureClassName);

        //  Make sure the Feature Class was opened successfully.
        if (fileGeodatabaseTable == null)
        {
          //  Let the user know that this process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Maintools.FeatureClassUtilities.GetFileGeodatabaseFeatureClassLastUpdate() Method failed to open the File Geodatabase Feature Class!");
          }

          //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
          return new System.DateTime(1900, 1, 1);

        }


        //  Determine the Last Modified Date of the Feature Class.
        fileGeodatabaseFileStat = (ESRI.ArcGIS.Geodatabase.IDatasetFileStat2)fileGeodatabaseTable;
        int modifiedNumOfSeconds = fileGeodatabaseFileStat.get_StatTime(ESRI.ArcGIS.Geodatabase.esriDatasetFileStatTimeMode.esriDatasetFileStatTimeLastModification);
        long win32FileTime = 10000000 * (long)modifiedNumOfSeconds + 116444736000000000;
        return DateTime.FromFileTimeUtc(win32FileTime);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The Maintools.FeatureClassUtilities.GetFileGeodatabaseFeatureClassLastUpdate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return January 1st, 1900 to the calling routine to indicate that this process failed.
        return new System.DateTime(1900, 1, 1);

      }
      finally
      {
        //  If the ESRI Geodatabase Dataset File Stat Object has been instantiated, close it.
        if (fileGeodatabaseFileStat != null)
        {
          fileGeodatabaseFileStat = null;
        }
        //  If the ESRI Geodatabase Table Object has been instantiated, close it.
        if (fileGeodatabaseTable != null)
        {
          fileGeodatabaseTable = null;
        }
        //  If the ESRI Geodatabase Feature Workspace Object has been instantiated, close it.
        if (fileGeodatabaseFeatureWorkspace == null)
        {
          fileGeodatabaseFeatureWorkspace = null;
        }
        //  If the ESRI Geodatabase Workspace Object has been instantiated, close it.
        if (fileGeodatabaseWorkspace == null)
        {
          fileGeodatabaseWorkspace = null;
        }

      }

    }   //  GetFileGeodatabaseFeatureClassLastUpdate


    public System.DateTime GetEntGeoDBFeatureClassLastUpdate(string FeatureClassName, string ServerName, string DatabaseName)
    {
      try
      {
        return GetEntGeoDBFeatureClassLastUpdate(FeatureClassName, ServerName, DatabaseName, "", "");
      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("");
          ErrorMessage("");
          ErrorMessage("The Maintools.FeatureClassUtilities.GetEntGeoDBFeatureClassLastUpdate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a January 1st, 1900 to the calling routine to indicate that this process failed.
        return new DateTime(1900, 1, 1);

      }

    }   //  GetEntGeoDBFeatureClassLastUpdate()


    public System.DateTime GetEntGeoDBFeatureClassLastUpdate(string FeatureClassName, string ServerName, string DatabaseName, string UserName)
    {
      PDX.BTS.DataMaintenance.MaintTools.ParameterManager parameterManager = null;

      try
      {
        //  Instantiate the Parameter Manager Object that will be used to determine the User Password.
        parameterManager = new PDX.BTS.DataMaintenance.MaintTools.ParameterManager();
        parameterManager.Initialize(_parameterTableName, _parameterDatabaseConnectionString);

        //  Make sure the Parameter Manager was initialized successfully before moving on.
        if (!parameterManager.IsInitialized())
        {
          //  Let the user know that this process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not Initialize the Parameter Manager. The FeatureClassUtilities.GetEntGeoDBFeatureClassLastUpdate() Method failed!");
          }

          //  Return a January 1st, 1900 to the calling routine to indicate that this process failed.
          return new DateTime(1900, 1, 1);

        }


        //  Determine the Password for the the User in the specified Database.
        string userPassword = parameterManager.ReadParameter(UserName + "_Password");


        //  Call the Overload of this method that requires a Database User Name and Password as input parameters using the passed User Name
        //  and the Password that was just determined
        return GetEntGeoDBFeatureClassLastUpdate(FeatureClassName, ServerName, DatabaseName, UserName, userPassword);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The FeatureClassUtilities.GetEntGeoDBFeatureClassLastUpdate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a January 1st, 1900 to the calling routine to indicate that this process failed.
        return new DateTime(1900, 1, 1);

      }
      finally
      {
        //  If the CGIS Data Maintenance Parameter Manager Object was instantiated, close it.
        if (parameterManager != null)
        {
          parameterManager = null;
        }

      }

    }


    public System.DateTime GetEntGeoDBFeatureClassLastUpdate(string FeatureClassName, string ServerName, string DatabaseName, string UserName, string UserPassword)
    {
      System.Data.SqlClient.SqlConnection updateDateConnection = null;
      System.Data.SqlClient.SqlCommand    updateDateCommand    = null;
      System.Data.SqlClient.SqlDataReader updateDateReader     = null;

      try
      {
        //  Build the Connection String that will be used to connect to the Source SDE Server.
        string sourceDatbaseConnectionString = null;
        if ((UserName.Length > 0) && (UserPassword.Length > 0))
        {
          //  Use the passed User Name and Password.
          sourceDatbaseConnectionString = "Initial Catalog=" + DatabaseName + ";"
                                        + "Data Source=" + ServerName + ";"
                                        + "User=" + UserName + ";"
                                        + "Password=" + UserPassword + ";";
        }
        else
        {
          //  No User Name or Password was passed so use Windows Authentication to connect to the Database.
          sourceDatbaseConnectionString = "Initial Catalog=" + DatabaseName + ";"
                                        + "Data Source=" + ServerName + ";"
                                        + "Integrated Security=SSPI;";

        }


        //  Connect to the Source SQL Server Database.
        updateDateConnection = ConnectToDatabase(sourceDatbaseConnectionString);
        if (updateDateConnection == null)
        {
          //  Let the User know that the process failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not establish a connection to the Source SQL Server Database FeatureClassUtilities.GetEntGeoDBFeatureClassLastUpdate() Failed!  Aborting proecess!");
          }

          //  Return a January 1st, 1900 to the calling application to indicate that this process failed.
          return new DateTime(1900, 1, 1);

        }


        //  Determine the Version of SQL Server the Data Source Server is running.
        string serverVersion = updateDateConnection.ServerVersion;


        //  Build the SQL Statement that will be used to determine the Last Update Date of the feature Class based on the SQL Server Version.
        string sqlStatement = null;
        string updateDateFieldName = null;
        if (serverVersion.IndexOf("09.") == 0)
        {
          //  Use the "Sys.Objects" Table to determine the last update date of the datasets.
          sqlStatement = "SELECT Modify_Date "
                       + "FROM Sys.Objects "
                       + "WHERE [Name] = '" + FeatureClassName + "'";

          //  Set the Udpate Date Field Name Variable to "Modify_Date".
          updateDateFieldName = "Modify_Date";

        }
        else
        {
          //  Use the SysObjects Table to determine the last update date of the datasets.
          sqlStatement = "SELECT RefDate "
                       + "FROM SysObjects "
                       + "WHERE [Name] = '" + FeatureClassName + "'";

          //  Set the Udpate Date Field Name Variable to "RefDate".
          updateDateFieldName = "RefDate";

        }


        //  Build the SQL Command Object that will be used to retrieve the Last Update Date Info from the Source SQL Server Database.
        updateDateCommand = new System.Data.SqlClient.SqlCommand();
        updateDateCommand.CommandText = sqlStatement;
        updateDateCommand.Connection = updateDateConnection;
        updateDateCommand.CommandType = System.Data.CommandType.Text;
        updateDateCommand.CommandTimeout = 20;


        //  Use the Command Object to open a SQL Data Reader that will house the Last Update Date Info from the Source SQL Server Database.
        updateDateReader = updateDateCommand.ExecuteReader();


        //  If some Last Update Date Info was retrieved from the database, return it to the calling routine.  Otherwise, return
        //  January 1st, 1900 as a default date to let the calling routine know that the date could not be determined.
        if (updateDateReader.HasRows)
        {
          //  Retrieve the Last Update Date from the SQL Data Reader.
          updateDateReader.Read();
          System.DateTime updateDate = (System.DateTime)updateDateReader[updateDateFieldName];

          //  Return the Last Update Date of the Feature class to the calling routine..
          return updateDate;

        }
        else
        {
          //  Return January 1st, 1900 as the last update date to let the calling routine know that the last update date could not
          //  be determined.
          return new System.DateTime(1900, 1, 1);

        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the user know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The FeatureClassUtilities.GetEntGeoDBFeatureClassLastUpdate() Method failed with error message - " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return a January 1st, 1900 to the calling routine to indicate that this process failed.
        return new DateTime(1900, 1, 1);

      }
      finally
      {
        //  If the Update Date SQL Data Reader Object was instantiated, close it.
        if (updateDateReader != null)
        {
          if (!updateDateReader.IsClosed)
          {
            updateDateReader.Close();
          }
          updateDateReader.Dispose();
          updateDateReader = null;
        }
        //  If the Update Date SQL Data Command Object was instantiated, close it.
        if (updateDateCommand != null)
        {
          updateDateCommand.Dispose();
          updateDateCommand = null;
        }
        //  If the SQL Client Rebuild Spatial Index Connetion Object was instantiated, close it.
        if (updateDateConnection != null)
        {
          if (updateDateConnection.State == System.Data.ConnectionState.Open)
          {
            updateDateConnection.Close();
          }
          updateDateConnection.Dispose();
          updateDateConnection = null;
        }

      }

    }   //  GetEntGeoDBFeatureClassLastUpdate()
    #endregion Update Dates


    #region Export Feature Classes
    public bool ExportGeodatabaseAnnotationFeatureClassToFileGeoDB(ESRI.ArcGIS.Geodatabase.IFeatureWorkspace InputFeatureWorkspace,
                                          ESRI.ArcGIS.Geodatabase.IFeatureClass InputFeatureClass, string OutputFeatureClassName, string OutputFileGeoDBPath,
                                          string OutputFileGeoDBName)
    {
      ESRI.ArcGIS.Geodatabase.IWorkspace                  inputWorkspace           = null;
      ESRI.ArcGIS.esriSystem.IPropertySet                 inputPropertySet         = null;
      System.Type                                         inputFactoryType         = null;
      System.Object                                       inputFactoryObject       = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceFactory           inputWorkspaceFactory    = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceName              inputWorkspaceName       = null;
      ESRI.ArcGIS.Geodatabase.IDataset                    inputDataset             = null;
      ESRI.ArcGIS.esriSystem.VarArray                     geoprocessorVariantArray = null;
      PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities generalUtilities         = null;
      ESRI.ArcGIS.Geoprocessing.GeoProcessor              geoProcessor             = null;

      try
      {
        //  QI to the Input Workspace.
        inputWorkspace = (ESRI.ArcGIS.Geodatabase.IWorkspace)InputFeatureWorkspace;


        //  Build a Property Set for the Current Workspace.
        inputPropertySet = new ESRI.ArcGIS.esriSystem.PropertySetClass();
        inputPropertySet = inputWorkspace.ConnectionProperties;

        //  The Folks at ESRI were not smart enough to handle a Property Set that did not include a Server Name so if there is not one in this propertyset dummy one in.
        if (inputPropertySet.GetProperty("Instance").ToString().ToUpper().IndexOf("SDE:SQLSERVER:") != -1)
        {
          //  Determine the server name from the "Instance" Property of the Property Set.
          string inputServerName = inputPropertySet.GetProperty("Instance").ToString();
          while (inputServerName.IndexOf(@":") != -1)
          {
            //  Strip the first character from the Input Server Name.
            inputServerName = inputServerName.Substring(1);
          }

          //  If the Server Name Includes an Instance Value, strip that from the server name.
          while (inputServerName.IndexOf(@"\") != -1)
          {
            //  Strip the last character from the Input Server Name.
            inputServerName = inputServerName.Substring(0, (inputServerName.Length - 1));
          }

          //  Add a Server Property to the Property Set.
          inputPropertySet.SetProperty("Server", inputServerName);

        }


        //  Determine which directory the Temporary SDE Connection File should be created in.
        string temporaryDirectory = null;
        if (System.IO.Directory.Exists(@"D:\Temp"))
        {
          //  Set the Temporary Directory to 'D:\TEMP\'.
          temporaryDirectory = @"D:\Temp\";

        }
        else
        {
          //  Check to see if there is a 'C:\TEMP' Directory.
          if (System.IO.Directory.Exists(@"C:\Temp"))
          {
            //  Set the Temporary Directory to 'C:\Temp\'
            temporaryDirectory = @"C:\Temp\";
          }
          else
          {
            //  Set the Temporary Directory to 'C:\'.
            temporaryDirectory = @"C:\";
          }

        }


        //  Make sure the Output Temporary Connection File does not already exist before attempting to create a new one.
        if (System.IO.File.Exists(temporaryDirectory + OutputFeatureClassName + "SDEConn.sde"))
        {
          //  Delete the existing File.
          System.IO.File.Delete(temporaryDirectory + OutputFeatureClassName + "SDEConn.sde");

        }


        //  Create the Temporary SDE Connection File that will be used to specify the Input Annotation Features for this export operation.
        inputFactoryType = System.Type.GetTypeFromProgID("esriDataSourcesGDB.SDEWorkspaceFactory");
        inputFactoryObject = System.Activator.CreateInstance(inputFactoryType);
        inputWorkspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory)inputFactoryObject;
        inputWorkspaceName = inputWorkspaceFactory.Create(temporaryDirectory, OutputFeatureClassName + "SDEConn.sde", inputPropertySet, 0);


        //  Specify the parameters for the export of this Feature Class to the Output File Geodatabase.
        inputDataset = (ESRI.ArcGIS.Geodatabase.IDataset)InputFeatureClass;
        string processDatasetName = null;
        if (inputDataset.Name.IndexOf(inputWorkspaceName.ConnectionProperties.GetProperty("Database").ToString()) > -1)
        {
          //  Drop the Server name from the Name before using it.
          processDatasetName = inputDataset.Name.Substring(inputWorkspaceName.ConnectionProperties.GetProperty("Database").ToString().Length + 1);
        }
        else
        {
          //  Use the Name as is.
          processDatasetName = inputDataset.Name.ToString();
        }


        //  Create a Field Mapping for this export.
        string inputAnnotationFeatures = inputWorkspaceName.PathName.ToString() + @"\" + processDatasetName;
        ESRI.ArcGIS.Geoprocessing.IGPUtilities geoprocessingUtilities = new ESRI.ArcGIS.Geoprocessing.GPUtilitiesClass();
        ESRI.ArcGIS.Geodatabase.IDETable inputTable = (ESRI.ArcGIS.Geodatabase.IDETable)geoprocessingUtilities.MakeDataElement(inputAnnotationFeatures, null, null);
        ESRI.ArcGIS.esriSystem.IArray inputTables = new ESRI.ArcGIS.esriSystem.ArrayClass();
        inputTables.Add(inputTable);
        ESRI.ArcGIS.Geoprocessing.IGPFieldMapping fieldMapping = new ESRI.ArcGIS.Geoprocessing.GPFieldMappingClass();



        //  Go through the fields in the Input Table and add them to the Field Mapping.
        object missing = Type.Missing;
        for (int i = 0; i < inputTable.Fields.FieldCount; i++)
        {
          if ((inputTable.Fields.get_Field(i).Type != ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeOID) &&
              (inputTable.Fields.get_Field(i).Type != ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeGeometry))
          {
            ESRI.ArcGIS.Geoprocessing.IGPFieldMap currentFieldMap = new ESRI.ArcGIS.Geoprocessing.GPFieldMapClass();
            currentFieldMap.AddInputField(inputTable, inputTable.Fields.get_Field(i), -1, -1);
            fieldMapping.AddFieldMap(currentFieldMap);
            currentFieldMap = null;

          }

        }


        //  Build the Variant Array that will be used to pass the parameters necessary for the export to the toolbox object.
        geoprocessorVariantArray = new ESRI.ArcGIS.esriSystem.VarArrayClass();
        geoprocessorVariantArray.Add(inputAnnotationFeatures);
        geoprocessorVariantArray.Add(System.IO.Path.Combine(OutputFileGeoDBPath, OutputFileGeoDBName + ".gdb"));
        geoprocessorVariantArray.Add(OutputFeatureClassName);
        geoprocessorVariantArray.Add(null);
        geoprocessorVariantArray.Add(fieldMapping);
        geoprocessorVariantArray.Add(null);


        //  Determine the ArcGIS Install Path so that the Projector Toolbox can be opened to be used.
        generalUtilities = new PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities();
        string installPath = generalUtilities.DetermineArcGISDesktopInstallPath();

        //  Make sure the Install Path was determined successfully before moving on.
        if (System.String.IsNullOrEmpty(installPath))
        {
          //  Let the user know that the ArcGIS Desktop Install Path could not be determined.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not Determine the ArcGIS Desktop Install Path to Initialize the Projection Toolbox.  The MaintTools.FeatureClassUtilities.ProjectFeatureClassHARNtoWGS() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Instantiate the Geoprocessing Object that will be used to export this Annotatioin Feature Class to the Output File Geodatabase.
        geoProcessor = new ESRI.ArcGIS.Geoprocessing.GeoProcessorClass();
        geoProcessor.AddToolbox(System.IO.Path.Combine(installPath, @"ArcToolbox\Toolboxes\Conversion Tools.tbx"));

        //  Perform the Export in a TRY Block so that any COM or IO Errors can be identified and handled.
        try
        {
          //  Perform the export.
          geoProcessor.Execute("FeatureClassToFeatureClass_Conversion", geoprocessorVariantArray, null);
          //  Write the messages from the Feature Class to Feature Class tool log file.
          int toolMessageCount = geoProcessor.MessageCount;
          int currentToolMessageIndex = 0;
          if (ProcessMessage != null)
          {
            ProcessMessage("         - Feature Class to Feature Class Operation Messages...");
          }
          while (currentToolMessageIndex < toolMessageCount)
          {
            //  Write the current message to the log file.
            if (ProcessMessage != null)
            {
              ProcessMessage("           + " + geoProcessor.GetMessage(currentToolMessageIndex));
            }
            //  Increment the Tool Message Index Counter.
            currentToolMessageIndex++;
          }

        }
        catch (System.IO.IOException ioException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Dissolve Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Feature Class to Feature Class Operation in the MaintTools.FeatureClassUtilities.ExportGeodatabaseAnnotationFeatureClassToFileGeoDB() Method Failed with error message - " + ioException.Message + " (" + ioException.Source + " Line:  " + lineNumber.ToString() + ")!");
          }
          int toolMessageCount = geoProcessor.MessageCount;
          int currentToolMessageIndex = 0;
          if (ProcessMessage != null)
          {
            ProcessMessage("The information from the Geoprocessor is:");
          }
          while (currentToolMessageIndex < toolMessageCount)
          {
            //  Write the current message to the log file.
            if (ProcessMessage != null)
            {
              ProcessMessage("   + " + geoProcessor.GetMessage(currentToolMessageIndex));
            }
            //  Increment to Toold Message Index Counter.
            currentToolMessageIndex++;
          }
          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;
        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Dissolve Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Feature Class to Feature Class Operation in the MaintTools.FeatureClassUtilities.ExportGeodatabaseAnnotationFeatureClassToFileGeoDB() Method Failed with error message - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
          }
          int toolMessageCount = geoProcessor.MessageCount;
          int currentToolMessageIndex = 0;
          if (ProcessMessage != null)
          {
            ProcessMessage("The information from the Geoprocessor is:");
          }
          while (currentToolMessageIndex < toolMessageCount)
          {
            //  Write the current message to the log file.
            if (ProcessMessage != null)
            {
              ProcessMessage("   + " + geoProcessor.GetMessage(currentToolMessageIndex));
            }
            //  Increment to Toold Message Index Counter.
            currentToolMessageIndex++;
          }
          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;
        }


        //  Delete the SDE Connection File since it is no longer needed.
        if (System.IO.File.Exists(temporaryDirectory + OutputFeatureClassName + "SDEConn.sde"))
        {
          //  Delete the existing File.
          System.IO.File.Delete(temporaryDirectory + OutputFeatureClassName + "SDEConn.sde");
        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ExportGeodatabaseAnnotationFeatureClassToFileGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the Input Workspace Object has been Instantiated, close it.
        if (inputWorkspace != null)
        {
          //  Close the Input Workspace Object.
          inputWorkspace = null;
        }
        //  If the Input Property Set Object was instantiated, close it.
        if (inputPropertySet != null)
        {
          inputPropertySet = null;
        }
        //  If the Input Factory Type Object was instantiated, close it.
        if (inputFactoryType != null)
        {
          inputFactoryType = null;
        }
        //  If the Input Factory Object was instantiated, close it.
        if (inputFactoryObject != null)
        {
          inputFactoryObject = null;
        }
        //  If the Input Workspace Factory Object was instantiated, close it.
        if (inputWorkspaceFactory != null)
        {
          inputWorkspaceFactory = null;
        }
        //  If the Input Workspace Name Object was instantiated, close it.
        if (inputWorkspaceName != null)
        {
          inputWorkspaceName = null;
        }
        //  If the Input Dataset name Object was instantiated, close it.
        if (inputDataset != null)
        {
          inputDataset = null;
        }
        //  If the Geoprocessor Variant Array Object was instantiated, close it.
        if (geoprocessorVariantArray != null)
        {
          geoprocessorVariantArray.RemoveAll();
          geoprocessorVariantArray = null;
        }
        //  If the General Utilities Object was instantiated, close it.
        if (generalUtilities != null)
        {
          generalUtilities = null;
        }
        //  If the Geoprocessor Object was instantiated, close it.
        if (geoProcessor != null)
        {
          geoProcessor = null;
        }

      }

      //  If the process made it to here it was successful so return TRUE to the calling method.
      return true;

    }   //  ExportGeodatabaseAnnotationFeatureClassToFileGeoDB()


    public bool ExportFeatureClass(ESRI.ArcGIS.Geodatabase.IFeatureClass InputFeatureClass, string OutputFeatureClassName, ESRI.ArcGIS.Geodatabase.IWorkspace OutputWorkspace)
    {
      try
      {
        //  Call the overload of this method that requires a Where Clause Parameter defaulting the Where Clause to a NULL String.
        return ExportFeatureClass(InputFeatureClass, OutputFeatureClassName, OutputWorkspace, null);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ExportFeatureClass() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

    }   //  ExportFeatureClass()


    public bool ExportFeatureClass(ESRI.ArcGIS.Geodatabase.IFeatureClass InputFeatureClass, string OutputFeatureClassName, ESRI.ArcGIS.Geodatabase.IWorkspace OutputWorkspace, string WhereClause)
    {
      ESRI.ArcGIS.Geodatabase.IDataset          inputDataset                 = null;
      ESRI.ArcGIS.Geodatabase.IFeatureClassName inputFeatureClassName        = null;
      ESRI.ArcGIS.Geodatabase.IDatasetName      inputDatasetName             = null;
      ESRI.ArcGIS.Geodatabase.IQueryFilter      inputQueryFilter             = null;
      ESRI.ArcGIS.Geodatabase.IFeatureClassName outputFeatureClassNameObject = null;
      ESRI.ArcGIS.Geodatabase.IDataset          outputDataset                = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceName    outputWorkspaceName          = null;
      ESRI.ArcGIS.Geodatabase.IDatasetName      outputDatasetName            = null;
      ESRI.ArcGIS.GeoDatabaseUI.ExportOperation exportOperation              = null;

      try
      {
        //  QI to the Input Dataset Name.
        inputDataset = (ESRI.ArcGIS.Geodatabase.IDataset)InputFeatureClass;
        inputFeatureClassName = (ESRI.ArcGIS.Geodatabase.IFeatureClassName)inputDataset.FullName;
        inputDatasetName = (ESRI.ArcGIS.Geodatabase.IDatasetName)inputFeatureClassName;


        //  If a Where Clause was passed to this method, build a Query Filter to be passed to the Export Operation.
        if (!System.String.IsNullOrEmpty(WhereClause))
        {
          inputQueryFilter = new ESRI.ArcGIS.Geodatabase.QueryFilterClass();
          inputQueryFilter.WhereClause = WhereClause;

        }


        //  Define the Output Feature Class.
        outputFeatureClassNameObject = new ESRI.ArcGIS.Geodatabase.FeatureClassNameClass();
        outputDatasetName = (ESRI.ArcGIS.Geodatabase.IDatasetName)outputFeatureClassNameObject;
        outputDatasetName.Name = OutputFeatureClassName;
        outputFeatureClassNameObject.FeatureType = InputFeatureClass.FeatureType;
        outputFeatureClassNameObject.ShapeType = InputFeatureClass.ShapeType;
        outputFeatureClassNameObject.ShapeFieldName = InputFeatureClass.ShapeFieldName;


        //  Define the Ouput Workspace Name Object.
        outputDataset = (ESRI.ArcGIS.Geodatabase.IDataset)OutputWorkspace;
        outputWorkspaceName = (ESRI.ArcGIS.Geodatabase.IWorkspaceName)outputDataset.FullName;

        
        //  Associate the Workspace with the output Feature class.
        outputDatasetName.WorkspaceName = outputWorkspaceName;

        
        //  Export the Feature Class to the output File GeoDatabase.
        exportOperation = new ESRI.ArcGIS.GeoDatabaseUI.ExportOperationClass();
        exportOperation.ExportFeatureClass(inputDatasetName, inputQueryFilter, null, null, outputFeatureClassNameObject, 0);


      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ExportFeatureClass() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the Input Dataset name Object was instantiated, close it.
        if (inputDataset != null)
        {
          inputDataset = null;
        }
        //  If the Input Feature Class Name Object was instantiated, close it.
        if (inputFeatureClassName != null)
        {
          inputFeatureClassName = null;
        }
        //  If the Input Dataset Name Object was instantiated, close it.
        if (inputDatasetName != null)
        {
          inputDatasetName = null;
        }
        //  If the Input Query Filter Object was instantiated, close it.
        if (inputQueryFilter != null)
        {
          inputQueryFilter = null;
        }
        //  If the Output Feature Class Name Object was instantiated, close it.
        if (outputFeatureClassNameObject != null)
        {
          outputFeatureClassNameObject = null;
        }
        //  If the Output Dataset Object was instantiated, close it.
        if (outputDataset != null)
        {
          outputDataset = null;
        }
        //  If the Output Workspace Name Object was instantiated, close it.
        if (outputWorkspaceName != null)
        {
          outputWorkspaceName = null;
        }
        //  If the Output Dataset Name Object was instantiated, close it.
        if (outputDatasetName != null)
        {
          outputDatasetName = null;
        }
        //  If the Export Operation Object was instantiated, close it.
        if (exportOperation != null)
        {
          exportOperation = null;
        }

      }

      //  If the process made it to here it was successful so return TRUE to the calling method.
      return true;

    }   //  ExportFeatureClass()
    #endregion Export Feature Classes


    #region Project Feature Class
    public bool ProjectFeatureClasstoWGSInEntGeoDB(ESRI.ArcGIS.esriSystem.IPropertySet EntGeoDBPropertySet, string InputFeatureClassName, string OutputFeatureClassName, string OutputProjectionDefinitionFile)
    {
      System.Type                               projectFactoryType      = null;
      System.Object                             projectFactoryObject    = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceFactory projectWorkspaceFactory = null;
      ESRI.ArcGIS.Geodatabase.IWorkspaceName    projectWorkspaceName    = null;

      try
      {
        //  Determine which directory the Temporary SDE Connection File should be created in.
        string temporaryDirectory = null;
        if (System.IO.Directory.Exists(@"D:\Temp"))
        {
          //  Set the Temporary Directory to 'D:\TEMP\'.
          temporaryDirectory = @"D:\Temp\";

        }
        else
        {
          //  Check to see if there is a 'C:\TEMP' Directory.
          if (System.IO.Directory.Exists(@"C:\Temp"))
          {
            //  Set the Temporary Directory to 'C:\Temp\'
            temporaryDirectory = @"C:\Temp\";
          }
          else
          {
            //  Set the Temporary Directory to 'C:\'.
            temporaryDirectory = @"C:\";
          }

        }


        //  Make sure the Output Temporary Connection File does not already exist before attempting to create a new one.
        if (System.IO.File.Exists(temporaryDirectory + OutputFeatureClassName + "SDEConn.sde"))
        {
          //  Delete the existing File.
          System.IO.File.Delete(temporaryDirectory + OutputFeatureClassName + "SDEConn.sde");

        }


        //  Create the Temporary SDE Connection File that will be used to specify the Input Annotation Features for this export operation.
        projectFactoryType = System.Type.GetTypeFromProgID("esriDataSourcesGDB.SDEWorkspaceFactory");
        projectFactoryObject = System.Activator.CreateInstance(projectFactoryType);
        projectWorkspaceFactory = (ESRI.ArcGIS.Geodatabase.IWorkspaceFactory)projectFactoryObject;
        projectWorkspaceName = projectWorkspaceFactory.Create(temporaryDirectory, OutputFeatureClassName + "SDEConn.sde", EntGeoDBPropertySet, 0);

        
        //  Create the string to identify the input Projection Features for this operation.
        string inputProjectFeatures = projectWorkspaceName.PathName.ToString() + @"\" + InputFeatureClassName;


        //  Setup the Output Features String.
        string outputFeatures = projectWorkspaceName.PathName.ToString() + @"\" + OutputFeatureClassName;


        //  Attempt to Project the Feature Class to the WGS Coordinate Plane.
        return ProjectFeatureClassHARNtoWGS(inputProjectFeatures, outputFeatures, OutputProjectionDefinitionFile);

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ProjectFeatureClasstoWGSInEntGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the Input Workspace Name Object was instantiated, close it.
        if (projectWorkspaceName != null)
        {
          projectWorkspaceName = null;
        }
        //  If the Input Workspace Factory Object was instantiated, close it.
        if (projectWorkspaceFactory != null)
        {
          projectWorkspaceFactory = null;
        }
        //  If the Input Factory Object was instantiated, close it.
        if (projectFactoryObject != null)
        {
          projectFactoryObject = null;
        }
        //  If the Input Factory Type Object was instantiated, close it.
        if (projectFactoryType != null)
        {
          projectFactoryType = null;
        }

      }

    }   //  ProjectFeatureClasstoWGSInEntGeoDB()


    public bool ProjectFeatureClasstoWGSInFileGeoDB(string FileGeodatabase, string InputFeatureClassName, string OutputFeatureClassName, string OutputProjectionDefinitionFile)
    {
      try
      {
        //  Setup the Source Features String.
        string sourceFeatures = System.IO.Path.Combine(FileGeodatabase, InputFeatureClassName);


        //  Setup the Output Features String.
        string outputFeatures = System.IO.Path.Combine(FileGeodatabase, OutputFeatureClassName);


        //  Attempt to Project the Feature Class to the WGS Coordinate Plane.
        bool projectedFeatureClass = ProjectFeatureClassHARNtoWGS(sourceFeatures, outputFeatures, OutputProjectionDefinitionFile);


        //  Return the results of the Projection Attempt to the calling method.
        return projectedFeatureClass;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ProjectFeatureClasstoWGSInFileGeoDB() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }

    }   //  ProjectFeatureClasstoWGSInFileGeoDB()


    private bool ProjectFeatureClassHARNtoWGS(string SourceFeatures, string OutputFeatures, string OutputProjectionDefinitionFile)
    {
      ESRI.ArcGIS.esriSystem.IVariantArray                geoprocessorVariantArray = null;
      PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities generalUtilities         = null;
      ESRI.ArcGIS.Geoprocessing.GeoProcessor              projectDataGeoProcessor  = null;

      try
      {
        //  Let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("              + Initializing the Projection Operation...");
        }

        //  Specify the parameters for the export of this Feature Class to the Output File Geodatabase.
        geoprocessorVariantArray = new ESRI.ArcGIS.esriSystem.VarArrayClass();
        geoprocessorVariantArray.Add(SourceFeatures);
        geoprocessorVariantArray.Add(OutputFeatures);
        geoprocessorVariantArray.Add(OutputProjectionDefinitionFile);
        geoprocessorVariantArray.Add("NAD_1983_HARN_To_WGS_1984");


        //  Determine the ArcGIS Install Path so that the Projector Toolbox can be opened to be used.
        generalUtilities = new PDX.BTS.DataMaintenance.MaintTools.GeneralUtilities();
        string installPath = generalUtilities.DetermineArcGISDesktopInstallPath();

        //  Make sure the Install Path was determined successfully before moving on.
        if (System.String.IsNullOrEmpty(installPath))
        {
          //  Let the user know that the ArcGIS Desktop Install Path could not be determined.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not Determine the ArcGIS Desktop Install Path to Initialize the Projection Toolbox.  The MaintTools.FeatureClassUtilities.ProjectFeatureClassHARNtoWGS() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Instantiate the Geoprocessing Object that will be used to export this Annotatioin Feature Class to the Output File Geodatabase.
        projectDataGeoProcessor = new ESRI.ArcGIS.Geoprocessing.GeoProcessorClass();
        projectDataGeoProcessor.AddToolbox(System.IO.Path.Combine(installPath, @"ArcToolbox\Toolboxes\Data Management Tools.tbx"));


        //  Let the user know what is happening.
        if (ProcessMessage != null)
        {
          ProcessMessage("              + Performing the Projection Operation...");
        }

        //  Perform the Projection in a TRY Block so that any COM or IO Errors can be identified and handled.
        try
        {
          //  Perform the export.
          projectDataGeoProcessor.Execute("Project_Management", geoprocessorVariantArray, null);

          //  Write the messages from the Projection Tool to the process log file.
          if (ProcessMessage != null)
          {
            int toolMessageCount = projectDataGeoProcessor.MessageCount;
            int currentToolMessageIndex = 0;
            ProcessMessage("");
            ProcessMessage("              + Project Feature Class Operation Messages...");
            while (currentToolMessageIndex < toolMessageCount)
            {
              //  Write the current message to the log file.
              ProcessMessage("                 >> " + projectDataGeoProcessor.GetMessage(currentToolMessageIndex));
              //  Increment the Tool Message Index Counter.
              currentToolMessageIndex++;
            }
          }

        }
        catch (System.IO.IOException ioException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(ioException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Dissolve Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Project Feature Class Operation in the ProjectFeatureClassHARNtoWGS() Method Failed with error message - " + ioException.Message + " (" + ioException.Source + " Line:  " + lineNumber.ToString() + ")!");
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;
        }
        catch (System.Runtime.InteropServices.COMException comException)
        {
          //  Determine the Line Number from which the exception was thrown.
          System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(comException, true);
          System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
          int lineNumber = stackFrame.GetFileLineNumber();

          //  Let the User know that the Dissolve Operation Failed.
          if (ErrorMessage != null)
          {
            ErrorMessage("The Project Feature Class Operation in the ProjectFeatureClassHARNtoWGS() Method Failed with error message - " + comException.Message + " (" + comException.ErrorCode + " Line:  " + lineNumber.ToString() + ")!");
          }

          //  Return FALSE to the calling routine ito indicate that this process failed.
          return false;
        }

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.ProjectFeatureClassHARNtoWGS() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {

      }

      //  If the process made it to here it was successful so return TRUE to the calling method.
      return true;

    }   //  ProjectFeatureClassHARNtoWGS()
    #endregion Project Feature Class


    #region SQL Spatial Feature Class Utilities
    public bool CreateSQLSpatialIndexOnEntGeoDBFeatureClass(string ServerName, string GeodatabaseName, string ProcedureName, string FeatureClassName, string GridLevel1 = "Medium",  string GridLevel2 = "Medium",  string GridLevel3 = "Medium",  string GridLevel4 = "Medium",  string UserName = "", string UserPassword = "")
    {
      System.Data.SqlClient.SqlConnection rebuildIndexSQLConnectionObject   = null;
      System.Data.SqlClient.SqlCommand    rebuildIndexSQLCommandObject      = null;
      System.Data.SqlClient.SqlParameter  rebuildIndexFClassSQLParameter    = null;
      System.Data.SqlClient.SqlParameter  rebuildIndexGridLevel1Parameter   = null;
      System.Data.SqlClient.SqlParameter  rebuildIndexGridLevel2Parameter   = null;
      System.Data.SqlClient.SqlParameter  rebuildIndexGridLevel3Parameter   = null;
      System.Data.SqlClient.SqlParameter  rebuildIndexGridLevel4Parameter   = null;


      try
      {
        //  If the User passed a User Name and Password to this method build a connection string including them.  Otherwise build an
        //  Operating System Authentication string.
        string connectionString = null;
        if ((!System.String.IsNullOrEmpty(UserName)) && (!System.String.IsNullOrEmpty(UserPassword)))
        {
          //  Build a Connection String using the User Name and Password that were passed to the method.
          connectionString = "User ID=" + UserName + ";"
                           + "Password=" + UserPassword + ";"
                           + "Initial Catalog=" + GeodatabaseName + ";"
                           + "Data Source=" + ServerName;
        }
        else
        {
          //  Build a Connection String using Operating System Authentication.
          connectionString = "Initial Catalog=" + GeodatabaseName + ";"
                           + "Data Source=" + ServerName + ";"
                           + "Integrated Security=SSPI;";
        }


        //  Attempt to connect to the Database.
        rebuildIndexSQLConnectionObject = ConnectToDatabase(connectionString);
        if (rebuildIndexSQLConnectionObject == null)
        {
          //  Let the user know that the ArcGIS Desktop Install Path could not be determined.
          if (ErrorMessage != null)
          {
            ErrorMessage("Could not Connect to the " + GeodatabaseName + " Geodatabase.  The MaintTools.FeatureClassUtilities.CreateSQLSpatialIndexOnEntGeoDBFeatureClass() Method failed!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Make sure the Grid Level Values are valid before attempting to run the Stored Procedure.
        string gridLevel1Value = "";
        if ((GridLevel1.Equals("Low", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel1.Equals("Medium", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel1.Equals("High", System.StringComparison.CurrentCultureIgnoreCase)))
        {
          //  Use the passed value.
          gridLevel1Value = GridLevel1;
        }
        else
        {
          //  Default Grid Level 1 to "Medium".
          gridLevel1Value = "Medium";
        }

        string gridLevel2Value = "";
        if ((GridLevel2.Equals("Low", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel2.Equals("Medium", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel2.Equals("High", System.StringComparison.CurrentCultureIgnoreCase)))
        {
          //  Use the passed value.
          gridLevel2Value = GridLevel2;
        }
        else
        {
          //  Default Grid Level 2 to "Medium".
          gridLevel2Value = "Medium";
        }

        string gridLevel3Value = "";
        if ((GridLevel3.Equals("Low", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel3.Equals("Medium", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel3.Equals("High", System.StringComparison.CurrentCultureIgnoreCase)))
        {
          //  Use the passed value.
          gridLevel3Value = GridLevel3;
        }
        else
        {
          //  Default Grid Level 3 to "Medium".
          gridLevel3Value = "Medium";
        }

        string gridLevel4Value = "";
        if ((GridLevel4.Equals("Low", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel4.Equals("Medium", System.StringComparison.CurrentCultureIgnoreCase)) ||
            (GridLevel4.Equals("High", System.StringComparison.CurrentCultureIgnoreCase)))
        {
          //  Use the passed value.
          gridLevel4Value = GridLevel4;
        }
        else
        {
          //  Default Grid Level 4 to "Medium".
          gridLevel4Value = "Medium";
        }


        //  Build the Command Object that will be used to run the Stored procedure to rebuild the Spatial Index on the specified Feature Class.
        rebuildIndexSQLCommandObject = new System.Data.SqlClient.SqlCommand();
        rebuildIndexSQLCommandObject.Connection = rebuildIndexSQLConnectionObject;
        rebuildIndexSQLCommandObject.CommandText = ProcedureName;
        rebuildIndexSQLCommandObject.CommandType = System.Data.CommandType.StoredProcedure;
        rebuildIndexSQLCommandObject.CommandTimeout = 300;


        //  Add the Parameters Necessary to run the procedure to the Command Object.
        //  Feature class Name Parameter.
        rebuildIndexFClassSQLParameter = new System.Data.SqlClient.SqlParameter();
        rebuildIndexFClassSQLParameter.ParameterName = "@dataName";
        rebuildIndexFClassSQLParameter.Direction = System.Data.ParameterDirection.Input;
        rebuildIndexFClassSQLParameter.SqlDbType = System.Data.SqlDbType.VarChar;
        rebuildIndexFClassSQLParameter.Value = FeatureClassName;
        rebuildIndexSQLCommandObject.Parameters.Add(rebuildIndexFClassSQLParameter);

        //  Grid Level 1 Parameter.
        rebuildIndexGridLevel1Parameter = new System.Data.SqlClient.SqlParameter();
        rebuildIndexGridLevel1Parameter.ParameterName = "@gridLevel1";
        rebuildIndexGridLevel1Parameter.Direction = System.Data.ParameterDirection.Input;
        rebuildIndexGridLevel1Parameter.SqlDbType = System.Data.SqlDbType.VarChar;
        rebuildIndexGridLevel1Parameter.Value = gridLevel1Value;
        rebuildIndexSQLCommandObject.Parameters.Add(rebuildIndexGridLevel1Parameter);

        //  Grid Level 2 Parameter.
        rebuildIndexGridLevel2Parameter = new System.Data.SqlClient.SqlParameter();
        rebuildIndexGridLevel2Parameter.ParameterName = "@gridLevel2";
        rebuildIndexGridLevel2Parameter.Direction = System.Data.ParameterDirection.Input;
        rebuildIndexGridLevel2Parameter.SqlDbType = System.Data.SqlDbType.VarChar;
        rebuildIndexGridLevel2Parameter.Value = gridLevel2Value;
        rebuildIndexSQLCommandObject.Parameters.Add(rebuildIndexGridLevel2Parameter);

        //  Grid Level 3 Parameter.
        rebuildIndexGridLevel3Parameter = new System.Data.SqlClient.SqlParameter();
        rebuildIndexGridLevel3Parameter.ParameterName = "@gridLevel3";
        rebuildIndexGridLevel3Parameter.Direction = System.Data.ParameterDirection.Input;
        rebuildIndexGridLevel3Parameter.SqlDbType = System.Data.SqlDbType.VarChar;
        rebuildIndexGridLevel3Parameter.Value = gridLevel3Value;
        rebuildIndexSQLCommandObject.Parameters.Add(rebuildIndexGridLevel3Parameter);

        //  Grid Level 4 Parameter.
        rebuildIndexGridLevel4Parameter = new System.Data.SqlClient.SqlParameter();
        rebuildIndexGridLevel4Parameter.ParameterName = "@gridLevel4";
        rebuildIndexGridLevel4Parameter.Direction = System.Data.ParameterDirection.Input;
        rebuildIndexGridLevel4Parameter.SqlDbType = System.Data.SqlDbType.VarChar;
        rebuildIndexGridLevel4Parameter.Value = gridLevel4Value;
        rebuildIndexSQLCommandObject.Parameters.Add(rebuildIndexGridLevel4Parameter);


        //  Attempt to Rebuild the Spatial Index on the specified Feature Class.
        rebuildIndexSQLCommandObject.ExecuteNonQuery();


        //  If the process made it to here, it was successful so return TRUE to the calling method.
        return true;

      }
      catch (System.Exception caught)
      {
        //  Determine the Line Number from which the exception was thrown.
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(caught, true);
        System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
        int lineNumber = stackFrame.GetFileLineNumber();

        //  Let the User know that this process failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.FeatureClassUtilities.CreateSQLSpatialIndexOnEntGeoDBFeatureClass() Method failed with error message:  " + caught.Message + " (Line:  " + lineNumber.ToString() + ")!");
        }

        //  Return FALSE to the calling routine to indicate that this process failed.
        return false;

      }
      finally
      {
        //  If the SQL Client Grid Level 4 SQL Parameter Object was instantiated, close it.
        if (rebuildIndexGridLevel4Parameter != null)
        {
          rebuildIndexGridLevel4Parameter = null;
        }
        //  If the SQL Client Grid Level 3 SQL Parameter Object was instantiated, close it.
        if (rebuildIndexGridLevel3Parameter != null)
        {
          rebuildIndexGridLevel3Parameter = null;
        }
        //  If the SQL Client Grid Level 2 SQL Parameter Object was instantiated, close it.
        if (rebuildIndexGridLevel2Parameter != null)
        {
          rebuildIndexGridLevel2Parameter = null;
        }
        //  If the SQL Client Grid Level 1 SQL Parameter Object was instantiated, close it.
        if (rebuildIndexGridLevel1Parameter != null)
        {
          rebuildIndexGridLevel1Parameter = null;
        }
        //  If the SQL Client Feature Class Name SQL Parameter Object was instantiated, close it.
        if (rebuildIndexFClassSQLParameter != null)
        {
          rebuildIndexFClassSQLParameter = null;
        }
        //  If the SQL Client Rebuild Spatial Index Command Object was instantiated, close it.
        if (rebuildIndexSQLCommandObject != null)
        {
          rebuildIndexSQLCommandObject.Dispose();
          rebuildIndexSQLCommandObject = null;
        }
        //  If the SQL Client Rebuild Spatial Index Connetion Object was instantiated, close it.
        if (rebuildIndexSQLConnectionObject != null)
        {
          if (rebuildIndexSQLConnectionObject.State == System.Data.ConnectionState.Open)
          {
            rebuildIndexSQLConnectionObject.Close();
          }
          rebuildIndexSQLConnectionObject.Dispose();
          rebuildIndexSQLConnectionObject = null;
        }

      }

    }   //  CreateSQLSpatialIndexOnEntGeoDBFeatureClass()
    #endregion SQL Spatial Feature Class Utilities


    #region Database Connections
    private System.Data.SqlClient.SqlConnection ConnectToDatabase(string ConnectionString)
    {
      System.Data.SqlClient.SqlConnection databaseConnection = null;

      try
      {
        //  Create and open a new connection to the database.
        databaseConnection = new System.Data.SqlClient.SqlConnection();
        databaseConnection.ConnectionString = ConnectionString;
        databaseConnection.Open();



        //  If there is a valid connection to the database, return a pointer to the Geodatabase Connection.  Otherwise, return a NULL Pointer
        //  to the calling method to indicate that this method failed.
        if (databaseConnection.State == System.Data.ConnectionState.Open)
        {
          return databaseConnection;
        }
        else
        {
          return null;
        }
      }
      catch
      {
        //  Return a NULL Pointer to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  ConnectToDatabase()
    #endregion Database Connections


    #region Message Handling
    private void HandleProcessMessage(string Message)
    {
      //  If the Process Message Handler has been initialized, relay the Message to the calling method.
      if (ProcessMessage != null)
      {
        ProcessMessage(Message);
      }

    }

    private void HandleErrorMessage(string Message)
    {
      //  If the Error Message Handler has been initialized, relay the Message to the calling method.
      if (ErrorMessage != null)
      {
        ErrorMessage(Message);
      }

    }
    #endregion Message Handling

    
    #region Class Cleanup
    /// <summary>
    /// Public Dispose Method called by Calling Application to clean up after this method.
    /// </summary>
    public void Dispose()
    {
      try
      {
        //  Call the method to do the cleanup.  Indicate that the user called the Dispose Method.
        Dispose(true);


        //  Tell the Garbace Collector to not go through cleanup since this method was called.
        GC.SuppressFinalize(this);

      }
      catch
      {
        //  Exit this method.
        return;

      }

    }   //  Dispose()


    /// <summary>
    /// Method to do the work of cleaning up.
    /// </summary>
    /// <param name="Disposing">
    /// Indicator of wether the Dispose method was called by the calling application or if the
    /// pointer to this method was just set to NULL.
    /// </param>
    protected void Dispose(System.Boolean Disposing)
    {
      try
      {
        //  If the Class has not already been Disposed (cleaned up after) clean up.
        if (!_isDisposed)
        {
          //  If the User called the Dispose Method, clean up managed resources.
          if (Disposing)
          {
            //  Dispose of Managed Resources.
            _component.Dispose();
          }

          //  Clean up the Unmanaged resources.

        }


        //  Set the Disposed Indicator Variable to TRUE.
        _isDisposed = true;

      }
      catch
      {
        //  Exit this method.
        return;

      }

    }   //  Dispose()


    /// <summary>
    /// Class Destructor
    /// </summary>
    ~FeatureClassUtilities()
    {
      try
      {
        //  Call the Dispose Method to clean up any objects that were instantiated by this method.
        Dispose(false);

      }
      catch
      {
        //  Exit this method.
        return;

      }

    }   //  ~FeatureClassUtilities()
    #endregion  Class Cleanup


    //  The connection parameters for the Import Monitor Database.
    static string                               _monitorDatabaseServerName         = null;
    static string                               _monitorDatabaseName               = null;
    static string                               _parameterTableName                = null;
    static string                               _parameterDatabaseConnectionString = null;

    //  Indicator of whether or not the Class has been Disposed of by the calling application.
    private System.Boolean                      _isDisposed                        = false;

    //  A Component Object that will be used to hold all Managed Resources for disposal.
    private System.ComponentModel.Component     _component                         = null;

    //  A Collection of Shapefile to be used when Copying, Moving or Deleteing a Shapefile.
    private System.Collections.ArrayList        _shapefileExtensions               = null;
    private System.Collections.ArrayList        _shapefileNecessaryExtensions      = null;

  }   //  CLASS:  FeatureClassUtilities

}   //  NAMESPACE:  PDX.BTS.DataMaintenance.MaintTools
