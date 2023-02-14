using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Ookii.Dialogs;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public static class FileManager
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
    
    static string _SourcePath;
    static string _DestinationPath;

    static List<string> _SourceFilePaths;
    //static string[] _DestinationFilePaths;

    static int _ImagesAtDestination;
    static string _persistentPath;

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    public static void SetSource(string path = null)
    {
        if (path == null || path == "")
        {
            _SourcePath = SelectFolderPath("Select Source Folder");
            OnSetSource?.Invoke(_SourcePath);
        }
        else
            _SourcePath = path;
    }

    public static void SetDestination(string path = null)
    {
        if (path == null || path == "")
        {
            _DestinationPath = SelectFolderPath("Select Destination Folder");
            OnSetDestination?.Invoke(_DestinationPath);
        }
        else
            _DestinationPath = path;
    }

    static string SelectFolderPath(string title)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            UseDescriptionForTitle = true,
            Description = title
        };

        if (dialog.ShowDialog(new WindowWrapper(GetActiveWindow())) == DialogResult.OK)
        {
            return dialog.SelectedPath;
        }
        return null;
    }

    public static void InitializeFiles()
    {
        _SourceFilePaths = Directory.GetFiles(_SourcePath).OfType<string>().ToList();
        _ImagesAtDestination = Directory.GetFiles(_DestinationPath).Length;
                
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

        _persistentPath = UnityEngine.Application.persistentDataPath;
        //Debug.Log(_persistentPath);
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

    public static void SaveAsPNG(Texture2D sourceTexture)
    {            
        byte[] pngBytes = sourceTexture.EncodeToPNG();
        File.WriteAllBytes($"{_DestinationPath}\\test{++_ImagesAtDestination}.png", pngBytes);
    }

    public static void SaveAsPNG(byte[] pngBytes)
    {
        var path = $"{_DestinationPath}\\{++_ImagesAtDestination}.png";

        using (var fileStream = File.Create(path))
        {
            fileStream.Write(pngBytes);
        }
    }
}

class WindowWrapper : IWin32Window
{
    public WindowWrapper(IntPtr handle)
    {
        _hwnd = handle;
    }

    public IntPtr Handle
    {
        get { return _hwnd; }
    }

    private IntPtr _hwnd;
}
