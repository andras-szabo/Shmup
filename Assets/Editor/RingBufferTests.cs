using UnityEngine;
using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RingBuffer;

public class RingBufferTests
{
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
}
