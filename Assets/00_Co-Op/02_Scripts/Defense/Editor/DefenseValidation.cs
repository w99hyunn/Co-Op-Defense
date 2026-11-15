using System;
using System.Text;
using UnityEngine;

namespace OpenFrame.Editor
{
    public static class DefenseValidation
    {
        public static string Run()
        {
            int checks = 0;
            void Check(bool condition, string name)
            {
                if (!condition) throw new InvalidOperationException("Defense validation failed: " + name);
                checks++;
            }
            var m = new DefenseModel(new DefenseRules(), 12);
            Check(!m.Place(0, 0, 0), "cannot place before start");
            m.StartWave();
            for (int shape = 0; shape < 7; shape++)
                for (int r = 0; r < 4; r++)
                {
                    var p = new DefensePiece(shape, 1) { Rotation = r };
                    var cells = p.Cells();
                    var unique = new System.Collections.Generic.HashSet<Vector2Int>(cells);
                    Check(unique.Count == 4, "four unique cells in each rotation");
                    Check(m.CanPlace(p, 0, 0) && !m.CanPlace(p, 10, 0), "rotation bounds");
                }
            m.Redraw();
            m.Offers[0] = new DefensePiece(1, 1);
            for (int y = 0; y < 2; y++) for (int x = 2; x < 10; x++) m.Board[x, y] = 1;
            Check(m.Place(0, 0, 0), "square completes two rows");
            Check(m.Lines == 2 && m.BestClear == 2 && m.Overdrive == 8, "simultaneous clear reward");
            Check(Math.Abs(m.Damage - 4.1f) < 0.001f, "colored permanent damage");
            Check(m.Board[0, 0] == 0 && m.Board[9, 1] == 0, "rows removed");
            float elapsed = m.Elapsed;
            m.Paused = true;
            m.Tick(10);
            Check(m.Elapsed == elapsed && !m.Place(0, 0, 0), "pause freezes game and placement");
            m.Paused = false;
            for (int y = 0; y < 6; y++) for (int x = 0; x < 10; x++) m.Board[x, y] = 2;
            Check(!m.HasLegalMove(), "blocked detection across rotations");
            int lines = m.Lines;
            m.Rescue();
            Check(m.Board[0, 0] == 0 && m.Lines == lines, "rescue removes row without upgrade");
            m.Redraw();
            m.Offers[0] = new DefensePiece(0, 3);
            Check(m.Place(0, 0, 0), "rescued row playable");
            var idle = new DefenseModel(new DefenseRules(), 2);
            idle.StartWave();
            for (int i = 0; i < 20000 && idle.Phase != DefensePhase.Lost; i++)
            {
                if (idle.Phase == DefensePhase.Upgrade) idle.ChooseUpgrade(idle.Evolution[0] < 3 ? 0 : 1);
                idle.Tick(0.1f);
            }
            Check(idle.Phase == DefensePhase.Lost, "ignoring puzzle eventually loses");
            var report = new StringBuilder($"PASS: {checks} rule checks. Idle loses on wave {idle.Wave}.\n");
            for (int seed = 1; seed <= 3; seed++)
            {
                var bot = new DefenseModel(new DefenseRules(), seed);
                bot.StartWave();
                float nextPlacement = 1.5f;
                for (int step = 0; step < 10000 && bot.Phase != DefensePhase.Won && bot.Phase != DefensePhase.Lost; step++)
                {
                    if (bot.Phase == DefensePhase.Upgrade) bot.ChooseUpgrade(bot.Evolution[seed - 1] < 3 ? seed - 1 : seed % 3);
                    bot.Tick(0.1f);
                    nextPlacement -= 0.1f;
                    if (nextPlacement <= 0 && bot.IsPlaying)
                    {
                        nextPlacement = 2.8f;
                        AutoPlace(bot);
                    }
                }
                report.AppendLine($"Seed {seed}: {bot.Phase}, wave {bot.Wave}, enemies {bot.Enemies.Count}, lines {bot.Lines}, placed {bot.Placements}, elapsed {bot.Elapsed:0}s");
                
            }
            return report.ToString();
        }

        public static bool AutoPlace(DefenseModel model)
        {
            if(!model.HasPiece) model.Redraw();
            if(!model.HasPiece) return false;
            float bestScore = float.MinValue;
            int slot = -1, bx = 0, by = 0, rotation = 0;
            for (int i = 0; i < model.Offers.Length; i++)
            {
                var piece = model.Offers[i];
                int original = piece.Rotation;
                for (int r = 0; r < 4; r++)
                {
                    piece.Rotation = r;
                    var cells = piece.Cells();
                    for (int y = 0; y < model.Rules.rows; y++) for (int x = 0; x < model.Rules.columns; x++)
                    {
                        if (!model.CanPlace(piece, x, y)) continue;
                        float score = -y * 0.1f;
                        foreach (var p in cells) model.Board[x + p.x, y + p.y] = piece.Element;
                        for (int row = 0; row < model.Rules.rows; row++)
                        {
                            int count = 0;
                            for (int col = 0; col < model.Rules.columns; col++) if (model.Board[col, row] != 0) count++;
                            score += count == model.Rules.columns ? 1000 : count * count;
                        }
                        foreach (var p in cells) model.Board[x + p.x, y + p.y] = 0;
                        if (score > bestScore) { bestScore = score; slot = i; bx = x; by = y; rotation = r; }
                    }
                }
                piece.Rotation = original;
            }
            if (slot < 0) { model.Rescue(); return false; }
            model.Offers[slot].Rotation = rotation;
            return model.Place(slot, bx, by);
        }
    }
}



