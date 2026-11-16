using System;
using TMPro;
using UnityEngine;

namespace OpenFrame
{
    [Serializable]
    public sealed class DefenseOfferView
    {
        public UnityEngine.UI.Image background;
        public UnityEngine.UI.Image[] cells;
        public UnityEngine.UI.Image[] weapons;
        public TMP_Text label;
        public DefensePointer input;
    }

    [Serializable]
    public sealed class DefenseEnemyView
    {
        public RectTransform root;
        public UnityEngine.UI.Image body;
        public UnityEngine.UI.Image health;
        public TMP_Text label;
    }

    public sealed class DefenseView : MonoBehaviour
    {
        public DefenseWorldView world;
        public DefenseEvolutionGraphic evolutionGraphic;
        public TMP_Text[] upgradeLabels;
        public TMP_Text drawLabel;
        public TMP_Text rollCostLabel;
        public TMP_Text supportChargeLabel;
        public UnityEngine.UI.Image[] supportPips;
        public TMP_Text[] evolutionLevels;
        public UnityEngine.UI.Image[] evolutionIcons;
        public UnityEngine.UI.Image[] upgradeIcons;
        public RectTransform board;
        public RectTransform arena;
        public UnityEngine.UI.Image[] boardCells;
        public UnityEngine.UI.Image[] boardUnits;
        public Sprite[] unitSprites;
        public Sprite[] enemySprites;
        public Sprite[] projectileSprites;
        public UnityEngine.UI.Image[] projectiles;
        public UnityEngine.UI.Image[] ghosts;
        public DefenseOfferView[] offers;
        public DefenseEnemyView[] enemies;
        public TMP_Text status;
        public TMP_Text monsterCount;
        public TMP_Text stats;
        public TMP_Text notice;
        public TMP_Text supply;
        public TMP_Text progress;
        public TMP_Text record;
        public UnityEngine.UI.Image waveBar;
        public UnityEngine.UI.Image supplyBar;
        public UnityEngine.UI.Image pulse;
        public RectTransform beam;
        public TMP_Text hitText;
        public TMP_Text clearText;
        public UnityEngine.UI.Button rotateButton;
        public UnityEngine.UI.Button redrawButton;
        public UnityEngine.UI.Button rescueButton;
        public UnityEngine.UI.Button pauseButton;
        public TMP_Text pauseLabel;
        public GameObject overlay;
        public RectTransform modal;
        public Sprite[] modalSprites;
        public TMP_Text overlayTitle;
        public TMP_Text overlayBody;
        public UnityEngine.UI.Button primaryButton;
        public TMP_Text primaryLabel;
        public UnityEngine.UI.Button[] upgrades;
        public DefensePointer boardInput;
        public AudioSource feedbackSource;
        public AudioClip placementSound;
        public AudioClip attackSound;
        public AudioClip clearSound;

        private float soundCooldown;

        private float clearTime;
        private static readonly Color[] Palette =
        {
            new Color(0.69f, 0.80f, 0.46f),
            new Color(1f, 0.42f, 0.34f),
            new Color(0.3f, 0.7f, 1f),
            new Color(0.32f, 0.88f, 0.65f)
        };
        public static Color ElementColor(int element) => Palette[element];

        public void Refresh(DefenseModel model, int selected)
        {
            for (int i = 0; i < offers.Length; i++)
            {
                offers[i].background.gameObject.SetActive(i == 0);
                if (i != 0) continue;
                var piece = model.Offers[0];
                var cells = piece.Cells();
                int width = 1, height = 1;
                foreach (var p in cells) { width = Math.Max(width, p.x + 1); height = Math.Max(height, p.y + 1); }
                float pitch=Mathf.Min(40,160f/width,160f/height);
                for (int j = 0; j < 4; j++)
                {
                    offers[i].cells[j].gameObject.SetActive(model.HasPiece);
                    offers[i].cells[j].rectTransform.anchoredPosition = new Vector2((cells[j].x - (width - 1) / 2f) * pitch, (cells[j].y - (height - 1) / 2f) * pitch + 10);
                    offers[i].cells[j].rectTransform.sizeDelta=Vector2.one*(pitch-3);
                    offers[i].cells[j].color = Color.white;
                    int level=model.Evolution[piece.Element-1];
                    offers[i].cells[j].sprite = unitSprites[piece.Element-1];
                    offers[i].weapons[j].gameObject.SetActive(model.HasPiece&&level>0);
                    offers[i].weapons[j].rectTransform.sizeDelta=Vector2.one*(pitch-5)*.85f;
                    if(level>0)offers[i].weapons[j].sprite=world.weaponSprites[(piece.Element-1)*3+level-1];
                }
                offers[i].background.color = Color.clear;
                offers[i].label.rectTransform.anchoredPosition=new Vector2(0,model.HasPiece ? -96 : 0);
                offers[i].label.fontSize=model.HasPiece ? 32:36;
                offers[i].label.text = model.HasPiece ? (piece.Element == 1 ? "화력" : piece.Element == 2 ? "속사" : "냉기") : "";
            }
            for(int i=0;i<upgrades.Length;i++)
            {
                upgrades[i].interactable=model.Evolution[i]<3;
                var description=model.UpgradeDescription(i);
                int lineBreak=description.IndexOf('\n');
                upgradeLabels[i].text=lineBreak<0 ? description : description.Substring(0,lineBreak)+"\n<size=32><color=#746B65>"+description.Substring(lineBreak+1)+"</color></size>";
                upgradeIcons[i].sprite=world.weaponSprites[i*3+Math.Min(model.Evolution[i],2)];
                evolutionLevels[i].text=$"Lv.{model.Evolution[i]+1}";
                evolutionIcons[i].sprite=world.weaponSprites[i*3+Math.Max(0,model.Evolution[i]-1)];
            }
            notice.gameObject.SetActive(false);
            bool playing = model.IsPlaying;
            rotateButton.interactable = playing && model.HasPiece;
            redrawButton.interactable = playing && model.Coins >= model.RollCost;
            rescueButton.interactable = playing;
            pauseButton.interactable = model.Phase == DefensePhase.Combat;
            pauseLabel.text = model.Paused ? "계속" : "II";
            var phase = model.Phase;
            bool show = phase != DefensePhase.Combat || model.Paused;
            overlay.SetActive(show);
            if(show)
            {
                bool compact=model.Paused||phase==DefensePhase.Ready;
                modal.TryGetComponent<UnityEngine.UI.Image>(out var modalImage);
                modalImage.sprite=modalSprites[compact ? 1:0];
                float height=760*modalImage.sprite.rect.height/modalImage.sprite.rect.width;
                modal.sizeDelta=new Vector2(760,height);
                overlayTitle.rectTransform.anchoredPosition=new Vector2(0,height*.5f-height*(compact ? .125f:.06f));
                primaryButton.image.rectTransform.anchoredPosition=new Vector2(0,-height*.5f+100);
                overlayBody.rectTransform.sizeDelta=new Vector2(650,height-220);
                overlayBody.rectTransform.anchoredPosition=new Vector2(0,15);
            }
            foreach (var button in upgrades) button.gameObject.SetActive(phase == DefensePhase.Upgrade);
            primaryButton.gameObject.SetActive(phase != DefensePhase.Upgrade);
            if (model.Paused)
            {
                overlayTitle.text = "일시정지";
                overlayBody.text = "";
                primaryLabel.text = "계속하기";
            }
            else if (phase == DefensePhase.Ready)
            {
                overlayTitle.text = "작은 블록, 거대한 방어";
                overlayBody.text = "";
                primaryLabel.text = "방어 시작";
            }
            else if (phase == DefensePhase.Upgrade)
            {
                overlayTitle.text = "무기 진화";
                overlayBody.text = "";
            }
            else if (phase == DefensePhase.Won || phase == DefensePhase.Lost)
            {
                overlayTitle.text = phase == DefensePhase.Won ? "방어 성공" : "게임 종료";
                overlayBody.text = $"<size=72><color=#D94230>{model.Kills}</color></size>\n처치한 몬스터\n\n<size=32>WAVE {model.Wave}   ·   {model.Elapsed:0}초\n완성 {model.Lines}줄   /   최고 {model.BestClear}줄</size>";
                primaryLabel.text = "다시 시작";
            }
            RefreshLive(model);
        }

        public void RefreshLive(DefenseModel model)
        {
            stats.text = $"블록 공격력 {model.Damage:0.0}   ·   배치 {model.ActiveCells}칸   ·   완성 {model.Lines}줄";
            status.text = $"WAVE {model.Wave:00}";
            monsterCount.text = $"{model.Enemies.Count} / {model.Rules.maximumEnemies}";
            supply.text = model.Coins.ToString();
            supplyBar.gameObject.SetActive(false);
            drawLabel.text = model.HasPiece ? "리롤" : "블록 생성";
            rollCostLabel.text=model.EmergencyDraw ? "무료" : model.RollCost.ToString();
            supportChargeLabel.text="지원";
            for(int i=0;i<supportPips.Length;i++)supportPips[i].color=i<model.SupportCharge ? new Color(1,.38f,.25f) : new Color(.45f,.4f,.36f,.3f);
            redrawButton.interactable = model.IsPlaying && model.Coins >= model.RollCost;
            progress.text = model.Overdrive > 0 ? $"해방 {model.Overdrive:0.0}s" : model.Wave == model.Rules.finalWave && model.Spawned == model.WaveSize ? $"정리 {model.FinalCountdown:0}s" : $"{Mathf.Max(0, model.WaveDuration - model.WaveElapsed):0}s";
            float danger=(float)model.Enemies.Count/model.Rules.maximumEnemies;
            waveBar.fillAmount=danger;waveBar.color=danger>=.8f ? new Color(1,.3f,.2f) : Color.white;
            monsterCount.color=danger>=.8f ? new Color(1,.42f,.28f) : danger>=.6f ? new Color(1,.76f,.4f) : Color.white;
            record.text = model.Salvage.ToString();
        }

        public void RenderEnemies(DefenseModel model, float dt)
        {
            world.Render(model,dt);
            soundCooldown = Mathf.Max(0,soundCooldown-dt);
            clearTime = Mathf.Max(0, clearTime - dt);
            clearText.gameObject.SetActive(clearTime > 0);
            clearText.color = new Color(.85f,.24f,.17f,Mathf.Min(1,clearTime*2));
        }

        public void ShowShot(DefenseShot shot)
        {
            if (soundCooldown <= 0)
            {
                feedbackSource.PlayOneShot(attackSound, 0.12f);
                soundCooldown = 0.08f;
            }
            world.ShowShot(shot);
        }
        public void ResetEffects()
        {
            world.Clear();
            clearTime=0;
        }
        public void ShowSupport(int reward)
        {
            feedbackSource.PlayOneShot(clearSound,.8f);
            clearTime=2;
            clearText.text=reward==0 ? "+ 포격 지원" : reward==1 ? "+ 저격 지원" : "+ 냉기 지원";
        }
        public void ShowClear(int lines)
        {
            feedbackSource.PlayOneShot(clearSound, 0.6f);

            clearTime = 1.6f;
            clearText.text = lines >= 2 ? $"{lines} LINE  ·  OVERDRIVE!" : "LINE CLEAR  ·  무기 해방!";
        }

        public bool GetCell(Vector2 screen,out Vector2Int cell) => world.GetCell(screen,out cell);
        public void ShowGhost(DefenseModel model,int selected,Vector2Int cell,bool visible) => world.ShowGhost(model,selected,cell,visible);
    }
}




