using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameForestFE
{
    public enum GFXResponseType
    {
        Normal          = 0,
        InvalidInput    = 1,

        NotFound        = 10,
        DuplicateEntry  = 11,

        FatalError      = 30,
        RuntimeError    = 31,

        NotSupported    = 50,
    }

    public struct GFXRestResponse
    {
        public GFXResponseType  ResponseType    { get; set; }
        public object           AdditionalData  { get; set; }
    }

}