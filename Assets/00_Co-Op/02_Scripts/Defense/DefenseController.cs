using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace OpenFrame
{
    public sealed class DefenseController : MonoBehaviour
    {
        [SerializeField] private DefenseView view;
        [SerializeField] private DefenseRules rules = new DefenseRules();
        public DefenseModel Model { get; private set; }
        private int selected;
        private bool hovering;
        private Vector2Int hoverCell;
        private float hudClock;
        private bool recorded;

        public void Configure(DefenseView configuredView) => view = configuredView;

        private void Awake()
        {
            for (int i = 0; i < view.offers.Length; i++)
            {
                int slot = i;
                view.offers[i].input.Clicked = _ => Select(slot);
                view.offers[i].input.DragStarted = data => { Select(slot); Preview(data); };
                view.offers[i].input.Dragged = Preview;
                view.offers[i].input.DragEnded = Drop;
            }
            view.boardInput.Clicked = Drop;
            view.boardInput.Moved = Preview;
            view.boardInput.Exited = HideGhost;
            view.rotateButton.onClick.AddListener(Rotate);
            view.redrawButton.onClick.AddListener(() => Model.Redraw());
            view.rescueButton.onClick.AddListener(() => Model.Rescue());
            view.pauseButton.onClick.AddListener(TogglePause);
            view.primaryButton.onClick.AddListener(Primary);
            for (int i = 0; i < view.upgrades.Length; i++)
            {
                int choice = i;
                view.upgrades[i].onClick.AddListener(() => Model.ChooseUpgrade(choice));
            }
            NewRun();
        }

        private void NewRun()
        {
            view.ResetEffects();
            Model = new DefenseModel(rules, System.Environment.TickCount);
            Model.Changed += Refresh;
            Model.Shot += view.ShowShot;
            Model.AttackEffect += view.world.AddEffect;
            Model.Cleared += view.ShowClear;
            Model.SupportSummoned += view.ShowSupport;
            selected = 0;
            hovering = false;
            recorded = false;
            Model.StartWave();
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            Model.Tick(dt);
            view.RenderEnemies(Model, Model.Paused ? 0 : dt);
            hudClock -= dt;
            if (hudClock <= 0)
            {
                hudClock = 0.1f;
                view.RefreshLive(Model);
                view.ShowGhost(Model, selected, hoverCell, hovering && Model.IsPlaying);
            }
            var keyboard = Keyboard.current;
            if (keyboard == null) return; // Touch-only devices have no keyboard.
            if (keyboard.rKey.wasPressedThisFrame) Rotate();
            if (keyboard.escapeKey.wasPressedThisFrame) TogglePause();
            if (keyboard.digit1Key.wasPressedThisFrame) Select(0);


        }

        private void Refresh()
        {
            if (!recorded && (Model.Phase == DefensePhase.Won || Model.Phase == DefensePhase.Lost))
            {
                recorded = true;
                int completed = Model.Phase == DefensePhase.Won ? Model.Wave : Model.Wave - 1;
                PlayerPrefs.SetInt("OpenFrame.Defense.BestWave", Mathf.Max(completed, PlayerPrefs.GetInt("OpenFrame.Defense.BestWave", 0)));
                PlayerPrefs.SetInt("OpenFrame.Defense.BestLines", Mathf.Max(Model.Lines, PlayerPrefs.GetInt("OpenFrame.Defense.BestLines", 0)));
                PlayerPrefs.Save();
            }
            view.Refresh(Model, selected);
            view.ShowGhost(Model, selected, hoverCell, hovering && Model.IsPlaying);
        }

        private void Select(int slot) { if (!Model.IsPlaying || slot != 0) return; selected = 0; Refresh(); }
        private void Rotate() { Model.Rotate(selected); }
        private void Preview(PointerEventData data)
        {
            hovering = view.GetCell(data.position, out hoverCell);
            view.ShowGhost(Model, selected, hoverCell, hovering && Model.IsPlaying);
        }
        private void Drop(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            if (view.GetCell(data.position, out var cell) && Model.Place(selected, cell.x, cell.y))
                view.feedbackSource.PlayOneShot(view.placementSound, 0.45f);
            HideGhost();
        }
        private void HideGhost() { hovering = false; view.ShowGhost(Model, selected, hoverCell, false); }
        private void TogglePause()
        {
            if (Model.Phase != DefensePhase.Combat) return;
            Model.Paused = !Model.Paused;
            HideGhost();
            Refresh();
        }
        private void Primary()
        {
            if (Model.Paused) TogglePause();
            else if (Model.Phase == DefensePhase.Ready) Model.StartWave();
            else if (Model.Phase == DefensePhase.Won || Model.Phase == DefensePhase.Lost) NewRun();
        }
    }
}







