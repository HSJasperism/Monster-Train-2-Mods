using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardChanges
{
    public static class ModLocalization
    {
        public static readonly List<string[]> ModStrings = new List<string[]>(8);
        public static readonly List<string> UsedKeys = new List<string>(8);
        public static readonly string[] Header = new string[12]
        {
            "Key", "Type", "Desc", "Plural", "Group", "Descriptions", "English [en-US]", "French [fr-FR]",
            "German [de-DE]", "Russian", "Portuguese (Brazil)", "Chinese [zh-CN]"
        };

        public static void AddLocalization(string key, string en_us, string fr_fr = "", string de_de = "", string ru_ru = "", string pt_br = "", string zh_cn = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(en_us))
            {
                Logging.LogWarning($"AddLocalization called with invalid parameters: {key}, {en_us}");
                return;
            }
            lock (UsedKeys)
            {
                if (UsedKeys.Contains(key))
                {
                    Logging.LogWarning($"Duplicate key detected: {key}");
                    return;
                }
                UsedKeys.Add(key);
            }
            ModStrings.Add(new string[12] { key, "Text", "", "", "", "", en_us, fr_fr, de_de, ru_ru, pt_br, zh_cn });
        }

        public static void Apply()
        {
            if (ModStrings.Count == 0) return;

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            ModStrings.Insert(0, Header);

            string result = LocalizationManager.Sources.FirstOrDefault().Import_ModStrings(ModStrings);

            stopwatch.Stop();

            if (string.IsNullOrEmpty(result))
            {
                ModStrings.Clear();
                Logging.LogInfo($"Imported localization for {CardChanges.GUID} in {stopwatch.ElapsedMilliseconds}ms.");
            }
            else
            {
                ModStrings.RemoveAt(0);
                Logging.LogError($"Failed to import localization for {CardChanges.GUID} in {stopwatch.ElapsedMilliseconds}ms with the message:\n{result}");
            }
        }
    }

    public static class StringOperations
    {
        internal static bool ArrayContains(string MainText, params string[] texts)
        {
            foreach (string item in texts)
            {
                if (MainText.Equals(item, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        internal static int FindColumnHeader(string[] headers, params string[] headerToFind)
        {
            int cap = headers.Length;
            for (int n = 0; n < cap; n++)
            {
                if (ArrayContains(headers[n], headerToFind)) return n;
            }
            return -1;
        }

        // Needs work
        internal static void SimplifyPluralsMod(this LanguageSourceData dict)
        {
            TextPlurals textPlurals = new TextPlurals();
            List<List<ePluralType>> pluralsUsedByLanguages = LocalizationUtil.GetPluralsUsedByLanguages();
            string translation;
            foreach (TermData mTerm in dict.mTerms)
            {
                translation = textPlurals.Set(mTerm.Languages[0]).GenerateTranslation(useFallbackText: false, removeDuplicates: true);
                mTerm.SetTranslation(0, translation);

                for (int i = 1; i < mTerm.Languages.Length; i++)
                {
                    textPlurals.Set(mTerm.Languages[i]);
                    foreach (ePluralType value in Enum.GetValues(typeof(ePluralType)))
                    {
                        if (!pluralsUsedByLanguages[i].Contains(value)) textPlurals.ClearPlural(value);
                    }

                    mTerm.SetTranslation(i, textPlurals.GenerateTranslation());
                }
            }
        }

        internal static string GetPluralTranslation(string translation, TermData termData, int langIndex, ePluralType? pluralOnly)
        {
            TextPlurals textPlurals;
            if (pluralOnly.HasValue)
            {
                textPlurals = new TextPlurals(termData.Languages[langIndex]);
                textPlurals.SetPluralText(pluralOnly.GetValueOrDefault(), translation);
            }
            else
            {
                textPlurals = new TextPlurals(translation);
            }
            return textPlurals.GenerateTranslation();
        }

        static readonly string[] headerKey = new string[2] { "Key", "Keys" };
        static readonly string[] headerType = new string[1] { "Type" };
        static readonly string[] headerDesc = new string[2] { "Desc", "Description" };
        static readonly string[] headerPlural = new string[1] { "Plural" };
        static readonly string[] headerGroup = new string[1] { "Group" };
        static readonly string[] headerDescription = new string[1] { "Descriptions" };

        internal static string Import_ModStrings(this LanguageSourceData dict, List<string[]> ToImport)
        {
            string[] Headers = ToImport[0];
            int CurrentColumn = 0;
            if (FindColumnHeader(Headers, headerKey) != CurrentColumn) return "Bad Spreadsheet Format";
            CurrentColumn++;

            int TypeColumn = FindColumnHeader(Headers, headerType);
            if (TypeColumn >= 0)
            {
                if (TypeColumn != CurrentColumn) return "Bad Spreadsheet Format";
                CurrentColumn++;
            }

            int DescColumn = FindColumnHeader(Headers, headerDesc);
            if (DescColumn >= 0)
            {
                if (DescColumn != CurrentColumn) return "Bad Spreadsheet Format";
                CurrentColumn++;
            }

            int PluralColumn = FindColumnHeader(Headers, headerPlural);
            if (PluralColumn >= 0)
            {
                if (PluralColumn != CurrentColumn) return "Bad Spreadsheet Format";
                CurrentColumn++;
            }

            int GroupColumn = FindColumnHeader(Headers, headerGroup);
            if (GroupColumn >= 0)
            {
                if (GroupColumn != CurrentColumn) return "Bad Spreadsheet Format";
                CurrentColumn++;
            }

            int DescriptionColumn = FindColumnHeader(Headers, headerDescription);
            if (DescriptionColumn >= 0)
            {
                if (DescriptionColumn != CurrentColumn) return "Bad Spreadsheet Format";
                CurrentColumn++;
            }

            int TagColumns = CurrentColumn;

            // Everything before this is validation

            int LangCount = Math.Max(Headers.Length - TagColumns, 0);
            int[] LangIDs = new int[LangCount];

            string text;
            for (int i = 0; i < LangCount; i++)
            {
                text = Headers[i + TagColumns];
                if (string.IsNullOrEmpty(text))
                {
                    LangIDs[i] = -1;
                    continue;
                }

                if (text.StartsWith("$"))
                {
                    text = text[1..];
                }

                GoogleLanguages.UnPackCodeFromLanguageName(text, out string Language, out string code);
                LangIDs[i] = string.IsNullOrEmpty(code) ? dict.GetLanguageIndex(Language, AllowDiscartingRegion: true, SkipDisabled: false) : dict.GetLanguageIndexFromCode(code);
            }


            // More validation
            int dictLangCount = dict.mLanguages.Count;
            foreach (TermData term in dict.mTerms)
            {
                if (term.Languages.Length < dictLangCount)
                {
                    Logging.LogError("Term Data length is too low for " + term.ToString());
                }
            }

            string Term;
            string[] CurrentRow;
            ePluralType? pluralOnly = null;

            int TotalRows = ToImport.Count;
            bool skipRow;
            int BraceIndex;
            int mCap;
            int langCount;
            for (int RowIndex = 1; RowIndex < TotalRows; RowIndex++)
            {
                CurrentRow = ToImport[RowIndex];
                Term = CurrentRow[0];
                skipRow = false;
                if (Term.EndsWith("]"))
                {
                    BraceIndex = Term.LastIndexOf('[');
                    if (BraceIndex >= 0)
                    {
                        if (Enum.TryParse<ePluralType>(Term.Substring(BraceIndex + 1, Term.Length - BraceIndex - 2), ignoreCase: true, out var result))
                        {
                            pluralOnly = result;
                        }
                        else
                        {
                            Logging.LogError("Localization: Spreadsheet import: Unable to parse plural tag: " + Term.Substring(BraceIndex + 1, Term.Length - BraceIndex - 2));
                            skipRow = true;
                        }

                        Term = Term[..BraceIndex];
                    }
                }

                LanguageSourceData.ValidateFullTerm(ref Term);
                if (string.IsNullOrEmpty(Term) || skipRow)
                {
                    continue;
                }

                TermData termData2 = dict.GetTermData(Term);
                if (termData2 == null)
                {
                    Logging.LogError("GetTermData from " + Term + " failed");
                }

                mCap = Math.Min(LangIDs.Length, CurrentRow.Length - TagColumns);
                for (int m = 0; m < mCap; m++)
                {
                    if (string.IsNullOrEmpty(CurrentRow[m + TagColumns])) continue;

                    langCount = LangIDs[m];
                    if (langCount >= 0 && (0 & (1 << langCount)) == 0)
                    {
                        termData2.SetTranslation(langCount, GetPluralTranslation(CurrentRow[m + TagColumns], termData2, langCount, pluralOnly));
                    }
                }
            }

            dict.SimplifyPluralsMod();
            if (Application.isPlaying) dict.SaveLanguages(dict.HasUnloadedLanguages());
            return string.Empty;
        }
    }
}
