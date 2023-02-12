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
    [SerializeField] TextMeshProUGUI _sourceTextUI_;
    [SerializeField] TextMeshProUGUI _destinationTextUI_;
    [SerializeField] Button _startButton_;
    [SerializeField] SceneAsset croppingScene;


    private void Start()
    {
        SetSourceText(null);
        SetDestinationText(null);
        //_startButton_.onClick.AddListener(StartCropping);
        _startButton_.gameObject.SetActive(false);

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

        SceneManager.LoadScene(croppingScene.name);
    }
}
