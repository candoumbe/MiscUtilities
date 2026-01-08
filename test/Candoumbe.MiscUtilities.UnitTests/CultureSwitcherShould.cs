using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;
using Xunit.OpenCategories.V3;

namespace Candoumbe.MiscUtilities.UnitTests;

[UnitTest]
public class CultureSwitcherShould
{
    [Property]
    public void Change_the_culture_inside_the_action(CultureInfo cultureInfo)
    {
        // Arrange
        string cultureInsideAction = null;
        string cultureUiInsideAction = null;

        CultureSwitcher cultureSwitcher = new();

        // Act
        cultureSwitcher.Run(cultureInfo, () =>
        {
            cultureInsideAction = CultureInfo.CurrentCulture.Name;
            cultureUiInsideAction = CultureInfo.CurrentUICulture.Name;

            // Assert
            cultureInsideAction.Should().Be(cultureInfo.Name, """the culture inside the action was changed to the culture specified when calling "Run" method""");
            cultureUiInsideAction.Should().Be(cultureInfo.Name, """the culture UI inside the action was changed to the culture specified when calling "Run" method""");
        });
    }

    [Property]
    public async Task Change_the_culture_inside_the_task(CultureInfo cultureInfo)
    {
        // Arrange
        string cultureInsideAction = null;
        string cultureUiInsideAction = null;

        CultureSwitcher cultureSwitcher = new();

        // Act
        await cultureSwitcher.RunAsync(cultureInfo, _ => Task.Run(() =>
        {
            cultureInsideAction = Thread.CurrentThread.CurrentCulture.Name;
            cultureUiInsideAction = Thread.CurrentThread.CurrentUICulture.Name;

            // Assert
            cultureInsideAction.Should().Be(cultureInfo.Name, """the culture inside the action was changed to the culture specified when calling "Run" method""");
            cultureUiInsideAction.Should().Be(cultureInfo.Name, """the culture UI inside the action was changed to the culture specified when calling "Run" method""");
        }, TestContext.Current.CancellationToken));
    }

    [Property]
    public void Run_action_on_the_same_thread_as_the_calling_thread(CultureInfo cultureInfo)
    {
        // Arrange
        int currentThreadId = Environment.CurrentManagedThreadId;
        int actionThreadId = 0;

        CultureSwitcher cultureSwitcher = new();

        // Act
        cultureSwitcher.Run(cultureInfo, () => actionThreadId = Environment.CurrentManagedThreadId);

        // Assert
        actionThreadId.Should().Be(currentThreadId, "the action is executed on the caller's thread");
    }

    [Property]
    public async Task RunAsync_action_on_thread_that_is_different_from_the_caller_thread(CultureInfo cultureInfo)
    {
        // Arrange
        int currentThreadId = Environment.CurrentManagedThreadId;
        int actionThreadId = 0;

        CultureSwitcher cultureSwitcher = new();

        // Act
        await cultureSwitcher.RunAsync(cultureInfo,
                                       action: _ => actionThreadId = Environment.CurrentManagedThreadId,
                                       cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        actionThreadId.Should().NotBe(currentThreadId, "the action is executed on a different thread than the caller's thread");
    }

    [Property]
    public async Task Bubble_exception_to_the_caller(CultureInfo cultureInfo)
    {
        // Arrange
        CultureSwitcher cultureSwitcher = new();
        Action<CancellationToken> action = _ => throw new NullReferenceException();

        Func<Task> runThrowsException = () => cultureSwitcher.RunAsync(cultureInfo, action);

        // Act & Assert
        await runThrowsException
            .Should()
            .ThrowAsync<NullReferenceException>();
    }

    [Property]
    public async Task Throws_TimeoutException_when_action_exceeds_cancellationtoken_deadline(CultureInfo cultureInfo)
    {
        // Arrange
        CultureSwitcher cultureSwitcher = new();
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        Action<CancellationToken> action = token =>
        {
            // Simulate long-running work
            Task.Delay(TimeSpan.FromSeconds(5), token).Wait(token);
        };

        Func<Task> runThrowsException = () => cultureSwitcher.RunAsync(cultureInfo,
                                                                       action, cts.Token);

        // Act & Assert
        await runThrowsException
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }
}