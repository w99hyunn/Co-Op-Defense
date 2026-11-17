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
        public TMP_Text[] upgradeActionLabels = new TMP_Text[3];
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
        public RectTransform offerContent;
        public GameObject heldShadow;
        public TMP_Text rescueCostLabel;
        public UnityEngine.UI.Image rescueCostIcon;
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
        public RectTransform popupContent;
        public CanvasGroup popupGroup;
        public CanvasGroup[] upgradeGroups;
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
        private float offerTime = 1;
        private float modalTime = 1;
        private float coinTime = 1;
        private int lastCoins = -1;
        private int lastSupport;
        private float supportTime = 1;
        private bool popupVisible;
        private int summonedSupport = -1;
        private float clearDuration;
        private float rejectionTime;

        public void AnimateOffer() => offerTime = 0;
        public void RejectPlacement() => rejectionTime = .22f;

        private void LateUpdate()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
            offerTime = Mathf.Min(1, offerTime + dt / .2f);
            float offerScale = Mathf.Lerp(.9f, 1, 1 - Mathf.Pow(1 - offerTime, 3));
            offerContent.localScale = Vector3.one * offerScale;
            offerContent.anchoredPosition = Vector2.down * (12 * (1 - offerTime));
            rejectionTime = Mathf.Max(0,rejectionTime-dt);
            offerContent.anchoredPosition += Vector2.right * (Mathf.Sin((.22f-rejectionTime)*65)*10*rejectionTime/.22f);
            modalTime = Mathf.MoveTowards(modalTime, popupVisible ? 1 : 0, dt / (popupVisible ? .22f : .12f));
            float ease = 1 - Mathf.Pow(1 - modalTime, 3);
            popupContent.localScale = Vector3.one * Mathf.Lerp(.985f, 1, ease);
            popupContent.anchoredPosition = Vector2.down * (12 * (1 - ease));
            popupGroup.alpha = ease;
            for (int i = 0; i < upgradeGroups.Length; i++)
                upgradeGroups[i].alpha = Mathf.SmoothStep(0, 1, Mathf.Clamp01(modalTime * 1.65f - i * .18f));
            if (!popupVisible && modalTime == 0) overlay.SetActive(false);
            coinTime = Mathf.Min(1, coinTime + dt / .22f);
            supply.transform.localScale = Vector3.one * (1 + .075f * Mathf.Sin(coinTime * Mathf.PI));
            supportTime = Mathf.Min(1, supportTime + dt / .3f);
            for (int i = 0; i < supportPips.Length; i++)
                supportPips[i].transform.localScale = Vector3.one * (i == lastSupport - 1 ? 1 + .2f * Mathf.Sin(supportTime * Mathf.PI) : 1);
        }

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
            heldShadow.SetActive(model.HasPiece);
            for (int i = 0; i < offers.Length; i++)
            {
                offers[i].background.gameObject.SetActive(i == 0);
                if (i != 0) continue;
                var piece = model.Offers[0];
                var cells = piece.Cells();
                int width = 1, height = 1;
                foreach (var p in cells) { width = Math.Max(width, p.x + 1); height = Math.Max(height, p.y + 1); }
                float pitch=Mathf.Min(58,216f/width,216f/height);
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
                offers[i].label.rectTransform.anchoredPosition=new Vector2(0,model.HasPiece ? -120 : 0);
                offers[i].label.fontSize=model.HasPiece ? 32:36;
                offers[i].label.text = model.HasPiece ? (piece.Element == 1 ? "화력" : piece.Element == 2 ? "속사" : "냉기") : "";
            }
            for(int i=0;i<upgrades.Length;i++)
            {
                upgrades[i].interactable=model.Evolution[i]<3;
                var description=model.UpgradeDescription(i);
                int lineBreak=description.IndexOf('\n');
                upgradeLabels[i].text=lineBreak<0 ? description : description.Substring(0,lineBreak)+"\n<size=28><color=#746B65>"+description.Substring(lineBreak+1)+"</color></size>";
                upgradeActionLabels[i].text = model.Evolution[i] < 3 ? "선택" : "완료";
                upgradeIcons[i].sprite=world.weaponSprites[i*3+Math.Min(model.Evolution[i],2)];
                evolutionLevels[i].text=$"Lv.{model.Evolution[i]+1}";
                evolutionIcons[i].sprite=world.weaponSprites[i*3+Math.Max(0,model.Evolution[i]-1)];
            }
            notice.gameObject.SetActive(false);
            bool playing = model.IsPlaying;
            rotateButton.interactable = playing && model.HasPiece;
            redrawButton.interactable = playing && model.Coins >= model.RollCost;
            rescueButton.interactable = model.CanRescue;
            pauseButton.interactable = model.Phase == DefensePhase.Combat;
            pauseLabel.text = model.Paused ? "계속" : "II";
            var phase = model.Phase;
            bool show = phase != DefensePhase.Combat || model.Paused;
            popupVisible = show;
            popupGroup.interactable = show;
            popupGroup.blocksRaycasts = show;
            if (show) overlay.SetActive(true);
            if(show)
            {
                bool compact=model.Paused||phase==DefensePhase.Ready;
                modal.TryGetComponent<UnityEngine.UI.Image>(out var modalImage);
                modalImage.sprite=modalSprites[compact ? 1:0];
                float height=760*modalImage.sprite.rect.height/modalImage.sprite.rect.width;
                modal.sizeDelta=new Vector2(760,height);
                overlayTitle.rectTransform.anchoredPosition=new Vector2(0,height*.5f-82);
                primaryButton.image.rectTransform.anchoredPosition=new Vector2(0,-height*.5f+90);
                overlayBody.rectTransform.sizeDelta=new Vector2(650,height-220);
                overlayBody.rectTransform.anchoredPosition=new Vector2(0,15);
            }
            if (show)
            {
                foreach (var button in upgrades) button.gameObject.SetActive(phase == DefensePhase.Upgrade);
                primaryButton.gameObject.SetActive(phase != DefensePhase.Upgrade);
            }
            if (model.Paused)
            {
                overlayTitle.text = "일시정지";
                overlayBody.text = "";
                primaryLabel.text = "계속하기";
            }
            else if (phase == DefensePhase.Ready)
            {
                overlayTitle.text = "정원 방어";
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
                overlayBody.text = $"<size=72><color=#61794B>{model.Kills}</color></size>\n처치한 몬스터\n\n<size=32>WAVE {model.Wave}   ·   {model.Elapsed:0}초\n완성 {model.Lines}줄   /   최고 {model.BestClear}줄</size>";
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
            if (lastCoins >= 0 && model.Coins > lastCoins) coinTime = 0;
            lastCoins = model.Coins;
            if (model.SupportCharge > lastSupport) supportTime = 0;
            lastSupport = model.SupportCharge;
            supplyBar.gameObject.SetActive(false);
            drawLabel.text = model.HasPiece ? "리롤" : "블록 생성";
            rollCostLabel.text=model.EmergencyDraw ? "무료" : model.RollCost.ToString();
            supportChargeLabel.text="지원";
            for(int i=0;i<supportPips.Length;i++)supportPips[i].color=i<model.SupportCharge ? Color.white : new Color(.49f,.57f,.37f,.5f);
            redrawButton.interactable = model.IsPlaying && model.Coins >= model.RollCost;
            rescueButton.interactable = model.CanRescue;
            bool freeRescue = model.CanRescue && model.Salvage < 4;
            rescueCostLabel.text = freeRescue ? "무료" : "4";
            rescueCostIcon.gameObject.SetActive(!freeRescue);
            progress.text = model.Overdrive > 0 ? $"해방 {model.Overdrive:0.0}s" : model.Wave == model.Rules.finalWave && model.Spawned == model.WaveSize ? $"정리 {model.FinalCountdown:0}s" : $"{Mathf.Max(0, model.WaveDuration - model.WaveElapsed):0}s";
            float danger=(float)model.Enemies.Count/model.Rules.maximumEnemies;
            waveBar.fillAmount=danger;waveBar.color=danger>=.8f ? new Color(.93f,.46f,.29f) : new Color(.88f,.74f,.32f);
            monsterCount.color=danger>=.8f ? new Color(1,.42f,.28f) : danger>=.6f ? new Color(1,.76f,.4f) : Color.white;
            record.text = model.Salvage.ToString();
        }

        public void RenderEnemies(DefenseModel model, float dt)
        {
            world.Render(model,dt);
            soundCooldown = Mathf.Max(0,soundCooldown-dt);
            clearTime = Mathf.Max(0, clearTime - dt);
            clearText.gameObject.SetActive(clearTime > 0);
            clearText.color = new Color(.27f,.34f,.19f,Mathf.Min(1,clearTime*2));
            float arrival = Mathf.Clamp01((clearDuration-clearTime)/.18f);
            clearText.rectTransform.localScale = Vector3.one * Mathf.Lerp(.94f,1,Mathf.SmoothStep(0,1,arrival));
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
            offerTime = modalTime = coinTime = supportTime = 1;
            lastCoins = -1;
            lastSupport = 0;
            summonedSupport = -1;
            rejectionTime = 0;
            popupVisible = false;
            modalTime = 0;
            popupGroup.alpha = 0;
            overlay.SetActive(false);
            world.Clear();
            clearTime=0;
            clearText.gameObject.SetActive(false);
        }
        public void ShowSupport(int reward)
        {
            feedbackSource.PlayOneShot(clearSound,.8f);
            summonedSupport=reward;
        }
        public void ShowClear(int lines)
        {
            if (summonedSupport < 0) feedbackSource.PlayOneShot(clearSound, 0.6f);
            clearTime = clearDuration = 1.8f;
            string reward = summonedSupport == 0 ? "포격 지원" : summonedSupport == 1 ? "저격 지원" : "냉기 지원";
            clearText.text = summonedSupport >= 0 ? $"{lines}줄 완성 <size=28>· {reward} 소환</size>" : lines >= 2 ? $"{lines}줄 완성 <size=28>· 과충전!</size>" : "줄 완성 <size=28>· 영구 강화</size>";
            summonedSupport = -1;
        }

        public bool GetCell(Vector2 screen,out Vector2Int cell) => world.GetCell(screen,out cell);
        public void ShowGhost(DefenseModel model,int selected,Vector2Int cell,bool visible) => world.ShowGhost(model,selected,cell,visible);
    }
}




