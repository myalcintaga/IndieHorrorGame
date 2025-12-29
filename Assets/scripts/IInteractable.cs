using UnityEngine;

public interface IInteractable
{
    // Bu nesneyle etkileşime geçilince ne olacak? (Kapı açılır, lamba yanar...)
    void Interact();

    // Ekranda oyuncuya ne ipucu vereceğiz? (Örn: "Kapıyı Aç [E]")
    string GetDescription();
}