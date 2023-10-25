using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardObjectForView : MonoBehaviour
{
    [Header("UI Elements")]
    public Image titleBackground;
    public TMP_Text titleText;
    public RawImage cardImage;
    public TMP_Text impactText;
    public TMP_Text descriptionText;
    public TMP_Text costText;

    public Button selectButton;

    private CardForEditor cardInfo;

    private void Start()
    {
        selectButton.onClick.AddListener(SelectThisCard);
    }

    public void Initialize(CardForEditor card, string imageFolderDirectory)
    {
        cardInfo = card;
        if (cardInfo.team.Equals("Red"))
        {
            titleBackground.color = CardViewer.instance.redTeamColor;
        }
        else if (cardInfo.team.Equals("Blue"))
        {
            titleBackground.color = CardViewer.instance.blueTeamColor;
        }
        else
        {
            titleBackground.color = Color.white;
            Debug.LogError("Undefined Team: " + cardInfo.team);
        }

        titleText.text = cardInfo.title;
        LoadImageIntoRawImage(imageFolderDirectory + cardInfo.image);
        impactText.text = cardInfo.impact;
        descriptionText.text = cardInfo.description;
        costText.text = cardInfo.cost.ToString();
    }

    public void LoadImageIntoRawImage(string imagePath)
    {
        // Load the image bytes
        byte[] imageBytes = File.ReadAllBytes(imagePath);

        // Create a texture and assign the loaded bytes
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageBytes))
        {
            // If successfully loaded, assign the texture to the RawImage
            cardImage.texture = texture;
        }
        else
        {
            Debug.LogError("Failed to load image at path: " + imagePath);
        }
    }

    public void SelectThisCard()
    {
        CardViewer.instance.UpdateSelectedCard(cardInfo);
        CardViewer.instance.ShowCardEditor();
    }
}
