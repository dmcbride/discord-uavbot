using System.Collections.Generic;

namespace uav.logic.Extensions
{
    internal class NaturalStringComparer : IComparer<object>
    {
        public int Compare(object? x, object? y)
        {
            var xStr = x?.ToString();
            var yStr = y?.ToString();

            if (xStr is null && yStr is null)
            {
                return 0;
            }
            else if (xStr is null)
            {
                return -1;
            }
            else if (yStr is null)
            {
                return 1;
            }

            var xLen = xStr.Length;
            var yLen = yStr.Length;
            var xIndex = 0;
            var yIndex = 0;
            while (xIndex < xLen && yIndex < yLen)
            {
                var xChar = xStr[xIndex];
                var yChar = yStr[yIndex];
                if (char.IsDigit(xChar) && char.IsDigit(yChar))
                {
                    var xNumber = 0;
                    var yNumber = 0;
                    while (xIndex < xLen && char.IsDigit(xStr[xIndex]))
                    {
                        xNumber = xNumber * 10 + (xStr[xIndex] - '0');
                        xIndex++;
                    }
                    while (yIndex < yLen && char.IsDigit(yStr[yIndex]))
                    {
                        yNumber = yNumber * 10 + (yStr[yIndex] - '0');
                        yIndex++;
                    }
                    if (xNumber != yNumber)
                    {
                        return xNumber.CompareTo(yNumber);
                    }
                }
                else
                {
                    if (xChar != yChar)
                    {
                        return xChar.CompareTo(yChar);
                    }
                    xIndex++;
                    yIndex++;
                }
            }
            return xLen.CompareTo(yLen);
        }
    }
}