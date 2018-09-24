using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine
{
    public static class StringUtility
    {
        public static int[] StringToIntArray(string sSource, char cSeperator)
        {
            List<int> intResult = new List<int>();
            string[] stringArray = sSource.Split(cSeperator);
            for (int i = 0; i < stringArray.Length; i++)
            {
                int num;
                if (!string.IsNullOrEmpty(stringArray[i].Replace(" ", "")))
                {
                    if (int.TryParse(stringArray[i], out num))
                    {
                        intResult.Add(num);
                    }
                }
            }
            return intResult.ToArray();
        }

        public static float[] StringToFloatArray(string sSource, char cSeperator)
        {
            float[] intResult;
            string[] stringArray = sSource.Split(cSeperator);
            intResult = new float[stringArray.Length];
            for (int i = 0; i < stringArray.Length; i++)
            {
                float num;
                if (float.TryParse(stringArray[i], out num))
                {
                    intResult[i] = num;
                }
            }
            return intResult;
        }

        public static string[] StringArray(string sSource, char cSeperator)
        {
            string[] sArray = sSource.Split(cSeperator);
            return sArray;
        }
    }
}
