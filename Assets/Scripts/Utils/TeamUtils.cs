using UnityEngine;

namespace Utils
{
    public static class TeamUtils
    {
        public static Color GetTeamColor(byte team)
        {
            return team switch
            {
                0 => Color.cyan,
                1 => Color.red,
                byte.MaxValue => Color.lightGray,
                _ => Color.white,
            };
        }
    }
}