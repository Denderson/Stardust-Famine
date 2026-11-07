using lsfUtils.RegionParams;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Music.MusicPiece;
using static Music.ProceduralMusic.ProceduralMusicInstruction;

namespace lsfUtils.CWTs;

public static class ProcedualMusicTrackCWT
{

    public static readonly ConditionalWeakTable<Track, DataClass> procedualMusicTrackCWT = new();
    public static bool TryGetData(Track key, out DataClass data)
    {
        if (key != null)
        {
            data = procedualMusicTrackCWT.GetOrCreateValue(key);
        }
        else data = null;

        return data != null;
    }
    public class DataClass
    {
        public List<ParamKey> paramKeys = [];
    }
}
