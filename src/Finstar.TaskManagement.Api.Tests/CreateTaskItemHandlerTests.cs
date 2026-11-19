using Finstar.TaskManagement.Api.Handlers;
using Finstar.TaskManagement.Api.Handlers.CreateTaskItem;
using Finstar.TaskManagement.ChangeTracker.Contracts;
using Finstar.TaskManagement.Dal;
using Finstar.TaskManagement.Models.Entities;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace Finstar.TaskManagement.Api.Tests;

public class CreateTaskItemHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    private readonly ITaskManagementStorage _storage = Substitute.For<ITaskManagementStorage>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();
    private readonly FakeTimeProvider _timeProvider = new(Now);

    private readonly DbSet<TaskItem> _taskItems = new List<TaskItem>().BuildMockDbSet();
    private readonly List<TaskItem> _added = new();

    private readonly CreateTaskItemHandler _sut;

    public CreateTaskItemHandlerTests()
    {
        _taskItems
            .When(x => x.Add(Arg.Any<TaskItem>()))
            .Do(call => _added.Add(call.Arg<TaskItem>()));

        _storage.TaskItems.Returns(_taskItems);
        SetupUsers(); // по умолчанию пользователей нет

        _sut = new CreateTaskItemHandler(_storage, _timeProvider, _publisher);
    }

    [Fact]
    public async Task Handle_WithoutUserId_CreatesUnassignedTask()
    {
        var request = new CreateTaskItemRequest { Title = "Title", Description = "Description", UserId = null };

        var result = await _sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var item = _added.Should().ContainSingle().Subject;


        item.Title.Should().Be("Title");
        item.Description.Should().Be("Description");
        item.Status.Should().Be(Models.TaskStatus.New);
        item.UserId.Should().BeNull();
        item.CreatedAt.Should().Be(Now);
        item.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public async Task Handle_WithoutUserId_ReturnsIdOfCreatedItem()
    {
        var request = new CreateTaskItemRequest { Title = "Title" };

        var result = await _sut.Handle(request, CancellationToken.None);

        var item = _added.Single();
        item.Id.Should().NotBeEmpty();
        item.Id.Version.Should().Be(7);
        result.Data!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task Handle_WithExistingUser_AssignsTaskToUser()
    {
        var userId = Guid.NewGuid(); 
        SetupUsers(new User { Id = userId });
        var request = new CreateTaskItemRequest { Title = "Title", UserId = userId };
    
        var result = await _sut.Handle(request, CancellationToken.None);
    
        result.IsSuccess.Should().BeTrue();
        _added.Should().ContainSingle().Which.UserId.Should().Be(userId);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingUser_ReturnsUserNotFoundFailure()
    {
        SetupUsers(new User { Id = Guid.NewGuid() }); // в базе есть другой пользователь
        var request = new CreateTaskItemRequest { Title = "Title", UserId = Guid.NewGuid() };
    
        var result = await _sut.Handle(request, CancellationToken.None);
    
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ErrorCodes.CreateTaskItem.UserNotFound);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingUser_DoesNotSaveOrPublishAnything()
    {
        var request = new CreateTaskItemRequest { Title = "Title", UserId = Guid.NewGuid() };
    
        await _sut.Handle(request, CancellationToken.None);
    
        _added.Should().BeEmpty();
        await _storage.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive()
            .Publish(Arg.Any<TaskChangedEvent>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_OnSuccess_PublishesCreatedEventOnce()
    {
        var request = new CreateTaskItemRequest { Title = "Title" };
    
        await _sut.Handle(request, CancellationToken.None);
    
        var item = _added.Single();
        await _publisher.Received(1).Publish(
            Arg.Is<TaskChangedEvent>(e => e.TaskId == item.Id), // TODO: поля события
            Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_OnSuccess_PublishesBeforeSaving()
    {
        var request = new CreateTaskItemRequest { Title = "Title" };
    
        await _sut.Handle(request, CancellationToken.None);
    
        Received.InOrder(() =>
        {
            _publisher.Publish(Arg.Any<TaskChangedEvent>(), Arg.Any<CancellationToken>());
            _storage.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }
    
    [Fact]
    public async Task Handle_OnSuccess_SavesChangesOnce()
    {
        await _sut.Handle(new CreateTaskItemRequest { Title = "Title" }, CancellationToken.None);
    
        await _storage.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task Handle_PassesCancellationTokenToDependencies()
    {
        using var cts = new CancellationTokenSource();
    
        await _sut.Handle(new CreateTaskItemRequest { Title = "Title" }, cts.Token);
    
        await _publisher.Received(1).Publish(Arg.Any<TaskChangedEvent>(), cts.Token);
        await _storage.Received(1).SaveChangesAsync(cts.Token);
    }
    
    [Fact]
    public async Task Handle_WhenPublishThrows_PropagatesExceptionAndDoesNotSave()
    {
        _publisher
            .Publish(Arg.Any<TaskChangedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("broker down")));
    
        var act = () => _sut.Handle(new CreateTaskItemRequest { Title = "Title" }, CancellationToken.None);
    
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _storage.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
    
    private void SetupUsers(params User[] users)
    {
        var dbSet = users.ToList().BuildMockDbSet();
        _storage.Users.Returns(dbSet);
    }
}