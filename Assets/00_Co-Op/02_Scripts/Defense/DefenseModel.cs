using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenFrame
{
    public enum DefensePhase { Ready, Combat, Upgrade, Won, Lost }

    [Serializable]
    public sealed class DefenseRules
    {
        public int columns = 10;
        public int rows = 6;
        public int finalWave = 6;
        public int maximumEnemies = 80;

        public float spawnSeconds = 1.35f;
        public float enemyTravelSeconds = 26f;
        public float startingDamage = 1.8f;
        public float attackSeconds = 1.1f;
    }

    public sealed class DefensePiece
    {
        public readonly int Shape;
        public readonly int Element;
        public int Rotation;
        public DefensePiece(int shape, int element) { Shape = shape; Element = element; }

        private static readonly Vector2Int[][] Shapes =
        {
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(3,0) },
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(1,1) },
            new[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,0) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(0,0) },
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(0,1), new Vector2Int(1,1) }
        };

        public Vector2Int[] Cells()
        {
            var cells = new Vector2Int[4];
            int minX = 0, minY = 0;
            for (int i = 0; i < cells.Length; i++)
            {
                var p = Shapes[Shape][i];
                for (int r = 0; r < Rotation; r++) p = new Vector2Int(-p.y, p.x);
                cells[i] = p;
                minX = Math.Min(minX, p.x);
                minY = Math.Min(minY, p.y);
            }
            for (int i = 0; i < cells.Length; i++) cells[i] -= new Vector2Int(minX, minY);
            return cells;
        }
    }

    public sealed class DefenseEnemy
    {
        public int Id;
        public float Progress;
        public float Health;
        public float MaxHealth;
        public float Speed;
        public int Kind; // 0: normal, 1: runner, 2: armored, 3: boss
        public float HitFlash;
        public int Laps;
        public float FrostTime;
    }

    /// <summary>Pure session state. No scene, input, or UI dependencies.</summary>
    public sealed partial class DefenseModel
    {
        public readonly DefenseRules Rules;
        public readonly int[,] Board;
        public readonly DefensePiece[] Offers = new DefensePiece[1];
        public readonly List<DefenseEnemy> Enemies = new List<DefenseEnemy>();
        public DefensePhase Phase { get; private set; } = DefensePhase.Ready;
        public int Wave { get; private set; }
        public int Kills { get; private set; }
        public int Lines { get; private set; }
        public int Placements { get; private set; }
        public int BestClear { get; private set; }
        public int Salvage { get; private set; } = 6;

        public float Damage { get; private set; }
        public float Haste { get; private set; }
        public float Slow { get; private set; }
        public float Elapsed { get; private set; }
        public float Overdrive { get; private set; }
        public int Spawned { get; private set; }
        public float WaveElapsed { get; private set; }
        public float FinalCountdown { get; private set; } = 65;
        public float WaveDuration => Math.Max(28, 4 + WaveSize * Rules.spawnSeconds);
        public int ActiveCells
        {
            get { int count = 0; foreach (int element in Board) if (element != 0) count++; return count; }
        }
        public int WaveSize => 10 + Wave * 3;
        public float AttackInterval => Rules.attackSeconds / (1f + Haste) / (Overdrive > 0 ? 1.6f : 1f);
        public bool Paused { get; set; }
        public bool IsPlaying => Phase == DefensePhase.Combat && !Paused;
        public string Notice { get; private set; } = "블록을 배치하고 가로 한 줄을 완성하세요";
        public event Action Changed;
        public event Action<DefenseShot> Shot;
        public event Action<int> Cleared;
        private readonly System.Random random;
        private readonly List<int> bag = new List<int>();
        private float spawnClock;
        private readonly float[,] attackClocks;
        private int nextId;


        public DefenseModel(DefenseRules rules, int seed)
        {
            Rules = rules;
            Board = new int[rules.columns, rules.rows];
            attackClocks = new float[rules.columns, rules.rows];
            Damage = rules.startingDamage;
            random = new System.Random(seed);
            for (int i = 0; i < Offers.Length; i++) Offers[i] = Draw();
        }

        private DefensePiece Draw()
        {
            if (bag.Count == 0) for (int i = 0; i < 7; i++) bag.Add(i);
            int index = random.Next(bag.Count);
            int shape = bag[index];
            bag.RemoveAt(index);
            return new DefensePiece(shape, random.Next(1, 4));
        }

        public void StartWave()
        {
            if (Phase != DefensePhase.Ready && Phase != DefensePhase.Upgrade) return;
            Wave++;
            Spawned = 0;
            spawnClock = 2.5f;
            WaveElapsed = 0;

            Phase = DefensePhase.Combat;
            Notice = Wave % 3 == 0 ? "보스 출현! 줄 완성 충격파를 활용하세요" : $"웨이브 {Wave} · 블록을 맞추며 방어하세요";
            Changed?.Invoke();
        }

        public bool CanPlace(DefensePiece piece, int x, int y)
        {
            foreach (var p in piece.Cells())
            {
                int cx = x + p.x, cy = y + p.y;
                if (cx < 0 || cy < 0 || cx >= Rules.columns || cy >= Rules.rows || Board[cx, cy] != 0) return false;
            }
            return true;
        }

        public bool Place(int slot, int x, int y)
        {
            if (!IsPlaying || slot < 0 || slot >= Offers.Length) return false;
            if (!HasPiece) return false;
            var piece = Offers[slot];
            if (!CanPlace(piece, x, y)) return false;
            foreach (var p in piece.Cells())
            {
                Board[x + p.x, y + p.y] = piece.Element;
                attackClocks[x + p.x, y + p.y] = 0.1f + ((x + p.x + (y + p.y) * Rules.columns) % 7) * 0.045f;
            }
            HasPiece = false;
            Placements++;

            int cleared = 0;
            for (int row = 0; row < Rules.rows; row++)
            {
                bool full = true;
                for (int col = 0; col < Rules.columns; col++) if (Board[col, row] == 0) { full = false; break; }
                if (!full) continue;
                cleared++;
                RowCleared?.Invoke(row);
                for (int col = 0; col < Rules.columns; col++)
                {
                    switch (Board[col, row])
                    {
                        case 1: Damage += 0.08f; break;
                        case 2: Haste = Math.Min(1.5f, Haste + 0.012f); break;
                        case 3: Slow = Math.Min(0.4f, Slow + 0.006f); break;
                    }
                    Board[col, row] = 0;
                }
            }
            if (cleared > 0)
            {
                Lines += cleared;
                BestClear = Math.Max(BestClear, cleared);
                Damage += cleared * 0.35f;
                Salvage += cleared * 2;
                Coins += cleared * 10;
                Overdrive = cleared >= 2 ? 8 : 6;
                float pulse = Damage * (5f + cleared * 2f);
                foreach (var enemy in Enemies) { enemy.Health -= pulse; enemy.HitFlash = 0.2f; }
                RemoveDead();
                Notice = cleared >= 2 ? $"{cleared}줄 동시 완성! 8초 과충전 + 전체 충격파" : "줄 완성! 영구 강화 + 10G + 코어 충전";
                ChargeSupport(cleared);
                Cleared?.Invoke(cleared);
            }
            else Notice = "배치 완료 · 가로줄을 채워 영구 강화를 얻으세요";

            Changed?.Invoke();
            return true;
        }

        public void Rotate(int slot)
        {
            if (!IsPlaying || !HasPiece || slot < 0 || slot >= Offers.Length) return;
            Offers[slot].Rotation = (Offers[slot].Rotation + 1) % 4;
            Changed?.Invoke();
        }

        public bool HasLegalMove()
        {
            foreach (var piece in Offers)
            {
                int original = piece.Rotation;
                for (int r = 0; r < 4; r++)
                {
                    piece.Rotation = r;
                    for (int y = 0; y < Rules.rows; y++)
                        for (int x = 0; x < Rules.columns; x++)
                            if (CanPlace(piece, x, y)) { piece.Rotation = original; return true; }
                }
                piece.Rotation = original;
            }
            return false;
        }

        public event Action<int> RowCleared;
        public event Action<Vector2> EnemyDefeated;
        public bool CanRescue => IsPlaying && ActiveCells > 0 && (Salvage >= 4 || !HasLegalMove());

        public void Rescue()
        {
            if (!CanRescue) return;
            int best = -1, count = 0;
            for (int y = 0; y < Rules.rows; y++)
            {
                int occupied = 0;
                for (int x = 0; x < Rules.columns; x++) if (Board[x, y] != 0) occupied++;
                if (occupied > count) { count = occupied; best = y; }
            }
            if (best < 0) return;
            if (Salvage >= 4) Salvage -= 4;
            else if (HasLegalMove()) { Notice = "정리에 부품 4개 필요 · 완전히 막히면 무료 긴급 정리"; Changed?.Invoke(); return; }
            for (int x = 0; x < Rules.columns; x++) Board[x, best] = 0;
            Notice = "긴급 정리 완료 · 강화 없이 가장 찬 줄을 비웠습니다";
            Changed?.Invoke();
        }

        public void ChooseUpgrade(int choice)
        {
            if (Phase != DefensePhase.Upgrade) return;
            if (choice < 0 || choice > 2 || Evolution[choice] >= 3) return;
            Evolution[choice]++;
            StartWave();
        }

        public void Tick(float dt)
        {
            if (!IsPlaying) return;
            Elapsed += dt;
            WaveElapsed += dt;
            TickSupport(dt);
            Overdrive = Math.Max(0, Overdrive - dt);
            spawnClock -= dt;
            if (Spawned < WaveSize && spawnClock <= 0)
            {
                spawnClock += Rules.spawnSeconds;
                int kind = Wave % 3 == 0 && Spawned == WaveSize - 1 ? 3 : (Spawned % 5 == 3 ? 1 : (Wave >= 2 && Spawned % 5 == 4 ? 2 : 0));
                float hp = (20 + Wave * 4) * Mathf.Pow(1.20f, Wave - 1) * (kind == 3 ? 7 : kind == 2 ? 1.6f : kind == 1 ? 0.75f : 1);
                Enemies.Add(new DefenseEnemy { Id = nextId++, Health = hp, MaxHealth = hp, Kind = kind, Speed = (kind == 1 ? 1.35f : kind == 3 ? 0.8f : 1) / Rules.enemyTravelSeconds });
                Spawned++;
            }
            foreach (var enemy in Enemies)
            {
                enemy.Progress += dt * enemy.Speed * (1 - Slow) * (enemy.FrostTime > 0 ? (Evolution[2] >= 3 ? 0.1f : Evolution[2] >= 2 ? 0.4f : 0.7f) : 1);
                if (enemy.Progress >= 1) { enemy.Progress -= 1; enemy.Laps++; }
                enemy.FrostTime = Math.Max(0, enemy.FrostTime - dt);
                enemy.HitFlash = Math.Max(0, enemy.HitFlash - dt);
            }
            for (int y = 0; y < Rules.rows; y++) for (int x = 0; x < Rules.columns; x++)
            {
                int element = Board[x, y];
                if (element == 0) continue;
                attackClocks[x, y] -= dt;
                if (attackClocks[x, y] > 0 || Enemies.Count == 0) continue;
                attackClocks[x, y] = AttackInterval * (element == 2 ? 0.75f : 1);
                Vector2 source = DefenseArena.CellPosition(x, y);
                var target = Enemies[0];
                float nearest = float.MaxValue;
                foreach (var enemy in Enemies)
                {
                    float distance = (DefenseArena.LoopPoint(enemy.Progress) - source).sqrMagnitude;
                    if (distance < nearest) { nearest = distance; target = enemy; }
                }
                float damage = Damage * (element == 1 ? 1.15f : element == 2 ? 0.8f : 0.9f);
                target.Health -= damage;
                target.HitFlash = 0.12f;
                if (element == 3) target.FrostTime = 1.2f;
                var targetPosition = DefenseArena.LoopPoint(target.Progress);
                Shot?.Invoke(new DefenseShot(new Vector2Int(x, y), targetPosition, element, damage));
                EvolvedAttack(new Vector2Int(x,y), element, target, damage, source);
                RemoveDead();
            }
            if (Enemies.Count >= Rules.maximumEnemies)
            {
                Phase = DefensePhase.Lost;
                Notice = "몬스터가 너무 많이 쌓였습니다!";
                Changed?.Invoke();
                return;
            }
            if (Wave == Rules.finalWave && Spawned == WaveSize)
            {
                FinalCountdown -= dt;
                if (Enemies.Count == 0) { Phase = DefensePhase.Won; Changed?.Invoke(); }
                else if (FinalCountdown <= 0) { Phase = DefensePhase.Lost; Notice = "최종 정리 시간이 끝났습니다"; Changed?.Invoke(); }
            }
            else if (Spawned == WaveSize && WaveElapsed >= WaveDuration)
            {
                Phase = DefensePhase.Upgrade;
                Salvage += 2;
                Notice = "다음 웨이브 준비 · 남은 몬스터는 계속 순환합니다";
                Changed?.Invoke();
            }
        }
        private void RemoveDead()
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                if (Enemies[i].Health > 0) continue;
                Coins += Enemies[i].Kind == 3 ? 24 : Enemies[i].Kind == 2 ? 6 : 4;
                EnemyDefeated?.Invoke(DefenseArena.LoopPoint(Enemies[i].Progress));
                Enemies.RemoveAt(i);
                Kills++;
                if (Kills % 4 == 0) Salvage++;
            }
        }
    }
}






