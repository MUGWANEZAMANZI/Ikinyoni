using System;

namespace Unity.Muse.Common
{
    [Serializable]
    sealed class GenerateBatchPbrMapRequest : GuidItemRequest
    {
        public string[] map_types;

        public GenerateBatchPbrMapRequest(string guid, string[] map_types) : base(guid)
        {
            this.map_types = map_types;
        }
    }
}
