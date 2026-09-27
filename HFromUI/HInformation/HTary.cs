using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HFromUI.HInterface;

namespace HFromUI.HInformation
{
    using HFromUI.HLangage;
    public class HTary
    {
        public HMaterial this[int index]
        {
            get
            {
                return Materials[index];
            }
            set
            {
                Materials[index] = value;
            }
        }
        public HMaterial this[int row, int column]
        {
            get
            {
                HMaterial userControlMaterial = null;

                foreach (var item in Materials.Keys)
                {
                    if (Materials[item].Row == row && Materials[item].Column == column)
                    {
                        userControlMaterial = Materials[item];
                        break;
                    }
                }
                return userControlMaterial;
            }
            set
            {
                bool isok = false;
                foreach (var item in Materials.Keys)
                {
                    if (Materials[item].Row == row && Materials[item].Column == column)
                    {
                        Materials[item] = value;
                        isok = true;
                        break;
                    }
                }
                if (!isok)
                {
                    throw new Exception(HTranslation.GetContent("设置失败：未指定行或列"));
                }
            }
        }
        public int Count
        {
            get
            {
                return Materials.Count;
            }
        }
        /// <summary>添加。</summary>
        public bool Add(int row, int column, int index)
        {
            bool isok = true;

            foreach (var item in Materials.Keys)
            {
                if (Materials[item].Row == row && Materials[item].Column == column)
                {
                    isok = false;
                }
            }
            if (isok)
            {
                HMaterial material = new HMaterial();
                material.Row = row;
                material.Column = column;
                material.Index = index;
                isok = Materials.TryAdd(index, material);
            }
            return isok;
        }
        /// <summary>移除。</summary>
        public bool Remove(int index)
        {
            return Materials.TryRemove(index, out HMaterial material);
        }
        /// <summary>名称。</summary>
        public string Name { get; set; }
        /// <summary>SelectIndex 成员。</summary>
        public int SelectIndex { set; get; }
        /// <summary>ID 成员。</summary>
        public int ID { set; get; }
        /// <summary>SN 成员。</summary>
        public string SN { set; get; }
        /// <summary>Work 成员。</summary>
        public iHFrom Work { set; get; }

        /// <summary>rowList 成员。</summary>
        public List<int> rowList = new List<int>();
        /// <summary>Calculate 方法。</summary>
        public void Calculate()
        {
            rowList.Clear();
            List<HMaterial> materialslist = GetMaterials;
            int max = materialslist.Max(t => t.Row);
            for (int i = 0; i <= max; i++)
            {
                int c = 0;
                foreach (var item in materialslist)
                {
                    if (item.Row == i)
                    {
                        c++;
                    }

                }
                rowList.Add(c - 1);
            }
        }
        /// <summary>清空。</summary>
        public void Clear()
        {
            Materials.Clear();
        }
        public ConcurrentDictionary<int, HMaterial> Materials { set; get; } = new ConcurrentDictionary<int, HMaterial>();
        private List<HMaterial> materials;
        public List<HMaterial> GetMaterials
        {
            get
            {
                if (materials == null)
                {
                    materials = new List<HMaterial>();
                }
                materials.Clear();
                foreach (var item in Materials.Keys)
                {
                    materials.Add(Materials[item]);
                }
                return materials;
            }

        }
    }
}
