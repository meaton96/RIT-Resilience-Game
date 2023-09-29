using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFront : MonoBehaviour
{
    // Establish necessary fields
    public Card.Type type;
    //public NativeArray<byte> title;
    public byte[] title;
    public byte[] description;
    public byte[] impact;
    public int cost;
    public GameObject innerTexts;
    public GameObject costText;
    //public NativeArray<byte> description;
    public Texture2D img;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
