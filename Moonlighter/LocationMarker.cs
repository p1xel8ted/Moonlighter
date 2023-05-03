using System;
using DG.Tweening;

namespace Moonlighter;

using UnityEngine;
using UnityEngine.UI;

public class LocationMarker : MonoBehaviour
{
    private Transform TargetTransform { get; set; }
    private Image _whiteDotImage;
    private RectTransform _whiteDotRectTransform;
    private Text _positionText;
    private Canvas _canvas;
    private Color Color { get; set; }

    private void Start()
    {
        // Create a new canvas and configure it
        var canvasObject = new GameObject("Canvas");
        _canvas = canvasObject.AddComponent<Canvas>();
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Create a new Image GameObject
        var whiteDotObject = new GameObject("WhiteDot");
        _whiteDotImage = whiteDotObject.AddComponent<Image>();

        // Set the Image GameObject as a child of the Canvas
        whiteDotObject.transform.SetParent(_canvas.transform, false);

        // Configure the Image component
        _whiteDotImage.color = Color;
        _whiteDotRectTransform = whiteDotObject.GetComponent<RectTransform>();
        _whiteDotRectTransform.sizeDelta = new Vector2(10, 10); // Set the size of the white dot

        // Create a new Text GameObject
        var textObject = new GameObject("PositionText");
        _positionText = textObject.AddComponent<Text>();

        // Set the Text GameObject as a child of the Canvas
        textObject.transform.SetParent(_canvas.transform, false);

        // Configure the Text component
        _positionText.horizontalOverflow = HorizontalWrapMode.Overflow;
        _positionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _positionText.fontSize = 18;
        _positionText.color = Color;
        _positionText.alignment = TextAnchor.MiddleCenter;
    }

    private void Update()
    {
        if (TargetTransform != null && Camera.main != null)
        {
            // Convert the target transform's position to screen space
            var screenPosition = Camera.main.WorldToScreenPoint(TargetTransform.position);

            // Set the white dot's position to the screen position of the target transform
            _whiteDotRectTransform.position = screenPosition;

            // Set the position text to the screen position of the target transform, moving it down by 40 units

            var pos = Color switch
            {
                var color when color == Color.white => -60,
                var color when color == Color.green => 60,
                var color when color == Color.magenta => -120,
                _ => -60
            };
            _positionText.transform.position = screenPosition + new Vector3(0, pos, 0);

            // Check if TargetTransform has a parent before accessing its name
            var parentName = TargetTransform.parent != null ? TargetTransform.parent.name : "No Parent";

            // Update the position text with the target transform's world position
            _positionText.text = $"Name: {parentName}\nPosition: {TargetTransform.position.ToString("F2")}\nLocalPosition: {TargetTransform.localPosition.ToString("F2")}\nLocalRotation: {TargetTransform.localRotation.ToString("F2")}";
        }
    }

    
    public void SetTransform(Transform t, Color c)
    {
        TargetTransform = t;
        Color = c;
    }

    public void Show()
    {
        // Set the white dot and position text objects to active
        _whiteDotImage.gameObject.SetActive(true);
        _positionText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        // Set the white dot and position text objects to inactive
        _whiteDotImage.gameObject.SetActive(false);
        _positionText.gameObject.SetActive(false);
    }
}