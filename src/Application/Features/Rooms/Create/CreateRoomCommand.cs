using Application.Abstractions.Messaging;

namespace Application.Features.Rooms.Create;

public sealed record CreateRoomCommand(Guid ConferenceId, string Name, int? Capacity) : ICommand<CreateRoomResponse>;

public sealed record CreateRoomResponse(Guid Id, Guid ConferenceId, string Name, int? Capacity);
