namespace Speed.Application
{
    public enum PutCardResultType { Success, InvalidRule, PileBlocked }

    public class PutCardResult
    {
        public PutCardResultType Type { get; }
        public bool IsSuccess => Type == PutCardResultType.Success;

        private PutCardResult(PutCardResultType type) { Type = type; }

        public static PutCardResult Success()     => new PutCardResult(PutCardResultType.Success);
        public static PutCardResult InvalidRule() => new PutCardResult(PutCardResultType.InvalidRule);
        public static PutCardResult PileBlocked() => new PutCardResult(PutCardResultType.PileBlocked);
    }
}
