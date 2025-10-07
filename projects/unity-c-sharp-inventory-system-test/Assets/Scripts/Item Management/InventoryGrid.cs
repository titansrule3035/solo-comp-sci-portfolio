using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryGrid : MonoBehaviour
{
    public UnityEngine.UI.Image backgroundImage;
    public UnityEngine.UI.Image itemImage;
    public TextMeshProUGUI counter;
    public bool itemEquipped = false;

    private void Start()
    {
        Update();
    }

    private void Update()
    {
        backgroundImage.gameObject.SetActive(itemEquipped);
    }
}
