using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub : Hub
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;

    public MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _presenceHub = presenceHub;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContex = Context.GetHttpContext();
        var otherUser = httpContex.Request.Query["user"];

        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await _messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);

        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public async Task SendMessage(CreateMessageDTO createMessageDTO)
    {

        var username = Context.User.GetUsername();

        if (username == createMessageDTO.RecipientUsername) throw new HubException("You cannot message yourself");

        var sender = await _userRepository.GetUserByUsernameAsync(username);
        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDTO.RecipientUsername);

        if (recipient == null) throw new HubException("User not found");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = username,
            RecipientUsername = recipient.UserName,
            Content = createMessageDTO.content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await _messageRepository.GetMessageGroup(groupName);

        if (group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageRecieved"
                    , new { username = sender.UserName, KnownAs = sender.KnownAs });
            }
        }

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync()) await Clients.Group(groupName)
        .SendAsync("NewMessage", _mapper.Map<MessageDTO>(message));
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup");

        await base.OnDisconnectedAsync(exception);
    }

    private string GetGroupName(string caller, string other)
    {
        var strCompare = string.CompareOrdinal(caller, other) < 0;
        return strCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _messageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
        if (group == null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);
        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from group");
    }
}
