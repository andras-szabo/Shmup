using UnityEngine;
using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RingBuffer;

public class RingBufferTests
{
	public class TestData
	{
		public TestData(int v) { someValue = v; }
		public int someValue;
	}

	[Test]
	public void CtorTest()
	{
		var buffer = new RingBuffer<int>(50);
		Assert.IsTrue(buffer != null);
		Assert.IsTrue(buffer.Capacity == 50);
		Assert.IsTrue(buffer.Count == 0, buffer.Count.ToString());
	}

	[Test]
	public void SimplePushTest()
	{
		var buffer = new RingBuffer<int>(50);
		for (int i = 0; i < 50; ++i)
		{
			buffer.Push(i);
		}

		Assert.IsTrue(buffer.Count == 50, buffer.Count.ToString());
		for (int i = 0; i < 10; ++i)
		{
			var newItem = buffer.Pop();
			Assert.IsTrue(newItem == 49 - i, string.Format("Exp: {0}, act: {1}", 50-i, newItem));
		}

		Assert.IsTrue(buffer.Count == 40, buffer.Count.ToString());
	}

	[Test]
	public void EdgeCaseTest()
	{
		var b = new RingBuffer<int>(128);
		for (int i = 0; i < 128; ++i) { b.Push(i); }
		Assert.IsTrue(b.Count == 128);

		for (int i = 0; i < 3; ++i) { b.Push(-1); }
		Assert.IsTrue(b.Count == 128);

		for (int i = 0; i < 3; ++i) { b.Pop(); }
		Assert.IsTrue(b.Peek() == 127);
	}

	[Test]
	public void WrapAroundTest()
	{
		var buffer = new RingBuffer<int>(10);
		for (int i = 0; i < 11; ++i)
		{
			buffer.Push(i);
		}
		Assert.IsTrue(buffer.Count == 10, buffer.Count.ToString());

		var expected = 10;
		while (!buffer.IsEmpty)
		{
			var newItem = buffer.Pop();
			Assert.IsTrue(newItem == expected--, newItem.ToString());
		}
	}

	[Test]
	public void WrapAroundTwiceTest()
	{
		var buffer = new RingBuffer<int>(10);
		for (int i = 0; i < 25; ++i)
		{
			buffer.Push(i);
		}

		Assert.IsTrue(buffer.Count == 10, buffer.Count.ToString());

		var expected = 24;
		while (!buffer.IsEmpty)
		{
			var newItem = buffer.Pop();
			Assert.IsTrue(newItem == expected--, newItem.ToString());
		}

		Assert.IsTrue(buffer.Count == 0, buffer.Count.ToString());

		for (int i = 0; i < 8; ++i)
		{
			buffer.Push(i);
		}

		expected = 7;
		while (!buffer.IsEmpty)
		{
			var newItem = buffer.Pop();
			Assert.IsTrue(newItem == expected--, newItem.ToString());
		}
	}

	[Test]
	public void ClearBuffer()
	{
		var buffer = new RingBuffer<int>(10);
		for (int i = 0; i < 10; ++i)
		{
			buffer.Push(i);
		}

		Assert.IsTrue(buffer.Count == 10);

		buffer.Clear();

		Assert.IsTrue(buffer.IsEmpty && buffer.Count == 0);
	}

	[Test]
	public void PeekTest()
	{
		var b = new RingBuffer<string>(4);

		b.Push("John");
		b.Push("Paul");
		b.Push("George");
		b.Push("Ringo");

		Assert.IsTrue(b.Peek() == "Ringo", b.Peek());
		Assert.IsTrue(b.Count == 4);

		b.Push("Pete");

		Assert.IsTrue(b.Peek() == "Pete", b.Peek());
		Assert.IsTrue(b.Count == 4);

		b.Pop();

		Assert.IsTrue(b.Peek() == "Ringo", b.Peek());
		Assert.IsTrue(b.Count == 3);
	}

	[Test]
	public void UpdateLastEntryTest()
	{
		var b = new RingBuffer<string>(4);

		b.Push("John");	b.Push("Paul");	b.Push("George"); b.Push("Pete");

		b.UpdateLastEntry(entry => "Ringo");

		Assert.IsTrue(b.Peek() == "Ringo", b.Peek());
	}

	[Test]
	public void UpdateVectorLastEntryTest()
	{
		var b = new RingBuffer<Vector2>(3);

		for (int i = 0; i < 3; ++i)
		{
			b.Push(new Vector2(1f, 1f));
		}

		Assert.IsTrue(b.Peek() == Vector2.one, b.Peek().ToString());

		b.UpdateLastEntry(vec => new Vector2(vec.x + 2f, vec.y + 2f));

		var target = new Vector2(3f, 3f);

		Assert.IsTrue(b.Peek() == target, b.Peek().ToString());

		b.Pop();

		Assert.IsTrue(b.Peek() == new Vector2(1f, 1f));

		b.UpdateLastEntry(vec => { vec.x = 2f; return vec; });	// this copies twice

		Assert.IsTrue(b.Peek() == new Vector2(2f, 1f));

		// but this wouldn't work: b.Peek().x = 4; because T is Vector2 which is a struct
	}

	[Test]
	public void UpdateClassData()
	{
		var b = new RingBuffer<TestData>(4);
		for (int i = 0; i < 4; ++i) { b.Push(new TestData(i)); }
		Assert.IsTrue(b.Peek().someValue == 3, b.Peek().someValue.ToString());

		b.UpdateLastEntry(entry => { entry.someValue = 9; return entry; });
		Assert.IsTrue(b.Peek().someValue == 9, b.Peek().someValue.ToString());

		b.Peek().someValue = 12;

		Assert.IsTrue(b.Peek().someValue == 12, b.Peek().someValue.ToString());
	}

	[Test]
	public void RandomAccessTest()
	{
		var buffer = new RingBuffer<int>(100);

		for (int i = 0; i < 50; ++i) { buffer.Push(i); }
		var expected = 49;
		for (int i = 0; i < 25; ++i)
		{
			var nextItem = buffer.Pop();
			Assert.IsTrue(nextItem == expected--);
		}

		for (int i = 0; i < 30; ++i) { buffer.Push(199); }
		for (int i = 0; i < 30; ++i) { Assert.IsTrue(buffer.Pop() == 199); }
		for (int i = 0; i < 10; ++i) { Assert.IsTrue(buffer.Pop() == expected--); }
		
		for (int i = 0; i < 4; ++i) { buffer.Push(922); }
		for (int i = 0; i < 4; ++i) { Assert.IsTrue(buffer.Pop() == 922); }
		for (int i = 0; i < 15; ++i) { Assert.IsTrue(buffer.Pop() == expected--); }

		Assert.IsTrue(buffer.IsEmpty, "Not empty");
	}

	[Test]
	public void ClearBeforeFull()
	{
		var buffer = new RingBuffer<int>(50);
		for (int i = 0; i < 10; ++i)
		{
			buffer.Push(i);
		}

		Assert.IsTrue(buffer.Count == 10);

		buffer.Clear();

		Assert.IsTrue(buffer.IsEmpty && buffer.Count == 0);

		for (int i = 20; i < 25; ++i)
		{
			buffer.Push(i);
		}

		Assert.IsTrue(buffer.Count == 5);

		var expected = 24;
		while (!buffer.IsEmpty)
		{
			var nextItem = buffer.Pop();
			Assert.IsTrue(nextItem == expected--);
		}
	}
}

