using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FileManager : MonoBehaviour
{         
    public delegate void TextEvent(string text);
    public static event TextEvent OnSetSource;
    public static event TextEvent OnSetDestination;
    public delegate void FileEvent();
    public static event FileEvent OnFilesInitialized;

    public static int NumberOfImages => _SourceFilePaths != null ? _SourceFilePaths.Count : 0;
    public static bool IsInitialized => _IsInitialized;
    static bool _IsInitialized = false;

    static readonly List<string> _ImageExtensions = new List<string> { ".JPG", ".JPEG", ".JPE", ".BMP", ".PNG" };

    static FileManager Instance;
    
    static string _SourcePath;
    static string _DestinationPath;

    static List<string> _SourceFilePaths;
    static string[] _DestinationFilePaths;

    static Texture _CurrentTexture;
    static int _CurrentID;
    
    
    //private void Awake()
    //{
    //    _IsInitialized = false;
    //    Instance = this;
    //    DontDestroyOnLoad(this);
    //}

    public static void SetSource(string path = null)
    {
        if (path == null || path == "")
        {
            _SourcePath = EditorUtility.OpenFolderPanel("Select Source Folder", "", "");
            OnSetSource?.Invoke(_SourcePath);
        }
        else
            _SourcePath = path;
    }

    public static void SetDestination()
    {
        _DestinationPath = EditorUtility.OpenFolderPanel("Select Destination Folder", "", "");
        OnSetDestination?.Invoke(_DestinationPath);
    }            

    public static void InitializeFiles()
    {
        _SourceFilePaths = Directory.GetFiles(_SourcePath).OfType<string>().ToList();
        //_DestinationFilenames = Directory.GetFiles(_DestinationPath);
                
        int numberOfFiles = NumberOfImages;
        for (int i = 0; i < NumberOfImages;)
        {
            string filePath = _SourceFilePaths[i];
            if (!File.Exists(filePath) || !_ImageExtensions.Contains(Path.GetExtension(filePath).ToUpperInvariant()))
                _SourceFilePaths.Remove(filePath);
            else
                i++;
        }
        
        if (NumberOfImages != 0)
        {
            OnFilesInitialized?.Invoke();
            _IsInitialized = true;
        }
        else
            Debug.LogWarning("No images in folder " + _SourcePath);
    }

    public static Texture LoadTexture(int id)
    {
        if (id > NumberOfImages) return null;

        Texture2D texture = new(0, 0);
        if (_IsInitialized)
            texture.LoadImage(File.ReadAllBytes(_SourceFilePaths[id]));
        else
            texture.LoadImage(File.ReadAllBytes("F:\\Projects\\Cropper\\TestFolder\\42069.png"));
        return texture;
    }

    //public static void StartLoadingTexture(int id)
    //{
    //    _CurrentID = id;
    //    Instance.StartCoroutine(GetTexture(_SourceFilePaths[id]));
    //}

    //static IEnumerator GetTexture(string path)
    //{
    //    UnityWebRequest web = UnityWebRequestTexture.GetTexture("file:///" + path);

    //    yield return web.SendWebRequest();

    //    if (web.result == UnityWebRequest.Result.Success)
    //        _CurrentTexture = ((DownloadHandlerTexture)web.downloadHandler).texture;

    //    Texture2D t;
    //    t.LoadImage
    //}

    //static void SelectFolders()
    //{
    //    string path = EditorUtility.OpenFolderPanel("Load png Textures", "", "");
    //    string[] files = Directory.GetFiles(path);

    //    //foreach (string file in files)
    //    //    if (file.EndsWith(".png"))
    //    //        File.Copy(file, EditorApplication.currentScene);
    //}
}
