using System;
using System.Collections.Generic;
using Acornima.Helpers;

namespace Acornima;

// https://github.com/acornjs/acorn/blob/8.11.3/acorn/src/unicode-property-data.js

internal static class UnicodeProperties
{
    private static readonly HashSet<ReadOnlyMemory<char>> s_generalCategoryLookup = new(StringSliceEqualityComparer.Instance);
    private static readonly Dictionary<ReadOnlyMemory<char>, EcmaVersion> s_scriptValueLookup = new(StringSliceEqualityComparer.Instance);
    private static readonly Dictionary<ReadOnlyMemory<char>, EcmaVersion> s_binaryValueLookup = new(StringSliceEqualityComparer.Instance);
    private static readonly Dictionary<ReadOnlyMemory<char>, EcmaVersion> s_binaryOfStringsValueLookup = new(StringSliceEqualityComparer.Instance);

    static UnicodeProperties()
    {
        static void PopulateGeneralCategoryLookup(string name1, string name2)
        {
            s_generalCategoryLookup.Add(name1.AsMemory());
            s_generalCategoryLookup.Add(name2.AsMemory());
        }

        static void PopulateGeneralCategoryLookupAlt(string name1, string name2, string name3)
        {
            PopulateGeneralCategoryLookup(name1, name2);
            s_generalCategoryLookup.Add(name3.AsMemory());
        }

        PopulateGeneralCategoryLookup("L", "Letter");
        PopulateGeneralCategoryLookup("LC", "Cased_Letter");
        PopulateGeneralCategoryLookup("Lu", "Uppercase_Letter");
        PopulateGeneralCategoryLookup("Ll", "Lowercase_Letter");
        PopulateGeneralCategoryLookup("Lt", "Titlecase_Letter");
        PopulateGeneralCategoryLookup("Lm", "Modifier_Letter");
        PopulateGeneralCategoryLookup("Lo", "Other_Letter");

        PopulateGeneralCategoryLookupAlt("M", "Mark", "Combining_Mark");
        PopulateGeneralCategoryLookup("Mn", "Nonspacing_Mark");
        PopulateGeneralCategoryLookup("Mc", "Spacing_Mark");
        PopulateGeneralCategoryLookup("Me", "Enclosing_Mark");

        PopulateGeneralCategoryLookup("N", "Number");
        PopulateGeneralCategoryLookupAlt("Nd", "Decimal_Number", "digit");
        PopulateGeneralCategoryLookup("Nl", "Letter_Number");
        PopulateGeneralCategoryLookup("No", "Other_Number");

        PopulateGeneralCategoryLookupAlt("P", "Punctuation", "punct");
        PopulateGeneralCategoryLookup("Pc", "Connector_Punctuation");
        PopulateGeneralCategoryLookup("Pd", "Dash_Punctuation");
        PopulateGeneralCategoryLookup("Ps", "Open_Punctuation");
        PopulateGeneralCategoryLookup("Pe", "Close_Punctuation");
        PopulateGeneralCategoryLookup("Pi", "Initial_Punctuation");
        PopulateGeneralCategoryLookup("Pf", "Final_Punctuation");
        PopulateGeneralCategoryLookup("Po", "Other_Punctuation");

        PopulateGeneralCategoryLookup("S", "Symbol");
        PopulateGeneralCategoryLookup("Sm", "Math_Symbol");
        PopulateGeneralCategoryLookup("Sc", "Currency_Symbol");
        PopulateGeneralCategoryLookup("Sk", "Modifier_Symbol");
        PopulateGeneralCategoryLookup("So", "Other_Symbol");

        PopulateGeneralCategoryLookup("Z", "Separator");
        PopulateGeneralCategoryLookup("Zs", "Space_Separator");
        PopulateGeneralCategoryLookup("Zl", "Line_Separator");
        PopulateGeneralCategoryLookup("Zp", "Paragraph_Separator");

        PopulateGeneralCategoryLookup("C", "Other");
        PopulateGeneralCategoryLookupAlt("Cc", "Control", "cntrl");
        PopulateGeneralCategoryLookup("Cf", "Format");
        PopulateGeneralCategoryLookup("Cs", "Surrogate");
        PopulateGeneralCategoryLookup("Co", "Private_Use");
        PopulateGeneralCategoryLookup("Cn", "Unassigned");

        static void PopulateVersionLookup(Dictionary<ReadOnlyMemory<char>, EcmaVersion> dictionary, EcmaVersion ecmaVersion, params string[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                dictionary[values[i].AsMemory()] = ecmaVersion;
            }
        }

        PopulateVersionLookup(s_scriptValueLookup, EcmaVersion.ES14,
            "Berf", "Beria_Erfe", "Gara", "Garay", "Gukh", "Gurung_Khema", "Hrkt", "Katakana_Or_Hiragana", "Kawi", "Kirat_Rai", "Krai", "Nag_Mundari", "Nagm", "Ol_Onal", "Onao", "Sidetic", "Sidt", "Sunu", "Sunuwar", "Tai_Yo", "Tayo", "Todhri", "Todr", "Tolong_Siki", "Tols", "Tulu_Tigalari", "Tutg", "Unknown", "Zzzz");

        PopulateVersionLookup(s_scriptValueLookup, EcmaVersion.ES13,
            "Cypro_Minoan", "Cpmn", "Old_Uyghur", "Ougr", "Tangsa", "Tnsa", "Toto", "Vithkuqi", "Vith");

        PopulateVersionLookup(s_scriptValueLookup, EcmaVersion.ES12,
            "Chorasmian", "Chrs", "Diak", "Dives_Akuru", "Khitan_Small_Script", "Kits", "Yezi", "Yezidi");

        PopulateVersionLookup(s_scriptValueLookup, EcmaVersion.ES11,
            "Elymaic", "Elym", "Nandinagari", "Nand", "Nyiakeng_Puachue_Hmong", "Hmnp", "Wancho", "Wcho");

        PopulateVersionLookup(s_scriptValueLookup, EcmaVersion.ES10,
            "Dogra", "Dogr", "Gunjala_Gondi", "Gong", "Hanifi_Rohingya", "Rohg", "Makasar", "Maka", "Medefaidrin", "Medf", "Old_Sogdian", "Sogo", "Sogdian", "Sogd");

        PopulateVersionLookup(s_scriptValueLookup, EcmaVersion.ES9,
            "Adlam", "Adlm", "Ahom", "Anatolian_Hieroglyphs", "Hluw", "Arabic", "Arab", "Armenian", "Armn", "Avestan", "Avst", "Balinese", "Bali", "Bamum", "Bamu", "Bassa_Vah", "Bass", "Batak", "Batk", "Bengali", "Beng", "Bhaiksuki", "Bhks", "Bopomofo", "Bopo", "Brahmi", "Brah", "Braille", "Brai", "Buginese", "Bugi", "Buhid", "Buhd", "Canadian_Aboriginal", "Cans", "Carian", "Cari", "Caucasian_Albanian", "Aghb", "Chakma", "Cakm", "Cham", "Cham", "Cherokee", "Cher", "Common", "Zyyy", "Coptic", "Copt", "Qaac", "Cuneiform", "Xsux", "Cypriot", "Cprt", "Cyrillic", "Cyrl", "Deseret", "Dsrt", "Devanagari", "Deva", "Duployan", "Dupl", "Egyptian_Hieroglyphs", "Egyp", "Elbasan", "Elba", "Ethiopic", "Ethi", "Georgian", "Geor", "Glagolitic", "Glag", "Gothic", "Goth", "Grantha", "Gran", "Greek", "Grek", "Gujarati", "Gujr", "Gurmukhi", "Guru", "Han", "Hani", "Hangul", "Hang", "Hanunoo", "Hano", "Hatran", "Hatr", "Hebrew", "Hebr", "Hiragana", "Hira", "Imperial_Aramaic", "Armi", "Inherited", "Zinh", "Qaai", "Inscriptional_Pahlavi", "Phli", "Inscriptional_Parthian", "Prti", "Javanese", "Java", "Kaithi", "Kthi", "Kannada", "Knda", "Katakana", "Kana", "Kayah_Li", "Kali", "Kharoshthi", "Khar", "Khmer", "Khmr", "Khojki", "Khoj", "Khudawadi", "Sind", "Lao", "Laoo", "Latin", "Latn", "Lepcha", "Lepc", "Limbu", "Limb", "Linear_A", "Lina", "Linear_B", "Linb", "Lisu", "Lisu", "Lycian", "Lyci", "Lydian", "Lydi", "Mahajani", "Mahj", "Malayalam", "Mlym", "Mandaic", "Mand", "Manichaean", "Mani", "Marchen", "Marc", "Masaram_Gondi", "Gonm", "Meetei_Mayek", "Mtei", "Mende_Kikakui", "Mend", "Meroitic_Cursive", "Merc", "Meroitic_Hieroglyphs", "Mero", "Miao", "Plrd", "Modi", "Mongolian", "Mong", "Mro", "Mroo", "Multani", "Mult", "Myanmar", "Mymr", "Nabataean", "Nbat", "New_Tai_Lue", "Talu", "Newa", "Newa", "Nko", "Nkoo", "Nushu", "Nshu", "Ogham", "Ogam", "Ol_Chiki", "Olck", "Old_Hungarian", "Hung", "Old_Italic", "Ital", "Old_North_Arabian", "Narb", "Old_Permic", "Perm", "Old_Persian", "Xpeo", "Old_South_Arabian", "Sarb", "Old_Turkic", "Orkh", "Oriya", "Orya", "Osage", "Osge", "Osmanya", "Osma", "Pahawh_Hmong", "Hmng", "Palmyrene", "Palm", "Pau_Cin_Hau", "Pauc", "Phags_Pa", "Phag", "Phoenician", "Phnx", "Psalter_Pahlavi", "Phlp", "Rejang", "Rjng", "Runic", "Runr", "Samaritan", "Samr", "Saurashtra", "Saur", "Sharada", "Shrd", "Shavian", "Shaw", "Siddham", "Sidd", "SignWriting", "Sgnw", "Sinhala", "Sinh", "Sora_Sompeng", "Sora", "Soyombo", "Soyo", "Sundanese", "Sund", "Syloti_Nagri", "Sylo", "Syriac", "Syrc", "Tagalog", "Tglg", "Tagbanwa", "Tagb", "Tai_Le", "Tale", "Tai_Tham", "Lana", "Tai_Viet", "Tavt", "Takri", "Takr", "Tamil", "Taml", "Tangut", "Tang", "Telugu", "Telu", "Thaana", "Thaa", "Thai", "Tibetan", "Tibt", "Tifinagh", "Tfng", "Tirhuta", "Tirh", "Ugaritic", "Ugar", "Vai", "Vaii", "Warang_Citi", "Wara", "Yi", "Yiii", "Zanabazar_Square", "Zanb");

        // https://tc39.es/ecma262/#table-binary-unicode-properties
        PopulateVersionLookup(s_binaryValueLookup, EcmaVersion.ES12,
            "EBase", "EComp", "EMod", "EPres", "ExtPict");

        PopulateVersionLookup(s_binaryValueLookup, EcmaVersion.ES10,
            "Extended_Pictographic");

        PopulateVersionLookup(s_binaryValueLookup, EcmaVersion.ES9,
            "ASCII", "ASCII_Hex_Digit", "AHex", "Alphabetic", "Alpha", "Any", "Assigned", "Bidi_Control", "Bidi_C", "Bidi_Mirrored", "Bidi_M", "Case_Ignorable", "CI", "Cased", "Changes_When_Casefolded", "CWCF", "Changes_When_Casemapped", "CWCM", "Changes_When_Lowercased", "CWL", "Changes_When_NFKC_Casefolded", "CWKCF", "Changes_When_Titlecased", "CWT", "Changes_When_Uppercased", "CWU", "Dash", "Default_Ignorable_Code_Point", "DI", "Deprecated", "Dep", "Diacritic", "Dia", "Emoji", "Emoji_Component", "Emoji_Modifier", "Emoji_Modifier_Base", "Emoji_Presentation", "Extender", "Ext", "Grapheme_Base", "Gr_Base", "Grapheme_Extend", "Gr_Ext", "Hex_Digit", "Hex", "IDS_Binary_Operator", "IDSB", "IDS_Trinary_Operator", "IDST", "ID_Continue", "IDC", "ID_Start", "IDS", "Ideographic", "Ideo", "Join_Control", "Join_C", "Logical_Order_Exception", "LOE", "Lowercase", "Lower", "Math", "Noncharacter_Code_Point", "NChar", "Pattern_Syntax", "Pat_Syn", "Pattern_White_Space", "Pat_WS", "Quotation_Mark", "QMark", "Radical", "Regional_Indicator", "RI", "Sentence_Terminal", "STerm", "Soft_Dotted", "SD", "Terminal_Punctuation", "Term", "Unified_Ideograph", "UIdeo", "Uppercase", "Upper", "Variation_Selector", "VS", "White_Space", "space", "XID_Continue", "XIDC", "XID_Start", "XIDS");

        // https://tc39.es/ecma262/#table-binary-unicode-properties-of-strings
        PopulateVersionLookup(s_binaryOfStringsValueLookup, EcmaVersion.ES15,
            "Basic_Emoji", "Emoji_Keycap_Sequence", "RGI_Emoji_Modifier_Sequence", "RGI_Emoji_Flag_Sequence", "RGI_Emoji_Tag_Sequence", "RGI_Emoji_ZWJ_Sequence", "RGI_Emoji");
    }

    public static bool IsAllowedGeneralCategoryValue(ReadOnlyMemory<char> propertyValue)
    {
        return s_generalCategoryLookup.Contains(propertyValue);
    }

    public static bool IsAllowedScriptValue(ReadOnlyMemory<char> propertyValue, EcmaVersion ecmaVersion)
    {
        return s_scriptValueLookup.TryGetValue(propertyValue, out var version) && ecmaVersion >= version;
    }

    public static bool IsAllowedBinaryValue(ReadOnlyMemory<char> propertyValue, EcmaVersion ecmaVersion)
    {
        return s_binaryValueLookup.TryGetValue(propertyValue, out var version) && ecmaVersion >= version;
    }

    public static bool IsAllowedBinaryOfStringsValue(ReadOnlyMemory<char> propertyValue, EcmaVersion ecmaVersion)
    {
        return s_binaryOfStringsValueLookup.TryGetValue(propertyValue, out var version) && ecmaVersion >= version;
    }
}
