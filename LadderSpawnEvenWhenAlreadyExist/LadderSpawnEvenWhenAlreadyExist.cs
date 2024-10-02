using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace LadderSpawnEvenWhenAlreadyExist
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private static List<Vector2> _previousLadders = new List<Vector2>();

        public override void Entry(IModHelper helper)
        {
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.Player.Warped += this.OnPlayerWarped;
            _previousLadders = new List<Vector2>();
        }

        private void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            if (e.NewLocation is MineShaft mine)
            {
                populateLadders(mine);
            }
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (e.IsCurrentLocation && e.Location is MineShaft mine)
            {
                // If any stones broken were in the old ladder list, make shaft
                foreach (KeyValuePair<Vector2, StardewValley.Object> r in e.Removed)
                {
                    if (r.Value.Name == "Stone" && _previousLadders.Contains(r.Key))
                    {
                        mine.createLadderDown((int)r.Key.X, (int)r.Key.Y);
                    }
                }

                populateLadders(mine);
            }
        }

        private void populateLadders(MineShaft mine)
        {
            // Calculate next ladders
            _previousLadders = new List<Vector2>();
            int farmerLuckLevel = Game1.player.LuckLevel;

            double chanceForLadderDown =
                0.02
                + 1.0 / (double)Math.Max(1, mine.stonesLeftOnThisLevel - 1)
                + (double)farmerLuckLevel / 100.0
                + Game1.player.DailyLuck / 5.0;
            if (mine.EnemyCount == 0)
            {
                chanceForLadderDown += 0.04;
            }
            if (Game1.player.hasBuff("dwarfStatue_1"))
            {
                chanceForLadderDown *= 1.25;
            }

            // Most other code copied from original game code, except this line.
            if (mine.ladderHasSpawned)
            {
                chanceForLadderDown /= 2;
            }

            foreach (KeyValuePair<Vector2, StardewValley.Object> o in mine.objects.Pairs)
            {
                Random r = Utility.CreateDaySaveRandom(o.Key.X * 1000, o.Key.Y, mine.mineLevel);
                r.NextDouble();

                if (
                    !mine.mustKillAllMonstersToAdvance()
                    && (
                        (mine.stonesLeftOnThisLevel - 1) == 0
                        || r.NextDouble() < chanceForLadderDown
                    )
                    && mine.shouldCreateLadderOnThisLevel()
                )
                {
                    _previousLadders.Add(o.Key);
                }
            }
        }
    }
}
