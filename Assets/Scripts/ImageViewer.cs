using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using Unity;
using TMPro;
using UnityEngine.SceneManagement;

public class ImageViewer : MonoBehaviour
{
    public float CurrentScale => _currentScale;
    public Vector2 DimensionsInScreenSpace => _rectTransform.sizeDelta;
    public Vector2 BottomLeftInScreenSpace => CenterInScreenSpace - _rectTransform.sizeDelta / 2;
    public Vector2 UpperRightInScreenSpace => CenterInScreenSpace + _rectTransform.sizeDelta / 2;
    public Vector2 CenterInScreenSpace => new Vector2(_parentCanvas_.pixelRect.width / 2, _parentCanvas_.pixelRect.height / 2) + _rectTransform.anchoredPosition;
    public Vector2 ImageSize => _imageSize_;
    public Texture2D Texture => _rawImage_.texture as Texture2D;
    [SerializeField] RawImage _rawImage_;
    [SerializeField] Canvas _parentCanvas_;
    RectTransform _rectTransform;
    Vector2 _imageSize_;
    float _currentScale;

    private void Awake()
    {
        _rectTransform = _rawImage_.GetComponent<RectTransform>();
    }

    public void DisplayNewTexture(Texture texture)
    {
        float maxWidth = _parentCanvas_.pixelRect.width;
        float maxHeight = _parentCanvas_.pixelRect.height;

        float clampingRatio = 1f / Mathf.Max(texture.width / maxWidth, texture.height / maxHeight, 1);

        _rawImage_.texture = texture;
        _imageSize_ = new(texture.width, texture.height);
        Rescale(clampingRatio);
    }

    public void Rescale(float newScale)
    {
        float x = _imageSize_.x * newScale;
        float y = _imageSize_.y * newScale;

        _rectTransform.sizeDelta = new Vector2(x, y);

        _currentScale = newScale;
    }
}
