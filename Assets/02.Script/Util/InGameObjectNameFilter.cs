namespace DeskCat.FindIt.Scripts.Core.Main.System
{
    public static class InGameObjectNameFilter
    {
        public static string GetBaseGroupName(string objectName)
        {
            if (objectName == null) return string.Empty;
            
            if (!objectName.Contains("hide_")) return objectName;

            // "hide_" 이후의 문자열을 가져옴
            string baseName = objectName.Substring(objectName.IndexOf("hide_") + 5);
            
            // 마지막 숫자와 그 앞의 언더스코어를 제거
            int endIndex = baseName.Length - 1;
            while (endIndex >= 0 && (char.IsDigit(baseName[endIndex]) || baseName[endIndex] == '_'))
            {
                endIndex--;
            }
            
            return baseName.Substring(0, endIndex + 1).Trim();
        }
    }
}
