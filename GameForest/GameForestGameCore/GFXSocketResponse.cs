namespace GameForestCoreWebSocket
{
    public enum GFXResponseType
    {
        Normal          = 0,
        DoesNotExist    = 1,
        DuplicateEntry  = 2,
        InvalidInput    = 3,
        FatalError      = 4,
    }

    public struct GFXSocketResponse
    {
        public GFXResponseType  ResponseCode    { get; set; }

        public string           Subject         { get; set; }
        public string           Message         { get; set; }
    }
}
