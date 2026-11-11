using Application.Abstractions.Messaging;

namespace Application.Features.Rooms.Get;

public sealed record GetRoomsQuery(Guid ConferenceId) : IQuery<List<RoomResponse>>;

public sealed record RoomResponse(Guid Id, string Name, int? Capacity);
