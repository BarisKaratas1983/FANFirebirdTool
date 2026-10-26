using System;

namespace FANFirebirdTool.Engines
{
    public static class CharacterSets
    {
        public static readonly string[] All =
        {
            "UTF8", "WIN1254", "ISO8859_9", "NONE",

            "ASCII", "BIG_5", "CP943C", "CYRL",
            "DOS437", "DOS737", "DOS775", "DOS850", "DOS852", "DOS857", "DOS858", "DOS860",
            "DOS861", "DOS862", "DOS863", "DOS864", "DOS865", "DOS866", "DOS869",
            "EUCJ_0208", "GB18030", "GB_2312", "GBK",
            "ISO8859_1", "ISO8859_2", "ISO8859_3", "ISO8859_4", "ISO8859_5", "ISO8859_6",
            "ISO8859_7", "ISO8859_8", "ISO8859_13",
            "KOI8R", "KOI8U", "KSC_5601", "NEXT", "OCTETS", "SJIS_0208", "TIS620", "UNICODE_FSS",
            "WIN1250", "WIN1251", "WIN1252", "WIN1253", "WIN1255", "WIN1256", "WIN1257", "WIN1258"
        };
        public static bool Contains(string name)
        {
            return Array.IndexOf(All, name) >= 0;
        }
    }
}