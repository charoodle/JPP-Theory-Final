using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System.Text;

/// <summary>
/// Save and load a serializable to disk.
/// </summary>
public static class JsonDataService
{
    /// <summary>
    /// Save an object to the user's persistent data path, in some filename you want.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj">The serializable object to save to a file in JSON format.</param>
    /// <param name="relativePath">Use "\" to indicate folder hierarchy. Make sure to include file extension too.
    ///     <para>Example: "\player_settings.json"</para>
    ///     </param>
    /// <param name="prettyIndent">Leave false, better processing(?). True for pretty file indent for debugging.</param>
    /// <returns>Filepath where it saved the object.</returns>
    public static string Save<T>(T obj, string relativePath, bool prettyIndent = false)
    {
        // Get file's path.
        string filepath = Application.persistentDataPath + relativePath;

        try
        {
            // Serialize the object into a string.
            Formatting formatting = prettyIndent ? Formatting.Indented : Formatting.None;
            string serialized = JsonConvert.SerializeObject(obj, formatting);

            // If file doesn't exist, create it.
            if (!File.Exists(filepath))
            {
                Debug.LogWarning("File doesn't exist, creating a new file.");

                FileStream filestream = File.Create(filepath);

                // Filestream works in byte arrays.
                byte[] bytes = Encoding.ASCII.GetBytes(serialized);

                // Write byte object to file.
                filestream.Write(bytes);

                // Close filestream.
                filestream.Close();

                // Return filepath.
                return filepath;
            }
            // Else if file exists, overwrite it.
            else
            {
                // Write string object to file.
                File.WriteAllText(filepath, serialized);

                // Return filepath.
                return filepath;
            }
        }
        // Something went wrong trying to save object into user's file.
        catch(System.Exception e)
        {
            throw e;
        }
    }

    public static T Load<T>(string relativePath)
    {
        throw new System.NotImplementedException();
    }

    public static void DeleteFile(string relativePath)
    {
        throw new System.NotImplementedException();
    }

    //protected string GetFilePath(string relativePath)
    //{
    //    // Use persistent data path to get file path.
    //    return Application.persistentDataPath + relativePath;
    //}
}
