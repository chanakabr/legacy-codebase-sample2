using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Tvinci.Helpers
{
    public static class TextHelper
    {
        //private const double defaultSize = 5.6;
        //static Dictionary<char, double> sizeDictionary = new Dictionary<char, double>();
        //static TextHelper()
        //{
        //    sizeDictionary.Add('א', 7);
        //    sizeDictionary.Add('ב', 7);
        //    sizeDictionary.Add('ג', 6);
        //    sizeDictionary.Add('ד', 7);
        //    sizeDictionary.Add('ה', 7);
        //    sizeDictionary.Add('ו', 3);
        //    sizeDictionary.Add('ז', 5);
        //    sizeDictionary.Add('ח', 6);
        //    sizeDictionary.Add('ט', 8);
        //    sizeDictionary.Add('י', 3);
        //    sizeDictionary.Add('כ', 6);
        //    sizeDictionary.Add('ל', 6);
        //    sizeDictionary.Add('מ', 8);
        //    sizeDictionary.Add('נ', 5);
        //    sizeDictionary.Add('ס', 7);
        //    sizeDictionary.Add('ע', 7);
        //    sizeDictionary.Add('פ', 7);
        //    sizeDictionary.Add('צ', 6);
        //    sizeDictionary.Add('ק', 7);
        //    sizeDictionary.Add('ר', 6);
        //    sizeDictionary.Add('ש', 9);
        //    sizeDictionary.Add('ת', 8);
        //    sizeDictionary.Add('1', 5);
        //    sizeDictionary.Add('2', 7);
        //    sizeDictionary.Add('3', 7);
        //    sizeDictionary.Add('4', 7);
        //    sizeDictionary.Add('5', 7);
        //    sizeDictionary.Add('6', 7);
        //    sizeDictionary.Add('7', 7);
        //    sizeDictionary.Add('8', 7);
        //    sizeDictionary.Add('9', 7);
        //    sizeDictionary.Add('0', 7);
        //    sizeDictionary.Add('.', 3);
        //    sizeDictionary.Add(',', 3);
        //    sizeDictionary.Add('!', 3);
        //    sizeDictionary.Add(':', 3);
        //    sizeDictionary.Add('"', 6);
        //    sizeDictionary.Add('-', 5);
        //    sizeDictionary.Add(' ', 2.5);
        //    sizeDictionary.Add('\'', 3);
        //    sizeDictionary.Add('ף', 7);
        //    sizeDictionary.Add('ך', 6);
        //    sizeDictionary.Add('ץ', 6);
        //    sizeDictionary.Add('ם', 7);
        //    sizeDictionary.Add('ן', 3);
        //    sizeDictionary.Add('A', 8);
        //    sizeDictionary.Add('B', 8);
        //    sizeDictionary.Add('C', 8);
        //    sizeDictionary.Add('D', 8);
        //    sizeDictionary.Add('E', 7);
        //    sizeDictionary.Add('F', 6);
        //    sizeDictionary.Add('G', 8);
        //    sizeDictionary.Add('H', 8);
        //    sizeDictionary.Add('I', 3);
        //    sizeDictionary.Add('J', 7);
        //    sizeDictionary.Add('K', 8);
        //    sizeDictionary.Add('L', 7);
        //    sizeDictionary.Add('M', 10);
        //    sizeDictionary.Add('N', 8);
        //    sizeDictionary.Add('O', 9);
        //    sizeDictionary.Add('P', 8);
        //    sizeDictionary.Add('Q', 9);
        //    sizeDictionary.Add('R', 8);
        //    sizeDictionary.Add('S', 7);
        //    sizeDictionary.Add('T', 7);
        //    sizeDictionary.Add('U', 8);
        //    sizeDictionary.Add('V', 8);
        //    sizeDictionary.Add('W', 12);
        //    sizeDictionary.Add('X', 8);
        //    sizeDictionary.Add('Y', 7);
        //    sizeDictionary.Add('Z', 8);
        //    sizeDictionary.Add('a', 7);
        //    sizeDictionary.Add('b', 7);
        //    sizeDictionary.Add('c', 7);
        //    sizeDictionary.Add('d', 7);
        //    sizeDictionary.Add('e', 7);
        //    sizeDictionary.Add('f', 4);
        //    sizeDictionary.Add('g', 7);
        //    sizeDictionary.Add('h', 7);
        //    sizeDictionary.Add('i', 3);
        //    sizeDictionary.Add('j', 4);
        //    sizeDictionary.Add('k', 7);
        //    sizeDictionary.Add('l', 3);
        //    sizeDictionary.Add('m', 11);
        //    sizeDictionary.Add('n', 7);
        //    sizeDictionary.Add('o', 7);
        //    sizeDictionary.Add('p', 7);
        //    sizeDictionary.Add('q', 7);
        //    sizeDictionary.Add('r', 6);
        //    sizeDictionary.Add('s', 7);
        //    sizeDictionary.Add('t', 4);
        //    sizeDictionary.Add('u', 7);
        //    sizeDictionary.Add('v', 6);
        //    sizeDictionary.Add('w', 10);
        //    sizeDictionary.Add('x', 7);
        //    sizeDictionary.Add('y', 6);
        //    sizeDictionary.Add('z', 7);

        //}

        //private const int threeDotSize = 9;

        public static string FormatText(object originalString, float fontSize, int maxWidth, int numberOfLines)
        {
            return FormatText(originalString, fontSize, maxWidth, numberOfLines, string.Empty);
        }

        public static string FormatText(object originalString, float fontSize, int maxWidth, int numberOfLines, string substractText)
        {
            if (originalString == null || string.IsNullOrEmpty(originalString.ToString()))
                return string.Empty;

            return GetCutString(originalString, fontSize, maxWidth, numberOfLines, substractText);
        }

        //public static string FormatText(string text, double maxSize)
        //{
        //    return FormatText(text, maxSize, string.Empty);
        //}

        //public static string FormatText(string text, double maxSize, string substractText)
        //{
        //    if (!string.IsNullOrEmpty(substractText))
        //    {
        //        maxSize -= CalculateTextLength(substractText);

        //    }
        //    if (string.IsNullOrEmpty(text) || maxSize <= threeDotSize)
        //    {
        //        return text;
        //    }

        //    int lastSpaceIndex = -1;
        //    double safeSize = maxSize - threeDotSize;
        //    int safeModeIndex = -1;
        //    double size = 0;
        //    bool shouldCut = false;
        //    bool storedSafeMode = false;

        //    for(int i=0;i<text.Length;i++)
        //    {
        //        double letterSize;

        //        if (!sizeDictionary.TryGetValue(text[i], out letterSize))                
        //        {
        //            letterSize = defaultSize;
        //        }

        //        if (storedSafeMode || size + letterSize > safeSize)
        //        {
        //            if (!storedSafeMode)
        //            {
        //                storedSafeMode = true;
        //                safeModeIndex = i - 1;
        //            }

        //            if (size + letterSize > maxSize)
        //            {
        //                shouldCut = true;
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            if (text[i] == ' ')
        //            {
        //                lastSpaceIndex = i;
        //            }
        //        }

        //        size += letterSize;


        //    }

        //    if (shouldCut)
        //    {
        //        if (safeModeIndex <= 0)
        //        {
        //            return text;
        //        }

        //        int releventIndex = (lastSpaceIndex != -1 && (safeModeIndex - lastSpaceIndex) <= 2) ? lastSpaceIndex : safeModeIndex;
        //        return string.Format("{0}...", text.Substring(0, releventIndex));
        //    }
        //    else
        //    {
        //        return text;
        //    }
        //}

        //public static double CalculateTextLength(string text)
        //{
        //    double size = 0;

        //    for (int i = 0; i < text.Length; i++)
        //    {
        //        double letterSize;

        //        if (!sizeDictionary.TryGetValue(text[i], out letterSize))
        //        {
        //            letterSize = defaultSize;
        //        }

        //        size += letterSize;
        //    }

        //    return size;
        //}

        public static string GetCutString(object originalString, float fontSize, int maxWidth, int numberOfLines, string substractText)
        {
            if (originalString == null || string.IsNullOrEmpty(originalString.ToString()))
                return string.Empty;

            Bitmap bmp = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(bmp);
            g.PageUnit = GraphicsUnit.Pixel;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Font fnt = new Font("Arial", fontSize, GraphicsUnit.Pixel);
            string orig = originalString.ToString();
            string dots = "...";

            StringFormat format = new StringFormat(StringFormat.GenericTypographic);
            format.Trimming = StringTrimming.Word;

            int availWidth = maxWidth;

            // Check if only one line and given a substract text
            if (numberOfLines == 1 && !string.IsNullOrEmpty(substractText))
            {
                // Measure substract string length
                SizeF subSize = g.MeasureString(substractText, fnt, 100000, format);
                availWidth -= (int)subSize.Width;
            }            
            
            // Calculate total height by number of lines
            float maxHeight = (float)fnt.Height * (float)numberOfLines;

            // See if no cutting needed
            SizeF totalSize = g.MeasureString(orig, fnt, availWidth, format);
            if (totalSize.Height <= maxHeight && totalSize.Width <= availWidth)
                return orig;

            for (int i = 0; i < orig.Length + 1; i++)
            {
                // Check if string takes more than maximum height
                if (g.MeasureString(orig.Substring(0, i), fnt, availWidth, format).Height > maxHeight)
                {
                    // Run back and try to fit dots
                    for (int j = i - 1; j >= 0; j--)
                    {
                        string cutString = orig.Substring(0, j).Trim() + dots;
                        if (g.MeasureString(cutString, fnt, availWidth, format).Height <= maxHeight)
                        {
                            return cutString;
                        }
                    }
                }
            }
            return orig;
        }
    }
}
