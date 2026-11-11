namespace Api.Requests.Rooms;

public sealed record CreateRoomRequest(string Name, int? Capacity);
