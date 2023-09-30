﻿using AutoMapper;
using Humanizer;
using NextSolution.Core.Entities;
using NextSolution.Core.Extensions.FileStorage;
using NextSolution.Core.Extensions.Identity;
using NextSolution.Core.Models.Chats;
using NextSolution.Core.Models.Medias;
using NextSolution.Core.Models.Users;
using NextSolution.Core.Repositories;
using NextSolution.Core.Shared;
using NextSolution.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NextSolution.Core.Models
{
    public interface IModelBuilder
    {
        // User
        Task<UserWithSessionModel> BuildAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default);

        Task<UserModel> BuildAsync(User user, CancellationToken cancellationToken = default);

        Task<UserPageModel> BuildAsync(IPageable<User> users, CancellationToken cancellationToken = default);

        Task<UserListModel> BuildAsync(IEnumerable<User> users, CancellationToken cancellationToken = default);

        // Chat
        Task<ChatModel> BuildAsync(Chat chat, CancellationToken cancellationToken = default);

        Task<ChatPageModel> BuildAsync(IPageable<Chat> chats, CancellationToken cancellationToken = default);

        Task<ChatListModel> BuildAsync(IEnumerable<Chat> chats, CancellationToken cancellationToken = default);
    }

    public class ModelBuilder : IModelBuilder
    {
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly IUserRepository _userRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IClientRepository _clientRepository;

        public ModelBuilder(IMapper mapper, IFileStorage fileStorage, IUserRepository userRepository, IMediaRepository mediaRepository, IClientRepository clientRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public async Task<UserWithSessionModel> BuildAsync(User user, UserSessionInfo session, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (session == null) throw new ArgumentNullException(nameof(session));

            var model = _mapper.Map(session, _mapper.Map<UserWithSessionModel>(user));
            model.Online = await _clientRepository.AnyAsync(_ => _.Active && _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToList();
            if ((user.Avatar = user.AvatarId != null ? await _mediaRepository.GetByIdAsync(user.AvatarId.Value) : null) != null)
            {
                model.AvatarUrl = await _fileStorage.GetPublicUrlAsync(user.Avatar.Path, cancellationToken);
            }
            return model;
        }

        public async Task<UserModel> BuildAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var model = _mapper.Map<UserModel>(user);

            model.Online = await _clientRepository.AnyAsync(_ => _.Active && _.UserId == user.Id, cancellationToken);
            model.Roles = (await _userRepository.GetRolesAsync(user, cancellationToken)).Select(_ => _.Camelize()).ToList();

            if ((user.Avatar = user.AvatarId != null ? await _mediaRepository.GetByIdAsync(user.AvatarId.Value) : null) != null)
            {
                model.AvatarUrl = await _fileStorage.GetPublicUrlAsync(user.Avatar.Path, cancellationToken);
            }

            return model;
        }

        public async Task<UserPageModel> BuildAsync(IPageable<User> users, CancellationToken cancellationToken = default)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var pageModel = await BuildAsync<UserPageModel>(users, cancellationToken);
            pageModel.Offset = users.Offset;
            pageModel.Limit = users.Limit;
            pageModel.Length = users.Length;
            pageModel.Previous = users.Previous;
            pageModel.Next = users.Next;
            return pageModel;
        }

        public Task<UserListModel> BuildAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
        {
            return BuildAsync<UserListModel>(users, cancellationToken);
        }

        public async Task<TListModel> BuildAsync<TListModel>(IEnumerable<User> users, CancellationToken cancellationToken = default)
            where TListModel : UserListModel
        {
            if (users == null) throw new ArgumentNullException(nameof(users));

            var items = new List<UserModel>();

            foreach (var user in users)
            {
                var userModel = await BuildAsync(user, cancellationToken);
                items.Add(userModel);
            }

            var listModel = Activator.CreateInstance<TListModel>();
            listModel.Items = items;
            return listModel;
        }

        public Task<ChatModel> BuildAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            var model = _mapper.Map<ChatModel>(chat);
            return Task.FromResult(model);
        }

        public async Task<ChatPageModel> BuildAsync(IPageable<Chat> chats, CancellationToken cancellationToken = default)
        {
            if (chats == null) throw new ArgumentNullException(nameof(chats));

            var pageModel = await BuildAsync<ChatPageModel>(chats, cancellationToken);
            pageModel.Offset = chats.Offset;
            pageModel.Limit = chats.Limit;
            pageModel.Length = chats.Length; 
            pageModel.Previous = chats.Previous;
            pageModel.Next = chats.Next;
            return pageModel;
        }

        public Task<ChatListModel> BuildAsync(IEnumerable<Chat> chats, CancellationToken cancellationToken = default)
        {
            return BuildAsync<ChatListModel>(chats, cancellationToken);
        }

        public async Task<TListModel> BuildAsync<TListModel>(IEnumerable<Chat> chats, CancellationToken cancellationToken = default)
            where TListModel : ChatListModel
        {
            if (chats == null) throw new ArgumentNullException(nameof(chats));

            var items = new List<ChatModel>();

            foreach (var chat in chats)
            {
                var chatModel = await BuildAsync(chat, cancellationToken);
                items.Add(chatModel);
            }

            var listModel = Activator.CreateInstance<TListModel>();
            listModel.Items = items;
            return listModel;
        }
    }
}