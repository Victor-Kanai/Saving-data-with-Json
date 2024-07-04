using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class JsonDataService : IDataService
{

    private readonly string KEY = "";
    private readonly string IV = "";

    public bool SaveData<T>(string relativePath, T data)
    {
        string path = Application.persistentDataPath + relativePath;

            try
            {
                if (File.Exists(path))
                {
                    Debug.Log($"The file: {path}; exists, deleting it and creating a new one!");
                    File.Delete(path);
                }  
                using FileStream stream = File.Create(path);
                WriteEncryptedData(data, stream);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"We couldnt do it because of: {e.Message} {e.StackTrace}");
                return false;
            }
    }

    private void WriteEncryptedData<T>(T data, FileStream stream)
    {
        using Aes aesProvider = Aes.Create();
        aesProvider.Key = Convert.FromBase64String(KEY);
        aesProvider.IV = Convert.FromBase64String(IV);
        using ICryptoTransform cryptoTransform = aesProvider.CreateEncryptor();
        using CryptoStream cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Write);
        
        //Uncomment this to recieve your Key and IV
        //Debug.Log($"Key: {Convert.ToBase64String(aesProvider.Key)}");
        //Debug.Log($"IV: {Convert.ToBase64String(aesProvider.IV)}");

        cryptoStream.Write(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data)));
    }

    public T LoadData<T>(string relativePath)
    {
        string path = Application.persistentDataPath + relativePath;
        if(!File.Exists(path))
        {
            Debug.LogError($"The file {path} does not exist!");
            throw new FileNotFoundException($"Path {path} does not exist");
        }
        try
        {
            T data;

            data = ReadEncryptedData<T>(path);

            return data;
        }
        catch(Exception e)
        {
            Debug.LogError($"We couldn't load it because of: {e.Message} {e.StackTrace}");
            throw e;
        }
    }

    private T ReadEncryptedData<T>(string path)
    {
        byte[] fileBytes = File.ReadAllBytes(path);
        using Aes aesProvider = Aes.Create();

        aesProvider.Key = Convert.FromBase64String(KEY);
        aesProvider.IV= Convert.FromBase64String(IV);

        using ICryptoTransform cryptoTransform = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV);

        using MemoryStream decrytorStream = new MemoryStream(fileBytes);
        using CryptoStream cryptoStream = new CryptoStream(decrytorStream, cryptoTransform, CryptoStreamMode.Read);
        using StreamReader reader = new StreamReader(cryptoStream);

        string result = reader.ReadToEnd();

        return JsonConvert.DeserializeObject<T>(result);
    }

    public void DeleteData(string relativePath)
    {
        string path = Application.persistentDataPath + relativePath;
        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"We couldn't do it because of: {e.Message} {e.StackTrace}");
        }
    }
}
