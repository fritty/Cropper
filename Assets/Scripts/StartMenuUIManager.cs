using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using Unity;
using TMPro;
using UnityEngine.SceneManagement;

public class StartMenuUIManager : MonoBehaviour
{
    [SerializeField] Settings _settings_;
    [SerializeField] TextMeshProUGUI _sourceTextUI_;
    [SerializeField] TextMeshProUGUI _destinationTextUI_;
    [SerializeField] Button _startButton_;


    private void Start()
    {
        SetSourceText(null);
        SetDestinationText(null);
        _startButton_.gameObject.SetActive(false);

        _settings_.Resolutions = new Settings.Resolution[4];

        for (int i = 0; i < 4; i++)
        {
            _settings_.Resolutions[i] = new(512 + (i >> 1) * 256, 512 + (i - (i >> 1)*2) * 256);
        }
        _settings_.ActiveResolution = _settings_.Resolutions[0];

        FileManager.OnSetSource += SetSourceText;
        FileManager.OnSetDestination += SetDestinationText;
        FileManager.OnFilesInitialized += StartCropping;
    }

    void SetSourceText(string sourcePath) => SetText(sourcePath, _sourceTextUI_);

    void SetDestinationText(string destinationPath) => SetText(destinationPath, _destinationTextUI_);

    void SetText(string text, TextMeshProUGUI uiElement)
    {
        if (text == null || text == "")
        {
            uiElement.text = "None";
            uiElement.color = Color.red;
        }
        else
        {
            uiElement.color = Color.gray;
            uiElement.text = text;
        }
        CheckStartButtonCondition();
    }

    void CheckStartButtonCondition()
    {
        if (_sourceTextUI_.text == "None" || _destinationTextUI_.text == "None")
        {
            _startButton_.gameObject.SetActive(false);
            return;
        }

        _startButton_.gameObject.SetActive(true);
    }

    void StartCropping()
    {
        FileManager.OnSetSource -= SetSourceText;
        FileManager.OnSetDestination -= SetDestinationText;
        FileManager.OnFilesInitialized -= StartCropping;

        SceneManager.LoadScene("CroppingScene");
    }

    public void SetSourceWrap() => FileManager.SetSource();
    public void SetDestinationWrap() => FileManager.SetDestination();
}
