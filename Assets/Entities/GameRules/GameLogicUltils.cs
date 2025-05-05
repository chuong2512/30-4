namespace HoleBox
{
    using UnityEngine;

    public class GameLogicUltils
    {
        public static Color GetColor(int holeID)
        {
            switch (holeID)
            {
                case -1:
                case 0:
                    return Color.clear;
                case 1:
                    return Color.red;
                case 2:
                    return Color.green;
                case 3:
                    return Color.yellow;
            }

            return Color.cyan;
        }
    }
}