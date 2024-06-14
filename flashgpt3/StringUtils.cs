using Microsoft.ProgramSynthesis.Transformation.Formula.Semantics.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlashGPT3
{
    internal class StringUtils
    {
        public static readonly Regex alphanumeric = new Regex(@"[a-zA-Z0-9]*", RegexOptions.Compiled);

        class MyStrComparer : Comparer<string>
        {
            string delimiter;
            bool isAscending;

            public MyStrComparer(string aStr, bool ascending)
            {
                delimiter = aStr;
                isAscending = ascending;
            }

            public override int Compare(string x, string y)
            {
                var r = GetMySubstring(x).CompareTo(GetMySubstring(y));
                return isAscending ? r : -r;
            }

            string GetMySubstring(string str)
            {
                return str.IndexOf(delimiter) != -1 ? str.Substring(str.LastIndexOf(delimiter)) : string.Empty;
            }

        }

        //public static List<String> findCommonSubstring(String src, String tgt)
        //{
        //    List<String> resList = new List<String>();

        //    List<String> tokens1 = new List<String>();

        //    foreach (Match match in StringUtils.alphanumeric.Matches(src))
        //    {
        //        tokens1.Add(match.Value);
        //    }
        //    List<String> tokens2 = new List<string>();
        //    foreach (Match match in StringUtils.alphanumeric.Matches(tgt))
        //    {
        //        tokens2.Add(match.Value);
        //    }

        //    int i = 0;
        //    while (i < tokens1.Count)
        //    {
        //        int j = 0;
        //        while (j < tokens2.Count)
        //        {
        //            int k = 0;
        //            while ((i + k < tokens1.Count) && (j + k < tokens2.Count) && (tokens1[i + k] == tokens2[j + k])) 
        //            {
        //                k += 1;

        //                if (k > 1)
        //                    resList.Append(String.Join(" ", src.Skip(i).Take(k)));
        //                else if (k == 1)
        //                    resList.Append(src.Skip(i).Take(k));

        //                i += Math.Max(1, k);
        //                j += Math.Max(1, k);
        //            }

        //        }

        //    }


        //    return resList;
        //}

        public static string[] findCommonSubstring(string left, string right)
        {
            List<string> result = new List<string>();
            string[] rightArray = right.Split();
            string[] leftArray = left.Split();

            //result.AddRange(rightArray.Where(r => leftArray.Any(l => l.StartsWith(r))));

            // must check other way in case left array contains smaller words than right array
            result.AddRange(leftArray.Where(l => rightArray.Any(r => r.StartsWith(l))));

            String[] resultTokArr = result.Distinct().ToArray();
            //Array.Sort(resultTokArr, resIndex.ToArray());
            List<string> resultArr = new List<string>();

            string currstr = "";
            foreach (string str in resultTokArr)
            {

                if (String.IsNullOrEmpty(currstr))
                {
                    currstr = str;
                }
                else
                {
                    //currstr = string.Join(" ", currstr.Split(" ").Append(str));
                    currstr = currstr + " " + str;
                }
                if (!left.Contains(currstr) || !right.Contains(currstr))
                {
                    string tempstr = str;
                    currstr = string.Join(" ", currstr.Split(" ").Take(currstr.Split().Length - 1));
                    resultArr.Add(currstr);
                    currstr = tempstr;
                }
            }
            if (left.Contains(currstr) && right.Contains(currstr) && !resultArr.Contains(currstr))
            {
                if(left.StartsWith(currstr) && right.StartsWith(currstr))
                {
                    resultArr.Add(currstr);
                } else if (!left.StartsWith(currstr) && left[left.IndexOf(currstr) - 1].Equals(' ') && !right.StartsWith(currstr) && right[right.IndexOf(currstr) - 1].Equals(' '))
                {
                    resultArr.Add(currstr);
                } else if (!left.StartsWith(currstr) && left[left.IndexOf(currstr) - 1].Equals(' ') && right.StartsWith(currstr))
                {
                    resultArr.Add(currstr);
                } else if (left.StartsWith(currstr) && !right.StartsWith(currstr) && right[right.IndexOf(currstr) - 1].Equals(' '))
                {
                    resultArr.Add(currstr);
                }
                
            }

            return resultArr.ToArray();
        }



        //public static string[] findCommonSubstring(string left, string right)
        //{
        //    List<string> wordsLeft = GetWords(left);
        //    List<string> wordsRight = GetWords(right);

        //    var commonSubstrings = new List<string>();

        //    // Compare consecutive sequences of words to find common substrings
        //    for (int i = 0; i < wordsLeft.Count; i++)
        //    {
        //        for (int j = 0; j < wordsRight.Count; j++)
        //        {
        //            int leftIndex = i;
        //            int rightIndex = j;
        //            while (leftIndex < wordsLeft.Count && rightIndex < wordsRight.Count &&
        //                   wordsLeft[leftIndex].Equals(wordsRight[rightIndex], StringComparison.OrdinalIgnoreCase))
        //            {
        //                leftIndex++;
        //                rightIndex++;
        //            }

        //            if (leftIndex > i)
        //            {
        //                string commonSubstring = string.Join(" ", wordsLeft.GetRange(i, leftIndex - i));
        //                commonSubstrings.Add(commonSubstring);
        //            }
        //        }
        //    }

        //    return commonSubstrings.ToArray();
        //}

        //public static string[] findCommonSubstring(string left, string right)
        //{
        //    List<string> LTokens = GetWords(left);
        //    List<string> RTokens = GetWords(right);

        //    HashSet<int> visited_L = new HashSet<int>();
        //    HashSet<int> visited_R = new HashSet<int>();

        //    List<string> commonSubstring = new List<string>();

        //    for (int i = 0; i < LTokens.Count; i++)
        //    {
        //        if (visited_L.Contains(i)) { continue; }
        //        for (int j = 0; j < RTokens.Count; j++)
        //        {
        //            if (visited_R.Contains(j)) { continue; }
        //            if (LTokens[i].Equals(RTokens[j]))
        //            {
        //                int inc = 1;
        //                string LTemp = LTokens[i].ToString();
        //                string RTemp = RTokens[j].ToString();

        //                while(i + inc < LTokens.Count && j + inc < RTokens.Count) 
        //                {
        //                    LTemp = LTemp + " " + LTokens[i + inc].ToString();
        //                    RTemp = RTemp + " " + RTokens[j + inc].ToString();
        //                    if(LTemp.Equals(RTemp))
        //                    {
        //                        visited_L.Add(i + inc);
        //                        visited_R.Add(j + inc);
        //                        inc++;
        //                    } else
        //                    {
        //                        LTemp = string.Join(" ", LTemp.Split().Take(LTemp.Split().Length - 1));
        //                        break;
        //                    }
        //                }
        //                commonSubstring.Add(LTemp);
        //            } else
        //            {
        //                visited_L.Add((int)i);
        //                visited_R.Add((int)j);
        //                commonSubstring.Add(LTokens[i]);
        //            }
        //        }

        //    }

        //    return commonSubstring.ToArray();
        //}

        public static List<string> GetWords(string input)
        {
            var words = new List<string>();
            MatchCollection matches = Regex.Matches(input, @"\w+");

            foreach (Match match in matches)
            {
                words.Add(match.Value);
            }

            return words;
        }
        //public static List<string> GetWords(string input)
        //{
        //    return input.Split(new char[] { ' ' }).ToList();
        //}

        public static List<string> GetAllSubstringForward(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return new List<string>();
            }
            List<string> substrings = new List<string>();

            for (int i = 1; i <= str.Length; i++)
            {
                substrings.Add(str.Substring(0, i));
            }

            return substrings;
        }

        // (smaller/smallest)

        public static List<string> GetAllSubStringBackward(string str)
        {
            if (str.IsNullOrEmpty())
            {
                return new List<string>();
            }
            List<string> substrings = new List<string>();

            for (int i = str.Length; i > 0; i--)
            {
                substrings.Add(str.Substring(str.Length - i));
            }

            return substrings;
        }
    }
}
