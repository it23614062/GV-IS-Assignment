using System.Collections.Generic;
using UnityEngine;
using TMPro; // Used for the UI Text

public class PlayerInventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform equipPosition; // Where the item appears before throwing
    [SerializeField] private TextMeshProUGUI inventoryUI; // The text on the bottom of the screen

    [Header("Physics Parameters")]
    [SerializeField] private float pickupRange = 10.0f;
    [SerializeField] private float throwForce = 20.0f;

    // Inventory Data
    private List<GameObject> inventory = new List<GameObject>();
    private GameObject equippedItem;
    private int equippedIndex = -1;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        UpdateUI();
    }

    void Update()
    {
        // 1. Handle Left Click
        if (Input.GetMouseButtonDown(0))
        {
            if (equippedItem != null)
            {
                // If holding an item, throw it
                ThrowEquippedItem();
            }
            else
            {
                // If hands are empty, try to pick something up
                TryPickup();
            }
        }

        // 2. Handle Number Keys (1-9) for Equipping Items
        HandleInventorySelection();
    }

    void TryPickup()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            GameObject hitObj = hit.transform.gameObject;

            if (hitObj.GetComponent<Rigidbody>())
            {
                // Add to inventory list
                inventory.Add(hitObj);

                // Hide the object in the game world
                hitObj.SetActive(false);

                UpdateUI();
            }
        }
    }

    void HandleInventorySelection()
    {
        // Check keys 1 through 9
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                // Arrays and Lists start at 0, so Key '1' is index 0
                EquipItem(i - 1);
            }
        }
    }

    void EquipItem(int index)
    {
        // Make sure we aren't trying to equip a slot that doesn't exist
        if (index >= inventory.Count) return;

        // If we are already holding something, hide it back in the inventory
        if (equippedItem != null)
        {
            equippedItem.SetActive(false);
        }

        // Setup the new equipped item
        equippedIndex = index;
        equippedItem = inventory[equippedIndex];

        // Show it in the world, parent it to the equip area, and lock its physics
        equippedItem.SetActive(true);
        equippedItem.transform.position = equipPosition.position;
        equippedItem.transform.parent = equipPosition;

        Rigidbody rb = equippedItem.GetComponent<Rigidbody>();
        rb.isKinematic = true; // This stops gravity and physics while we hold it

        UpdateUI();
    }

    void ThrowEquippedItem()
    {
        // Un-parent the item and turn physics back on
        equippedItem.transform.parent = null;
        Rigidbody throwRb = equippedItem.GetComponent<Rigidbody>();
        throwRb.isKinematic = false;

        // Calculate throw direction based on mouse position
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 throwDirection = (targetPoint - equippedItem.transform.position).normalized;
        throwRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        // Remove from inventory and clear hands
        inventory.RemoveAt(equippedIndex);
        equippedItem = null;
        equippedIndex = -1;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (inventoryUI == null) return;

        string uiText = "<b>Inventory:</b>\n";

        for (int i = 0; i < inventory.Count; i++)
        {
            // Highlight the currently equipped item with color
            if (i == equippedIndex)
            {
                uiText += $"<color=yellow>{i + 1}: [EQUIPPED] {inventory[i].name}</color>\n";
            }
            else
            {
                uiText += $"{i + 1}: {inventory[i].name}\n";
            }
        }

        if (inventory.Count == 0)
        {
            uiText += "<i>Empty</i>";
        }

        inventoryUI.text = uiText;
    }
}