using UnityEngine;

public class SelectionFrame : MonoBehaviour
{
    public Vector2 Position { get { return _lineTransform.anchoredPosition; } set { _lineTransform.anchoredPosition = value; } }
    public Vector2 DimensionsInScreenSpace { get { return _lineTransform.localScale; } set { _lineTransform.localScale = value; } }
    public Vector2Int TargetResolution;
    public Color Color { get { return _mainRenderer_.startColor; } set { _mainRenderer_.startColor = value; _mainRenderer_.endColor = value; } }
    public bool IsActive => gameObject.activeSelf;

    [SerializeField]
    LineRenderer _mainRenderer_;
    RectTransform _lineTransform;

    private void Awake()
    {
        _mainRenderer_ = GetComponent<LineRenderer>();
        _lineTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _mainRenderer_.loop = true;
        _mainRenderer_.positionCount = 4;
        _mainRenderer_.SetPosition(0, new(-0.5f, -0.5f, 0));
        _mainRenderer_.SetPosition(1, new(-0.5f, 0.5f, 0));
        _mainRenderer_.SetPosition(2, new(0.5f, 0.5f, 0));
        _mainRenderer_.SetPosition(3, new(0.5f, -0.5f, 0));

        Color = Color.yellow;
    }

    public void SetParent(Transform transform)
    {
        _lineTransform.SetParent(transform);
    }

    public void SetAsCurrent(bool current)
    {
        if (current == false)
        {
            _mainRenderer_.widthMultiplier = 0.05f;
            Color = Color * 0.5f;
        }
        else
        {
            _mainRenderer_.widthMultiplier = 0.08f;
        }
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
