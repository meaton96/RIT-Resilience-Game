using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Unity.Collections;
using System.IO;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
using System.Text;

public class CardReader : MonoBehaviour
{
    // Properties needed by the new design
    public GameObject cardPrefab;
    public Transform cardTemplateContainer;
    public string cardFileLoc;


    // Establish necessary fields


    public List<Card> allCards;
    public List<Card> resilientCards;
    public List<Card> maliciousCards;
    public List<Card> globalModifiers;

    public Hashtable blueCardTargets;
    public Hashtable blueMitMods;
    public CardFront[] CardFronts;
    public NativeArray<int> CardIDs;
    public NativeArray<int> CardTeam;
    public NativeArray<int> CardCost;
    public NativeArray<int> CardDuration;
    public NativeArray<int> CardTarget;
    public NativeArray<int> CardCount;
    public NativeArray<int> CardTargetCount;
    public NativeArray<int> CardFacilityStateReqs;
    public NativeArray<int> CardSubType;
    public NativeArray<float> CardImpact;
    public NativeArray<float> CardSpreadChance;
    public NativeArray<float> CardPercentChance;



    public List<Player> players;

    public MaliciousActor maliciousActor;



    // Start is called before the first frame update
    void Start()
    {
        //CSVRead();
        LoadCards(cardFileLoc);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public List<Card> LoadCards(string filePath)
    {
        List<Card> cards = new List<Card>();

        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return cards;
        }

        string[] lines = File.ReadAllLines(filePath);
        Regex csvPattern = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

        string[] headers = csvPattern.Split(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = csvPattern.Split(lines[i]);
            if (fields.Length >= headers.Length)
            {
                GameObject cardObject = Instantiate(cardPrefab, cardTemplateContainer);
                Card card = cardObject.GetComponent<Card>();

                for (int j = 0; j < fields.Length; j++)
                {
                    fields[j] = fields[j].Trim('\"');
                }

                card.cardTitle = fields[6];
                card.cardDescription = fields[36];
                card.cardTeam = fields[0];
                card.rollDicePrerequisite = int.Parse(fields[34]);
                card.totalMeepleCost = int.Parse(fields[16]);
                card.backgroundColor = HexToColor(fields[7]);


                card.prerequisiteEffects.Add(new Effect(fields[29]));

                for (int j = 0; j < int.Parse(fields[17]); j++)
                {
                    Meeple blueMeeple = new Meeple("Blue");
                    card.cardCost.Add(blueMeeple);
                }

                for (int j = 0; j < int.Parse(fields[18]); j++)
                {
                    Meeple blackMeeple = new Meeple("Black");
                    card.cardCost.Add(blackMeeple);
                }

                for (int j = 0; j < int.Parse(fields[19]); j++)
                {
                    Meeple purpleMeeple = new Meeple("Purple");
                    card.cardCost.Add(purpleMeeple);
                }

                string[] methods = fields[2].Split(';');

                foreach (string method in methods)
                {
                    CardAction action = new CardAction();
                    action.type = (CardAction.ActionType)System.Enum.Parse(typeof(CardAction.ActionType), method);
                    action.parameters = new List<string>();

                    // Add parameters based on action type
                    switch (action.type)
                    {
                        case CardAction.ActionType.DrawAndDiscardCards:
                            action.parameters.Add(fields[23]); // Draw Count
                            action.parameters.Add(fields[24]); // Discard Count
                            break;
                        case CardAction.ActionType.ShuffleAndDrawCards:
                            action.parameters.Add(fields[25]); // Shuffle Count
                            action.parameters.Add(fields[23]); // Draw Count
                            break;
                        case CardAction.ActionType.ChangeNetworkPoints:
                            action.parameters.Add(fields[20]); // Network Change
                            break;
                        case CardAction.ActionType.ChangePhysicalPoints:
                            action.parameters.Add(fields[21]); // Physical Change
                            break;
                        case CardAction.ActionType.ChangeFinancialPoints:
                            action.parameters.Add(fields[22]); // Financial Change
                            break;
                        case CardAction.ActionType.AddEffect:
                            action.parameters.Add(fields[26]); // Effect Type
                            action.parameters.Add(fields[30]); // Effect Duration
                            break;
                        case CardAction.ActionType.RemoveEffect:
                            action.parameters.Add(fields[27]); // Effect Type
                            break;
                        case CardAction.ActionType.NegateEffect:
                            action.parameters.Add(fields[27]); // Effect Type
                            break;
                        case CardAction.ActionType.ReduceCardCost:
                            action.parameters.Add(fields[15]); // Cost Reduction
                            action.parameters.Add(fields[14]); // Meeple Types
                            break;
                        case CardAction.ActionType.SpreadEffect:
                            action.parameters.Add(fields[26]); // Effect Type
                            break;
                        case CardAction.ActionType.ChangeMeepleAmount:
                            action.parameters.Add(fields[15]); // Cost Reduction
                            action.parameters.Add(fields[14]); // Meeple Types
                            break;
                        case CardAction.ActionType.IncreaseOvertimeAmount:
                            action.parameters.Add("1"); // Increase Amount
                            break;
                        case CardAction.ActionType.ShuffleCardsFromDiscard:
                            action.parameters.Add(fields[25]); // Shuffle Count
                            break;
                        case CardAction.ActionType.RemoveEffectByTeam:
                            action.parameters.Add(fields[27]); // Effect Team
                            action.parameters.Add(fields[28]); // Effect Count
                            break;
                    }

                    card.actions.Add(action);
                }

                cards.Add(card);

                cardObject.GetComponent<CardUI>().SetCardUI(card);
            }
            else
            {
                Debug.LogWarning("Skipped line due to insufficient data: " + lines[i]);
            }
        }

        return cards;
    }

    Color HexToColor(string hex)
    {
        Color color = new Color();
        byte r, g, b;

        if (hex.StartsWith("0x"))
        {
            hex = hex.Substring(2);
        }

        if (hex.Length != 6 ||
            !byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out r) ||
            !byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out g) ||
            !byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out b))
        {
            return Color.white;
        }

        color = new Color(r / 255f, g / 255f, b / 255f);
        return color;
    }

    // Reformat to an SOA style 
    public void CSVRead()
    {
        // Check to see if the file exists
        cardFileLoc = Application.streamingAssetsPath + "\\Cards - Sheet1 (4).csv";
        if (File.Exists(cardFileLoc))
        {
            FileStream stream = File.OpenRead(cardFileLoc);
            TextReader reader = new StreamReader(stream);

            string allCardText = reader.ReadToEnd();

            // Split the read in CSV file into seperate objects at the new line character
            string[] allCSVObjects = allCardText.Split("\n");

            CardFronts = new CardFront[allCSVObjects.Length];


            blueCardTargets = new Hashtable();

            blueMitMods = new Hashtable();

            // Allocate the space in memory for the Cards data
            CardIDs = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardTeam = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardCost = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardDuration = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardTarget = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardCount = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardTargetCount = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardFacilityStateReqs = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardSubType = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardImpact = new NativeArray<float>(allCSVObjects.Length, Allocator.Persistent);
            CardSpreadChance = new NativeArray<float>(allCSVObjects.Length, Allocator.Persistent);
            CardPercentChance = new NativeArray<float>(allCSVObjects.Length, Allocator.Persistent);

            // Make sure to get the atlas first, as we only need to query it once.
            Texture2D tex = new Texture2D(1, 1);
            byte[] tempBytes = File.ReadAllBytes(GetComponent<CreateTextureAtlas>().mOutputFileName); // This gets the entire atlast right now.
            tex.LoadImage(tempBytes);

            for (int i = 0; i < allCSVObjects.Length; i++)
            {
                // Then in each of the lines of csv data, split them based on commas to get the different pieces of information on each object
                // and instantiate a base card object to then fill in with data.
                string[] individualCSVObjects = allCSVObjects[i].Split(",");
                GameObject tempCardObj = Instantiate(cardPrefab);
                CardIDs[i] = i;

                // Get a reference to the Card component on the card gameobject.
                Card tempCard = tempCardObj.GetComponent<Card>();

                //CardFront tempCardFront = new CardFront();
                CardFront tempCardFront = tempCardObj.GetComponent<CardFront>();

                // Assign the cards type based on a switch statement of either Resilient, Malicious, or a Global Modifier
                switch (individualCSVObjects[0].Trim())
                {
                    case "Resilient":
                        //tempCard.type = Card.Type.Resilient;
                        //Debug.Log("Res: " + i);
                        CardTeam[i] = 0;
                        tempCardFront.type = Card.Type.Resilient;
                        break;

                    case "Blue":
                        //tempCard.type = Card.Type.Resilient;
                        //Debug.Log("Blue: " + i);
                        CardTeam[i] = 0;
                        tempCardFront.type = Card.Type.Resilient;
                        break;

                    case "Malicious":

                        //tempCard.type = Card.Type.Malicious;
                        //Debug.Log("Mal: " + i);
                        CardTeam[i] = 1;
                        tempCardFront.type = Card.Type.Malicious;
                        break;

                    case "Red":
                        //tempCard.type = Card.Type.Malicious;
                        //Debug.Log("Red: " + i);
                        CardTeam[i] = 1;
                        tempCardFront.type = Card.Type.Malicious;
                        break;

                    case "Global":
                        //tempCard.type = Card.Type.GlobalModifier;
                        //Debug.Log("Global: " + i);
                        CardTeam[i] = 2;
                        tempCardFront.type = Card.Type.GlobalModifier;
                        break;

                }

                // Then assign the necessary values to each card based off of their csv input.
                tempCardObj.name = individualCSVObjects[1];
                byte[] temp = Encoding.ASCII.GetBytes(individualCSVObjects[1]);
                tempCardFront.title = temp;


                //tempCardFront.title = individualCSVObjects[1];
                //tempCardFront.description = individualCSVObjects[2];
                //tempCardFront.title = individualCSVObjects[1].ToCharArray();
                //tempCardFront.description = individualCSVObjects[4];

                //tempCard.title = individualCSVObjects[1]; //NEED THESE JUST COMMENTING TO DROP ERRORS RN

                //tempCard.description = individualCSVObjects[2]; // NEED THESE JUST COMMENTING TO DROP ERRORS RN

                if (individualCSVObjects[2].Length > 0)
                {
                    tempCard.cost = int.Parse(individualCSVObjects[2]);
                    CardCost[i] = int.Parse(individualCSVObjects[2]);
                }

                byte[] temp2 = Encoding.ASCII.GetBytes(individualCSVObjects[4]);
                tempCardFront.description = temp2;



                // Check to make sure that this is actually a number, but if it has text in it then we don't parse it.
                if (CardTeam[i] == 1)
                {
                    string temp3 = individualCSVObjects[13].Trim();
                    switch (temp3)
                    {
                        case "Reconnaissance":
                            tempCard.malCardType = Card.MalCardType.Reconnaissance;
                            CardSubType[i] = 3;
                            break;

                        case "Inital Access":
                            tempCard.malCardType = Card.MalCardType.InitialAccess;
                            CardSubType[i] = 4;

                            break;

                        case "Impact":
                            tempCard.malCardType = Card.MalCardType.Impact;
                            CardSubType[i] = 5;
                            if (individualCSVObjects[5].Length > 1)
                            {
                                tempCard.potentcy = float.Parse(individualCSVObjects[5]);
                                CardImpact[i] = float.Parse(individualCSVObjects[5]);
                            }
                            break;
                        
                        case "Lateral Movement":
                            tempCard.malCardType = Card.MalCardType.LateralMovement;
                            CardSubType[i] = 6;
                            break;

                        case "Exfiltration":
                            tempCard.malCardType = Card.MalCardType.Exfiltration;
                            CardSubType[i] = 7;
                            break;

                    }

                    //if (individualCSVObjects[5].Contains("|") == false)
                    //{
                    //    if (individualCSVObjects[5].Contains(" ") == false)
                    //    {
                    //        if (individualCSVObjects[5].Contains("e") == false)
                    //        {
                    //            if (individualCSVObjects[5].Length > 1)
                    //            {
                    //                tempCard.potentcy = float.Parse(individualCSVObjects[5]);
                    //                CardImpact[i] = float.Parse(individualCSVObjects[5]);
                    //            }

                    //        }
                    //    }

                    //}
                }
                else
                {
                    string temp4 = individualCSVObjects[13].Trim();
                    switch (temp4)
                    {
                        case "Detection":
                            tempCard.resCardType = Card.ResCardType.Detection;
                            CardSubType[i] = 0;
                            string[] detectPot = individualCSVObjects[5].Split('&'); // This is used for blue cards which have a potency like this "phishing -40% & browser session hijacking -40% & Infect with Removable Media -40%" so we split to find the different impacts between & and then split once more based off :
                            if (detectPot.Length > 1)
                            {
                                string toBePrinted = "";
                                int[] targets = new int[detectPot.Length];
                                for (int j = 0; j < detectPot.Length; j++)
                                {
                                    string[] tempIndPot = detectPot[j].Split(':');
                                    if (tempIndPot.Length > 1)
                                    {
                                        tempIndPot[1] = tempIndPot[1].Trim();
                                        for(int k = 0; k < CardFronts.Length; k++)
                                        {
                                            if(CardFronts[k].name == tempIndPot[1])
                                            {
                                                targets[j] = k;
                                                break;
                                            }
                                        }
                                    }
                                    toBePrinted += detectPot[j] + '\n';
                                }
                                blueCardTargets.Add(i, targets);

                                byte[] temp3 = Encoding.ASCII.GetBytes(toBePrinted);
                                tempCardFront.impact = temp3;
                            }
                            else
                            {
                                if (detectPot[0].Contains("cancel"))
                                {
                                    string tempString = detectPot[0].Trim();
                                    Debug.Log(tempString);
                                }
                                else if (detectPot[0].Contains("-"))
                                {
                                    detectPot[0].Remove(detectPot[0].IndexOf('-'));
                                    Debug.Log(detectPot[0]);
                                }
                                byte[] temp3 = Encoding.ASCII.GetBytes(detectPot[0]);
                                tempCardFront.impact = temp3;
                            }
                            break;

                        case "Mitigation":
                            tempCard.resCardType = Card.ResCardType.Mitigation;
                            CardSubType[i] = 1;
                            
                            break;

                        case "Prevention":
                            tempCard.resCardType = Card.ResCardType.Prevention;
                            CardSubType[i] = 2;
                            string[] tempPot4 = individualCSVObjects[5].Split('&'); // This is used for blue cards which have a potency like this "phishing -40% & browser session hijacking -40% & Infect with Removable Media -40%" so we split to find the different impacts between & and then split once more based off :
                            List<int> tempImpList = new List<int>();
                            Hashtable tempTable = new Hashtable();
                            if (tempPot4.Length > 1)
                            {
                                string toBePrinted = "";
                                int potency = 0;
                                for (int j = 0; j < tempPot4.Length; j++)
                                {
                                    string[] tempIndPot = tempPot4[j].Split(':');
                                    if (tempIndPot.Length > 1)
                                    {
                                        tempIndPot[0] = tempIndPot[0].Trim();
                                        tempIndPot[1] = tempIndPot[1].Trim();
                                        if (tempIndPot[1].Contains("-") == true)
                                        {
                                            tempIndPot[1] = tempIndPot[1].Substring(tempIndPot[1].IndexOf('-') + 1, 2);
                                            //tempImpList.Add(int.Parse(tempIndPot[1]));
                                            potency = int.Parse(tempIndPot[1]);
                                            for (int k = 0; k < CardFronts.Length; k++)
                                            {
                                                if (CardFronts[k].name.ToLower().Trim() == tempIndPot[0].Trim().ToLower())
                                                {
                                                    //tempIndPot[1] = tempIndPot[1].Remove(tempIndPot[1].IndexOf('-'));
                                                    //tempIndPot[1] = tempIndPot[1].Remove(tempIndPot[1].IndexOf('%'));
                                                    tempImpList.Add(k);
                                                    //tempTable.Add(k, float.Parse(tempIndPot[1]));
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    toBePrinted += tempPot4[j] + '\n';
                                    Debug.Log(toBePrinted);
                                }
                                tempImpList.Insert(0, potency);
                                blueMitMods.Add(i, tempImpList);
                                //blueMitMods.Add(i, tempTable);
                                byte[] temp3 = Encoding.ASCII.GetBytes(toBePrinted);
                                tempCardFront.impact = temp3;
                            }
                            else
                            {
                                tempPot4 = individualCSVObjects[5].Split(':');
                                if (tempPot4.Length > 1)
                                {
                                    tempPot4[0] = tempPot4[0].Trim();
                                    //tempPot4[1].Remove(tempPot4[1].IndexOf('-'));
                                    //tempPot4[1].Remove(tempPot4[1].IndexOf('%'));
                                    tempPot4[1] = tempPot4[1].Substring(tempPot4[1].IndexOf('-') + 1, 2);
                                    List<int> tempImpactList = new List<int>();
                                    tempImpactList.Add(int.Parse(tempPot4[1]));
                                    Hashtable tempIndTable = new Hashtable();
                                    for (int k = 0; k < CardFronts.Length; k++)
                                    {
                                        if (CardFronts[k].name.ToLower().Trim() == tempPot4[0].Trim().ToLower())
                                        {
                                            tempImpactList.Add(k);
                                            //tempIndTable.Add(k, float.Parse(tempPot4[1]));
                                            break;
                                        }
                                    }
                                    blueMitMods.Add(i, tempImpactList);
                                    //blueMitMods.Add(i, tempIndTable);
                                }

                                //Debug.Log(tempPot4[0]);
                                byte[] temp3 = Encoding.ASCII.GetBytes(tempPot4[0]);
                                tempCardFront.impact = temp3;
                            }
                            break;

                        default:
                            string[] tempPot = individualCSVObjects[5].Split('&'); // This is used for blue cards which have a potency like this "phishing -40% & browser session hijacking -40% & Infect with Removable Media -40%" so we split to find the different impacts between & and then split once more based off :
                            if (tempPot.Length > 1)
                            {
                                string toBePrinted = "";
                                for (int j = 0; j < tempPot.Length; j++)
                                {
                                    string[] tempIndPot = tempPot[j].Split(':');
                                    if (tempIndPot.Length > 1)
                                    {
                                        tempIndPot[1] = tempIndPot[1].Trim();
                                        if (tempIndPot[1].Contains("-") == true)
                                        {
                                            tempIndPot[1].Remove(tempIndPot[1].IndexOf('-'));
                                        }
                                    }
                                    toBePrinted += tempPot[j] + '\n';
                                    Debug.Log(toBePrinted);
                                }
                                byte[] temp3 = Encoding.ASCII.GetBytes(toBePrinted);
                                tempCardFront.impact = temp3;
                            }
                            else
                            {
                                if (tempPot[0].Contains("cancel"))
                                {
                                    string tempString = tempPot[0].Trim();
                                    Debug.Log(tempString);
                                }
                                else if (tempPot[0].Contains("-"))
                                {
                                    tempPot[0].Remove(tempPot[0].IndexOf('-'));
                                    Debug.Log(tempPot[0]);
                                }
                                byte[] temp3 = Encoding.ASCII.GetBytes(tempPot[0]);
                                tempCardFront.impact = temp3;
                            }
                            break;


                    }
                    

                }


                if (individualCSVObjects[6].Contains("|") == false)
                {
                    if (individualCSVObjects[6].Contains(" ") == false)
                    {
                        if (individualCSVObjects[6].Contains("e") == false)
                        {
                            if (individualCSVObjects[6].Length > 1)
                            {
                                tempCard.percentSuccess = float.Parse(individualCSVObjects[6]); // Parse bc it is a percent
                                CardPercentChance[i] = float.Parse(individualCSVObjects[6]);
                            }

                        }
                    }

                }

                CardSpreadChance[i] = int.Parse(individualCSVObjects[7]);
                tempCard.percentSpread = int.Parse(individualCSVObjects[7]);


                tempCard.duration = int.Parse(individualCSVObjects[8]);
                CardDuration[i] = int.Parse(individualCSVObjects[8]);
                if (individualCSVObjects[10].Contains("All") == false)
                {
                    if (individualCSVObjects[10].Length > 0)
                    {
                        tempCard.targetCount = int.Parse(individualCSVObjects[10]);
                        CardTargetCount[i] = int.Parse(individualCSVObjects[10]);
                    }


                }
                else
                {
                    Debug.Log("READ IN AMOUNT FAIL: " + individualCSVObjects[10]);
                    tempCard.targetCount = int.MaxValue;
                    CardTargetCount[i] = int.MaxValue;
                }

                CardCount[i] = int.Parse(individualCSVObjects[12]);
                //Debug.Log("Card Count for " + i + ": " + CardCount[i]);

                // Assign the cards facility state requirements so that when the csv is read in, we can assign the cards the proper requirement
                switch (individualCSVObjects[11].Trim()) // Change this to the correct column for the CSV for the Fac State reqs
                {
                    case "Normal":
                        CardFacilityStateReqs[i] = 0;
                        break;

                    case "Any":
                        CardFacilityStateReqs[i] = 0;
                        break;

                    case "Informed":
                        CardFacilityStateReqs[i] = 1;
                        break;

                    case "Accessed":
                        CardFacilityStateReqs[i] = 2;
                        break;

                    case "Down":
                        CardFacilityStateReqs[i] = 3;
                        break;

                }




                // Then we use a for loop to check the image location of the current CSV and the textureUVs
                // made when the atlas is made. If it is the matching image, then we take a sub-section of the atlas
                // and add it to the card.
                // ** VERY IMPORTANT ** The texture2D width and Height need to match what is in the TextureAtlas.cs file and all images for cards need to adhere to this size for this to work properly.
                for (int j = 0; j < TextureAtlas.textureUVs.Count; j++)
                {
                    TextureUV texUV = TextureAtlas.textureUVs[j];
                    if (texUV.location.Trim() == individualCSVObjects[3].Trim()) // Check to make sure that the TextureUV and the current CSV objects image are the same
                    {

                        Texture2D tex3 = new Texture2D(128, 128); // This needs to match the textureatlas pixel width


                        //tempCardObj.GetComponentInChildren<RawImage>().texture = tex3;
                        //tempCard.img.texture = tex3;
                        tempCardFront.img = tex3;

                        Color[] tempColors = tex.GetPixels(texUV.column * 128, texUV.row * 128, 128, 128); // This needs to match the textureatlas pixel width
                        tex3.SetPixels(tempColors);
                        tex3.Apply();
                        break;
                    }
                }
                //Debug.Log("CARD FRONT: " + tempCardFront);
                CardFronts[i] = tempCardFront;
                tempCardObj.SetActive(false);

                //Add target count into impact description of the card
                foreach(var item in tempCardObj.GetComponentsInChildren<TMP_Text>())
                {
                    if (item.gameObject.name.Contains("Impact"))
                    {
                        item.text = "Target Count: " + tempCard.targetCount;
                    }
                }

                // Add the card to all card list and then based off a switch on the cards type we add it to a list of all resilient, malicious, or global modifier cards.
                allCards.Add(tempCard);
                //switch (tempCard.type)
                //{
                //    case Card.Type.Resilient:
                //        resilientCards.Add(tempCard);
                //        break;

                //    case Card.Type.Malicious:
                //        maliciousCards.Add(tempCard);
                //        break;

                //    case Card.Type.GlobalModifier:
                //        globalModifiers.Add(tempCard);
                //        break;
                //}
            }
            // Close at the end
            reader.Close();
            stream.Close();
        
        }
        else
        {
            Application.Quit();
            
        }

    }

    public void OnDestroy()
    {
        // Must dispose of the allocated memory
        CardIDs.Dispose();
        CardTeam.Dispose();
        CardCost.Dispose();
        CardDuration.Dispose();
        CardTarget.Dispose();
        CardFacilityStateReqs.Dispose();
        CardCount.Dispose();
        CardSubType.Dispose();
        CardTargetCount.Dispose();
        CardImpact.Dispose();
        CardSpreadChance.Dispose();
        CardPercentChance.Dispose();
    }
}
