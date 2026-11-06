
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleCharacterUI : MonoBehaviour
{
    [Header("Información")]
    public TMP_Text hpText;
    public TMP_Text mpText;

    [Header("Retrato")]
    public Image portrait;

    [Header("Efecto de muerte")]
    public Material grayscaleMaterial;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Material originalMaterial;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
            originalPosition = rectTransform.anchoredPosition;

        if (portrait != null)
            originalMaterial = portrait.material;
    }

    public void UpdateUI(CharacterData character, bool selected)
    {
        if (character == null || character.characterClass == null)
            return;

        bool isDead = character.currentHP <= 0;

        if (hpText != null)
            hpText.text = "HP: " + character.currentHP + "/" + character.maxHP;

        if (mpText != null)
            mpText.text = "MP: " + character.currentMP + "/" + character.maxMP;

        // Mover al personaje seleccionado 25 píxeles a la izquierda.
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = selected
                ? originalPosition + new Vector2(-25f, 0f)
                : originalPosition;
        }

        // Cambiar el retrato a blanco y negro al morir.
        if (portrait != null)
        {
            portrait.sprite = character.characterClass.portrait;

            if (isDead && grayscaleMaterial != null)
                portrait.material = grayscaleMaterial;
            else
                portrait.material = originalMaterial;
        }
    }
}
