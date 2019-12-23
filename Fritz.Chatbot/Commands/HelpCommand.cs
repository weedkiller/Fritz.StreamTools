﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fritz.StreamLib.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Fritz.Chatbot.Commands
{
	public class HelpCommand : IBasicCommand
	{
		public string Trigger => "help";
		public string Description => "Get information about the functionality available on this channel";
		public TimeSpan? Cooldown => null;

		private readonly IServiceProvider _serviceProvider;

		public HelpCommand(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task Execute(IChatService chatService, string userName, ReadOnlyMemory<char> rhs)
		{
			var commands = _serviceProvider.GetServices<IBasicCommand>();
			var textCommandArray = TextCommand._Commands.Select(kv => "!" + kv.Key.ToLower());
			var soundFxCommands = SoundFxCommand.Effects.Select(kv => "!" + kv.Key.ToLower());

			if (rhs.IsEmpty)
			{

				var availableCommandArray = commands.Where(c => !string.IsNullOrEmpty(c.Trigger)).Select(c => $"!{c.Trigger.ToLower()}");

				var availableCommands = string.Join(' ', availableCommandArray.Union(textCommandArray).Union(soundFxCommands).OrderBy(s => s));

				await chatService.SendMessageAsync($"Supported commands: {availableCommands}");
				return;
			}

			var cmd = commands.FirstOrDefault(c => rhs.Span.Equals(c.Trigger.AsSpan(), StringComparison.OrdinalIgnoreCase));
			if (cmd == null)
			{

				if (textCommandArray.Contains("!" + rhs.Span.ToString()))
				{
					await chatService.SendMessageAsync($"{rhs}: A helpful text message");
				}
				else if (soundFxCommands.Contains("!" + rhs.Span.ToString()))
				{
					await chatService.SendMessageAsync($"{rhs}: A fun sound effect");
				}
				else
				{
					await chatService.SendMessageAsync("Unknown command to provide help with.");
				}
				return;
			}

			await chatService.SendMessageAsync($"{rhs}: {cmd.Description}");
		}
	}
}
