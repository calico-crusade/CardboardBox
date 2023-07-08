using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CardboardBox.Discord;

/// <summary>
/// Exposes useful extension methods
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Returns the correct <see cref="IEmote"/> for the given emote
	/// </summary>
	/// <param name="emote">The string representation of the emote</param>
	/// <returns>The correct <see cref="IEmote"/></returns>
	/// <exception cref="InvalidDataException">Thrown if the emote is not of type <see cref="Emote"/> or <see cref="Emoji"/></exception>
	public static IEmote GetEmote(this string emote)
	{
		if (Emote.TryParse(emote, out var e))
			return e;

		if (Emoji.TryParse(emote, out var ej))
			return ej;

		throw new InvalidDataException($"'{emote}' is not a valid emote or emoji");
	}

	/// <summary>
	/// Responds to the interaction message and returns the original message
	/// </summary>
	/// <param name="cmd"></param>
	/// <param name="text"></param>
	/// <param name="embeds"></param>
	/// <param name="isTTS"></param>
	/// <param name="ephemeral"></param>
	/// <param name="allowedMentions"></param>
	/// <param name="components"></param>
	/// <param name="embed"></param>
	/// <param name="options"></param>
	/// <returns>The original interaction message</returns>
	public static async Task<RestInteractionMessage> Respond(this SocketSlashCommand cmd, string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, MessageComponent? components = null, Embed? embed = null, RequestOptions? options = null)
	{
		await cmd.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
		return await cmd.GetOriginalResponseAsync();
	}

	/// <summary>
	/// Modifies the exisitng socket message with the given content
	/// </summary>
	/// <param name="cmd">The slash command that triggered the request</param>
	/// <param name="message">The message to update the response to</param>
	/// <returns>The interaction result</returns>
	public static Task<RestInteractionMessage> Modify(this SocketSlashCommand cmd, string message)
	{
		return cmd.ModifyOriginalResponseAsync(f =>
		{
			f.Content = message;
			f.Embed = null;
		});
	}

	/// <summary>
	/// Modifies the exisitng socket message with the given content
	/// </summary>
	/// <param name="cmd">The slash command that triggered the request</param>
	/// <param name="message">The content to update the response to</param>
	/// <returns>The interaction result</returns>
	public static Task<RestInteractionMessage> Modify(this SocketSlashCommand cmd, Embed message)
	{
		return cmd.ModifyOriginalResponseAsync(f =>
		{
			f.Content = null;
			f.Embed = message;
		});
	}

	/// <summary>
	/// Modifies the exisitng socket message with the given content
	/// </summary>
	/// <param name="cmd">The slash command that triggered the request</param>
	/// <param name="message">The content to update the response to</param>
	/// <returns>The interaction result</returns>
	public static Task<RestInteractionMessage> Modify(this SocketSlashCommand cmd, EmbedBuilder message)
	{
		return cmd.ModifyOriginalResponseAsync(f =>
		{
			f.Content = null;
			f.Embed = message.Build();
		});
	}

	/// <summary>
	/// Modifies the exisitng socket message with the given content
	/// </summary>
	/// <param name="cmd">The slash command that triggered the request</param>
	/// <param name="message">The content to update the response to</param>
	/// <returns>The interaction result</returns>
	public static Task<RestInteractionMessage> Modify(this SocketSlashCommand cmd, Action<EmbedBuilder> message)
	{
		var em = new EmbedBuilder();
		message(em);
		var res = em.Build();
		return cmd.Modify(res);
	}

	/// <summary>
	/// Generates an embedable timestamp in a format that discord can use
	/// </summary>
	/// <param name="time">The UNIX epoch timestamp</param>
	/// <param name="format">The format to use (defaults to "f") <see href="https://discord.com/developers/docs/reference#message-formatting-timestamp-styles"/></param>
	/// <returns>The embedable timestamp</returns>
	public static string GenerateTimestamp(this long time, string format = "f")
	{
		var isMilli = time.ToString().Length > 10;
		if (isMilli)
			time = time.FromEpoch(true).ToLocalTime().ToEpoch();
		return $"<t:{time}:{format}>";
	}

	/// <summary>
	/// Generates an embedable timestamp in a format that discord can use
	/// </summary>
	/// <param name="time">The timestamp</param>
	/// <param name="format">The format to use (defaults to "f") <see href="https://discord.com/developers/docs/reference#message-formatting-timestamp-styles"/></param>
	/// <returns>The embedable timestamp</returns>
	public static string GenerateTimestamp(this DateTime time, string format = "f")
	{
		return GenerateTimestamp(time.ToEpoch(), format);
	}

	/// <summary>
	/// Gets the true boolean value from the <see cref="OptBool"/>
	/// </summary>
	/// <param name="opt">The value</param>
	/// <returns>The true boolean value</returns>
	public static bool? From(this OptBool opt)
	{
		return opt switch
		{
			OptBool.True => true,
			OptBool.False => false,
			_ => null
		};
	}

	/// <summary>
	/// Gets the <see cref="OptBool"/> value from the true boolean value
	/// </summary>
	/// <param name="opt">The value</param>
	/// <returns>The <see cref="OptBool"/> value</returns>
	public static OptBool From(this bool? opt)
	{
		if (opt == null) return OptBool.NotSet;
		return opt.Value ? OptBool.True : OptBool.False;
	}
}
