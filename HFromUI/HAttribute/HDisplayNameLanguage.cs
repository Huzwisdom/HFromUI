using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HAttribute
{
    using HFromUI.HLangage;
    public class HDisplayNameLanguageAttribute : DisplayNameAttribute
    {
        /// <summary>OriginalEmpty 成员。</summary>
        public string OriginalEmpty = string.Empty;
        public HDisplayNameLanguageAttribute(string original)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.DisplayNameValue = original;
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
        public override string DisplayName
        {
            get
            {
                if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                {
                    return OriginalEmpty;
                }
                if (string.IsNullOrEmpty(OriginalEmpty))
                {
                    return OriginalEmpty = HTranslation.GetContent(base.DisplayName);
                }
                return OriginalEmpty;
            }
        }
    }
}
