using UnityEngine;
using I2.Loc;
using System.Collections.Generic;

namespace Util
{
    public static class I2LocalizationHelper
    {
        /// <summary>
        /// 한글 원본 문자열을 I2 Localization 데이터에서 찾아 현재 언어로 번역해주는 헬퍼 메서드입니다.
        /// </summary>
        /// <param name="koreanText">원본 한글 문자열 (예: "당근 디저트 세트")</param>
        /// <returns>현재 언어로 번역된 문자열. 찾지 못하면 원본 문자열 반환.</returns>
        public static string GetLocalizedTextFromKorean(string koreanText)
        {
            if (string.IsNullOrEmpty(koreanText)) return koreanText;

            // 1. 혹시라도 Term(키) 자체가 한글 문자열과 동일하게 등록되어 있는 경우 직접 반환
            string directTranslation = LocalizationManager.GetTranslation(koreanText);
            if (!string.IsNullOrEmpty(directTranslation))
            {
                return directTranslation;
            }

            // 2. 키가 아니라 한국어 텍스트 데이터 값으로 들어있는 경우를 위해 소스를 순회하며 검색
            var source = LocalizationManager.GetSourceContaining(koreanText, false);
            if (source == null && LocalizationManager.Sources.Count > 0)
            {
                source = LocalizationManager.Sources[0]; // 기본 소스 사용
            }

            if (source != null)
            {
                // 인덱스 찾기 ("Korean" 또는 "한국어")
                int korIndex = source.GetLanguageIndex("Korean");
                if (korIndex < 0) korIndex = source.GetLanguageIndex("한국어");

                if (korIndex >= 0)
                {
                    List<string> terms = source.GetTermsList();
                    foreach (var term in terms)
                    {
                        var termData = source.GetTermData(term);
                        if (termData != null && termData.Languages.Length > korIndex)
                        {
                            if (termData.Languages[korIndex] == koreanText)
                            {
                                // 일치하는 항목을 찾으면 해당 Term을 현재 언어로 번역해서 반환
                                string translatedText = LocalizationManager.GetTranslation(term);
                                if (!string.IsNullOrEmpty(translatedText))
                                {
                                    return translatedText;
                                }
                            }
                        }
                    }
                }
            }

            // 3. I2 번역 데이터에 아예 등록되지 않은 경우 원래 텍스트를 그대로 반환 (안전 장치)
            return koreanText;
        }
    }
}