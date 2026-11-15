using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace OpenFrame.Editor
{
    public static class DefenseEconomyAuthoring
    {
        public static string Apply()
        {
            var v=Object.FindAnyObjectByType<DefenseView>();
            if(v.world!=null)return "Sprite world already configured";
            var stage=v.transform.Find("Stage");
            v.redrawButton.transform.GetChild(0).TryGetComponent<TMP_Text>(out v.drawLabel);
            for(int i=1;i<v.offers.Length;i++)v.offers[i].background.gameObject.SetActive(false);
            v.offers[0].background.rectTransform.anchoredPosition=new Vector2(450,207);
            stage.Find("SupplyBack").gameObject.SetActive(false);
            var center=v.arena.Find("CenterField");
            foreach(Transform child in center)child.gameObject.SetActive(false);
            center.TryGetComponent<UnityEngine.UI.Image>(out var panel);panel.color=new Color(.88f,.70f,.49f);
            for(int y=0;y<2;y++)for(int x=0;x<8;x++)
            {
                var cell=Node(center,$"CenterCell_{x}_{y}",new Vector2(-280+x*80,-38+y*76),new Vector2(73,67));
                if(!cell.TryGetComponent<UnityEngine.UI.Image>(out var tile))tile=cell.gameObject.AddComponent<UnityEngine.UI.Image>();
                tile.sprite=v.offers[0].background.sprite;tile.type=UnityEngine.UI.Image.Type.Sliced;tile.pixelsPerUnitMultiplier=3;
                tile.color=new Color(.95f,.79f,.57f);tile.raycastTarget=false;
            }
            var help=stage.Find("RollHelp"); if(help!=null)help.gameObject.SetActive(false);
            var costs=stage.Find("RollCostHelp"); if(costs!=null)costs.gameObject.SetActive(false);
            v.stats.gameObject.SetActive(false);
            v.notice.gameObject.SetActive(false);
            v.supply.fontSize=30;
            EditorUtility.SetDirty(v);EditorSceneManager.MarkSceneDirty(v.gameObject.scene);EditorSceneManager.SaveScene(v.gameObject.scene);
            return "Central deployment field restored";
        }
        private static RectTransform Node(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var found=parent.Find(name);
            var rect=found!=null ? (RectTransform)found : (RectTransform)new GameObject(name,typeof(RectTransform)).transform;
            rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=parent.name=="Stage" ? Vector2.zero : new Vector2(.5f,.5f);
            rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;rect.gameObject.SetActive(true);return rect;
        }
        private static TMP_Text Text(Transform parent,string name,string value,Vector2 position,Vector2 size,float fontSize,DefenseView view)
        {
            var rect=Node(parent,name,position,size);
            if(!rect.TryGetComponent<TextMeshProUGUI>(out var text))text=rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font=view.status.font;text.fontSize=fontSize;text.text=value;text.alignment=TextAlignmentOptions.Center;text.color=new Color(.95f,.9f,1);text.raycastTarget=false;return text;
        }
    }
}




