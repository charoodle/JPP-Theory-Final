using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System.Text;

/// <summary>
/// Save and load a serializable object to disk into some filename, in JSON format.
/// </summary>
public static class JsonDataService
{
    /// <summary>
    /// Save an object to the user's persistent data path, in some filename you want.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj">The serializable object to save to a file in JSON format.</param>
    /// <param name="relativePath">Relative path to a game file contained in the persistent data path. Use "/" to indicate folder hierarchies. Make sure to include file extension too.
    ///     <para>Example: "/player_settings.json"</para>
    ///     </param>
    /// <param name="prettyIndent">Leave false, better processing(?). True for pretty file indent for debugging.</param>
    /// <returns>Filepath where it saved the object.</returns>
    public static string Save<T>(T obj, string relativePath, bool prettyIndent = false)
    {
        // Get file's path.
        string filepath = GetFilePath(relativePath);

        try
        {
            // Serialize the object into a string.
            Formatting formatting = prettyIndent ? Formatting.Indented : Formatting.None;
            string serialized = JsonConvert.SerializeObject(obj, formatting);

            // If file doesn't exist, create it.
            if (!File.Exists(filepath))
            {
                // Create file.
                FileStream filestream = File.Create(filepath);
                // Close filestream; using a different method of write instead of filestream.
                //  Alt method: convert to byte array with Encoding.ASCII.GetBytes & use filestream; but don't want to waste time doing that.
                filestream.Close();
            }

            // File exists now. Write string object to file.
            File.WriteAllText(filepath, serialized);

            // Return filepath.
            return filepath;
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
    /// <returns></returns>
    /// <inheritdoc cref="Save"/>
    public static T Load<T>(string relativePath)
    {
        // Read serialized object from filepath
        string filepath = GetFilePath(relativePath);

        try
        {
            // File must exist.
            if (!File.Exists(filepath))
                throw new System.Exception($"Cannot load. File does not exist at: {filepath}");

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

    /// <summary>
    /// Delete a file at the relative path.
    /// </summary>
    /// <returns></returns>
    /// <inheritdoc cref="Save"/>
    public static void DeleteFile(string relativePath)
    {
        try
        {
            string filepath = GetFilePath(relativePath);

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



    /// <summary>
    /// Combines the game's persistent data folder with the relative file path provided.
    /// </summary>
    /// <returns>Complete path of a game folder (persistent data path + relative).</returns>
    /// <inheritdoc cref="Save"/>
    private static string GetFilePath(string relativePath)
    {
        // Use persistent data path to get file path.
        return Application.persistentDataPath + relativePath;
    }
}
