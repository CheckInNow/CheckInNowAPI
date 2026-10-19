
using Domain.Common;

namespace Application.Abstractions.Messaging;
public interface ICommand : ICommand<Result>;

public interface ICommand<TResponse>;
