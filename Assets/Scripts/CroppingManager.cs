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

public class CroppingManager : MonoBehaviour
{
    [SerializeField] Settings _settings_;
    [SerializeField] ImageViewer _image_;
    [SerializeField] Canvas _canvas_;
    [SerializeField] SelectionFrame _framePrefab_;
    [Space]
    [SerializeField] TextMeshProUGUI _counterText_;
    [SerializeField] TextMeshProUGUI _ratiosText_;
    [SerializeField] TextMeshProUGUI _ratiosText2_;
    [SerializeField] TextMeshProUGUI _resolutionText_;

    SelectionFrame _currentFrame;
    float _scalingSpeed = 0.1f;
    List<SelectionFrame> _frames;
    int _numberOfActiveFrames;

    int _imageID;
    int _numberOfSourceImages;

    ImageProcessor _imageProcessor;

    bool _targetResolutionChanged_;

    private void Start()
    {
        if (!FileManager.IsInitialized)
        {
            FileManager.SetSource("F:\\Projects\\Cropper\\TestFolder\\in");
            FileManager.SetDestination("F:\\Projects\\Cropper\\TestFolder\\out");
            FileManager.InitializeFiles();
        }
        _imageID = 0;
        _numberOfSourceImages = FileManager.NumberOfImages;

        _numberOfActiveFrames = 0;
        _frames = new();
        _currentFrame = GetNewFrame();

        _imageProcessor = new();

        DisplayImage(_imageID);
    }

    private void Update()
    {
        SetTargetResolution();
        ScrollThroughImages();
        //ScaleImage();
        ScaleSelectionFrame();
        MoveSelectionFrame();
        SetSelection();

        //if (Input.GetKeyDown(KeyCode.S))
        //    SaveSelected();

        if (Input.GetKeyDown(_settings_.Key_UndoSelection))
            RemoveLastSetFrame();
    }

    void SetTargetResolution()
    {
        _targetResolutionChanged_ = true;
        if (Input.GetKeyDown(_settings_.Key_SelectResolution1)) 
        {
            _settings_.ActiveResolution = _settings_.Resolutions[0];
            return;
        }
        if (Input.GetKeyDown(_settings_.Key_SelectResolution2))
        {
            _settings_.ActiveResolution = _settings_.Resolutions[1];
            return;
        }
        if (Input.GetKeyDown(_settings_.Key_SelectResolution3))
        {
            _settings_.ActiveResolution = _settings_.Resolutions[2];
            return;
        }
        if (Input.GetKeyDown(_settings_.Key_SelectResolution4))
        {
            _settings_.ActiveResolution = _settings_.Resolutions[3];
            return;
        }
        _targetResolutionChanged_ = false;
    }

    SelectionFrame GetNewFrame()
    {
        if (_numberOfActiveFrames < _frames.Count)
        {
            _frames[_numberOfActiveFrames].SetActive(true);
            _frames[_numberOfActiveFrames].SetAsCurrent(true);
            return _frames[_numberOfActiveFrames++];
        }
        else
            return CreateFrame();
    }

    void RemoveLastSetFrame()
    {
        if (_numberOfActiveFrames > 1)
        {
            _currentFrame.SetActive(false);
            _numberOfActiveFrames--;
            _currentFrame = _frames[_numberOfActiveFrames-1];
            _currentFrame.SetAsCurrent(true);
            _currentFrame.TargetResolution = new(_settings_.ActiveResolution.Width, _settings_.ActiveResolution.Height);
            RescaleFrameRelativeToTarget(1, false);
            UpdateFrameColor(1);
        }
    }

    SelectionFrame CreateFrame()
    {
        SelectionFrame frame = Instantiate(_framePrefab_);
        frame.SetParent(_canvas_.transform);
        frame.TargetResolution = new(_settings_.ActiveResolution.Width, _settings_.ActiveResolution.Height);
        frame.DimensionsInScreenSpace = (Vector2)frame.TargetResolution * _image_.CurrentScale;
        frame.SetAsCurrent(true);
        _frames.Add(frame);
        _numberOfActiveFrames++;
        return frame;
    }

    void SetSelection()
    {
        if (!Input.GetKeyDown(_settings_.Key_SetSelection)) return;

        _currentFrame.SetAsCurrent(false);
        _currentFrame = GetNewFrame();
    }

    void SaveSelected()
    {
        byte[] rawData = null;
        foreach (SelectionFrame frame in _frames)
        {
            if (frame.IsActive && frame != _currentFrame)
            {
                float selectionScale = frame.DimensionsInScreenSpace.x / _image_.CurrentScale / frame.TargetResolution.x;
                Vector2 selectionPosition = ToImageSpace(frame.Position);

                if (rawData == null)
                    rawData = _image_.Texture.EncodeToPNG(); // Ideally it should use GetRawTextureData() and convert that to sensible format on a processing thread. But there are compatibility issues

                _imageProcessor.Process(new ImageProcessor.SelectedImageData(rawData, frame.TargetResolution.x, frame.TargetResolution.y, selectionScale, selectionPosition));
            }
        }
    }

    void ScrollThroughImages()
    {
        if (!Input.GetKeyDown(_settings_.Key_NextImage) && !Input.GetKeyDown(_settings_.Key_PreviousImage))
            return;
        
        SaveSelected();
        while (_numberOfActiveFrames > 1)
            RemoveLastSetFrame();

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

    void ScaleImage()
    {
        if (Input.mouseScrollDelta.y == 0 || !Input.GetKey(_settings_.Key_RescaleImage)) return;

        float newScale = _image_.CurrentScale * (1 + Input.mouseScrollDelta.y * _scalingSpeed);
        _image_.Rescale(newScale);
        UpdateFrameColor(_currentFrame.DimensionsInScreenSpace.x / _image_.CurrentScale / _settings_.ActiveResolution.Width);
        UpdateImageRatiosText();
    }

    void ScaleSelectionFrame()
    {
        if ((Input.mouseScrollDelta.y == 0 || Input.GetKey(_settings_.Key_RescaleImage)) && !_targetResolutionChanged_) return;

        if (_targetResolutionChanged_)
            _currentFrame.TargetResolution = new(_settings_.ActiveResolution.Width, _settings_.ActiveResolution.Height);

        float desiredScale = _currentFrame.DimensionsInScreenSpace.x / (_settings_.ActiveResolution.Width * _image_.CurrentScale);

        if (Input.GetKey(_settings_.Key_SlowScrolling))
            desiredScale += Input.mouseScrollDelta.y * _scalingSpeed / _image_.CurrentScale / 10;
        else
            desiredScale += Input.mouseScrollDelta.y * _scalingSpeed / _image_.CurrentScale;

        RescaleFrameRelativeToTarget(desiredScale, Input.GetKey(_settings_.Key_ExceedConstrains));
    }

    void MoveSelectionFrame()
    {
        Vector2 position = Input.mousePosition;

        if (!Input.GetKey(_settings_.Key_ExceedConstrains))
           position = new(Mathf.Clamp(position.x, _image_.BottomLeftInScreenSpace.x + _currentFrame.DimensionsInScreenSpace.x / 2, _image_.UpperRightInScreenSpace.x - _currentFrame.DimensionsInScreenSpace.x / 2),
                          Mathf.Clamp(position.y, _image_.BottomLeftInScreenSpace.y + _currentFrame.DimensionsInScreenSpace.y / 2, _image_.UpperRightInScreenSpace.y - _currentFrame.DimensionsInScreenSpace.y / 2));
       
        position = new(Mathf.Clamp(position.x, _currentFrame.DimensionsInScreenSpace.x / 2, _canvas_.pixelRect.width - _currentFrame.DimensionsInScreenSpace.x / 2),
                       Mathf.Clamp(position.y, _currentFrame.DimensionsInScreenSpace.y / 2, _canvas_.pixelRect.height - _currentFrame.DimensionsInScreenSpace.y / 2));
        
        _currentFrame.Position = new(position.x, position.y);
    }

    void DisplayImage(int id)
    {
        _counterText_.text = $"{id + 1} / {_numberOfSourceImages}";
        if (id < _numberOfSourceImages)
        {
            _image_.DisplayNewTexture(FileManager.LoadTexture(id));
            _currentFrame.DimensionsInScreenSpace = new(_image_.CurrentScale * _settings_.ActiveResolution.Width, _image_.CurrentScale * _settings_.ActiveResolution.Height);
            UpdateFrameRatiosText();
            UpdateImageRatiosText();
        }
    }

    void UpdateFrameRatiosText()
    {
        Vector2 pixelScale = new(Mathf.Round(_currentFrame.DimensionsInScreenSpace.x / _image_.CurrentScale), Mathf.Round(_currentFrame.DimensionsInScreenSpace.y / _image_.CurrentScale));
        Vector2 screenScale = new(Mathf.Round(_currentFrame.DimensionsInScreenSpace.x) /* 10)/10f*/, Mathf.Round(_currentFrame.DimensionsInScreenSpace.y) /* 10)/10f*/);
        _ratiosText_.text = $"Frame\nin image pixels: {pixelScale.x}/{pixelScale.y}\n" +
                $"in screen pixels: {screenScale.x}/{screenScale.y}\n" +
                $"ratio {_currentFrame.DimensionsInScreenSpace.x / _currentFrame.DimensionsInScreenSpace.y}. scale to target: {Mathf.Round(pixelScale.x * 100 / _settings_.ActiveResolution.Width) / 100f}";
    }

    void UpdateImageRatiosText()
    {
        _ratiosText2_.text = $"Screen resolution: {_canvas_.pixelRect.width}x{_canvas_.pixelRect.height}\nImageResolution: {_image_.ImageSize.x}x{_image_.ImageSize.y}\n" +
            $"Image in pixels: {Mathf.Round(_image_.DimensionsInScreenSpace.x)}x{Mathf.Round(_image_.DimensionsInScreenSpace.y)}\nImage scale: {_image_.CurrentScale}";
    }

    void RescaleFrameRelativeToTarget(float scale, bool exceedConstrains)
    {
        float desiredWidth = _settings_.ActiveResolution.Width * scale;
        float desiredHeight = _settings_.ActiveResolution.Height * scale;

        bool restricted = !Input.GetKey(_settings_.Key_ExceedConstrains);

        Vector2 minSize = new(1, 1);
        Vector2 maxSize = new(_canvas_.pixelRect.width / _image_.CurrentScale, _canvas_.pixelRect.height / _image_.CurrentScale);
        if (restricted)
        {
            minSize = new(Mathf.Min(maxSize.x, _settings_.ActiveResolution.Width), Mathf.Min(maxSize.y, _settings_.ActiveResolution.Height));
            maxSize = new(Mathf.Min(maxSize.x, _image_.ImageSize.x), Mathf.Min(maxSize.y, _image_.ImageSize.y));
        }
        float newWidth = Mathf.Clamp(desiredWidth, minSize.x, maxSize.x);
        float newHeight = Mathf.Clamp(desiredHeight, minSize.y, maxSize.y);

        float newScale = Mathf.Min(newWidth / _settings_.ActiveResolution.Width, newHeight / _settings_.ActiveResolution.Height);

        newWidth = newScale * _settings_.ActiveResolution.Width;
        newHeight = newScale * _settings_.ActiveResolution.Height;

        _currentFrame.DimensionsInScreenSpace = new(newWidth * _image_.CurrentScale, newHeight * _image_.CurrentScale);
        UpdateFrameColor(newWidth / _settings_.ActiveResolution.Width);
        UpdateFrameRatiosText();
    }

    void UpdateFrameColor(float relativeToTargetScale)
    {
        _currentFrame.Color = relativeToTargetScale <= 1 ? (relativeToTargetScale < 1 ? Color.red : Color.yellow) : Color.green;
    }

    Vector2 ToImageSpace(Vector2 inScreenSpace) => new((inScreenSpace.x - _canvas_.pixelRect.width / 2f + _image_.DimensionsInScreenSpace.x / 2f) / _image_.CurrentScale,
                                                       (inScreenSpace.y - _canvas_.pixelRect.height / 2f + _image_.DimensionsInScreenSpace.y / 2f) / _image_.CurrentScale);
}
