using UnityEditor;
using System.Reflection;
using System;

namespace BigBlit.Eddie
{
    public static class MathUtilsEx
    {
        public static float RoundBasedOnMinimumDifference(
          float valueToRound, float minDifference)
        {
             var types =  new Type[] { typeof(float), typeof(float)};
           return (float) typeof(MathUtils).GetMethod("RoundBasedOnMinimumDifference", BindingFlags.Static | BindingFlags.NonPublic, null, types, null)
            .Invoke(null, new object[] {valueToRound, minDifference });

        }
    }
}
