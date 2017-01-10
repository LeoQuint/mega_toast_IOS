using System.Collections;
//Class holding the player's saved data in the form of a bitarray of unlocked.
[System.Serializable]
public class PlayerData {

    public float jumpForce = 400f;
    public float jumpForceMulti = 1f;

    public float bonusForceMulti = 1f;

#region unlockedData
    /// <summary>
    /// unlocked BitArray (512) bits:
    /// bits 0-63   : Characters unlocked.
    ///               0-
    ///               1-
    ///               2-
    /// bits 64-127 : Level unlocked.
    ///               64-
    ///               65-
    ///               66-
    /// bits 128-191: Cosmetics unlocked
    ///               128-
    ///               129-
    ///               130-
    /// bits 192-255: Items unlocked
    ///               192-
    ///               193-
    ///               194-
    /// bits 256-319: Achievments
    ///               256-
    ///               257-
    ///               258-
    /// </summary>
#endregion
    public BitArray unlocked;


    //Constructor 
    public PlayerData()
    {
        unlocked = new BitArray(512);
    }

    //Constructor
    public PlayerData(BitArray data)
    {
        this.unlocked = data;
    }

    //returns the percentage of unlocked elements.
    public float Completion() 
    {
        int numberUnlocked = 0;
        for (int bitarrayIndex = 0; bitarrayIndex < unlocked.Length; bitarrayIndex++)
        {
            if (unlocked[bitarrayIndex])
            {
                numberUnlocked++;
            }
        }

        return ((float)numberUnlocked/(float)unlocked.Length)*100f;
    }

}
