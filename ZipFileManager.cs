using System;
using System.Collections.Generic;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{
  public class ZipFileManager : System.IDisposable
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
    public ZipFileManager()
    {
      try
      {
        //  Instantiate the Component that will hold the Managed Resources used by this method.
        _component = new System.ComponentModel.Component();

      }
      catch
      {
        //  Exit this method.
        return;
      }

      //  Exit the constructor method.
      return;

    }   //  ZipFileManager()


    public bool ZipDirectory(string SourceDirectory, string ZipFilePath, bool OverWrite = false)
    {
      try
      {
        //  Make sure the specified Source Directory Exists.
        if (!System.IO.Directory.Exists(SourceDirectory))
        {
          //  Let the user know that the directory does not exist.
          if (ErrorMessage != null)
          {
            ErrorMessage("The specified source directory - " + SourceDirectory + " - does not exist.  Aborting the MaintTools.ZipFileManager.ZipDirectory() Method!");
          }

          //  Return FALSE to the calling method to indicate that this method failed.
          return false;

        }


        //  Make sure the specified output Directory Exists.
        if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(ZipFilePath)))
        {
          //  Attempt to create the directory.
          System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ZipFilePath));

        }


        //  Determine if the specified Output File already exists.
        string outputFilePath = ZipFilePath;
        if (System.IO.File.Exists(ZipFilePath))
        {
          //  If the user wants the old file overwritten, delete it.
          if (OverWrite)
          {
            System.IO.File.Delete(ZipFilePath);
          }
          else
          {
            //  Try to find a file name to use.
            string outputBasePath = outputFilePath.Substring(0, (outputFilePath.Length - 4));
            string extension = outputFilePath.Substring((outputFilePath.Length - 4));
            int i = 0;
            outputFilePath = "";
            while ((i < 100) && (outputFilePath.Length == 0))
            {
              if (!System.IO.File.Exists(outputBasePath + "(" + i.ToString() + ")" + extension))
              {
                outputFilePath = outputBasePath + "(" + i.ToString() + ")" + extension;
              }
              //  Increment the counter.
              i++;
            }
          }

        }


        //  Retrieve the Directory Name to be included in the Zip File.
        string directoryName = SourceDirectory;
        while (directoryName.IndexOf(@"\") != -1)
        {
          //  Drop the Left Most character from the Directory Name.
          directoryName = directoryName.Substring(1);
        }


        //  Attempt to zip the files in the directory.
        using (Ionic.Zip.ZipFile zipFile = new Ionic.Zip.ZipFile())
        {
          zipFile.AddDirectory(SourceDirectory, directoryName);
          zipFile.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
          zipFile.Save(ZipFilePath);
        }

      }
      catch (System.Exception caught)
      {
        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ErrorMessage("The MaintTools.ZipFileManager.ZipDirectory() Method failed with error message:  " + caught.Message);
        }

        //  Return FALSE to the calling method to indicate that this process failed.
        return false;

      }

      //  If the process made it to here, it was successful so return TRUE to the calling method.
      return true;

    }


    public bool? ExtractAllFilesFromZipPackage(string ZipFilePath, string OutputDirectory)
    {
      try
      {
        //  Extract the Files from the Zip Package.
        using (Ionic.Zip.ZipFile inputZipFile = Ionic.Zip.ZipFile.Read(ZipFilePath))
        {
          foreach (Ionic.Zip.ZipEntry currentEntry in inputZipFile)
          {
            currentEntry.Extract(OutputDirectory);
          }

        }


      }
      catch(System.Exception caught)
      {
        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ProcessMessage("The MaintTools.ZipFileManager.ExtractAllFilesFromZipPackage() Method failed with error message:  " + caught.Message);
        }

        //  Return NULL to the calling method to indicate that this process failed.
        return null;
      }

      //  If the process made it to here, it was successful so return TRUE to the calling method.
      return true;

    }   //  ExtractAllFilesFromZipPackage()


    public bool? ExtractAllFilesFromZipPackageOverwrite(string ZipFilePath, string OutputDirectory)
    {
      try
      {
        //  Extract the Files from the Zip Package.
        using (Ionic.Zip.ZipFile inputZipFile = Ionic.Zip.ZipFile.Read(ZipFilePath))
        {
          foreach (Ionic.Zip.ZipEntry currentEntry in inputZipFile)
          {
            if (System.IO.File.Exists(System.IO.Path.Combine(OutputDirectory, currentEntry.FileName)))
            {
              System.IO.File.Delete(System.IO.Path.Combine(OutputDirectory, currentEntry.FileName));
            }
            currentEntry.Extract(OutputDirectory);
          }

        }


      }
      catch (System.Exception caught)
      {
        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ProcessMessage("The MaintTools.ZipFileManager.ExtractAllFilesFromZipPackageOverwrite() Method failed with error message:  " + caught.Message);
        }

        //  Return NULL to the calling method to indicate that this process failed.
        return null;
      }

      //  If the process made it to here, it was successful so return TRUE to the calling method.
      return true;

    }   //  ExtractAllFilesFromZipPackageOverwrite()


    public System.Collections.Specialized.StringCollection RetrieveListOfFilesInZipFile(string ZipFilePath)
    {
      System.Collections.Specialized.StringCollection fileList = null;

      try
      {
        //  Instantiate the List that will be used to store the file names so that they can be sent to the calling method.
        fileList = new System.Collections.Specialized.StringCollection();


        //  Retrieve the List the Files from the Zip Package.
        using (Ionic.Zip.ZipFile inputZipFile = Ionic.Zip.ZipFile.Read(ZipFilePath))
        {
          foreach (Ionic.Zip.ZipEntry currentEntry in inputZipFile)
          {
            fileList.Add(currentEntry.FileName);
          }

        }


        //  Return the list of file names to the calling method.
        return fileList;

      }
      catch (System.Exception caught)
      {
        //  Let the User know that this method failed.
        if (ErrorMessage != null)
        {
          ProcessMessage("The MaintTools.ZipFileManager.RetrieveListOfFilesInZipFile() Method failed with error message:  " + caught.Message);
        }

        //  Return NULL Pointer to the calling method to indicate that this process failed.
        return null;
      }
      finally
      {
        //  If the File List was instantiated, close it.
        if (fileList != null)
        {
          fileList = null;
        }

      }

    }   //  RetrieveListOfFilesInZipFile()

 
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
    ~ZipFileManager()
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


    //  Indicator of whether or not the Class has been Disposed of by the calling application.
    private System.Boolean                  _isDisposed                   = false;

    //  A Component Object that will be used to hold all Managed Resources for disposal.
    private System.ComponentModel.Component _component                    = null;

  }   //  CLASS:  ZipFileManager

}  // NAMESPACE:  PDX.BTS.DataMaintenance.MaintTools
