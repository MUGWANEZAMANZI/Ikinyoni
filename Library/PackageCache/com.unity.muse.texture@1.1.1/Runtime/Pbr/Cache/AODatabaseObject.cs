namespace Unity.Muse.Texture.Pbr.Cache
{
    internal class AODatabaseObject
    {
        public string AlbedoGuid {get; set;}
        public byte[] AOMapPNGData {get; set;}
        
        public AODatabaseObject(){}

        public AODatabaseObject(string albedoGuid, byte[] aoMapPNGData)
        {
            AlbedoGuid = albedoGuid;
            AOMapPNGData = aoMapPNGData;
        }
    }
}