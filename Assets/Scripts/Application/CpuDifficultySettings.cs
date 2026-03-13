namespace Speed.Application
{
    public readonly struct CpuDifficultySettings
    {
        public CpuDifficultySettings(float reactionSeconds, float mistakeRate)
        {
            ReactionSeconds = reactionSeconds;
            MistakeRate = mistakeRate;
        }

        public float ReactionSeconds { get; }
        public float MistakeRate { get; }
    }
}
