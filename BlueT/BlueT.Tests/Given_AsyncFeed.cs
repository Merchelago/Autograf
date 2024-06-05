#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Extensions.Reactive.Testing;
using Uno.Extensions.Reactive;
using Uno.Extensions.Reactive.Core;
using Uno.Extensions.Reactive.Sources;

namespace Uno.Extensions.Reactive.Tests.Sources;

public class Given_AsyncFeed : FeedTests
{
	[Test]
	public async Task When_ProviderReturnsValueSync_Then_GetSome()
	{
		var sut = Feed.Async(_ => ValueTask.FromResult(42));

		using var result = sut.Record();

		await result.Should().BeAsync(r => r
			.Message(Changed.Data, 42, Error.No, Progress.Final)
		);
	}

	[Test]
	public async Task When_ProviderReturnsValueAsync_Then_GetSome()
	{
		using var result =  F<int>.Record(r => Feed.Async(async ct =>
		{
			await r.WaitForMessages(1, ct);
			return 42;
		}));

		await result.Should().BeAsync(r => r
			.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
			.Message(Changed.Data & Changed.Progress, 42, Error.No, Progress.Final)
		);
	}

	[Test]
	public async Task When_ProviderReturnsDefaultSync_Then_GetNone()
	{
		var sut = Feed.Async(_ => ValueTask.FromResult(Option.None<int>()));
		using var result = sut.Record();

		await result.Should().BeAsync(r => r
			.Message(Changed.Data, Data.Some, Error.No, Progress.Final)
		);
	}

	[Test]
	public async Task When_ProviderReturnsSomeAsync_Then_GetValue()
	{
		using var result = F<Option<int>>.Record(r => Feed.Async(async ct =>
		{
			await r.WaitForMessages(1, ct);
			return Option.None<int>();
		}));

		await result.Should().BeAsync(r => r
			.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
			.Message(Changed.Data & Changed.Progress, Data.Some, Error.No, Progress.Final)
		);
	}

	[Test]
	public async Task When_ProviderThrowsSync_Then_GetError()
	{
		var sut = Feed.Async(_ =>
		{
			throw new TestException();
#pragma warning disable CS0162
			return ValueTask.FromResult(42);
#pragma warning restore CS0162
		});
		using var result = sut.Record();

		await result.Should().BeAsync(r => r
			.Message(Changed.Error, Data.Undefined, typeof(TestException), Progress.Final)
		);
	}

	[Test]
	public async Task When_ProviderThrowsAsync_Then_GetError()
	{
		using var result = F<int>.Record(r => Feed.Async(async ct =>
		{
			await r.WaitForMessages(1, ct);
			throw new TestException();
#pragma warning disable CS0162
			return 42;
#pragma warning restore CS0162
		}));

		await result.Should().BeAsync(r => r
			.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
			.Message(Changed.Error & Changed.Progress, Data.Undefined, typeof(TestException), Progress.Final)
		);
	}

	[Test]
	public async Task When_ProviderReturnsValueSyncAndRefresh_Then_GetSome()
	{
		var refresh = new Signal();
		var sut = Feed.Async(_ => ValueTask.FromResult(42), refresh);
		using var result = sut.Record();

		refresh.Raise();

		await result.Should().BeAsync(r => r
			.Message(Changed.Data, 42, Error.No, Progress.Final)
			.Message(Changed.Refreshed, 42, Error.No, Progress.Final/*, Refreshed.Is(1)*/)
		);
	}

	//[Test]
	//public async Task When_ProviderReturnsValueSyncAndRefreshUsingContext_Then_GetSome()
	//{
	//	var sut = new AsyncFeed<int>(async _ => 42);
	//	var requests = new RequestSource();
	//	await using var ctx = Context.SourceContext.CreateChild(requests);
	//	using var result = sut.Record(ctx);

	//	await result.WaitForMessages(1, CT);

	//	requests.RequestRefresh();

	//	await result.Should().BeAsync(r => r
	//		.Message(Changed.Data, 42, Error.No, Progress.Final)
	//		.Message(Changed.Refreshed, 42, Error.No, Progress.Final, Refreshed.Is(1))
	//	);
	//}

	[Test, CancelAfter(2000)]
	public async Task When_ProviderReturnsValueAsyncAndRefresh_Then_GetSome(CancellationToken ct)
	{
		var refresh = new Signal();
		var gate = new TaskCompletionSource();
		var sut = Feed.Async(
			async _ =>
			{
				await gate.Task;
				return 42;
			},
			refresh);
		using var result = sut.Record();

		await result.WaitForMessages(1, ct);
		gate.SetResult();

		await result.WaitForMessages(2, ct);
		gate = new TaskCompletionSource();
		refresh.Raise();

		await result.WaitForMessages(3, ct);
		gate.SetResult();

		await result.Should().BeAsync(r => r
			.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
			.Message(Changed.Data & Changed.Progress, 42, Error.No, Progress.Final)
			.Message(Changed.Progress & Changed.Refreshed, 42, Error.No, Progress.Transient/*, Refreshed.Is(1)*/)
			.Message(Changed.Progress, 42, Error.No, Progress.Final/*, Refreshed.Is(1)*/)
			, ct);
	}

	////[Test]
	////public async Task When_ProviderReturnsValueAsyncAndRefreshUsingContext_Then_GetSome()
	////{
	////	var ct = TestContext.CancellationTokenSource.Token;
	////	var requests = new RequestSource();
	////	await using var ctx = Context.SourceContext.CreateChild(requests);
	////	var gate = new TaskCompletionSource();
	////	var sut = new AsyncFeed<int>(
	////		async _ =>
	////		{
	////			await gate.Task;
	////			return 42;
	////		});
	////	using var result = sut.Record(ctx);

	////	await result.WaitForMessages(1, ct);
	////	gate.SetResult();

	////	await result.WaitForMessages(2, ct);
	////	gate = new TaskCompletionSource();
	////	requests.RequestRefresh();

	////	await result.WaitForMessages(3, ct);
	////	gate.SetResult();

	////	await result.Should().BeAsync(r => r
	////		.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
	////		.Message(Changed.Data & Changed.Progress, 42, Error.No, Progress.Final)
	////		.Message(Changed.Progress & Changed.Refreshed, 42, Error.No, Progress.Transient, Refreshed.Is(1))
	////		.Message(Changed.Progress, 42, Error.No, Progress.Final, Refreshed.Is(1))
	////	);
	////}

	[Test]
	public async Task When_ProviderReturnsDefaultSyncAndRefresh_Then_GetNone()
	{
		var refresh = new Signal();
		var sut = Feed.Async(_ => ValueTask.FromResult(Option.None<int>()), refresh);
		using var result = sut.Record();

		refresh.Raise();

		await result.Should().BeAsync(r => r
			.Message(Changed.Data, Data.Some, Error.No, Progress.Final)
			.Message(Changed.Refreshed, Data.Some, Error.No, Progress.Final/*, Refreshed.Is(1)*/)
		);
	}

	//[Test]
	//public async Task When_ProviderReturnsDefaultSyncAndRefreshUsingContext_Then_GetNone()
	//{
	//	var requests = new RequestSource();
	//	await using var ctx = Context.SourceContext.CreateChild(requests);
	//	var sut = new AsyncFeed<int>(async _ => Option.None<int>());
	//	using var result = sut.Record(ctx);

	//	requests.RequestRefresh();

	//	await result.Should().BeAsync(r => r
	//		.Message(Changed.Data, Data.None, Error.No, Progress.Final)
	//		.Message(Changed.Refreshed, Data.None, Error.No, Progress.Final, Refreshed.Is(1))
	//	);
	//}

	[Test]
	public async Task When_ProviderReturnsSomeAsyncAndRefresh_Then_GetValue()
	{
		var refresh = new Signal();
		var gate = new TaskCompletionSource();
		var sut = Feed.Async(
			async _ =>
			{
				await gate.Task;
				return Option.None<int>();
			},
			refresh);
		using var result = sut.Record();

		await result.WaitForMessages(1, CT);
		gate.SetResult();

		await result.WaitForMessages(2, CT);
		gate = new TaskCompletionSource();
		await Task.Delay(10, CT);
		refresh.Raise();

		await result.WaitForMessages(3, CT);
		gate.SetResult();

		await result.Should().BeAsync(r => r
			.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
			.Message(Changed.Data & Changed.Progress, Data.Some, Error.No, Progress.Final)
			.Message(Changed.Progress & Changed.Refreshed, Data.Some, Error.No, Progress.Transient/*, Refreshed.Is(1)*/)
			.Message(Changed.Progress, Data.Some, Error.No, Progress.Final/*, Refreshed.Is(1)*/)
		);
	}

	//[Test]
	//public async Task When_ProviderReturnsSomeAsyncAndRefreshUsingContext_Then_GetValue()
	//{
	//	var requests = new RequestSource();
	//	await using var ctx = Context.SourceContext.CreateChild(requests);
	//	var gate = new TaskCompletionSource();
	//	var sut = new AsyncFeed<int>(
	//		async _ =>
	//		{
	//			await gate.Task;
	//			return Option.None<int>();
	//		});
	//	using var result = sut.Record(ctx);

	//	await result.WaitForMessages(1, CT);
	//	gate.SetResult();

	//	await result.WaitForMessages(2, CT);
	//	gate = new TaskCompletionSource();
	//	await Task.Delay(10, CT);
	//	requests.RequestRefresh();

	//	await result.WaitForMessages(3, CT);
	//	gate.SetResult();

	//	await result.Should().BeAsync(r => r
	//		.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
	//		.Message(Changed.Data & Changed.Progress, Data.None, Error.No, Progress.Final)
	//		.Message(Changed.Progress & Changed.Refreshed, Data.None, Error.No, Progress.Transient, Refreshed.Is(1))
	//		.Message(Changed.Progress, Data.None, Error.No, Progress.Final, Refreshed.Is(1))
	//	);
	//}

	[Test]
	public async Task When_ProviderThrowsSyncAndRefresh_Then_GetError()
	{
		var refresh = new Signal();
		var sut = Feed.Async(_ =>
		{
			throw new TestException();
#pragma warning disable CS0162
			return ValueTask.FromResult(42);
#pragma warning restore CS0162
		}, refresh);
		using var result = sut.Record();

		refresh.Raise();

		await result.Should().BeAsync(r => r
			.Message(Data.Undefined, typeof(TestException), Progress.Final)
			.Message(Changed.Error & Changed.Refreshed, Data.Undefined, typeof(TestException), Progress.Final/*, Refreshed.Is(1)*/)
		);
	}

	////[Test]
	////public async Task When_ProviderThrowsSyncAndRefreshUsingContext_Then_GetError()
	////{
	////	var requests = new RequestSource();
	////	await using var ctx = Context.SourceContext.CreateChild(requests);
	////	var sut = new AsyncFeed<int>(async _ => throw new TestException());
	////	using var result = sut.Record(ctx);

	////	requests.RequestRefresh();

	////	await result.Should().BeAsync(r => r
	////		.Message(Changed.Error, Data.Undefined, typeof(TestException), Progress.Final)
	////		.Message(Changed.Error & Changed.Refreshed, Data.Undefined, typeof(TestException), Progress.Final, Refreshed.Is(1))
	////	);
	////}
	
	[Test, CancelAfter(2000)]

	public async Task When_ProviderThrowsAsyncAndRefresh_Then_GetError(CancellationToken ct)
	{
		var refresh = new Signal();
		var gate = new TaskCompletionSource();
		var sut = Feed.Async(
			async _ =>
			{
				await gate.Task;
				throw new TestException();
#pragma warning disable CS0162
				return 42;
#pragma warning restore CS0162
			},
			refresh);
		using var result = sut.Record();

		await result.WaitForMessages(1, ct);
		gate.SetResult();

		await result.WaitForMessages(2, ct);
		gate = new TaskCompletionSource();
		refresh.Raise();

		await result.WaitForMessages(3, ct);
		gate.SetResult();

		await result.Should().BeAsync(r => r
			.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
			.Message(Changed.Error & Changed.Progress, Data.Undefined, typeof(TestException), Progress.Final)
			.Message(Changed.Progress & Changed.Refreshed, Data.Undefined, typeof(TestException), Progress.Transient/*, Refreshed.Is(1)*/)
			.Message(Changed.Error & Changed.Progress, Data.Undefined, typeof(TestException), Progress.Final/*, Refreshed.Is(1)*/)
			, ct);
	}

	////[Test]
	////public async Task When_ProviderThrowsAsyncAndRefreshUsingContext_Then_GetError()
	////{
	////	var requests = new RequestSource();
	////	await using var ctx = Context.SourceContext.CreateChild(requests);
	////	var gate = new TaskCompletionSource();
	////	var sut = new AsyncFeed<int>(
	////		async _ =>
	////		{
	////			await gate.Task;
	////			throw new TestException();
	////		});
	////	using var result = sut.Record(ctx);

	////	await result.WaitForMessages(1, CT);
	////	gate.SetResult();

	////	await result.WaitForMessages(2, CT);
	////	gate = new TaskCompletionSource();
	////	requests.RequestRefresh();

	////	await result.WaitForMessages(3, CT);
	////	gate.SetResult();

	////	await result.Should().BeAsync(r => r
	////		.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
	////		.Message(Changed.Error & Changed.Progress, Data.Undefined, typeof(TestException), Progress.Final)
	////		.Message(Changed.Progress & Changed.Refreshed, Data.Undefined, typeof(TestException), Progress.Transient, Refreshed.Is(1))
	////		.Message(Changed.Error & Changed.Progress, Data.Undefined, typeof(TestException), Progress.Final, Refreshed.Is(1))
	////	);
	////}

	[Test]
	public async Task When_ProviderReturnSomeThenThrows_Then_KeepPreviousData()
	{
		var refresh = new Signal();
		var shouldThrow = false;
		var sut = Feed.Async(_ =>
			{
				if (shouldThrow)
					throw new TestException();
				
				return ValueTask.FromResult(42);
			},
			refresh);
		using var result = sut.Record();

		await result.WaitForMessages(1, CT);
		shouldThrow = true;
		refresh.Raise();

		await result.WaitForMessages(2, CT);
		shouldThrow = false;
		refresh.Raise();

		await result.Should().BeAsync(r => r
			.Message(Changed.Data, 42, Error.No, Progress.Final)
			.Message(Changed.Error & Changed.Refreshed, 42, typeof(TestException), Progress.Final/*, Refreshed.Is(1)*/)
			.Message(Changed.Error & Changed.Refreshed, 42, Error.No, Progress.Final/*, Refreshed.Is(2)*/)
		);
	}

	[Test]
	public async Task When_NoContext_Then_NoCaching()
	{
		//Context.ResignCurrent();

		var invokeCount = 0;
		var sut = Feed.Async(_ =>
		{
			invokeCount++;
			return ValueTask.FromResult(Option.None<int>());
		});

		await sut;
		await sut;

		invokeCount.Should().Be(2);
	}

	//[Test]
	//public async Task When_InvokeProvider_Then_ContextSet()
	//{
	//	Context.ResignCurrent(); // We prefer to explicitly give it in GetSource to avoid any uncontrolled flowing

	//	var sut = Feed.Async(_ =>
	//	{
	//		SourceContext.Current.Should().Be(Context.SourceContext);
	//		return ValueTask.FromResult(Option.None<int>());
	//	});
	//	using var result = sut.Record(Context.SourceContext);

	//	await result.Should().BeAsync(r => r
	//		.Message(Error.No)
	//	);
	//}
}
