using NUnit.Framework;
using DataOrientedDriver;
using System;

namespace DODUnitTest
{
    public class Tests
    {
        [Test]
        public void MockBuilder_BuildFromXml_MockCountTree()
        {
            var system = new BehaviorSystem();
            var blackBoard = new MockBlackBoard();
            var builder = new MockBuilder(system, blackBoard);
            var root = builder.BuildFromXml("../../../MockCountTree.xml");
            Assert.NotNull(root);
        }

        [TestCase(10)]
        [TestCase(955)]
        [TestCase(3940)]
        [TestCase(59307)]
        public void MockBuilder_BuildFromXml_CountMatch(int count)
        {
            var system = new BehaviorSystem();
            var blackBoard = new MockBlackBoard();
            var builder = new MockBuilder(system, blackBoard);
            var root = builder.BuildFromXml("../../../MockCountTree.xml");
            root.Enter();
            for (int i=0; i < count; i++)
            {
                system.Step(1f);
            }
            var mockCount = blackBoard.GetInt("count");
            Console.WriteLine($"the count is: {mockCount}");
            Assert.AreEqual((count - 1) / 3, mockCount);
        }
    }
}