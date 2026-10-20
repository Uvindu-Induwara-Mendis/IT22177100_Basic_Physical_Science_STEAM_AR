using UnityEngine;

public class MagneticTestItem : MonoBehaviour
{
    [Header("Item Properties")]
    public string itemName; 
    public bool isMagnetic; 
    public string materialType; // e.g., "Plastic", "Wood", "Copper", "Steel"

    [Header("Hover Settings")]
    public bool enableHover = true;
    public float hoverSpeed = 2f; 
    public float hoverHeight = 0.015f; 

    [HideInInspector] public Vector3 startingLocalPosition; 
    [HideInInspector] public bool isLocked = false;
    
    // NEW: The safety switch to stop the Math vs. Finger fight!
    [HideInInspector] public bool isBeingDragged = false; 

    void Start()
    {
        startingLocalPosition = transform.localPosition; 
    }

    void Update()
    {
        // Only hover if it's NOT locked to the magnet, and NOT being grabbed!
        if (enableHover && !isLocked && !isBeingDragged)
        {
            float newY = startingLocalPosition.y + (Mathf.Sin(Time.time * hoverSpeed) * hoverHeight);
            
            // We use startingLocalPosition for X and Z so it doesn't drift away!
            transform.localPosition = new Vector3(startingLocalPosition.x, newY, startingLocalPosition.z);
        }
    }
}