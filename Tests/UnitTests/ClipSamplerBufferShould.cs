using NUnit.Framework;
using Unity.Entities;
using AssertionException = UnityEngine.Assertions.AssertionException;

namespace DMotion.Tests
{
    public class ClipSamplerBufferShould : ECSTestBase
    {
        private DynamicBuffer<ClipSampler> CreateSamplerBuffer()
        {
            var newEntity = manager.CreateEntity();
            return manager.AddBuffer<ClipSampler>(newEntity);
        }

        private static byte LastSamplerIndex = ClipSamplerUtils.MaxSamplersCount - 1;

        [Test]
        public void Add_And_Return_Id()
        {
            var samplers = CreateSamplerBuffer();
            Assert.Zero(samplers.Length);
            var id = samplers.AddWithId(new ClipSampler());
            Assert.Zero(id);
            Assert.AreEqual(samplers.Length, 1);
        }


        [Test]
        public void Add_And_Keep_Ids_Sorted()
        {
            var samplers = CreateSamplerBuffer();
            {
                var id = samplers.AddWithId(default);
                Assert.Zero(id);
                var s = samplers[0];
                s.Id = 12;
                samplers[0] = s;
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 13);
                var s = samplers[1];
                s.Id = 40;
                samplers[1] = s;
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 41);
                var s = samplers[2];
                s.Id = LastSamplerIndex;
                samplers[2] = s;
            }
            {
                var id = samplers.AddWithId(default);
                //we should loop back and return the next smallest id available
                Assert.AreEqual(id, 13);
            }

            for (var i = 1; i < samplers.Length; i++)
            {
                Assert.Greater(samplers[i].Id, samplers[i - 1].Id);
            }
        }

        [Test]
        public void Keep_Ids_Stable_When_Remove()
        {
            var samplers = CreateSamplerBuffer();
            {
                var id = samplers.AddWithId(default);
                Assert.Zero(id);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 1);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 2);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 3);
            }

            samplers.RemoveWithId(2);
            Assert.AreEqual(0, samplers[0].Id);
            Assert.AreEqual(1, samplers[1].Id);
            //We removed sampler with index and Id 2, id 3 should be stable
            Assert.AreEqual(3, samplers[2].Id);
        }

        [Test]
        public void Keep_Ids_Stable_When_RemoveRange()
        {
            var samplers = CreateSamplerBuffer();
            {
                var id = samplers.AddWithId(default);
                Assert.Zero(id);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 1);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 2);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 3);
            }

            samplers.RemoveRangeWithId(1, 2);
            Assert.AreEqual(0, samplers[0].Id);
            //We removed sampler 1 and 2, id 3 should be stable
            Assert.AreEqual(3, samplers[1].Id);
        }

        [Test]
        public void Limit_Reserve_Count()
        {
            var samplers = CreateSamplerBuffer();
            Assert.Throws<AssertionException>(() =>
            {
                samplers.TryFindIdAndInsertIndex(ClipSamplerUtils.MaxReserveCount + 1, out _, out _);
            });
        }

        [Test]
        public void Limit_Clip_SamplerCount()
        {
            var samplers = CreateSamplerBuffer();
            Assert.Throws<AssertionException>(() =>
            {
                samplers.Length += ClipSamplerUtils.MaxSamplersCount;
                samplers.TryFindIdAndInsertIndex(1, out _, out _);
            });
        }

        [Test]
        public void Return_Id_Zero_When_Empty()
        {
            var samplers = CreateSamplerBuffer();
            var success = samplers.TryFindIdAndInsertIndex(1, out var id, out var insertIndex);
            Assert.IsTrue(success);
            Assert.Zero(id);
            Assert.Zero(insertIndex);
        }

        [Test]
        public void IncrementId_When_Add()
        {
            var samplers = CreateSamplerBuffer();
            var id1 = samplers.AddWithId(default);
            var id2 = samplers.AddWithId(default);
            var id3 = samplers.AddWithId(default);
            Assert.AreEqual(id1, 0);
            Assert.AreEqual(id2, 1);
            Assert.AreEqual(id3, 2);
        }

        [Test]
        public void IncrementId_From_LastElement()
        {
            var samplers = CreateSamplerBuffer();
            var id1 = samplers.AddWithId(default);
            Assert.Zero(id1);
            var s = samplers[0];
            s.Id = 37;
            samplers[0] = s;
            var id2 = samplers.AddWithId(default);
            Assert.AreEqual(id2, 38);
        }


        [Test]
        public void LoopBackIndex_Length_One()
        {
            var samplers = CreateSamplerBuffer();
            var id1 = samplers.AddWithId(default);
            Assert.Zero(id1);
            var s = samplers[0];

            //change id to MaxSamplersCount to force loopback
            s.Id = LastSamplerIndex;
            samplers[0] = s;
            var success = samplers.TryFindIdAndInsertIndex(1, out var loopedId, out var insertIndex);
            Assert.IsTrue(success);
            Assert.Zero(loopedId, "Expected looped id to be zero in this case");
            Assert.AreEqual(insertIndex, 1);
        }

        [Test]
        public void LoopBackIndex_Length_GreatherThanOne()
        {
            var samplers = CreateSamplerBuffer();
            {
                var id = samplers.AddWithId(default);
                Assert.Zero(id);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 1);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(id, 2);
            }

            var s = samplers[2];
            s.Id = LastSamplerIndex;
            samplers[2] = s;
            {
                var id = samplers.AddWithId(default);
                //we should loop back to first available id
                Assert.AreEqual(id, 2);
            }
        }

        [Test]
        public void ReserveRange()
        {
            var samplers = CreateSamplerBuffer();
            {
                var id = samplers.AddWithId(default);
                Assert.Zero(id);
            }
            {
                var success = samplers.TryFindIdAndInsertIndex(10, out var id, out var insertIndex);
                Assert.True(success);
                Assert.AreEqual(1, id);
            }
        }
        
        [Test]
        public void ReserveRange_Loopback()
        {
            var reserveCount = 10;
            var samplers = CreateSamplerBuffer();
            {
                var id = samplers.AddWithId(default);
                Assert.Zero(id);
                var s = samplers[0];
                s.Id = (byte) (LastSamplerIndex - reserveCount / 2);
                samplers[0] = s;
            }
            {
                var success = samplers.TryFindIdAndInsertIndex(10, out var id, out var insertIndex);
                Assert.True(success);
                Assert.AreEqual(0, id);
            }
        }
        
        [Test]
        public void ReserveRange_BetweenElements_LoopBack()
        {
            const byte reserveCount = 10;
            var samplers = CreateSamplerBuffer();
            {
                var id = samplers.AddWithId(default);
                Assert.Zero(id);
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(1, id);
                var s = samplers[1];
                s.Id = 7;
                samplers[1] = s;
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(8, id);
                var s = samplers[2];
                s.Id += reserveCount + 1;
            }
            {
                var id = samplers.AddWithId(default);
                Assert.AreEqual(9, id);
                var s = samplers[3];
                s.Id = (byte) (LastSamplerIndex - reserveCount / 2);
                samplers[3] = s;
            }
            {
                var success = samplers.TryFindIdAndInsertIndex(10, out var id, out var insertIndex);
                Assert.True(success);
                Assert.AreEqual(9, id);
                Assert.AreEqual(insertIndex, 3);
            }
        }
    }
}