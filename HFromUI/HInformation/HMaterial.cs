using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HFromUI.HInterface;

namespace HFromUI.HInformation
{
    using HFromUI.HData;
    
    public class HMaterial
    {
        /// <summary>Work 成员。</summary>
        public iHFrom Work { set; get; }
        /// <summary>名称。</summary>
        public string Name { set; get; }
        /// <summary>ID 成员。</summary>
        public string ID { set; get; }

        /// <summary>Row 成员。</summary>
        public int Row { get; set; }

        /// <summary>Column 成员。</summary>
        public int Column { get; set; }

        /// <summary>索引。</summary>
        public int Index { set; get; }

        /// <summary>Status 成员。</summary>
        public int Status { set; get; } = 2;

        /// <summary>IsSelect 成员。</summary>
        public bool IsSelect { set; get; } = true;

        /// <summary>IsEnabelSelect 成员。</summary>
        public bool IsEnabelSelect { set; get; }
        /// <summary>IsEnable 成员。</summary>
        public bool IsEnable { set; get; }
        /// <summary>ShowContent 成员。</summary>
        public string ShowContent { set; get; }
        private string textContent;


        public string TextContent
        {
            get
            {
                if (HAppData.DefaultMode == 1)
                {
                    if (string.IsNullOrWhiteSpace(textContent))
                    {
                        return Index.ToString();
                    }
                }
                else if (HAppData.DefaultMode == 2)
                {
                    if (string.IsNullOrWhiteSpace(textContent))
                    {
                        return (Index+1).ToString();
                    }
                }

                return textContent;
            }
            set
            {
                textContent = value;
            }
        }
    }
}

