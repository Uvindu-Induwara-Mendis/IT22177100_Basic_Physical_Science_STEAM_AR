using UnityEngine;

// =========================================================================================
// LESSON 2 (ELECTROMAGNETS): 3D TESTABLE ITEM (AR SCENE)
// =========================================================================================
// Context: Attached to each 3D object in the Lesson 2 AR testing area (Paperclip, Safety Pin,
// Eraser, Wooden Block, Copper Coin). Defines whether the material is magnetic or non-magnetic,
// and applies an idle floating hover animation until grabbed by the student's finger.
public class MagneticTestItem : MonoBehaviour
{
    // --- Material & Item Properties ---
    [Header("Item Properties")]
    public string itemName;                  // e.g., "Paperclip", "Safety Pin"
    public bool isMagnetic;                  // True for iron/steel items that attract to electromagnet
    public string materialType;              // "Plastic", "Wood", "Copper", "Steel"

    // --- Idle Hover Animation ---
    [Header("Hover Settings")]
    public bool enableHover = true;          // Gentle vertical bobbing while on the desk
    public float hoverSpeed = 2f; 
    public float hoverHeight = 0.015f; 

    [HideInInspector] public Vector3 startingLocalPosition; 
    [HideInInspector] public bool isLocked = false;        // True once stuck to the electromagnet nail
    [HideInInspector] public bool isBeingDragged = false;  // Disables hover math while finger is dragging

    void Start()
    {
        startingLocalPosition = transform.localPosition; 
    }

    void Update()
    {
        // Gently bob up and down on the desk while waiting to be picked up
        if (enableHover && !isLocked && !isBeingDragged)
        {
            float newY = startingLocalPosition.y + (Mathf.Sin(Time.time * hoverSpeed) * hoverHeight);
            transform.localPosition = new Vector3(startingLocalPosition.x, newY, startingLocalPosition.z);
        }
    }
}