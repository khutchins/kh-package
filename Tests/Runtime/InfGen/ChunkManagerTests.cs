using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

namespace KH.Infinite {
    public class ChunkManagerTests : MonoBehaviour {


        [Test]
        public void BasicChunkManagerBehavior() {
            TestChunkManager manager = new TestChunkManager(new Vector2Int(50, 50));
            manager.SetRadius(5, 500);
            manager.ExpectInfoGenerationLocations(new Vector2Long(0, 0));

            // Should only generate the current player chunk.
            manager.Update(new Vector2Long(10, 10));
            manager.AssertAllExpectedInfosGenerated();

            var generatedInfo = manager.LastGeneratedChunk;
            Assert.AreEqual(new Vector2Long(0, 0), generatedInfo.Location.Key);
            Assert.AreEqual(new Vector2Long(0, 0), generatedInfo.Location.Bound1);
            Assert.AreEqual(new Vector2Long(50, 50), generatedInfo.Location.Bound2);

            // Shouldn't generate any additional cells.
            manager.Update(new Vector2Long(10, 10));
            manager.AssertAllExpectedInfosGenerated();
        }

        [Test]
        public void SmallCreateAndLargeBoundsGeneratesCorrectly() {
            var manager = new TestChunkManager(new Vector2Int(200, 200));
            manager.SetRadius(10, 500);

            manager.ExpectInfoGenerationLocations(
                new Vector2Long(0, 0),
                new Vector2Long(-200, 0),
                new Vector2Long(0, -200),
                new Vector2Long(-200, -200)
            );

            // Should generate all 4 chunks around the player.
            manager.Update(new Vector2Long(5, 5));
            manager.AssertAllExpectedInfosGenerated();
        }

        [Test]
        public void ChunkCreationOnlyHappensInRadius() {
            var manager = new TestChunkManager(new Vector2Int(200, 200));
            manager.SetRadius(10, 500);

            // Current behavior is that infos are generated aggressively (in square box) but it
            // only generates chunks if they're actually within the range. I might optimize info
            // generation later, so that is not asserted here.

            manager.ExpectChunkGenerationLocations(
                new Vector2Long(0, 0),
                new Vector2Long(-200, 0),
                new Vector2Long(0, -200)
            );

            // Should not generate -200, -200, as the closest corner (0, 0) is > 10 away from (9, 9).
            manager.Update(new Vector2Long(9, 9));
            manager.AssertAllExpectedChunksGenerated();
        }
    }

    class TestInfo : ChunkInfo {
        public TestInfo(ChunkLocation location) : base(location) { }
    }

    class TestChunkManager : ChunkManager<TestInfo> {
        public int InfoGeneratorCalls = 0;
        public int ChunkGeneratorCalls = 0;
        public TestInfo LastGeneratedChunk;

        private List<Vector2Long> _expectedInfoGenerations;
        private List<Vector2Long> _expectedChunkGenerations;

        public TestChunkManager(Vector2Int smallestUnit) : base(smallestUnit) {
            SetInfoGenerator((ChunkLocation loc) => {
                if (_expectedInfoGenerations != null) {
                    if (_expectedInfoGenerations.Contains(loc.Bound1)) {
                        _expectedInfoGenerations.Remove(loc.Bound1);
                    } else {
                        Assert.Fail($"Manager unexpectedly generated info for chunk at ${loc.Bound1}");
                    }
                }
                LastGeneratedChunk = new TestInfo(loc);
                InfoGeneratorCalls++;
                return LastGeneratedChunk;
            });
            SetChunkGenerator((Vector2Int chunkSize, TestInfo info) => {
                if (_expectedChunkGenerations != null) {
                    if (_expectedChunkGenerations.Contains(info.Location.Bound1)) {
                        _expectedChunkGenerations.Remove(info.Location.Bound1);
                    } else {
                        Assert.Fail($"Manager unexpectedly generated chunk at ${info.Location.Bound1}");
                    }
                }
                ChunkGeneratorCalls++;
                return null;
            });
        }

        public void ExpectInfoGenerationLocations(params Vector2Long[] locs) {
            if (_expectedInfoGenerations != null) {
                _expectedInfoGenerations.AddRange(locs);
            } else {
                _expectedInfoGenerations = locs.ToList();
            }
        }

        public void AssertAllExpectedInfosGenerated() {
            if (_expectedInfoGenerations == null) {
                Assert.Fail("No infos were expected.");
            } else if (_expectedInfoGenerations.Count > 0) {
                Assert.Fail($"Did not generate info for locs: ${string.Join(", ", _expectedInfoGenerations)}");
            }
        }

        public void ExpectChunkGenerationLocations(params Vector2Long[] locs) {
            if (_expectedChunkGenerations != null) {
                _expectedChunkGenerations.AddRange(locs);
            } else {
                _expectedChunkGenerations = locs.ToList();
            }
        }

        public void AssertAllExpectedChunksGenerated() {
            if (_expectedChunkGenerations == null) {
                Assert.Fail("No chunks were expected.");
            } else if (_expectedChunkGenerations.Count > 0) {
                Assert.Fail($"Did not generate chunk for locs: ${string.Join(", ", _expectedInfoGenerations)}");
            }
        }
    }
}
