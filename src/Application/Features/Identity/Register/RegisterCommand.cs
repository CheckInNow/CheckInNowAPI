
using Application.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Identity.Register;
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword) : ICommand;
