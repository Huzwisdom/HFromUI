using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HAttribute
{
    using HFromUI.HLangage;
    public class HDescriptionLanguageAttribute : DescriptionAttribute
    {
        /// <summary>Original 成员。</summary>
        public string Original = string.Empty;

        /// <summary>OriginalEmpty 成员。</summary>
        public string OriginalEmpty = string.Empty;
        public HDescriptionLanguageAttribute(string original)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            Original = original;

            HTranslation.TranslationLanguageChanged += HTranslation_TranslationLanguageChanged;
        }

        private void HTranslation_TranslationLanguageChanged(object sender, EventArgs e)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            OriginalEmpty = string.Empty;
        }
        public override string Description
        {
            get
            {
                if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                {
                    return OriginalEmpty;
                }
                if (string.IsNullOrEmpty(OriginalEmpty))
                {
                    return OriginalEmpty = HTranslation.GetContent(Original);
                }
                return OriginalEmpty;
            }
        }
    }
}
