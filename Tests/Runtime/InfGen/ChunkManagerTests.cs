using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

namespace KH.Infinite {
    public class ChunkManagerTests {
        [Test]
        public void BasicChunkManagerBehavior() {
            TestChunkManager manager = new TestChunkManager(new Vector2Int(50, 50));
            manager.SetRadius(5, 500);
            manager.ExpectInfoGenerationLocations(new Vector2Long(0, 0));
            // Shouldn't clean up anything.
            manager.ExpectChunkCleanupLocations();

            // Should only generate the current player chunk.
            manager.UpdateWithCleanup(new Vector2Long(10, 10));
            manager.AssertAllExpectedInfosGenerated();

            var generatedInfo = manager.LastGeneratedChunk;
            Assert.AreEqual(new Vector2Long(0, 0), generatedInfo.Location.Key);
            Assert.AreEqual(new Vector2Long(0, 0), generatedInfo.Location.Bound1);
            Assert.AreEqual(new Vector2Long(50, 50), generatedInfo.Location.Bound2);

            // Shouldn't generate any additional cells.
            manager.UpdateWithCleanup(new Vector2Long(10, 10));
            manager.AssertAllExpectedInfosGenerated();
        }

        [Test]
        public void SmallCreateAndLargeBoundsGeneratesCorrectly() {
            var manager = new TestChunkManager(new Vector2Int(200, 200));
            manager.SetRadius(10, 500);

            // Should generate all 4 chunks around the player.
            manager.ExpectInfoGenerationLocations(
                new Vector2Long(0, 0),
                new Vector2Long(-200, 0),
                new Vector2Long(0, -200),
                new Vector2Long(-200, -200)
            );
            // Shouldn't clean up anything.
            manager.ExpectChunkCleanupLocations();
            manager.ExpectInfoCleanupLocations();

            manager.UpdateWithCleanup(new Vector2Long(5, 5));
            manager.AssertAllExpectedInfosGenerated();
            manager.AssertAllExpectedChunksCleared();
            manager.AssertAllExpectedInfosCleared();
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
            // Shouldn't clean up anything.
            manager.ExpectChunkCleanupLocations();
            manager.ExpectInfoCleanupLocations();

            // Should not generate -200, -200, as the closest corner (0, 0) is > 10 away from (9, 9).
            manager.UpdateWithCleanup(new Vector2Long(9, 9));
            manager.AssertAllExpectedChunksGenerated();
            manager.AssertAllExpectedChunksCleared();
            manager.AssertAllExpectedInfosCleared();
        }

        [Test]
        public void BasicCleanup() {
            var manager = new TestChunkManager(new Vector2Int(200, 200));
            manager.SetRadius(10, 15);

            manager.ExpectInfoGenerationLocations(
                new Vector2Long(0, 0)
            );
            manager.ExpectChunkGenerationLocations(
                new Vector2Long(0, 0)
            );
            manager.ExpectChunkCleanupLocations();
            manager.ExpectInfoCleanupLocations();
            manager.UpdateWithCleanup(new Vector2Long(20, 20));

            // No cleanups at this stage.
            manager.AssertAllExpectedChunksGenerated();
            manager.AssertAllExpectedInfosGenerated();
            manager.AssertAllExpectedChunksCleared();
            manager.AssertAllExpectedInfosCleared();

            manager.ClearAllExpectations();

            manager.ExpectInfoCleanupLocations(
                new Vector2Long(0, 0)
            );
            manager.ExpectChunkCleanupLocations(
                new Vector2Long(0, 0)
            );
            manager.UpdateWithCleanup(new Vector2Long(-20, 20));
            manager.AssertAllExpectedChunksCleared();
            manager.AssertAllExpectedInfosCleared();
        }

        [Test]
        public void OtherManagerLookup() {
            var manager = new TestChunkManager(new Vector2Int(10, 10));
            manager.SetRadius(5, 5);
            var bigManager = new TestChunkManager(new Vector2Int(200, 200));
            bigManager.SetRadius(5, 5);

            TestInfo chunk = manager.ChunkInfoForWorldSpace(1, 1);

            // Should trigger a lookup in big manager;
            bigManager.ExpectInfoGenerationLocations(new Vector2Long(0, 0));
            manager.LookupChunkInfo(bigManager, chunk.Location);
            bigManager.AssertAllExpectedInfosGenerated();

            bigManager.ClearAllExpectations();

            // Should not trigger a lookup in big manager.
            chunk = manager.ChunkInfoForWorldSpace(-1, -1);
            bigManager.ExpectInfoGenerationLocations();
            Assert.IsFalse(manager.TryLookupChunkInfo(bigManager, chunk.Location, out var info));
            Assert.IsNull(info);
            bigManager.AssertAllExpectedInfosGenerated();

            bigManager.ClearAllExpectations();

            // Should correctly return on pre-existing chunk (generated by other lookup).
            chunk = manager.ChunkInfoForWorldSpace(1, 1);
            bigManager.ExpectInfoGenerationLocations();
            Assert.IsTrue(manager.TryLookupChunkInfo(bigManager, chunk.Location, out info));
            Assert.IsNotNull(info);
        }
    }

    class TestInfo : ChunkInfo {
        public TestInfo(ChunkLocation location) : base(location) { }
    }

    class TestChunkManager : ChunkManager<TestInfo> {
        public int InfoGeneratorCalls = 0;
        public int ChunkGeneratorCalls = 0;
        public int ChunkCleanupCalls = 0;
        public int InfoCleanupCalls = 0;
        public TestInfo LastGeneratedChunk;

        private List<Vector2Long> _expectedInfoGenerations;
        private List<Vector2Long> _expectedChunkGenerations;
        private List<Vector2Long> _expectedCleanups;
        private List<Vector2Long> _expectedInfoCleanups;

        public TestChunkManager(Vector2Int smallestUnit) : base(smallestUnit) {
            SetInfoGenerator((ChunkLocation loc) => {
                if (_expectedInfoGenerations != null) {
                    if (_expectedInfoGenerations.Contains(loc.Bound1)) {
                        _expectedInfoGenerations.Remove(loc.Bound1);
                    } else {
                        Assert.Fail($"Manager unexpectedly generated info for chunk at {loc.Bound1}");
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
                        Assert.Fail($"Manager unexpectedly generated chunk at {info.Location.Bound1}");
                    }
                }
                ChunkGeneratorCalls++;
                return null;
            });

            SetChunkClearer((TestInfo info) => {
                if (_expectedCleanups != null) {
                    if (_expectedCleanups.Contains(info.Location.Bound1)) {
                        _expectedCleanups.Remove(info.Location.Bound1);
                    } else {
                        Assert.Fail($"Manager unexpectedly cleaned up chunk at {info.Location.Bound1}");
                    }
                }
                ChunkCleanupCalls++;
            });
            SetInfoClearer((TestInfo info) => {
                if (_expectedInfoCleanups != null) {
                    if (_expectedInfoCleanups.Contains(info.Location.Bound1)) {
                        _expectedInfoCleanups.Remove(info.Location.Bound1);
                    } else {
                        Assert.Fail($"Manager unexpectedly cleaned up info at {info.Location.Bound1}");
                    }
                }
                InfoCleanupCalls++;
            });
        }

        public void UpdateWithCleanup(Vector2Long position) {
            Update(position);
            ForceCleanupAll();
        }

        public void ClearAllExpectations() {
            _expectedInfoGenerations = null;
            _expectedChunkGenerations = null;
            _expectedInfoCleanups = null;
            _expectedCleanups = null;
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
                Assert.Fail($"Did not generate info for locs: {string.Join(", ", _expectedInfoGenerations)}");
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
                Assert.Fail($"Did not generate chunk for locs: {string.Join(", ", _expectedChunkGenerations)}");
            }
        }

        public void ExpectChunkCleanupLocations(params Vector2Long[] locs) {
            if (_expectedCleanups != null) {
                _expectedCleanups.AddRange(locs);
            } else {
                _expectedCleanups = locs.ToList();
            }
        }

        public void AssertAllExpectedChunksCleared() {
            if (_expectedCleanups == null) {
                Assert.Fail("No cleanups were expected.");
            } else if (_expectedCleanups.Count > 0) {
                Assert.Fail($"Did not clear chunk for locs: {string.Join(", ", _expectedCleanups)}");
            }
        }

        public void ExpectInfoCleanupLocations(params Vector2Long[] locs) {
            if (_expectedInfoCleanups != null) {
                _expectedInfoCleanups.AddRange(locs);
            } else {
                _expectedInfoCleanups = locs.ToList();
            }
        }

        public void AssertAllExpectedInfosCleared() {
            if (_expectedInfoCleanups == null) {
                Assert.Fail("No info cleanups were expected.");
            } else if (_expectedInfoCleanups.Count > 0) {
                Assert.Fail($"Did not clear infos for locs: {string.Join(", ", _expectedCleanups)}");
            }
        }
    }
}
