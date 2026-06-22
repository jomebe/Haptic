using System;

namespace Haptic.Gameplay
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public string Name { get; }
        public string Subtitle { get; }
        public string[] Rows { get; }
        public bool RequiresKey { get; }
        public TutorialPattern Tutorial { get; }

        public LevelDefinition(string name, string subtitle, bool requiresKey, TutorialPattern tutorial, params string[] rows)
        {
            Name = name;
            Subtitle = subtitle;
            RequiresKey = requiresKey;
            Tutorial = tutorial;
            Rows = rows;
        }
    }

    public enum TutorialPattern
    {
        None,
        Wall,
        Goal,
        Trap,
        Key
    }
}

