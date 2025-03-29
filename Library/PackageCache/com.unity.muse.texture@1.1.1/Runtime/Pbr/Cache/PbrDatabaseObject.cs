namespace Unity.Muse.Texture.Pbr.Cache
{
    internal class PbrDatabaseObject
    {
        public string AlbedoGuid {get; set;}
        public string NormalGuid {get; set;}
        public string MetallicGuid {get; set;}
        public string SmoothnessGuid {get; set;}
        public string HeightGuid {get; set;}
        public string DiffuseGuid {get; set;}

        public PbrDatabaseObject(){}

        public PbrDatabaseObject(ProcessedPbrMaterialData materialData)
        {
            AlbedoGuid = materialData.BaseMap?.Guid;
            NormalGuid = materialData.NormalMap?.Guid;
            MetallicGuid = materialData.MetallicMap?.Guid;
            SmoothnessGuid = materialData.SmoothnessMap?.Guid;
            HeightGuid = materialData.HeightmapMap?.Guid;
            DiffuseGuid = materialData.DiffuseMap?.Guid;
        }
    }
}
