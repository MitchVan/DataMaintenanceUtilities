using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PDX.BTS.DataMaintenance.MaintTools
{

  //  Set the GUID that will be used when this Library is referenced by other applications.  Specifying a 
  //  GUID insures that the library will maintain binary compatibility through all build operations.
  [System.Runtime.InteropServices.Guid("0F9701AD-87EA-4bc1-B88F-D5C19E7E5AD7")]


  public class EncryptionManager
  {

    //  The Constructor Method.
		public EncryptionManager()
		{

      //  Set the Initialization String that will be used for all encryptions and decryptions.
			intializationVectorBytes = System.Text.Encoding.ASCII.GetBytes("@1B2c3D4e5F6g7H8");
 
		}


    		/// <summary>
		/// Takes an input Text String and produces an encrypted string using the combination of the input
		/// and default the Encryption Key or password (sombra1) and running them through a Rjindael Encryption
		/// Algorithm.
		/// </summary>
		/// <param name="stringValue">
		/// The input text string that is to be encrypted.
		/// </param>
		/// <returns>
		/// An encrypted string produced by running the input text string and password through a Rjindael
		/// encryption algorithm.
		/// </returns>
		public string EncryptString(string StringValue)
		{
			string encryptedString;

			//  Encrypt the string using "SOMBRA1" as the Encryption Key.
			encryptedString = EncryptString(StringValue, "sombra1");


			//  Return the Encrypted String to the calling routine.
			return encryptedString;

		}   //  EncryptString


		/// <summary>
		/// Takes an input Text String and an Encryption Key (password) and produces an encrypted string using
		/// the combination of the inputs and running them through a Rjindael Decryption algorithm.
		/// </summary>
		/// <param name="stringValue">
		/// The input text string that is to be encrypted.
		/// </param>
		/// <param name="encryptionKey">
		/// The user specified Encryption Key or password that is used to encrypt the Input Text String.
		/// </param>
		/// <returns>
		/// An encrypted string produced by running the input text string and password through a Rjindael
		/// encryption algorithm.
		/// </returns>
		public string EncryptString(string StringValue, string EncryptionKey)
		{
			string encryptedData = null;

      //  Encrypt the Input String.
      encryptedData = Encrypt(StringValue, EncryptionKey);


			//  Now return the resulting encrypted string.
			return encryptedData;

		}   //  EncryptString


		/// <summary>
		/// Takes an input Text String and produces an encrypted string using the combination of the input and the
		/// default Encryption Key or password (sombra1) and running them through a Rjindael Decryption algorithm.
		/// </summary>
		/// <param name="stringValue">
		/// The input encrypted string that will be decrypted.
		/// </param>
		/// <returns>
		/// The decrypted original text string that was used to create the input encrypted string.
		/// </returns>
		public string DecryptString(string StringValue)
		{

			string decryptedString;

			//  Decrypt the string using "SOMBRA1" as the Encryption Key.
			decryptedString = DecryptString(StringValue, "sombra1");


			//  Return the Decrypted String to the calling routine.
			return decryptedString;

		}   //  DecryptString


		/// <summary>
		/// Takes an input encrypted string (encrypted using the Rjindael routine) and the Encryption Key (password)
		/// that was used to encrypt the original text string and decrypts the input information to produce the
		/// original text string that was encrypted.
		/// </summary>
		/// <param name="stringValue">
		/// The input encrypted string that will be decrypted.
		/// </param>
		/// <param name="encryptionKey">
		/// The user specified Encryption Key or password that is used to decrypt the Input Text String.
		/// </param>
		/// <returns>
		/// The decrypted original text string that was used to create the input encrypted string.
		/// </returns>
		public string DecryptString(string StringValue, string EncryptionKey)
		{
			string decryptedData = null;

			//  Get the Key/IV and do the decryption using the function taht accepts byte arrays.
      decryptedData = Decrypt(StringValue, EncryptionKey);


			//  Turn the resultant Byte Array into a string and return the string to the calling routine.
			return decryptedData;

		}   //  DecryptString


		/// <summary>
		/// Takes an Input Byte Array, a Key Byte Array and and IV Byte Array and produces an output encrypted
		/// Byte Array after running the input array through a Rjindael Encryption algorithm using the Key and
		/// IV as parameters.
		/// </summary>
		/// <param name="clearData">
		/// Input Byte Array that is to be encrypted.
		/// </param>
		/// <param name="key">
		/// The first 32 bytes of the encryption key or password.
		/// </param>
		/// <param name="IV">
		/// Initialization Vector used by the encryption algorithm operating in its default mode called CBS
		/// (Cipher Block Chaining).  The IV is XOR'ed with the first block (8 byte) of the data before it is
		/// encrypted and then each encrypted block is XOR'ed with the following block of plaintext.
		/// </param>
		/// <returns>
		/// An Encrypted Byte Array produced by running the input Byte Array through a Rjindael Algorithm using the
		/// Key and IV as parameters.
		/// </returns>
    private static string Encrypt(string InputString, string EncryptionKey)
    {
      System.Configuration.AppSettingsReader settingsReader;
      MD5CryptoServiceProvider               hashmd5        = null;
      byte[]                                 keyArray;
      TripleDESCryptoServiceProvider         tdes           = null;
      ICryptoTransform                       cTransform     = null;
      byte[]                                 toEncryptArray;
      byte[]                                 resultArray;

      settingsReader = new System.Configuration.AppSettingsReader();
      // Get the key from config file

      //key = (string)settingsReader.GetValue(EncryptionKey, typeof(String));

      //  Get hashcode regards to your key
      hashmd5 = new MD5CryptoServiceProvider();
      keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
      
      
      //Always release the resources and flush data of the Cryptographic service provide. Best Practice
      hashmd5.Clear();
      

      tdes = new TripleDESCryptoServiceProvider();
      //set the secret key for the tripleDES algorithm
      tdes.Key = keyArray;
      //mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
      tdes.Mode = CipherMode.ECB;
      //padding mode(if any extra byte added)

      tdes.Padding = PaddingMode.PKCS7;

      cTransform = tdes.CreateEncryptor();

      toEncryptArray = UTF8Encoding.UTF8.GetBytes(InputString);

      //transform the specified region of bytes array to resultArray
      resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
      
      //Release resources held by TripleDes Encryptor
      tdes.Clear();
      
      
      //Return the encrypted data into unreadable string format
      return Convert.ToBase64String(resultArray, 0, resultArray.Length);

    }
    //private static byte[] Encrypt(byte[] ClearData, byte[] Key, byte[] IV)
    //{
    //  MemoryStream memoryStream;
    //  Rijndael     algorithm;
    //  CryptoStream cryptographyStream;
    //  byte[]       encryptedData;

    //  //  Create a Memory Stream that to accept the encrypted data.
    //  memoryStream = new MemoryStream();


    //  //  Create a Rijndael Symmetric Algorithm.
    //  algorithm = Rijndael.Create();
    //  algorithm.Padding = System.Security.Cryptography.PaddingMode.None;


    //  //  Set the algorithm Key and IV.
    //  algorithm.Key = Key;
    //  algorithm.IV = IV;


    //  //  Create a Crypto Stream to pump the data through.  CryptoStream.Write means that data is going to
    //  //  be written to the the stream and the output will be written in the Memory Stream.
    //  cryptographyStream = new CryptoStream(memoryStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);


    //  //  Write the data and perform the encryption.
    //  cryptographyStream.Write(ClearData, 0, ClearData.Length);


    //  //  Close the Crypto Stream.  This will cause the Stream to apply padding and finalize the encryption
    //  //  process.
    //  cryptographyStream.Close();


    //  //  Get the encrypted data from the Memory Stream.
    //  encryptedData = memoryStream.ToArray();


    //  //  Return the encrypted Byte Array to the calling routine.
    //  return encryptedData;

    //}   //  Encrypt


		/// <summary>
		/// Takes an Input Ecrypted Byte Array, a Key Byte Array and and IV Byte Array and produces an output 
		/// Decrypted Byte Array after running the input array through a Rjindael Decryption Algorithm using the
		/// Key and IV as parameters.
		/// </summary>
		/// <param name="cipherData">
		/// Input Encrypted Byte Array that will be decrypted.
		/// </param>
		/// <param name="key">
		/// The first 32 bytes of the encryption key or password.
		/// </param>
		/// <param name="IV">
		/// Initialization Vector used by the decryption algorithm operating in its default mode called CBS
		/// (Cipher Block Chaining).  The IV is XOR'ed with the first block (8 byte) of the data before it is
		/// encrypted and then each encrypted block is XOR'ed with the following block of plaintext.
		/// </param>
		/// <returns>
		/// An Decrypted Byte Array produced by running the input EncryptedByte Array through a Rjindael Algorithm
		/// using the Key and IV as parameters.
		/// </returns>
    private static string Decrypt(string EncryptedString, string EncryptionKey)
    {
      byte[]                                 toEncryptArray;
      System.Configuration.AppSettingsReader settingsReader = null;
      MD5CryptoServiceProvider               hashmd5        = null;
      byte[]                                 keyArray;
      TripleDESCryptoServiceProvider         tdes           = null;
      ICryptoTransform                       cTransform     = null;
      byte[]                                 resultArray;

      //  If the String to Decrypt is an empty string return an empty string to the calling routine.
      if (EncryptedString == "")
      {
        return "";
      }


      //get the byte code of the string
      toEncryptArray = Convert.FromBase64String(EncryptedString);


      settingsReader = new System.Configuration.AppSettingsReader();


      // Get the hash code with regards to your key
      hashmd5 = new MD5CryptoServiceProvider();
      keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
      
      //release any resource held by the MD5CryptoServiceProvider
      hashmd5.Clear();


      tdes = new TripleDESCryptoServiceProvider();
      //set the secret key for the tripleDES algorithm
      tdes.Key = keyArray;
      //mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)

      tdes.Mode = CipherMode.ECB;
      //padding mode(if any extra byte added)
      tdes.Padding = PaddingMode.PKCS7;

      cTransform = tdes.CreateDecryptor();
      resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
      
      //Release resources held by TripleDes Encryptor                
      tdes.Clear();
      
      //return the Clear decrypted TEXT
      return UTF8Encoding.UTF8.GetString(resultArray);

    }
    //private static byte[] Decrypt(byte[] CipherData, byte[] Key, byte[] IV)
    //{

    //  MemoryStream memoryStream;
    //  Rijndael     algorithm;
    //  CryptoStream cryptographyStream;
    //  byte[]       decryptedData;

    //  //  Create a Memory Stream to accept the decrypted bytes.
    //  memoryStream = new MemoryStream();


    //  //  Create a Rijndael Symmetric Algorithm.
    //  algorithm = Rijndael.Create();
    //  algorithm.Padding = System.Security.Cryptography.PaddingMode.None;


    //  //  Set the algorithm Key and IV.
    //  algorithm.Key = Key;
    //  algorithm.IV = IV;


    //  //  Create a Crypto Stream to pump the data through.  CryptoStream.Write means that data is going to
    //  //  be written to the the stream and the output will be written in the Memory Stream.
    //  cryptographyStream = new CryptoStream(memoryStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write);


    //  //  Write the data and perform the decryption.
    //  cryptographyStream.Write(CipherData, 0, CipherData.Length);


    //  //  Close the Crypto Stream.  This will cause the Stream to apply padding and finalize the encryption
    //  //  process.
    //  cryptographyStream.FlushFinalBlock();
    //  cryptographyStream.Close();


    //  //  Get the decrypted data from the Memory Stream.
    //  decryptedData = memoryStream.ToArray();


    //  //  Return the Decrypted Byte Array to the calling routine.
    //  return decryptedData;

    //}   //  Decrypt
	
		//  Specify an Initialization Vector that will be used for all Encryptions and Decryptions.

		static byte[] intializationVectorBytes = null;

  }   //  Class:  EncryptionManager

}   //  NameSpace:  PDX.BTS.DataMaintenance.MaintTools
