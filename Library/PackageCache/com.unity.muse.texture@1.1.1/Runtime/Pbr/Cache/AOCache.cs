using System.IO;
using LiteDB;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture.Pbr.Cache
{
    internal class AOCache
    {
        private static string k_FileStreamPath => PbrDataCache.k_FileStreamPath;
        const string k_ArtifactCollectionName = "AoData";
        
        static FileStream s_Fs;
        static LiteDatabase s_Db;
        static LiteDatabase Db
        {
            get
            {
                try
                {
                    s_Fs = new FileStream(k_FileStreamPath, FileMode.OpenOrCreate);
                    s_Db = new LiteDatabase(s_Fs);
                    var collection = s_Db.GetCollection<AOCache>(k_ArtifactCollectionName);
                    collection.EnsureIndex("AlbedoGuid");
                    return s_Db;
                }
                catch (LiteException)
                {
                    s_Fs?.Dispose();
                    File.Delete(k_FileStreamPath);
                    return Db;
                }
            }
        }

        static ILiteCollection<AODatabaseObject> GetCollection()
        {
            var collection = Db.GetCollection<AODatabaseObject>(k_ArtifactCollectionName);
            return collection;
        }
        
        public static AODatabaseObject FindOne(string albedoGuid)
        {
            var collection = GetCollection();
            var result = collection.FindOne(item => item.AlbedoGuid == albedoGuid);
            s_Db.Dispose();
            s_Fs.Dispose();
            return result;
        }
        
        static void Upsert(AODatabaseObject artifactObject)
        {
            var collection = GetCollection();
            collection.Upsert(artifactObject);
            s_Db.Dispose();
            s_Fs.Dispose();
        }
        
        public static bool IsInCache(Artifact albedoArtifact)
        {
            return FindOne(albedoArtifact.Guid) != null;
        }

        public static void Write(Artifact albedoArtifact, byte[] aoMapPNGData)
        {
            var artifactObject = FindOne(albedoArtifact.Guid) ?? new AODatabaseObject(albedoArtifact.Guid, aoMapPNGData);
            Upsert(artifactObject);
        }
    }
}