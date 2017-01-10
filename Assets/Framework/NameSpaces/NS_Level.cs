namespace NS_Level
{

#region Enums
    /// Contains all toppings
    public enum Toppings
    {
        Tomato,
        Lettuce,
        Bacon,
        Swiss,
        Ham,
        Onions,
        Pickles,
        Egg,
        COUNT
    }
    /// Contains all condiments
    public enum Condiments
    {
        Mustard,
        Ketchup,
        Salt,
        Pepper,
        Honey,
        Mayo,
        COUNT
    }
    /// Contains all bonuses
    public enum Bonuses
    {
        Chili,
        COUNT
    }
#endregion

#region Sandwiches
    ///Struct used to build pre-made sandwiches.
    [System.Serializable]
    public struct Sandwich
    {
        public Toppings[] toppings;
        public Condiments[] condiments;

        public Sandwich(Toppings[] t, Condiments[] c) 
        {
            toppings = t;
            condiments = c;
        }
    }

#endregion

}
