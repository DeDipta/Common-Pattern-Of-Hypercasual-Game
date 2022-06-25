using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Balaso
{
    public class Settings : ScriptableObject
    {
        [Multiline(5)]
        public string PopupMessage;
        //public List<string> SkAdNetworkIds;
        public List<MediationInfo> SkAdNetworkIds;

        public bool nameSorting;
        public bool statusSorting;


        [SerializeField] TextAsset textAsset;
        [ContextMenu("Update Ids Ids")]
        void UpdateIdsIds()
        {
            var targetList = SkAdNetworkIds;
            var targetIds = targetList.SelectMany(x => x.skAdNetworkIds).ToList();

            var list = textAsset.text.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in list)
            {
                var tokens = item.Split(new char[] { ',' });
                var name = tokens[0];
                var skadId = tokens[1];

                if (targetIds.Contains(skadId))
                {
                    Debug.Log("Id already added : " + skadId);
                    continue;
                }
                else
                {
                    var obj = new MediationInfo();
                    obj.sdkName = name;
                    obj.skAdNetworkIds = new string[] { skadId };
                    targetList.Add(obj);
                }
            }
        }
    }

    [System.Serializable]
    public class MediationInfo
    {
        public string sdkName;
        public bool enable;
        //public string website;
        //public string privacyPolicy;
        public string[] skAdNetworkIds;
    }


    //[SerializeField]

}
