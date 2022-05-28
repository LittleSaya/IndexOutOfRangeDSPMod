using DSPTransportStat.CacheObjects;
using DSPTransportStat.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DSPTransportStat.Translation
{
    class UIStationCountInListTranslation : MonoBehaviour
    {
        public RectTransform TextBeforeNumberPosition { get; set; }

        public Text TextBeforeNumber { get; set; }

        public RectTransform TextAfterNumberPosition { get; set; }

        public Text TextAfterNumber { get; set; }

        public RectTransform NumberPosition { get; set; }

        public Text Number { get; set; }

        private bool plural;

        static public GameObject Create ()
        {
            GameObject instance = new GameObject("station-count-in-list", typeof(RectTransform), typeof(UIStationCountInListTranslation));

            GameObject goTextBeforeNumber = new GameObject("text-before", typeof(RectTransform), typeof(Text));
            goTextBeforeNumber.transform.SetParent(instance.transform);

            Text textBeforeNumber = goTextBeforeNumber.GetComponent<Text>();

            GameObject goTextAfterNumber = new GameObject("text-after", typeof(RectTransform), typeof(Text));
            goTextAfterNumber.transform.SetParent(instance.transform);

            Text textAfterNumber = goTextAfterNumber.GetComponent<Text>();

            GameObject goNumber = new GameObject("number", typeof(RectTransform), typeof(Text));
            goNumber.transform.SetParent(instance.transform);

            Text number = goNumber.GetComponent<Text>();

            instance.GetComponent<UIStationCountInListTranslation>().Init(
                goTextBeforeNumber.GetComponent<RectTransform>(),
                textBeforeNumber,
                goTextAfterNumber.GetComponent<RectTransform>(),
                textAfterNumber,
                goNumber.GetComponent<RectTransform>(),
                number
            );

            return instance;
        }

        public void SetNumber (int number)
        {
            Number.text = $"{number}";
            plural = number > 1;

            Reposition(Strings.Language);
        }

        private void Init (RectTransform textBeforeNumberPosition, Text textBeforeNumber, RectTransform textAfterNumberPosition, Text textAfterNumber, RectTransform numberPosition, Text number)
        {
            TextBeforeNumberPosition = textBeforeNumberPosition;
            TextBeforeNumber = textBeforeNumber;
            TextAfterNumberPosition = textAfterNumberPosition;
            TextAfterNumber = textAfterNumber;
            NumberPosition = numberPosition;
            Number = number;

            TextBeforeNumber.horizontalOverflow = HorizontalWrapMode.Overflow;
            TextBeforeNumber.verticalOverflow = VerticalWrapMode.Overflow;
            TextBeforeNumber.alignment = TextAnchor.MiddleRight;
            TextBeforeNumber.font = ResourceCache.FontSAIRASB;
            TextBeforeNumber.text = "";

            TextAfterNumber.horizontalOverflow = HorizontalWrapMode.Overflow;
            TextAfterNumber.verticalOverflow = VerticalWrapMode.Overflow;
            TextAfterNumber.alignment = TextAnchor.MiddleLeft;
            TextAfterNumber.font = ResourceCache.FontSAIRASB;
            TextAfterNumber.text = "";

            Number.horizontalOverflow = HorizontalWrapMode.Overflow;
            Number.verticalOverflow = VerticalWrapMode.Overflow;
            Number.alignment = TextAnchor.MiddleCenter;
            Number.font = ResourceCache.FontSAIRASB;
            Number.text = "";

            plural = false;

            SetLanguage(Strings.Language);
        }

        private void SetLanguage (Language lang)
        {
            switch (lang)
            {
                case Language.zhCN:
                    TextBeforeNumber.text = "当前列表中有 ";
                    TextAfterNumber.text = " 个站点";
                    break;
                case Language.enUS:
                default:
                    TextBeforeNumber.text = "";
                    if (plural)
                    {
                        TextAfterNumber.text = " stations in list";
                    }
                    else
                    {
                        TextAfterNumber.text = " station in list";
                    }
                    break;
            }
        }

        private void Reposition (Language lang)
        {
            switch (lang)
            {
                case Language.zhCN:
                    TextBeforeNumberPosition.Zeroize();
                    TextBeforeNumberPosition.anchorMax = new Vector2(0, 1);
                    TextBeforeNumberPosition.offsetMax = new Vector2(100, 0);

                    NumberPosition.Zeroize();
                    NumberPosition.anchorMax = new Vector2(0, 1);
                    NumberPosition.offsetMin = new Vector2(100, 0);
                    NumberPosition.offsetMax = new Vector2(100 + Number.preferredWidth, 0);

                    TextAfterNumberPosition.Zeroize();
                    TextAfterNumberPosition.anchorMax = new Vector2(0, 1);
                    TextAfterNumberPosition.offsetMin = new Vector2(100 + Number.preferredWidth, 0);
                    TextAfterNumberPosition.offsetMax = new Vector2(100 + Number.preferredWidth + 50, 0);
                    break;
                case Language.enUS:
                default:
                    TextBeforeNumberPosition.Zeroize();

                    NumberPosition.Zeroize();
                    NumberPosition.anchorMax = new Vector2(0, 1);
                    NumberPosition.offsetMax = new Vector2(Number.preferredWidth, 0);

                    TextAfterNumberPosition.Zeroize();
                    TextAfterNumberPosition.anchorMax = new Vector2(0, 1);
                    TextAfterNumberPosition.offsetMin = new Vector2(Number.preferredWidth, 0);
                    TextAfterNumberPosition.offsetMax = new Vector2(Number.preferredWidth + 50, 0);
                    if (plural)
                    {
                        TextAfterNumber.text = " stations in list";
                    }
                    else
                    {
                        TextAfterNumber.text = " station in list";
                    }
                    break;
            }
        }
    }
}
