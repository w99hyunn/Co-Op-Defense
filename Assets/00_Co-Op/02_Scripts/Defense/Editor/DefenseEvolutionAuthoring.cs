namespace OpenFrame.Editor { public static class DefenseEvolutionAuthoring { public static void Apply() {var v=UnityEngine.Object.FindAnyObjectByType<OpenFrame.DefenseView>();
if(v.world!=null)return;
var stage=v.transform.Find("Stage");
var existing=stage.Find("EvolutionEffects");
var go=existing!=null ? existing.gameObject : new UnityEngine.GameObject("EvolutionEffects",typeof(UnityEngine.RectTransform),typeof(UnityEngine.CanvasRenderer));
go.transform.SetParent(stage,false);
go.TryGetComponent<UnityEngine.RectTransform>(out var rect);
rect.anchorMin=rect.anchorMax=UnityEngine.Vector2.zero;rect.pivot=UnityEngine.Vector2.zero;rect.anchoredPosition=UnityEngine.Vector2.zero;rect.sizeDelta=new UnityEngine.Vector2(900,1800);
if(!go.TryGetComponent<OpenFrame.DefenseEvolutionGraphic>(out var graphic))graphic=go.AddComponent<OpenFrame.DefenseEvolutionGraphic>();
graphic.raycastTarget=false;v.evolutionGraphic=graphic;
go.transform.SetSiblingIndex(stage.Find("Overlay").GetSiblingIndex());
v.upgradeLabels=new TMPro.TMP_Text[3];
for(int i=0;i<3;i++){v.upgrades[i].transform.GetChild(0).TryGetComponent<TMPro.TMP_Text>(out var label);v.upgradeLabels[i]=label;label.fontSize=23;}

UnityEditor.EditorUtility.SetDirty(v);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(v.gameObject.scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(v.gameObject.scene);

 } } }


