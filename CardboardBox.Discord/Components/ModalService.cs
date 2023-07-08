using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;

namespace CardboardBox.Discord.Components;

/// <summary>
/// Service for handling modals
/// </summary>
public interface IModalService
{
    /// <summary>
    /// Resolves a modal from the given method
    /// </summary>
    /// <typeparam name="T">The type of class the method is on</typeparam>
    /// <param name="selector">The method selector</param>
    /// <returns>The modal built from the method</returns>
    Modal Modal<T>(Expression<Func<T, Delegate>> selector);

    /// <summary>
    /// Handles an inbound modal interaction
    /// </summary>
    /// <param name="modal">The modal data</param>
    void HandleModal(SocketModal modal);
}

/// <summary>
/// Service for handling modals
/// </summary>
public class ModalService : IModalService
{
    private readonly ILogger _logger;
    private readonly IComponentHandlerService _handler;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _provider;

    /// <summary></summary>
    /// <param name="logger"></param>
    /// <param name="handler"></param>
    /// <param name="client"></param>
    /// <param name="provider"></param>
    public ModalService(
        ILogger<ModalService> logger,
        IComponentHandlerService handler,
        DiscordSocketClient client,
        IServiceProvider provider)
    {
        _logger = logger;
        _handler = handler;
        _client = client;
        _provider = provider;
    }

    /// <summary>
    /// Resolves a modal from the given method
    /// </summary>
    /// <typeparam name="T">The type of class the method is on</typeparam>
    /// <param name="selector">The method selector</param>
    /// <returns>The modal built from the method</returns>
    public Modal Modal<T>(Expression<Func<T, Delegate>> selector)
    {
        var exp = (UnaryExpression)selector.Body;
        var del = (MethodCallExpression)exp.Operand;
        var meth = (MethodInfo)((ConstantExpression)del.Object).Value;
        return Modal(meth);
    }

    /// <summary>
    /// Resolves a modal from the given method information
    /// </summary>
    /// <param name="method">The method information for the modal target</param>
    /// <returns>The modal built from the method info</returns>
    /// <exception cref="NotImplementedException">Thrown if the parameters have invalid arttibutes</exception>
    public Modal Modal(MethodInfo method)
    {
        var id = _handler.IdFromMethod(method);
        if (!_handler.GetHandler(id, out _, out _))
            throw new NotImplementedException($"No handler found for {method.Name}. Is it registered?");

        var attr = method.GetCustomAttribute<ModalAttribute>()
            ?? throw new NotImplementedException($"No modal attribute found for {method.Name}");

        var modal = new ModalBuilder()
            .WithTitle(attr.Title)
            .WithCustomId(id);

        var rows = new Dictionary<int, List<IMessageComponent>>();

        var parameters = method.GetParameters();
        for(var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var compAtr = param.GetCustomAttribute<TextAttribute>();
            if (compAtr == null) continue;

            var ctxId = $"{id}-{i}";

            var comp = HandleText(ctxId, compAtr, param);
            if (comp == null)
            {
                _logger.LogWarning($"Unknown `{nameof(TextAttribute)}`: {compAtr.GetType().Name}");
                continue;
            }

            if (!rows.ContainsKey(compAtr.Row))
                rows.Add(compAtr.Row, new List<IMessageComponent>());

            rows[compAtr.Row].Add(comp);
        }

        rows
            .OrderBy(t => t.Key)
            .Select(t => t.Value)
            .Each((row, comps) => modal.AddComponents(comps, row));
        
        return modal.Build();
    }

    /// <summary>
    /// Builds a <see cref="TextInputComponent"/> from the given attribute info
    /// </summary>
    /// <param name="id">The unique ID for this component</param>
    /// <param name="text">The attribute that describes the input</param>
    /// <param name="parameter">The parameter this input describes</param>
    /// <returns>The built message component</returns>
    public IMessageComponent HandleText(string id, TextAttribute text, ParameterInfo parameter)
    {
        var bob = new TextInputBuilder()
            .WithLabel(text.Label)
            .WithStyle(text.Style)
            .WithPlaceholder(text.Placeholder)
            .WithValue(text.Value)
            .WithCustomId(id);

        if (text.MinLength != null)
            bob.WithMinLength(text.MinLength.Value);

        if (text.MaxLength != null)
            bob.WithMaxLength(text.MaxLength.Value);

        var (_, nullable) = DiscordSlashCommandBuilder.DetermineType(parameter);
        var req = text.Required ?? !nullable ?? false;

        return bob
            .WithRequired(req)
            .Build();
    }

    /// <summary>
    /// Handles an inbound modal interaction
    /// </summary>
    /// <param name="modal">The modal data</param>
    /// <returns></returns>
    public void HandleModal(SocketModal modal)
    {
        _ = Task.Run(async () => 
        {
            try
            {
                await HandleModalAsync(modal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while handling modal: {modal.Message.Id}::{modal.User.Id}");
            }
        });
    }

    /// <summary>
    /// Handles an inbound modal interaction
    /// </summary>
    /// <param name="modal">The modal data</param>
    /// <returns></returns>
    public async Task HandleModalAsync(SocketModal modal)
    {
        if (modal.User.IsBot ||
            modal.User.Id == _client.CurrentUser.Id)
        {
            await modal.DeferAsync(ephemeral: true);
            return;
        }

        var id = modal.Data.CustomId;
        if (!_handler.GetHandler(id, out var method, out var type))
        {
            _logger.LogWarning($"Modal: Cannot find loaded modal for `{id}`");
            await modal.DeferAsync(ephemeral: true);
            return;
        }

        var atr = method?.GetCustomAttribute<ModalAttribute>();

        if (method == null || type == null || atr == null)
        {
            _logger.LogWarning($"Modal: Method or type is null for `{id}`");
            await modal.DeferAsync(ephemeral: true);
            return;
        }

        var service = _provider.GetService(type);
        if (service == null)
        {
            _logger.LogWarning($"Modal: Cannot find service for type `{type.Name}`");
            await modal.DeferAsync(ephemeral: true);
            return;
        }

        var args = GetParameters(modal, method);
        if (args == null)
        {
            _logger.LogWarning($"Modal: Cannot find parameters for `{id}`");
            await modal.DeferAsync(ephemeral: true);
            return;
        }

        var result = method.Invoke(service, args);
        if (result == null) return;

        var resType = result.GetType();
        if (resType == typeof(void)) return;
        if (result is not Task) return;
        await (Task)result;
    }

    /// <summary>
    /// Maps the modal data to the method parameters
    /// </summary>
    /// <param name="modal">The modal data</param>
    /// <param name="method">The method to match to</param>
    /// <returns>The method parameters</returns>
    public object?[]? GetParameters(SocketModal modal, MethodInfo method)
    {
        var pars = method.GetParameters();
        var args = new object?[pars.Length];

        var id = modal.Data.CustomId;

        for (var i = 0; i < args.Length; i++)
        {
            var ctxId = $"{id}-{i}";
            var param = pars[i];
            var act = modal.Data.Components.FirstOrDefault(t => t.CustomId == ctxId);
            if (act != null)
            {
                args[i] = act?.Value;
                continue;
            }

            if (param.ParameterType == typeof(SocketModal))
            {
                args[i] = modal;
                continue;
            }

            var serv = _provider.GetService(param.ParameterType);
            if (serv != null)
            {
                args[i] = serv;
                continue;
            }

            args[i] = null;
        }

        return args;
    }
}
