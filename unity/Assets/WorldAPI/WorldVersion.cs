using System;
using MongoDB.Bson;

namespace WorldAsSupport {
    namespace WorldAPI {
        public class WorldVersion {
            private const string DATE_PATTERN = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

            public WorldVersionData Data;
            public DateTime LastModified {
                get {
                    return DateTime.ParseExact(Data.lastModified, DATE_PATTERN, null);
                }
                set {
                    Data.lastModified = value.ToString(DATE_PATTERN);
                }
            }

            public WorldVersion() {
                // init Data for new version
                Data = new WorldVersionData();
                Data._id = ObjectId.GenerateNewId().ToString();
                LastModified = DateTime.Now;
            }

            public WorldVersion(WorldVersionData data) {
                Data = data;
            }
        }
    }
}
