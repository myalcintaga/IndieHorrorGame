using UnityEngine;
using TMPro; // TextMeshPro için gerekli
using UnityEngine.EventSystems; // Fare olayları (üzerine gelme/çıkma) için gerekli

public class TextOutlineHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textMesh;
    private Material textMaterial;
    private Color originalOutlineColor;

    [Header("Üzerine Gelince Olacak Çizgi Rengi")]
    public Color hoverColor = Color.red; // İstediğin rengi buradan seçebilirsin

    void Start()
    {
        // Butonun içindeki Text bileşenini bul
        textMesh = GetComponentInChildren<TextMeshProUGUI>();

        if (textMesh != null)
        {
            // Orijinal materyalin bir kopyasını oluştur (Diğer butonları etkilememesi için)
            textMaterial = new Material(textMesh.fontMaterial);
            textMesh.fontMaterial = textMaterial;

            // Mevcut dış çizgi rengini kaydet
            originalOutlineColor = textMaterial.GetColor("_OutlineColor");
        }
    }

    // Fare üzerine gelince (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textMaterial != null)
        {
            // Dış çizgi rengini değiştir
            textMaterial.SetColor("_OutlineColor", hoverColor);
        }
    }

    // Fare üzerinden gidince
    public void OnPointerExit(PointerEventData eventData)
    {
        if (textMaterial != null)
        {
            // Rengi orijinal haline döndür
            textMaterial.SetColor("_OutlineColor", originalOutlineColor);
        }
    }
}