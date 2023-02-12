using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using Unity;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CroppingUIManager : MonoBehaviour
{
    [SerializeField] Settings _settings_;
    [SerializeField] ImageViewer _image_;
    [SerializeField] Canvas _canvas_;
    [SerializeField] SelectionFrame _framePrefab_;
    [Space]
    [SerializeField] TextMeshProUGUI _counterText_;
    [SerializeField] TextMeshProUGUI _ratiosText_;

    SelectionFrame _activeFrame;
    float _scalingSpeed = 0.1f;
    List<SelectionFrame> _setFrames;

    int _imageID;
    int _numberOfSourceImages;

    private void Start()
    {
        if (!FileManager.IsInitialized)
        {
            FileManager.SetSource("F:\\Projects\\Cropper\\TestFolder\\bck");
            FileManager.InitializeFiles();
        }
        _imageID = 0;
        _numberOfSourceImages = FileManager.NumberOfImages;

        
        _activeFrame = Instantiate(_framePrefab_);
        _activeFrame.SetParent(_canvas_.transform);
        _activeFrame.DimensionsInScreenSpace = new(_settings_.TargetWidth, _settings_.TargetHeight);

        _setFrames = new();

        DisplayImage(_imageID);
    }

    private void Update()
    {
        ScrollThroughImages();
        ScaleSelectionFrame();
        MoveSelectionFrame();
    }

    void ScrollThroughImages()
    {
        if (Input.GetKeyDown(_settings_.Key_NextImage))
        {
            _imageID = (_imageID + 1) % _numberOfSourceImages;
            DisplayImage(_imageID);
        }
        if (Input.GetKeyDown(_settings_.Key_PreviousImage))
        {
            _imageID = (_imageID - 1 + _numberOfSourceImages) % _numberOfSourceImages;
            DisplayImage(_imageID);
        }
    }

    void ScaleSelectionFrame()
    {
        if (Input.mouseScrollDelta.y == 0 || Input.GetKey(_settings_.Key_RescaleImage)) return;

        float desiredScale = _activeFrame.DimensionsInScreenSpace.x / (_settings_.TargetWidth * _image_.CurrentScale);

        if (Input.GetKey(_settings_.Key_SlowScrolling))
            desiredScale += Input.mouseScrollDelta.y * _scalingSpeed / _image_.CurrentScale / 10;
        else
            desiredScale += Input.mouseScrollDelta.y * _scalingSpeed / _image_.CurrentScale;

        RescaleFrameRelativeToTarget(desiredScale, Input.GetKey(_settings_.Key_ExceedConstrains));
    }

    void ScaleImage()
    {
        if (Input.mouseScrollDelta.y == 0 || !Input.GetKey(_settings_.Key_RescaleImage)) return;


    }

    void MoveSelectionFrame()
    {
        Vector2 position = Input.mousePosition;

        if (!Input.GetKey(_settings_.Key_ExceedConstrains))
           position = new(Mathf.Clamp(position.x, _image_.BottomLeftInScreenSpace.x + _activeFrame.DimensionsInScreenSpace.x / 2, _image_.UpperRightInScreenSpace.x - _activeFrame.DimensionsInScreenSpace.x / 2),
                          Mathf.Clamp(position.y, _image_.BottomLeftInScreenSpace.y + _activeFrame.DimensionsInScreenSpace.y / 2, _image_.UpperRightInScreenSpace.y - _activeFrame.DimensionsInScreenSpace.y / 2));
       
        position = new(Mathf.Clamp(position.x, _activeFrame.DimensionsInScreenSpace.x / 2, _canvas_.pixelRect.width - _activeFrame.DimensionsInScreenSpace.x / 2),
                       Mathf.Clamp(position.y, _activeFrame.DimensionsInScreenSpace.y / 2, _canvas_.pixelRect.height - _activeFrame.DimensionsInScreenSpace.y / 2));
        
        _activeFrame.Position = new(position.x, position.y);
    }

    void SetSelection()
    {
        if (!Input.GetKeyDown(_settings_.Key_SetSelection)) return;


    }

    void DisplayImage(int id)
    {
        _counterText_.text = $"{id + 1} / {_numberOfSourceImages}";
        if (id < _numberOfSourceImages)
        {
            _image_.DisplayNewTexture(FileManager.LoadTexture(id));
            _activeFrame.DimensionsInScreenSpace = new(_image_.CurrentScale * _settings_.TargetWidth, _image_.CurrentScale * _settings_.TargetHeight);
            UpdateRatiosText();
        }
    }

    void UpdateRatiosText()
    {
        Vector2 pixelScale = new(Mathf.Round(_activeFrame.DimensionsInScreenSpace.x / _image_.CurrentScale), Mathf.Round(_activeFrame.DimensionsInScreenSpace.y / _image_.CurrentScale));
        Vector2 screenScale = new(Mathf.Round(_activeFrame.DimensionsInScreenSpace.x) /* 10)/10f*/, Mathf.Round(_activeFrame.DimensionsInScreenSpace.y) /* 10)/10f*/);
        _ratiosText_.text = $"Image scale: {_image_.CurrentScale}\nFrame\nin image pixels: {pixelScale.x}/{pixelScale.y}\n" +
                $"in screen pixels: {screenScale.x}/{screenScale.y}\n" +
                $"ratio {_activeFrame.DimensionsInScreenSpace.x / _activeFrame.DimensionsInScreenSpace.y}. scale to target: {Mathf.Round(pixelScale.x * 100 / _settings_.TargetWidth) / 100f}";
    }

    void RescaleFrameRelativeToTarget(float scale, bool exceedConstrains)
    {
        float desiredWidth = _settings_.TargetWidth * scale;
        float desiredHeight = _settings_.TargetHeight * scale;

        bool restricted = !Input.GetKey(_settings_.Key_ExceedConstrains);

        float newWidth = Mathf.Clamp(desiredWidth, restricted ? _settings_.TargetWidth : 1, restricted ? _image_.ImageSize.x : _canvas_.pixelRect.width / _image_.CurrentScale);
        float newHeight = Mathf.Clamp(desiredHeight, restricted ? _settings_.TargetHeight : 1, restricted ? _image_.ImageSize.y : _canvas_.pixelRect.height / _image_.CurrentScale);

        if (newWidth != desiredWidth)
            newHeight = newWidth * _settings_.TargetHeight / _settings_.TargetWidth;
        if (newHeight != desiredHeight)
            newWidth = newHeight * _settings_.TargetWidth / _settings_.TargetHeight;
        
        newHeight = newWidth * _settings_.TargetHeight / _settings_.TargetWidth; // make sure ratio is preserved

        _activeFrame.DimensionsInScreenSpace = new(newWidth * _image_.CurrentScale, newHeight * _image_.CurrentScale);
        _activeFrame.Color = newWidth <= _settings_.TargetWidth ? (newWidth < _settings_.TargetWidth ? Color.red : Color.yellow) : Color.green;
        UpdateRatiosText();
    }

    void SetFrameSelection()
    {

    }
}
