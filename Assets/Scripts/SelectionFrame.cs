using UnityEngine;

public class SelectionFrame : MonoBehaviour
{
    public Vector2 Position { get { return _lineTransform.anchoredPosition; } set { _lineTransform.anchoredPosition = value; } }
    public Vector2 DimensionsInScreenSpace { get { return _lineTransform.localScale; } set { _lineTransform.localScale = value; } }
    public Color Color { get { return _renderer.startColor; } set { _renderer.startColor = value; _renderer.endColor = value; } }
    LineRenderer _renderer;
    RectTransform _lineTransform;

    private void Awake()
    {
        _renderer = GetComponent<LineRenderer>();
        _lineTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _renderer.loop = true;
        _renderer.positionCount = 4;
        _renderer.SetPosition(0, new(-0.5f, -0.5f, 0));
        _renderer.SetPosition(1, new(-0.5f, 0.5f, 0));
        _renderer.SetPosition(2, new(0.5f, 0.5f, 0));
        _renderer.SetPosition(3, new(0.5f, -0.5f, 0));

        Color = Color.yellow;
    }

    public void SetParent(Transform transform)
    {
        _lineTransform.SetParent(transform);
    }
}
