using System;
using System.Collections.Generic;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("A353E472-6946-4c50-BD89-7374232CCBD3")]


  public class XMLSetttingsFileManager
  {
    public void XMLFile(string XMLFilePath)
    {

      //  Set the Path to the INI File.
      _filePath = XMLFilePath;

    }   //  XMLFile


    public string ReadUserSetting(string SectionName, string EntryName)
    {
      string settingValue = null;

      //  Set the Default Value to an empty string and call the overload of this method that
      //  requires that a default value to read the value from the Settings File.
      settingValue = ReadUserSetting(SectionName, EntryName, "");


      //  Return the value that was retrieved from the Settings File to the calling routine.
      return settingValue;

    }   //  ReadUserSetting


    public string ReadUserSetting(string SectionName, string EntryName, string DefaultValue)
    {

      //AMS.Profile.Xml xmlFile     = null;
      //object          objectValue = null;

      string section = SectionName;
      VerifyAndAdjustSection(ref section);
      string entry = EntryName;
      VerifyAndAdjustEntry(ref entry);

      try
      {
        //  Open the XML Document.
        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        doc.Load(_filePath);

        //  Retrieve the ROOT location in the XML Document.
        System.Xml.XmlElement root = doc.DocumentElement;


        System.Xml.XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
        return entryNode.InnerText;

      }
      catch
      {
        //  Return a NULL String to the calling routine to indicate that this process failed.
        return null;

      }

    }   //  readUserSetting


    public void WriteUserSetting(string SectionName, string EntryName, string EntryValue)
    {
      //  Make sure all of the necessary information was passed to this routine before attempting to write to
      //  the Settings File.
      if (SectionName == null || SectionName == "" || EntryName == null || EntryName == "" || EntryValue == "")
      {
        //  Exit the Method.
        return;
      }


      try
      {
        // If the value is null, remove the entry
        if (EntryValue == null)
        {
          RemoveUserSetting(SectionName, EntryValue);
          return;
        }

        //  Verify that the file is available for reading and that the XML Section and Entry Name values are valid.
        VerifyNotReadOnly();
        string section = SectionName;
        VerifyAndAdjustSection(ref section);
        string entry = EntryName;
        VerifyAndAdjustEntry(ref entry);


        //  If the File does not exist, create it.
        if (System.IO.File.Exists(_filePath))
        {
          System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(_filePath, System.Text.Encoding.UTF8);
          writer.Formatting = System.Xml.Formatting.Indented;
          writer.WriteStartDocument();
          writer.WriteStartElement("profile");
          writer.WriteStartElement("section");
          writer.WriteAttributeString("name", null, section);
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("name", null, entry);
          writer.WriteString(EntryValue);
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.Close();

          //  Exit the method now that the file has been created.
          return;

        }


        // The file exists, edit it
        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
        doc.Load(_filePath);
        System.Xml.XmlElement root = doc.DocumentElement;

        // Get the section element and add it if it's not there
        System.Xml.XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
        if (sectionNode == null)
        {
          System.Xml.XmlElement element = doc.CreateElement("section");
          System.Xml.XmlAttribute attribute = doc.CreateAttribute("name");
          attribute.Value = section;
          element.Attributes.Append(attribute);
          sectionNode = root.AppendChild(element);
        }

        // Get the entry element and add it if it's not there
        System.Xml.XmlNode entryNode = sectionNode.SelectSingleNode(GetEntryPath(entry));
        if (entryNode == null)
        {
          System.Xml.XmlElement element = doc.CreateElement("entry");
          System.Xml.XmlAttribute attribute = doc.CreateAttribute("name");
          attribute.Value = entry;
          element.Attributes.Append(attribute);
          entryNode = sectionNode.AppendChild(element);
        }

        // Add the value and save the file
        entryNode.InnerText = EntryValue;
        doc.Save(_filePath);

      }
      catch
      {
        //  Exit this Method.
        return;
      }

     }   //  WriteUserSetting


    /// <summary>
    ///   Removes an entry from a section. </summary>
    /// <param name="section">
    ///   The name of the section that holds the entry. </param>
    /// <param name="entry">
    ///   The name of the entry to remove. </param>
    /// <exception cref="InvalidOperationException">
    ///	  <see cref="Profile.Name" /> is null or empty or
    ///   <see cref="Profile.ReadOnly" /> is true. </exception>
    /// <exception cref="ArgumentNullException">
    ///   Either section or entry is null. </exception>
    /// <exception cref="XmlException">
    ///	  Parse error in the XML being loaded from the file or
    ///	  the resulting XML document would not be well formed. </exception>
    /// <remarks>
    ///   The <see cref="Profile.Changing" /> event is raised before removing the entry.  
    ///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
    ///   returns immediately without removing the entry.  After the entry has been removed, 
    ///   the <see cref="Profile.Changed" /> event is raised.
    ///   <para>
    ///   Note: If <see cref="XmlBased.Buffering" /> is enabled, the entry is not removed from the
    ///   XML file until the buffer is flushed (or closed). </para></remarks>
    /// <seealso cref="RemoveUserSection" />
    public void RemoveUserSetting(string section, string entry)
    {
      VerifyNotReadOnly();
      VerifyAndAdjustSection(ref section);
      VerifyAndAdjustEntry(ref entry);

      // Verify the document exists
      System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
      doc.Load(_filePath);
      if (doc == null)
        return;

      // Get the entry's node, if it exists
      System.Xml.XmlElement root = doc.DocumentElement;
      System.Xml.XmlNode entryNode = root.SelectSingleNode(GetSectionsPath(section) + "/" + GetEntryPath(entry));
      if (entryNode == null)
        return;


      entryNode.ParentNode.RemoveChild(entryNode);
      doc.Save(_filePath);

    }

    /// <summary>
    ///   Removes a section. </summary>
    /// <param name="section">
    ///   The name of the section to remove. </param>
    /// <exception cref="InvalidOperationException">
    ///	  <see cref="Profile.Name" /> is null or empty or
    ///   <see cref="Profile.ReadOnly" /> is true. </exception>
    /// <exception cref="ArgumentNullException">
    ///   section is null. </exception>
    /// <exception cref="XmlException">
    ///	  Parse error in the XML being loaded from the file or
    ///	  the resulting XML document would not be well formed. </exception>
    /// <remarks>
    ///   The <see cref="Profile.Changing" /> event is raised before removing the section.  
    ///   If its <see cref="ProfileChangingArgs.Cancel" /> property is set to true, this method 
    ///   returns immediately without removing the section.  After the section has been removed, 
    ///   the <see cref="Profile.Changed" /> event is raised.
    ///   <para>
    ///   Note: If <see cref="XmlBased.Buffering" /> is enabled, the section is not removed from the
    ///   XML file until the buffer is flushed (or closed). </para></remarks>
    /// <seealso cref="RemoveUserSetting" />
    public void RemoveUserSection(string section)
    {
      VerifyNotReadOnly();
      VerifyAndAdjustSection(ref section);

      // Verify the document exists
      System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
      doc.Load(_filePath);
      if (doc == null)
        return;

      // Get the root node, if it exists
      System.Xml.XmlElement root = doc.DocumentElement;
      if (root == null)
        return;

      // Get the section's node, if it exists
      System.Xml.XmlNode sectionNode = root.SelectSingleNode(GetSectionsPath(section));
      if (sectionNode == null)
        return;

      root.RemoveChild(sectionNode);
      doc.Save(_filePath);

    }


    public bool UserSectionExists(string SectionName)
    {
      //  Make sure a valid Section Name was passed to this Method before moving on.
      if (SectionName == null || SectionName == "")
      {
        //  Exit this Method.
        return false;
      }


      try
      {
        //  Retrieve the Array of Section Names from the XML Settings File.
        string[] sectionNames = GetSectionNames();


        //  Determine if the section exists.
        bool itExists = false;
        foreach(string currentSectionName in sectionNames)
        {
          if (System.String.Compare(currentSectionName, SectionName, System.StringComparison.CurrentCultureIgnoreCase) == 0)
          {
            //  The Section exists so set the itExists indicator to TRUE.
            itExists = true;
          }

        }


        //  Return the indicator of whether or not the section exists.
        return itExists;

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that the Section could not be found.
        return false;

      }

    }   //  UserSectionExists


    public bool UserSettingExists(string SectionName, string EntryName)
    {
      //  Make sure a valid Section Name and Entry Name were passed to this Method before moving on.
      if (SectionName == null || SectionName == "" || EntryName == null || EntryName == "")
      {
        //  Exit this Method.
        return false;

      }


      try
      {
        //  Retrieve the List of Entry Names in the specified Section in the XML Settings File.
        string[] sectionEntryNames = GetEntryNames(SectionName);


        //  Determine if the specified Entry Name is in the list of returned entry names.
        bool itExists = false;
        foreach (string currentEntryName in sectionEntryNames)
        {
          //  If the Current Entry Name matches the specified entry name, set the itExists indicator to TRUE.
          if (System.String.Compare(currentEntryName, EntryName, System.StringComparison.CurrentCultureIgnoreCase) == 0)
          {
            //  Set the Indicator to TRUE.
            itExists = true;
          }

        }


        //  Return the indicator of whether or not the entry name exists.
        return itExists;

      }
      catch
      {
        //  Return FALSE to the calling routine to indicate that the Entry could not be found in the Settings File.
        return false;

      }

    }   //  UserSettingExists


    /// <summary>
    ///   Retrieves the names of all the sections. </summary>
    /// <returns>
    ///   If the XML file exists, the return value is an array with the names of all the sections;
    ///   otherwise it's null. </returns>
    /// <exception cref="InvalidOperationException">
    ///	  <see cref="Profile.Name" /> is null or empty. </exception>
    /// <exception cref="XmlException">
    ///	  Parse error in the XML being loaded from the file. </exception>
    /// <seealso cref="Profile.HasSection" />
    /// <seealso cref="GetEntryNames" />
    public string[] GetSectionNames()
    {
      // Verify the document exists
      System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
      doc.Load(_filePath);

      if (doc == null)
        return null;

      // Get the root node, if it exists
      System.Xml.XmlElement root = doc.DocumentElement;
      if (root == null)
        return null;

      // Get the section nodes
      System.Xml.XmlNodeList sectionNodes = root.SelectNodes("section[@name]");
      if (sectionNodes == null)
        return null;

      // Add all section names to the string array			
      string[] sections = new string[sectionNodes.Count];
      int i = 0;

      foreach (System.Xml.XmlNode node in sectionNodes)
        sections[i++] = node.Attributes["name"].Value;

      //  Return the Array of Section Names to the calling method.
      return sections;

    }   //  GetSectionNames()


    /// <summary>
    ///   Retrieves the names of all the entries inside a section. </summary>
    /// <param name="section">
    ///   The name of the section holding the entries. </param>
    /// <returns>
    ///   If the section exists, the return value is an array with the names of its entries; 
    ///   otherwise it's null. </returns>
    /// <exception cref="InvalidOperationException">
    ///	  <see cref="Profile.Name" /> is null or empty. </exception>
    /// <exception cref="ArgumentNullException">
    ///   section is null. </exception>
    /// <exception cref="XmlException">
    ///	  Parse error in the XML being loaded from the file. </exception>
    /// <seealso cref="Profile.HasEntry" />
    /// <seealso cref="GetSectionNames" />
    public string[] GetEntryNames(string section)
    {
      // Verify the section exists
      if (!UserSectionExists(section))
        return null;

      VerifyAndAdjustSection(ref section);

      System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
      doc.Load(_filePath);

      if (doc == null)
        return null;

      // Get the root node, if it exists
      System.Xml.XmlElement root = doc.DocumentElement;
      if (root == null)
        return null;


      // Get the entry nodes
      System.Xml.XmlNodeList entryNodes = root.SelectNodes(GetSectionsPath(section) + "/entry[@name]");
      if (entryNodes == null)
        return null;


      // Add all entry names to the string array			
      string[] entries = new string[entryNodes.Count];
      int i = 0;

      foreach (System.Xml.XmlNode node in entryNodes)
        entries[i++] = node.Attributes["name"].Value;


      //  Return the Array of Entry Names to the calling method.
      return entries;

    }   //  GetEntryNames()


    public string XMLSettingsFile
    {
      get
      {
        return _filePath;
      }
      set
      {
        _filePath = value;
      }

    }   //  XMLSettingsFile - Property
      

    /// <summary>
    ///   Retrieves the XPath string used for retrieving a section from the XML file. </summary>
    /// <returns>
    ///   An XPath string. </returns>
    /// <seealso cref="GetEntryPath" />
    private string GetSectionsPath(string section)
    {
      return "section[@name=\"" + section + "\"]";
    }


    /// <summary>
    ///   Retrieves the XPath string used for retrieving an entry from the XML file. </summary>
    /// <returns>
    ///   An XPath string. </returns>
    /// <seealso cref="GetSectionsPath" />
    private string GetEntryPath(string entry)
    {
      return "entry[@name=\"" + entry + "\"]";
    }


    /// <summary>
    ///   Verifies the given section name is not null and trims it. </summary>
    /// <param name="section">
    ///   The section name to verify and adjust. </param>
    /// <exception cref="ArgumentNullException">
    ///   section is null. </exception>
    /// <remarks>
    ///   This method may be used by derived classes to make sure that a valid
    ///   section name has been passed, and to make any necessary adjustments to it
    ///   before passing it to the corresponding APIs. </remarks>
    /// <seealso cref="VerifyAndAdjustEntry" />
    protected void VerifyAndAdjustSection(ref string section)
    {
      if (section == null)
        throw new ArgumentNullException("section");

      section = section.Trim();
    }


    /// <summary>
    ///   Verifies the given entry name is not null and trims it. </summary>
    /// <param name="entry">
    ///   The entry name to verify and adjust. </param>
    /// <remarks>
    ///   This method may be used by derived classes to make sure that a valid
    ///   entry name has been passed, and to make any necessary adjustments to it
    ///   before passing it to the corresponding APIs. </remarks>
    /// <exception cref="ArgumentNullException">
    ///   entry is null. </exception>
    /// <seealso cref="VerifyAndAdjustSection" />
    protected void VerifyAndAdjustEntry(ref string entry)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");

      entry = entry.Trim();
    }


    /// <summary>
    ///   Verifies the ReadOnly property is not true. </summary>
    /// <remarks>
    ///   This method may be used by derived classes as a convenient way to 
    ///   validate that modifications to the profile can be made. </remarks>
    /// <exception cref="InvalidOperationException">
    ///   ReadOnly is true. </exception>
    /// <seealso cref="ReadOnly" />
    private void VerifyNotReadOnly()
    {
      System.IO.FileInfo xmlFileInfo = new System.IO.FileInfo(_filePath);

      if (xmlFileInfo.IsReadOnly)
        throw new InvalidOperationException("Operation not allowed because ReadOnly property is true.");
    }

    //  Define a Field to hold the Path and File Name of the XML File.
    string _filePath = null;

  }   //  Class:  XMLSettingsFileManager

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools
