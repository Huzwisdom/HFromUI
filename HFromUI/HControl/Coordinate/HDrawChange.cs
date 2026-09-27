using HFromUI.HBase;
using HFromUI.HData;
using HFromUI.HEnum;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HControl.Coordinate
{
    public class HDrawChange
    {
        /// <summary>Group 方法。</summary>
        public static HDrawChange Group(int[] indexs, HDrawBase[] HDrawBases,int index)
        {
            if (indexs == null || HDrawBases == null || HDrawBases.Length != indexs.Length)
            {
                return null;
            }
            HDrawChange HDrawChangeGroup = new HDrawChange();
            HDrawChangeGroup.OperationMode = HOperationMode.Group;
            for (int i = 0; i < indexs.Length; i++)
            {
                HDrawChangeGroup.Data.TryAdd(indexs[i], HDrawBases[i]);
            }
            HDrawChangeGroup.Index = index;
            return HDrawChangeGroup;
        }
        /// <summary>UnGroup 方法。</summary>
        public static HDrawChange UnGroup(int indexsOld, HDrawBase HDrawBase)
        {
            HDrawChange HDrawChangeUnGroup = new HDrawChange();
            HDrawChangeUnGroup.OperationMode = HOperationMode.UnGroup;
            HDrawChangeUnGroup.Data.TryAdd(indexsOld, HDrawBase);
            HDrawChangeUnGroup.Index = indexsOld;
            return HDrawChangeUnGroup;
        }
        /// <summary>移除。</summary>
        public static HDrawChange Remove(int[] indexs, HDrawBase[] HDrawBases)
        {
            if (indexs==null|| HDrawBases==null|| HDrawBases.Length!= indexs.Length)
            {
                return null;
            }
            HDrawChange HDrawChangeRemove = new HDrawChange();

            HDrawChangeRemove.OperationMode = HOperationMode.Remove;
            for (int i = 0; i < indexs.Length; i++)
            {
                HDrawChangeRemove.Data.TryAdd(indexs[i], HDrawBases[i]);
            }
            return HDrawChangeRemove;
        }
        /// <summary>Change 方法。</summary>
        public static HDrawChange Change(int indexsOld,bool isLast)
        {
            HDrawChange HDrawChangeChange2 = new HDrawChange();
            HDrawChangeChange2.OperationMode = HOperationMode.Change;
            HDrawChangeChange2.OldIndex = indexsOld;
            if (isLast)
            {
                HDrawChangeChange2.NewIndex = -1;
            }
            else
            {
                HDrawChangeChange2.NewIndex = -2;
            }
            return HDrawChangeChange2;
        }
        /// <summary>Change 方法。</summary>
        public static HDrawChange Change(int indexsOld, int indexNew)
        {
            HDrawChange HDrawChangeChange = new HDrawChange();
            HDrawChangeChange.OperationMode = HOperationMode.Change;
            HDrawChangeChange.OldIndex = indexsOld;
            HDrawChangeChange.NewIndex = indexNew;
            return HDrawChangeChange;
        }
        /// <summary>更新。</summary>
        public static HDrawChange Update(int indexsOld, HDrawBase HDrawBase)
        {
            HDrawChange HDrawChangeUpdate2 = new HDrawChange();
            HDrawChangeUpdate2.OperationMode = HOperationMode.Update;
            HDrawChangeUpdate2.Data.TryAdd(indexsOld, HDrawBase);
            return HDrawChangeUpdate2;
        }
        /// <summary>更新。</summary>
        public static HDrawChange Update(int[] indexs, HDrawBase[] HDrawBases)
        {
            HDrawChange HDrawChangeUpdate = new HDrawChange();
            HDrawChangeUpdate.OperationMode = HOperationMode.Update;
            for (int i = 0; i < indexs.Length; i++)
            {
                HDrawChangeUpdate.Data.TryAdd(indexs[i], HDrawBases[i]);
            }
            return HDrawChangeUpdate;
        }
        /// <summary>添加。</summary>
        public static HDrawChange Add(int indexsOld, HDrawBase HDrawBase)
        {
            HDrawChange HDrawChangeAdd = new HDrawChange();
            HDrawChangeAdd.OperationMode = HOperationMode.Add;
            HDrawChangeAdd.Name = HDrawBase.GuidCode;
            HDrawChangeAdd.OldIndex = indexsOld;
            HDrawChangeAdd.Data.TryAdd(indexsOld, HDrawBase);
            return HDrawChangeAdd;
        }
        /// <summary>添加。</summary>
        public static HDrawChange Add(int[] indexsOld, HDrawBase[] HDrawBase)
        {
            HDrawChange HDrawChangeAdd = new HDrawChange();
            HDrawChangeAdd.OperationMode = HOperationMode.Add;
            for (int i = 0; i < indexsOld.Length; i++)
            {
                HDrawChangeAdd.Data.TryAdd(indexsOld[i], HDrawBase[i]);
            }
            return HDrawChangeAdd;
        }
        public HDrawChange()
        { 
        
        }
        public HDrawChange(HOperationMode cOperationMode)
        {
            OperationMode=cOperationMode;
        }
        public HDrawChange(HOperationMode cOperationMode, ConcurrentDictionary<int, HDrawBase> data)
        {
            OperationMode = cOperationMode; Data=data;
        }
        /// <summary>OperationMode 成员。</summary>
        public HOperationMode OperationMode { get; set; } = HOperationMode.None;

        public ConcurrentDictionary<int, HDrawBase> Data { get; set; }=new ConcurrentDictionary<int, HDrawBase>();
   
        /// <summary>名称。</summary>
        public string Name { set; get; }
        /// <summary>索引。</summary>
        public int Index { set; get; }
        /// <summary>OldIndex 成员。</summary>
        public int OldIndex { set; get; }
        /// <summary>NewIndex 成员。</summary>
        public int NewIndex { set; get; }
        private HDrawChange drawChange;
        public HDrawChange DrawChange
        { get { return drawChange; }
            set { drawChange = value; }
        }
        /// <summary>克隆。</summary>
        public HDrawChange Clone()
        {
            HDrawChange drawChangeClone = new HDrawChange();
            drawChangeClone.OperationMode = OperationMode;
            drawChangeClone.Data = new ConcurrentDictionary<int, HDrawBase>(Data);
            drawChangeClone.OldIndex = OldIndex;
            drawChangeClone.NewIndex= NewIndex;
            drawChangeClone.Index = Index;
            drawChangeClone.Name= Name;
            return drawChangeClone;
        }
    }
}
