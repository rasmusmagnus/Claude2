using Chess.Shared;

namespace Events.Commands;

public readonly record struct PromoteToCommand(PromoteOptions Option) : ICommand;