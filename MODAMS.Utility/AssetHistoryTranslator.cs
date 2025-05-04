using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODAMS.Utility
{
    public static class AssetHistoryTranslator
    {
        public static string Translate(string englishMessage)
        {
            if (englishMessage.StartsWith("Asset Registered by "))
            {
                var user = englishMessage.Substring("Asset Registered by ".Length);
                return $"Hantida waxaa diiwaangeliyay {user}";
            }
            else if (englishMessage.StartsWith("Asset Transferred from "))
            {
                var parts = englishMessage.Replace("Asset Transferred from ", "").Split(" to ");
                var fromStore = parts[0];
                var toStore = parts[1];
                return $"Hantida waxaa laga wareejiyay {fromStore} waxaa loo wareejiyay {toStore}";
            }
            else if (englishMessage.StartsWith("Asset disposed by "))
            {
                var user = englishMessage.Substring("Asset disposed by ".Length);
                return $"Hantida waxa qubay {user}";
            }
            else if (englishMessage.StartsWith("Asset un-disposed by "))
            {
                var user = englishMessage.Substring("Asset un-disposed by ".Length);
                return $"Hantida waxa dib usoo celiyay {user}";
            }
            else if (englishMessage.StartsWith("Asset Deleted by "))
            {
                var user = englishMessage.Substring("Asset Deleted by ".Length);
                return $"Hantida waxaa tirtiray {user}";
            }
            else if (englishMessage.StartsWith("Asset Recovered by "))
            {
                var user = englishMessage.Substring("Asset Recovered by ".Length);
                return $"Hantida waxaa soo helay {user}";
            }
            else
            {
                return englishMessage; 
            }
        }
    }
}
