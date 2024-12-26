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

    /// <summary>
    /// Load an object from a json formatted file.
    /// </summary>
    /// <typeparam name="T">Object type to load the json file into.</typeparam>
    /// <inheritdoc cref="Save"/>
    /// <returns></returns>
    public static T Load<T>(string relativePath)
    {
        // Read serialized object from filepath
        string filepath = Application.persistentDataPath + relativePath;

        try
        {
            // File must exist.
            if (!File.Exists(filepath))
                throw new FileNotFoundException();
                //throw new System.Exception($"Cannot load. File does not exist at: {filepath}");

            // Read file at path.
            string serialized = File.ReadAllText(filepath);

            // Turn it into an object.
            T obj = JsonConvert.DeserializeObject<T>(serialized);

            // Return the loaded object.
            return obj;
        }
        // Something went wrong trying to read object from user's file.
        catch(System.Exception e)
        {
            throw e;
        }
    }

    public static void DeleteFile(string relativePath)
    {
        try
        {
            string filepath = Application.persistentDataPath + relativePath;

            // File must exist
            if (!File.Exists(filepath))
                throw new System.Exception($"Cannot delete. File at {filepath} doesn't exist!");

            // Delete the file
            File.Delete(filepath);
        }
        catch(System.Exception e)
        {
            // Something went wrong while deleting the file.
            throw e;
        }
    }

    //protected string GetFilePath(string relativePath)
    //{
    //    // Use persistent data path to get file path.
    //    return Application.persistentDataPath + relativePath;
    //}
}
