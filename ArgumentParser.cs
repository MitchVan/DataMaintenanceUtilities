using System;
using System.Collections.Generic;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{
  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("c8560fad-b225-49ab-b9e1-ca59b4da7cb2")]


  public class ArgumentParser
  {
    /// <summary>
    ///    Arguments Parser Class.
    ///      --  Parses Command Line Arguments passed to a Command Line application.
    ///      --  Accepts the string array of arguments that was captured by the application
    ///          at startup.
    ///      --  Parameters should be prefixed with one of the following characters ("-",
    ///          "--", "/", ":").
    ///      --  Calling Syntax:  PDX.BTS.DataMaintenanceUtilites.ArgumentParser commandLine = new PDX.BTS.DataMaintenanceUtilities.ArgumentParser(args);
    ///            -  Returns a String Array of the Parameters with the parameter names as
    ///               the array indices.
    ///            -  For example in the resulting array, the value for the Parameter
    ///               "DataPath" (passed as /DataPath=C:\) will be obtained with -
    ///               commandLine["DataPath"].
    ///            -  For Boolean arguments.  To determine if the parameter was passed is -
    ///               "Usage" (passed as /Usage) will be determined by testing -
    ///               if (commandLine["Usage"] == "true")
    /// </summary>
    // Constructor Method
    public ArgumentParser(string[] Args)
    {
      System.Text.RegularExpressions.Regex splitter         = null;
      System.Text.RegularExpressions.Regex remover          = null;
      string                               currentParameter = null;
      string[]                             parameterParts   = null;

      try
      {
        //  Instantiate a String Dictionary Object to hold the parameters that are found.
        _parsedParameters = new System.Collections.Specialized.StringDictionary();


        //  Set the Set of values that will be searched to find the parameter start
        //  identifiers ("-", "--", "/", or ":").
        splitter = new System.Text.RegularExpressions.Regex(@"^-{1,2}|^/|=|:",
                                  System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                                  System.Text.RegularExpressions.RegexOptions.Compiled);


        //  Set the Set of values that will be removed from the Parameters strings ("'", ".*").
        remover = new System.Text.RegularExpressions.Regex(@"^['""]?(.*?)['""]?$",
                                  System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                                  System.Text.RegularExpressions.RegexOptions.Compiled);


        // Valid parameters forms:  {-,/,--}param{ ,=,:}((",')value(",'))
        // Examples:  -param1 value1 --param2 /param3:"Test-:-work"
        //           /param4=happy -param5 '--=nice=--'
        foreach (string currentTextString in Args)
        {
          // Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
          parameterParts = splitter.Split(currentTextString, 3);

          //  Populate the String Dictionary Object with the values in the current parameter.
          switch (parameterParts.Length)
          {
            // Found a value (for the last parameter found (space separator))
            case 1:
              if (currentParameter != null)
              {
                if (!_parsedParameters.ContainsKey(currentParameter))
                {
                  parameterParts[0] = remover.Replace(parameterParts[0], "$1");
                  _parsedParameters.Add(currentParameter, parameterParts[0]);
                }
                currentParameter = null;
              }
              // else Error: no parameter waiting for a value (skipped)
              break;
            // Found just a parameter
            case 2:
              // The last parameter is still waiting.  With no value, set it to true.
              if (currentParameter != null)
              {
                if (!_parsedParameters.ContainsKey(currentParameter))
                {
                  _parsedParameters.Add(currentParameter, "true");
                }
              }
              currentParameter = parameterParts[1];
              //  Exit this case.
              break;
            // Parameter with enclosed value
            case 3:
              // The last parameter is still waiting.  With no value, set it to true.
              if (currentParameter != null)
              {
                if (!_parsedParameters.ContainsKey(currentParameter))
                {
                  _parsedParameters.Add(currentParameter, "true");
                }
              }
              currentParameter = parameterParts[1];
              // Remove possible enclosing characters (",')
              if (!_parsedParameters.ContainsKey(currentParameter))
              {
                parameterParts[2] = remover.Replace(parameterParts[2], "$1");
                _parsedParameters.Add(currentParameter, parameterParts[2]);
              }
              currentParameter = null;
              //  Exit this case.
              break;
          }
        }


        // In case a parameter is still waiting
        if (currentParameter != null)
        {
          if (!_parsedParameters.ContainsKey(currentParameter))
          {
            _parsedParameters.Add(currentParameter, "true");
          }

        }

      }
      catch
      {
        //  Exit the method.
        return;

      }

    }   //  ArgumentParser()


    // Retrieve a parameter value if it exists (overriding C# indexer property).
    public string this[string Parameter]
    {
      get
      {
        return (_parsedParameters[Parameter]);
      }

    }   //  Parameter Get.

    //  A String Dictionary Object to hold the Parameters.
    private System.Collections.Specialized.StringDictionary _parsedParameters = null;

  }   // CLASS:  ArgumentParser

}   //  NAMESPACE:  PDX.BTS.DataMaintenance.MaintTools
