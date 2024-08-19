using KH.Infinite;
using Pomerandomian;
using Ratferences;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The chunk info is information about the base unit of generation.
/// It should contain deterministic information about the tile that
/// it represents, even if it's destroyed and recreated. In this
/// approach, I do so by passing it in a string seed made up of 
/// the location coordinates.
/// 
/// Of course, if you don't care about consistent generation or 
/// you have no randomness in the chunks themselves, you don't need
/// to provide a seed or access to random.
/// </summary>
public class SampleChunkInfo : ChunkInfo {
    private readonly string _seed;
    public readonly int SomethingRandom;

    private IRandom _random;

    public SampleChunkInfo(ChunkLocation location, string seed) : base(location) {
        _seed = seed;
        SomethingRandom = Random.Next(5000);
    }

    /// <summary>
    /// Deterministic random.
    /// 
    /// NOTE: This should only be used in chunk generation itself and should not be used
    /// anywhere it may be called multiple times (expecting the same result), as that will
    /// lead to inconsistent RNG.
    /// </summary>
    public IRandom Random {
        get {
            _random ??= new SystemRandom(_seed);
            return _random;
        }
    }
}

public class BiggerSampleChunkInfo : ChunkInfo {
    public readonly Color color;
    public BiggerSampleChunkInfo(ChunkLocation location, string seed) : base(location) {
        color = Color.HSVToRGB((float)new SystemRandom(seed).NextDouble(), 0.3f, 1f);
    }
}

public class SampleInfGenerator : InfFollower {
    [SerializeField] int ChunkSize = 20;
    [SerializeField] GameObject TilePrefab;
    [SerializeField] GameObject TilePrefabBig;
    [SerializeField] string BaseSeed = "Everyone loves procgen!";
    [SerializeField] bool AlwaysRandomizeSeed;
    protected override void OnAwake() {
        // This chooses between randomly getting a new seed every run or using the predefined seed.
        // A seed that you save out to disk can let you load saves just by using the same seed. Be
        // careful, though, as changing the creation algorithm can make your seed generate different
        // results than one saved from a previous generation.
        if (AlwaysRandomizeSeed) {
            BaseSeed = new SystemRandom().Next(10000000).ToString();
        }
    }

    /// <summary>
    /// Meat and potatoes for the actual generation. If you only are generating one set of tiles,
    /// you can override CreateManager() instead.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<IChunkManager> CreateManagers() {

        // Setting a chunk generator isn't required. If we just wanted to generate the info for the lower manager to use,
        // we could have skipped the chunk generator and cleanup.
        ChunkManager<BiggerSampleChunkInfo> bigMan = new ChunkManager<BiggerSampleChunkInfo>(new Vector2Int(ChunkSize * 2, ChunkSize * 2));
        bigMan.SetInfoGenerator((ChunkLocation loc) => {
            return new BiggerSampleChunkInfo(loc, SeedForChunkLocation(BaseSeed + "2", loc));
        });
        bigMan.SetChunkGenerator((Vector2Int chunkSize, BiggerSampleChunkInfo info) => {
            // Here we use the info generated in the block above to populate the tile. Note that no random
            // generation is done here. It was done when generating the info. This allows us to clean up the
            // generated chunks while still keeping the lighter infos around.
            GameObject go = Instantiate(TilePrefabBig);
            go.name = $"Big Chunk {info.Location.Bound1}";
            go.transform.parent = this.transform;
            go.transform.localPosition = info.Location.Bound1.ToWorldPosition(Vector2Long.zero);

            go.GetComponent<TileThingBig>().Initialize(info);

            return go;
        });
        bigMan.SetChunkClearer((BiggerSampleChunkInfo chunk) => {
            Destroy(chunk.GeneratedObject as GameObject);
        });

        ChunkManager<SampleChunkInfo> man = new ChunkManager<SampleChunkInfo>(new Vector2Int(ChunkSize, ChunkSize));
        man.SetInfoGenerator((ChunkLocation loc) => {
            return new SampleChunkInfo(loc, SeedForChunkLocation(BaseSeed, loc));
        });
        man.SetChunkGenerator((Vector2Int chunkSize, SampleChunkInfo info) => {
            // Here we use the info generated in the block above to populate the tile. Note that no random
            // generation is done here. It was done when generating the info. This allows us to clean up the
            // generated chunks while still keeping the lighter infos around.
            GameObject go = Instantiate(TilePrefab);
            go.name = $"Chunk {info.Location.Bound1}";
            go.transform.parent = this.transform;
            go.transform.localPosition = info.Location.Bound1.ToWorldPosition(Vector2Long.zero);

            // In the chunk generator, you can access chunk infos of other tiles. Do NOT
            // access infos for other tiles in the info generator, as that WILL cause an
            // infinite loop. Here it'll either get a pre-existing one or generate it on
            // demand.
            SampleChunkInfo leftChunk = man.ChunkInfoWithOffset(info.Location, new Vector2Long(-1, 0));

            // You can also access the info for other managers. Same situation: you can access
            // infos here, but don't access infos from the info generator.
            BiggerSampleChunkInfo bigChunk = man.LookupChunkInfo(bigMan, info.Location);

            go.GetComponent<TileThing>().Initialize(info, leftChunk, bigChunk);

            return go;
        });
        man.SetChunkClearer((SampleChunkInfo chunk) => {
            // Clean up the chunk object. Usually that just means
            // destroying what you returned in SetChunkGenerator (it
            // gets assigned to GeneratedObject).
            Destroy(chunk.GeneratedObject as GameObject);
        });
        return new IChunkManager[] {
            bigMan,
            man
        };
    }


}
